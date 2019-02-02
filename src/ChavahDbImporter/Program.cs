using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

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
                 })
                 .ConfigureLogging((hostContext, configLogging) =>
                 {
                     configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                     configLogging.AddConsole();
                     configLogging.AddDebug();
                 })
                 .ConfigureServices(services =>
                 {
                     services.AddSingleton<IHostedService, RavenDbService>();
                 })
                 .UseConsoleLifetime()
                 .Build();

            var srv = host.Services;

            using (host)
            {
                await host.RunAsync();

                await host.WaitForShutdownAsync();
            }
        }

    }
}
