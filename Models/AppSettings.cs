using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunigateAntispamHelper.Models
{
    public class AppSettings
    {

        public required string baseDir { get; init; } //The directory where CGPro places files
        public required string currentDir { get; init; } //The directory for your own needs 
        public required string excludedRecipientsFileName { get; init; }
        public required string whiteListSenderDomainsFileName { get; init; }
        public required string whiteListSenderAddressesFileName { get; init; }
        public required string blackListSenderDomainsFileName { get; init; }
        public required string blackListSenderAddressesFileName { get; init; }
        public required string prohibitedTextInBodyFileName { get; init; }
        public required string prohibitedRegExInBodyFileName { get; init; }

        public required string goodMessageFileName { get; init; }
        public required string badMessageFileName { get; init; }
        public required int updateIntervalInSeconds { get; init; }

    }


}
