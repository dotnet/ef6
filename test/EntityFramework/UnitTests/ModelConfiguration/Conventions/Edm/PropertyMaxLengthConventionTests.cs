// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class PropertyMaxLengthConventionTests
    {
        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(true, primitiveTypeFacets.IsUnicode);
            Assert.Equal(false, primitiveTypeFacets.IsFixedLength);
            Assert.Null(primitiveTypeFacets.MaxLength);
            Assert.Equal(true, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(true, primitiveTypeFacets.IsUnicode);
            Assert.Equal(128, primitiveTypeFacets.MaxLength);
            Assert.Equal(null, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_non_unicode_fixed_length_strings()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsUnicode = false;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(128, primitiveTypeFacets.MaxLength);
            Assert.Equal(null, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_string_keys()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            entityType.DeclaredProperties.Add(property);
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(128, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(false, primitiveTypeFacets.IsFixedLength);
            Assert.Null(primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(128, primitiveTypeFacets.MaxLength);
            Assert.Equal(null, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_binary_key()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            entityType.DeclaredProperties.Add(property);
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(128, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(true, primitiveTypeFacets.IsUnicode);
            Assert.Equal(false, primitiveTypeFacets.IsFixedLength);
            Assert.Null(primitiveTypeFacets.MaxLength);
            Assert.Equal(true, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(true, primitiveTypeFacets.IsUnicode);
            Assert.Equal(128, primitiveTypeFacets.MaxLength);
            Assert.Equal(null, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_non_unicode_fixed_length_strings()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            property.PropertyType.PrimitiveTypeFacets.IsUnicode = false;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(128, primitiveTypeFacets.MaxLength);
            Assert.Equal(null, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(false, primitiveTypeFacets.IsFixedLength);
            Assert.Null(primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(128, primitiveTypeFacets.MaxLength);
            Assert.Equal(null, primitiveTypeFacets.IsMaxLength);
        }

        [Fact]
        public void Apply_should_update_foreign_keys()
        {
            var associationType = new EdmAssociationType().Initialize();
            var entityType = new EdmEntityType();
            var principalProperty = new EdmProperty().AsPrimitive();
            principalProperty.PropertyType.PrimitiveTypeFacets.MaxLength = 23;
            entityType.DeclaredProperties.Add(principalProperty);
            entityType.DeclaredKeyProperties.Add(principalProperty);
            associationType.TargetEnd.EntityType = entityType;
            associationType.Constraint = new EdmAssociationConstraint
                {
                    DependentEnd = associationType.SourceEnd
                };
            var dependentProperty = new EdmProperty().AsPrimitive();
            dependentProperty.PropertyType.EdmType = EdmPrimitiveType.String;
            associationType.Constraint.DependentProperties.Add(dependentProperty);

            ((IEdmConvention<EdmAssociationType>)new PropertyMaxLengthConvention())
                .Apply(associationType, new EdmModel());

            Assert.Equal(23, dependentProperty.PropertyType.PrimitiveTypeFacets.MaxLength);
        }
    }
}