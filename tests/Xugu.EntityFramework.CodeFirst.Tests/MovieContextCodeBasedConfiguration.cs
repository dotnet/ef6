// Copyright (c) 2013, 2019, Oracle and/or its affiliates. All rights reserved.
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


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using XuguClient;
using System.Data.Common;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.History;
using System.Data.Entity.Spatial;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    public class MovieCBC
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class MovieCodedBasedConfigDBContext : DbContext
    {
        public DbSet<MovieCBC> Movies { get; set; }

        public MovieCodedBasedConfigDBContext(DbConnection existingConnection, bool contextOwnsConnection)
          : base(existingConnection, contextOwnsConnection)
        {
            Database.SetInitializer<MovieCodedBasedConfigDBContext>(new MovieCBCDBInitialize<MovieCodedBasedConfigDBContext>());
            Database.SetInitializer<MovieCodedBasedConfigDBContext>(new MigrateDatabaseToLatestVersion<MovieCodedBasedConfigDBContext, Configuration<MovieCodedBasedConfigDBContext>>());
        }
        public MovieCodedBasedConfigDBContext() : base(CodeFirstFixture.GetEFConnectionString<MovieCodedBasedConfigDBContext>())
        {
            Database.SetInitializer<MovieCodedBasedConfigDBContext>(new MovieCBCDBInitialize<MovieCodedBasedConfigDBContext>());
            Database.SetInitializer<MovieCodedBasedConfigDBContext>(new MigrateDatabaseToLatestVersion<MovieCodedBasedConfigDBContext, Configuration<MovieCodedBasedConfigDBContext>>());
            //Database.SetInitializer<MovieCodedBasedConfigDBContext>(new DropCreateDatabaseAlways<MovieCodedBasedConfigDBContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.HasDefaultSchema("SYSDBA");
            modelBuilder.Entity<MovieCBC>().ToTable("MovieCBC","SYSDBA");
            //modelBuilder.Entity<MovieCBC>().Property(x => x.Price).HasPrecision(16, 2);
            modelBuilder.Conventions.Add<MyCustomConventions>();
            //modelBuilder.Entity<MovieCBC>().MapToStoredProcedures(
            //  sp => sp.Insert(i => i.HasName("insert_movie","SYSDBA").Parameter(p => p.Title, "movie_name"))
            //        .Update(u => u.HasName("update_movie","SYSDBA").Parameter(p => p.Title, "movie_name"))
            //        .Delete(d => d.HasName("delete_movie","SYSDBA"))
            //  );
        }
    }

    public class MovieCBCDBInitialize<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        public void InitializeDatabase(TContext context)
        {
            context.Database.Delete();
            context.Database.CreateIfNotExists();
            this.Seed(context);
            context.SaveChanges();
        }

        protected virtual void Seed(TContext context)
        {
        }
    }
    public class MyHistoryContext : XGHistoryContext
    {
        public MyHistoryContext(DbConnection existingConnection, string defaultSchema)
          : base(existingConnection, defaultSchema)
        {
            //Database.SetInitializer<MyHistoryContext>(new DropCreateDatabaseAlways<MyHistoryContext>());
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.Entity<HistoryRow>().ToTable("__XGMigrations");
            modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasColumnName("_MigrationId");
            modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasColumnName("_ContextKey");
            modelBuilder.Entity<HistoryRow>().Property(h => h.Model).HasColumnName("_Model");
            modelBuilder.Entity<HistoryRow>().Property(h => h.ProductVersion).HasColumnName("_ProductVersion");
        }
    }

    public class Configuration<TContext> : DbMigrationsConfiguration<TContext> where TContext : DbContext
    {
        public Configuration()
        {
            CodeGenerator = new XGMigrationCodeGenerator();
            AutomaticMigrationsEnabled = true;
            SetSqlGenerator("XuguClient", new XGMigrationSqlGenerator());
            SetHistoryContextFactory("XuguClient", (existingConnection, defaultSchema) => new MyHistoryContext(existingConnection, "SYSDBA"));
        }
    }

    #region EnumSupport
    public enum SchoolSubject
    {
        Math,
        History,
        Chemistry
    }

    public class SchoolSchedule
    {
        public int Id { get; set; }
        public string TeacherName { get; set; }
        public SchoolSubject Subject { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class EnumTestSupportContext : DbContext
    {
        public DbSet<SchoolSchedule> SchoolSchedules { get; set; }

        public EnumTestSupportContext() : base(CodeFirstFixture.GetEFConnectionString<EnumTestSupportContext>())
        {
            Database.SetInitializer<EnumTestSupportContext>(new EnumTestSupportInitialize<EnumTestSupportContext>());
            Database.SetInitializer<EnumTestSupportContext>(new MigrateDatabaseToLatestVersion<EnumTestSupportContext, EnumCtxConfiguration>());
            //Database.SetInitializer<EnumTestSupportContext>(new DropCreateDatabaseAlways<EnumTestSupportContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            //modelBuilder.HasDefaultSchema("SYSDBA");
            modelBuilder.Entity<SchoolSchedule>().ToTable("SchoolSchedules","SYSDBA");
            //modelBuilder.HasDefaultSchema("dbo");
        }
    }

    public class EnumTestSupportInitialize<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        public void InitializeDatabase(TContext context)
        {
            context.Database.Delete();
            context.Database.CreateIfNotExists();
            this.Seed(context);
            context.SaveChanges();
        }

        protected virtual void Seed(TContext context)
        {
        }
    }

    public class EnumCtxConfiguration : DbMigrationsConfiguration<EnumTestSupportContext>
    {
        public EnumCtxConfiguration()
        {
            CodeGenerator = new XGMigrationCodeGenerator();
            AutomaticMigrationsEnabled = true;
            SetSqlGenerator("XuguClient", new XGMigrationSqlGenerator());
            SetHistoryContextFactory("XuguClient", (existingConnection, defaultSchema) => new MyHistoryContext(existingConnection, "SYSDBA"));
        }
    }
    #endregion

    #region SpatialDataType
    public class MyPlace
    {
        [Key]
        public int Id { get; set; }
        public string name { get; set; }
        public DbGeometry location { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class JourneyContext : DbContext
    {
        public DbSet<MyPlace> MyPlaces { get; set; }
        public JourneyContext() : base(CodeFirstFixture.GetEFConnectionString<JourneyContext>())
        {
            Database.SetInitializer<JourneyContext>(new JourneyInitialize<JourneyContext>());
            Database.SetInitializer<JourneyContext>(new MigrateDatabaseToLatestVersion<JourneyContext, JourneyConfiguration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class JourneyInitialize<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        public void InitializeDatabase(TContext context)
        {
            context.Database.Delete();
            context.Database.CreateIfNotExists();
            this.Seed(context);
            context.SaveChanges();
        }

        protected virtual void Seed(TContext context)
        {
        }
    }

    public class JourneyConfiguration : DbMigrationsConfiguration<JourneyContext>
    {
        public JourneyConfiguration()
        {
            CodeGenerator = new XGMigrationCodeGenerator();
            AutomaticMigrationsEnabled = true;
            SetSqlGenerator("XuguClient", new XGMigrationSqlGenerator());
            SetHistoryContextFactory("XuguClient", (existingConnection, defaultSchema) => new MyHistoryContext(existingConnection, "SYSDBA"));
        }
    }
    #endregion

    #region CustomCodeFirstConvention
    public class MyCustomConventions : System.Data.Entity.ModelConfiguration.Conventions.Convention
    {
        public MyCustomConventions()
        {
            Properties().
              Where(prop => typeof(decimal) == prop.GetType()).
              Configure(config => config.HasPrecision(16, 2));
        }
    }
    #endregion

    #region ComplexTypeSupport
    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
    }
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public List<SchoolSchedule> Schedule { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class EntityAndComplexTypeContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<SchoolSchedule> Schedules { get; set; }

        public EntityAndComplexTypeContext() : base(CodeFirstFixture.GetEFConnectionString<EntityAndComplexTypeContext>())
        {
            Database.SetInitializer<EntityAndComplexTypeContext>(new MovieCBCDBInitialize<EntityAndComplexTypeContext>());
            Database.SetInitializer<EntityAndComplexTypeContext>(new MigrateDatabaseToLatestVersion<EntityAndComplexTypeContext, Configuration<EntityAndComplexTypeContext>>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
    #endregion
}