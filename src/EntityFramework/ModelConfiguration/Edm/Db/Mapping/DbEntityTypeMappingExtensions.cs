// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbEntityTypeMappingExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static object GetConfiguration(this StorageEntityTypeMapping entityTypeMapping)
        {
            Contract.Requires(entityTypeMapping != null);

            return entityTypeMapping.Annotations.GetConfiguration();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static void SetConfiguration(this StorageEntityTypeMapping entityTypeMapping, object configuration)
        {
            Contract.Requires(entityTypeMapping != null);
            Contract.Requires(configuration != null);

            entityTypeMapping.Annotations.SetConfiguration(configuration);
        }

        public static ColumnMappingBuilder GetPropertyMapping(
            this StorageEntityTypeMapping entityTypeMapping, params EdmProperty[] propertyPath)
        {
            Contract.Requires(entityTypeMapping != null);
            Contract.Requires(propertyPath != null);
            Contract.Assert(propertyPath.Length > 0);

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
            Contract.Requires(entityTypeMappping != null);

            return entityTypeMappping.Annotations.GetClrType();
        }

        public static void SetClrType(this StorageEntityTypeMapping entityTypeMapping, Type type)
        {
            Contract.Requires(entityTypeMapping != null);
            Contract.Requires(type != null);

            entityTypeMapping.Annotations.SetClrType(type);
        }

        public static StorageEntityTypeMapping Clone(this StorageEntityTypeMapping entityTypeMapping)
        {
            Contract.Requires(entityTypeMapping != null);

            var clone = new StorageEntityTypeMapping(null);

            clone.AddType(entityTypeMapping.EntityType);

            entityTypeMapping.Annotations.Copy(clone.Annotations);

            return clone;
        }
    }
}
