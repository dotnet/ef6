// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    // <summary>
    // Used to configure a primitive property of an entity type or complex type.
    // </summary>
    internal class PrimitivePropertyConfiguration : PropertyConfiguration
    {
        private readonly IDictionary<string, object> _annotations = new Dictionary<string, object>();

        // <summary>
        // Initializes a new instance of the PrimitivePropertyConfiguration class.
        // </summary>
        public PrimitivePropertyConfiguration()
        {
            OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace |
                                            OverridableConfigurationParts.OverridableInSSpace;
        }

        // <summary>
        // Initializes a new instance of the <see cref="T:System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration" /> 
        // class with the same settings as another configuration.
        // </summary>
        // <param name="source">The configuration to copy settings from.</param>
        protected PrimitivePropertyConfiguration(PrimitivePropertyConfiguration source)
        {
            Check.NotNull(source, "source");

            TypeConfiguration = source.TypeConfiguration;
            IsNullable = source.IsNullable;
            ConcurrencyMode = source.ConcurrencyMode;
            DatabaseGeneratedOption = source.DatabaseGeneratedOption;
            ColumnType = source.ColumnType;
            ColumnName = source.ColumnName;
            ParameterName = source.ParameterName;
            ColumnOrder = source.ColumnOrder;
            OverridableConfigurationParts = source.OverridableConfigurationParts;

            foreach (var annotation in source._annotations)
            {
                _annotations.Add(annotation);
            }
        }

        internal virtual PrimitivePropertyConfiguration Clone()
        {
            return new PrimitivePropertyConfiguration(this);
        }

        // <summary>
        // Gets a value indicating whether the property is optional.
        // </summary>
        public bool? IsNullable { get; set; }

        // <summary>
        // Gets or sets the concurrency mode to use for the property.
        // </summary>
        public ConcurrencyMode? ConcurrencyMode { get; set; }

        // <summary>
        // Gets or sets the pattern used to generate values in the database for the
        // property.
        // </summary>
        public DatabaseGeneratedOption? DatabaseGeneratedOption { get; set; }

        // <summary>
        // Gets or sets the type of the database column used to store the property.
        // </summary>
        public string ColumnType { get; set; }

        // <summary>
        // Gets or sets the name of the database column used to store the property.
        // </summary>
        public string ColumnName { get; set; }

        public IDictionary<string, object> Annotations
        {
            get { return _annotations; }
        }

        public virtual void SetAnnotation(string name, object value)
        {
            // Technically we could accept some names that are invalid in EDM, but this is not too restrictive
            // and is an easy way of ensuring that name is valid all places we want to use it--i.e. in the XML
            // and in the MetadataWorkspace.
            if (!name.IsValidUndottedName())
            {
                throw new ArgumentException(Strings.BadAnnotationName(name));
            }

            _annotations[name] = value;
        }

        // <summary>Gets or sets the name of the parameter used in stored procedures for this property.</summary>
        // <returns>The name of the parameter used in stored procedures for this property.</returns>
        public string ParameterName { get; set; }

        // <summary>
        // Gets or sets the order of the database column used to store the property.
        // </summary>
        public int? ColumnOrder { get; set; }

        internal OverridableConfigurationParts OverridableConfigurationParts { get; set; }
        internal StructuralTypeConfiguration TypeConfiguration { get; set; }

        internal virtual void Configure(EdmProperty property)
        {
            DebugCheck.NotNull(property);
            Debug.Assert(property.TypeUsage != null);

            var clone = Clone();
            var mergedConfiguration = clone.MergeWithExistingConfiguration(
                property,
                errorMessage =>
                {
                    var propertyInfo = property.GetClrPropertyInfo();
                    var declaringTypeName = propertyInfo == null
                        ? string.Empty
                        : ObjectContextTypeCache.GetObjectType(propertyInfo.DeclaringType).
                            FullNameWithNesting();
                    return Error.ConflictingPropertyConfiguration(property.Name, declaringTypeName, errorMessage);
                },
                inCSpace: true,
                fillFromExistingConfiguration: false);

            mergedConfiguration.ConfigureProperty(property);
        }

        private PrimitivePropertyConfiguration MergeWithExistingConfiguration(
            EdmProperty property, Func<string, Exception> getConflictException, bool inCSpace, bool fillFromExistingConfiguration)
        {
            var existingConfiguration = property.GetConfiguration() as PrimitivePropertyConfiguration;
            if (existingConfiguration != null)
            {
                var space = inCSpace ? OverridableConfigurationParts.OverridableInCSpace : OverridableConfigurationParts.OverridableInSSpace;
                if (existingConfiguration.OverridableConfigurationParts.HasFlag(space)
                    || fillFromExistingConfiguration)
                {
                    return existingConfiguration.OverrideFrom(this, inCSpace);
                }

                string errorMessage;
                if (OverridableConfigurationParts.HasFlag(space)
                    || existingConfiguration.IsCompatible(this, inCSpace, errorMessage: out errorMessage))
                {
                    return OverrideFrom(existingConfiguration, inCSpace);
                }

                throw getConflictException(errorMessage);
            }

            return this;
        }

        private PrimitivePropertyConfiguration OverrideFrom(PrimitivePropertyConfiguration overridingConfiguration, bool inCSpace)
        {
            if (overridingConfiguration.GetType().IsAssignableFrom(GetType()))
            {
                MakeCompatibleWith(overridingConfiguration, inCSpace);
                FillFrom(overridingConfiguration, inCSpace);

                return this;
            }
            else
            {
                overridingConfiguration.FillFrom(this, inCSpace);

                return overridingConfiguration;
            }
        }

        protected virtual void ConfigureProperty(EdmProperty property)
        {
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

            property.SetConfiguration(this);
        }

        internal void Configure(
            IEnumerable<Tuple<ColumnMappingBuilder, EntityType>> propertyMappings,
            DbProviderManifest providerManifest,
            bool allowOverride = false,
            bool fillFromExistingConfiguration = false)
        {
            DebugCheck.NotNull(propertyMappings);
            DebugCheck.NotNull(providerManifest);

            propertyMappings.Each(pm => Configure(
                pm.Item1.ColumnProperty,
                pm.Item2,
                providerManifest,
                allowOverride,
                fillFromExistingConfiguration));
        }

        internal void ConfigureFunctionParameters(IEnumerable<FunctionParameter> parameters)
        {
            DebugCheck.NotNull(parameters);

            parameters.Each(ConfigureParameterName);
        }

        private void ConfigureParameterName(FunctionParameter parameter)
        {
            DebugCheck.NotNull(parameter);

            if (string.IsNullOrWhiteSpace(ParameterName)
                || string.Equals(ParameterName, parameter.Name, StringComparison.Ordinal))
            {
                return;
            }

            parameter.Name = ParameterName;

            // find other unconfigured parameters that have the same preferred name

            var pendingRenames
                = from p in parameter.DeclaringFunction.Parameters
                  let configuration = p.GetConfiguration() as PrimitivePropertyConfiguration
                  where (p != parameter)
                        && string.Equals(ParameterName, p.Name, StringComparison.Ordinal)
                        && ((configuration == null) || (configuration.ParameterName == null))
                  select p;

            var renamedParameters
                = new List<FunctionParameter>
                    {
                        parameter
                    };

            // re-uniquify the conflicting parameters
            pendingRenames
                .Each(
                    c =>
                    {
                        c.Name = renamedParameters.UniquifyName(ParameterName);
                        renamedParameters.Add(c);
                    });

            parameter.SetConfiguration(this);
        }

        internal void Configure(
            EdmProperty column, EntityType table, DbProviderManifest providerManifest,
            bool allowOverride = false,
            bool fillFromExistingConfiguration = false)
        {
            DebugCheck.NotNull(column);
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(providerManifest);

            var clone = Clone();
            if (allowOverride)
            {
                clone.OverridableConfigurationParts |= OverridableConfigurationParts.OverridableInSSpace;
            }

            var mergedConfiguration = clone.MergeWithExistingConfiguration(
                column,
                errorMessage => Error.ConflictingColumnConfiguration(column.Name, table.Name, errorMessage),
                /* inCSpace: */ false,
                fillFromExistingConfiguration);

            mergedConfiguration.ConfigureColumn(column, table, providerManifest);
        }

        protected virtual void ConfigureColumn(EdmProperty column, EntityType table, DbProviderManifest providerManifest)
        {
            ConfigureColumnName(column, table);

            ConfigureAnnotations(column);

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

            var renamedColumns
                = new List<EdmProperty>
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

        private void ConfigureAnnotations(EdmProperty column)
        {
            foreach (var annotation in _annotations)
            {
                column.AddAnnotation(XmlConstants.CustomAnnotationPrefix + annotation.Key, annotation.Value);
            }
        }

        internal virtual void Configure(EdmProperty column, FacetDescription facetDescription)
        {
            DebugCheck.NotNull(column);
            DebugCheck.NotNull(facetDescription);
        }

        internal virtual void CopyFrom(PrimitivePropertyConfiguration other)
        {
            if (ReferenceEquals(this, other))
            {
                return;
            }

            ColumnName = other.ColumnName;
            ParameterName = other.ParameterName;
            ColumnOrder = other.ColumnOrder;
            ColumnType = other.ColumnType;
            ConcurrencyMode = other.ConcurrencyMode;
            DatabaseGeneratedOption = other.DatabaseGeneratedOption;
            IsNullable = other.IsNullable;
            OverridableConfigurationParts = other.OverridableConfigurationParts;

            _annotations.Clear();
            foreach (var annotation in other._annotations)
            {
                _annotations[annotation.Key] = annotation.Value;
            }
        }

        internal virtual void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            if (ReferenceEquals(this, other))
            {
                return;
            }

            if (inCSpace)
            {
                if (ConcurrencyMode == null)
                {
                    ConcurrencyMode = other.ConcurrencyMode;
                }

                if (DatabaseGeneratedOption == null)
                {
                    DatabaseGeneratedOption = other.DatabaseGeneratedOption;
                }

                if (IsNullable == null)
                {
                    IsNullable = other.IsNullable;
                }

                if (!other.OverridableConfigurationParts.HasFlag(OverridableConfigurationParts.OverridableInCSpace))
                {
                    OverridableConfigurationParts &= ~OverridableConfigurationParts.OverridableInCSpace;
                }
            }
            else
            {
                if (ColumnName == null)
                {
                    ColumnName = other.ColumnName;
                }

                if (ParameterName == null)
                {
                    ParameterName = other.ParameterName;
                }

                if (ColumnOrder == null)
                {
                    ColumnOrder = other.ColumnOrder;
                }

                if (ColumnType == null)
                {
                    ColumnType = other.ColumnType;
                }

                foreach (var annotation in other._annotations)
                {
                    if (_annotations.ContainsKey(annotation.Key))
                    {
                        var mergeableAnnotation = _annotations[annotation.Key] as IMergeableAnnotation;
                        if (mergeableAnnotation != null)
                        {
                            _annotations[annotation.Key] = mergeableAnnotation.MergeWith(annotation.Value);
                        }
                    }
                    else
                    {
                        _annotations[annotation.Key] = annotation.Value;
                    }
                }

                if (!other.OverridableConfigurationParts.HasFlag(OverridableConfigurationParts.OverridableInSSpace))
                {
                    OverridableConfigurationParts &= ~OverridableConfigurationParts.OverridableInSSpace;
                }
            }
        }

        internal virtual void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            DebugCheck.NotNull(other);

            if (ReferenceEquals(this, other))
            {
                return;
            }

            if (inCSpace)
            {
                if (other.ConcurrencyMode != null)
                {
                    ConcurrencyMode = null;
                }
                if (other.DatabaseGeneratedOption != null)
                {
                    DatabaseGeneratedOption = null;
                }
                if (other.IsNullable != null)
                {
                    IsNullable = null;
                }
            }
            else
            {
                if (other.ColumnName != null)
                {
                    ColumnName = null;
                }
                if (other.ParameterName != null)
                {
                    ParameterName = null;
                }
                if (other.ColumnOrder != null)
                {
                    ColumnOrder = null;
                }
                if (other.ColumnType != null)
                {
                    ColumnType = null;
                }

                foreach (var annotationName in other._annotations.Keys)
                {
                    if (_annotations.ContainsKey(annotationName))
                    {
                        var mergeableAnnotation = _annotations[annotationName] as IMergeableAnnotation;
                        if (mergeableAnnotation == null
                            || !mergeableAnnotation.IsCompatibleWith(other._annotations[annotationName]))
                        {
                            _annotations.Remove(annotationName);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        internal virtual bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (other == null
                || ReferenceEquals(this, other))
            {
                return true;
            }

            var isNullableIsCompatible = !inCSpace || IsCompatible(c => c.IsNullable, other, ref errorMessage);
            var concurrencyModeIsCompatible = !inCSpace || IsCompatible(c => c.ConcurrencyMode, other, ref errorMessage);
            var databaseGeneratedOptionIsCompatible = !inCSpace || IsCompatible(c => c.DatabaseGeneratedOption, other, ref errorMessage);
            var columnNameIsCompatible = inCSpace || IsCompatible(c => c.ColumnName, other, ref errorMessage);
            var parameterNameIsCompatible = inCSpace || IsCompatible(c => c.ParameterName, other, ref errorMessage);
            var columnOrderIsCompatible = inCSpace || IsCompatible(c => c.ColumnOrder, other, ref errorMessage);
            var columnTypeIsCompatible = inCSpace || IsCompatible(c => c.ColumnType, other, ref errorMessage);
            var annotationsAreCompatible = inCSpace || AnnotationsAreCompatible(other, ref errorMessage);

            return isNullableIsCompatible &&
                   concurrencyModeIsCompatible &&
                   databaseGeneratedOptionIsCompatible &&
                   columnNameIsCompatible &&
                   parameterNameIsCompatible &&
                   columnOrderIsCompatible &&
                   columnTypeIsCompatible &&
                   annotationsAreCompatible;
        }

        private bool AnnotationsAreCompatible(PrimitivePropertyConfiguration other, ref string errorMessage)
        {
            var annotationsAreCompatible = true;

            foreach (var annotation in Annotations)
            {
                if (other.Annotations.ContainsKey(annotation.Key))
                {
                    var value = annotation.Value;
                    var otherValue = other.Annotations[annotation.Key];

                    var mergeableAnnotation = value as IMergeableAnnotation;
                    if (mergeableAnnotation != null)
                    {
                        var isCompatible = mergeableAnnotation.IsCompatibleWith(otherValue);
                        if (!isCompatible)
                        {
                            annotationsAreCompatible = false;

                            errorMessage += Environment.NewLine + "\t" + isCompatible.ErrorMessage;
                        }
                    }
                    else if (!Equals(value, otherValue))
                    {
                        annotationsAreCompatible = false;

                        errorMessage += Environment.NewLine + "\t" +
                                        Strings.ConflictingAnnotationValue(
                                            annotation.Key, value.ToString(), otherValue.ToString());
                    }
                }
            }
            return annotationsAreCompatible;
        }

        // <summary>Gets a value that indicates whether the provided model is compatible with the current model provider.</summary>
        // <returns>true if the provided model is compatible with the current model provider; otherwise, false.</returns>
        // <param name="propertyExpression">The original property expression that specifies the member and instance.</param>
        // <param name="other">The property to compare.</param>
        // <param name="errorMessage">The error message.</param>
        // <typeparam name="TProperty">The type of the property.</typeparam>
        // <typeparam name="TConfiguration">The type of the configuration to look for.</typeparam>
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

        // <summary>Gets a value that indicates whether the provided model is compatible with the current model provider.</summary>
        // <returns>true if the provided model is compatible with the current model provider; otherwise, false.</returns>
        // <param name="propertyExpression">The property expression.</param>
        // <param name="other">The property to compare.</param>
        // <param name="errorMessage">The error message.</param>
        // <typeparam name="TConfiguration">The type of the configuration to look for.</typeparam>
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

        // <summary>Gets a value that indicates whether the provided model is compatible with the current model provider.</summary>
        // <returns>true if the provided model is compatible with the current model provider; otherwise, false.</returns>
        // <param name="thisConfiguration">The configuration property.</param>
        // <param name="other">The property to compare</param>
        // <typeparam name="T">The type property.</typeparam>
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

        // <summary>Gets a value that indicates whether the provided model is compatible with the current model provider.</summary>
        // <returns>true if the provided model is compatible with the current model provider; otherwise, false.</returns>
        // <param name="thisConfiguration">The configuration property.</param>
        // <param name="other">The property to compare.</param>
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
