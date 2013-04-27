// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using Xunit;

    [PartialTrustFixture]
    public class PartialTrustMigrationsConfigurationFinderTests : TestBase
    {
        [Fact]
        public void FindMigrationsConfiguration_preserve_stack_trace_on_net45_in_partial_trust()
        {
            var exception =
                Assert.Throws<MigrationsException>(
                    () => new MigrationsConfigurationFinder(
                              new TypeFinder(typeof(ContextWithBadConfig).Assembly))
                              .FindMigrationsConfiguration(typeof(ContextWithBadConfig), null));

            Assert.Equal(Strings.DbMigrationsConfiguration_RootedPath(@"\Test"), exception.Message);
#if !NET40
            Assert.Contains("set_MigrationsDirectory", exception.StackTrace);
#endif
        }

        public class ContextWithBadConfig : DbContext
        {
        }

        public class BadConfig : DbMigrationsConfiguration<ContextWithBadConfig>
        {
            public BadConfig()
            {
                MigrationsDirectory = @"\Test";
            }
        }
    }
}
