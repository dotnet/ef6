// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal abstract class StructuralTypeConfiguration : ConfigurationBase
    {
        internal static Type GetPropertyConfigurationType(Type propertyType)
        {
            Contract.Requires(propertyType != null);

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
            Contract.Requires(clrType != null);

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

        public void Ignore(PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);

            _ignoredProperties.Add(propertyInfo);
        }

        internal PrimitivePropertyConfiguration Property(
            PropertyPath propertyPath, OverridableConfigurationParts? overridableConfigurationParts = null)
        {
            Contract.Requires(propertyPath != null);

            return Property(
                propertyPath,
                () =>
                    {
                        var configuration = (PrimitivePropertyConfiguration)Activator
                                                                                .CreateInstance(
                                                                                    GetPropertyConfigurationType(
                                                                                        propertyPath.Last().PropertyType));
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
            Contract.Requires(propertyPath != null);

            PrimitivePropertyConfiguration primitivePropertyConfiguration;
            if (!_primitivePropertyConfigurations.TryGetValue(propertyPath, out primitivePropertyConfiguration))
            {
                _primitivePropertyConfigurations.Add(
                    propertyPath, primitivePropertyConfiguration = primitivePropertyConfigurationCreator());
            }

            return (TPrimitivePropertyConfiguration)primitivePropertyConfiguration;
        }

        internal void Configure(
            IEnumerable<Tuple<DbEdmPropertyMapping, DbTableMetadata>> propertyMappings,
            DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            Contract.Requires(propertyMappings != null);
            Contract.Requires(providerManifest != null);

            foreach (var configuration in PrimitivePropertyConfigurations)
            {
                var propertyPath = configuration.Key;
                var propertyConfiguration = configuration.Value;

                propertyConfiguration.Configure(
                    propertyMappings.Where(
                        pm =>
                        propertyPath ==
                        new PropertyPath(
                            pm.Item1.PropertyPath
                            .Skip(pm.Item1.PropertyPath.Count - propertyPath.Count)
                            .Select(p => p.GetClrPropertyInfo()))
                        ),
                    providerManifest,
                    allowOverride);
            }
        }

        internal void Configure(
            string structuralTypeName,
            IEnumerable<EdmProperty> properties,
            ICollection<DataModelAnnotation> dataModelAnnotations)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(structuralTypeName));
            Contract.Requires(properties != null);
            Contract.Requires(dataModelAnnotations != null);

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

            if (property.PropertyType.IsUnderlyingPrimitiveType)
            {
                propertyConfiguration.Configure(property);
            }
            else
            {
                Configure(
                    property.PropertyType.ComplexType.Name,
                    property.PropertyType.ComplexType.DeclaredProperties,
                    new PropertyPath(propertyPath.Skip(1)),
                    propertyConfiguration);
            }
        }
    }
}
