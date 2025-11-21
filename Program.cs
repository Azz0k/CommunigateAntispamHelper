using CommunigateAntispamHelper.Models;
using CommunigateAntispamHelper.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommunigateAntispamHelper
{
    internal class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
            IConfiguration config = builder.Build();
            var appSettings = config.GetSection("Settings").Get<AppSettings>();
            if (appSettings == null)
            {
                Console.Error.WriteLine("* Unable to read appsettings file.");
                return;
            }
            var serviceProvider = new ServiceCollection()
                .AddSingleton<EmailChecker>()
                .AddSingleton<AppSettings>(appSettings)
                .AddSingleton<UpdateService>()
                .AddSingleton<WorkerService>()
                .BuildServiceProvider();
            var updateService = serviceProvider.GetRequiredService<UpdateService>();
            var workerService = serviceProvider.GetRequiredService<WorkerService>();
            await updateService.UpdateDataFirstTime();
            workerService.Print("* ToCCAddressHelper Free");
            await workerService.Work();
        }
    }
}
