using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.Migrations
{
    public interface IAddMigrationOperation
    {
        void AddOperation(MigrationOperation operation);
    }
}
