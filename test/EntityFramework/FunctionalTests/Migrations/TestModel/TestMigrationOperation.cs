using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.Migrations.TestModel
{
    public class TestMigrationOperation
        : MigrationOperation
    {
        public TestMigrationOperation(string testName, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            this.TestName = testName;
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        public string TestName { get; private set; }
    }
}
