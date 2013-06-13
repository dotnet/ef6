// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using Xunit;

    public class MigratorLoggingDecoratorTests : DbTestCase
    {
        [MigrationsTheory]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal("innerMigrator", Assert.Throws<ArgumentNullException>(() => new MigratorLoggingDecorator(null, null)).ParamName);
            Assert.Equal(
                "logger", Assert.Throws<ArgumentNullException>(() => new MigratorLoggingDecorator(new DbMigrator(), null)).ParamName);
        }
    }
}
