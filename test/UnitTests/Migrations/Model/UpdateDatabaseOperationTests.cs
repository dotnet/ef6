// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Resources;
    using Xunit;

    public class UpdateDatabaseOperationTests
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                "historyQueryTrees",
                Assert.Throws<ArgumentNullException>(() => new UpdateDatabaseOperation(null)).ParamName);
        }

        [Fact]
        public void AddMigration_should_validate_preconditions()
        {
            var updateDatabaseOperation
                = new UpdateDatabaseOperation(new List<DbQueryCommandTree>());

            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("migrationId")).Message,
                Assert.Throws<ArgumentException>(() => updateDatabaseOperation.AddMigration(null, null)).Message);

            Assert.Equal(
                "operations",
                Assert.Throws<ArgumentNullException>(() => updateDatabaseOperation.AddMigration("M", null)).ParamName);
        }

        [Fact]
        public void Can_add_and_retrieve_migrations()
        {
            var updateDatabaseOperation
                = new UpdateDatabaseOperation(new List<DbQueryCommandTree>());

            updateDatabaseOperation.AddMigration("M1", new List<MigrationOperation>());
            updateDatabaseOperation.AddMigration("M2", new List<MigrationOperation>());

            Assert.Equal(2, updateDatabaseOperation.Migrations.Count);
        }
    }
}
