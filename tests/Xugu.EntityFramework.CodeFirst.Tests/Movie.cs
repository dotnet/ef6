// Copyright (c) 2014, 2019, Oracle and/or its affiliates. All rights reserved.
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
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Migrations;
using System.ComponentModel.DataAnnotations.Schema;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    public class Movie
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public Director Director { get; set; }
        public virtual ICollection<MovieFormat> Formats { get; set; }
        public virtual ICollection<MovieMedia> Medias { get; set; }
        public byte[] Data { get; set; }
    }

    public class MovieMedia
    {
        public int ID { get; set; }
        public int MovieID { get; set; }
        public string Format { get; set; }
    }

    public class Director
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int YearBorn { get; set; }
    }

    public class MovieFormat
    {
        [Key]
        public float Format { get; set; }
    }

    [DbConfigurationType(typeof(XGEFConfiguration))]
    public class MovieDBContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieFormat> MovieFormats { get; set; }
        public DbSet<MovieRelease> MovieReleases { get; set; }
        public DbSet<MovieRelease2> MovieReleases2 { get; set; }
        public DbSet<EntitySingleColumn> EntitySingleColumns { get; set; }
        public DbSet<MovieMedia> Medias { get; set; }



        public MovieDBContext() : base(/*"IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK"*/CodeFirstFixture.GetEFConnectionString<MovieDBContext>())
        {
            Database.SetInitializer<MovieDBContext>(new MovieDBInitialize<MovieDBContext>());
            //Database.SetInitializer<MovieDBContext>(new MigrateDatabaseToLatestVersion<MovieDBContext, DbMigrationsConfiguration<MovieDBContext>>());
            //Database.SetInitializer<MovieDBContext>(new DropCreateDatabaseAlways<MovieDBContext>());
            //Database.Connection.Open();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("SYSDBA");
            modelBuilder.Configurations.AddFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            modelBuilder.Entity<Movie>().Property(x => x.Price).HasPrecision(16, 2);
            modelBuilder.Entity<Movie>().HasMany(p => p.Formats);
            modelBuilder.Entity<Movie>().HasMany(p => p.Medias);
        }
        //public override int SaveChanges()
        //{
        //    Database.Connection.Close();
        //    Database.Connection.Open();
        //    return base.SaveChanges();
        //}
    }

    public class MovieDBInitialize<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
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

    public class EntitySingleColumn
    {
        public int Id { get; set; }
    }

    public class MovieRelease
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime Timestamp { get; set; }

        // Test: ConcurrencyCheck + Not Computed
        [Required, MaxLength(45)]
        public virtual string Name { get; set; }
    }

    public class MovieRelease2
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        //public virtual DateTime Timestamp { get; set; }

        // Test: non computed column
        [Required, MaxLength(45)]
        public virtual string Name { get; set; }

        // Test: ConcurrencyCheck + Computed
        //[ConcurrencyCheck, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "bigint")]
        public virtual long RowVersion { get; set; }
    }

    public class MovieDBInitialize : DropCreateDatabaseReallyAlways<MovieDBContext>
    {
        public static Movie[] data = new Movie[] {
          new Movie() { ID = 4, Title = "Star Wars, The Sith Revenge", ReleaseDate = new DateTime( 2005, 5, 19 ) },
          new Movie() { ID = 3, Title = "Predator", ReleaseDate = new DateTime(1987, 6, 12) },
          new Movie() { ID = 2, Title = "The Matrix", ReleaseDate = new DateTime( 1999, 3, 31 ) },
          new Movie() { ID = 1, Title = "Terminator 1", ReleaseDate = new DateTime(1984, 10, 26) }
        };

        internal static void DoDataPopulation(MovieDBContext ctx)
        {
            ctx.Database.Connection.Close();
            ctx.Database.Connection.Open();
            ctx.Database.ExecuteSqlCommand("CREATE PROCEDURE IF NOT EXISTS GetCount() AS BEGIN SELECT 5; END");
            Movie m1 = new Movie() { Title = "Terminator 1", ReleaseDate = new DateTime(1984, 10, 26) };
            Movie m2 = new Movie() { Title = "The Matrix", ReleaseDate = new DateTime(1999, 3, 31) };
            Movie m3 = new Movie() { Title = "Predator", ReleaseDate = new DateTime(1987, 6, 12) };
            Movie m4 = new Movie() { Title = "Star Wars, The Sith Revenge", ReleaseDate = new DateTime(2005, 5, 19) };
            ctx.Movies.Add(m1);
            ctx.Movies.Add(m2);
            ctx.Movies.Add(m3);
            ctx.Movies.Add(m4);
            ctx.SaveChanges();
            ctx.Entry(m1).Collection(p => p.Medias).Load();
            m1.Medias.Add(new MovieMedia() { Format = "DVD" });
            m1.Medias.Add(new MovieMedia() { Format = "BlueRay" });
            ctx.Entry(m2).Collection(p => p.Medias).Load();
            m2.Medias.Add(new MovieMedia() { Format = "DVD" });
            m2.Medias.Add(new MovieMedia() { Format = "Digital" });
            ctx.Entry(m3).Collection(p => p.Medias).Load();
            m3.Medias.Add(new MovieMedia() { Format = "DVD" });
            m3.Medias.Add(new MovieMedia() { Format = "VHS" });
            ctx.Entry(m4).Collection(p => p.Medias).Load();
            m4.Medias.Add(new MovieMedia() { Format = "Digital" });
            m4.Medias.Add(new MovieMedia() { Format = "VHS" });
            ctx.SaveChanges();
        }
    }
}