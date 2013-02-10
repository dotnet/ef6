using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.TestModel;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Data.Entity.Migrations.Sql
{
    public class CustomSqlGeneratorSupportTests
    {
        [Fact]
        public void Calling_Generate_Also_Calls_Inherited_Methods()
        {
            var customGenerator = new CustomSqlGenerator();
            var customOperation = new TestMigrationOperation("Calling_Generate_Also_Calls_Inherited_Methods");

            var sql = customGenerator.Generate(new[] { customOperation }, "2008").Join(s => s.Sql, Environment.NewLine);

            Assert.Equal("Migration Operation: Calling_Generate_Also_Calls_Inherited_Methods called", sql);
        }

        [Fact]
        public void Calling_Custom_Generate_Preserves_Order()
        {
            var customGenerator = new CustomSqlGenerator();

            var sqlGenerator = new SqlOperation("SELECT * FROM");
            var customOperation = new TestMigrationOperation("Calling_Custom_Generate_Preserves_Order");
            var sqlGenerator2 = new SqlOperation("DELETE * FROM");

            var staments = customGenerator.Generate(new MigrationOperation[] { sqlGenerator, customOperation, sqlGenerator2 }, "2008");

            var expectedStaments = new[] 
                                    { 
                                        "SELECT * FROM", 
                                        "Migration Operation: Calling_Custom_Generate_Preserves_Order called", 
                                        "DELETE * FROM" 
                                    };

            Assert.Equal(expectedStaments, staments.Select(s => s.Sql));
        }
    }
}
