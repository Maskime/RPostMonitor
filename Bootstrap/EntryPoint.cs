using System.Threading.Tasks;
using Common.Config;
using Common.Model.Repositories;
using DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostPoller;
using PostPoller.Config;

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
                .ConfigureServices(((context, services) =>
                {
                    services.AddHostedService<RGoddessesPoller>();
                    services.Configure<IApplicationConfiguration>(context.Configuration.GetSection(@"Application"));
                    services
                        .AddSingleton<IApplicationConfiguration>(
                            sp => sp.GetRequiredService<IOptions<ApplicationConfiguration>>().Value);
                    // services
                    //     .Configure<IDatabaseSettings>(context.Configuration.GetSection(@"DatabaseSettings"));
                    // services
                    //     .AddSingleton<IDatabaseSettings>(
                    //         sp => sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
                    // services.AddSingleton<IMonitoredPostRepository, MonitoredPostRepository>();

                }))
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(context.Configuration.GetSection(@"Logging"));
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile(@"appsettings.json", optional: false, reloadOnChange: true);
                });
    }
}