using System.Collections.Generic;

namespace CryptoDNS.Models
{
    public class AppSettings
    {
        public bool Debug { get; set; } = false;
        public string ListenIP { get; set; }
        public int TTL { get; set; }
        public int NumberOfAnswers { get; set; } = 10;
        public int AnswersCacheTTL { get; set; } = 10;
        public IEnumerable<DomainSettings> Domains { get; set; }
    }
}