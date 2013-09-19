// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.TestHelpers;
    using Xunit;

    public class DbConfigurationTests : FunctionalTestBase
    {
        [Fact]
        public void DefaultConnectionFactory_set_in_config_file_can_be_overriden_before_config_is_locked()
        {
            Assert.IsType<SqlConnectionFactory>(DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>());
            Assert.IsType<DefaultFunctionalTestsConnectionFactory>(FunctionalTestsConfiguration.OriginalConnectionFactories[0]);
        }
    }
}
