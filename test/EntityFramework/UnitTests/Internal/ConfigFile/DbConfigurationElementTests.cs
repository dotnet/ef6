// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using Xunit;

    public class DbConfigurationElementTests : TestBase
    {
        [Fact]
        public void ConfigurationTypeName_can_be_accessed()
        {
            var providerElement = new DbConfigurationElement
                {
                    ConfigurationTypeName = "Play.Them.Drums"
                };

            Assert.Equal("Play.Them.Drums", providerElement.ConfigurationTypeName);
        }
    }
}
