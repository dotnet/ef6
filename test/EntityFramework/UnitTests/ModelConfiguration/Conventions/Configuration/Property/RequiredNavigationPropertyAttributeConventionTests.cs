// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using Xunit;

    public sealed class RequiredNavigationPropertyAttributeConventionTests : TestBase
    {
        [Fact]
        public void Apply_should_make_end_kind_required()
        {
            var associationConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());

            new RequiredNavigationPropertyAttributeConvention.RequiredNavigationPropertyAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), associationConfiguration, new RequiredAttribute());

            Assert.Equal(EdmAssociationEndKind.Required, associationConfiguration.EndKind);
        }

        [Fact]
        public void Apply_should_ignore_when_end_kind_set()
        {
            var associationConfiguration
                = new NavigationPropertyConfiguration(new MockPropertyInfo()) { EndKind = EdmAssociationEndKind.Optional };

            new RequiredNavigationPropertyAttributeConvention.RequiredNavigationPropertyAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), associationConfiguration, new RequiredAttribute());

            Assert.Equal(EdmAssociationEndKind.Optional, associationConfiguration.EndKind);
        }

        [Fact]
        public void Apply_should_ignore_when_end_kind_is_collection()
        {
            var associationConfiguration
                = new NavigationPropertyConfiguration(new MockPropertyInfo(typeof(List<string>), "N"))
                    {
                        EndKind = EdmAssociationEndKind.Many
                    };

            new RequiredNavigationPropertyAttributeConvention.RequiredNavigationPropertyAttributeConventionImpl()
                .Apply(new MockPropertyInfo(), associationConfiguration, new RequiredAttribute());

            Assert.Equal(EdmAssociationEndKind.Many, associationConfiguration.EndKind);
        }
    }
}