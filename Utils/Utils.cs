using CommunigateAntispamHelper.Models;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Ude;

namespace CommunigateAntispamHelper.Utils
{
    internal static class Utils
    {
        public static bool IsEqualWithWildcard(string senderDomain, string domainPattern)
        {
            string regexPattern = $"^{Regex.Escape(domainPattern).Replace(@"\*", ".*")}$";
            Regex regex = new(regexPattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(senderDomain);

        }
        public static void Print(string message)
        {
            Console.WriteLine(message);
            Console.Out.Flush();
        }
        public static string? ReadAsciiLine(BinaryReader br)
        {
            using var ms = new MemoryStream();
            while (true)
            {
                int b;
                try { b = br.ReadByte(); }
                catch { return null; }

                if (b == '\n') break;
                if (b == '\r') continue;
                ms.WriteByte((byte)b);
            }
            return Encoding.ASCII.GetString(ms.ToArray());
        }
        public static string DetectCharsetAndDecode(byte[] data)
        {
            var detector = new CharsetDetector();
            detector.Feed(data, 0, data.Length);
            detector.DataEnd();
            var charset = detector.Charset;
            Encoding CodePage = Encoding.GetEncoding(charset);
            return CodePage.GetString(data);
        }
        public static string? GetRecipient(string line)
        {
            string pattern = @".*<(.*)>";
            if (line.StartsWith("R W "))
            {
                Match regexMatch = Regex.Match(line, pattern);
                if (regexMatch.Success)
                {
                    string recipient = regexMatch.Groups[1].Value;
                    return recipient;
                }
            }
            return null;
        }
        public static string? GetSender(string line)
        {
            string pattern = @".*<(.*)>";
            if (line.StartsWith("P I "))
            {
                Match regexMatch = Regex.Match(line, pattern);
                if (regexMatch.Success)
                {
                    string sender = regexMatch.Groups[1].Value;
                    return sender;
                }
            }
            return null;
        }

    }

}
