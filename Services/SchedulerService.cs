using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoDNS.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CryptoDNS.Services
{
    public class SchedulerService : BackgroundService
    {
        private readonly ILogger<SchedulerService> logger;
        private readonly IServiceProvider serviceProvider;

        public SchedulerService(
            ILogger<SchedulerService> logger,
            IServiceProvider serviceProvider
        )
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Scheduler Service starting.");

            await base.StartAsync(stoppingToken);

            logger.LogInformation("Scheduler Service started.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Scheduler Service executing.");

            while (true)
            {
                if(stoppingToken.IsCancellationRequested) break;

                var daemonsRecurringJob = serviceProvider.GetService<DaemonsRecurringJob>();

                await daemonsRecurringJob.Execute();

                if(stoppingToken.IsCancellationRequested) break;

                var domainsCleanupJob = serviceProvider.GetService<DomainsCleanupJob>();

                domainsCleanupJob.Execute();

                if(stoppingToken.IsCancellationRequested) break;

                var gcCollectJob = serviceProvider.GetService<GCCollectJob>();

                gcCollectJob.Execute();

                if(stoppingToken.IsCancellationRequested) break;

                await Task.Delay(60 * 1000, stoppingToken);
            }

            logger.LogInformation("Scheduler Service executed.");

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Scheduler Service stopping.");

            await base.StopAsync(stoppingToken);

            logger.LogInformation("Scheduler Service stopped.");
        }
    }
}