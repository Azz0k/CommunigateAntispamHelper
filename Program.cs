using CommunigateAntispamHelper.Models;
using CommunigateAntispamHelper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static CommunigateAntispamHelper.Utils.Utils;

namespace CommunigateAntispamHelper
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
            IConfiguration config = builder.Build();
            var appSettings = config.GetSection("Settings").Get<AppSettings>();
            if (appSettings == null)
            {
                Console.Error.WriteLine("* CommunigateAntispamHelper. Unable to read appsettings file.");
                return;
            }
            var serviceProvider = new ServiceCollection()
                .AddSingleton<EmailChecker>()
                .AddSingleton<AppSettings>(appSettings)
                .AddSingleton<UpdateService>()
                .AddSingleton<WorkerService>()
                .AddSingleton<MonitoredFiles>()
                .BuildServiceProvider();
            var updateService = serviceProvider.GetRequiredService<UpdateService>();
            var workerService = serviceProvider.GetRequiredService<WorkerService>();
            updateService.UpdateDataFirstTime();
            Print("* CommunigateAntispamHelper Free");
            await workerService.Work();
        }
    }
}
