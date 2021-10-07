using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using CryptoDNS.Models;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace CryptoDNS.Repositories
{
    public class DomainsRepository
    {
        private readonly ILogger<DomainsRepository> logger;
        private readonly AppSettings appSettings;

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<RecordType, ConcurrentDictionary<string, DomainEntry>>> entries =
            new ConcurrentDictionary<string, ConcurrentDictionary<RecordType, ConcurrentDictionary<string, DomainEntry>>>();

        public DomainsRepository(
            ILogger<DomainsRepository> logger,
            IOptions<AppSettings> appSettings
        )
        {
            this.logger = logger;
            this.appSettings = appSettings.Value;
        }

        public void Add(string domain, IPAddress ip)
        {
            var entry = new DomainEntry()
            {
                Domain = domain,
                IP = ip,
            };

            if (!entries.ContainsKey(domain))
            {
                entries[domain] = new ConcurrentDictionary<RecordType, ConcurrentDictionary<string, DomainEntry>>();
            }

            if (!entries[domain].ContainsKey(entry.RecordType))
            {
                entries[domain][entry.RecordType] = new ConcurrentDictionary<string, DomainEntry>();
            }

            if (!entries[domain][entry.RecordType].ContainsKey(entry.Id))
            {
                entries[domain][entry.RecordType][entry.Id] = entry;
            }
            else
            {
                entries[domain][entry.RecordType][entry.Id].LastSeen = DateTime.Now;
            }
        }

        public DomainEntry GetDomainEntry(string domain, IPAddress ip) {
            
            var entry = new DomainEntry()
            {
                Domain = domain,
                IP = ip,
            };

            if (entries.ContainsKey(domain) &&
                entries[domain].ContainsKey(entry.RecordType) &&
                entries[domain][entry.RecordType].ContainsKey(entry.Id))
            {
                return entries[domain][entry.RecordType][entry.Id];
            }

            return null;
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
                        if (entry.LastSeen < DateTime.Now.AddSeconds(-appSettings.TTL * 1.5))
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
                    OrderBy(d => Guid.NewGuid()).
                    Take(appSettings.NumberOfAnswers).
                    Select(d => d.ResourceRecord);
        }
    }
}