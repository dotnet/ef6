// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class PropertyMaxLengthConventionTests
    {
        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(true, property.IsUnicode);
            Assert.Equal(false, property.IsFixedLength);
            Assert.Null(property.MaxLength);
            Assert.Equal(true, property.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(true, property.IsUnicode);
            Assert.Equal(128, property.MaxLength);
            Assert.False(property.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_non_unicode_fixed_length_strings()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            property.IsUnicode = false;
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(128, property.MaxLength);
            Assert.False(property.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_string_keys()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(128, property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary));
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Null(property.IsUnicode);
            Assert.Equal(false, property.IsFixedLength);
            Assert.Null(property.MaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<EntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Null(property.IsUnicode);
            Assert.Equal(128, property.MaxLength);
            Assert.False(property.IsMaxLength);
        }

        [Fact]
        public void Apply_should_set_correct_defaults_for_binary_key()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary));
            entityType.AddMember(property);
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Null(property.IsUnicode);
            Assert.Equal(128, property.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_strings()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(true, property.IsUnicode);
            Assert.Equal(false, property.IsFixedLength);
            Assert.Null(property.MaxLength);
            Assert.Equal(true, property.IsMaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unicode_fixed_length_strings()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(true, property.IsUnicode);
            Assert.Equal(128, property.MaxLength);
            Assert.False(property.IsMaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_non_unicode_fixed_length_strings()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            property.IsFixedLength = true;
            property.IsUnicode = false;
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(128, property.MaxLength);
            Assert.False(property.IsMaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_unconfigured_binary()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary));
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Null(property.IsUnicode);
            Assert.Equal(false, property.IsFixedLength);
            Assert.Null(property.MaxLength);
        }

        [Fact]
        public void ComplexType_apply_should_set_correct_defaults_for_fixed_length_binary()
        {
            var entityType = new ComplexType("C");
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary));
            property.IsFixedLength = true;
            entityType.AddMember(property);

            ((IEdmConvention<ComplexType>)new PropertyMaxLengthConvention())
                .Apply(entityType, new EdmModel(DataSpace.CSpace));

            Assert.Null(property.IsUnicode);
            Assert.Equal(128, property.MaxLength);
            Assert.False(property.IsMaxLength);
        }

        [Fact]
        public void Apply_should_update_foreign_keys()
        {
            var entityType = new EntityType();
            var principalProperty = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            principalProperty.MaxLength = 23;
            entityType.AddMember(principalProperty);
            entityType.AddKeyMember(principalProperty);

            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", entityType);
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());

            var dependentProperty = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            associationType.Constraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[] { principalProperty },
                    new[] { dependentProperty });

            ((IEdmConvention<AssociationType>)new PropertyMaxLengthConvention())
                .Apply(associationType, new EdmModel(DataSpace.CSpace));

            Assert.Equal(23, dependentProperty.MaxLength);
        }
    }
}
