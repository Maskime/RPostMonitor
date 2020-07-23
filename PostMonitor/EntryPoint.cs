using System.Threading.Tasks;

using AutoMapper;

using Common.Model.Repositories;
using Common.Reddit;

using DataAccess;
using DataAccess.Config;
using DataAccess.RedditClient;
using DataAccess.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PostMonitor.Config;
using PostMonitor.HostedServices;

using Serilog;

namespace PostMonitor
{
    public static class EntryPoint
    {
        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args)
                  .Build()
                  .RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<PostPoller>();
                    services.AddHostedService<PostUpdater>();
                    // services.AddHostedService<CsvExport>();
                    
                    services.Configure<PollerConfiguration>(context.Configuration.GetSection(PollerConfiguration.ConfigKey));
                    services.Configure<DatabaseSettings>(context.Configuration.GetSection(DatabaseSettings.ConfigKey));
                    services.Configure<RedditConfiguration>(context.Configuration.GetSection(RedditConfiguration.ConfigKey));
                    services.Configure<UpdaterConfiguration>(context.Configuration.GetSection(UpdaterConfiguration.ConfigKey));
                    services.Configure<CsvExportConfiguration>(context.Configuration.GetSection(CsvExportConfiguration.ConfigKey));
                    
                    services.AddAutoMapper(typeof(DataAccessAutoMapperProfile), typeof(PostMonitorAutoMapperProfile));

                    services.AddSingleton<IDatabaseContext, DatabaseContext>();
                    services.AddSingleton<IMonitoredPostRepository, MonitoredPostRepository>();
                    services.AddSingleton<IWatchedSubRedditRepository, WatchedSubRedditRepository>();
                    
                    services.AddSingleton<IRedditClientWrapper, RedditClientWrapper>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    Log.Logger = new LoggerConfiguration()
                                 .ReadFrom.Configuration(context.Configuration)
                                 .CreateLogger();
                    logging.AddSerilog(dispose: true);
                })
                .ConfigureAppConfiguration((config) =>
                {
                    config.AddJsonFile(@"appsettings.json", optional: false, reloadOnChange: true);
                });
    }
}