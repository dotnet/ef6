// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class DependentNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Has_foreign_key_with_single_property_should_create_constraint_with_dependent_key()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new DependentNavigationPropertyConfiguration<D>(navigationPropertyConfiguration)
                .HasForeignKey(d => d.Fk1);

            var foreignKeyConstraint = (ForeignKeyConstraintConfiguration)navigationPropertyConfiguration.Constraint;

            Assert.Equal("Fk1", foreignKeyConstraint.DependentProperties.Single().Name);
        }

        [Fact]
        public void Has_foreign_key_with_multiple_properties_should_create_constraint_with_dependent_keys()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new DependentNavigationPropertyConfiguration<D>(navigationPropertyConfiguration)
                .HasForeignKey(d => new { d.Fk1, d.Fk2 });

            var foreignKeyConstraint = (ForeignKeyConstraintConfiguration)navigationPropertyConfiguration.Constraint;

            Assert.Equal(2, foreignKeyConstraint.DependentProperties.Count());
            Assert.Equal("Fk1", foreignKeyConstraint.DependentProperties.First().Name);
            Assert.Equal("Fk2", foreignKeyConstraint.DependentProperties.ElementAt(1).Name);
        }

        [Fact]
        public void Has_foreign_key_should_throw_when_invalid_key_expression()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            Assert.Equal(Strings.InvalidPropertiesExpression("d => d.ToString()"), Assert.Throws<InvalidOperationException>(() => new DependentNavigationPropertyConfiguration<D>(navigationPropertyConfiguration)
                                                                                                                                            .HasForeignKey(d => d.ToString())).Message);
        }

        #region Test Fixtures

        private class D
        {
            public int? Fk1 { get; set; }
            public int? Fk2 { get; set; }
        }

        #endregion
    }
}