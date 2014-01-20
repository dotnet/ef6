// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DatabaseLoggerTests : TestBase
    {
        [Fact]
        public void Constructors_validate_arguments()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("path"),
                Assert.Throws<ArgumentException>(() => new DatabaseLogger(null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("path"),
                Assert.Throws<ArgumentException>(() => new DatabaseLogger(null, append: true)).Message);
        }
    }
}
