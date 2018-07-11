// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Relay.Bridge.Configuration
{
    [Serializable]
    public class ConfigException : Exception
    {
        public string FileName { get; }

        public ConfigException()
        {
        }

        public ConfigException(string fileName, string message) : base(message)
        {
            FileName = fileName;
        }

        public ConfigException(string fileName, string message, Exception innerException) : base(message, innerException)
        {
            FileName = fileName;
        }

        protected ConfigException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}