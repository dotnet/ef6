// Copyright (c) 2021 Oracle and/or its affiliates.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of MySQL hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// MySQL.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of MySQL Connector/NET, is also subject to the
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

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Xugu.Data.EntityFramework;
using System;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    //ContextForString
    class ContextForString : DbContext
    {
        public ContextForString() : base(CodeFirstFixture.GetEFConnectionString<ContextForString>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SYSDBA");
        }
        public DbSet<StringUser> StringUsers { get; set; }
    }
    public class StringUser
    {
        public int StringUserId { get; set; }

        [StringLength(50)]
        public string Name50 { get; set; }

        [StringLength(100)]
        public string Name100 { get; set; }

        [StringLength(200)]
        public string Name200 { get; set; }

        [StringLength(300)]
        public string Name300 { get; set; }
    }

    //ContextForTinyPk
    public class ContextForTinyPk : DbContext
    {
        public ContextForTinyPk() : base(CodeFirstFixture.GetEFConnectionString<ContextForTinyPk>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SYSDBA");
        }
        public DbSet<TinyPkUser> TinyPkUseRs { get; set; }
    }

    public class TinyPkUser
    {
        [Key]
        [DataType("TINYINT")]
        public sbyte StringUserId { get; set; }

        [StringLength(50)]
        public string Name50 { get; set; }

        [StringLength(100)]
        public string Name100 { get; set; }

        [StringLength(200)]
        public string Name200 { get; set; }

        [StringLength(300)]
        public string Name300 { get; set; }
    }

    //ContextForBigIntPk
    public class ContextForBigIntPk : DbContext
    {
        public ContextForBigIntPk() : base(CodeFirstFixture.GetEFConnectionString<ContextForBigIntPk>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SYSDBA");
        }
        public DbSet<BigIntPkUser> BigIntPkUseRs { get; set; }
    }

    public class BigIntPkUser
    {
        [Key]
        [DataType("BIGINT")]
        public long StringUserId { get; set; }

        [StringLength(50)]
        public string Name50 { get; set; }

        [StringLength(100)]
        public string Name100 { get; set; }

        [StringLength(200)]
        public string Name200 { get; set; }

        [StringLength(300)]
        public string Name300 { get; set; }
    }

    //EducationContext
    [Table("passports")]
    public class Passport
    {
        [Key]
        public int Key { get; set; }
    }

    public class EducationContext : DbContext
    {
        public EducationContext() : base(CodeFirstFixture.GetEFConnectionString<EducationContext>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("SYSDBA");
        }
        public DbSet<Passport> Passports { get; set; }
    }
    [Table("TestForeignBigIntPkUser", Schema = "SYSDBA")]
    public class TestForeignBigIntPkUser
    {
        [Key]
        [DataType("BIGINT")]
        public long StringUserId { get; set; }

        [StringLength(50)]
        public string Name50 { get; set; }

        [StringLength(100)]
        public string Name100 { get; set; }

        [StringLength(200)]
        public string Name200 { get; set; }

        [StringLength(300)]
        public string Name300 { get; set; }

        [InverseProperty("TestForeignBigIntPkUser")]
        public virtual ICollection<TestForeignPassport> TestForeignPassports { get; set; }
    }

    public class TestForeignPassport
    {
        [Key]
        public int Key { get; set; }

        [DataType("BIGINT")]
        public long StringUserId { get; set; }

        public virtual TestForeignBigIntPkUser TestForeignBigIntPkUser { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid guid { get; set; }
    }

    public class TestForeignContext : DbContext
    {
        public TestForeignContext() : base(CodeFirstFixture.GetEFConnectionString<TestForeignContext>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
        public DbSet<TestForeignBigIntPkUser> TestForeignBigIntPkUsers { get; set; }
        public DbSet<TestForeignPassport> TestForeignPassports { get; set; }
    }


    public class TestForeignBigIntPkUser2
    {
        [Key]
        [DataType("BIGINT")]
        public long StringUserId { get; set; }

        [InverseProperty("TestForeignBigIntPkUser")]
        public virtual ICollection<TestForeignPassport2> TestForeignPassports { get; set; }
    }

    [Table("TestForeignPassport2", Schema = "SYSDBA")]
    public class TestForeignPassport2
    {
        [Key]
        public int Key { get; set; }

        [DataType("BIGINT")]
        public long StringUserId { get; set; }

        public virtual TestForeignBigIntPkUser2 TestForeignBigIntPkUser { get; set; }
    }

    public class TestForeignContext2 : DbContext
    {
        public TestForeignContext2() : base(CodeFirstFixture.GetEFConnectionString<TestForeignContext2>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
        public DbSet<TestForeignBigIntPkUser2> TestForeignBigIntPkUsers { get; set; }
        public DbSet<TestForeignPassport2> TestForeignPassports { get; set; }
    }

    public class TestForeignBigIntPkUser3
    {
        [Key]
        [DataType("BIGINT")]
        public long StringUserId { get; set; }
        public virtual ICollection<TestForeignPassport3> TestForeignPassports { get; set; }
    }

    [Table("TestForeignPassport3", Schema = "SYSDBA")]
    public class TestForeignPassport3
    {
        [Key]
        public int Key { get; set; }

        [DataType("BIGINT")]
        public long StringUserId { get; set; }

        public virtual TestForeignBigIntPkUser3 TestForeignBigIntPkUser { get; set; }
    }

    public class TestForeignContext3 : DbContext
    {
        public TestForeignContext3() : base(CodeFirstFixture.GetEFConnectionString<TestForeignContext3>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestForeignPassport3>().HasRequired(i=>i.TestForeignBigIntPkUser).WithMany(s=>s.TestForeignPassports).HasForeignKey(i=>i.StringUserId);
        }
        public DbSet<TestForeignBigIntPkUser3> TestForeignBigIntPkUsers { get; set; }
        public DbSet<TestForeignPassport3> TestForeignPassports { get; set; }
    }

    public class TestManyUser
    {
        [Key]
        [DataType("BIGINT")]
        public long StringUserId { get; set; }
        public virtual ICollection<TestManyPassport> TestManyPassports { get; set; }
    }

    [Table("TestManyPassport", Schema = "SYSDBA")]
    public class TestManyPassport
    {
        [Key]
        public int Key { get; set; }

        public virtual ICollection<TestManyUser> TestManyUsers { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid guid { get; set; }
    }

    public class TestManyContext : DbContext
    {
        public TestManyContext() : base(CodeFirstFixture.GetEFConnectionString<TestManyContext>())
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestManyPassport>().HasMany(i => i.TestManyUsers).WithMany(i => i.TestManyPassports).Map(m =>m.ToTable("TestManyUsersTestManyPassports"));
        }
        public DbSet<TestManyUser> TestManyUsers { get; set; }
        public DbSet<TestManyPassport> TestManyPassports { get; set; }
    }

    public enum TestSchoolSubject
    {
        Math,
        History,
        Chemistry
    }

    public class TestSchoolSchedule
    {
        public int Id { get; set; }
        public string TeacherName { get; set; }
        public TestSchoolSubject Subject { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class TestEnumTestSupportContext : DbContext
    {
        

        public TestEnumTestSupportContext() : base(CodeFirstFixture.GetEFConnectionString<TestEnumTestSupportContext>())
        {
            //Database.SetInitializer<EnumTestSupportContext>(new EnumTestSupportInitialize<EnumTestSupportContext>());
            //Database.SetInitializer<EnumTestSupportContext>(new MigrateDatabaseToLatestVersion<EnumTestSupportContext, EnumCtxConfiguration>());
            //Database.SetInitializer<EnumTestSupportContext>(new DropCreateDatabaseAlways<EnumTestSupportContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            //modelBuilder.HasDefaultSchema("SYSDBA");
        }
        public DbSet<TestSchoolSchedule> SchoolSchedules { get; set; }
    }
}
