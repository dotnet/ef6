namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbEntityTypeMappingExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static object GetConfiguration(this DbEntityTypeMapping entityTypeMapping)
        {
            Contract.Requires(entityTypeMapping != null);

            return entityTypeMapping.Annotations.GetConfiguration();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static void SetConfiguration(this DbEntityTypeMapping entityTypeMapping, object configuration)
        {
            Contract.Requires(entityTypeMapping != null);
            Contract.Requires(configuration != null);

            entityTypeMapping.Annotations.SetConfiguration(configuration);
        }

        public static DbEdmPropertyMapping GetPropertyMapping(
            this DbEntityTypeMapping entityTypeMapping, params EdmProperty[] propertyPath)
        {
            Contract.Requires(entityTypeMapping != null);
            Contract.Requires(propertyPath != null);
            Contract.Assert(propertyPath.Length > 0);

            return entityTypeMapping.TypeMappingFragments
                .SelectMany(f => f.PropertyMappings)
                .Single(p => p.PropertyPath.SequenceEqual(propertyPath));
        }

        public static DbTableMetadata GetPrimaryTable(this DbEntityTypeMapping entityTypeMapping)
        {
            return entityTypeMapping.TypeMappingFragments.First().Table;
        }

        public static bool UsesOtherTables(this DbEntityTypeMapping entityTypeMapping, DbTableMetadata table)
        {
            return entityTypeMapping.TypeMappingFragments.Any(f => f.Table != table);
        }

        public static Type GetClrType(this DbEntityTypeMapping entityTypeMappping)
        {
            Contract.Requires(entityTypeMappping != null);

            return entityTypeMappping.Annotations.GetClrType();
        }

        public static void SetClrType(this DbEntityTypeMapping entityTypeMapping, Type type)
        {
            Contract.Requires(entityTypeMapping != null);
            Contract.Requires(type != null);

            entityTypeMapping.Annotations.SetClrType(type);
        }

        public static DbEntityTypeMapping Clone(this DbEntityTypeMapping entityTypeMappping)
        {
            Contract.Requires(entityTypeMappping != null);

            var clone = new DbEntityTypeMapping
                {
                    EntityType = entityTypeMappping.EntityType
                };
            entityTypeMappping.Annotations.Copy(clone.Annotations);

            return clone;
        }
    }
}
