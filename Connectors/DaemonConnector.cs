using System.Diagnostics;
using System.Linq;
using CryptoDNS.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CryptoDNS.Connectors
{
    public class DaemonConnector
    {
        private readonly ILogger<DaemonConnector> logger;

        public DaemonConnector(
            ILogger<DaemonConnector> logger
        )
        {
            this.logger = logger;
        }

        public T ExecuteRpcCommand<T>(
            DomainSettings domainSettings,
            string command,
            params string[] arguments)
        {
            logger.LogInformation($"ExecuteRpcCommand { command }({ string.Join(", ", arguments) }) Executing for { domainSettings.Domain }");

            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = domainSettings.CliExecutable,
                    Arguments = string.Join(" ", new string[] { command }.Concat(arguments)),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            p.Start();

            var response = p.StandardOutput.ReadToEnd();

            var ret = JsonConvert.DeserializeObject<T>(response);

            logger.LogInformation($"ExecuteRpcCommand { command }({ string.Join(", ", arguments) }) Executed for { domainSettings.Domain }");

            return ret;
        }
    }
}