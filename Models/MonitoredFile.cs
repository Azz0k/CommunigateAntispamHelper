using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunigateAntispamHelper.Models
{
    enum FileTypes { 
        prohibitedRegExInBodyFile, 
        prohibitedTextInBodyFile, 
        blackListAddressesFile, 
        blackListDomainsFile, 
        excludedRecipientsFile, 
        whiteListSenderAddressesFile, 
        whiteListSenderDomainsFile };
    internal class MonitoredFile
    {
        public required string FullName { get; set; } 
        public DateTime ModifiedTime { get; set; } = DateTime.MinValue;
        protected bool _isChanged { get; set; } = false;
        public bool IsChanged { get {  return _isChanged; }  }
        public required FileTypes FileType { get; set; }

        [SetsRequiredMembers]
        public MonitoredFile(string fullName, FileTypes fileType)
        {
            FullName = fullName;
            FileType = fileType;
        }
    }
    internal class MonitoredFileOnDisk : MonitoredFile
    {
        [SetsRequiredMembers]
        public MonitoredFileOnDisk(string fullName, FileTypes fileType) : base(fullName, fileType)
        {
            CheckFile();
        }
        public void CheckFile()
        {
            _isChanged = false;
            var file = new FileInfo(FullName);
            if (!file.Exists)
            {
                file.Create().Close();
                ModifiedTime = file.LastWriteTime;
                return;
            }
            if (ModifiedTime!= file.LastWriteTime)
                _isChanged = true;
        }
        public List<string> ReadAllLines()
        {
            var lines = new List<string>();
            using (FileStream fs = File.Open(FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line.Trim());
                }
            }
            return lines;
        }
    }
    internal class MonitoredFiles
    {
        private Dictionary<string, MonitoredFileOnDisk> _files = new Dictionary<string, MonitoredFileOnDisk>();
        private EmailChecker emailChecker;
        public MonitoredFiles(AppSettings appsettings, EmailChecker emailChecker)
        {
            this.emailChecker = emailChecker;
            _files.Add(appsettings.excludedRecipientsFileName, 
                new MonitoredFileOnDisk(Path.Combine(appsettings.currentDir, appsettings.excludedRecipientsFileName), FileTypes.excludedRecipientsFile));
            _files.Add(appsettings.blackListSenderAddressesFileName, 
                new MonitoredFileOnDisk(Path.Combine(appsettings.currentDir, appsettings.blackListSenderAddressesFileName), FileTypes.blackListAddressesFile));
            _files.Add(appsettings.blackListSenderDomainsFileName, 
                new MonitoredFileOnDisk(Path.Combine(appsettings.currentDir, appsettings.blackListSenderDomainsFileName), FileTypes.blackListDomainsFile));
            _files.Add(appsettings.whiteListSenderAddressesFileName, 
                new MonitoredFileOnDisk(Path.Combine(appsettings.currentDir, appsettings.whiteListSenderAddressesFileName), FileTypes.whiteListSenderAddressesFile));
            _files.Add(appsettings.whiteListSenderDomainsFileName, 
                new MonitoredFileOnDisk(Path.Combine(appsettings.currentDir,appsettings.whiteListSenderDomainsFileName), FileTypes.whiteListSenderDomainsFile));
            _files.Add(appsettings.prohibitedTextInBodyFileName, 
                new MonitoredFileOnDisk(Path.Combine(appsettings.currentDir,appsettings.prohibitedTextInBodyFileName), FileTypes.prohibitedTextInBodyFile));
            _files.Add(appsettings.prohibitedRegExInBodyFileName, 
                new MonitoredFileOnDisk(Path.Combine(appsettings.currentDir,appsettings.prohibitedRegExInBodyFileName), FileTypes.prohibitedRegExInBodyFile));
        }
        public void CheckAllFiles()
        {
            foreach (var file in _files.Values)
            {
                file.CheckFile();
                if (file.IsChanged)
                {
                    List<string> data = file.ReadAllLines();
                    if (emailChecker.isUpdateAllowed)
                        emailChecker.UpdateStore(file.FileType, data);
                }
            }
        }

    }


}
