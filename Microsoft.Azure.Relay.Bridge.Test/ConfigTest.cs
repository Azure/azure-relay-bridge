

namespace Microsoft.Azure.Relay.Bridge.Test
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Azure.Relay.Bridge.Configuration;
    using Xunit;

    public class ConfigTest
    {

        string CreateMaxCommandLine()
        {
            return " -b 127.0.0.4" +
                   " -C" +
                   " -E sb://cvrelay.servicebus.windows.net/" +
                   " -F foo.txt" +
                   " -g" +
                   " -K send" +
                   " -k abcdefgh" +
                   " -L 127.0.100.1:8008:foo" +
                   " -L 127.0.100.2:8008:bar" +
                   " -L name:baz" +
                   " -o GatewayPort:true" +
                   " -q" +
                   " -R foo:123" +
                   " -R bar:10.1.1.1:123" +
                   " -R baz:abc" +
                   " -S sigsig" +
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
                       @"AddressFamily : inet4" + textWriter.NewLine +
                        "AzureRelayConnectionString: Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=" + textWriter.NewLine +
                        "BindAddress : 127.0.0.4" + textWriter.NewLine +
                        "ClearAllForwardings : false " + textWriter.NewLine +
                        "Compression : true " + textWriter.NewLine +
                        "CompressionLevel : 9 " + textWriter.NewLine +
                        "ConnectionAttempts : 1 " + textWriter.NewLine +
                        "ConnectTimeout : 60 " + textWriter.NewLine +
                        "ExitOnForwardFailure : true " + textWriter.NewLine +
                        "LocalForward : " + textWriter.NewLine +
                        "  - BindAddress : 127.0.100.1" + textWriter.NewLine +
                        "    BindPort : 8008" + textWriter.NewLine +
                        "    RelayName : foo" + textWriter.NewLine +
                        "  - BindAddress : 127.0.100.2" + textWriter.NewLine +
                        "    BindPort : 8008" + textWriter.NewLine +
                        "    RelayName : bar" + textWriter.NewLine +
                        "  - BindLocalSocket : name" + textWriter.NewLine +
                        "    RelayName : baz" + textWriter.NewLine +
                        "RemoteForward : " + textWriter.NewLine +
                        "  - RelayName : foo" + textWriter.NewLine +
                        "    HostPort : 123" + textWriter.NewLine +
                        "  - RelayName : bar" + textWriter.NewLine +
                        "    HostPort : 8008" + textWriter.NewLine +
                        "    Host : 10.1.1.1" + textWriter.NewLine +
                        "  - RelayName : baz" + textWriter.NewLine +
                        "    HostPort : 8008" + textWriter.NewLine +
                        "    Host : foo.example.com" + textWriter.NewLine);
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
            CommandLineSettings.Run = (settings) => {
                
                Config config = Config.LoadConfig(settings);

                CheckMaxCommandLine(config);

                return Task.FromResult(0);
            };
            CommandLineApplication.Execute<CommandLineSettings>(CreateMaxCommandLine().Split(' '));
        }

        private static void CheckMaxCommandLine(Config config)
        {
            Assert.Equal("Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=;", config.AzureRelayConnectionString);
            Assert.Equal("sb://cvrelay.servicebus.windows.net/", config.AzureRelayEndpoint);
            Assert.Equal("send", config.AzureRelaySharedAccessKeyName);
            Assert.Equal("abcdefgh", config.AzureRelaySharedAccessKey);
            Assert.Equal("sigsig", config.AzureRelaySharedAccessSignature);
            Assert.Equal("127.0.0.4", config.BindAddress);
            Assert.False(config.ClearAllForwardings);
            Assert.True(config.Compression);
            
            Assert.Equal(3, config.LocalForward.Count);
            Assert.Equal("127.0.100.1", config.LocalForward[0].BindAddress);
            Assert.Equal(8008, config.LocalForward[0].BindPort);
            Assert.Equal("foo", config.LocalForward[0].RelayName);
            Assert.Equal("127.0.100.2", config.LocalForward[1].BindAddress);
            Assert.Equal(8008, config.LocalForward[1].BindPort);
            Assert.Equal("bar", config.LocalForward[1].RelayName);
            Assert.Equal("abc", config.LocalForward[2].BindLocalSocket);
            Assert.Equal("baz", config.LocalForward[2].RelayName);

            Assert.Equal(3, config.RemoteForward.Count);
            Assert.Equal("foo", config.RemoteForward[0].RelayName);
            Assert.Equal(123, config.RemoteForward[0].HostPort);
            Assert.Equal("bar", config.RemoteForward[1].RelayName);
            Assert.Equal(8008, config.RemoteForward[1].HostPort);
            Assert.Equal("10.1.1.1", config.RemoteForward[1].Host);
            Assert.Equal("baz", config.RemoteForward[2].RelayName);
            Assert.Equal("abc", config.RemoteForward[2].LocalSocket);
            
        }

        private static void CheckMaxConfig(Config config)
        {
            Assert.Equal("inet4", config.AddressFamily);
            Assert.Equal("Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=;", config.AzureRelayConnectionString);
            Assert.Equal("sb://cvrelay.servicebus.windows.net/", config.AzureRelayEndpoint);
            Assert.Equal("RootManageSharedAccessKey", config.AzureRelaySharedAccessKeyName);
            Assert.Equal("P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=", config.AzureRelaySharedAccessKey);
            Assert.Null(config.AzureRelaySharedAccessSignature);
            Assert.Equal("127.0.0.4", config.BindAddress);
            Assert.False(config.ClearAllForwardings);
            Assert.True(config.Compression);
            Assert.Equal(9, config.CompressionLevel);
            Assert.Equal(1, config.ConnectionAttempts);
            Assert.Equal(60, config.ConnectTimeout);
            Assert.True(config.ExitOnForwardFailure);

            Assert.Equal(3, config.LocalForward.Count);
            Assert.Equal("127.0.100.1", config.LocalForward[0].BindAddress);
            Assert.Equal(8008, config.LocalForward[0].BindPort);
            Assert.Equal("foo", config.LocalForward[0].RelayName);
            Assert.Equal("foo.example.com", config.LocalForward[0].HostName);
            Assert.Equal("127.0.100.2", config.LocalForward[1].BindAddress);
            Assert.Equal(8008, config.LocalForward[1].BindPort);
            Assert.Equal("bar", config.LocalForward[1].RelayName);
            Assert.Equal("foo.example.com", config.LocalForward[1].HostName);
            Assert.Equal("test", config.LocalForward[2].BindLocalSocket);
            Assert.Equal("baz", config.LocalForward[2].RelayName);
            Assert.Equal("Endpoint=sb://cvrelay.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=P0CQgKxRl8S0ABAlmbitHDEWfwWUQzKB34J0w48SB/w=;", config.LocalForward[2].ConnectionString);
            Assert.Equal("foo.example.com", config.LocalForward[2].HostName);

            Assert.Equal(3, config.RemoteForward.Count);
            Assert.Equal("foo", config.RemoteForward[0].RelayName);
            Assert.Equal(8008, config.RemoteForward[0].HostPort);
            Assert.Equal("foo.example.com", config.RemoteForward[0].Host);
            Assert.Equal("bar", config.RemoteForward[1].RelayName);
            Assert.Equal(8008, config.RemoteForward[1].HostPort);
            Assert.Equal("foo.example.com", config.RemoteForward[1].Host);
            Assert.Equal("baz", config.RemoteForward[2].RelayName);
            Assert.Equal(8008, config.RemoteForward[2].HostPort);
            Assert.Equal("foo.example.com", config.RemoteForward[2].Host);
        }

        [Fact]
        public void ConfigSaveLoadFileTest()
        {
            var configFileName = CreateMaxConfig();

            CommandLineSettings settings = new CommandLineSettings();
            settings.ConfigFile = configFileName;
            Config config = Config.LoadConfig(settings);

            config.SaveConfigFile(configFileName, false);

            config = Config.LoadConfigFile(configFileName);

            CheckMaxConfig(config);

            File.Delete(configFileName);
        }
    }
}
