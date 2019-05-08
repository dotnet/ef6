// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class CreateStoredProcedureScenarios : DbTestCase
    {
        public CreateStoredProcedureScenarios(DatabaseProviderFixture databaseProviderFixture)
            : base(databaseProviderFixture)
        {
        }

        private class NonEdmElementsProcedureMigration : DbMigration
        {
            public override void Up()
            {
                CreateStoredProcedure(
                    "TestA",
                    p => new
                             {
                                 id = p.Int(defaultValue: 23),
                                 name = p.Decimal(defaultValueSql: "123.4"),
                                 out_param = p.String(outParameter: true, name: "_out_param")
                             },
                    body: "RETURN");
            }
        }

        [MigrationsTheory]
        public void Can_create_procedure_with_non_edm_elements()
        {
            ResetDatabase();

            var migrator = CreateMigrator<ShopContext_v1>();

            migrator.Update();

            migrator = CreateMigrator<ShopContext_v1>(new NonEdmElementsProcedureMigration());

            migrator.Update();
        }
    }
}
