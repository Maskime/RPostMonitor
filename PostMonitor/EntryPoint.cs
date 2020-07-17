using System;
using System.Threading.Tasks;

using AutoMapper;

using Common.Model.Repositories;
using Common.Reddit;

using DataAccess.Config;
using DataAccess.RedditClient;
using DataAccess.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using PostMonitor.Config;
using PostMonitor.Poller;
using PostMonitor.Updater;
using Serilog;

namespace PostMonitor
{
    public static class EntryPoint
    {
        static async Task Main(string[] args)
        {
            // try
            // {
                // Log.Information(@"Starting application");
                await CreateHostBuilder(args)
                      .Build()
                      .RunAsync();
            // }
            // catch (Exception exception)
            // {
            //     Log.Error(exception, @"Application crashed");
            // }
            // finally
            // {
            //     Log.CloseAndFlush();
            // }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<PostUpdater>();
                    services.AddHostedService<PostPoller>();
                    
                    services.Configure<PollerConfiguration>(context.Configuration.GetSection(PollerConfiguration.ConfigKey));
                    services.Configure<DatabaseSettings>(context.Configuration.GetSection(DatabaseSettings.ConfigKey));
                    services.Configure<RedditConfiguration>(context.Configuration.GetSection(RedditConfiguration.ConfigKey));
                    services.Configure<UpdaterConfiguration>(context.Configuration.GetSection(UpdaterConfiguration.ConfigKey));
                    services.AddAutoMapper(typeof(DataAccessAutoMapperProfile), typeof(PostMonitorAutoMapperProfile));

                    services.AddSingleton<IMonitoredPostRepository, MonitoredPostRepository>();
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