using NUnit.Framework;
using System;
using System.Configuration;
using System.Data;
using System.Data.Entity.Migrations;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Xugu.Data.EntityFramework;
using Xugu.EntityFramework.Tests;

namespace Xugu.EntityFramework.Migrations.Tests
{
    public class SetUpMigrationsTests : DefaultFixture
    {

        private Configuration configuration;
        public DbMigrator Migrator;
        public static string ConnectionStringBlogContext { get; set; }

        [OneTimeSetUp]
        public new void OneTimeSetup()
        {
            ConnectionStringBlogContext = "IP=127.0.0.1;DB=BlogContext;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK";//RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Xugu.EntityFramework.Migrations.Tests.Properties.Resources.ConnStringMacOS : Xugu.EntityFramework.Migrations.Tests.Properties.Resources.ConnString;

            configuration = new Configuration();
            DataSet dataSet = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
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
            Migrator = new DbMigrator(configuration);
            //Type type = typeof(DbMigrator);
            //type.GetField("_defaultSchema", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Migrator, "SYSDBA");
        }

        [OneTimeTearDown]
        public new void OneTimeTearDown()
        {
            using (BlogContext context = new BlogContext())
            {
                if (context.Database.Exists())
                {
                    context.Database.Delete();
                }
            }
        }
    }

    internal sealed class Configuration : DbMigrationsConfiguration<BlogContext>
    {
        public Configuration()
        {
            CodeGenerator = new XGMigrationCodeGenerator();
            AutomaticMigrationsEnabled = false;
            SetSqlGenerator("XuguClient", new Xugu.Data.EntityFramework.XGMigrationSqlGenerator());
        }

        protected override void Seed(BlogContext context)
        {
        }
    }

    internal sealed class EF6Configuration : DbMigrationsConfiguration<BlogContext>
    {
        public EF6Configuration()
        {
            CodeGenerator = new XGMigrationCodeGenerator();
            AutomaticMigrationsEnabled = true;
            SetSqlGenerator("XuguClient", new Xugu.Data.EntityFramework.XGMigrationSqlGenerator());
        }

        protected override void Seed(BlogContext context)
        {
        }
    }
}