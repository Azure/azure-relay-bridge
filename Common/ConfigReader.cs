// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ForwardingServiceCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public class ConfigReader
    {
        public static Configuration ReadConfigurationFromEnvironmentVariable(string environmentVariableName)
        {
            // BUGBUG: For some reason, Environment.ExpandEnvironmentVariables does NOT expand the environment block...?
            var envVars = Environment.GetEnvironmentVariables();

            var bytes = Convert.FromBase64String((string)envVars[environmentVariableName]);

            string envVar = Encoding.Unicode.GetString(bytes);

            return ReadConfiguration(envVar);
        }

        public static List<ConfigurationBinding> ReadXmlBindings(string xml)
        {
            List<ConfigurationBinding> bindings = null;

            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "Bindings")
                        {
                            bindings = ReadBindings(reader);
                        }
                    }
                }
            }

            return bindings;
        }

        public static List<ConfigurationBinding> ReadBindings(XmlReader reader)
        {
            List<ConfigurationBinding> bindings = new List<ConfigurationBinding>();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Bindings")
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Binding")
                {
                    bindings.Add(ReadBinding(reader));
                }
            }

            return bindings;
        }

        public static ConfigurationBinding ReadBinding(XmlReader reader)
        {
            ConfigurationBinding binding = new ConfigurationBinding();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Binding")
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Port")
                {
                    reader.Read();
                    if (reader.NodeType != XmlNodeType.Text)
                    {
                        throw new FormatException("Expected text in <port> tags");
                    }
                    else
                    {
                        binding.Port = int.Parse(reader.Value);
                    }
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "ConnectionString")
                {
                    reader.Read();
                    if (reader.NodeType != XmlNodeType.Text)
                    {
                        throw new FormatException("Expected text in <ConnectionString> tags");
                    }
                    else
                    {
                        binding.ConnectionString = reader.Value;
                    }
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "IsV2")
                {
                    reader.Read();
                    if (reader.NodeType != XmlNodeType.Text)
                    {
                        throw new FormatException("Expected text in <IsV2> tag");
                    }
                    else
                    {
                        binding.IsV2 = bool.Parse(reader.Value);
                    }
                }
            }

            return binding;
        }

        public static Configuration ReadConfiguration(string data)
        {
            Configuration config = new Configuration();
            config.Bindings = ReadXmlBindings(data);

            return config;
        }
    }
}
