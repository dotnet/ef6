// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using Xunit;

    public class DbConfigurationEventArgsTests
    {
        [Fact]
        public void DbConfiguration_set_in_constructor_is_returned()
        {
            var configuration = new DbConfiguration();

            Assert.Same(configuration, new DbConfigurationEventArgs(configuration).Configuration);
        }
    }
}
