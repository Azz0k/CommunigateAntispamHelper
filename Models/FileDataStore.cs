using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace CommunigateAntispamHelper.Models
{
    internal class FileDataStore
    {
        private HashSet<string> _whiteListSenderDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> whiteListSenderDomains { get { return _whiteListSenderDomains; } }
        private HashSet<string> _whiteListSenderAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> whiteListSenderAddresses  { get { return _whiteListSenderAddresses; } }
        private HashSet<string> _excludedRecipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> excludedRecipients { get { return _excludedRecipients; } }
        private HashSet<string> _blackListSenderDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> blackListSenderDomains { get { return _blackListSenderDomains; } }
        private HashSet<string> _blackListSenderAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> blackListSenderAddresses {  get { return _blackListSenderAddresses; } }
        private List<string> prohibitedTextInBody = new List<string>();
        private List<string> prohibitedRegExInBody = new List<string>();
        private readonly Dictionary<FileTypes, Action<List<string>>> updateHandlers;
        public FileDataStore()
        {
            updateHandlers = new Dictionary<FileTypes, Action<List<string>>>
            {
                { FileTypes.whiteListSenderDomainsFile, data =>
                    {
                        _whiteListSenderDomains.Clear();
                        _whiteListSenderDomains.UnionWith(data);
                    }
                },
                { FileTypes.whiteListSenderAddressesFile, data =>
                    {
                        _whiteListSenderAddresses.Clear();
                        _whiteListSenderAddresses.UnionWith(data);
                    }
                },
                { FileTypes.excludedRecipientsFile, data =>
                    {
                        _excludedRecipients.Clear();
                        _excludedRecipients.UnionWith(data);
                    }
                },
                { FileTypes.blackListDomainsFile, data =>
                    {
                        _blackListSenderDomains.Clear();
                        _blackListSenderDomains.UnionWith(data);
                    }
                },
                { FileTypes.blackListAddressesFile, data =>
                    {
                        _blackListSenderAddresses.Clear();
                        _blackListSenderAddresses.UnionWith(data);
                    }
                },
                { FileTypes.prohibitedTextInBodyFile, data =>
                    {
                        prohibitedTextInBody = data;
                    }
                },
                { FileTypes.prohibitedRegExInBodyFile, data =>
                    {
                        prohibitedRegExInBody = data;
                    }
                },
            };

        }
        public void UpdateStore(FileTypes fileType, List<string> data)
        {
            if (updateHandlers.TryGetValue(fileType, out var handler))
            {
                handler(data);
            }
        }
        

    }
}
