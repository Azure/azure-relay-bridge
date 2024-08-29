// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Microsoft.Azure.Relay.Bridge
{
    public static class Constants
    {
        public static Regex RelayNameRegex = new Regex(@"^[0-9A-Za-z/_\-\.]+$", RegexOptions.Compiled);
    }
}