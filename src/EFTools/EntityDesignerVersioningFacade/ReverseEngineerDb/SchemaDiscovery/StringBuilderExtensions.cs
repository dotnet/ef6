// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System.Diagnostics;
    using System.Text;

    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendIfNotEmpty(this StringBuilder input, string value)
        {
            Debug.Assert(input != null, "input != null");

            return
                input.Length > 0
                    ? input.Append(value)
                    : input;
        }
    }
}
