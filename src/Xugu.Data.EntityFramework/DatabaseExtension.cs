using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XuguClient;

namespace Xugu.Data.EntityFramework
{
    public static class DatabaseExtension
    {
        //public static bool Delete(this Database database,string databaseName)
        //{
        //    using (XGConnection c = new XGConnection())
        //    {
        //        c.ConnectionString = database.Connection.ConnectionString;
        //        c.Open();
        //        XGCommand cmd=new XGCommand($"USE SYSTEM;DROP DATABASE IF EXISTS {databaseName};" ,c);
        //        bool ret=cmd.ExecuteNonQuery() > 0;
        //        c.Close();
        //        return ret;
        //    }
        //}

        //public static bool Exists(this Database database, string databaseName)
        //{
        //    database.Connection.ConnectionString= database.Connection.ConnectionString.Replace("SYSTEM", databaseName);
        //    bool ret=database.Exists();
            
        //    return ret;
        //}
    }
}
