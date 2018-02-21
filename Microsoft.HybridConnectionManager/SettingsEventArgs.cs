// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;

    class SettingsEventArgs : EventArgs
    {
        public SettingsEventArgs(SettingsBase info)
        {
            Info = info;
        }
        public SettingsBase Info { get; private set; }
    }
}
