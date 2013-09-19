// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class ForeignKeyConstraintConfigurationTests
    {
        [Fact]
        public void Can_initialize_and_enumerate_dependent_keys()
        {
            Assert.True(new ForeignKeyConstraintConfiguration(new[] { new MockPropertyInfo().Object }).ToProperties.Any());
        }

        [Fact]
        public void Equals_should_compare_references_or_dependent_key_sequences()
        {
            var propertyInfo = new MockPropertyInfo().Object;
            var constraintConfiguration1 = new ForeignKeyConstraintConfiguration(new[] { propertyInfo });

            Assert.True(constraintConfiguration1.Equals(constraintConfiguration1));

            var constraintConfiguration2 = new ForeignKeyConstraintConfiguration(new[] { propertyInfo });

            Assert.True(constraintConfiguration1.Equals(constraintConfiguration2));
        }

        [Fact]
        public void Configure_should_add_properties_to_dependent_properties()
        {
            var mockPropertyInfo = new MockPropertyInfo(typeof(int), "P");
            var constraintConfiguration = new ForeignKeyConstraintConfiguration(new[] { mockPropertyInfo.Object });
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;
            property.SetClrPropertyInfo(mockPropertyInfo);
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", entityType);
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            constraintConfiguration.Configure(
                associationType, associationType.SourceEnd, new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, associationType.Constraint.ToProperties.Count);
        }

        [Fact]
        public void Configure_should_propagate_end_kind_to_keys_when_required()
        {
            var mockPropertyInfo = new MockPropertyInfo(typeof(int), "P");
            var constraintConfiguration = new ForeignKeyConstraintConfiguration(new[] { mockPropertyInfo.Object });
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var property1 = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);
            var property = property1;
            property.Nullable = true;
            property.SetClrPropertyInfo(mockPropertyInfo);
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", entityType);
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.One;

            constraintConfiguration.Configure(
                associationType, associationType.SourceEnd, new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, associationType.Constraint.ToProperties.Count);
            Assert.Equal(false, property.Nullable);
        }

        [Fact]
        public void Configure_should_throw_when_dependent_property_not_found()
        {
            var constraintConfiguration
                = new ForeignKeyConstraintConfiguration(
                    new[]
                        {
                            new MockPropertyInfo(typeof(int), "P").Object
                        });
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);

            Assert.Equal(
                Strings.ForeignKeyPropertyNotFound("P", "T"),
                Assert.Throws<InvalidOperationException>(
                    () => constraintConfiguration.Configure(
                        associationType,
                        new AssociationEndMember(
                              "E", new EntityType("T", "N", DataSpace.CSpace))
                              , new EntityTypeConfiguration(typeof(object)))).Message);
        }
    }
}
