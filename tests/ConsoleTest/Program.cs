using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xugu.EntityFramework.CodeFirst.Tests;
using XuguClient;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //try
            //{
            //    var assembly = Assembly.Load("XuguClient");
            //    Console.WriteLine("Assembly loaded successfully!");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Failed to load assembly: {ex.Message}");
            //}

            //var systemRuntimeAssembly = typeof(System.Runtime.Versioning.FrameworkName).Assembly;
            //Console.WriteLine(systemRuntimeAssembly.FullName);
            //var test= systemRuntimeAssembly.GetName().Version;
            //Console.WriteLine(test.ToString());
            //Console.WriteLine(new System.Runtime.Versioning.FrameworkName("XuguClient", test).FullName);


            string port = Environment.GetEnvironmentVariable("XG_PORT");

            Console.WriteLine(port);



            //string versionString = "XuGu 12.0.0";
            //int i = 0;
            //int s = 0;
            //while (i < versionString.Length/* && (Char.IsDigit(versionString[i]) || versionString[i] == '.')*/)
            //{
            //    if (!Char.IsDigit(versionString[i]) && versionString[i] != '.')
            //    {
            //        s++;
            //    }
            //    i++;
            //}

            //Console.WriteLine(versionString.Substring(s, i-s));

            //using (DefaultContext ctx=new DefaultContext("IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK"))
            //{
            //    Child c = new Child();
            //    c.ChildId = "ConsoleABC"+new Random().Next();
            //    c.Name = "first";
            //    c.BirthTime = DateTime.Now;
            //    c.Label = Guid.NewGuid();
            //    ctx.Children.Add(c);
            //    ctx.SaveChanges();
            //}
            //Console.WriteLine("完成");

            //using (var context = new ContextForNormalFk())
            //{
            //    context.Database.Connection.Open();
            //    context.Database.Initialize(true);
            //    using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
            //    {
            //        //conn.Open();
            //        var cmd = new XGCommand();
            //        var entityName = (context.Permisos.GetType().FullName.Split(',')[0]).Substring(66).ToLowerInvariant();
            //        var contextName = context.GetType().Name.ToLowerInvariant();
            //        cmd.Connection = conn;
            //        cmd.CommandText =
            //            $"SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE TABLE_ID = (SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='{entityName}' LIMIT 1);";
            //        cmd.ExecuteNonQuery();

            //        using (var reader = cmd.ExecuteReader())
            //        {
            //            while (reader.Read())
            //            {
            //                var val = reader.GetValue(0);
            //                Assert.True(val.ToString().Contains("FK_"));
            //            }
            //        }
            //    }
            //}

            //using (var ctx = new DefaultContext())
            //{
            //    ctx.Database.Connection.Open();
            //    using(var trans= ctx.Database.Connection.BeginTransaction())
            //    {
            //        //var test = context.Database.SqlQuery<object>("select * from all_tables");
            //        //var test2=test.ToList();
            //        Child c = new Child();
            //        c.ChildId = "ConsoleABC" + new Random().Next();
            //        c.Name = "first";
            //        c.BirthTime = DateTime.Now;
            //        c.Label = Guid.NewGuid();
            //        ctx.Children.Add(c);
            //        ChildTest a = new ChildTest();
            //        a.ChildTestId = "TestConsoleABC" + new Random().Next();
            //        a.Name = "firstTEST";
            //        a.Label = Guid.NewGuid();
            //        ctx.ChildrenTest.Add(a);
            //        ctx.SaveChanges();

            //        var test1 = ctx.Children.Select(i => i.ChildId).ToList();
            //        foreach (var item in test1)
            //        {
            //            Console.WriteLine(item);
            //        }
            //        var test2 = ctx.ChildrenTest.Select(i => i.ChildTestId).ToList();
            //        foreach (var item in test2)
            //        {
            //            Console.WriteLine(item);
            //        }
            //        ctx.Children.First().Name = "changed";
            //        ctx.ChildrenTest.First().Name = "changedtest";
            //        ctx.SaveChanges();
            //    }
            //}


            Console.ReadLine();
        }
    }
}
