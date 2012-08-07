// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
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
            Assert.True(new ForeignKeyConstraintConfiguration(new[] { new MockPropertyInfo().Object }).DependentProperties.Any());
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
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;
            property.SetClrPropertyInfo(mockPropertyInfo);
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = entityType;

            constraintConfiguration.Configure(
                associationType, associationType.SourceEnd, new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, associationType.Constraint.DependentProperties.Count);
        }

        [Fact]
        public void Configure_should_propagate_end_kind_to_keys_when_required()
        {
            var mockPropertyInfo = new MockPropertyInfo(typeof(int), "P");
            var constraintConfiguration = new ForeignKeyConstraintConfiguration(new[] { mockPropertyInfo.Object });
            var entityType = new EdmEntityType();
            var property = entityType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;
            property.PropertyType.IsNullable = true;
            property.SetClrPropertyInfo(mockPropertyInfo);
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = entityType;
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Required;

            constraintConfiguration.Configure(
                associationType, associationType.SourceEnd, new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(1, associationType.Constraint.DependentProperties.Count);
            Assert.Equal(false, property.PropertyType.IsNullable);
        }

        [Fact]
        public void Configure_should_throw_when_dependent_property_not_found()
        {
            var constraintConfiguration = new ForeignKeyConstraintConfiguration(new[] { new MockPropertyInfo(typeof(int), "P").Object });
            var associationType = new EdmAssociationType();

            Assert.Equal(
                Strings.ForeignKeyPropertyNotFound("P", "T"),
                Assert.Throws<InvalidOperationException>(
                    () => constraintConfiguration.Configure(
                        associationType,
                        new EdmAssociationEnd
                            {
                                EntityType = new EdmEntityType
                                                 {
                                                     Name = "T"
                                                 }
                            }, new EntityTypeConfiguration(typeof(object)))).Message);
        }
    }
}
