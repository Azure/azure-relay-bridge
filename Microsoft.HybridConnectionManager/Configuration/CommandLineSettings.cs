// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.HybridConnectionManager.Configuration
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    public class CommandLineSettings
    {
        [Option('C', "config-file", Required = false, HelpText = "configuration-file")]
        public string ConfigurationFile { get; set; }

        [Usage()]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("basic usage", new CommandLineSettings { ConfigurationFile = "myconfig.cfg" });
            }
        }
    }
}
