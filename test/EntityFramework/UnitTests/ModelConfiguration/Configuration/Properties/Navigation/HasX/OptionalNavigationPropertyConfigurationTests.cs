// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using Xunit;

    public sealed class OptionalNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Ctor_should_set_source_end_kind_to_optional()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration);

            Assert.Equal(EdmAssociationEndKind.Optional, associationConfiguration.EndKind);
        }

        [Fact]
        public void With_many_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithMany(t => t.Ss);

            Assert.Equal("Ss", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_many_should_set_target_end_kind_to_many()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithMany();

            Assert.Equal(EdmAssociationEndKind.Many, associationConfiguration.InverseEndKind);
        }

        [Fact]
        public void With_required_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithRequired(t => t.S);

            Assert.Equal("S", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_required_should_set_target_end_kind_to_required()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithRequired();

            Assert.Equal(EdmAssociationEndKind.Required, associationConfiguration.InverseEndKind);
        }

        [Fact]
        public void With_optional_dependent_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalDependent(t => t.S);

            Assert.Equal("S", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_optional_principal_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalPrincipal(t => t.S);

            Assert.Equal("S", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_optional_dependent_should_set_target_end_kind_to_optional()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalDependent();

            Assert.Equal(EdmAssociationEndKind.Optional, associationConfiguration.InverseEndKind);
        }

        [Fact]
        public void With_optional_principal_should_set_target_end_kind_to_optional()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalPrincipal();

            Assert.Equal(EdmAssociationEndKind.Optional, associationConfiguration.InverseEndKind);
        }

        #region Test Fixtures

        private class S
        {
        }

        private class T
        {
            public ICollection<S> Ss { get; set; }
            public S S { get; set; }
        }

        #endregion
    }
}