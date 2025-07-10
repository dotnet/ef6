using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XuguClient;

namespace System.Data.Entity
{
    public class DropCreateDatabase<TContext> : DropCreateDatabaseAlways<TContext> where TContext : DbContext
    {
        public DropCreateDatabase(): base()
        {
        }
        //static DropCreateDatabase()
        //{
        //    DbConfigurationManager.Instance.EnsureLoadedForContext(typeof(TContext));
        //}

        public new void InitializeDatabase(TContext context)
        {
            string dbName= context.Database.Connection.Database;
            if (!dbName.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
            {
                context.Database.Connection.ConnectionString = context.Database.Connection.ConnectionString.Replace(dbName, "SYSDBA");
                ((XGConnection)context.Database.Connection).ChangeDatabase("SYSDBA", false);
            }
            
            base.InitializeDatabase(context);
            if (!dbName.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase))
            {
                context.Database.Connection.ConnectionString = context.Database.Connection.ConnectionString.Replace("SYSDBA", dbName);
                ((XGConnection)context.Database.Connection).ChangeDatabase(dbName, false);
            }
            
        }
    }
}
