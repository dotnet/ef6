// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal static class StorageMappingFragmentExtensions
    {
        private const string DefaultDiscriminatorAnnotation = "DefaultDiscriminator";
        private const string ConditionOnlyFragmentAnnotation = "ConditionOnlyFragment";
        private const string UnmappedPropertiesFragmentAnnotation = "UnmappedPropertiesFragment";

        public static EdmProperty GetDefaultDiscriminator(
            this MappingFragment entityTypeMapppingFragment)
        {
            DebugCheck.NotNull(entityTypeMapppingFragment);

            return
                (EdmProperty)
                entityTypeMapppingFragment.Annotations.GetAnnotation(DefaultDiscriminatorAnnotation);
        }

        public static void SetDefaultDiscriminator(
            this MappingFragment entityTypeMappingFragment, EdmProperty discriminator)
        {
            DebugCheck.NotNull(entityTypeMappingFragment);

            entityTypeMappingFragment.Annotations.SetAnnotation(DefaultDiscriminatorAnnotation, discriminator);
        }

        public static void RemoveDefaultDiscriminatorAnnotation(
            this MappingFragment entityTypeMappingFragment)
        {
            DebugCheck.NotNull(entityTypeMappingFragment);

            entityTypeMappingFragment.Annotations.RemoveAnnotation(DefaultDiscriminatorAnnotation);
        }

        public static void RemoveDefaultDiscriminator(
            this MappingFragment entityTypeMappingFragment, EntitySetMapping entitySetMapping)
        {
            DebugCheck.NotNull(entityTypeMappingFragment);

            var discriminatorColumn = entityTypeMappingFragment.RemoveDefaultDiscriminatorCondition();
            if (discriminatorColumn != null)
            {
                var table = entityTypeMappingFragment.Table;

                table.Properties
                     .Where(c => c.Name.Equals(discriminatorColumn.Name, StringComparison.Ordinal))
                     .ToList()
                     .Each(table.RemoveMember);
            }

            if (entitySetMapping != null
                && entityTypeMappingFragment.IsConditionOnlyFragment()
                &&
                !entityTypeMappingFragment.ColumnConditions.Any())
            {
                var entityTypeMapping =
                    entitySetMapping.EntityTypeMappings.Single(
                        etm => etm.MappingFragments.Contains(entityTypeMappingFragment));

                entityTypeMapping.RemoveFragment(entityTypeMappingFragment);

                if (entityTypeMapping.MappingFragments.Count == 0)
                {
                    entitySetMapping.RemoveTypeMapping(entityTypeMapping);
                }
            }
        }

        public static EdmProperty RemoveDefaultDiscriminatorCondition(
            this MappingFragment entityTypeMappingFragment)
        {
            DebugCheck.NotNull(entityTypeMappingFragment);

            var discriminatorColumn = entityTypeMappingFragment.GetDefaultDiscriminator();

            if (discriminatorColumn != null
                && entityTypeMappingFragment.ColumnConditions.Any())
            {
                Debug.Assert(entityTypeMappingFragment.ColumnConditions.Count() == 1);

                entityTypeMappingFragment.ClearConditions();
            }

            entityTypeMappingFragment.RemoveDefaultDiscriminatorAnnotation();

            return discriminatorColumn;
        }

        public static void AddDiscriminatorCondition(
            this MappingFragment entityTypeMapppingFragment,
            EdmProperty discriminatorColumn,
            object value)
        {
            DebugCheck.NotNull(entityTypeMapppingFragment);
            DebugCheck.NotNull(discriminatorColumn);
            DebugCheck.NotNull(value);

            entityTypeMapppingFragment
                .AddConditionProperty(
                    new ConditionPropertyMapping(null, discriminatorColumn, value, null));
        }

        public static void AddNullabilityCondition(
            this MappingFragment entityTypeMapppingFragment,
            EdmProperty column,
            bool isNull)
        {
            DebugCheck.NotNull(entityTypeMapppingFragment);
            DebugCheck.NotNull(column);

            entityTypeMapppingFragment
                .AddConditionProperty(
                    new ConditionPropertyMapping(null, column, null, isNull));
        }

        public static bool IsConditionOnlyFragment(this MappingFragment entityTypeMapppingFragment)
        {
            DebugCheck.NotNull(entityTypeMapppingFragment);

            var isConditionOnlyFragment =
                entityTypeMapppingFragment.Annotations.GetAnnotation(ConditionOnlyFragmentAnnotation);
            if (isConditionOnlyFragment != null)
            {
                return (bool)isConditionOnlyFragment;
            }
            return false;
        }

        public static void SetIsConditionOnlyFragment(
            this MappingFragment entityTypeMapppingFragment, bool isConditionOnlyFragment)
        {
            DebugCheck.NotNull(entityTypeMapppingFragment);

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

        public static bool IsUnmappedPropertiesFragment(this MappingFragment entityTypeMapppingFragment)
        {
            DebugCheck.NotNull(entityTypeMapppingFragment);

            var isUnmappedPropertiesFragment =
                entityTypeMapppingFragment.Annotations.GetAnnotation(UnmappedPropertiesFragmentAnnotation);
            if (isUnmappedPropertiesFragment != null)
            {
                return (bool)isUnmappedPropertiesFragment;
            }
            return false;
        }

        public static void SetIsUnmappedPropertiesFragment(
            this MappingFragment entityTypeMapppingFragment, bool isUnmappedPropertiesFragment)
        {
            DebugCheck.NotNull(entityTypeMapppingFragment);

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
