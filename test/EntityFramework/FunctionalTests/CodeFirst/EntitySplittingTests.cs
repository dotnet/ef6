// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class EntitySplitingTests : FunctionalTestBase
    {
        public class MyEntity
        {
            [Key]
            [Column(Order = 1)]
            public int Key1 { get; set; }
            [Key]
            [Column("Key2A", Order = 2)]
            public int Key2 { get; set; }
            [Key]
            [Column(Order = 3)]
            public int Key3 { get; set; }

            public string Column1 { get; set; }
            public string Column2 { get; set; }
            public string Column3 { get; set; }
        }

        public class MyContext : DbContext
        {
            public DbSet<MyEntity> Entities { get; set; }

            static MyContext()
            {
                Database.SetInitializer<MyContext>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MyEntity>()
                    .Map(map =>
                    {
                        map.Properties(e => new
                        {
                            e.Column1
                        });

                        map.ToTable("Table1");
                    })
                    .Map(map =>
                    {
                        map.Properties(e => new
                        {
                            e.Key3,
                            e.Key1,
                            e.Key2,
                            e.Column2
                        });

                        map.ToTable("Table2");

                        map.Property(e => e.Key2).HasColumnName("T2Key2");
                        map.Property(e => e.Key3).HasColumnName("T2Key3");
                        map.Property(e => e.Key1).HasColumnName("T2Key1");
                    })
                    .Map(map =>
                    {
                        map.Property(e => e.Column3).HasColumnName("T3Column3");
                        map.Property(e => e.Key1).HasColumnName("T3Key1");

                        map.ToTable("Table3");
                    });

                modelBuilder.Entity<MyEntity>().Property(c => c.Key3).HasColumnName("Key3B");
            }
        }

        [Fact]
        public static void Column_names_are_configured_correctly()
        {
            using (var context = new MyContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var modelEntityTypes = objectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.CSpace);
                Assert.Equal(1, modelEntityTypes.Count);
                Assert.Equal(6, modelEntityTypes[0].Properties.Count);

                var storeEntityTypes = objectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.SSpace);
                Assert.Equal(3, storeEntityTypes.Count);

                Assert.Equal(4, storeEntityTypes[0].Properties.Count);
                Assert.Equal("Key1", storeEntityTypes[0].Properties[0].Name);
                Assert.Equal("Key2A", storeEntityTypes[0].Properties[1].Name);
                Assert.Equal("Key3B", storeEntityTypes[0].Properties[2].Name);
                Assert.Equal("Column1", storeEntityTypes[0].Properties[3].Name);

                Assert.Equal(4, storeEntityTypes[1].Properties.Count);
                Assert.Equal("T2Key1", storeEntityTypes[1].Properties[0].Name);
                Assert.Equal("T2Key2", storeEntityTypes[1].Properties[1].Name);
                Assert.Equal("T2Key3", storeEntityTypes[1].Properties[2].Name);
                Assert.Equal("Column2", storeEntityTypes[1].Properties[3].Name);

                Assert.Equal(4, storeEntityTypes[2].Properties.Count);
                Assert.Equal("T3Key1", storeEntityTypes[2].Properties[0].Name);
                Assert.Equal("Key2A", storeEntityTypes[2].Properties[1].Name);
                Assert.Equal("Key3B", storeEntityTypes[2].Properties[2].Name);
                Assert.Equal("T3Column3", storeEntityTypes[2].Properties[3].Name);
            }
        }
    }
}
