namespace Microsoft.Azure.Relay.Bridge.Test
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Xunit;

    public class ConfigTest : IClassFixture<LaunchSettingsFixture>
    {
        private readonly LaunchSettingsFixture launchSettingsFixture;

        public ConfigTest(LaunchSettingsFixture launchSettingsFixture)
        {
            this.launchSettingsFixture = launchSettingsFixture;
            // before we localize, make sure we have all the error
            // messages in en-us. Mirroring Program.cs
            CultureInfo.CurrentUICulture =
                    CultureInfo.DefaultThreadCurrentUICulture =
                    CultureInfo.GetCultureInfoByIetfLanguageTag("en-us");
        }

        string CreateMaxCommandLine()
        {
            return "-b 127.0.0.4" +
                   " -e sb://cvrelay.servicebus.windows.net/" +
                   " -f foo.txt" +
                   " -g" +
                   " -K send" +
                   " -k abcdefgh" +
                   " -L 127.0.100.1:8008:foo" +
                   " -L 127.0.100.2:8008:bar" +
#if !NETFRAMEWORK                                           
                   " -L name:baz" +
#endif                   
                   " -o ConnectTimeout:44" +
                   " -q" +
                   " -R foo:123" +
                   " -R bar:10.1.1.1:123" +
#if !NETFRAMEWORK                                           
                   " -R baz:abc" +
#endif                   
                   " -v";
        }

        string CreateMaxConfig()
        {
            string myFile = Path.GetTempFileName();
            using (var myFileStream = File.OpenWrite(myFile))
            {
                using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                {
                    textWriter.Write(
                       @"AddressFamily : inet" + textWriter.NewLine +
                        "AzureRelayConnectionString: Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=" + textWriter.NewLine +
                        "BindAddress : 127.0.0.4" + textWriter.NewLine +
                        "ClearAllForwardings : false " + textWriter.NewLine +
                        "ConnectionAttempts : 1 " + textWriter.NewLine +
                        "ConnectTimeout : 60 " + textWriter.NewLine +
                        "ExitOnForwardFailure : true " + textWriter.NewLine +
                        "LocalForward : " + textWriter.NewLine +
                        "  - BindAddress : 127.0.100.1" + textWriter.NewLine +
                        "    BindPort : 8008" + textWriter.NewLine +
                        "    RelayName : foo" + textWriter.NewLine +
                        "    HostName : foo.example.com" + textWriter.NewLine +
                        "  - BindAddress : 127.0.100.2" + textWriter.NewLine +
                        "    BindPort : 8008" + textWriter.NewLine +
                        "    RelayName : bar" + textWriter.NewLine +
                        "    HostName : bar.example.com" + textWriter.NewLine +
#if !NETFRAMEWORK                        
                        "  - BindLocalSocket : test" + textWriter.NewLine +
                        "    RelayName : baz" + textWriter.NewLine +
#endif                        
                        "RemoteForward : " + textWriter.NewLine +
                        "  - RelayName : foo" + textWriter.NewLine +
                        "    HostPort : 123" + textWriter.NewLine +
                        "  - RelayName : bar" + textWriter.NewLine +
                        "    HostPort : 8008" + textWriter.NewLine +
                        "    Host : 10.1.1.1" + textWriter.NewLine 
#if !NETFRAMEWORK                                                
                        + "  - RelayName : baz" + textWriter.NewLine +
                        "    LocalSocket : foo" + textWriter.NewLine 
#endif                        
                        );
                }
            }
            return myFile;
        }

        [Fact]
        public void ConfigFileMaxTest()
        {
            var configFileName = CreateMaxConfig();

            CommandLineSettings settings = new CommandLineSettings();
            settings.ConfigFile = configFileName;
            Config config = Config.LoadConfig(settings);

            CheckMaxConfig(config);

            File.Delete(configFileName);
        }



        [Fact]
        public void CommandLineMaxTest()
        {
            bool callbackInvoked = false;
            CommandLineSettings.Run(CreateMaxCommandLine().Split(' '),
                (settings) =>
                {
                    Config config = Config.LoadConfig(settings);
                    CheckMaxCommandLine(config);
                    callbackInvoked = true;
                    return 0;
                });

            Assert.True(callbackInvoked);
        }

        [Fact]
        public void CommandLineBadArgumentTest()
        {
            var ex = Assert.Throws<UnrecognizedCommandParsingException>(() =>
            {
                CommandLineSettings.Run(new string[] { "-bad" },
                    (settings) => { return 0; });
            });
            Assert.Contains("-bad", ex.Message);
        }

        [Fact]
        public void CommandLineBadConnectionStringTest()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                CommandLineSettings.Run(new string[] { "-x", "Endpoint=total^^^garbage" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-x", ex.Message);
        }

        [Fact]
        public void CommandLineBadBindAddressTest()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                CommandLineSettings.Run(new string[] { "-b", "abc^^$foo" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-b", ex.Message);

            var ex1 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // public IP address that isn't "here" and thus can't be bound
                CommandLineSettings.Run(new string[] { "-b", "1.1.1.1" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-b", ex1.Message);

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // public IP address that isn't "here" and thus can't be bound
                CommandLineSettings.Run(new string[] { "-b", "bing.com" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-b", ex2.Message);
        }

        [Fact]
        public void CommandLineBadLocalForwardTest()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                CommandLineSettings.Run(new string[] { "-L", "70000:foobar" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-L", ex.Message);
            Assert.Contains("BindPort", ex.Message);

            var ex1 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // public IP address that isn't "here" and thus can't be bound
                CommandLineSettings.Run(new string[] { "-L", "80:foo^^^bar" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-L", ex1.Message);
            Assert.Contains("RelayName", ex1.Message);

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // public IP address that isn't "here" and thus can't be bound
                CommandLineSettings.Run(new string[] { "-L", "1.1.1.1:80:foobar" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-L", ex2.Message);
            Assert.Contains("BindAddress", ex2.Message);
        }

        [Fact]
        public void CommandLineBadRemoteForwardTest()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                CommandLineSettings.Run(new string[] { "-R", "foobar:70000" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-R", ex.Message);
            Assert.Contains("HostPort", ex.Message);

            var ex1 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // public IP address that isn't "here" and thus can't be bound
                CommandLineSettings.Run(new string[] { "-R", "foo^^^bar:80" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-R", ex1.Message);
            Assert.Contains("RelayName", ex1.Message);

            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                // public IP address that isn't "here" and thus can't be bound
                CommandLineSettings.Run(new string[] { "-R", "foobar:foo^^^bar:80" },
                    (settings) =>
                    {
                        Config.LoadConfig(settings);
                        return 0;
                    });
            });
            Assert.Contains("-R", ex2.Message);
            Assert.Contains("Host", ex2.Message);
        }

        [Fact]
        public void CommandLineGoodBindAddressTest()
        {
            CommandLineSettings.Run(new string[] { "-b", "localhost" },
                (settings) =>
                {
                    Config.LoadConfig(settings);
                    return 0;
                });

            CommandLineSettings.Run(new string[] { "-b", "\"127.0.1.0\"" },
                (settings) =>
                {
                    Config.LoadConfig(settings);
                    return 0;
                });

            CommandLineSettings.Run(new string[] { "-b", "\"[::1]\"" },
                (settings) =>
                {
                    Config.LoadConfig(settings);
                    return 0;
                });
        }

        [Fact]
        public void CommandLineBadEndpointUriTest()
        {
            {
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    CommandLineSettings.Run(new string[] { "-e", "\"^^^^\"" },
                        (settings) =>
                        {
                            Config.LoadConfig(settings);
                            return 0;
                        });
                });
                Assert.Contains("-e", ex.Message);
            }
        }

        [Fact]
        public void CommandLineBadKeyNameTest()
        {
            {
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    CommandLineSettings.Run(new string[] { "-K", "\"^^^^\"" },
                        (settings) =>
                        {
                            Config.LoadConfig(settings);
                            return 0;
                        });
                });
                Assert.Contains("-K", ex.Message);
            }
        }

        [Fact]
        public void CommandLineGoodKeyNameTest()
        {
            CommandLineSettings.Run(new string[] { "-K", "\"sendlisten\"" },
                (settings) =>
                {
                    Config.LoadConfig(settings);
                    return 0;
                });
        }

        [Fact]
        public void CommandLineBadKeyValueTest()
        {
            {
                var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    CommandLineSettings.Run(new string[] { "-k", "\"^^^^\"" },
                        (settings) =>
                        {
                            Config.LoadConfig(settings);
                            return 0;
                        });
                });
                Assert.Contains("-k", ex.Message);
            }
        }

        [Fact]
        public void CommandLineGoodKeyValueTest()
        {
            CommandLineSettings.Run(new string[] { "-k", "\"e29y7Y09bQpdcc/0KfO4WYUOJIMvs6I8cNM8EZpAdHQ=\"" },
                (settings) =>
                {
                    Config.LoadConfig(settings);
                    return 0;
                });
        }


        [Fact]
        public void ConfigBadOptionTest()
        {
            string myFile = Path.GetTempFileName();
            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write("Fompa : ssjsjshshjs" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains("Fompa", ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }
        }

        [Fact]
        public void ConfigBadBindAddressTest()
        {
            string myFile = Path.GetTempFileName();
            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write("BindAddress : ssjs^^^hjs" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains("BindAddress", ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }
        }

        [Fact]
        public void ConfigBadConnectionAttemptsTest()
        {
            string myFile = Path.GetTempFileName();
            const string optionName = "ConnectionAttempts";
            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write($"{optionName} : ssjs^^^hjs" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains(optionName, ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }

            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write($"{optionName} : -1" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains(optionName, ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }

            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write($"{optionName} : 11" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains(optionName, ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }
        }


        [Fact]
        public void ConfigBadConnectTimeoutTest()
        {
            string myFile = Path.GetTempFileName();
            const string optionName = "ConnectTimeout";
            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write($"{optionName} : ssjs^^^hjs" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains(optionName, ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }

            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write($"{optionName} : -1" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains(optionName, ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }

            try
            {
                using (var myFileStream = File.OpenWrite(myFile))
                {
                    using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                    {
                        textWriter.Write($"{optionName} : 121" + textWriter.NewLine);
                    }
                }

                var ex = Assert.Throws<ConfigException>(() =>
                {
                    Config.LoadConfigFile(myFile);
                });
                Assert.Contains(optionName, ex.Message);
            }
            finally
            {
                File.Delete(myFile);
            }
        }
        private static void CheckMaxCommandLine(Config config)
        {
            Assert.Equal("Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey=abcdefgh;", config.AzureRelayConnectionString);
            Assert.Equal("sb://cvrelay.servicebus.windows.net/", config.AzureRelayEndpoint);
            Assert.Equal("send", config.AzureRelaySharedAccessKeyName);
            Assert.Equal("abcdefgh", config.AzureRelaySharedAccessKey);
            Assert.Equal("127.0.0.4", config.BindAddress);
            Assert.Equal(44, config.ConnectTimeout);
            Assert.True(config.GatewayPorts);
#if !NETFRAMEWORK
            Assert.Equal(3, config.LocalForward.Count);
#else
            Assert.Equal(2, config.LocalForward.Count);
#endif
            Assert.Equal("127.0.100.1", config.LocalForward[0].BindAddress);
            Assert.Equal(8008, config.LocalForward[0].BindPort);
            Assert.Equal("foo", config.LocalForward[0].RelayName);
            Assert.Equal("127.0.100.2", config.LocalForward[1].BindAddress);
            Assert.Equal(8008, config.LocalForward[1].BindPort);
            Assert.Equal("bar", config.LocalForward[1].RelayName);
#if !NETFRAMEWORK
            Assert.Equal("name", config.LocalForward[2].BindLocalSocket);
            Assert.Equal("baz", config.LocalForward[2].RelayName);
#endif

#if !NETFRAMEWORK
            Assert.Equal(3, config.RemoteForward.Count);
#else
            Assert.Equal(2, config.RemoteForward.Count);
#endif
            Assert.Equal("foo", config.RemoteForward[0].RelayName);
            Assert.Equal(123, config.RemoteForward[0].HostPort);
            Assert.Equal("bar", config.RemoteForward[1].RelayName);
            Assert.Equal(123, config.RemoteForward[1].HostPort);
            Assert.Equal("10.1.1.1", config.RemoteForward[1].Host);
#if !NETFRAMEWORK
            Assert.Equal("baz", config.RemoteForward[2].RelayName);
            Assert.Equal("abc", config.RemoteForward[2].LocalSocket);
#endif

        }

        private static void CheckMaxConfig(Config config)
        {
            Assert.Equal("inet", config.AddressFamily);
            Assert.Equal("Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=;", config.AzureRelayConnectionString);
            Assert.Equal("sb://cvrelay.servicebus.windows.net/", config.AzureRelayEndpoint);
            Assert.Equal("RootManageSharedAccessKey", config.AzureRelaySharedAccessKeyName);
            Assert.Equal("P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=", config.AzureRelaySharedAccessKey);
            Assert.Null(config.AzureRelaySharedAccessSignature);
            Assert.Equal("127.0.0.4", config.BindAddress);
            Assert.False(config.ClearAllForwardings);
            Assert.Equal(1, config.ConnectionAttempts);
            Assert.Equal(60, config.ConnectTimeout);
            Assert.True(config.ExitOnForwardFailure);

#if !NETFRAMEWORK
            Assert.Equal(3, config.LocalForward.Count);
#else
            Assert.Equal(2, config.LocalForward.Count);
#endif
            Assert.Equal("127.0.100.1", config.LocalForward[0].BindAddress);
            Assert.Equal(8008, config.LocalForward[0].BindPort);
            Assert.Equal("foo", config.LocalForward[0].RelayName);
            Assert.Equal("foo.example.com", config.LocalForward[0].HostName);
            Assert.Equal("127.0.100.2", config.LocalForward[1].BindAddress);
            Assert.Equal(8008, config.LocalForward[1].BindPort);
            Assert.Equal("bar", config.LocalForward[1].RelayName);
            Assert.Equal("bar.example.com", config.LocalForward[1].HostName);
#if !NETFRAMEWORK
            Assert.Equal("test", config.LocalForward[2].BindLocalSocket);
            Assert.Equal("baz", config.LocalForward[2].RelayName);
#endif

#if !NETFRAMEWORK
            Assert.Equal(3, config.RemoteForward.Count);
#else
            Assert.Equal(2, config.RemoteForward.Count);
#endif
            Assert.Equal("foo", config.RemoteForward[0].RelayName);
            Assert.Equal(123, config.RemoteForward[0].HostPort);
            Assert.Equal("bar", config.RemoteForward[1].RelayName);
            Assert.Equal(8008, config.RemoteForward[1].HostPort);
            Assert.Equal("10.1.1.1", config.RemoteForward[1].Host);
#if !NETFRAMEWORK
            Assert.Equal("baz", config.RemoteForward[2].RelayName);
            Assert.Equal("foo", config.RemoteForward[2].LocalSocket);
#endif
        }

        [Fact]
        public void ConfigSaveLoadFileTest()
        {
            var configFileName = CreateMaxConfig();
            try
            {
                CommandLineSettings settings = new CommandLineSettings();
                settings.ConfigFile = configFileName;
                Config config = Config.LoadConfig(settings);

                config.SaveConfigFile(configFileName, false);

                config = Config.LoadConfigFile(configFileName);

                CheckMaxConfig(config);
            }
            finally
            {
                File.Delete(configFileName);
            }
        }
    }
}
