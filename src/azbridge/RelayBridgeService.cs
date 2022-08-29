using Microsoft.Azure.Relay.Bridge.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
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
            DiagnosticListener.AllListeners.Subscribe(new SubscriberObserver(logger));
        }

        public RelayBridgeService(Config settings, ILogger<RelayBridgeService> logger):this(settings, (ILogger)logger)
        {
            
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (host == null)
                {
                    host = new Host(settings);
                }
                return base.StartAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                Environment.Exit(1);
                return Task.CompletedTask;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (host != null)
                {
                    host.Stop();
                    host = null;
                }
                return base.StopAsync(cancellationToken);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                Environment.Exit(1);
                return Task.CompletedTask;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (host != null)
                {
                    host.Start();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                Environment.Exit(1);
            }
            return Task.CompletedTask;
        }
    }

}
