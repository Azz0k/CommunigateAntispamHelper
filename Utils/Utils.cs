using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CommunigateAntispamHelper.Utils
{
    internal static class Utils
    {
        public static string GetAddressFromToHeader(string headerTo)
        {
            string pattern = @".*<(.*)>";
            Match regexMatch = Regex.Match(headerTo, pattern);
            if (regexMatch.Success)
            {
                string recipient = regexMatch.Groups[1].Value;
                return recipient;
            }
            return "";
        }
    }
}
