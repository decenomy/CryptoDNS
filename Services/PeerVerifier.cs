using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoDNS.Models;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CryptoDNS.Services
{
    public class PeerVerifier
    {
        private readonly ILogger<PeerVerifier> logger;

        public PeerVerifier(
            ILogger<PeerVerifier> logger
        ) {
            this.logger = logger;
        }

        public async Task<bool> Verify(DomainSettings domainSettings, IPAddress ip)
        {
            var port = domainSettings.Port;

            try
            {
                using (var client = new TcpClient()) {

                    await client.ConnectAsync(ip, port); // just try to connect and free the connection immediately
                    client.Close(); 

                    return true;
                }
            }
            catch
            {
                logger.LogTrace($"Error connection to host { ip }:{ port }");
                return false;
            }
        }
    }
}