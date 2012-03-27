namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class IdKeyDiscoveryConventionTests
    {
        [Fact]
        public void Apply_should_match_simple_id()
        {
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("Id");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.True(entityType.DeclaredKeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_make_key_not_nullable()
        {
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("Id");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.False(property.PropertyType.IsNullable.Value);
        }

        [Fact]
        public void Apply_should_match_type_prefixed_id()
        {
            var entityType = new EdmEntityType { Name = "Foo" };
            var property = entityType.AddPrimitiveProperty("FooId");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.True(entityType.DeclaredKeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_match_id_ahead_of_type_and_id()
        {
            var entityType = new EdmEntityType { Name = "Foo" };
            var typeIdProperty = entityType.AddPrimitiveProperty("FooId");
            var idProperty = entityType.AddPrimitiveProperty("Id");
            typeIdProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;
            idProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.Equal(1, entityType.DeclaredKeyProperties.Count);
            Assert.True(entityType.DeclaredKeyProperties.Contains(idProperty));
        }

        [Fact]
        public void Apply_should_ignore_case()
        {
            var entityType = new EdmEntityType { Name = "Foo" };
            var property = entityType.AddPrimitiveProperty("foOid");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.True(entityType.DeclaredKeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_ignore_non_primitive_type()
        {
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("Id");

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.False(entityType.DeclaredKeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_ignore_when_key_already_specified()
        {
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("Id");
            entityType.DeclaredKeyProperties.Add(new EdmProperty());

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.False(entityType.DeclaredKeyProperties.Contains(property));
        }

        [Fact]
        public void Apply_should_ignore_when_type_is_derived()
        {
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("Id");
            entityType.BaseType = new EdmEntityType();

            ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel());

            Assert.False(entityType.DeclaredKeyProperties.Contains(property));
        }

        [Fact] // Dev11 347225
        public void Apply_should_throw_if_two_Id_properties_are_matched_that_differ_only_by_case()
        {
            var entityType = new EdmEntityType { Name = "Foo" };
            var IDProperty = entityType.AddPrimitiveProperty("ID");
            var IdProperty = entityType.AddPrimitiveProperty("Id");
            IDProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;
            IdProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;

            Assert.Equal(Strings.MultiplePropertiesMatchedAsKeys("ID", "Foo"), Assert.Throws<InvalidOperationException>(() => ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel())).Message);
        }

        [Fact] // Dev11 347225
        public void Apply_should_throw_if_two_type_Id_properties_are_matched_that_differ_only_by_case()
        {
            var entityType = new EdmEntityType { Name = "Foo" };
            var FOOIdProperty = entityType.AddPrimitiveProperty("FOOId");
            var FooIdProperty = entityType.AddPrimitiveProperty("FooId");
            FOOIdProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;
            FooIdProperty.PropertyType.EdmType = EdmPrimitiveType.Int32;

            Assert.Equal(Strings.MultiplePropertiesMatchedAsKeys("FOOId", "Foo"), Assert.Throws<InvalidOperationException>(() => ((IEdmConvention<EdmEntityType>)new IdKeyDiscoveryConvention()).Apply(entityType, new EdmModel())).Message);
        }
    }
}
