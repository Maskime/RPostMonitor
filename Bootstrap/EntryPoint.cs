using System.Threading.Tasks;
using Common.Model.Repositories;
using DataAccess;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PostPoller;

namespace Bootstrap
{
    public static class EntryPoint
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<RGoddessesPoller>();
                    services.Configure<PollerConfiguration>(context.Configuration.GetSection(PollerConfiguration.ConfigKey));
                    services.Configure<DatabaseSettings>(context.Configuration.GetSection(DatabaseSettings.ConfigKey));

                    services.AddSingleton<IMonitoredPostRepository, MonitoredPostRepository>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection(@"Logging"));
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((config) =>
                {
                    config.AddJsonFile(@"appsettings.json", optional: false, reloadOnChange: true);
                });
    }
}