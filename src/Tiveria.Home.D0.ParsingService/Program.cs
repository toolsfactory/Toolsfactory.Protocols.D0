using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions;
using System.IO;

namespace Tiveria.Home.D0.SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await BuildHost(args).RunConsoleAsync();
        }

        static IHostBuilder BuildHost(string[] args)
        {
            return new HostBuilder()
                .ConfigureHostConfiguration((config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName.ToLower()}.json", true)
                        .AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions()
                        .Configure<DaemonConfig>(hostContext.Configuration.GetSection("Daemon"))
                        .AddSingleton<IHostedService, DaemonService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .CreateLogger();
                    logging.AddSerilog(logger);
                })
                .UseSystemd() ;
        }
    }
}
