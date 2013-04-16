// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Linq;
    using Xunit;

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class AddPrimaryKeyScenarios : DbTestCase
    {
        private class AddPrimaryKeyMigration : DbMigration
        {
            public override void Up()
            {
                CreateTable(
                    "T", t => new
                                  {
                                      Id = t.Int(nullable: false)
                                  });

                AddPrimaryKey("T", "Id", name: "my_pk", clustered: false);
            }
        }

        [MigrationsTheory]
        public void Can_add_non_clustered_primary_key()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v2>(new AddPrimaryKeyMigration());

            migrator.Update();

            Assert.NotNull(Info.TableConstraints.SingleOrDefault(c => c.Name == "my_pk"));
        }
    }
}
