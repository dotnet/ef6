// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Resources;
    using Xunit;

    public class MigrationsCheckerTests
    {
        [Fact]
        public void IsMigrationsConfigured_returns_true_if_DbMigrationsConfiguration_is_discovered_and_database_exists()
        {
            Assert.True(new MigrationsChecker().IsMigrationsConfigured(new ContextWithMigrations().InternalContext, () => true));
        }

        [Fact]
        public void IsMigrationsConfigured_returns_false_if_no_DbMigrationsConfiguration_can_be_discovered_and_database_exists()
        {
            Assert.False(new MigrationsChecker().IsMigrationsConfigured(new FakeContext().InternalContext, () => true));
        }

        [Fact]
        public void IsMigrationsConfigured_throws_if_DbMigrationsConfiguration_is_discovered_and_database_does_not_exist()
        {
            Assert.Equal(
                Strings.DatabaseInitializationStrategy_MigrationsEnabled("ContextWithMigrations"),
                Assert.Throws<InvalidOperationException>(
                () => new MigrationsChecker().IsMigrationsConfigured(new ContextWithMigrations().InternalContext, () => false)).Message);
        }

        [Fact]
        public void IsMigrationsConfigured_returns_false_if_no_DbMigrationsConfiguration_can_be_discovered_and_database_does_not_exist()
        {
            Assert.False(new MigrationsChecker().IsMigrationsConfigured(new FakeContext().InternalContext, () => false));
        }

        public class DiscoverableConfiguration : DbMigrationsConfiguration<ContextWithMigrations>
        {
        }

        public class ContextWithMigrations : DbContext
        {
            static ContextWithMigrations()
            {
                Database.SetInitializer<ContextWithMigrations>(null);
            }
        }

        public class FakeContext : DbContext
        {
            static FakeContext()
            {
                Database.SetInitializer<FakeContext>(null);
            }
        }
    }
}
