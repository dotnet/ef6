namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using EdmProperty = System.Data.Entity.Edm.EdmProperty;

    internal class PrimitivePropertyConfiguration : PropertyConfiguration
    {
        public PrimitivePropertyConfiguration()
        {
            OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace |
                                            OverridableConfigurationParts.OverridableInSSpace;
        }

        public PrimitivePropertyConfiguration(PrimitivePropertyConfiguration source)
        {
            Contract.Requires(source != null);

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

        public bool? IsNullable { get; set; }

        public EdmConcurrencyMode? ConcurrencyMode { get; set; }
        public DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }

        public string ColumnType { get; set; }
        public string ColumnName { get; set; }
        public int? ColumnOrder { get; set; }

        public OverridableConfigurationParts OverridableConfigurationParts { get; set; }

        internal virtual void Configure(EdmProperty property)
        {
            Contract.Requires(property != null);
            Contract.Assert(property.PropertyType != null);
            Contract.Assert(property.PropertyType.PrimitiveTypeFacets != null);

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
                property.PropertyType.IsNullable = IsNullable;
            }

            if (ConcurrencyMode != null)
            {
                property.ConcurrencyMode = ConcurrencyMode.Value;
            }

            if (DatabaseGeneratedOption != null)
            {
                property.SetStoreGeneratedPattern((DbStoreGeneratedPattern)DatabaseGeneratedOption.Value);

                if (DatabaseGeneratedOption.Value
                    == ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)
                {
                    property.PropertyType.IsNullable = false;
                }
            }
        }

        internal virtual void Configure(
            IEnumerable<Tuple<DbEdmPropertyMapping, DbTableMetadata>> propertyMappings,
            DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            Contract.Requires(propertyMappings != null);
            Contract.Requires(providerManifest != null);

            propertyMappings.Each(pm => Configure(pm.Item1.Column, pm.Item2, providerManifest, allowOverride));
        }

        internal virtual void Configure(
            DbTableColumnMetadata column, DbTableMetadata table, DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            Contract.Requires(column != null);
            Contract.Requires(table != null);
            Contract.Requires(providerManifest != null);

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
                column.TypeName = ColumnType;
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
                storeType.FacetDescriptions.Each(f => Configure(column.Facets, f));
            }

            column.SetConfiguration(this);
            column.SetAllowOverride(allowOverride);
        }

        private void ConfigureColumnName(DbTableColumnMetadata column, DbTableMetadata table)
        {
            if (string.IsNullOrWhiteSpace(ColumnName)
                || string.Equals(ColumnName, column.Name, StringComparison.Ordinal))
            {
                return;
            }

            column.Name = ColumnName;

            // find other unconfigured columns that have the same preferred name
            var pendingRenames
                = from c in table.Columns
                  let configuration = c.GetConfiguration() as PrimitivePropertyConfiguration
                  where (c != column)
                        && string.Equals(ColumnName, c.GetPreferredName(), StringComparison.Ordinal)
                        && ((configuration == null) || (configuration.ColumnName == null))
                  select c;

            var renamedColumns = new List<DbColumnMetadata>
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

        internal virtual void Configure(DbPrimitiveTypeFacets facets, FacetDescription facetDescription)
        {
            Contract.Requires(facets != null);
            Contract.Requires(facetDescription != null);
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

        public virtual void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
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

        public virtual bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
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

        protected bool IsCompatible<T, C>(Expression<Func<C, T?>> propertyExpression, C other, ref string errorMessage)
            where T : struct
            where C : PrimitivePropertyConfiguration
        {
            Contract.Requires(propertyExpression != null);
            Contract.Requires(other != null);

            var propertyInfo = propertyExpression.GetSimplePropertyAccess().Single();
            var thisValue = (T?)propertyInfo.GetValue(this, null);
            var otherValue = (T?)propertyInfo.GetValue(other, null);

            if (IsCompatible(thisValue, otherValue))
            {
                return true;
            }

            errorMessage += Environment.NewLine + "\t" +
                            Strings.ConflictingConfigurationValue(
                                propertyInfo.Name, thisValue, propertyInfo.Name, otherValue);
            return false;
        }

        protected bool IsCompatible<C>(Expression<Func<C, string>> propertyExpression, C other, ref string errorMessage)
            where C : PrimitivePropertyConfiguration
        {
            Contract.Requires(propertyExpression != null);
            Contract.Requires(other != null);

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
