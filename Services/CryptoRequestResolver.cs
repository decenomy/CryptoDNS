using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using DNS.Client.RequestResolver;
using Microsoft.Extensions.Logging;
using CryptoDNS.Models;
using Microsoft.Extensions.Options;
using CryptoDNS.Repositories;
using System;
using Microsoft.Extensions.Caching.Memory;

namespace CryptoDNS.Services
{
    public class CryptoRequestResolver : IRequestResolver
    {
        private readonly ILogger<DnsService> logger;
        private readonly AppSettings appSettings;
        private readonly DomainsRepository domainsRepository;
        private readonly IMemoryCache cache;

        public CryptoRequestResolver(
            ILogger<DnsService> logger,
            IOptions<AppSettings> appSettings,
            DomainsRepository domainsRepository,
            IMemoryCache cache
        )
        {
            this.logger = logger;
            this.appSettings = appSettings.Value;
            this.domainsRepository = domainsRepository;
            this.cache = cache;
        }

        public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var answers = default(List<IResourceRecord>);
            var key = string.Join("/", request.Questions.Select(q => $"{q.Name}_{q.Type}"));

            if (!cache.TryGetValue(key, out answers))
            {
                answers = new List<IResourceRecord>();

                foreach (var question in request.Questions)
                {
                    if (question.Type == RecordType.A ||
                        question.Type == RecordType.AAAA)
                    {
                        answers.AddRange(
                            domainsRepository.GetResourceRecords(
                                question.Name.ToString(), question.Type));
                    }
                }

                if (answers.Count > 0)
                {
                    cache.Set(
                        key,
                        answers,
                        new MemoryCacheEntryOptions().
                            SetAbsoluteExpiration(
                                TimeSpan.FromSeconds(appSettings.AnswersCacheTTL)
                            )
                    );
                }
            }

            var response = Response.FromRequest(request) as IResponse;

            response.AnswerRecords.Clear();
            foreach (var answer in answers ?? new List<IResourceRecord>())
            {
                response.AnswerRecords.Add(answer);
            }

            return Task.FromResult(response);
        }
    }
}
