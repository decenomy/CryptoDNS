using System;
using Microsoft.Extensions.Logging;

namespace CryptoDNS.Jobs
{
    public class GCCollectJob
    {
        private readonly ILogger<GCCollectJob> logger;

        public GCCollectJob(
            ILogger<GCCollectJob> logger
        )
        {
            this.logger = logger;
        }

        public void Execute()
        {
            logger.LogInformation("GCCollectJob Executing");
            GC.Collect(0, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            logger.LogInformation("GCCollectJob Executed");
        }
    }
}