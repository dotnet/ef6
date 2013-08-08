// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Allows configuration to be performed for a type in a model.
    /// </summary>
    internal abstract class StructuralTypeConfiguration : ConfigurationBase
    {
        internal static Type GetPropertyConfigurationType(Type propertyType)
        {
            DebugCheck.NotNull(propertyType);

            propertyType.TryUnwrapNullableType(out propertyType);

            if (propertyType == typeof(string))
            {
                return typeof(StringPropertyConfiguration);
            }

            if (propertyType == typeof(decimal))
            {
                return typeof(DecimalPropertyConfiguration);
            }

            if (propertyType == typeof(DateTime)
                || propertyType == typeof(TimeSpan)
                || propertyType == typeof(DateTimeOffset))
            {
                return typeof(DateTimePropertyConfiguration);
            }

            if (propertyType == typeof(byte[]))
            {
                return typeof(BinaryPropertyConfiguration);
            }

            return (propertyType.IsValueType
                    || propertyType == typeof(DbGeography)
                    || propertyType == typeof(DbGeometry)
                   )
                       ? typeof(PrimitivePropertyConfiguration)
                       : typeof(NavigationPropertyConfiguration);
        }

        private readonly Dictionary<PropertyPath, PrimitivePropertyConfiguration> _primitivePropertyConfigurations
            = new Dictionary<PropertyPath, PrimitivePropertyConfiguration>();

        private readonly HashSet<PropertyInfo> _ignoredProperties = new HashSet<PropertyInfo>();

        private readonly Type _clrType;

        internal StructuralTypeConfiguration()
        {
        }

        internal StructuralTypeConfiguration(Type clrType)
        {
            DebugCheck.NotNull(clrType);

            _clrType = clrType;
        }

        internal StructuralTypeConfiguration(StructuralTypeConfiguration source)
        {
            source._primitivePropertyConfigurations.Each(
                c => _primitivePropertyConfigurations.Add(c.Key, c.Value.Clone()));

            _ignoredProperties.AddRange(source._ignoredProperties);

            _clrType = source._clrType;
        }

        internal virtual IEnumerable<PropertyInfo> ConfiguredProperties
        {
            get { return _primitivePropertyConfigurations.Keys.Select(p => p.Last()); }
        }

        internal IEnumerable<PropertyInfo> IgnoredProperties
        {
            get { return _ignoredProperties; }
        }

        internal Type ClrType
        {
            get { return _clrType; }
        }

        internal IEnumerable<KeyValuePair<PropertyPath, PrimitivePropertyConfiguration>> PrimitivePropertyConfigurations
        {
            get { return _primitivePropertyConfigurations; }
        }

        /// <summary>
        /// Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <param name="propertyInfo"> The property to be configured. </param>
        public void Ignore(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, "propertyInfo");

            _ignoredProperties.Add(propertyInfo);
        }

        internal PrimitivePropertyConfiguration Property(
            PropertyPath propertyPath, OverridableConfigurationParts? overridableConfigurationParts = null)
        {
            DebugCheck.NotNull(propertyPath);

            return Property(
                propertyPath,
                () =>
                    {
                        var configuration = (PrimitivePropertyConfiguration)Activator
                                                                                .CreateInstance(
                                                                                    GetPropertyConfigurationType(
                                                                                        propertyPath.Last().PropertyType));
                        configuration.TypeConfiguration = this;

                        if (overridableConfigurationParts.HasValue)
                        {
                            configuration.OverridableConfigurationParts = overridableConfigurationParts.Value;
                        }
                        return configuration;
                    });
        }

        internal virtual void RemoveProperty(PropertyPath propertyPath)
        {
            _primitivePropertyConfigurations.Remove(propertyPath);
        }

        internal TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(
            PropertyPath propertyPath, Func<TPrimitivePropertyConfiguration> primitivePropertyConfigurationCreator)
            where TPrimitivePropertyConfiguration : PrimitivePropertyConfiguration
        {
            DebugCheck.NotNull(propertyPath);

            PrimitivePropertyConfiguration primitivePropertyConfiguration;
            if (!_primitivePropertyConfigurations.TryGetValue(propertyPath, out primitivePropertyConfiguration))
            {
                _primitivePropertyConfigurations.Add(
                    propertyPath, primitivePropertyConfiguration = primitivePropertyConfigurationCreator());
            }

            return (TPrimitivePropertyConfiguration)primitivePropertyConfiguration;
        }

        internal void ConfigurePropertyMappings(
            IList<Tuple<ColumnMappingBuilder, EntityType>> propertyMappings,
            DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            DebugCheck.NotNull(propertyMappings);
            DebugCheck.NotNull(providerManifest);

            foreach (var configuration in PrimitivePropertyConfigurations)
            {
                var propertyPath = configuration.Key;
                var propertyConfiguration = configuration.Value;

                propertyConfiguration.Configure(
                    propertyMappings.Where(
                        pm =>
                        propertyPath.Equals(
                            new PropertyPath(
                            pm.Item1.PropertyPath
                              .Skip(pm.Item1.PropertyPath.Count - propertyPath.Count)
                              .Select(p => p.GetClrPropertyInfo()))
                            )),
                    providerManifest,
                    allowOverride);
            }
        }

        internal void ConfigureFunctionParameters(IList<StorageModificationFunctionParameterBinding> parameterBindings)
        {
            DebugCheck.NotNull(parameterBindings);

            foreach (var configuration in PrimitivePropertyConfigurations)
            {
                var propertyPath = configuration.Key;
                var propertyConfiguration = configuration.Value;

                var parameters
                    = parameterBindings
                        .Where(
                            pb =>
                            (pb.MemberPath.AssociationSetEnd == null)
                            && propertyPath.Equals(
                                new PropertyPath(
                                   pb.MemberPath.Members
                                     .Skip(pb.MemberPath.Members.Count - propertyPath.Count)
                                     .Select(m => m.GetClrPropertyInfo()))))
                        .Select(pb => pb.Parameter);

                propertyConfiguration.ConfigureFunctionParameters(parameters);
            }
        }

        internal void Configure(
            string structuralTypeName,
            IEnumerable<EdmProperty> properties,
            ICollection<MetadataProperty> dataModelAnnotations)
        {
            DebugCheck.NotEmpty(structuralTypeName);
            DebugCheck.NotNull(properties);
            DebugCheck.NotNull(dataModelAnnotations);

            dataModelAnnotations.SetConfiguration(this);

            foreach (var configuration in _primitivePropertyConfigurations)
            {
                var propertyPath = configuration.Key;
                var propertyConfiguration = configuration.Value;

                Configure(structuralTypeName, properties, propertyPath, propertyConfiguration);
            }
        }

        private static void Configure(
            string structuralTypeName,
            IEnumerable<EdmProperty> properties,
            IEnumerable<PropertyInfo> propertyPath,
            PrimitivePropertyConfiguration propertyConfiguration)
        {
            var property = properties.SingleOrDefault(p => p.GetClrPropertyInfo().IsSameAs(propertyPath.First()));

            if (property == null)
            {
                throw Error.PropertyNotFound(propertyPath.First().Name, structuralTypeName);
            }

            if (property.IsUnderlyingPrimitiveType)
            {
                propertyConfiguration.Configure(property);
            }
            else
            {
                Configure(
                    property.ComplexType.Name,
                    property.ComplexType.Properties,
                    new PropertyPath(propertyPath.Skip(1)),
                    propertyConfiguration);
            }
        }
    }
}
