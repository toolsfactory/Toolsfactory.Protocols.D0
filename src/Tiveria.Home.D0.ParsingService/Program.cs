using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions;
using System.IO;
using System.Globalization;

namespace Tiveria.Home.D0.SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await BuildHost(args).RunConsoleAsync().ConfigureAwait(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Zeichenfolgen in Großbuchstaben normalisieren", Justification = "<Ausstehend>")]
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
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName.ToLower(CultureInfo.InvariantCulture)}.json", true)
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
