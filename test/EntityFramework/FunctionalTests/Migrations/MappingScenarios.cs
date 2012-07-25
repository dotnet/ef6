// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{

    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlServerCe, ProgrammingLanguage.CSharp)]
    [Variant(DatabaseProvider.SqlClient, ProgrammingLanguage.VB)]
    public class MappingScenarios : DbTestCase
    {
        [MigrationsTheory]
        public void Can_migrate_model_with_advanced_mapping_scenarios()
        {
            ResetDatabase();

            var migrator = CreateMigrator<MappingScenariosContext>();

            migrator.Update();
        }
    }
}