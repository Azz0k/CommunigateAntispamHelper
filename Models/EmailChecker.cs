using System;
using System.Collections.Generic;
using System.Text;
using static MsgReader.Outlook.Storage;

namespace CommunigateAntispamHelper.Models
{
    internal class EmailChecker
    {
        private readonly AppSettings appSettings;
        private bool _isUpdateAllowed = true;
        public bool isUpdateAllowed { get { return _isUpdateAllowed; } }
        private FileDataStore fileDataStore = new FileDataStore();
        public EmailChecker(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }
        public void DisableUpdates()
        {
            _isUpdateAllowed = false;
        }
        public void EnableUpdates()
        {
            _isUpdateAllowed = true;
        }
        public void UpdateStore(FileTypes fileType, List<string> data)
        {
            fileDataStore.UpdateStore(fileType, data);
        }
        public bool IsRecipentExcluded(string email)
        {
            return fileDataStore.excludedRecipients.Contains(email);
        }
        public bool IsSenderWhiteListed(string email)
        {
            if (fileDataStore.whiteListSenderAddresses.Contains(email)) return true;
            string domain = email.Substring(email.IndexOf('@') + 1);
            if (fileDataStore.whiteListSenderDomains.Contains(domain)) return true;
            return false;
        }
        public bool IsSenderBlackListed(string email)
        {
            if (fileDataStore.blackListSenderAddresses.Contains(email)) return true;
            string domain = email.Substring(email.IndexOf('@') + 1);
            if (fileDataStore.blackListSenderDomains.Contains(domain)) return true;
            return false;
        }
        public bool IsThereProhibitedTextInBody(string body)
        {
            foreach (string line in fileDataStore.prohibitedTextInBody)
            {
                if (body.Contains(line)) return true;
            }
            return false;
        }

    }
}
