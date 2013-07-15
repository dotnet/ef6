// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    internal sealed class TypeMapper
    {
        private readonly MappingContext _mappingContext;
        private readonly List<Type> _knownTypes = new List<Type>();

        public TypeMapper(MappingContext mappingContext)
        {
            DebugCheck.NotNull(mappingContext);

            _mappingContext = mappingContext;

            _knownTypes.AddRange(
                mappingContext.ModelConfiguration
                    .ConfiguredTypes
                    .Select(t => t.Assembly)
                    .Distinct()
                    .SelectMany(a => a.GetAccessibleTypes().Where(type => type.IsValidStructuralType())));
        }

        public MappingContext MappingContext
        {
            get { return _mappingContext; }
        }

        public EnumType MapEnumType(Type type)
        {
            DebugCheck.NotNull(type);
            Debug.Assert(type.IsEnum);

            var enumType = GetExistingEdmType<EnumType>(_mappingContext.Model, type);

            if (enumType == null)
            {
                PrimitiveType primitiveType;
                if (!Enum.GetUnderlyingType(type).IsPrimitiveType(out primitiveType))
                {
                    return null;
                }

                enumType = _mappingContext.Model.AddEnumType(type.Name, _mappingContext.ModelConfiguration.ModelNamespace);
                enumType.IsFlags = type.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
                enumType.SetClrType(type);

                enumType.UnderlyingType = primitiveType;

                foreach (var name in Enum.GetNames(type))
                {
                    enumType.AddMember(
                        new EnumMember(
                            name,
                            Convert.ChangeType(Enum.Parse(type, name), type.GetEnumUnderlyingType(), CultureInfo.InvariantCulture)));
                }
            }

            return enumType;
        }

        public ComplexType MapComplexType(Type type, bool discoverNested = false)
        {
            DebugCheck.NotNull(type);

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

            var complexType = GetExistingEdmType<ComplexType>(_mappingContext.Model, type);

            if (complexType == null)
            {
                complexType = _mappingContext.Model.AddComplexType(type.Name, _mappingContext.ModelConfiguration.ModelNamespace);

                var complexTypeConfiguration
                    = new Func<ComplexTypeConfiguration>(() => _mappingContext.ModelConfiguration.ComplexType(type));

                _mappingContext.ConventionsConfiguration.ApplyTypeConfiguration(
                    type, complexTypeConfiguration, _mappingContext.ModelConfiguration);

                MapStructuralElements(
                    type,
                    complexType.Annotations,
                    (m, p) => m.Map(p, complexType, complexTypeConfiguration),
                    false,
                    complexTypeConfiguration);
            }

            return complexType;
        }

        public EntityType MapEntityType(Type type)
        {
            DebugCheck.NotNull(type);

            if (!type.IsValidStructuralType()
                || _mappingContext.ModelConfiguration.IsIgnoredType(type)
                || _mappingContext.ModelConfiguration.IsComplexType(type))
            {
                return null;
            }

            var entityType = GetExistingEdmType<EntityType>(_mappingContext.Model, type);

            if (entityType == null)
            {
                _mappingContext.ConventionsConfiguration.ApplyModelConfiguration(type, _mappingContext.ModelConfiguration);

                if (_mappingContext.ModelConfiguration.IsIgnoredType(type)
                    || _mappingContext.ModelConfiguration.IsComplexType(type))
                {
                    return null;
                }

                entityType = _mappingContext.Model.AddEntityType(type.Name, _mappingContext.ModelConfiguration.ModelNamespace);
                entityType.Abstract = type.IsAbstract;

                Debug.Assert(type.BaseType != null);

                var baseType = _mappingContext.Model.GetEntityType(type.BaseType.Name);

                if (baseType == null)
                {
                    _mappingContext.Model.AddEntitySet(entityType.Name, entityType);
                }
                else if (ReferenceEquals(baseType, entityType))
                {
                    throw new NotSupportedException(Strings.SimpleNameCollision(type.FullName, type.BaseType.FullName, type.Name));
                }

                entityType.BaseType = baseType;

                var entityTypeConfiguration
                    = new Func<EntityTypeConfiguration>(() => _mappingContext.ModelConfiguration.Entity(type));

                _mappingContext.ConventionsConfiguration.ApplyTypeConfiguration(
                    type, entityTypeConfiguration, _mappingContext.ModelConfiguration);

                MapStructuralElements(
                    type,
                    entityType.Annotations,
                    (m, p) => m.Map(p, entityType, entityTypeConfiguration),
                    entityType.BaseType != null,
                    entityTypeConfiguration);

                // If the base type was discovered through a navigation property
                // then the inherited properties mapped afterwards need to be lifted
                if (entityType.BaseType != null)
                {
                    LiftInheritedProperties(type, entityType);
                }

                MapDerivedTypes(type, entityType);
            }

            return entityType;
        }

        private static T GetExistingEdmType<T>(EdmModel model, Type type) where T : EdmType
        {
            var edmType = model.GetStructuralOrEnumType(type.Name);
            if (edmType != null
                && type != edmType.GetClrType())
            {
                throw new NotSupportedException(Strings.SimpleNameCollision(type.FullName, edmType.GetClrType().FullName, type.Name));
            }
            return edmType as T;
        }

        private void MapStructuralElements<TStructuralTypeConfiguration>(
            Type type,
            ICollection<MetadataProperty> annotations,
            Action<PropertyMapper, PropertyInfo> propertyMappingAction,
            bool mapDeclaredPropertiesOnly,
            Func<TStructuralTypeConfiguration> structuralTypeConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(annotations);
            DebugCheck.NotNull(propertyMappingAction);
            DebugCheck.NotNull(structuralTypeConfiguration);

            annotations.SetClrType(type);

            new AttributeMapper(_mappingContext.AttributeProvider).Map(type, annotations);

            var propertyMapper = new PropertyMapper(this);

            foreach (var propertyInfo in new PropertyFilter(_mappingContext.ModelBuilderVersion)
                .GetProperties(
                    type,
                    mapDeclaredPropertiesOnly,
                    _mappingContext.ModelConfiguration.GetConfiguredProperties(type),
                    _mappingContext.ModelConfiguration.StructuralTypes))
            {
                _mappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(
                    propertyInfo, _mappingContext.ModelConfiguration);
                _mappingContext.ConventionsConfiguration.ApplyPropertyTypeConfiguration(
                    propertyInfo, structuralTypeConfiguration, _mappingContext.ModelConfiguration);

                if (!_mappingContext.ModelConfiguration.IsIgnoredProperty(type, propertyInfo))
                {
                    propertyMappingAction(propertyMapper, propertyInfo);
                }
            }
        }

        private void MapDerivedTypes(Type type, EntityType entityType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(entityType);

            if (type.IsSealed)
            {
                return;
            }

            if (!_knownTypes.Contains(type))
            {
                _knownTypes.AddRange(type.Assembly.GetAccessibleTypes().Where(t => t.IsValidStructuralType()));
            }

            foreach (var derivedType in _knownTypes.Where(t => t.BaseType == type).OrderBy(t => t.FullName).ToList())
            {
                var derivedEntityType = MapEntityType(derivedType);

                if (derivedEntityType != null)
                {
                    derivedEntityType.BaseType = entityType;

                    LiftDerivedType(derivedType, derivedEntityType, entityType);
                }
            }
        }

        private void LiftDerivedType(Type derivedType, EntityType derivedEntityType, EntityType entityType)
        {
            DebugCheck.NotNull(derivedType);
            DebugCheck.NotNull(derivedEntityType);
            DebugCheck.NotNull(entityType);

            _mappingContext.Model.ReplaceEntitySet(derivedEntityType, _mappingContext.Model.GetEntitySet(entityType));

            LiftInheritedProperties(derivedType, derivedEntityType);
        }

        private void LiftInheritedProperties(Type type, EntityType entityType)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(entityType);

            var entityTypeConfiguration
                = _mappingContext.ModelConfiguration.GetStructuralTypeConfiguration(type) as EntityTypeConfiguration;

            if (entityTypeConfiguration != null)
            {
                entityTypeConfiguration.ClearKey();

                foreach (var property in type.BaseType.GetProperties(PropertyFilter.DefaultBindingFlags))
                {
                    if (!_mappingContext.AttributeProvider.GetAttributes(property).OfType<NotMappedAttribute>().Any()
                        && entityTypeConfiguration.IgnoredProperties.Any(p => p.IsSameAs(property)))
                    {
                        throw Error.CannotIgnoreMappedBaseProperty(property.Name, type, property.DeclaringType);
                    }
                }
            }

            LiftInheritedProperties(type, entityType, entityTypeConfiguration);
        }

        private void LiftInheritedProperties(
            Type type, EntityType entityType, EntityTypeConfiguration entityTypeConfiguration)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(entityType);

            var members = entityType.DeclaredMembers.ToList();

            var declaredProperties
                = new PropertyFilter(_mappingContext.ModelBuilderVersion)
                    .GetProperties(
                        type,
                        /*declaredOnly:*/ true,
                        _mappingContext.ModelConfiguration.GetConfiguredProperties(type),
                        _mappingContext.ModelConfiguration.StructuralTypes);

            foreach (var member in members)
            {
                var propertyInfo = member.GetClrPropertyInfo();

                if (!declaredProperties.Contains(propertyInfo))
                {
                    var navigationProperty = member as NavigationProperty;

                    if (navigationProperty != null)
                    {
                        _mappingContext.Model.RemoveAssociationType(navigationProperty.Association);
                    }

                    entityType.RemoveMember(member);

                    if (entityTypeConfiguration != null)
                    {
                        entityTypeConfiguration.RemoveProperty(new PropertyPath(propertyInfo));
                    }
                }
            }
        }
    }
}
