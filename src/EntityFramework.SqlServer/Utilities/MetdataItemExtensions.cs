// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    internal static class MetdataItemExtensions
    {
        public static T GetMetadataPropertyValue<T>(this MetadataItem item, string propertyName)
        {
            DebugCheck.NotNull(item);

            var property = item.MetadataProperties.FirstOrDefault(p => p.Name == propertyName);
            return property == null ? default(T) : (T)property.Value;
        }
    }
}
