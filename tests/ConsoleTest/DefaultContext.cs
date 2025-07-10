// Copyright (c) 2014, 2022, Oracle and/or its affiliates.
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using Xugu.Data.EntityFramework;
using XuguClient;

namespace ConsoleTest
{
    [Table("Children", Schema = "SYSDBA")]
    public class Child
    {
        public string ChildId { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        public Guid Label { get; set; }
        public DateTime BirthTime
        {
            get;
            set;
        }
        //public string teststr { get; set; }
        [InverseProperty("Child")]
        public virtual ICollection<ChildTest> ChildrenTest { get; set; }
    }
    
    public class ChildTest
    {
        public string ChildTestId { get; set; }
        public string Name { get; set; }
        public Guid Label { get; set; }
        public string teststrchangename { get; set; }
        public string ChildId { get; set; }
        public virtual Child Child { get; set; }
    }

    [Table("OneChildren", Schema = "SYSDBA")]
    public class OneChild
    {
        public string OneChildId { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        public Guid Label { get; set; }
        public DateTime BirthTime
        {
            get;
            set;
        }
        
        public string ChildTestId { get; set; }

        public virtual OneChildTest OneChildTest { get; set; }
    }

    public class OneChildTest
    {
        public string OneChildTestId { get; set; }
        public string Name { get; set; }
        public Guid Label { get; set; }
        public string teststrchangename { get; set; }
        public string OneChildId { get; set; }

        public virtual OneChild OneChild { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TestGuid { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class DefaultContext : DbContext
    {
        public DefaultContext() : base(GetEFConnectionString<DefaultContext>("TESTDDD"))
        {
        }
        public DefaultContext(string connStr) : base(connStr)
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("testschema");
            //modelBuilder.Entity<OneChild>().HasOptional(o => o.OneChildTest).WithOptionalPrincipal(o => o.OneChild);
            //modelBuilder.Entity<OneChild>().HasRequired(o => o.OneChildTest).WithRequiredPrincipal(o => o.OneChild);
            modelBuilder.Entity<OneChildTest>().HasRequired(o => o.OneChild).WithRequiredPrincipal(o => o.OneChildTest);
            //modelBuilder.Entity<OneChildTest>().MapToStoredProcedures(
            //  sp => sp.Insert(i => i.HasName("insert_OneChildTest", "SYSDBA").Parameter(p => p.Name, "movie_name"))
            //        .Update(u => u.HasName("update_OneChildTest", "SYSDBA").Parameter(p => p.Name, "movie_name"))
            //        .Delete(d => d.HasName("delete_OneChildTest", "SYSDBA"))
            //  );
        }
        public DbSet<Child> Children { get; set; }
        public DbSet<ChildTest> ChildrenTest { get; set; }
        public DbSet<OneChild> OneChildren { get; set; }
        public DbSet<OneChildTest> OneChildrenTest { get; set; }
        public static string GetEFConnectionString<T>(string database = null) where T : DbContext
        {
            XGConnectionStringBuilder sb = new XGConnectionStringBuilder();
            string port = Environment.GetEnvironmentVariable("XG_PORT");
            //sb.ConnectionString = $"IP=127.0.0.1;DB={database ?? typeof(T).Name};User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK";
            sb.ConnectionString = $"IP=127.0.0.1;DB={database ?? typeof(T).Name};User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK";
            return sb.ToString();
        }
    }
}