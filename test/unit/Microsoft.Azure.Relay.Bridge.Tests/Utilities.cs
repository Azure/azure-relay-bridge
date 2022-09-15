// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Azure.Relay.Bridge.Test
{
    internal static class Utilities
    {
        public static TimeSpan DefaultTimeout => TimeSpan.FromMinutes(1);


        internal static string GetConnectionString()
        {
            string cxn = Environment.GetEnvironmentVariable("AZBRIDGE_TEST_CXNSTRING")?.Trim('\"');
            if (string.IsNullOrEmpty(cxn))
            {
                throw new InvalidOperationException("The AZBRIDGE_TEST_CXNSTRING environment string must be set to a preconfigured Relay namespace for this test to execute");
            }
            return cxn;
        }
    }
}
