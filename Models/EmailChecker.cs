using System;
using System.Collections.Generic;
using System.Text;
using static MsgReader.Outlook.Storage;
using static CommunigateAntispamHelper.Utils.Utils;
using System.Text.RegularExpressions;

namespace CommunigateAntispamHelper.Models
{
    internal class EmailChecker
    {
        private readonly AppSettings appSettings;
        private bool _isUpdateAllowed = true;
        public bool isUpdateAllowed { get { return _isUpdateAllowed; } }
        private FileDataStore fileDataStore = new FileDataStore();
        public string goodMessage { get { return fileDataStore.goodMessage; } }
        public string badMessage { get { return fileDataStore.badMessage; } }
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
        public bool IsRecipentExcluded(string? email)
        {
            if (email == null) return false;
            return fileDataStore.excludedRecipients.Contains(email);
        }
        public bool IsSenderWhiteListed(string? email)
        {
            if (email == null) return false;
            if (fileDataStore.whiteListSenderAddresses.Contains(email)) return true;
            string domain = email.Substring(email.IndexOf('@') + 1);
            if (fileDataStore.whiteListSenderDomains.Contains(domain)) return true;
            return false;
        }
        public bool IsSenderBlackListed(string? email)
        {
            if (email == null) return false;
            if (fileDataStore.blackListSenderAddresses.Contains(email))
            {
                PrintLogMessage($"blacklisted sender found: {email}");
                return true;
            }
                
            string domain = email.Substring(email.IndexOf('@') + 1);
            if (fileDataStore.blackListSenderDomains.Contains(domain))
            {
                PrintLogMessage($"blacklisted domain found: {domain}");
                return true;
            }
                
            foreach (var domainPattern in fileDataStore.blackListWildcardSenderDomains)
            {
                if (IsEqualWithWildcard(email, domainPattern))
                {
                    PrintLogMessage($"blacklisted domain found: {domain}");
                    return true;
                }
            }
            return false;
        }
        public bool IsThereProhibitedTextInBody(string body)
        {
            foreach (string line in fileDataStore.prohibitedTextInBody)
            {
                if (body.Contains(line))
                {
                    PrintLogMessage($"blacklisted line found: {line}");
                    return true;
                }
            }
            return false;
        }
        public bool IsThereProhibitedRegexInBody(string body)
        {
            foreach(string line in fileDataStore.prohibitedRegExInBody)
            {
                Regex regex = new(line, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (regex.IsMatch(body))
                {
                    PrintLogMessage($"blacklisted regex found: {line}");
                    return true;
                }
            }
            return false;
        }

    }
}
