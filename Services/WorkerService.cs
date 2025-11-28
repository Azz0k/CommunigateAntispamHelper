using CommunigateAntispamHelper.Models;
using System.Text;
using static CommunigateAntispamHelper.Utils.Utils;

namespace CommunigateAntispamHelper.Services
{
    internal class WorkerService
    {
        private EmailChecker emailChecker;
        private readonly AppSettings appSettings;
        private string goodMessage = "OK";
        private string badMessage = "ADDHEADER \"X-SPAM-SCORE: 100 CommunigateAntispamHelper\" OK";
        public WorkerService(AppSettings appSettings, EmailChecker emailChecker)
        {
            this.appSettings = appSettings;
            this.emailChecker = emailChecker;
            var goodMessageFileName = Path.Combine(appSettings.currentDir, appSettings.goodMessageFileName);
            var badMessageFileName = Path.Combine(appSettings.currentDir , appSettings.badMessageFileName); 
            goodMessage = ReadFirstLineFromFile(goodMessageFileName) ?? goodMessage;
            badMessage = ReadFirstLineFromFile(badMessageFileName) ?? badMessage;
        }
        private string? ReadFirstLineFromFile(string fileName)
        {
            try
            {
                using FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                using StreamReader sr = new(fs);
                string? line = sr.ReadLine();
                return line;
            }
            catch
            {
                return null; 
            }
        }
        public void PrintGoodMessage(string lineNumber)
        {
            Print($"{lineNumber} {goodMessage}");
        }
        public void PrintBadMessage(string lineNumber)
        {
            Print($"{lineNumber} {badMessage}");
        }
        public async System.Threading.Tasks.Task Work()
        {
            while (true)
            {
                string? line = await Console.In.ReadLineAsync();
                if (line == null)
                {
                    break;
                }
                _ = System.Threading.Tasks.Task.Run(() =>
                {
                    ProcessMessage(line);
                });
            }
        }
        private void ProcessMessage(string input)
        {
            emailChecker.DisableUpdates();
            string[] inputParts = input.Split();
            if (inputParts.Length == 0)
            {
                return;
            }
            string lineNumberStr = inputParts[0];
            if (!int.TryParse(lineNumberStr, out _))
            {
                return;
            }
            string command = inputParts[1].ToLower();
            switch (command)
            {
                case "quit":
                    Print($"{lineNumberStr} OK");
                    Environment.Exit(0);
                    break;
                case "intf":
                    Print($"{lineNumberStr} INTF 3");
                    break;
                case "file":
                    if (inputParts.Length != 3)
                    {
                        return;
                    }
                    string fileName = inputParts[2];
                    var file = Path.Combine(appSettings.baseDir, fileName.Trim());
                    ParseFile(file, lineNumberStr);
                    break;
                default:
                    break;
            }
            emailChecker.EnableUpdates();
        }
        public bool EnsureFileExists(string file, string lineNumberStr)
        {
            FileInfo fileInfo = new(file);
            if (!fileInfo.Exists)
            {
                PrintGoodMessage(lineNumberStr);
                Print($"* CommunigateAntispamHelper: file is not exists {file}");
                return false;
            }
            return true;
        }
        private void ParseFile(string file, string lineNumberStr)
        {
            if (!EnsureFileExists(file, lineNumberStr)) return;
            try
            {
                using FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true);
                string? line = null;
                while ((line = ReadAsciiLine(br)) != null)
                {
                    if (line == "") break;
                    if (emailChecker.IsRecipentExcluded(GetRecipient(line)))
                    {
                        PrintGoodMessage(lineNumberStr); return;
                    }
                    string? sender = GetSender(line);
                    if (emailChecker.IsSenderWhiteListed(sender))
                    {
                        PrintGoodMessage(lineNumberStr); return;
                    }
                    if (emailChecker.IsSenderBlackListed(sender))
                    {
                        PrintBadMessage(lineNumberStr); return;
                    }
                }
                var eml = MsgReader.Mime.Message.Load(fs);
                if (eml.TextBody != null && eml.TextBody.ContentType.CharSet != null)
                {
                    Encoding CodePage = Encoding.GetEncoding(eml.TextBody.ContentType.CharSet);
                    string textBody = CodePage.GetString(eml.TextBody.Body);
                    if (emailChecker.IsThereProhibitedTextInBody(textBody))
                    {
                        PrintBadMessage(lineNumberStr); return;
                    }
                }
                if (eml.HtmlBody != null && eml.HtmlBody.ContentType.CharSet != null)
                {
                    Encoding CodePage = Encoding.GetEncoding(eml.HtmlBody.ContentType.CharSet);
                    var htmlBody = CodePage.GetString(eml.HtmlBody.Body);
                    if (emailChecker.IsThereProhibitedTextInBody(htmlBody))
                    {
                        PrintBadMessage(lineNumberStr); return;
                    }
                }
            }
            catch
            {
                Print($"* CommunigateAntispamHelper cannot process message {file}");
            }
            PrintGoodMessage(lineNumberStr);
        }
    }
}
