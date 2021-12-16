using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoDNS.Connectors;
using CryptoDNS.Jobs;
using CryptoDNS.Models;
using CryptoDNS.Repositories;
using CryptoDNS.Services;
using DNS.Client.RequestResolver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CryptoDNS
{

    public class Program
    {
        private static AppSettings appSettings = new AppSettings();

        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Add memory cache to DI
                    services.AddMemoryCache();

                    // Register configuration and services 
                    services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
                    services.AddSingleton<IRequestResolver, CryptoRequestResolver>();
                    services.AddSingleton<DomainsRepository>();
                    services.AddTransient<DaemonConnector>();
                    services.AddSingleton<PeerVerifier>();

                    // Register jobs
                    services.AddTransient<DaemonsRecurringJob>();
                    services.AddTransient<DaemonFetchPeersJob>();
                    services.AddTransient<DomainsCleanupJob>();
                    services.AddTransient<DomainsVerificationJob>();
                    services.AddTransient<GCCollectJob>();

                    // Register background services
                    services.AddHostedService<DnsService>();
                    services.AddHostedService<SchedulerService>();

                    // Register the service provider as the last registration operation
                    services.AddSingleton<IServiceProvider>(sp => sp);
                }).UseConsoleLifetime();
    }
}
