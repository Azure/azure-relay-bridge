

namespace Microsoft.HybridConnectionManager.Test
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.HybridConnectionManager.Configuration;
    using Xunit;

    public class ConfigTest
    {

        string CreateConfig()
        {
            string myFile = Path.GetTempFileName();
            using (var myFileStream = File.OpenWrite(myFile))
            {
                using (var textWriter = new StreamWriter(myFileStream, Encoding.UTF8))
                {
                    textWriter.Write(
                       @"{" +
                        "   \"connections\" : {" +
                        "         \"targets\" : [ " +
                        "           { \"relayConnectionString\":\"Endpoint=sb://test\", \"targetHostName\":\"test1.example.com\", \"targetPort\": 81 }," +
                        "           { \"relayConnectionString\":\"Endpoint=sb://test\", \"targetHostName\":\"test2.example.com\", \"targetPort\": 82 }," +
                        "           { \"relayConnectionString\":\"Endpoint=sb://test\", \"targetHostName\":\"test3.example.com\", \"targetPort\": 83 }" +
                        "         ]," +
                        "         \"listeners\" : [ " +
                        "           { \"relayConnectionString\":\"Endpoint=sb://test\", \"listenHostName\":\"test1.example.com\", \"listenPort\": 81 }," +
                        "           { \"relayConnectionString\":\"Endpoint=sb://test\", \"listenHostName\":\"test2.example.com\", \"listenPort\": 82 }," +
                        "           { \"relayConnectionString\":\"Endpoint=sb://test\", \"listenHostName\":\"test3.example.com\", \"listenPort\": 83 }" +
                        "         ]," +
                        "   }" +
                        "}");
                }
            }
            return myFile;
        }

        [Fact]
        public void Test1()
        {
            var configFileName = CreateConfig();

            var connectionConfig = new ConnectionConfig();
            var builder = new ConfigurationBuilder()
               .SetBasePath(Path.GetDirectoryName(configFileName))
               .AddJsonFile(Path.GetFileName(configFileName));

            var config = builder.Build();
            config.GetSection("Connections").Bind(connectionConfig);

            Assert.Equal(3, connectionConfig.Listeners.Length);
            Assert.Equal(3, connectionConfig.Targets.Length);

            Assert.Equal("Endpoint=sb://test", connectionConfig.Listeners[0].RelayConnectionString);
            Assert.Equal("test1.example.com", connectionConfig.Listeners[0].ListenHostName);
            Assert.Equal(81, connectionConfig.Listeners[0].ListenPort);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Listeners[1].RelayConnectionString);
            Assert.Equal("test2.example.com", connectionConfig.Listeners[1].ListenHostName);
            Assert.Equal(82, connectionConfig.Listeners[1].ListenPort);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Listeners[2].RelayConnectionString);
            Assert.Equal("test3.example.com", connectionConfig.Listeners[1].ListenHostName);
            Assert.Equal(83, connectionConfig.Listeners[2].ListenPort);

            Assert.Equal("Endpoint=sb://test", connectionConfig.Targets[0].RelayConnectionString);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Targets[1].RelayConnectionString);
            Assert.Equal("Endpoint=sb://test", connectionConfig.Targets[2].RelayConnectionString);

            File.Delete(configFileName);
        }
    }
}
