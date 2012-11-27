// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal static class DbEdmPropertyMappingExtensions
    {
        public static void SyncNullabilityCSSpace(this ColumnMappingBuilder propertyMappingBuilder)
        {
            DebugCheck.NotNull(propertyMappingBuilder);

            propertyMappingBuilder.ColumnProperty.Nullable = propertyMappingBuilder.PropertyPath.Last().Nullable;
        }
    }
}
