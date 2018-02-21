// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionInstallHelper
{
    using System;
    using System.IO;
    using Microsoft.Deployment.WindowsInstaller;
    using Microsoft.Win32;

    public class CustomActions
    {
        public const string HybridConnectionsRegistryPath = @"SOFTWARE\Microsoft\HybridConnectionManager";
        public const string InstallDirKeyName = "installDir";

        [CustomAction]
        public static ActionResult CopyHybridConnectionConfigIfExists(Session session)
        {
            session.Log("Checking if previous config exists");
            string previousInstallDir = PreviousInstallDirectory(session);

            if (previousInstallDir == null)
            {
                session.Log("Could not find previous install, skipping");
                return ActionResult.Success;
            }

            session.Log("Using previous install dir " + previousInstallDir);

            if (!File.Exists(Path.Combine(previousInstallDir, "Microsoft.HybridConnectionManager.Listener.exe.config")))
            {
                session.Log("Previous config file does not exist, skipping.");
                return ActionResult.Success;
            }

            string newInstallDir = session.CustomActionData["INSTALLDIR"];

            session.Log("Using new install dir: " + newInstallDir);

            if (!Directory.Exists(newInstallDir))
            {
                session.Log("New install directory does not exist yet, creating");
                Directory.CreateDirectory(newInstallDir);
            }

            session.Log("Copying old config to new directory (with .old postfix)");

            File.Copy(
                Path.Combine(previousInstallDir, "Microsoft.HybridConnectionManager.Listener.exe.config"),
                Path.Combine(newInstallDir, "Microsoft.HybridConnectionManager.Listener.exe.config.old"));

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult OverwriteConfigIfOldExists(Session session)
        {
            string newInstallDir = session.CustomActionData["INSTALLDIR"];

            session.Log("Using new install dir: " + newInstallDir);

            if (!File.Exists(Path.Combine(newInstallDir, "Microsoft.HybridConnectionManager.Listener.exe.config.old")))
            {
                session.Log("No old config exists, skipping.");
                return ActionResult.Success;
            }

            session.Log("Copying and overwriting config");

            File.Copy(
                Path.Combine(newInstallDir, "Microsoft.HybridConnectionManager.Listener.exe.config.old"),
                Path.Combine(newInstallDir, "Microsoft.HybridConnectionManager.Listener.exe.config"),
                overwrite: true);

            session.Log("Deleting old config");
            File.Delete(Path.Combine(newInstallDir, "Microsoft.HybridConnectionManager.Listener.exe.config.old"));

            return ActionResult.Success;
        }

        static string PreviousInstallDirectory(Session session)
        {
            // Enumerate registries
            var hklm64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            using (RegistryKey registryKey = hklm64.OpenSubKey(HybridConnectionsRegistryPath))
            {
                if (registryKey == null)
                {
                    session.Log("Can't find previous installs, exitting.");
                    return null;
                }

                string largestVersion = null;
                int largestMajor = 0;
                int largestMinor = 0;
                int largestPatch = 0;
                foreach (var key in registryKey.GetSubKeyNames())
                {
                    // Split the key by '.', ensure there are three parts
                    var parts = key.Split('.');

                    if (parts.Length != 3)
                    {
                        session.Log("Skipping key " + key);
                        continue;
                    }

                    bool isLargestVersion = false;
                    int major, minor, patch;
                    if (!int.TryParse(parts[0], out major) || !int.TryParse(parts[1], out minor) || !int.TryParse(parts[2], out patch))
                    {
                        session.Log("Skipping key " + key);
                        continue;
                    }

                    if (major > largestMajor)
                    {
                        isLargestVersion = true;
                    }
                    else if (major == largestMajor && minor > largestMinor)
                    {
                        isLargestVersion = true;
                    }
                    else if (major == largestMajor && minor == largestMinor && patch > largestPatch)
                    {
                        isLargestVersion = true;
                    }

                    if (isLargestVersion)
                    {
                        largestMajor = major;
                        largestMinor = minor;
                        largestPatch = patch;
                        largestVersion = key;
                    }
                }

                if (largestVersion != null)
                {
                    session.Log("Using latest version " + largestVersion);

                    // Return the install dir registry value.
                    using (var previousVersionRegistryKey = hklm64.OpenSubKey(string.Format(@"{0}\{1}", HybridConnectionsRegistryPath, largestVersion)))
                    {
                        if (previousVersionRegistryKey != null)
                        {
                            object obj = previousVersionRegistryKey.GetValue(InstallDirKeyName);
                            if (obj != null)
                            {
                                var installPath = (string)obj;
                                return installPath;
                            }
                        }
                    }

                    return null;
                }
                else
                {
                    session.Log("No versions found.");
                    return null;
                }
            }
        }
    }
}
