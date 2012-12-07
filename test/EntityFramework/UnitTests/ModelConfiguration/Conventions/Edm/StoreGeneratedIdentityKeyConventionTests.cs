// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Linq;
    using Xunit;

    public sealed class StoreGeneratedIdentityKeyConventionTests
    {
        [Fact]
        public void Apply_should_match_simple_int_key()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().InitializeConceptual());

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_match_simple_long_key()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().InitializeConceptual());

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_match_simple_short_key()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().InitializeConceptual());

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_key_that_is_also_an_fk()
        {
            var model = new EdmModel().InitializeConceptual();
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));

            entityType.AddKeyMember(property);

            var associationType
                = model.AddAssociationType(
                    "A", new EntityType(), RelationshipMultiplicity.ZeroOrOne,
                    entityType, RelationshipMultiplicity.Many);

            associationType.Constraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[] { property },
                    new[] { property });

            ((IEdmConvention<EntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, model);

            Assert.Null(entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        // Dev11 345384
        [Fact]
        public void Apply_should_match_key_that_is_an_fk_used_in_table_splitting()
        {
            var model = new EdmModel().InitializeConceptual();
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int64));

            entityType.AddKeyMember(property);

            var targetConfig = new EntityTypeConfiguration(typeof(object));
            targetConfig.ToTable("SharedTable");
            entityType.Annotations.SetConfiguration(targetConfig);

            var sourceEntityType = new EntityType();
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

            ((IEdmConvention<EntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, model);

            Assert.Equal(
                StoreGeneratedPattern.Identity,
                entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_simple_key_of_wrong_type()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            entityType.AddKeyMember(property);

            ((IEdmConvention<EntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().InitializeConceptual());

            Assert.Null(entityType.DeclaredKeyProperties.Single().GetStoreGeneratedPattern());
        }

        [Fact]
        public void Apply_should_not_match_composite_key()
        {
            var entityType = new EntityType();
            var property = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddKeyMember(property);
            entityType.AddKeyMember(EdmProperty.Primitive("K", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            ((IEdmConvention<EntityType>)new StoreGeneratedIdentityKeyConvention())
                .Apply(entityType, new EdmModel().InitializeConceptual());

            Assert.Equal(
                0,
                entityType.DeclaredKeyProperties
                    .Count(p => p.GetStoreGeneratedPattern() == StoreGeneratedPattern.Identity));
        }
    }
}
