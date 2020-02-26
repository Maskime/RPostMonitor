using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RedditClientTest
{
    class Program
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
                    services.Configure<ApplicationConfiguration>(context.Configuration.GetSection(@"application"));

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
