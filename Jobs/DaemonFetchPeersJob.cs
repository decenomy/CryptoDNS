using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CryptoDNS.Connectors;
using CryptoDNS.Models;
using CryptoDNS.Repositories;
using CryptoDNS.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoDNS.Jobs
{
    public class DaemonFetchPeersJob
    {
        private readonly ILogger<DaemonFetchPeersJob> logger;
        private readonly AppSettings appSettings;
        private readonly DaemonConnector daemonConnector;
        private readonly DomainsRepository domainsRepository;

        public DaemonFetchPeersJob(
            ILogger<DaemonFetchPeersJob> logger,
            IOptions<AppSettings> appSettings,
            DaemonConnector daemonConnector,
            DomainsRepository domainsRepository
        )
        {
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.daemonConnector = daemonConnector;
            this.domainsRepository = domainsRepository;
        }

        public async Task Execute(DomainSettings domain)
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
                        var ip = entry.IP.Substring(
                                0,
                                entry.IP.LastIndexOf(":")
                            ).Replace("[", "").Replace("]", "");

                        if (!IPAddress.TryParse(ip, out var ipAddress)) continue;

                        await domainsRepository.Add(
                            domain.Domain,
                            ipAddress
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
                        var ip = entry.Addr.Substring(
                                0,
                                entry.Addr.LastIndexOf(":")
                            ).Replace("[", "").Replace("]", "");

                        if (!IPAddress.TryParse(ip, out var ipAddress)) continue;

                        await domainsRepository.Add(
                            domain.Domain,
                            ipAddress
                        );
                    }
                }
            }

            logger.LogInformation("DaemonFetchPeersJob Executed");
        }
    }
}