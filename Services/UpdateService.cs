using CommunigateAntispamHelper.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("CGPAHTests")]
namespace CommunigateAntispamHelper.Services
{

    internal class UpdateService
    {
        private readonly AppSettings appSettings;
        private EmailChecker emailChecker;
        private CancellationTokenSource updateSource = new CancellationTokenSource();
        private int updateInterval = 60;
        private MonitoredFiles monitoredFiles;
        public UpdateService(AppSettings appSettings, EmailChecker emailChecker, MonitoredFiles files)
        {
            this.monitoredFiles = files;
            this.appSettings = appSettings;
            this.emailChecker = emailChecker;
            if (appSettings.updateIntervalInSeconds > 0)
            {
                updateInterval = appSettings.updateIntervalInSeconds;
            }
        }
        public void UpdateDataFirstTime()
        {
            monitoredFiles.CheckAllFiles();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var backgroundTask = Task.Run(() => { BackGroundLoop(); }, updateSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        private async Task BackGroundLoop()
        {
            while (!updateSource.Token.IsCancellationRequested)
            {
                await Task.Delay(1000 * updateInterval, updateSource.Token);
                while (!emailChecker.isUpdateAllowed || updateSource.Token.IsCancellationRequested)
                {
                }
                monitoredFiles.CheckAllFiles();
            }
        }
    }
}
