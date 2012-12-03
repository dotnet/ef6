// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal static class StorageEntityTypeMappingExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static object GetConfiguration(this StorageEntityTypeMapping entityTypeMapping)
        {
            DebugCheck.NotNull(entityTypeMapping);

            return entityTypeMapping.Annotations.GetConfiguration();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static void SetConfiguration(this StorageEntityTypeMapping entityTypeMapping, object configuration)
        {
            DebugCheck.NotNull(entityTypeMapping);
            DebugCheck.NotNull(configuration);

            entityTypeMapping.Annotations.SetConfiguration(configuration);
        }

        public static ColumnMappingBuilder GetPropertyMapping(
            this StorageEntityTypeMapping entityTypeMapping, params EdmProperty[] propertyPath)
        {
            DebugCheck.NotNull(entityTypeMapping);
            DebugCheck.NotNull(propertyPath);
            Debug.Assert(propertyPath.Length > 0);

            return entityTypeMapping.MappingFragments
                                    .SelectMany(f => f.ColumnMappings)
                                    .Single(p => p.PropertyPath.SequenceEqual(propertyPath));
        }

        public static EntityType GetPrimaryTable(this StorageEntityTypeMapping entityTypeMapping)
        {
            return entityTypeMapping.MappingFragments.First().Table;
        }

        public static bool UsesOtherTables(this StorageEntityTypeMapping entityTypeMapping, EntityType table)
        {
            return entityTypeMapping.MappingFragments.Any(f => f.Table != table);
        }

        public static Type GetClrType(this StorageEntityTypeMapping entityTypeMappping)
        {
            DebugCheck.NotNull(entityTypeMappping);

            return entityTypeMappping.Annotations.GetClrType();
        }

        public static void SetClrType(this StorageEntityTypeMapping entityTypeMapping, Type type)
        {
            DebugCheck.NotNull(entityTypeMapping);
            DebugCheck.NotNull(type);

            entityTypeMapping.Annotations.SetClrType(type);
        }

        public static StorageEntityTypeMapping Clone(this StorageEntityTypeMapping entityTypeMapping)
        {
            DebugCheck.NotNull(entityTypeMapping);

            var clone = new StorageEntityTypeMapping(null);

            clone.AddType(entityTypeMapping.EntityType);

            entityTypeMapping.Annotations.Copy(clone.Annotations);

            return clone;
        }
    }
}
