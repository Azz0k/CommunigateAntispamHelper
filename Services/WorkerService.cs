using CommunigateAntispamHelper.Models;

using System;
using System.Collections.Generic;
using System.Text;

namespace CommunigateAntispamHelper.Services
{
    internal class WorkerService
    {
        private EmailChecker emailChecker;
        private readonly AppSettings appSettings;
        public WorkerService(AppSettings appSettings, EmailChecker emailChecker)
        {
            this.appSettings = appSettings;
            this.emailChecker = emailChecker;
        }
        public void Print(string message)
        {
            Console.WriteLine(message);
            Console.Out.Flush();
        }
        public async Task Work()
        {

        }
    }
}
