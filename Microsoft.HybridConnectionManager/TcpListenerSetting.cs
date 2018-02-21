
namespace Microsoft.HybridConnectionManager
{
    using Microsoft.HybridConnectionManager.Configuration;

    public class TcpListenerSetting : SettingsBase
    {
        public TcpListenerSetting(TcpListenerElement cfg):base(cfg.RelayConnectionString)
        {
            this.ListenHostName = cfg.ListenHostName;
            this.ListenPort = cfg.ListenPort;
        }       
        public string ListenHostName { get; set; }
        public int ListenPort { get; set; }
    }
}
