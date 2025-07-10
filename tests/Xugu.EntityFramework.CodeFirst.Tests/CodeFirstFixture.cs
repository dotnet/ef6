// Copyright (c) 2013, 2021, Oracle and/or its affiliates.
//
// Xugu Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// Xugu Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.Xugu.com/about/legal/licensing/foss-exception.html>.
using System.Data;
using System.Configuration;
using System.Reflection;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity;
using XuguClient;
using Xugu.Data.EntityFramework;
using Xugu.EntityFramework.Tests;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    public class CodeFirstFixture : DefaultFixture
    {
        // A trace listener to use during testing.
        private AssertFailTraceListener asertFailListener = new AssertFailTraceListener();

        public CodeFirstFixture()
        {
            // Initilizes Xugu EF configuration
            XGEFConfiguration.SetConfiguration(new XGEFConfiguration());
        }

        [OneTimeSetUp]
        public new void OneTimeSetup()
        {

            // Replace existing listeners with listener for testing.
            Trace.Listeners.Clear();
            Trace.Listeners.Add(this.asertFailListener);

            DataSet dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet != null)
            {
                DataView vi = dataSet.Tables[0].DefaultView;
                vi.Sort = "Name";
                int idx = -1;
                if (((idx = vi.Find("Xugu")) != -1) || ((idx = vi.Find("Xugu Data Provider")) != -1))
                {
                    DataRow row = vi[idx].Row;
                    dataSet.Tables[0].Rows.Remove(row);
                }
                dataSet.Tables[0].Rows.Add("Xugu"
                  , "XuguClient"
                  //, "XuguClient"
                  ,
                  typeof(XGConnectionFactory).AssemblyQualifiedName);
            }

            XGCommand cmd = new XGCommand("SELECT COUNT(SCHEMA_NAME) FROM ALL_SCHEMAS WHERE SCHEMA_NAME = 'sakila'", Connection);

            if (Convert.ToInt32(cmd.ExecuteScalar() ?? 0) == 0)
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                //using (var stream = executingAssembly.GetManifestResourceStream("Xugu.EntityFramework.CodeFirst.Tests.Properties.sakila-schema.sql"))
                //{
                //    using (StreamReader sr = new StreamReader(stream))
                //    {
                //        string sql = sr.ReadToEnd();
                //        XGScript s = new XGScript(Connection, sql);
                //        s.Execute();
                //    }
                //}


                //using (var stream = executingAssembly.GetManifestResourceStream("Xugu.EntityFramework.CodeFirst.Tests.Properties.sakila-data.sql"))
                //{
                //    using (StreamReader sr = new StreamReader(stream))
                //    {
                //        string sql = sr.ReadToEnd();
                //        XGScript s = new XGScript(Connection, sql);
                //        s.Execute();
                //    }
                //}
            }
        }

        [OneTimeTearDown]
        public new void OneTimeTearDown()
        {
            DeleteContext<AutoIncrementBugContext>();
            DeleteContext<MovieDBContext>();
            DeleteContext<SakilaDb>();
            DeleteContext<DinosauriaDBContext>();
            DeleteContext<MovieCodedBasedConfigDBContext>();
            DeleteContext<EnumTestSupportContext>();
            DeleteContext<JourneyContext>();
            DeleteContext<EntityAndComplexTypeContext>();
            DeleteContext<PromotionsDB>();
            DeleteContext<ShipContext>();
            DeleteContext<SiteDbContext>();
            DeleteContext<VehicleDbContext>();
            DeleteContext<VehicleDbContext2>();
            DeleteContext<VehicleDbContext3>();
            DeleteContext<ProductsDbContext>();
            DeleteContext<ShortDbContext>();
            DeleteContext<UsingUnionContext>();
            DeleteContext<BlogContext>();
            DeleteContext<ContextForString>();
            DeleteContext<ContextForNormalFk>();
            DeleteContext<ContextForLongFk>();
            DeleteContext<ContextForTinyPk>();
            DeleteContext<ContextForBigIntPk>();
        }

        public static string GetEFConnectionString<T>(string database = null) where T : DbContext
        {
            XGConnectionStringBuilder sb = new XGConnectionStringBuilder();
            string port = Environment.GetEnvironmentVariable("XG_PORT");
            sb.ConnectionString = $"IP=127.0.0.1;DB={database ?? typeof(T).Name};User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK";

            return sb.ToString();
        }

        private void DeleteContext<T>() where T : DbContext, new()
        {
            using (var context = new T())
            {
                context.Database.Delete();
            }
        }

        private EntityConnection GetEntityConnection()
        {
            return null;
        }

        internal protected static new void CheckSql(string sql, string refSql)
        {
            StringBuilder str1 = new StringBuilder();
            StringBuilder str2 = new StringBuilder();
            foreach (char c in sql)
                if (!Char.IsWhiteSpace(c))
                    str1.Append(c);
            foreach (char c in refSql)
                if (!Char.IsWhiteSpace(c))
                    str2.Append(c);
            Assert.AreEqual(0, String.Compare(str1.ToString(), str2.ToString(), true));
        }

        private class AssertFailTraceListener : DefaultTraceListener
        {
            public override void Fail(string message)
            {
                Assert.True(message == String.Empty, "Failure: " + message);
            }

            public override void Fail(string message, string detailMessage)
            {
                Assert.True(message == String.Empty, "Failure: " + message);
            }
        }
    }
}