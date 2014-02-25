// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{

    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Xunit;

    public class ConventionsTypesFinderTests
    {
        [Fact]
        public void AddConvention_add_only_convention_types()
        {
            var conventionsTypesFinder = new ConventionsTypeFinder();

            var types = new Type[]
            {
                typeof(String),
                typeof(Array),
                typeof(RegularConvention),
                typeof(IList<String>)
            };

            conventionsTypesFinder.AddConventions(types, (convention) =>
            {
                Assert.IsType<RegularConvention>(convention);
            });
        }

        class RegularConvention
            :Convention
        {
        }
    }
}
