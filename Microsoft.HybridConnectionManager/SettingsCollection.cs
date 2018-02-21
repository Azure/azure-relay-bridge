// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    abstract class SettingsCollection<T> : Dictionary<string, T> where T : SettingsBase
    {
        private string configFileName;
        FileSystemWatcher settingsFileWatcher;

        public string ConfigFileName { get => configFileName; }

        public event EventHandler<SettingsEventArgs> InfoAdded;
        public event EventHandler<SettingsEventArgs> InfoRemoved;
        public event EventHandler<SettingsEventArgs> InfoChanged;

        public SettingsCollection(string configFileName)
        {
            if (string.IsNullOrEmpty(configFileName) || !File.Exists(configFileName))
            {
                throw new ArgumentException("TODO");
            }

            this.configFileName = configFileName;
            this.settingsFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(configFileName), Path.GetFileName(configFileName))
            {
                NotifyFilter = NotifyFilters.LastWrite
            };
            this.settingsFileWatcher.Changed += SettingsFileWatcher_Changed;
            this.settingsFileWatcher.EnableRaisingEvents = true;
        }

        void SettingsFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadConfiguration();
        }

        protected abstract void LoadConfiguration();

        public new void Add(string key, T data)
        {
            base.Add(key, data);
            OnInfoAdded(data);
        }

        public new void Remove(string key)
        {
            T hci;

            if (this.TryGetValue(key, out hci))
            {
                base.Remove(key);
                OnInfoRemoved(hci);
            }
        }

        public new T this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                base[key] = value;
                OnInfoChanged(value);
            }
        }

        private void OnInfoAdded(T data)
        {
            InfoAdded?.Invoke(this, new SettingsEventArgs(data));
        }

        private void OnInfoRemoved(T data)
        {
            InfoRemoved?.Invoke(this, new SettingsEventArgs(data));
        }

        private void OnInfoChanged(T data)
        {
            InfoChanged?.Invoke(this, new SettingsEventArgs(data));
        }
    }
}
