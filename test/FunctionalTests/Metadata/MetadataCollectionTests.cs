// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using Xunit;

    public class MetadataCollectionTests : FunctionalTestBase
    {
        [Fact]
        public void Verify_that_models_with_more_than_UseSortedListCrossover_entities_work_fine()
        {
            using (var ctx = new MyContext())
            {
                ctx.MyEntities_01.Add(new MyEntity_01());
            }
        }

        [Fact]
        public void Verify_that_entities_with_more_than_UseSortedListCrossover_properties_work_fine()
        {
            using (var ctx = new MyContext2())
            {
                ctx.MyBigEntities.Add(
                    new MyBigEntity
                        {
                            Property01 = "Test"
                        });
            }
        }

        // Model with more than 25 entities
        public class MyEntity_01
        {
            public int Id { get; set; }
        }

        public class MyEntity_02
        {
            public int Id { get; set; }
        }

        public class MyEntity_03
        {
            public int Id { get; set; }
        }

        public class MyEntity_04
        {
            public int Id { get; set; }
        }

        public class MyEntity_05
        {
            public int Id { get; set; }
        }

        public class MyEntity_06
        {
            public int Id { get; set; }
        }

        public class MyEntity_07
        {
            public int Id { get; set; }
        }

        public class MyEntity_08
        {
            public int Id { get; set; }
        }

        public class MyEntity_09
        {
            public int Id { get; set; }
        }

        public class MyEntity_10
        {
            public int Id { get; set; }
        }

        public class MyEntity_11
        {
            public int Id { get; set; }
        }

        public class MyEntity_12
        {
            public int Id { get; set; }
        }

        public class MyEntity_13
        {
            public int Id { get; set; }
        }

        public class MyEntity_14
        {
            public int Id { get; set; }
        }

        public class MyEntity_15
        {
            public int Id { get; set; }
        }

        public class MyEntity_16
        {
            public int Id { get; set; }
        }

        public class MyEntity_17
        {
            public int Id { get; set; }
        }

        public class MyEntity_18
        {
            public int Id { get; set; }
        }

        public class MyEntity_19
        {
            public int Id { get; set; }
        }

        public class MyEntity_20
        {
            public int Id { get; set; }
        }

        public class MyEntity_21
        {
            public int Id { get; set; }
        }

        public class MyEntity_22
        {
            public int Id { get; set; }
        }

        public class MyEntity_23
        {
            public int Id { get; set; }
        }

        public class MyEntity_24
        {
            public int Id { get; set; }
        }

        public class MyEntity_25
        {
            public int Id { get; set; }
        }

        public class MyEntity_26
        {
            public int Id { get; set; }
        }

        public class MyContext : DbContext
        {
            static MyContext()
            {
                Database.SetInitializer<MyContext>(null);
            }

            public DbSet<MyEntity_01> MyEntities_01 { get; set; }
            public DbSet<MyEntity_02> MyEntities_02 { get; set; }
            public DbSet<MyEntity_03> MyEntities_03 { get; set; }
            public DbSet<MyEntity_04> MyEntities_04 { get; set; }
            public DbSet<MyEntity_05> MyEntities_05 { get; set; }
            public DbSet<MyEntity_06> MyEntities_06 { get; set; }
            public DbSet<MyEntity_07> MyEntities_07 { get; set; }
            public DbSet<MyEntity_08> MyEntities_08 { get; set; }
            public DbSet<MyEntity_09> MyEntities_09 { get; set; }
            public DbSet<MyEntity_10> MyEntities_10 { get; set; }
            public DbSet<MyEntity_11> MyEntities_11 { get; set; }
            public DbSet<MyEntity_12> MyEntities_12 { get; set; }
            public DbSet<MyEntity_13> MyEntities_13 { get; set; }
            public DbSet<MyEntity_14> MyEntities_14 { get; set; }
            public DbSet<MyEntity_15> MyEntities_15 { get; set; }
            public DbSet<MyEntity_16> MyEntities_16 { get; set; }
            public DbSet<MyEntity_17> MyEntities_17 { get; set; }
            public DbSet<MyEntity_18> MyEntities_18 { get; set; }
            public DbSet<MyEntity_19> MyEntities_19 { get; set; }
            public DbSet<MyEntity_20> MyEntities_20 { get; set; }
            public DbSet<MyEntity_21> MyEntities_21 { get; set; }
            public DbSet<MyEntity_22> MyEntities_22 { get; set; }
            public DbSet<MyEntity_23> MyEntities_23 { get; set; }
            public DbSet<MyEntity_24> MyEntities_24 { get; set; }
            public DbSet<MyEntity_25> MyEntities_25 { get; set; }
            public DbSet<MyEntity_26> MyEntities_26 { get; set; }
        }

        // Model with an entity with more than 25 properties
        public class MyBigEntity
        {
            public int Id { get; set; }
            public string Property01 { get; set; }
            public string Property02 { get; set; }
            public string Property03 { get; set; }
            public string Property04 { get; set; }
            public string Property05 { get; set; }
            public string Property06 { get; set; }
            public string Property07 { get; set; }
            public string Property08 { get; set; }
            public string Property09 { get; set; }
            public string Property10 { get; set; }
            public string Property11 { get; set; }
            public string Property12 { get; set; }
            public string Property13 { get; set; }
            public string Property14 { get; set; }
            public string Property15 { get; set; }
            public string Property16 { get; set; }
            public string Property17 { get; set; }
            public string Property18 { get; set; }
            public string Property19 { get; set; }
            public string Property20 { get; set; }
            public string Property21 { get; set; }
            public string Property22 { get; set; }
            public string Property23 { get; set; }
            public string Property24 { get; set; }
            public string Property25 { get; set; }
            public string Property26 { get; set; }
        }

        public class MyContext2 : DbContext
        {
            static MyContext2()
            {
                Database.SetInitializer<MyContext2>(null);
            }

            public DbSet<MyBigEntity> MyBigEntities { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MyBigEntity>().Property(e => e.Property01).HasColumnName("NewProp01");
            }
        }
    }
}
