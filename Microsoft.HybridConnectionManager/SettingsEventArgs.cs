using System;

namespace Microsoft.HybridConnectionManager
{
    public class SettingsEventArgs : EventArgs
    {
        public SettingsEventArgs(SettingsBase info)
        {
            Info = info;
        }
        public SettingsBase Info { get; private set; }
    }
}
