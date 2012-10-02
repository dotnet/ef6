// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class SqlCePropertyMaxLengthConventionTests
    {
        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
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

            ((IEdmConvention<EdmEntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_string_keys()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            entityType.DeclaredProperties.Add(property);
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_binary_key()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            entityType.DeclaredProperties.Add(property);
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
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

            ((IEdmConvention<EdmComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new EdmComplexType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Binary;
            property.PropertyType.PrimitiveTypeFacets.IsFixedLength = true;
            entityType.DeclaredProperties.Add(property);

            ((IEdmConvention<EdmComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            Assert.Null(primitiveTypeFacets.IsUnicode);
            Assert.Equal(4000, primitiveTypeFacets.MaxLength);
        }

        private static EdmModel CreateEdmModel()
        {
            var model = new EdmModel();

            model.SetProviderInfo(ProviderRegistry.SqlCe4_ProviderInfo);

            return model;
        }
    }
}
