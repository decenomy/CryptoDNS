using System;
using System.Collections.Generic;
using System.Linq;
using CryptoDNS.Models;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Collections.Concurrent;
using CryptoDNS.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace CryptoDNS.Repositories
{
    public class DomainsRepository
    {
        private readonly ILogger<DomainsRepository> logger;
        private readonly AppSettings appSettings;
        private readonly PeerVerifier peerVerifier;
        
        private bool firstTime = true; 

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<RecordType, ConcurrentDictionary<string, DomainEntry>>> entries =
            new ConcurrentDictionary<string, ConcurrentDictionary<RecordType, ConcurrentDictionary<string, DomainEntry>>>();

        private readonly Dictionary<string, DomainSettings> domains;

        public DomainsRepository(
            ILogger<DomainsRepository> logger,
            IOptions<AppSettings> appSettings,
            PeerVerifier peerVerifier
        )
        {
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.peerVerifier = peerVerifier;

            domains = new Dictionary<string, DomainSettings>(
                this.appSettings.Domains.ToDictionary(d => d.Domain)
            );
        }

        public void Add(string domain, IPAddress ip)
        {
            if (!domains.ContainsKey(domain)) return;

            if (!entries.ContainsKey(domain))
            {
                entries[domain] = new ConcurrentDictionary<RecordType, ConcurrentDictionary<string, DomainEntry>>();
            }

            var entry = new DomainEntry()
            {
                Domain = domain,
                IP = ip,
            };

            if (!entries[domain].ContainsKey(entry.RecordType))
            {
                entries[domain][entry.RecordType] = new ConcurrentDictionary<string, DomainEntry>();
            }

            if (!entries[domain][entry.RecordType].ContainsKey(entry.Id))
            {
                entries[domain][entry.RecordType][entry.Id] = entry;
            }
        }

        public void Cleanup()
        {
            foreach (var domain in entries.Keys)
            {
                foreach (var recordType in entries[domain].Keys)
                {
                    var toDelete = new List<string>();

                    foreach (var entry in entries[domain][recordType].Values)
                    {
                        if (entry.LastSeen < DateTime.Now.AddSeconds(-appSettings.TTL))
                        {
                            toDelete.Add(entry.Id);
                        }
                    }

                    foreach (var entryId in toDelete)
                    {
                        ((IDictionary<string, DomainEntry>)entries[domain][recordType]).Remove(entryId);
                    }
                }
            }
        }

        public async Task Verify(CancellationToken cancellationToken)
        {
            var toVerify = 
                (from domain in entries.Keys
                 from recordType in entries[domain].Keys
                 from entry in entries[domain][recordType].Values
                 where entry.LastVerified < DateTime.Now.AddSeconds(-appSettings.VerifyInterval)
                 orderby entry.LastVerified 
                 select entry);

            toVerify = toVerify.
                Where(e => e.IP.AddressFamily == AddressFamily.InterNetwork).
                Take(firstTime ? int.MaxValue : 10).Concat(
                    toVerify.
                    Where(e => e.IP.AddressFamily == AddressFamily.InterNetworkV6).
                    Take(firstTime ? int.MaxValue : 10)
                );

            var tasks = toVerify.Select(async entry => {
                entry.Online = await peerVerifier.Verify(entry.IP, domains[entry.Domain].Port, cancellationToken);
                if (entry.Online)
                {
                    entry.LastSeen = DateTime.Now;
                }
                entry.LastVerified = DateTime.Now;
            });

            await Task.WhenAll(tasks);

            firstTime = false;
        }

        public IEnumerable<IResourceRecord> GetResourceRecords(string domain, RecordType recordType)
        {
            if (!entries.ContainsKey(domain))
            {
                entries[domain] = new ConcurrentDictionary<RecordType, ConcurrentDictionary<string, DomainEntry>>();
            }

            if (!entries[domain].ContainsKey(recordType))
            {
                entries[domain][recordType] = new ConcurrentDictionary<string, DomainEntry>();
            }

            return
                entries[domain][recordType].Values.
                    Where(e => e.Online).
                    OrderBy(e => Guid.NewGuid()).
                    Take(appSettings.NumberOfAnswers).
                    Select(e => e.ResourceRecord);
        }
    }
}