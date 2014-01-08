// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.TestHelpers;
    using System.Globalization;
    using System.Linq;
    using SimpleModel;
    using Xunit;

    public class DbConfigurationTests : FunctionalTestBase
    {
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

            Assert.Equal(
                new[] { "Hook1()", "Hook1(2013, 'December 31')", "Hook2()", "Hook2('January 1', 2014)", "Hook2()", "Hook1(4102, '1 yraunaJ')", "Hook1()" },
                TestLoadedInterceptor.HooksRun.ToArray().Reverse());
        }

        public class TestLoadedInterceptor2 : IDbConfigurationInterceptor
        {
            private readonly string _tag;

            public TestLoadedInterceptor2()
            {
                _tag = "Hook2()";
            }

            public TestLoadedInterceptor2(string p1, int p2)
            {
                _tag = string.Format(CultureInfo.InvariantCulture, "Hook2('{0}', {1})", p1, p2);
            }

            public void Loaded(
                DbConfigurationLoadedEventArgs loadedEventArgs, 
                DbConfigurationInterceptionContext interceptionContext)
            {
                TestLoadedInterceptor.HooksRun.Push(_tag);
            }
        }
    }
}
