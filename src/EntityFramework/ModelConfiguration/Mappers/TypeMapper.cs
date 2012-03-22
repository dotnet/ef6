namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal sealed class TypeMapper
    {
        private readonly MappingContext _mappingContext;
        private readonly List<Type> _knownTypes = new List<Type>();

        public TypeMapper(MappingContext mappingContext)
        {
            Contract.Requires(mappingContext != null);

            _mappingContext = mappingContext;

            _knownTypes.AddRange(
                mappingContext.ModelConfiguration
                    .ConfiguredTypes
                    .Select(t => t.Assembly)
                    .Distinct()
                    .SelectMany(a => a.GetTypes().Where(type => type.IsValidStructuralType())));
        }

        public MappingContext MappingContext
        {
            get { return _mappingContext; }
        }

        public EdmEnumType MapEnumType(Type type)
        {
            Contract.Requires(type != null);
            Contract.Assert(type.IsEnum);

            var enumType = _mappingContext.Model.GetEnumType(type.Name);

            if (enumType == null)
            {
                EdmPrimitiveType primitiveType;
                if (!Enum.GetUnderlyingType(type).IsPrimitiveType(out primitiveType))
                {
                    return null;
                }

                enumType = _mappingContext.Model.AddEnumType(type.Name);
                enumType.IsFlags = type.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
                enumType.SetClrType(type);

                enumType.UnderlyingType = primitiveType;

                Enum.GetNames(type)
                    .Zip(
                        Enum.GetValues(type).Cast<object>(), (n, v) => new
                                                                           {
                                                                               n,
                                                                               v
                                                                           })
                    .Each(m => enumType.AddMember(m.n, Convert.ToInt64(m.v, CultureInfo.InvariantCulture)));
            }
            else if (type != enumType.GetClrType())
            {
                // O/C mapping collision
                return null;
            }

            return enumType;
        }

        public EdmComplexType MapComplexType(Type type, bool discoverNested = false)
        {
            Contract.Requires(type != null);

            if (!type.IsValidStructuralType())
            {
                return null;
            }

            _mappingContext.ConventionsConfiguration.ApplyModelConfiguration(type, _mappingContext.ModelConfiguration);

            if (_mappingContext.ModelConfiguration.IsIgnoredType(type)
                || (!discoverNested && !_mappingContext.ModelConfiguration.IsComplexType(type)))
            {
                return null;
            }

            var complexType = _mappingContext.Model.GetComplexType(type.Name);

            if (complexType == null)
            {
                complexType = _mappingContext.Model.AddComplexType(type.Name);

                var complexTypeConfiguration
                    = new Func<ComplexTypeConfiguration>(() => _mappingContext.ModelConfiguration.ComplexType(type));

                _mappingContext.ConventionsConfiguration.ApplyTypeConfiguration(type, complexTypeConfiguration);

                MapStructuralElements(
                    type,
                    complexType.Annotations,
                    (m, p) => m.Map(p, complexType, complexTypeConfiguration),
                    false,
                    complexTypeConfiguration);
            }
            else if (type != complexType.GetClrType())
            {
                // O/C mapping collision
                return null;
            }

            return complexType;
        }

        public EdmEntityType MapEntityType(Type type)
        {
            Contract.Requires(type != null);

            if (!type.IsValidStructuralType())
            {
                return null;
            }

            _mappingContext.ConventionsConfiguration.ApplyModelConfiguration(type, _mappingContext.ModelConfiguration);

            if (_mappingContext.ModelConfiguration.IsIgnoredType(type)
                || _mappingContext.ModelConfiguration.IsComplexType(type))
            {
                return null;
            }

            var entityType = _mappingContext.Model.GetEntityType(type.Name);

            if (entityType == null)
            {
                entityType = _mappingContext.Model.AddEntityType(type.Name);
                entityType.IsAbstract = type.IsAbstract;

                Contract.Assert(type.BaseType != null);

                var baseType = _mappingContext.Model.GetEntityType(type.BaseType.Name);

                if (baseType == null)
                {
                    _mappingContext.Model.AddEntitySet(entityType.Name, entityType);
                }
                else if (ReferenceEquals(baseType, entityType))
                {
                    // O/C mapping collision
                    return null;
                }

                entityType.BaseType = baseType;

                var entityTypeConfiguration
                    = new Func<EntityTypeConfiguration>(() => _mappingContext.ModelConfiguration.Entity(type));

                _mappingContext.ConventionsConfiguration.ApplyTypeConfiguration(type, entityTypeConfiguration);

                MapStructuralElements(
                    type,
                    entityType.Annotations,
                    (m, p) => m.Map(p, entityType, entityTypeConfiguration, type),
                    entityType.BaseType != null,
                    entityTypeConfiguration);

                if (entityType.BaseType != null)
                {
                    LiftDeclaredProperties(type, entityType);
                }

                MapDerivedTypes(type, entityType);
            }
            else if (type != entityType.GetClrType())
            {
                // O/C mapping collision
                return null;
            }

            return entityType;
        }

        private void MapStructuralElements<TStructuralTypeConfiguration>(
            Type type,
            ICollection<DataModelAnnotation> annotations,
            Action<PropertyMapper, PropertyInfo> propertyMappingAction,
            bool mapDeclaredPropertiesOnly,
            Func<TStructuralTypeConfiguration> structuralTypeConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            Contract.Requires(type != null);
            Contract.Requires(annotations != null);
            Contract.Requires(propertyMappingAction != null);
            Contract.Requires(structuralTypeConfiguration != null);

            annotations.SetClrType(type);

            new AttributeMapper(_mappingContext.AttributeProvider).Map(type, annotations);

            var propertyMapper = new PropertyMapper(this);

            foreach (var propertyInfo in new PropertyFilter(_mappingContext.Model.Version)
                .GetProperties(
                    type,
                    mapDeclaredPropertiesOnly,
                    _mappingContext.ModelConfiguration.GetConfiguredProperties(type),
                    _mappingContext.ModelConfiguration.StructuralTypes))
            {
                _mappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(
                    propertyInfo, _mappingContext.ModelConfiguration);
                _mappingContext.ConventionsConfiguration.ApplyPropertyTypeConfiguration(
                    propertyInfo, structuralTypeConfiguration);

                if (!_mappingContext.ModelConfiguration.IsIgnoredProperty(type, propertyInfo))
                {
                    propertyMappingAction(propertyMapper, propertyInfo);
                }
            }
        }

        private void MapDerivedTypes(Type type, EdmEntityType entityType)
        {
            Contract.Requires(type != null);
            Contract.Requires(entityType != null);

            if (type.IsSealed)
            {
                return;
            }

            if (!_knownTypes.Contains(type))
            {
                _knownTypes.AddRange(type.Assembly.GetTypes().Where(t => t.IsValidStructuralType()));
            }

            foreach (var derivedType in _knownTypes.Where(t => t.BaseType == type).ToList())
            {
                var derivedEntityType = MapEntityType(derivedType);

                if (derivedEntityType != null)
                {
                    derivedEntityType.BaseType = entityType;

                    LiftDerivedType(derivedType, derivedEntityType, entityType);
                }
            }
        }

        private void LiftDerivedType(Type derivedType, EdmEntityType derivedEntityType, EdmEntityType entityType)
        {
            Contract.Requires(derivedType != null);
            Contract.Requires(derivedEntityType != null);
            Contract.Requires(entityType != null);

            _mappingContext.Model.ReplaceEntitySet(derivedEntityType, _mappingContext.Model.GetEntitySet(entityType));

            LiftDeclaredProperties(derivedType, derivedEntityType);
        }

        private void LiftDeclaredProperties(Type type, EdmEntityType entityType)
        {
            Contract.Requires(type != null);
            Contract.Requires(entityType != null);

            var entityTypeConfiguration
                = _mappingContext.ModelConfiguration.GetStructuralTypeConfiguration(type) as EntityTypeConfiguration;

            if (entityTypeConfiguration != null)
            {
                entityTypeConfiguration.ClearKey();

                foreach (
                    var property in
                        type.BaseType.GetProperties(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (property.DeclaringType != type
                        && entityTypeConfiguration.IgnoredProperties.Any(p => p.IsSameAs(property)))
                    {
                        throw Error.CannotIgnoreMappedBaseProperty(property.Name, type, property.DeclaringType);
                    }
                }
            }

            LiftDeclaredProperties(type, entityType.DeclaredKeyProperties, entityTypeConfiguration);
            LiftDeclaredProperties(type, entityType.DeclaredProperties, entityTypeConfiguration);
            LiftDeclaredProperties(type, entityType.DeclaredNavigationProperties, entityTypeConfiguration);
        }

        private void LiftDeclaredProperties<TProperty>(
            Type type, IList<TProperty> properties, EntityTypeConfiguration entityTypeConfiguration)
            where TProperty : EdmStructuralMember
        {
            Contract.Requires(type != null);
            Contract.Requires(properties != null);

            for (var i = properties.Count - 1; i >= 0; i--)
            {
                var property = properties[i];
                var propertyInfo = property.GetClrPropertyInfo();
                var declaringType = propertyInfo.DeclaringType;

                if (declaringType != type)
                {
                    Contract.Assert(declaringType.IsAssignableFrom(type));

                    var navigationProperty = property as EdmNavigationProperty;

                    if (navigationProperty != null)
                    {
                        _mappingContext.Model.RemoveAssociationType(navigationProperty.Association);
                    }

                    properties.RemoveAt(i);

                    if (entityTypeConfiguration != null)
                    {
                        entityTypeConfiguration.RemoveProperty(new PropertyPath(propertyInfo));
                    }
                }
            }
        }
    }
}
