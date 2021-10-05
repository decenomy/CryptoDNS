using System.Collections.Generic;
using CryptoDNS.Connectors;
using CryptoDNS.Models;
using CryptoDNS.Repositories;
using Microsoft.Extensions.Logging;

namespace CryptoDNS.Jobs
{
    public class DaemonFetchPeersJob
    {
        private readonly ILogger<DaemonFetchPeersJob> logger;
        private readonly DaemonConnector daemonConnector;
        private readonly DomainsRepository domainsRepository;

        public DaemonFetchPeersJob(
            ILogger<DaemonFetchPeersJob> logger,
            DaemonConnector daemonConnector,
            DomainsRepository domainsRepository
        )
        {
            this.logger = logger;
            this.daemonConnector = daemonConnector;
            this.domainsRepository = domainsRepository;
        }

        public void Execute(DomainSettings domain)
        {
            logger.LogInformation("DaemonFetchPeersJob Executing");

            logger.LogInformation("DaemonFetchPeersJob fetching masternodes");
            var masternodes =
                daemonConnector.ExecuteRpcCommand<List<MasternodeEntry>>(
                    domain, "listmasternodes");

            if (masternodes != null && masternodes.Count > 0)
            {
                foreach (var entry in masternodes)
                {
                    if (entry.IP.EndsWith(domain.Port.ToString()))
                    {
                        domainsRepository.Add(
                            domain.Domain,
                            entry.IP.Substring(
                                0,
                                entry.IP.LastIndexOf(":")
                            ).Replace("[", "").Replace("]", "")
                        );
                    }
                }
            }

            logger.LogInformation("DaemonFetchPeersJob fetching peers");

            var peers =
                daemonConnector.ExecuteRpcCommand<List<PeerEntry>>(
                    domain, "getpeerinfo");

            if (peers != null && peers.Count > 0)
            {
                foreach (var entry in peers)
                {
                    if (entry.Addr.EndsWith(domain.Port.ToString()))
                    {
                        domainsRepository.Add(
                            domain.Domain,
                            entry.Addr.Substring(
                                0,
                                entry.Addr.LastIndexOf(":")
                            ).Replace("[", "").Replace("]", "")
                        );
                    }
                }
            }

            logger.LogInformation("DaemonFetchPeersJob Executed");
        }
    }
}