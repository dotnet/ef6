// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Used to configure a primitive property of an entity type or complex type.
    /// </summary>
    public class PrimitivePropertyConfiguration : PropertyConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the PrimitivePropertyConfiguration class.
        /// </summary>
        public PrimitivePropertyConfiguration()
        {
            OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace |
                                            OverridableConfigurationParts.OverridableInSSpace;
        }

        protected PrimitivePropertyConfiguration(PrimitivePropertyConfiguration source)
        {
            Check.NotNull(source, "source");

            TypeConfiguration = source.TypeConfiguration;
            IsNullable = source.IsNullable;
            ConcurrencyMode = source.ConcurrencyMode;
            DatabaseGeneratedOption = source.DatabaseGeneratedOption;
            ColumnType = source.ColumnType;
            ColumnName = source.ColumnName;
            ColumnOrder = source.ColumnOrder;
            OverridableConfigurationParts = source.OverridableConfigurationParts;
        }

        internal virtual PrimitivePropertyConfiguration Clone()
        {
            return new PrimitivePropertyConfiguration(this);
        }

        /// <summary>
        ///     Gets a value indicating whether the property is optional.
        /// </summary>
        public bool? IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets the concurrency mode to use for the property.
        /// </summary>
        public ConcurrencyMode? ConcurrencyMode { get; set; }

        /// <summary>
        ///     Gets or sets the pattern used to generate values in the database for the
        ///     property.
        /// </summary>
        public DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }

        /// <summary>
        ///     Gets or sets the type of the database column used to store the property.
        /// </summary>
        public string ColumnType { get; set; }

        /// <summary>
        ///     Gets or sets the name of the database column used to store the property.
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the order of the database column used to store the property.
        /// </summary>
        public int? ColumnOrder { get; set; }

        internal OverridableConfigurationParts OverridableConfigurationParts { get; set; }
        internal StructuralTypeConfiguration TypeConfiguration { get; set; }

        internal virtual void Configure(EdmProperty property)
        {
            DebugCheck.NotNull(property);
            Debug.Assert(property.TypeUsage != null);

            var existingConfiguration = property.GetConfiguration() as PrimitivePropertyConfiguration;
            if (existingConfiguration != null)
            {
                string errorMessage;
                if ((existingConfiguration.OverridableConfigurationParts
                     & OverridableConfigurationParts.OverridableInCSpace)
                    != OverridableConfigurationParts.OverridableInCSpace
                    && !existingConfiguration.IsCompatible(this, inCSpace: true, errorMessage: out errorMessage))
                {
                    var propertyInfo = property.GetClrPropertyInfo();
                    var declaringTypeName = propertyInfo == null
                                                ? string.Empty
                                                : ObjectContextTypeCache.GetObjectType(propertyInfo.DeclaringType).
                                                                         FullName;
                    throw Error.ConflictingPropertyConfiguration(property.Name, declaringTypeName, errorMessage);
                }

                // Choose the more derived type for the merged configuration
                PrimitivePropertyConfiguration mergedConfiguration;
                if (existingConfiguration.GetType().IsAssignableFrom(GetType()))
                {
                    mergedConfiguration = (PrimitivePropertyConfiguration)MemberwiseClone();
                }
                else
                {
                    mergedConfiguration = (PrimitivePropertyConfiguration)existingConfiguration.MemberwiseClone();
                    mergedConfiguration.CopyFrom(this);
                }
                mergedConfiguration.FillFrom(existingConfiguration, inCSpace: true);
                property.SetConfiguration(mergedConfiguration);
            }
            else
            {
                property.SetConfiguration(this);
            }

            if (IsNullable != null)
            {
                property.Nullable = IsNullable.Value;
            }

            if (ConcurrencyMode != null)
            {
                property.ConcurrencyMode = ConcurrencyMode.Value;
            }

            if (DatabaseGeneratedOption != null)
            {
                property.SetStoreGeneratedPattern((StoreGeneratedPattern)DatabaseGeneratedOption.Value);

                if (DatabaseGeneratedOption.Value
                    == ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)
                {
                    property.Nullable = false;
                }
            }
        }

        internal virtual void Configure(
            IEnumerable<Tuple<ColumnMappingBuilder, EntityType>> propertyMappings,
            DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            DebugCheck.NotNull(propertyMappings);
            DebugCheck.NotNull(providerManifest);

            propertyMappings.Each(pm => Configure(pm.Item1.ColumnProperty, pm.Item2, providerManifest, allowOverride));
        }

        internal virtual void Configure(
            EdmProperty column, EntityType table, DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            DebugCheck.NotNull(column);
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(providerManifest);

            var existingConfiguration = column.GetConfiguration() as PrimitivePropertyConfiguration;

            if (existingConfiguration != null)
            {
                var overridable = column.GetAllowOverride();

                string errorMessage;
                if ((existingConfiguration.OverridableConfigurationParts
                     & OverridableConfigurationParts.OverridableInSSpace)
                    != OverridableConfigurationParts.OverridableInSSpace
                    && !overridable
                    && !allowOverride
                    && !existingConfiguration.IsCompatible(this, inCSpace: false, errorMessage: out errorMessage))
                {
                    throw Error.ConflictingColumnConfiguration(column.Name, table.Name, errorMessage);
                }

                FillFrom(existingConfiguration, inCSpace: false);
            }

            ConfigureColumnName(column, table);

            if (!string.IsNullOrWhiteSpace(ColumnType))
            {
                column.PrimitiveType = providerManifest.GetStoreTypeFromName(ColumnType);
            }

            if (ColumnOrder != null)
            {
                column.SetOrder(ColumnOrder.Value);
            }

            var storeType
                = providerManifest.GetStoreTypes()
                                  .SingleOrDefault(t => t.Name.Equals(column.TypeName, StringComparison.OrdinalIgnoreCase));

            if (storeType != null)
            {
                storeType.FacetDescriptions.Each(f => Configure(column, f));
            }

            column.SetConfiguration(this);
            column.SetAllowOverride(allowOverride);
        }

        private void ConfigureColumnName(EdmProperty column, EntityType table)
        {
            if (string.IsNullOrWhiteSpace(ColumnName)
                || string.Equals(ColumnName, column.Name, StringComparison.Ordinal))
            {
                return;
            }

            column.Name = ColumnName;

            // find other unconfigured columns that have the same preferred name
            var pendingRenames
                = from c in table.Properties
                  let configuration = c.GetConfiguration() as PrimitivePropertyConfiguration
                  where (c != column)
                        && string.Equals(ColumnName, c.GetPreferredName(), StringComparison.Ordinal)
                        && ((configuration == null) || (configuration.ColumnName == null))
                  select c;

            var renamedColumns = new List<EdmProperty>
                                     {
                                         column
                                     };

            // re-uniquify the conflicting columns
            pendingRenames
                .Each(
                    c =>
                        {
                            c.Name = renamedColumns.UniquifyName(ColumnName);
                            renamedColumns.Add(c);
                        });
        }

        internal virtual void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            DebugCheck.NotNull(column);
            DebugCheck.NotNull(facetDescription);
        }

        internal virtual void CopyFrom(PrimitivePropertyConfiguration other)
        {
            ColumnName = other.ColumnName;
            ColumnOrder = other.ColumnOrder;
            ColumnType = other.ColumnType;
            ConcurrencyMode = other.ConcurrencyMode;
            DatabaseGeneratedOption = other.DatabaseGeneratedOption;
            IsNullable = other.IsNullable;
            OverridableConfigurationParts = other.OverridableConfigurationParts;
        }

        internal virtual void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            if (ColumnName == null
                && !inCSpace)
            {
                ColumnName = other.ColumnName;
            }
            if (ColumnOrder == null
                && !inCSpace)
            {
                ColumnOrder = other.ColumnOrder;
            }
            if (ColumnType == null
                && !inCSpace)
            {
                ColumnType = other.ColumnType;
            }
            if (ConcurrencyMode == null)
            {
                ConcurrencyMode = other.ConcurrencyMode;
            }
            if (DatabaseGeneratedOption == null)
            {
                DatabaseGeneratedOption = other.DatabaseGeneratedOption;
            }
            if (IsNullable == null && inCSpace)
            {
                IsNullable = other.IsNullable;
            }

            OverridableConfigurationParts &= other.OverridableConfigurationParts;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        internal virtual bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (other == null)
            {
                return true;
            }

            var isNullableIsCompatible = !inCSpace || IsCompatible(c => c.IsNullable, other, ref errorMessage);
            var concurrencyModeIsCompatible = !inCSpace || IsCompatible(c => c.ConcurrencyMode, other, ref errorMessage);
            var databaseGeneratedOptionIsCompatible = !inCSpace
                                                      ||
                                                      IsCompatible(
                                                          c => c.DatabaseGeneratedOption, other, ref errorMessage);
            var columnNameIsCompatible = inCSpace || IsCompatible(c => c.ColumnName, other, ref errorMessage);
            var columnOrderIsCompatible = inCSpace || IsCompatible(c => c.ColumnOrder, other, ref errorMessage);
            var columnTypeIsCompatible = inCSpace || IsCompatible(c => c.ColumnType, other, ref errorMessage);

            return isNullableIsCompatible &&
                   concurrencyModeIsCompatible &&
                   databaseGeneratedOptionIsCompatible &&
                   columnNameIsCompatible &&
                   columnOrderIsCompatible &&
                   columnTypeIsCompatible;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        protected bool IsCompatible<TProperty, TConfiguration>(
            Expression<Func<TConfiguration, TProperty?>> propertyExpression, TConfiguration other, ref string errorMessage)
            where TProperty : struct
            where TConfiguration : PrimitivePropertyConfiguration
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotNull(other, "other");

            var propertyInfo = propertyExpression.GetSimplePropertyAccess().Single();
            var thisValue = (TProperty?)propertyInfo.GetValue(this, null);
            var otherValue = (TProperty?)propertyInfo.GetValue(other, null);

            if (IsCompatible(thisValue, otherValue))
            {
                return true;
            }

            errorMessage += Environment.NewLine + "\t" +
                            Strings.ConflictingConfigurationValue(
                                propertyInfo.Name, thisValue, propertyInfo.Name, otherValue);
            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        protected bool IsCompatible<TConfiguration>(
            Expression<Func<TConfiguration, string>> propertyExpression, TConfiguration other, ref string errorMessage)
            where TConfiguration : PrimitivePropertyConfiguration
        {
            Check.NotNull(propertyExpression, "propertyExpression");
            Check.NotNull(other, "other");

            var propertyInfo = propertyExpression.GetSimplePropertyAccess().Single();
            var thisValue = (string)propertyInfo.GetValue(this, null);
            var otherValue = (string)propertyInfo.GetValue(other, null);

            if (IsCompatible(thisValue, otherValue))
            {
                return true;
            }

            errorMessage += Environment.NewLine + "\t" +
                            Strings.ConflictingConfigurationValue(
                                propertyInfo.Name, thisValue, propertyInfo.Name, otherValue);
            return false;
        }

        protected static bool IsCompatible<T>(T? thisConfiguration, T? other)
            where T : struct
        {
            if (thisConfiguration.HasValue)
            {
                if (other.HasValue)
                {
                    return Equals(thisConfiguration.Value, other.Value);
                }

                return true;
            }

            return true;
        }

        protected static bool IsCompatible(string thisConfiguration, string other)
        {
            if (thisConfiguration != null)
            {
                if (other != null)
                {
                    return Equals(thisConfiguration, other);
                }

                return true;
            }

            return true;
        }
    }
}
