using System;
using System.Diagnostics;
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
        private const int SCHEDULER_INTERVAL = 60 * 1000;

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

            var sw = new Stopwatch();

            while (true)
            {
                sw.Start();

                try
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var daemonsRecurringJob = serviceProvider.GetService<DaemonsRecurringJob>();

                    daemonsRecurringJob.Execute();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Scheduler Service: an error as ocurred executing the DaemonsRecurringJob.");
                    logger.LogError(ex, ex.StackTrace);
                }

                try
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var domainsCleanupJob = serviceProvider.GetService<DomainsCleanupJob>();

                    domainsCleanupJob.Execute();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Scheduler Service: an error as ocurred executing the DomainsCleanupJob.");
                    logger.LogError(ex, ex.StackTrace);
                }

                try
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var domainsVerificationJob = serviceProvider.GetService<DomainsVerificationJob>();

                    domainsVerificationJob.Execute();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Scheduler Service: an error as ocurred executing the DomainsVerificationJob.");
                    logger.LogError(ex, ex.StackTrace);
                }

                try
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var gcCollectJob = serviceProvider.GetService<GCCollectJob>();

                    gcCollectJob.Execute();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Scheduler Service: an error as ocurred executing the GCCollectJob.");
                    logger.LogError(ex, ex.StackTrace);
                }

                if (stoppingToken.IsCancellationRequested) break;

                sw.Stop();

                if(sw.ElapsedMilliseconds < SCHEDULER_INTERVAL) {
                    await Task.Delay((int)(SCHEDULER_INTERVAL - sw.ElapsedMilliseconds), stoppingToken);
                }

                sw.Reset();
            }

            logger.LogInformation("Scheduler Service executed.");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Scheduler Service stopping.");

            await base.StopAsync(stoppingToken);

            logger.LogInformation("Scheduler Service stopped.");
        }
    }
}