using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CommunigateAntispamHelper.Utils
{
    internal static class Utils
    {
        public static void Print(string message)
        {
            Console.WriteLine(message);
            Console.Out.Flush();
        }

    }
}
