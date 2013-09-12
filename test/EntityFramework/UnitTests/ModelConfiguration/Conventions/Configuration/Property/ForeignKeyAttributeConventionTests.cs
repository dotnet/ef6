// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using Xunit;

    public sealed class ForeignKeyAttributeConventionTests
    {
        [Fact]
        public void Apply_ignores_constraint_when_already_specified()
        {
            var propertyInfo = typeof(BType3).GetInstanceProperty("A");
            var modelConfiguration = new ModelConfiguration();
            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(BType3)).Navigation(propertyInfo);
            navigationPropertyConfiguration.Constraint
                = new ForeignKeyConstraintConfiguration(new[] { typeof(BType3).GetInstanceProperty("AId1") });

            new ForeignKeyPrimitivePropertyAttributeConvention()
                .Apply(
                    typeof(BType3).GetInstanceProperty("AId2"),
                    new ConventionTypeConfiguration(typeof(BType3), () => modelConfiguration.Entity(typeof(BType3)), modelConfiguration),
                    new ForeignKeyAttribute("A"));

            var foreignKeyConstraint = (ForeignKeyConstraintConfiguration)navigationPropertyConfiguration.Constraint;

            Assert.Equal(new[] { typeof(BType3).GetInstanceProperty("AId1") }, foreignKeyConstraint.ToProperties);
        }

        public class AType3
        {
            public int BId { get; set; }
        }

        public class BType3
        {
            public int AId1 { get; set; }
            public int AId2 { get; set; }
            public AType3 A { get; set; }
        }

        [Fact]
        public void Apply_ignores_constraint_when_inverse_constraint_already_specified()
        {
            var mockPropertyInfo = typeof(AType4).GetInstanceProperty("B");
            var mockInversePropertyInfo = typeof(BType4).GetInstanceProperty("A");

            var modelConfiguration = new ModelConfiguration();
            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(AType4)).Navigation(mockPropertyInfo);

            var inverseNavigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(BType4)).Navigation(mockInversePropertyInfo);

            navigationPropertyConfiguration.InverseNavigationProperty = mockInversePropertyInfo;

            inverseNavigationPropertyConfiguration.Constraint
                = new ForeignKeyConstraintConfiguration(new[] { typeof(BType4).GetInstanceProperty("AId1") });

            new ForeignKeyPrimitivePropertyAttributeConvention()
                .Apply(
                    typeof(BType4).GetInstanceProperty("AId1"),
                    new ConventionTypeConfiguration(typeof(BType4), () => modelConfiguration.Entity(typeof(BType4)), modelConfiguration),
                    new ForeignKeyAttribute("A"));

            Assert.Null(navigationPropertyConfiguration.Constraint);
        }

        public class AType4
        {
            public int BId { get; set; }
            public BType4 B { get; set; }
        }

        public class BType4
        {
            public int AId1 { get; set; }
            public int AId2 { get; set; }
            public AType4 A { get; set; }
        }

        [Fact]
        public void Apply_adds_fk_column_when_nav_prop_is_valid()
        {
            var propertyInfo = typeof(BType2).GetInstanceProperty("AId");
            var navigationPropertyInfo = typeof(BType2).GetInstanceProperty("A");

            var modelConfiguration = new ModelConfiguration();

            new ForeignKeyPrimitivePropertyAttributeConvention()
                .Apply(
                    propertyInfo,
                    new ConventionTypeConfiguration(typeof(BType2), () => modelConfiguration.Entity(typeof(BType2)), modelConfiguration),
                    new ForeignKeyAttribute("A"));

            var navigationPropertyConfiguration
                = modelConfiguration.Entity(typeof(BType2)).Navigation(navigationPropertyInfo);

            Assert.NotNull(navigationPropertyConfiguration.Constraint);

            var foreignKeyConstraint = (ForeignKeyConstraintConfiguration)navigationPropertyConfiguration.Constraint;

            Assert.Equal(new[] { typeof(BType2).GetInstanceProperty("AId") }, foreignKeyConstraint.ToProperties);
            Assert.Null(navigationPropertyConfiguration.InverseNavigationProperty);
        }

        public class AType2
        {
            public int BId { get; set; }
        }

        public class BType2
        {
            public int AId { get; set; }
            public AType2 A { get; set; }
        }

        [Fact]
        public void Apply_throws_when_cannot_find_navigation_property()
        {
            var modelConfiguration = new ModelConfiguration();

            Assert.Equal(
                Strings.ForeignKeyAttributeConvention_InvalidNavigationProperty("BId", typeof(AType1), "Missing"),
                Assert.Throws<InvalidOperationException>(
                    () => new ForeignKeyPrimitivePropertyAttributeConvention()
                              .Apply(
                                  typeof(AType1).GetInstanceProperty("BId"),
                                  new ConventionTypeConfiguration(
                              typeof(AType1), () => modelConfiguration.Entity(typeof(AType1)), modelConfiguration),
                                  new ForeignKeyAttribute("Missing"))).Message);
        }

        public class AType1
        {
            public int BId { get; set; }
        }
    }
}
