using System.Diagnostics;
using System.Linq;
using CryptoDNS.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CryptoDNS.Connectors
{
    public class DaemonConnector
    {
        private readonly AppSettings appSettings;

        public DaemonConnector(
            IOptions<AppSettings> appSettings
        )
        {
            this.appSettings = appSettings.Value;
        }

        public T ExecuteRpcCommand<T>(
            DomainSettings domainSettings,
            string command,
            params string[] arguments)
        {

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

            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}