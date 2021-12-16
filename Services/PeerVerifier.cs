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

                    var stream = client.GetStream();

                    await stream.WriteAsync(new byte[4] { 0xf9, 0xbe, 0xb4, 0xd9 }, 0, 4);

                    var ret = client.Connected;

                    client.Close();

                    return ret;
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