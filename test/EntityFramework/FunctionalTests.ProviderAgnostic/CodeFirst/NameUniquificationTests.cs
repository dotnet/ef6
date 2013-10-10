// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Infrastructure;
    using System.Text;
    using System.Xml;
    using Xunit;

    public class NameUniquificationTests
    {
        // TPH, navigation properties with same name, on derived types.
        public class Case1
        {
            // E2 declared before E3.
            public class Order1
            {
                public class E1
                {
                    public int Id { get; set; }
                }

                public class E2 : E1
                {
                    public E4 P { get; set; }
                }

                public class E3 : E1
                {
                    public E4 P { get; set; }
                }

                public class E4
                {
                    public int Id { get; set; }
                }

                public class Context : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }

                [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
                public class ContextV5 : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }
            }

            // E2 declared after E3.
            public class Order2
            {
                public class E1
                {
                    public int Id { get; set; }
                }

                public class E3 : E1
                {
                    public E4 P { get; set; }
                }

                public class E2 : E1
                {
                    public E4 P { get; set; }
                }

                public class E4
                {
                    public int Id { get; set; }
                }

                public class Context : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }

                [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
                public class ContextV5 : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }
            }

            [Fact]
            public void Column_name_uniquification_is_deterministic()
            {
                Assert.Equal(WriteEdmx<Order1.Context>(), WriteEdmx<Order2.Context>());
            }

            [Fact]
            public void Column_name_uniquification_is_not_deterministic_for_model_builder_version_lower_than_V6()
            {
                Assert.NotEqual(WriteEdmx<Order1.ContextV5>(), WriteEdmx<Order2.ContextV5>());
            }
        }

        // TPH, inverse navigation properties to derived types.
        public class Case2
        {
            // E2 declared before E3.
            public class Order1
            {
                public class E1
                {
                    public int Id { get; set; }
                }

                public class E2 : E1
                {
                }

                public class E3 : E1
                {
                }

                public class E4
                {
                    public int Id { get; set; }
                                        
                    public ICollection<E2> P1 { get; set; }
                    public ICollection<E3> P2 { get; set; }                    
                }

                public class Context : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }

                [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
                public class ContextV5 : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }
            }

            // E2 declared after E3.
            public class Order2
            {
                public class E1
                {
                    public int Id { get; set; }
                }

                public class E3 : E1
                {
                }

                public class E2 : E1
                {
                }

                public class E4
                {
                    public int Id { get; set; }

                    public ICollection<E2> P1 { get; set; }
                    public ICollection<E3> P2 { get; set; }
                }

                public class Context : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }

                [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
                public class ContextV5 : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }
            }

            [Fact]
            public void Column_name_uniquification_is_deterministic()
            {
                Assert.Equal(WriteEdmx<Order1.Context>(), WriteEdmx<Order2.Context>());
            }

            [Fact]
            public void Column_name_uniquification_is_not_deterministic_for_model_builder_version_lower_than_V6()
            {
                Assert.NotEqual(WriteEdmx<Order1.ContextV5>(), WriteEdmx<Order2.ContextV5>());
            }
        }

        // Inverse navigation properties to same type.
        public class Case3
        {
            public class Order1
            {
                public class E1
                {
                    public int Id { get; set; }
                    
                    // P1 is declared before P2.
                    public ICollection<E2> P1 { get; set; }
                    public ICollection<E2> P2 { get; set; }
                }

                public class E2
                {
                    public int Id { get; set; }
                }

                public class Context : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }

                [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
                public class ContextV5 : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }
            }

            public class Order2
            {
                public class E1
                {
                    public int Id { get; set; }

                    // P1 is declared after P2.
                    public ICollection<E2> P2 { get; set; }
                    public ICollection<E2> P1 { get; set; }
                }

                public class E2
                {
                    public int Id { get; set; }
                }

                public class Context : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }

                [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
                public class ContextV5 : DbContext
                {
                    public DbSet<E1> E1s { get; set; }
                }
            }

            [Fact]
            public void Column_name_uniquification_is_deterministic()
            {
                Assert.Equal(WriteEdmx<Order1.Context>(), WriteEdmx<Order2.Context>());
            }

            [Fact]
            public void Column_name_uniquification_is_not_deterministic_for_model_builder_version_lower_than_V6()
            {
                Assert.NotEqual(WriteEdmx<Order1.ContextV5>(), WriteEdmx<Order2.ContextV5>());
            }
        }

        // Table splitting, same property names.
        public class Case4
        {
            public class E1
            {
                public int Id { get; set; }

                public E2 E2 { get; set; }

                [ForeignKey("P")]
                public int PId { get; set; }

                public E3 P { get; set; }
            }

            public class E2
            {
                public int Id { get; set; }

                public E1 E1 { get; set; }

                [ForeignKey("P")]
                public int PId { get; set; }
                
                public E4 P { get; set; }
            }

            public class E3
            {
                public int Id { get; set; }
            }

            public class E4
            {
                public int Id { get; set; }
            }

            public class Context1 : DbContext
            {
                // E1s declared before E2s
                public DbSet<E1> E1s { get; set; }
                public DbSet<E2> E2s { get; set; }

                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    base.OnModelCreating(modelBuilder);

                    modelBuilder.Entity<E1>().ToTable("T1");
                    modelBuilder.Entity<E2>().ToTable("T1").HasRequired(e => e.E1).WithRequiredPrincipal(e => e.E2);
                }
            }

            public class Context2 : DbContext
            {
                // E1s declared after E2s
                public DbSet<E2> E2s { get; set; }
                public DbSet<E1> E1s { get; set; }                

                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    base.OnModelCreating(modelBuilder);

                    modelBuilder.Entity<E1>().ToTable("T1");
                    modelBuilder.Entity<E2>().ToTable("T1").HasRequired(e => e.E1).WithRequiredPrincipal(e => e.E2);
                }
            }

            [Fact]
            public void Column_name_uniquification_is_deterministic()
            {
                Assert.Equal(WriteEdmx<Context1>(), WriteEdmx<Context2>().Replace("Context2", "Context1"));
            }
        }

        private static string WriteEdmx<T>() where T : DbContext, new()
        {
            Database.SetInitializer<T>(null);

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = true };

            using (var writer = XmlWriter.Create(builder, settings))
            {
                using (var context = new T())
                {
                    EdmxWriter.WriteEdmx(context, writer);
                }
            }

            return builder.ToString();
        }
    }
}
