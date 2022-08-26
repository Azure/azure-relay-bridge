using Microsoft.Azure.Relay.Bridge.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Host = Microsoft.Azure.Relay.Bridge.Host;

namespace azbridge
{
    public class RelayBridgeService : BackgroundService
    {
        private readonly Config settings;
        private readonly ILogger _logger;
        private Host host;

        public RelayBridgeService(Config settings, ILogger logger)
        {
            this.settings = settings;
            _logger = logger;
        }

        
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (host == null)
            {
                host = new Host(settings);
            }
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if ( host != null )
            {
                host.Stop();
                host = null;
            }
            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (host != null)
            {
                host.Start();
            }
            return Task.CompletedTask;
        }
    }

}
