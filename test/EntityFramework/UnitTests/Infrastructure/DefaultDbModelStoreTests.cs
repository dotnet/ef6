// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using Xunit;
    using System.Data.Entity.Resources;

    public class DefaultDbModelStoreTests : TestBase
    {
        [Fact]
        public void Constructor_location_cannot_be_null()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("location"),
                Assert.Throws<ArgumentException>(() => new DefaultDbModelStore(null)).Message);
        }

        [Fact]
        public void Constructor_location_cannot_be_empty()
        {
            
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("location"),
                Assert.Throws<ArgumentException>(() => new DefaultDbModelStore(string.Empty)).Message);
        }

        [Fact]
        public void Constructor_sets_Location()
        {
            var location = "zz:\\filelocation";
            var modelStore = new DefaultDbModelStore(location);
            Assert.Equal(location, modelStore.Location);
        }
    }
}
