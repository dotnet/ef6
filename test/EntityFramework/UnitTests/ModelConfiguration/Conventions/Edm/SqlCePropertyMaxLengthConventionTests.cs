// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class SqlCePropertyMaxLengthConventionTests
    {
        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_non_unicode_fixed_length_strings()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            property.IsUnicode = false;
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_string_keys()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Null(property.IsUnicode);
            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Null(property.IsUnicode);
            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_binary_key()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Null(property.IsUnicode);
            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_non_unicode_fixed_length_strings()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            property.IsUnicode = false;
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Equal(4000, property.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new SqlCePropertyMaxLengthConvention())
                .Apply(entityType, CreateEdmModel());

            Assert.Null(property.IsUnicode);
            Assert.Equal(4000, property.MaxLength);
        }

        private static EdmModel CreateEdmModel()
        {
            var model
                = new EdmModel(DataSpace.SSpace)
                      {
                          ProviderInfo = ProviderRegistry.SqlCe4_ProviderInfo
                      };

            return model;
        }
    }
}
