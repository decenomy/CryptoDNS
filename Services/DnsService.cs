using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CryptoDNS.Models;
using DNS.Client.RequestResolver;
using DNS.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoDNS.Services
{
    public class DnsService : BackgroundService
    {
        private readonly ILogger<DnsService> logger;
        private readonly AppSettings appSettings;
        private readonly IRequestResolver resolver;
        private DnsServer dnsServer;

        public DnsService(
            ILogger<DnsService> logger,
            IOptions<AppSettings> appSettings,
            IRequestResolver resolver
        )
        {
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.resolver = resolver;
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("DNS Service starting.");

                dnsServer = new DnsServer(resolver, "8.8.8.8");

                dnsServer.Requested += (sender, e) => logger.LogTrace("DNS Service requested: {0}", e.Request);
                dnsServer.Responded += (sender, e) => logger.LogTrace("DNS Service replied: {0} => {1}", e.Request, e.Response);
                dnsServer.Listening += (sender, e) => logger.LogInformation("DNS Service listening");
                dnsServer.Errored += (sender, e) => logger.LogError(e.Exception, "DNS Service an error as occurred: {0}", e.Exception.Message);

                await base.StartAsync(stoppingToken);

                logger.LogInformation("DNS Service started.");
            }
            catch
            {
                Environment.Exit(-1);

                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("DNS Service executing.");

            await dnsServer.Listen(ip: IPAddress.Parse(appSettings.ListenIP));

            logger.LogInformation("DNS Service executed.");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("DNS Service stopping.");

            if (dnsServer != null)
            {
                dnsServer.Dispose();
                dnsServer = null;
            }

            await base.StopAsync(stoppingToken);

            logger.LogInformation("DNS Service stopped.");
        }

        public override void Dispose()
        {
            logger.LogInformation("DNS Service disposing.");

            if (dnsServer != null)
            {
                dnsServer.Dispose();
                dnsServer = null;
            }

            logger.LogInformation("DNS Service disposed.");

            base.Dispose();
        }
    }
}
