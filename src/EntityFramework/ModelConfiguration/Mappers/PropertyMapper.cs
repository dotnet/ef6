namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    internal sealed class PropertyMapper
    {
        private readonly TypeMapper _typeMapper;

        public PropertyMapper(TypeMapper typeMapper)
        {
            Contract.Requires(typeMapper != null);

            _typeMapper = typeMapper;
        }

        public void Map(
            PropertyInfo propertyInfo, EdmComplexType complexType,
            Func<ComplexTypeConfiguration> complexTypeConfiguration)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(complexType != null);

            var property = MapPrimitiveOrComplexOrEnumProperty(
                propertyInfo, complexTypeConfiguration, discoverComplexTypes: true);

            if (property != null)
            {
                complexType.DeclaredProperties.Add(property);
            }
        }

        public void Map(
            PropertyInfo propertyInfo, EdmEntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(entityType != null);

            var property = MapPrimitiveOrComplexOrEnumProperty(propertyInfo, entityTypeConfiguration);

            if (property != null)
            {
                entityType.DeclaredProperties.Add(property);
            }
            else
            {
                new NavigationPropertyMapper(_typeMapper).Map(propertyInfo, entityType, entityTypeConfiguration);
            }
        }

        private EdmProperty MapPrimitiveOrComplexOrEnumProperty(
            PropertyInfo propertyInfo, Func<StructuralTypeConfiguration> structuralTypeConfiguration,
            bool discoverComplexTypes = false)
        {
            Contract.Requires(propertyInfo != null);

            var property = propertyInfo.AsEdmPrimitiveProperty();

            if (property == null)
            {
                var propertyType = propertyInfo.PropertyType;
                var complexType = _typeMapper.MapComplexType(propertyType, discoverComplexTypes);

                if (complexType != null)
                {
                    property = new EdmProperty
                                   {
                                       Name = propertyInfo.Name
                                   }.AsComplex(complexType);
                }
                else
                {
                    var isNullable = propertyType.TryUnwrapNullableType(out propertyType);

                    if (propertyType.IsEnum)
                    {
                        var enumType = _typeMapper.MapEnumType(propertyType);

                        if (enumType != null)
                        {
                            property = new EdmProperty
                                           {
                                               Name = propertyInfo.Name,
                                           }.AsEnum(enumType);
                            property.PropertyType.IsNullable = isNullable;
                        }
                    }
                }
            }

            if (property != null)
            {
                property.SetClrPropertyInfo(propertyInfo);

                new AttributeMapper(_typeMapper.MappingContext.AttributeProvider)
                    .Map(propertyInfo, property.Annotations);

                if (!property.PropertyType.IsComplexType)
                {
                    _typeMapper.MappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(
                        propertyInfo, () => structuralTypeConfiguration().Property(new PropertyPath(propertyInfo)));
                }
            }

            return property;
        }
    }
}
