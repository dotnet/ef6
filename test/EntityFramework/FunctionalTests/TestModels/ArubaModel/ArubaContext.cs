// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Infrastructure;

    public class ArubaContext : DbContext
    {
        static ArubaContext()
        {
            Database.SetInitializer(new ArubaInitializer());
        }

        public DbSet<ArubaAllTypes> AllTypes { get; set; }
        public DbSet<ArubaBaseline> Baselines { get; set; }
        public DbSet<ArubaBug> Bugs { get; set; }
        public DbSet<ArubaConfig> Configs { get; set; }
        public DbSet<ArubaFailure> Failures { get; set; }
        public DbSet<ArubaOwner> Owners { get; set; }
        public DbSet<ArubaRun> Runs { get; set; }
        public DbSet<ArubaTask> Tasks { get; set; }
        public DbSet<ArubaPerson> People { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ModelNamespaceConvention>();
            modelBuilder.Entity<ArubaFailure>().ToTable("ArubaFailures");
            modelBuilder.Entity<ArubaBaseline>().ToTable("ArubaBaselines");
            modelBuilder.Entity<ArubaTestFailure>().ToTable("ArubaTestFailures");
            
            // composite key, non-integer key
            modelBuilder.Entity<ArubaTask>().HasKey(k => new { k.Id, k.Name });
            
            // need to map key explicitly, otherwise we get into invalid state
            modelBuilder.Entity<ArubaRun>().HasMany(r => r.Tasks).WithRequired().Map(m => { });
            
            // non-generated key
            modelBuilder.Entity<ArubaOwner>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<ArubaRun>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            
            modelBuilder.Entity<ArubaRun>().HasRequired(r => r.RunOwner).WithRequiredDependent(o => o.OwnedRun);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c6_smalldatetime).HasColumnType("smalldatetime");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c7_decimal_28_4).HasColumnType("decimal").HasPrecision(28, 4);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c8_numeric_28_4).HasColumnType("numeric").HasPrecision(28, 4);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c11_money).HasColumnType("money");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c12_smallmoney).HasColumnType("smallmoney");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c13_varchar_512_).HasMaxLength(512).IsVariableLength().IsUnicode(false);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c14_char_512_).HasMaxLength(512).IsFixedLength().IsUnicode(false);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c15_text).HasColumnType("text");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c16_binary_512_).HasMaxLength(512).IsFixedLength();
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c17_varbinary_512_).HasMaxLength(512).IsVariableLength();
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c18_image).HasColumnType("image");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c19_nvarchar_512_).HasMaxLength(512).IsVariableLength().IsUnicode(true);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c20_nchar_512_).HasMaxLength(512).IsFixedLength().IsUnicode(true);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c21_ntext).HasColumnType("ntext");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c24_varchar_max_).IsMaxLength().IsVariableLength().IsUnicode(false);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c25_nvarchar_max_).IsMaxLength().IsVariableLength().IsUnicode(true);
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c26_varbinary_max_).IsMaxLength().IsVariableLength();
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c28_date).HasColumnType("date");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c29_datetime2).HasColumnType("datetime2");
            modelBuilder.Entity<ArubaAllTypes>().Property(p => p.c35_timestamp).HasColumnType("timestamp");

            // self reference
            modelBuilder.Entity<ArubaPerson>().HasOptional(p => p.Partner).WithOptionalPrincipal();
            modelBuilder.Entity<ArubaPerson>().HasMany(p => p.Children).WithMany(p => p.Parents);

            // entity splitting
            modelBuilder.Entity<ArubaBug>().Map(m =>
                {
                    m.Properties(p => new { p.Id, p.Comment });
                    m.ToTable("Bugs1");
                });
            modelBuilder.Entity<ArubaBug>().Map(m =>
                {
                    m.Properties(p => new { p.Number, p.Resolution });
                    m.ToTable("Bugs2");
                });

            modelBuilder.Entity<ArubaOwner>().Property(o => o.FirstName).HasMaxLength(30);
        }
    }
}
