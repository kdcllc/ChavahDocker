using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChavahDbImporter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine(args);

            var host = new HostBuilder()
                 .ConfigureHostConfiguration(configHost =>
                 {
                     configHost.SetBasePath(Directory.GetCurrentDirectory());
                     configHost.AddJsonFile("hostsettings.json", optional: true);
                     configHost.AddEnvironmentVariables(prefix: "PREFIX_");
                     configHost.AddCommandLine(args);
                 })
                 .ConfigureAppConfiguration((hostContext, configApp) =>
                 {
                     configApp.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                     configApp.AddJsonFile(
                         $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                         optional: true);
                     configApp.AddEnvironmentVariables(prefix: "PREFIX_");
                     configApp.AddCommandLine(args);

                     // hostContext.HostingEnvironment.ContentRootPath = Directory.GetCurrentDirectory();
                 })
                 .ConfigureLogging((hostContext, configLogging) =>
                 {
                     configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                     configLogging.AddConsole();
                     configLogging.AddDebug();
                 })
                 .ConfigureServices((hostingContext,services) =>
                 {
                     services.Configure<DatabaseSettings>(hostingContext.Configuration.GetSection("Database"));
                     services.AddSingleton(x => x.GetRequiredService<IOptions<DatabaseSettings>>().Value);
                     services.AddSingleton<IHostedService, RavenDbService>();
                 })
                 .UseConsoleLifetime()
                 .Build();

            var srv = host.Services;

            using (host)
            {
                var logger = srv.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Program));

                logger.LogInformation("Start {name}", nameof(ChavahDbImporter));

                await host.RunAsync();
                logger.LogInformation("Running {name}", nameof(ChavahDbImporter));

                await host.WaitForShutdownAsync();

                logger.LogInformation("Shutdown {name}", nameof(ChavahDbImporter));
            }
        }
    }
}
