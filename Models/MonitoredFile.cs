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
        public bool IsChanged { get; set; } = false;
        public required FileTypes FileType { get; set; }

        [SetsRequiredMembers]
        public MonitoredFile(string fullName, FileTypes fileType)
        {
            FullName = fullName;
            FileType = fileType;
        }
    }


}
