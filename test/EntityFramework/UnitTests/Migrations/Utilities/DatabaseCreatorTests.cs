// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    public class DatabaseCreatorTests : DbTestCase
    {
        public DatabaseCreatorTests(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        [MigrationsTheory]
        public void DatabaseCreator_can_create_delete_and_check_for_existence_of_database()
        {
            DropDatabase();

            using (var connection = ProviderFactory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;

                var databaseCreator = new DatabaseCreator(33);

                Assert.False(databaseCreator.Exists(connection));

                databaseCreator.Create(connection);

                Assert.True(databaseCreator.Exists(connection));

                databaseCreator.Delete(connection);

                Assert.False(databaseCreator.Exists(connection));
            }
        }
    }
}
