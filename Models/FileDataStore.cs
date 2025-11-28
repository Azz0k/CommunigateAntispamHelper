using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Linq;

namespace CommunigateAntispamHelper.Models
{
    internal class FileDataStore
    {
        private HashSet<string> _whiteListSenderDomains = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> whiteListSenderDomains { get { return _whiteListSenderDomains; } }
        private HashSet<string> _whiteListSenderAddresses = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> whiteListSenderAddresses  { get { return _whiteListSenderAddresses; } }
        private HashSet<string> _excludedRecipients = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> excludedRecipients { get { return _excludedRecipients; } }
        private HashSet<string> _blackListSenderDomains = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> blackListSenderDomains { get { return _blackListSenderDomains; } }
        private List<string> _blackListWildcardSenderDomains = [];
        public List<string> blackListWildcardSenderDomains { get { return _blackListWildcardSenderDomains; } }
        private HashSet<string> _blackListSenderAddresses = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> blackListSenderAddresses {  get { return _blackListSenderAddresses; } }
        private List<string> _prohibitedTextInBody = [];
        public List<string> prohibitedTextInBody { get { return _prohibitedTextInBody; } }
        private List<string> _prohibitedRegExInBody = [];
        public List<string> prohibitedRegExInBody { get { return _prohibitedRegExInBody; }  }
        private string _goodMessage = string.Empty;
        public string goodMessage { get { return _goodMessage; } }
        private string _badMessage = string.Empty;
        public string badMessage {  get { return _badMessage; } }
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
                        _blackListSenderDomains.UnionWith(data.Where(e => !e.Contains("*")));
                        _blackListWildcardSenderDomains.Clear();
                        _blackListWildcardSenderDomains.AddRange(data.Where(e => e.Contains("*")));
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
                        _prohibitedTextInBody = data;
                    }
                },
                { FileTypes.prohibitedRegExInBodyFile, data =>
                    {
                        _prohibitedRegExInBody = data;
                    }
                },
                { FileTypes.goodMessageFile, data =>
                    {
                        _goodMessage = data.Count > 0 ? data[0] : "";
                    }
                },
                { FileTypes.badMessageFile, data =>
                    {
                        _badMessage = data.Count > 0 ? data[0] : "";
                    }
                }
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
