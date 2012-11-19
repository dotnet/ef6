// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    internal static class DbAssociationSetMappingExtensions
    {
        public static StorageAssociationSetMapping Initialize(this StorageAssociationSetMapping associationSetMapping)
        {
            DebugCheck.NotNull(associationSetMapping);

            associationSetMapping.SourceEndMapping = new StorageEndPropertyMapping(null);
            associationSetMapping.TargetEndMapping = new StorageEndPropertyMapping(null);

            return associationSetMapping;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static object GetConfiguration(this StorageAssociationSetMapping associationSetMapping)
        {
            DebugCheck.NotNull(associationSetMapping);

            return associationSetMapping.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this StorageAssociationSetMapping associationSetMapping, object configuration)
        {
            DebugCheck.NotNull(associationSetMapping);

            associationSetMapping.Annotations.SetConfiguration(configuration);
        }
    }
}
