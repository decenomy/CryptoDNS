using CryptoDNS.Repositories;
using Microsoft.Extensions.Logging;

namespace CryptoDNS.Jobs
{
    public class DomainsCleanupJob
    {
        private readonly ILogger<DomainsCleanupJob> logger;
        private readonly DomainsRepository domainsRepository;

        public DomainsCleanupJob(
            ILogger<DomainsCleanupJob> logger,
            DomainsRepository domainsRepository
        )
        {
            this.logger = logger;
            this.domainsRepository = domainsRepository;
        }

        public void Execute()
        {
            logger.LogInformation("DomainsCleanupJob Executing");
            domainsRepository.Cleanup();
            logger.LogInformation("DomainsCleanupJob Executed");
        }
    }
}