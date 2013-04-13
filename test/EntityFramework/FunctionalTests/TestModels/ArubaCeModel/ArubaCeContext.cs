// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaCeModel
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;

    public class ArubaCeContext : DbContext
    {
        public ArubaCeContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Database.SetInitializer(new ArubaCeInitializer());
        }

        public DbSet<ArubaAllCeTypes> AllTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ModelNamespaceConvention>();
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c7_decimal_28_4).HasColumnType("decimal").HasPrecision(28, 4);
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c8_numeric_28_4).HasColumnType("numeric").HasPrecision(28, 4);
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c11_money).HasColumnType("money");
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c16_binary_512_).HasMaxLength(512).IsFixedLength();
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c17_varbinary_512_).HasMaxLength(512).IsVariableLength();
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c18_image).HasColumnType("image");
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c19_nvarchar_512_).HasMaxLength(512).IsVariableLength().IsUnicode(true);
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c20_nchar_512_).HasMaxLength(512).IsFixedLength().IsUnicode(true);
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c21_ntext).HasColumnType("ntext");
            modelBuilder.Entity<ArubaAllCeTypes>().Property(p => p.c35_timestamp).HasColumnType("timestamp");

        }
    }
}
