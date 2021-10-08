using System.Threading.Tasks;
using CryptoDNS.Repositories;
using Microsoft.Extensions.Logging;

namespace CryptoDNS.Jobs
{
    public class DomainsVerificationJob
    {
        private readonly ILogger<DomainsVerificationJob> logger;
        private readonly DomainsRepository domainsRepository;

        public DomainsVerificationJob(
            ILogger<DomainsVerificationJob> logger,
            DomainsRepository domainsRepository
        )
        {
            this.logger = logger;
            this.domainsRepository = domainsRepository;
        }

        public void Execute()
        {
            logger.LogInformation("DomainsVerificationJob Executing");
            domainsRepository.Verify();
            logger.LogInformation("DomainsVerificationJob Executed");
        }
    }
}