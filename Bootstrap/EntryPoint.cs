using System.Threading.Tasks;
using Common.Model.Repositories;
using Common.Reddit;
using DataAccess;
using DataAccess.Reddit;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PostPoller;
using PostUpdater;

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
                    services.AddHostedService<RGoddessesUpdater>();
                    services.AddHostedService<RGoddessesPoller>();
                    
                    services.Configure<PollerConfiguration>(context.Configuration.GetSection(PollerConfiguration.ConfigKey));
                    services.Configure<DatabaseSettings>(context.Configuration.GetSection(DatabaseSettings.ConfigKey));
                    services.Configure<RedditConfiguration>(context.Configuration.GetSection(RedditConfiguration.ConfigKey));
                    services.Configure<UpdaterConfiguration>(context.Configuration.GetSection(UpdaterConfiguration.ConfigKey));

                    services.AddSingleton<IMonitoredPostRepository, MonitoredPostRepository>();
                    services.AddSingleton<IRedditWrapper, RedditWrapper>();
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