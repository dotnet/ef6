// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using Xunit;

    public class ProviderInvariantNameTests
    {
        [Fact]
        public void Invariant_name_passed_to_constructor_is_returned()
        {
            Assert.Equal("That's Not My Name", new ProviderInvariantName("That's Not My Name").Name);
        }
    }
}
