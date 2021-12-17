using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<T> ExecuteRpcCommand<T>(
            DomainSettings domainSettings,
            CancellationToken cancellationToken,
            string command,
            params string[] arguments
        )
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
                    CreateNoWindow = true,
                }
            };

            if(!p.Start()) return default(T);

            var response = await p.StandardOutput.ReadToEndAsync();

            var ret = JsonConvert.DeserializeObject<T>(response);

            logger.LogInformation($"ExecuteRpcCommand { command }({ string.Join(", ", arguments) }) Executed for { domainSettings.Domain }");

            return ret;
        }
    }
}