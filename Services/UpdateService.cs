using CommunigateAntispamHelper.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CommunigateAntispamHelper.Services
{

    internal class UpdateService
    {
        private readonly AppSettings appSettings;
        public UpdateService(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }
        public async Task UpdateDataFirstTime()
        {

        }
    }
}
