// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class IdKeyDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_match_simple_id()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.True(entityType.KeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_make_key_not_nullable()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.False(property.Nullable);
        }

        [Fact]
        public void Apply_should_match_type_prefixed_id()
        {
            var entityType = new EntityType("Foo", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.True(entityType.KeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_match_id_ahead_of_type_and_id()
        {
            var entityType = new EntityType("Foo", "N", DataSpace.CSpace);
            var property = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);
            var property1 = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var idProperty = property1;

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal(1, entityType.KeyProperties.Count);
            Assert.True(entityType.KeyProperties.Contains(idProperty));
        }

        [Fact]
        public void Apply_should_ignore_case()
        {
            var entityType = new EntityType("Foo", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("foOid", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.True(entityType.KeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_ignore_non_primitive_type()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreateComplex("Id", new ComplexType("C"));

            entityType.AddMember(property1);
            var property = property1;

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.False(entityType.KeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_ignore_when_key_already_specified()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;
            entityType.AddKeyMember(EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.False(entityType.KeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_ignore_when_type_is_derived()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;
            entityType.BaseType = new EntityType("E", "N", DataSpace.CSpace);

            (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.False(entityType.KeyProperties.Contains(property));
        }

        [Fact] // Dev11 347225
        public void Apply_should_throw_if_two_Id_properties_are_matched_that_differ_only_by_case()
        {
            var entityType = new EntityType("Foo", "N", DataSpace.CSpace);
            var property = EdmProperty.CreatePrimitive("ID", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);
            var IDProperty = property;
            var property1 = EdmProperty.CreatePrimitive("Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var IdProperty = property1;

            Assert.Equal(
                Strings.MultiplePropertiesMatchedAsKeys("ID", "Foo"),
                Assert.Throws<InvalidOperationException>(
                    () => (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null))).Message);
        }

        [Fact]
        public void Apply_should_throw_if_two_type_Id_properties_are_matched_that_differ_only_by_case()
        {
            var entityType = new EntityType("Foo", "N", DataSpace.CSpace);
            var property = EdmProperty.CreatePrimitive("FOOId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property);
            var FOOIdProperty = property;
            var property1 = EdmProperty.CreatePrimitive("FooId", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var FooIdProperty = property1;

            Assert.Equal(
                Strings.MultiplePropertiesMatchedAsKeys("FOOId", "Foo"),
                Assert.Throws<InvalidOperationException>(
                    () => (new IdKeyDiscoveryConvention()).Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null))).Message);
        }
    }
}
