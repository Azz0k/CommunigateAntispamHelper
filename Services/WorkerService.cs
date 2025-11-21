using CommunigateAntispamHelper.Models;
using static CommunigateAntispamHelper.Utils.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunigateAntispamHelper.Services
{
    internal class WorkerService
    {
        private EmailChecker emailChecker;
        private readonly AppSettings appSettings;
        public WorkerService(AppSettings appSettings, EmailChecker emailChecker)
        {
            this.appSettings = appSettings;
            this.emailChecker = emailChecker;
        }
        public void Print(string message)
        {
            Console.WriteLine(message);
            Console.Out.Flush();
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
                Print($"{lineNumberStr} OK");
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
                }
            }
            string tempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".antispam";
            System.IO.File.WriteAllLines(tempFile, lines);
            var eml = MsgReader.Mime.Message.Load(new FileInfo(tempFile));
            System.IO.File.Delete(tempFile);
            if (eml.Headers == null)
            {
                Print($"{lineNumberStr} OK");
                return;
            }
            foreach (var recipient in eml.Headers.To)
            {
                var to = GetAddressFromToHeader(recipient.Address);
            }
            if (eml.TextBody != null)
            {
                var textBody = System.Text.Encoding.UTF8.GetString(eml.TextBody.Body);
            }

            if (eml.HtmlBody != null)
            {
                var htmlBody = System.Text.Encoding.UTF8.GetString(eml.HtmlBody.Body);
            }

            
        }
    }
}
