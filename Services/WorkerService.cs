using CommunigateAntispamHelper.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
                using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader sr = new StreamReader(fs))
                {
                    string? line = sr.ReadLine();
                    return line;
                }
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
        public async Task Work()
        {
            while (true)
            {
                string? line = await Console.In.ReadLineAsync();
                if (line == null)
                {
                    break;
                }
                _ = Task.Run(() =>
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
        private void ParseFile(string file, string lineNumberStr)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
            {
                PrintGoodMessage(lineNumberStr);
                Print($"* CommunigateAntispamHelper: unable to read file {file}");
                return;
            }
            List<string> lines = new List<string>();
            using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string? line;
                bool firstEmpyLineFound = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!firstEmpyLineFound && line == "")
                    {
                        firstEmpyLineFound = true;
                    }
                    else
                    if (firstEmpyLineFound)
                    {
                        lines.Add(line);
                    }
                    else
                    {
                        string pattern = @".*<(.*)>";
                        if (line.StartsWith("R W "))
                        {
                            Match regexMatch = Regex.Match(line, pattern);
                            if (regexMatch.Success)
                            {
                                string recipient = regexMatch.Groups[1].Value;
                                if (emailChecker.IsRecipentExcluded(recipient))
                                {
                                    PrintGoodMessage(lineNumberStr); return;
                                }
                            }
                        }
                        if (line.StartsWith("P I "))
                        {
                            Match regexMatch = Regex.Match(line, pattern);
                            if (regexMatch.Success)
                            {
                                string sender = regexMatch.Groups[1].Value;
                                if (emailChecker.IsSenderWhiteListed(sender))
                                {
                                    PrintGoodMessage(lineNumberStr); return;
                                }
                                if (emailChecker.IsSenderBlackListed(sender))
                                {
                                    PrintBadMessage(lineNumberStr); return;    
                                }

                            }
                        }
                    }
                }
                string tempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".antispam";
                System.IO.File.WriteAllLines(tempFile, lines);
                var eml = MsgReader.Mime.Message.Load(new FileInfo(tempFile));
                System.IO.File.Delete(tempFile);
                if (eml.TextBody != null)
                {
                    var textBody = System.Text.Encoding.UTF8.GetString(eml.TextBody.Body);
                    if (emailChecker.IsThereProhibitedTextInBody(textBody))
                    {
                        PrintBadMessage(lineNumberStr); return;
                    }
                }

                if (eml.HtmlBody != null)
                {
                    var htmlBody = System.Text.Encoding.UTF8.GetString(eml.HtmlBody.Body);
                    if (emailChecker.IsThereProhibitedTextInBody(htmlBody))
                    {
                        PrintBadMessage(lineNumberStr); return;
                    }
                }
                PrintGoodMessage(lineNumberStr);
            }
        }
    }
}
