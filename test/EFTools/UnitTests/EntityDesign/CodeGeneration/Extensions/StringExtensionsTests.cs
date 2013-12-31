// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void ContainsIgnoreCase_ignores_case()
        {
            var source = new[] { "AAA", "BBB" };

            Assert.True(source.ContainsIgnoreCase("aaa"));
            Assert.True(source.ContainsIgnoreCase("Aaa"));
            Assert.True(source.ContainsIgnoreCase("AAA"));
        }

        [Fact]
        public void ContainsIgnoreCase_returns_false_when_not_found()
        {
            Assert.False(new[] { "AAA", "BBB" }.ContainsIgnoreCase("CCC"));
        }
    }
}
