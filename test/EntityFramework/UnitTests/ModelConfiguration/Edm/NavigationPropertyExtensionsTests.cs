// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class NavigationPropertyExtensionsTests
    {
        [Fact]
        public void Can_get_and_set_configuration_facet()
        {
            var navigationProperty = new NavigationProperty("N", TypeUsage.Create(new EntityType("E", "N", DataSpace.CSpace)));
            navigationProperty.SetConfiguration(42);

            Assert.Equal(42, navigationProperty.GetConfiguration());
        }
    }
}
