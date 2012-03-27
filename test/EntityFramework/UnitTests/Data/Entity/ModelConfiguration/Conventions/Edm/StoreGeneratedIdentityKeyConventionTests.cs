namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class StoreGeneratedIdentityKeyConventionTests
    {
        [Fact]
        public void Apply_should_match_simple_int_key()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().Initialize());

            Assert.Equal(
                DbStoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_match_simple_long_key()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Int64;
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().Initialize());

            Assert.Equal(
                DbStoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_match_simple_short_key()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Int16;
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().Initialize());

            Assert.Equal(
                DbStoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_key_that_is_also_an_fk()
        {
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();

            property.PropertyType.EdmType = EdmPrimitiveType.Int64;
            entityType.DeclaredKeyProperties.Add(property);

            var associationType
                = model.AddAssociationType(
                    "A", new EdmEntityType(), EdmAssociationEndKind.Optional,
                    entityType, EdmAssociationEndKind.Many);
            associationType.Constraint = new EdmAssociationConstraint();
            associationType.Constraint.DependentProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, model);

            Assert.Null(entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        // Dev11 345384
        [Fact]
        public void Apply_should_match_key_that_is_an_fk_used_in_table_splitting()
        {
            var model = new EdmModel().Initialize();
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();

            property.PropertyType.EdmType = EdmPrimitiveType.Int64;
            entityType.DeclaredKeyProperties.Add(property);

            var targetConfig = new EntityTypeConfiguration(typeof(object));
            targetConfig.ToTable("SharedTable");
            entityType.SetConfiguration(targetConfig);

            var sourceEntityType = new EdmEntityType();
            var sourceConfig = new EntityTypeConfiguration(typeof(object));
            sourceConfig.ToTable("SharedTable");
            sourceEntityType.SetConfiguration(sourceConfig);

            var associationType
                = model.AddAssociationType(
                    "A", sourceEntityType, EdmAssociationEndKind.Required,
                    entityType, EdmAssociationEndKind.Required);
            associationType.Constraint = new EdmAssociationConstraint();
            associationType.Constraint.DependentProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, model);

            Assert.Equal(
                DbStoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_simple_key_of_wrong_type()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.String;
            entityType.DeclaredKeyProperties.Add(property);

            ((IEdmConvention<EdmEntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().Initialize());

            Assert.Null(entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_composite_key()
        {
            var entityType = new EdmEntityType();
            var property = new EdmProperty().AsPrimitive();
            property.PropertyType.EdmType = EdmPrimitiveType.Int64;
            entityType.DeclaredKeyProperties.Add(property);
            entityType.DeclaredKeyProperties.Add(new EdmProperty().AsPrimitive());

            ((IEdmConvention<EdmEntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().Initialize());

            Assert.Equal(
                0,
                entityType.DeclaredKeyProperties
                    .Count(p => p.GetStoreGeneratedPattern() == DbStoreGeneratedPattern.Identity));
        }
    }
}
