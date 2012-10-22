// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DbEntityTypeMappingFragmentExtensions
    {
        private const string DefaultDiscriminatorAnnotation = "DefaultDiscriminator";
        private const string ConditionOnlyFragmentAnnotation = "ConditionOnlyFragment";
        private const string UnmappedPropertiesFragmentAnnotation = "UnmappedPropertiesFragment";

        public static EdmProperty GetDefaultDiscriminator(
            this DbEntityTypeMappingFragment entityTypeMapppingFragment)
        {
            Contract.Requires(entityTypeMapppingFragment != null);

            return
                (EdmProperty)
                entityTypeMapppingFragment.Annotations.GetAnnotation(DefaultDiscriminatorAnnotation);
        }

        public static void SetDefaultDiscriminator(
            this DbEntityTypeMappingFragment entityTypeMappingFragment, EdmProperty discriminator)
        {
            Contract.Requires(entityTypeMappingFragment != null);

            entityTypeMappingFragment.Annotations.SetAnnotation(DefaultDiscriminatorAnnotation, discriminator);
        }

        public static void RemoveDefaultDiscriminatorAnnotation(
            this DbEntityTypeMappingFragment entityTypeMappingFragment)
        {
            Contract.Requires(entityTypeMappingFragment != null);

            entityTypeMappingFragment.Annotations.RemoveAnnotation(DefaultDiscriminatorAnnotation);
        }

        public static void RemoveDefaultDiscriminator(
            this DbEntityTypeMappingFragment entityTypeMappingFragment, DbEntitySetMapping entitySetMapping)
        {
            Contract.Requires(entityTypeMappingFragment != null);

            var discriminatorColumn = entityTypeMappingFragment.RemoveDefaultDiscriminatorCondition();
            if (discriminatorColumn != null)
            {
                var table = entityTypeMappingFragment.Table;

                table.Properties
                    .Where(c => c.Name.Equals(discriminatorColumn.Name, StringComparison.Ordinal))
                    .ToList()
                    .Each(table.RemoveMember);
            }

            if (entitySetMapping != null && entityTypeMappingFragment.IsConditionOnlyFragment()
                &&
                !entityTypeMappingFragment.ColumnConditions.Any())
            {
                var entityTypeMapping =
                    entitySetMapping.EntityTypeMappings.Single(
                        etm => etm.TypeMappingFragments.Contains(entityTypeMappingFragment));
                entityTypeMapping.TypeMappingFragments.Remove(entityTypeMappingFragment);
                if (entityTypeMapping.TypeMappingFragments.Count == 0)
                {
                    entitySetMapping.EntityTypeMappings.Remove(entityTypeMapping);
                }
            }
        }

        public static EdmProperty RemoveDefaultDiscriminatorCondition(
            this DbEntityTypeMappingFragment entityTypeMappingFragment)
        {
            Contract.Requires(entityTypeMappingFragment != null);

            var discriminatorColumn = entityTypeMappingFragment.GetDefaultDiscriminator();
            if (discriminatorColumn != null
                && entityTypeMappingFragment.ColumnConditions.Count > 0)
            {
                Contract.Assert(entityTypeMappingFragment.ColumnConditions.Count == 1);
                entityTypeMappingFragment.ColumnConditions.RemoveAt(0);
            }

            entityTypeMappingFragment.RemoveDefaultDiscriminatorAnnotation();

            return discriminatorColumn;
        }

        public static void AddDiscriminatorCondition(
            this DbEntityTypeMappingFragment entityTypeMapppingFragment,
            EdmProperty discriminatorColumn,
            object value)
        {
            Contract.Requires(entityTypeMapppingFragment != null);
            Contract.Requires(discriminatorColumn != null);
            Contract.Requires(value != null);

            entityTypeMapppingFragment
                .ColumnConditions
                .Add(
                    new DbColumnCondition
                        {
                            Column = discriminatorColumn,
                            Value = value
                        });
        }

        public static void AddNullabilityCondition(
            this DbEntityTypeMappingFragment entityTypeMapppingFragment,
            EdmProperty column,
            bool isNull)
        {
            Contract.Requires(entityTypeMapppingFragment != null);
            Contract.Requires(column != null);

            entityTypeMapppingFragment
                .ColumnConditions
                .Add(
                    new DbColumnCondition
                        {
                            Column = column,
                            IsNull = isNull
                        });
        }

        public static bool IsConditionOnlyFragment(this DbEntityTypeMappingFragment entityTypeMapppingFragment)
        {
            Contract.Requires(entityTypeMapppingFragment != null);

            var isConditionOnlyFragment =
                entityTypeMapppingFragment.Annotations.GetAnnotation(ConditionOnlyFragmentAnnotation);
            if (isConditionOnlyFragment != null)
            {
                return (bool)isConditionOnlyFragment;
            }
            return false;
        }

        public static void SetIsConditionOnlyFragment(
            this DbEntityTypeMappingFragment entityTypeMapppingFragment, bool isConditionOnlyFragment)
        {
            Contract.Requires(entityTypeMapppingFragment != null);

            if (isConditionOnlyFragment)
            {
                entityTypeMapppingFragment.Annotations.SetAnnotation(
                    ConditionOnlyFragmentAnnotation, isConditionOnlyFragment);
            }
            else
            {
                entityTypeMapppingFragment.Annotations.RemoveAnnotation(ConditionOnlyFragmentAnnotation);
            }
        }

        public static bool IsUnmappedPropertiesFragment(this DbEntityTypeMappingFragment entityTypeMapppingFragment)
        {
            Contract.Requires(entityTypeMapppingFragment != null);

            var isUnmappedPropertiesFragment =
                entityTypeMapppingFragment.Annotations.GetAnnotation(UnmappedPropertiesFragmentAnnotation);
            if (isUnmappedPropertiesFragment != null)
            {
                return (bool)isUnmappedPropertiesFragment;
            }
            return false;
        }

        public static void SetIsUnmappedPropertiesFragment(
            this DbEntityTypeMappingFragment entityTypeMapppingFragment, bool isUnmappedPropertiesFragment)
        {
            Contract.Requires(entityTypeMapppingFragment != null);

            if (isUnmappedPropertiesFragment)
            {
                entityTypeMapppingFragment.Annotations.SetAnnotation(
                    UnmappedPropertiesFragmentAnnotation, isUnmappedPropertiesFragment);
            }
            else
            {
                entityTypeMapppingFragment.Annotations.RemoveAnnotation(UnmappedPropertiesFragmentAnnotation);
            }
        }
    }
}
