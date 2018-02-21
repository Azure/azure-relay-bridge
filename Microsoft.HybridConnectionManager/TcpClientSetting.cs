
namespace Microsoft.HybridConnectionManager
{
    using Microsoft.HybridConnectionManager.Configuration;

    public class TcpClientSetting : SettingsBase
    {
        public TcpClientSetting(TcpClientElement cfg):base(cfg.RelayConnectionString)
        {
            this.ResourceIdentifier = cfg.ResourceIdentifier;
            this.TargetHostName = cfg.TargetHost;
            this.TargetPort = cfg.TargetPort;
        }
        public string ResourceIdentifier { get; set; }
        public string TargetHostName { get; set; }
        public int TargetPort { get; set; }
    }
}
