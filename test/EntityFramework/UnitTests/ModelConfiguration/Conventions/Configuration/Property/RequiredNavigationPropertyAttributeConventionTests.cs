// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using Xunit;

    public sealed class RequiredNavigationPropertyAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_make_end_kind_required()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new RequiredNavigationPropertyAttributeConvention()
                .Apply(new MockPropertyInfo(), associationConfiguration, new ModelConfiguration(), new RequiredAttribute());

            Assert.Equal(RelationshipMultiplicity.One, associationConfiguration.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_ignore_when_end_kind_set()
        {
            var associationConfiguration
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne
                      };

            new RequiredNavigationPropertyAttributeConvention()
                .Apply(new MockPropertyInfo(), associationConfiguration, new ModelConfiguration(), new RequiredAttribute());

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationConfiguration.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_ignore_when_end_kind_is_collection()
        {
            var associationConfiguration
                = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(List<string>), "N"))
                      {
                          RelationshipMultiplicity = RelationshipMultiplicity.Many
                      };

            new RequiredNavigationPropertyAttributeConvention()
                .Apply(new MockPropertyInfo(), associationConfiguration, new ModelConfiguration(), new RequiredAttribute());

            Assert.Equal(RelationshipMultiplicity.Many, associationConfiguration.RelationshipMultiplicity);
        }
    }
}
