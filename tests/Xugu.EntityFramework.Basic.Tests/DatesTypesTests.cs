// Copyright (c) 2011, 2023 Oracle and/or its affiliates.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of Xugu hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// Xugu.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of Xugu Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using NUnit.Framework;
using XuguClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;

namespace Xugu.EntityFramework.Tests
{
    public class Widget
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DateCreated { get; set; }

        public DateTime Timestamp { get; set; }

        public DateTime DateTimeWithPrecision { get; set; }
    }

    public class TestContext : DbContext
    {
        public TestContext(string connStr) : base(connStr)
        {
            Database.SetInitializer<TestContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Widget>()
                            .Property(e => e.DateTimeWithPrecision)
                            .HasPrecision(6);
        }

        public DbSet<Widget> Widgets { get; set; }
    }

    public class DatesTypesTests : DefaultFixture
    {
        [Test]
        public void CanCreateDBScriptWithDateTimePrecision()
        {
            //if (!Environment.OSVersion.Platform.ToString().StartsWith("Win")) 
            //  Assert.Ignore("Fix for Ubuntu. schema.Rows[3] -> System.IndexOutOfRangeException: There is no row at position 3.");
            //if (Version < new Version(5, 6, 5)) 
            //  Assert.Ignore("Xugu Server version no compatible");

            //using (var ctx = new TestContext(ConnectionString))
            //{
            //  var script = new XGScript(Connection);
            //  var context = ((IObjectContextAdapter)ctx).ObjectContext;
            //  script.Query = context.CreateDatabaseScript();
            //  script.Execute();

            //  DataTable schema = Connection.GetSchema("COLUMNS", new string[] { null, Connection.Database, "widgets" });

            //  DataRow row = schema.Rows[3];
            //  Assert.AreEqual("datetime", (string)row["DATA_TYPE"]);
            //  Assert.AreEqual("NO", (string)row["IS_NULLABLE"]);
            //  if (Version < new Version(8, 0))
            //    Assert.AreEqual((uint)6, (UInt64)row["DATETIME_PRECISION"]);
            //  else
            //    Assert.AreEqual((uint)6, (UInt32)row["DATETIME_PRECISION"]);
            //}
        }
    }
}
