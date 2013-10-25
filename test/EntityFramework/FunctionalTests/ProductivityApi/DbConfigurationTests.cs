// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.TestHelpers;
    using SimpleModel;
    using Xunit;

    public class DbConfigurationTests : FunctionalTestBase
    {
        public static readonly List<string> HooksRun = new List<string>();

        [Fact]
        public void DefaultConnectionFactory_set_in_config_file_can_be_overriden_before_config_is_locked()
        {
            Assert.IsType<SqlConnectionFactory>(DbConfiguration.DependencyResolver.GetService<IDbConnectionFactory>());
            Assert.IsType<DefaultFunctionalTestsConnectionFactory>(FunctionalTestsConfiguration.OriginalConnectionFactories[0]);
        }

        [Fact]
        public void Initialization_hooks_are_run_once_on_EF_initialization_in_the_app_domain()
        {
            // Make sure that EF has been used if this happens to be the first test run
            using (var context = new SimpleModelContext())
            {
                context.Database.Initialize(force: false);
            }

            Assert.Equal(new[] { "Hook1", "Hook2", "Hook2" }, HooksRun);
        }

        public class InitializationHooks
        {
            private static void Hook2(object sender, DbConfigurationLoadedEventArgs eventArgs)
            {
                HooksRun.Add("Hook2");
            }
        }
    }

    public class InitializationHooks
    {
        private static void Hook1(object sender, DbConfigurationLoadedEventArgs eventArgs)
        {
            DbConfigurationTests.HooksRun.Add("Hook1");
        }
    }
}
