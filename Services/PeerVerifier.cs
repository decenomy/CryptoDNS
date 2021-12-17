using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoDNS.Models;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CryptoDNS.Services
{
    public class PeerVerifier
    {
        private readonly ILogger<PeerVerifier> logger;

        public PeerVerifier(
            ILogger<PeerVerifier> logger
        )
        {
            this.logger = logger;
        }

        public async Task<bool> Verify(IPAddress ip, int port, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(ip, port, cancellationToken);

                    using var stream = client.GetStream();

                    var rnd = new Random((int)DateTime.Now.Ticks);

                    await stream.WriteAsync(Enumerable.Range(0, 4).Select(_ => (byte)rnd.Next()).ToArray(), 0, 4, cancellationToken);
                    await stream.FlushAsync(cancellationToken);

                    var ret = false;

                    try
                    {
                        if(client != null && client.Client != null) 
                        {
                            if(!(client.Client.Poll(100000, SelectMode.SelectRead) && client.Client.Available == 0)) 
                            {
                                ret = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        client.Close();
                    }

                    if(ret) logger.LogInformation($"Connection to host { ip }:{ port } is { (ret ? "OK" : "KO") }");
                    logger.LogTrace($"Connection to host { ip }:{ port } is { (ret ? "OK" : "KO") }");

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