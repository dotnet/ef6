// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class StoreGeneratedIdentityKeyConventionTests
    {
        [Fact]
        public void Apply_should_match_simple_int_key()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            entityType.AddKeyMember(property);

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_match_simple_long_key()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));
            entityType.AddKeyMember(property);

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_match_simple_short_key()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            entityType.AddKeyMember(property);

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_key_that_is_also_an_fk()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));

            entityType.AddKeyMember(property);

            var associationType
                = model.AddAssociationType(
                    "A", new EntityType("E", "N", DataSpace.CSpace), RelationshipMultiplicity.ZeroOrOne,
                    entityType, RelationshipMultiplicity.Many);

            associationType.Constraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[] { property },
                    new[] { property });

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(model, null));

            Assert.Null(entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }

        // Dev11 345384
        [Fact]
        public void Apply_should_match_key_that_is_an_fk_used_in_table_splitting()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));

            entityType.AddKeyMember(property);

            var targetConfig = new EntityTypeConfiguration(typeof(object));
            targetConfig.ToTable("SharedTable");
            entityType.Annotations.SetConfiguration(targetConfig);

            var sourceEntityType = new EntityType("E", "N", DataSpace.CSpace);
            var sourceConfig = new EntityTypeConfiguration(typeof(object));
            sourceConfig.ToTable("SharedTable");
            sourceEntityType.Annotations.SetConfiguration(sourceConfig);

            var associationType
                = model.AddAssociationType(
                    "A", sourceEntityType, RelationshipMultiplicity.One,
                    entityType, RelationshipMultiplicity.One);

            associationType.Constraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[] { property },
                    new[] { property });

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(model, null));

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_simple_key_of_wrong_type()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddKeyMember(property);

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Null(entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_composite_key()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddKeyMember(property);
            entityType.AddKeyMember(EdmProperty.Primitive("K", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.Equal(
                0,
                entityType.KeyProperties
                    .Count(p => p.GetStoreGeneratedPattern() == StoreGeneratedPattern.Identity));
        }

        [Fact]
        public void Apply_should_not_match_required_self_ref()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));

            entityType.AddKeyMember(property);

            var associationType
                = model.AddAssociationType(
                    "A",
                    entityType, RelationshipMultiplicity.One,
                    entityType, RelationshipMultiplicity.Many);

            model.AddAssociationType(associationType);

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(model, null));

            Assert.Null(entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_match_optional_self_ref()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));

            entityType.AddKeyMember(property);

            var associationType
                = model.AddAssociationType(
                    "A",
                    entityType, RelationshipMultiplicity.ZeroOrOne,
                    entityType, RelationshipMultiplicity.Many);

            model.AddAssociationType(associationType);

            (new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new DbModel(model, null));

            Assert.NotNull(entityType.KeyProperties.Single().GetStoreGeneratedPattern());
        }
    }
}
