// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Annotations
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class CompatibilityResultTests
    {
        [Fact]
        public void Constructor_validates_arguments()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("errorMessage"),
                Assert.Throws<ArgumentException>(() => new CompatibilityResult(false, null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("errorMessage"),
                Assert.Throws<ArgumentException>(() => new CompatibilityResult(false, " ")).Message);
        }

        [Fact]
        public void Properties_return_expected_values()
        {
            Assert.True(new CompatibilityResult(true, null).IsCompatible);
            Assert.Null(new CompatibilityResult(true, null).ErrorMessage);
            Assert.False(new CompatibilityResult(false, "Splat!").IsCompatible);
            Assert.Equal("Splat!", new CompatibilityResult(false, "Splat!").ErrorMessage);
        }

        [Fact]
        public void Can_implicitly_convert_to_bool()
        {
            Assert.True(new CompatibilityResult(true, null));
            Assert.False(new CompatibilityResult(false, "Splat!"));
        }
    }
}
