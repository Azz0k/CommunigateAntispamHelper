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
                    line = line.Trim();
                    if (line != "") lines.Add(line);
                }
            }
            return lines;
        }
    }

    internal class MonitoredFiles
    {
        private List<MonitoredFileOnDisk> _files = new List<MonitoredFileOnDisk>();
        private EmailChecker emailChecker;
        private AppSettings appSettings;
        public MonitoredFiles(AppSettings appsettings, EmailChecker emailChecker)
        {
            this.appSettings = appsettings;
            this.emailChecker = emailChecker;
            _files.Add(new MonitoredFileOnDisk(PathCombine(appsettings.excludedRecipientsFileName), FileTypes.excludedRecipientsFile));
            _files.Add(new MonitoredFileOnDisk(PathCombine(appsettings.blackListSenderAddressesFileName), FileTypes.blackListAddressesFile));
            _files.Add(new MonitoredFileOnDisk(PathCombine(appsettings.blackListSenderDomainsFileName), FileTypes.blackListDomainsFile));
            _files.Add(new MonitoredFileOnDisk(PathCombine(appsettings.whiteListSenderAddressesFileName), FileTypes.whiteListSenderAddressesFile));
            _files.Add(new MonitoredFileOnDisk(PathCombine(appsettings.whiteListSenderDomainsFileName), FileTypes.whiteListSenderDomainsFile));
            _files.Add(new MonitoredFileOnDisk(PathCombine(appsettings.prohibitedTextInBodyFileName), FileTypes.prohibitedTextInBodyFile));
            _files.Add(new MonitoredFileOnDisk(PathCombine(appsettings.prohibitedRegExInBodyFileName), FileTypes.prohibitedRegExInBodyFile));
        }
        private string PathCombine(string fileName)
        {
            return Path.Combine(appSettings.currentDir, fileName);
        }
        public void CheckAllFiles()
        {
            foreach (var file in _files)
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
