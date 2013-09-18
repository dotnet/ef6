// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    internal sealed class PropertyMapper
    {
        private readonly TypeMapper _typeMapper;

        public PropertyMapper(TypeMapper typeMapper)
        {
            DebugCheck.NotNull(typeMapper);

            _typeMapper = typeMapper;
        }

        public void Map(
            PropertyInfo propertyInfo, ComplexType complexType,
            Func<ComplexTypeConfiguration> complexTypeConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(complexType);

            var property = MapPrimitiveOrComplexOrEnumProperty(
                propertyInfo, complexTypeConfiguration, discoverComplexTypes: true);

            if (property != null)
            {
                complexType.AddMember(property);
            }
        }

        public void Map(
            PropertyInfo propertyInfo, EntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(entityType);

            var property = MapPrimitiveOrComplexOrEnumProperty(propertyInfo, entityTypeConfiguration);

            if (property != null)
            {
                entityType.AddMember(property);
            }
            else
            {
                new NavigationPropertyMapper(_typeMapper).Map(propertyInfo, entityType, entityTypeConfiguration);
            }
        }

        internal bool MapIfNotNavigationProperty(
            PropertyInfo propertyInfo, EntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(entityType);

            var property = MapPrimitiveOrComplexOrEnumProperty(propertyInfo, entityTypeConfiguration);

            if (property != null)
            {
                entityType.AddMember(property);
                return true;
            }

            return false;
        }

        private EdmProperty MapPrimitiveOrComplexOrEnumProperty(
            PropertyInfo propertyInfo, Func<StructuralTypeConfiguration> structuralTypeConfiguration,
            bool discoverComplexTypes = false)
        {
            DebugCheck.NotNull(propertyInfo);

            var property = propertyInfo.AsEdmPrimitiveProperty();

            if (property == null)
            {
                var propertyType = propertyInfo.PropertyType;
                var complexType = _typeMapper.MapComplexType(propertyType, discoverComplexTypes);

                if (complexType != null)
                {
                    property = EdmProperty.CreateComplex(propertyInfo.Name, complexType);
                }
                else
                {
                    var isNullable = propertyType.TryUnwrapNullableType(out propertyType);

                    if (propertyType.IsEnum())
                    {
                        var enumType = _typeMapper.MapEnumType(propertyType);

                        if (enumType != null)
                        {
                            property = EdmProperty.CreateEnum(propertyInfo.Name, enumType);
                            property.Nullable = isNullable;
                        }
                    }
                }
            }

            if (property != null)
            {
                property.SetClrPropertyInfo(propertyInfo);

                new AttributeMapper(_typeMapper.MappingContext.AttributeProvider)
                    .Map(propertyInfo, property.Annotations);

                if (!property.IsComplexType)
                {
                    _typeMapper.MappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(
                        propertyInfo,
                        () => structuralTypeConfiguration().Property(new PropertyPath(propertyInfo)),
                        _typeMapper.MappingContext.ModelConfiguration);
                }
            }

            return property;
        }
    }
}
