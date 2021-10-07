using System;
using System.Net;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace CryptoDNS.Models
{
    public class DomainEntry
    {
        public string Id => $"{Domain}_{IP}";
        public string Domain { get; set; }
        public IPAddress IP { get; set; }
        public DateTime LastSeen { get; set; } = DateTime.Now;

        public RecordType RecordType =>
            this.IP.GetAddressBytes().Length == 4 ?
            RecordType.A :
            RecordType.AAAA;

        public IPAddressResourceRecord ResourceRecord =>
            new IPAddressResourceRecord(
                new Domain(this.Domain),
                this.IP,
                new TimeSpan(0)
            );
    }
}