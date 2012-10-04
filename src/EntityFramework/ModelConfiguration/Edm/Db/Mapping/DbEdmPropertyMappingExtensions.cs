// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbEdmPropertyMappingExtensions
    {
        public static void SyncNullabilityCSSpace(this DbEdmPropertyMapping propertyMapping)
        {
            Contract.Requires(propertyMapping != null);

            propertyMapping.Column.IsNullable = propertyMapping.PropertyPath.Last().Nullable;
        }
    }
}
