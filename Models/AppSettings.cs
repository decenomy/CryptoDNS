using System.Collections.Generic;

namespace CryptoDNS.Models
{
    public class AppSettings
    {
        // add your external network interface here
        public string ListenIP { get; set; }
        // let it last one day even when offline
        public int TTL { get; set; } = 24 * 60 * 60;
        // this is the interval that service takes to test the connection again
        public int VerifyInterval { get; set; } = 600;
        // max number of DNS answers for each type og DNS query 
        public int NumberOfAnswers { get; set; } = 10;
        // number of seconds that a response lives without issuing a new calculation
        public int AnswersCacheTTL { get; set; } = 10;
        // domains configuration
        public IEnumerable<DomainSettings> Domains { get; set; }
    }
}