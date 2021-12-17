using System.Threading.Tasks;
using CryptoDNS.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Linq;

namespace CryptoDNS.Jobs
{
    public class DaemonsRecurringJob
    {
        private readonly ILogger<DaemonsRecurringJob> logger;
        private readonly AppSettings appSettings;
        private readonly IServiceProvider serviceProvider;

        public DaemonsRecurringJob(
            ILogger<DaemonsRecurringJob> logger,
            IOptions<AppSettings> appSettings,
            IServiceProvider serviceProvider
        )
        {
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.serviceProvider = serviceProvider;
        }

        public async Task Execute(CancellationToken cancellationToken)
        {
            logger.LogInformation("DaemonsRecurringJob Executing");

            var daemonFetchPeersJob = serviceProvider.GetService<DaemonFetchPeersJob>();

            var tasks = appSettings.Domains.Select(async domain => await daemonFetchPeersJob.Execute(domain, cancellationToken));

            await Task.WhenAll(tasks);

            logger.LogInformation("DaemonsRecurringJob Executed");
        }
    }
}