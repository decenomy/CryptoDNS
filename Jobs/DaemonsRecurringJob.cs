using CryptoDNS.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

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

        public void Execute()
        {
            logger.LogInformation("DaemonsRecurringJob Executing");

            var daemonFetchPeersJob = serviceProvider.GetService<DaemonFetchPeersJob>();

            foreach (var domain in appSettings.Domains)
            {
                daemonFetchPeersJob.Execute(domain);
            }
            logger.LogInformation("DaemonsRecurringJob Executed");
        }
    }
}