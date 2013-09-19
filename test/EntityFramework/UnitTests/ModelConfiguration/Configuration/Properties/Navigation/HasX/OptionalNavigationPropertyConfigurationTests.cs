// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class OptionalNavigationPropertyConfigurationTests
    {
        [Fact]
        public void Ctor_should_set_source_end_kind_to_optional()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration);

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationConfiguration.RelationshipMultiplicity);
        }

        [Fact]
        public void With_many_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithMany(t => t.Ss);

            Assert.Equal("Ss", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_many_should_set_target_end_kind_to_many()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithMany();

            Assert.Equal(RelationshipMultiplicity.Many, associationConfiguration.InverseEndKind);
        }

        [Fact]
        public void With_required_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithRequired(t => t.S);

            Assert.Equal("S", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_required_should_set_target_end_kind_to_required()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithRequired();

            Assert.Equal(RelationshipMultiplicity.One, associationConfiguration.InverseEndKind);
        }

        [Fact]
        public void With_optional_dependent_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalDependent(t => t.S);

            Assert.Equal("S", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_optional_principal_should_set_inverse_when_specified()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalPrincipal(t => t.S);

            Assert.Equal("S", associationConfiguration.InverseNavigationProperty.Name);
        }

        [Fact]
        public void With_optional_dependent_should_set_target_end_kind_to_optional()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalDependent();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationConfiguration.InverseEndKind);
        }

        [Fact]
        public void With_optional_principal_should_set_target_end_kind_to_optional()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            new OptionalNavigationPropertyConfiguration<S, T>(associationConfiguration).WithOptionalPrincipal();

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationConfiguration.InverseEndKind);
        }

        private class S
        {
        }

        private class T
        {
            public ICollection<S> Ss { get; set; }
            public S S { get; set; }
        }
    }
}
