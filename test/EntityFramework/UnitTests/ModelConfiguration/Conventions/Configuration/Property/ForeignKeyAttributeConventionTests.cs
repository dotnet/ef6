// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Resources;
    using Xunit;

    public sealed class ForeignKeyAttributeConventionTests
    {
        [Fact]
        public void Apply_ignores_constraint_when_already_specified()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property<int>("AId1").Property<int>("AId2");
            mockTypeA.Property(mockTypeB, "B");
            var mockPropertyInfo = mockTypeA.GetProperty("B");
            var modelConfiguration = new ModelConfiguration();
            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeA).Navigation(mockPropertyInfo);
            navigationPropertyConfiguration.Constraint
                = new ForeignKeyConstraintConfiguration(new[] { mockTypeB.GetProperty("AId1") });

            new ForeignKeyPrimitivePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new ForeignKeyAttribute("AId2"));

            var foreignKeyConstraint = (ForeignKeyConstraintConfiguration)navigationPropertyConfiguration.Constraint;

            Assert.Equal(new[] { mockTypeB.GetProperty("AId1") }, foreignKeyConstraint.DependentProperties);
        }

        [Fact]
        public void Apply_ignores_constraint_when_inverse_constraint_already_specified()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property(mockTypeA, "A").Property<int>("AId1").Property<int>("AId2");
            mockTypeA.Property(mockTypeB, "B");

            var mockPropertyInfo = mockTypeA.GetProperty("B");
            var mockInversePropertyInfo = mockTypeB.GetProperty("A");

            var modelConfiguration = new ModelConfiguration();
            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeA).Navigation(mockPropertyInfo);

            var inverseNavigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeB).Navigation(mockInversePropertyInfo);

            navigationPropertyConfiguration.InverseNavigationProperty = mockInversePropertyInfo;

            inverseNavigationPropertyConfiguration.Constraint
                = new ForeignKeyConstraintConfiguration(new[] { mockTypeB.GetProperty("AId1") });

            new ForeignKeyPrimitivePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new ForeignKeyAttribute("AId2"));

            Assert.Null(navigationPropertyConfiguration.Constraint);
        }

        [Fact]
        public void Apply_adds_fk_column_when_nav_prop_is_valid()
        {
            var mockTypeA = new MockType("A");
            var mockTypeB = new MockType("B").Property<int>("AId").Property(mockTypeA, "A");
            var mockPropertyInfo = mockTypeB.GetProperty("AId");
            var mockNavigationPropertyInfo = mockTypeB.GetProperty("A");

            var modelConfiguration = new ModelConfiguration();

            new ForeignKeyPrimitivePropertyAttributeConvention()
                .Apply(mockPropertyInfo, modelConfiguration, new ForeignKeyAttribute("A"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(mockTypeB).Navigation(mockNavigationPropertyInfo);

            Assert.NotNull(navigationPropertyConfiguration.Constraint);

            var foreignKeyConstraint = (ForeignKeyConstraintConfiguration)navigationPropertyConfiguration.Constraint;

            Assert.Equal(new[] { mockTypeB.GetProperty("AId") }, foreignKeyConstraint.DependentProperties);
            Assert.Null(navigationPropertyConfiguration.InverseNavigationProperty);
        }

        [Fact]
        public void Apply_throws_when_cannot_find_navigation_property()
        {
            var mockTypeA = new MockType("A");
            mockTypeA.Property(typeof(int), "BId");
            var mockPropertyInfo = mockTypeA.GetProperty("BId");

            Assert.Equal(
                Strings.ForeignKeyAttributeConvention_InvalidNavigationProperty("BId", mockTypeA.Object, "Missing"),
                Assert.Throws<InvalidOperationException>(
                    () => new ForeignKeyPrimitivePropertyAttributeConvention()
                              .Apply(mockPropertyInfo, new ModelConfiguration(), new ForeignKeyAttribute("Missing"))).Message);
        }
    }
}
