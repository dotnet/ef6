using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.Migrations.TestModel
{
    public class CustomSqlGenerator
        : SqlServerMigrationSqlGenerator
    {

        protected override void Generate(MigrationOperation operation)
        {
            if (operation is TestMigrationOperation)
            {
                var testOperation = (TestMigrationOperation)operation;

                string statementFormat = "Migration Operation: {0} called";

                this.Statement(String.Format(statementFormat, testOperation.TestName));
            }
        }
    }
}
