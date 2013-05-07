// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
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
            var propertyInfo = typeof(RequiredAttributeEntity).GetProperty("Navigation");
            var associationConfiguration = new NavigationPropertyConfiguration(propertyInfo);

            new RequiredNavigationPropertyAttributeConvention()
                .ApplyPropertyConfiguration(propertyInfo, () => associationConfiguration, new ModelConfiguration());

            Assert.Equal(RelationshipMultiplicity.One, associationConfiguration.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_ignore_when_end_kind_set()
        {
            var propertyInfo = typeof(RequiredAttributeEntity).GetProperty("Navigation");
            var associationConfiguration
                = new NavigationPropertyConfiguration(propertyInfo)
                    {
                        RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne
                    };

            new RequiredNavigationPropertyAttributeConvention()
                .ApplyPropertyConfiguration(propertyInfo, () => associationConfiguration, new ModelConfiguration());

            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationConfiguration.RelationshipMultiplicity);
        }

        [Fact]
        public void Apply_should_ignore_when_end_kind_is_collection()
        {
            var propertyInfo = typeof(RequiredAttributeEntity).GetProperty("Navigations");
            var associationConfiguration
                = new NavigationPropertyConfiguration(propertyInfo)
                    {
                        RelationshipMultiplicity = RelationshipMultiplicity.Many
                    };

            new RequiredNavigationPropertyAttributeConvention()
                .ApplyPropertyConfiguration(propertyInfo, () => associationConfiguration, new ModelConfiguration());

            Assert.Equal(RelationshipMultiplicity.Many, associationConfiguration.RelationshipMultiplicity);
        }

        public class RequiredAttributeEntity
        {
            [Required]
            public MockType Navigation { get; set; }

            [Required]
            public MockType Navigations { get; set; }
        }
    }
}
