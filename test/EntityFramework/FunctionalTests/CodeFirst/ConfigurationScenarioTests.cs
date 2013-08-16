// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Data.Entity.Infrastructure;
    using Xunit;

    public class ConfigurationScenarioTests : TestBase
    {
        [Fact]
        public void Can_call_Entity_after_adding_custom_configuration_class_during_OnModelCreating()
        {
            Database.SetInitializer<FunctionalTests.ConfigurationScenarioTests.BasicTypeContext>(null);
            using (var ctx = new FunctionalTests.ConfigurationScenarioTests.BasicTypeContext())
            {
                Assert.NotNull(((IObjectContextAdapter)ctx).ObjectContext);
            }
        }
    }
}
