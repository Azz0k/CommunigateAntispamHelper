using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CommunigateAntispamHelper.Models
{
    internal class FileDataStore
    {
        private HashSet<string> whiteListSenderDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> whiteListSenderAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> excludedRecipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> blackListDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> blackListAddresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private List<string> prohibitedTextInBody = new List<string>();
        private List<string> prohibitedRegExInBody = new List<string>();
        private readonly Dictionary<FileTypes, Action<List<string>>> updateHandlers;
        public FileDataStore()
        {
            updateHandlers = new Dictionary<FileTypes, Action<List<string>>>
            {
                { FileTypes.whiteListSenderDomainsFile, data =>
                    {
                        whiteListSenderDomains.Clear();
                        whiteListSenderDomains.UnionWith(data);
                    }
                },
                { FileTypes.whiteListSenderAddressesFile, data =>
                    {
                        whiteListSenderAddresses.Clear();
                        whiteListSenderAddresses.UnionWith(data);
                    }
                },
                { FileTypes.excludedRecipientsFile, data =>
                    {
                        excludedRecipients.Clear();
                        excludedRecipients.UnionWith(data);
                    }
                },
                { FileTypes.blackListDomainsFile, data =>
                    {
                        blackListDomains.Clear();
                        blackListDomains.UnionWith(data);
                    }
                },
                { FileTypes.blackListAddressesFile, data =>
                    {
                        blackListAddresses.Clear();
                        blackListAddresses.UnionWith(data);
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
