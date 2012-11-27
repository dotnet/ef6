// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    internal static class EdmFunctionExtensions
    {
        internal static bool IsCSpace(this EdmFunction function)
        {
            DebugCheck.NotNull(function);

            var property = function.MetadataProperties.FirstOrDefault(p => p.Name == "DataSpace");
            return property != null && (DataSpace)property.Value == DataSpace.CSpace;
        }

        internal static bool IsCanonicalFunction(this EdmFunction function)
        {
            DebugCheck.NotNull(function);

            return (IsCSpace(function) && function.NamespaceName == "Edm");
        }
    }
}
