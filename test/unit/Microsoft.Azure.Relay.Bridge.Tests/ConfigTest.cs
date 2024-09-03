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
    using Microsoft.Azure.Relay.Bridge.Tests;
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
                   " -L name:baz" +
                   " -o ConnectTimeout:44" +
                   " -q" +
                   " -R foo1:123" +
                   " -R bar1:10.1.1.1:123" +
                   " -R foo3:port/123" +
                   " -R bar3:10.1.1.1:port/123" +
                   " -R baz:abc" +
                   " -T foo2:123" +
                   " -T bar2:10.1.1.1:123" +
                   " -T foo4:port/123" +
                   " -T bar4:port/10.1.1.1:123" +
                   " -H foo5:https/service.example.com" +
                   " -H bar5:http/service.example.com:81" +
                   " -H baz5:http/service.example.com" +
                   " -H abc/def:http/service.example.com/foo" +
                   " -H ghi/jkl:http/service1.example.com/foo;http/service2.example.com/foo" +
                   " -v" +
                   " -a 60";
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
                        "KeepAliveInterval : 60 " + textWriter.NewLine +
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
                        "  - BindAddress : 127.0.100.3" + textWriter.NewLine +
                        "    BindPort : 8008" + textWriter.NewLine +
                        "    RelayName : bam" + textWriter.NewLine +
                        "    HostName : bam.example.com" + textWriter.NewLine +
                        "    NoAuthentication: true" + textWriter.NewLine +
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
        public void CommandLineTOptionTest()
        {
            bool callbackInvoked = false;
            CommandLineSettings.Run(new string[] { "-T", "relay1:port/1000", "-T", "relay2:port/name:1000", "-T", "relay3:1000", "-T", "relay4:sock" },
                (settings) =>
                {
                    Config config = Config.LoadConfig(settings);
                    Assert.Equal(4, config.RemoteForward.Count);
                    Assert.Equal("relay1", config.RemoteForward[0].RelayName);
                    Assert.Equal(1000, config.RemoteForward[0].HostPort);
                    Assert.Equal("port", config.RemoteForward[0].PortName);
                    Assert.Equal("relay2", config.RemoteForward[1].RelayName);
                    Assert.Equal(1000, config.RemoteForward[1].HostPort);
                    Assert.Equal("name", config.RemoteForward[1].Host);
                    Assert.Equal("relay3", config.RemoteForward[2].RelayName);
                    Assert.Equal(1000, config.RemoteForward[2].HostPort);
                    Assert.Equal("relay4", config.RemoteForward[3].RelayName);
                    Assert.Equal("sock", config.RemoteForward[3].LocalSocket);
                    callbackInvoked = true;
                    return 0;
                });

            Assert.True(callbackInvoked);
        }

        [Fact]
        public void CommandLineROptionTest()
        {
            bool callbackInvoked = false;
            CommandLineSettings.Run(new string[] { "-R", "relay1:port/1000", "-R", "relay2:name:port/1000", "-R", "relay3:1000", "-R", "relay4:sock" },
                (settings) =>
                {
                    Config config = Config.LoadConfig(settings);
                    Assert.Equal(4, config.RemoteForward.Count);
                    Assert.Equal("relay1", config.RemoteForward[0].RelayName);
                    Assert.Equal(1000, config.RemoteForward[0].HostPort);
                    Assert.Equal("port", config.RemoteForward[0].PortName);
                    Assert.Equal("relay2", config.RemoteForward[1].RelayName);
                    Assert.Equal(1000, config.RemoteForward[1].HostPort);
                    Assert.Equal("name", config.RemoteForward[1].Host);
                    Assert.Equal("relay3", config.RemoteForward[2].RelayName);
                    Assert.Equal(1000, config.RemoteForward[2].HostPort);
                    Assert.Equal("relay4", config.RemoteForward[3].RelayName);
                    Assert.Equal("sock", config.RemoteForward[3].LocalSocket);
                    callbackInvoked = true;
                    return 0;
                });

            Assert.True(callbackInvoked);
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
        public void CommandLineGoodRemoteForwardTest()
        {
            CommandLineSettings.Run(new string[] { "-R", "foobar:3000" },
            (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.RemoteForward);
                Assert.Equal("foobar", cfg.RemoteForward[0].RelayName);
                Assert.Equal(3000, cfg.RemoteForward[0].HostPort);
                return 0;
            });
            CommandLineSettings.Run(new string[] { "-R", "foobar:3000U" },
            (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.RemoteForward);
                Assert.Equal("foobar", cfg.RemoteForward[0].RelayName);
                Assert.Equal(-3000, cfg.RemoteForward[0].HostPort);
                return 0;
            });
            CommandLineSettings.Run(new string[] { "-R", "foobar_foobar:3000" },
            (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.RemoteForward);
                Assert.Equal("foobar_foobar", cfg.RemoteForward[0].RelayName);
                Assert.Equal(3000, cfg.RemoteForward[0].HostPort);
                return 0;
            });
            CommandLineSettings.Run(new string[] { "-R", "foobar/foobar:3000" },
            (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.RemoteForward);
                Assert.Equal("foobar/foobar", cfg.RemoteForward[0].RelayName);
                Assert.Equal(3000, cfg.RemoteForward[0].HostPort);
                return 0;
            });
            CommandLineSettings.Run(new string[] { "-R", "foobar.foobar:3000" },
            (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.RemoteForward);
                Assert.Equal("foobar.foobar", cfg.RemoteForward[0].RelayName);
                Assert.Equal(3000, cfg.RemoteForward[0].HostPort);
                return 0;
            });
            CommandLineSettings.Run(new string[] { "-R", "foobar-foobar:3000" },
            (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.RemoteForward);
                Assert.Equal("foobar-foobar", cfg.RemoteForward[0].RelayName);
                Assert.Equal(3000, cfg.RemoteForward[0].HostPort);
                return 0;
            });
        }

        [Fact]
        public void CommandLineGoodLocalForwardTest()
        {
            CommandLineSettings.Run(new string[] { "-L", "3000U:foobar" },
                (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.LocalForward);
                Assert.Equal(-3000, cfg.LocalForward[0].BindPort);
                Assert.Equal("foobar", cfg.LocalForward[0].RelayName);
                return 0;
            });
            CommandLineSettings.Run(new string[] { "-L", "3000:foobar" },
                (settings) =>
            {
                var cfg = Config.LoadConfig(settings);
                Assert.Single(cfg.LocalForward);
                Assert.Equal(3000, cfg.LocalForward[0].BindPort);
                Assert.Equal("foobar", cfg.LocalForward[0].RelayName);
                return 0;
            });
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
            Assert.Equal(60, config.KeepAliveInterval);
            Assert.Equal("Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=send;SharedAccessKey=abcdefgh;", config.AzureRelayConnectionString);
            Assert.Equal("sb://cvrelay.servicebus.windows.net/", config.AzureRelayEndpoint);
            Assert.Equal("send", config.AzureRelaySharedAccessKeyName);
            Assert.Equal("abcdefgh", config.AzureRelaySharedAccessKey);
            Assert.Equal("127.0.0.4", config.BindAddress);
            Assert.Equal(44, config.ConnectTimeout);
            Assert.True(config.GatewayPorts);
            Assert.Equal(3, config.LocalForward.Count);
            Assert.Equal("127.0.100.1", config.LocalForward[0].BindAddress);
            Assert.Equal(8008, config.LocalForward[0].BindPort);
            Assert.Equal("foo", config.LocalForward[0].RelayName);
            Assert.Equal("127.0.100.2", config.LocalForward[1].BindAddress);
            Assert.Equal(8008, config.LocalForward[1].BindPort);
            Assert.Equal("bar", config.LocalForward[1].RelayName);
            Assert.Equal("name", config.LocalForward[2].BindLocalSocket);
            Assert.Equal("baz", config.LocalForward[2].RelayName);

            Assert.Equal(14, config.RemoteForward.Count);
            Assert.Equal("foo1", config.RemoteForward[0].RelayName);
            Assert.Equal(123, config.RemoteForward[0].HostPort);
            Assert.Equal("bar1", config.RemoteForward[1].RelayName);
            Assert.Equal(123, config.RemoteForward[1].HostPort);
            Assert.Equal("10.1.1.1", config.RemoteForward[1].Host);
            Assert.Equal("foo3", config.RemoteForward[2].RelayName);
            Assert.Equal(123, config.RemoteForward[2].HostPort);
            Assert.Equal("port", config.RemoteForward[2].PortName);
            Assert.Equal("bar3", config.RemoteForward[3].RelayName);
            Assert.Equal(123, config.RemoteForward[3].HostPort);
            Assert.Equal("10.1.1.1", config.RemoteForward[3].Host);
            Assert.Equal("port", config.RemoteForward[3].PortName);
            Assert.Equal("baz", config.RemoteForward[4].RelayName);
            Assert.Equal("abc", config.RemoteForward[4].LocalSocket);
            Assert.Equal("foo2", config.RemoteForward[5].RelayName);
            Assert.Equal(123, config.RemoteForward[5].HostPort);
            Assert.Equal("bar2", config.RemoteForward[6].RelayName);
            Assert.Equal(123, config.RemoteForward[6].HostPort);
            Assert.Equal("10.1.1.1", config.RemoteForward[6].Host);
            Assert.Equal("foo4", config.RemoteForward[7].RelayName);
            Assert.Equal(123, config.RemoteForward[7].HostPort);
            Assert.Equal("port", config.RemoteForward[7].PortName);
            Assert.Equal("bar4", config.RemoteForward[8].RelayName);
            Assert.Equal(123, config.RemoteForward[8].HostPort);
            Assert.Equal("10.1.1.1", config.RemoteForward[8].Host);
            Assert.Equal("port", config.RemoteForward[8].PortName);
            Assert.Equal("foo5", config.RemoteForward[9].RelayName);
            Assert.Equal(443, config.RemoteForward[9].HostPort);
            Assert.Equal("https", config.RemoteForward[9].PortName);
            Assert.Equal("service.example.com", config.RemoteForward[9].Host);
            Assert.Equal("bar5", config.RemoteForward[10].RelayName);
            Assert.Equal(81, config.RemoteForward[10].HostPort);
            Assert.Equal("http", config.RemoteForward[10].PortName);
            Assert.Equal("service.example.com", config.RemoteForward[10].Host);
            Assert.Equal("baz5", config.RemoteForward[11].RelayName);
            Assert.Equal(80, config.RemoteForward[11].HostPort);
            Assert.Equal("http", config.RemoteForward[11].PortName);
            Assert.Equal("service.example.com", config.RemoteForward[11].Host);
            Assert.Equal("abc/def", config.RemoteForward[12].RelayName);
            Assert.Equal(80, config.RemoteForward[12].Bindings[0].HostPort);
            Assert.Equal("http", config.RemoteForward[12].Bindings[0].PortName);
            Assert.Equal("service.example.com", config.RemoteForward[12].Bindings[0].Host);
            Assert.Equal("/foo/", config.RemoteForward[12].Bindings[0].Path);
            Assert.Equal("ghi/jkl", config.RemoteForward[13].RelayName);
            Assert.Equal(80, config.RemoteForward[13].Bindings[0].HostPort);
            Assert.Equal("http", config.RemoteForward[13].Bindings[0].PortName);
            Assert.Equal("service1.example.com", config.RemoteForward[13].Bindings[0].Host);
            Assert.Equal("/foo/", config.RemoteForward[13].Bindings[1].Path);
            Assert.Equal(80, config.RemoteForward[13].Bindings[1].HostPort);
            Assert.Equal("http", config.RemoteForward[13].Bindings[1].PortName);
            Assert.Equal("service2.example.com", config.RemoteForward[13].Bindings[1].Host);
            Assert.Equal("/foo/", config.RemoteForward[13].Bindings[1].Path);

            Assert.True(config.RemoteForward[10].Http);


        }

        private static void CheckMaxConfig(Config config)
        {
            Assert.Equal(60, config.KeepAliveInterval);
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
            Assert.Equal(4, config.LocalForward.Count);
#else
            Assert.Equal(3, config.LocalForward.Count);
#endif
            Assert.Equal("127.0.100.1", config.LocalForward[0].BindAddress);
            Assert.Equal(8008, config.LocalForward[0].BindPort);
            Assert.Equal("foo", config.LocalForward[0].RelayName);
            Assert.Equal("foo.example.com", config.LocalForward[0].HostName);
            Assert.False(config.LocalForward[0].NoAuthentication);
            Assert.Equal("127.0.100.2", config.LocalForward[1].BindAddress);
            Assert.Equal(8008, config.LocalForward[1].BindPort);
            Assert.Equal("bar", config.LocalForward[1].RelayName);
            Assert.Equal("bar.example.com", config.LocalForward[1].HostName);
            Assert.False(config.LocalForward[1].NoAuthentication);
            Assert.Equal("127.0.100.3", config.LocalForward[2].BindAddress);
            Assert.Equal(8008, config.LocalForward[2].BindPort);
            Assert.Equal("bam", config.LocalForward[2].RelayName);
            Assert.True(config.LocalForward[2].NoAuthentication);
#if !NETFRAMEWORK
            Assert.Equal("test", config.LocalForward[3].BindLocalSocket);
            Assert.Equal("baz", config.LocalForward[3].RelayName);
#endif

#if !NETFRAMEWORK
            Assert.Equal(3, config.RemoteForward.Count);
#else
            Assert.Equal(4, config.RemoteForward.Count);
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
