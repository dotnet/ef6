// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;

    public class NamespaceAndNestingTests : FunctionalTestBase
    {
        [Fact]
        [UseDefaultExecutionStrategy]
        public void Code_First_can_use_types_that_have_the_same_name_but_different_namespaces()
        {
            // This test excerises queries and updates using models with a variety of types
            // including enums and complex types where the types have the same names but
            // are in different namespaces.

            using (var moon = new MoonContext())
            {
                var cheese = moon.Cheeses.Single();

                Assert.Equal("Wensleydale", cheese.Name);
                Assert.Equal(TheMoon.Maturity.Engineer, cheese.Info.Maturity);
                Assert.Equal(64, cheese.Info.Image.Length);
                Assert.Equal(new[] { "Branston", "Piccalilli" }, cheese.Pickles.Select(p => p.Name).OrderBy(n => n));

                using (moon.Database.BeginTransaction())
                {
                    cheese.Pickles.Add(
                        new TheMoon.Pickle
                            {
                                Name = "Gromit Special"
                            });
                    moon.SaveChanges();

                    Assert.Equal(
                        new[] { "Branston", "Gromit Special", "Piccalilli" },
                        moon.Pickles.AsNoTracking().Select(p => p.Name).OrderBy(n => n));
                }
            }

            using (var earth = new EarthContext())
            {
                var cheese = earth.Cheeses.Single();

                Assert.Equal("Cheddar", cheese.Name);
                Assert.Equal(TheEarth.Maturity.Teenager, cheese.Info.Maturity);
                Assert.Equal(64, cheese.Info.Image.Length);
                Assert.Equal(new[] { "Dill", "Relish" }, cheese.Pickles.Select(p => p.Name).OrderBy(n => n));

                using (earth.Database.BeginTransaction())
                {
                    earth.Cheeses.Add(
                        new TheEarth.Cheese
                            {
                                Name = "Swiss",
                                Info = new TheEarth.CheeseInfo(TheEarth.Maturity.Todler, new byte[32]),
                                Pickles = cheese.Pickles.ToList()
                            });

                    earth.SaveChanges();

                    cheese = earth.Cheeses.Single(c => c.Name == "Swiss");

                    Assert.Equal("Swiss", cheese.Name);
                    Assert.Equal(TheEarth.Maturity.Todler, cheese.Info.Maturity);
                    Assert.Equal(32, cheese.Info.Image.Length);
                    Assert.Equal(new[] { "Dill", "Relish" }, cheese.Pickles.Select(p => p.Name).OrderBy(n => n));
                }
            }
        }

        public class MoonContext : DbContext
        {
            static MoonContext()
            {
                Database.SetInitializer(new MoonInitializer());
            }

            public DbSet<TheMoon.Cheese> Cheeses { get; set; }
            public DbSet<TheMoon.Pickle> Pickles { get; set; }
        }

        public class MoonInitializer : DropCreateDatabaseIfModelChanges<MoonContext>
        {
            protected override void Seed(MoonContext context)
            {
                context.Cheeses.Add(
                    new TheMoon.Cheese
                        {
                            Name = "Wensleydale",
                            Info = new TheMoon.CheeseInfo(TheMoon.Maturity.Engineer, new byte[64]),
                            Pickles = new List<TheMoon.Pickle>
                                {
                                    new TheMoon.Pickle
                                        {
                                            Name = "Branston"
                                        },
                                    new TheMoon.Pickle
                                        {
                                            Name = "Piccalilli"
                                        }
                                }
                        });
            }
        }

        public class EarthContext : DbContext
        {
            static EarthContext()
            {
                Database.SetInitializer(new EarthInitializer());
            }

            public DbSet<TheEarth.Cheese> Cheeses { get; set; }
            public DbSet<TheEarth.Pickle> Pickles { get; set; }
        }

        public class EarthInitializer : DropCreateDatabaseIfModelChanges<EarthContext>
        {
            protected override void Seed(EarthContext context)
            {
                context.Cheeses.Add(
                    new TheEarth.Cheese
                        {
                            Name = "Cheddar",
                            Info = new TheEarth.CheeseInfo(TheEarth.Maturity.Teenager, new byte[64]),
                            Pickles = new List<TheEarth.Pickle>
                                {
                                    new TheEarth.Pickle
                                        {
                                            Name = "Relish"
                                        },
                                    new TheEarth.Pickle
                                        {
                                            Name = "Dill"
                                        }
                                }
                        });
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Code_First_can_use_nested_types()
        {
            using (var nested = new NestedContext())
            {
                var cheese = nested.Cheeses.Single();

                Assert.Equal("Swiss", cheese.Name);
                Assert.Equal(Maturity.Todler, cheese.Info.Maturity);
                Assert.Equal(16, cheese.Info.Image.Length);
                Assert.Equal(new[] { "Ketchup", "Mustard" }, cheese.Pickles.Select(p => p.Name).OrderBy(n => n));

                using (nested.Database.BeginTransaction())
                {
                    cheese.Pickles.Add(
                        new Pickle
                            {
                                Name = "Not Pickles"
                            });
                    nested.SaveChanges();

                    Assert.Equal(
                        new[] { "Ketchup", "Mustard", "Not Pickles" },
                        nested.Pickles.AsNoTracking().Select(p => p.Name).OrderBy(n => n));
                }
            }
        }

        public class NestedContext : DbContext
        {
            static NestedContext()
            {
                Database.SetInitializer(new NestedInitializer());
            }

            public DbSet<Cheese> Cheeses { get; set; }
            public DbSet<Pickle> Pickles { get; set; }
        }

        public class NestedInitializer : DropCreateDatabaseIfModelChanges<NestedContext>
        {
            protected override void Seed(NestedContext context)
            {
                context.Cheeses.Add(
                    new Cheese
                        {
                            Name = "Swiss",
                            Info = new CheeseInfo(Maturity.Todler, new byte[16]),
                            Pickles = new List<Pickle>
                                {
                                    new Pickle
                                        {
                                            Name = "Ketchup"
                                        },
                                    new Pickle
                                        {
                                            Name = "Mustard"
                                        }
                                }
                        });
            }
        }

        public class Cheese
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public CheeseInfo Info { get; set; }
            public virtual ICollection<Pickle> Pickles { get; set; }
        }

        public class Pickle
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int CheeseId { get; set; }
            public virtual Cheese Cheese { get; set; }
        }

        public enum Maturity
        {
            Todler,
            Teenager,
            Engineer,
        }

        public class CheeseInfo
        {
            private CheeseInfo()
            {
            }

            public CheeseInfo(Maturity maturity, byte[] image)
            {
                Maturity = maturity;
                Image = image;
            }

            public Maturity Maturity { get; private set; }
            public byte[] Image { get; private set; }
        }

        [Fact]
        public void GetDatabaseValues_can_be_used_with_nested_types()
        {
            using (var nested = new NestedContext())
            {
                var dbValues = nested.Entry(nested.Cheeses.Single()).GetDatabaseValues();

                Assert.Equal("Swiss", dbValues.GetValue<string>("Name"));
                Assert.Equal(Maturity.Todler, dbValues.GetValue<DbPropertyValues>("Info").GetValue<Maturity>("Maturity"));
            }
        }

        [Fact]
        public void Find_can_be_used_with_nested_types()
        {
            using (var nested = new NestedContext())
            {
                var cheese = nested.Cheeses.Find(nested.Cheeses.AsNoTracking().Single().Id);

                Assert.Equal("Swiss", cheese.Name);
                Assert.Equal(Maturity.Todler, cheese.Info.Maturity);
                Assert.Equal(16, cheese.Info.Image.Length);
                Assert.Equal(new[] { "Ketchup", "Mustard" }, cheese.Pickles.Select(p => p.Name).OrderBy(n => n));
            }
        }

        [Fact]
        public void Change_tracking_and_lazy_loading_proxies_can_be_created_for_nested_types()
        {
            using (var context = new ProxiesInANest())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var changeTrackingProxy = context.Eagers.Create();
                Assert.IsAssignableFrom<IEntityWithRelationships>(changeTrackingProxy);
                Assert.IsType<EntityCollection<LazyInANest>>(changeTrackingProxy.Lazies);

                Assert.IsNotType<LazyInANest>(context.Lazies.Create());
            }
        }

        public class ProxiesInANest : DbContext
        {
            static ProxiesInANest()
            {
                Database.SetInitializer<ProxiesInANest>(null);
            }

            public DbSet<ChangeTrackingInANest> Eagers { get; set; }
            public DbSet<LazyInANest> Lazies { get; set; }
        }

        public class ChangeTrackingInANest
        {
            public virtual int Id { get; set; }
            public virtual LazyInANest Lazy { get; set; }
            public virtual ICollection<LazyInANest> Lazies { get; set; }
        }

        public class LazyInANest
        {
            public int Id { get; set; }
            public virtual ChangeTrackingInANest Eager { get; set; }
            public virtual ICollection<ChangeTrackingInANest> Eagers { get; set; }
        }

        [Fact]
        public void Code_First_with_EF6_model_builder_version_brings_in_nested_types()
        {
            using (var context = new NestedByConventionContext())
            {
                context.Assert<NonNestedType>().IsInModel();
                context.Assert<NonNestedType.NestedType>().IsInModel();
            }
        }

        [DbModelBuilderVersion(DbModelBuilderVersion.V6_0)]
        public class NestedByConventionContext : DbContext
        {
            static NestedByConventionContext()
            {
                Database.SetInitializer<NestedByConventionContext>(null);
            }

            public DbSet<NonNestedType> NonNestedTypes { get; set; }
        }

        [Fact]
        public void Code_First_with_older_model_builder_version_does_not_bring_in_nested_types()
        {
            using (var context = new NestedNotByConventionContext())
            {
                context.Assert<NonNestedType>().IsInModel();
                context.Assert<NonNestedType.NestedType>().IsNotInModel();
            }
        }

        [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
        public class NestedNotByConventionContext : DbContext
        {
            static NestedNotByConventionContext()
            {
                Database.SetInitializer<NestedNotByConventionContext>(null);
            }

            public DbSet<NonNestedType> NonNestedTypes { get; set; }
        }

        [Fact]
        public void Code_First_with_older_model_builder_version_can_still_use_nested_entity_types_when_explicitly_mapped_with_DbSet()
        {
            using (var context = new ExplicitlyNestedContext1())
            {
                context.Assert<NonNestedType>().IsInModel();
                context.Assert<NonNestedType.NestedType>().IsInModel();
            }
        }

        [DbModelBuilderVersion(DbModelBuilderVersion.V5_0)]
        public class ExplicitlyNestedContext1 : DbContext
        {
            static ExplicitlyNestedContext1()
            {
                Database.SetInitializer<ExplicitlyNestedContext1>(null);
            }

            public DbSet<NonNestedType.NestedType> NestedTypes { get; set; }
        }

        [Fact]
        public void Code_First_with_older_model_builder_version_can_still_use_nested_entity_types_when_explicitly_mapped_with_Entity()
        {
            using (var context = new ExplicitlyNestedContext2())
            {
                context.Assert<NonNestedType>().IsInModel();
                context.Assert<NonNestedType.NestedType>().IsInModel();
            }
        }

        [DbModelBuilderVersion(DbModelBuilderVersion.V4_1)]
        public class ExplicitlyNestedContext2 : DbContext
        {
            static ExplicitlyNestedContext2()
            {
                Database.SetInitializer<ExplicitlyNestedContext2>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<NonNestedType.NestedType>();
            }
        }

        [Fact]
        public void Explicit_use_of_two_entity_types_with_DbSet_with_the_same_simple_name_throws()
        {
            using (var context = new NameCollisionContext())
            {
                Assert.Throws<NotSupportedException>(() => context.Database.Initialize(force: false))
                      .ValidateMessage(
                          "SimpleNameCollision",
                          typeof(Outer2.SynonymToastCrunch).FullName,
                          typeof(Outer1.SynonymToastCrunch).FullName,
                          typeof(Outer2.SynonymToastCrunch).Name);
            }
        }

        public class NameCollisionContext : DbContext
        {
            static NameCollisionContext()
            {
                Database.SetInitializer<NameCollisionContext>(null);
            }

            public DbSet<Outer1.SynonymToastCrunch> Crunch1 { get; set; }
            public DbSet<Outer2.SynonymToastCrunch> Crunch2 { get; set; }
        }

        public class Outer1
        {
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
            }

            public enum EnumToastCrunch
            {
                Tasty
            }
        }

        public class Outer2
        {
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
            }

            public enum EnumToastCrunch
            {
                Tasty
            }
        }

        [Fact]
        public void Explicit_use_of_two_complex_types_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.ComplexType<Outer1.SynonymToastCrunch>();
            modelBuilder.ComplexType<Outer2.SynonymToastCrunch>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(Outer2.SynonymToastCrunch).FullName,
                      typeof(Outer1.SynonymToastCrunch).FullName,
                      typeof(Outer2.SynonymToastCrunch).Name);
        }

        [Fact]
        public void Explicit_use_of_two_enum_types_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<EntityWithEnum1>().Property(e => e.Crunch);
            modelBuilder.Entity<EntityWithEnum2>().Property(e => e.Crunch);

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(Outer2.EnumToastCrunch).FullName,
                      typeof(Outer1.EnumToastCrunch).FullName,
                      typeof(Outer2.EnumToastCrunch).Name);
        }

        public class EntityWithEnum1
        {
            public int Id { get; set; }
            public Outer1.EnumToastCrunch Crunch { get; set; }
        }

        public class EntityWithEnum2
        {
            public int Id { get; set; }
            public Outer2.EnumToastCrunch Crunch { get; set; }
        }

        [Fact]
        public void Implicit_use_of_two_entity_types_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ImplicitOuter1.SynonymToastCrunch>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(ImplicitOuter2.SynonymToastCrunch).FullName,
                      typeof(ImplicitOuter1.SynonymToastCrunch).FullName,
                      typeof(ImplicitOuter2.SynonymToastCrunch).Name);

            modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ImplicitOuter2.SynonymToastCrunch>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(ImplicitOuter1.SynonymToastCrunch).FullName,
                      typeof(ImplicitOuter2.SynonymToastCrunch).FullName,
                      typeof(ImplicitOuter1.SynonymToastCrunch).Name);
        }

        public class ImplicitOuter1
        {
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
                public ImplicitOuter2.SynonymToastCrunch Other { get; set; }
            }
        }

        public class ImplicitOuter2
        {
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
                public ICollection<ImplicitOuter1.SynonymToastCrunch> Others { get; set; }
            }
        }

        [Fact]
        public void Implicit_use_of_two_complex_types_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ImplicitComplexOuter1.CountChocula>();
            modelBuilder.Entity<ImplicitComplexOuter2.LuckyCharms>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(ImplicitComplexOuter2.ComplexCarbs).FullName,
                      typeof(ImplicitComplexOuter1.ComplexCarbs).FullName,
                      typeof(ImplicitComplexOuter2.ComplexCarbs).Name);
        }

        public class ImplicitComplexOuter1
        {
            public class CountChocula
            {
                public int Id { get; set; }
                public ComplexCarbs Carbs { get; set; }
            }

            [ComplexType]
            public class ComplexCarbs
            {
                public string YabbaDabbaDo { get; set; }
            }
        }

        public class ImplicitComplexOuter2
        {
            public class LuckyCharms
            {
                public int Id { get; set; }
                public ComplexCarbs Carbs { get; set; }
            }

            [ComplexType]
            public class ComplexCarbs
            {
                public string WabbaWobbaWoo { get; set; }
            }
        }

        [Fact]
        public void Implicit_use_of_two_enum_types_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ImplicitEnumOuter1.CountChocula>();
            modelBuilder.Entity<ImplicitEnumOuter2.LuckyCharms>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(ImplicitEnumOuter2.ComplexCarbs).FullName,
                      typeof(ImplicitEnumOuter1.ComplexCarbs).FullName,
                      typeof(ImplicitEnumOuter2.ComplexCarbs).Name);
        }

        public class ImplicitEnumOuter1
        {
            public class CountChocula
            {
                public int Id { get; set; }
                public ComplexCarbs Carbs { get; set; }
            }

            public enum ComplexCarbs
            {
                YabbaDabbaDo
            }
        }

        public class ImplicitEnumOuter2
        {
            public class LuckyCharms
            {
                public int Id { get; set; }
                public ComplexCarbs Carbs { get; set; }
            }

            public enum ComplexCarbs
            {
                WabbaWobbaWoo
            }
        }

        [Fact]
        public void Implicit_use_of_an_entity_type_and_a_complex_type_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MixedImplicitOuter1.SynonymToastCrunch>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(MixedImplicitOuter2.SynonymToastCrunch).FullName,
                      typeof(MixedImplicitOuter1.SynonymToastCrunch).FullName,
                      typeof(MixedImplicitOuter2.SynonymToastCrunch).Name);
        }

        public class MixedImplicitOuter1
        {
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
                public MixedImplicitOuter2.CountChocula Other { get; set; }
            }
        }

        public class MixedImplicitOuter2
        {
            public class CountChocula
            {
                public int Id { get; set; }
                public SynonymToastCrunch Carbs { get; set; }
            }

            [ComplexType]
            public class SynonymToastCrunch
            {
                public string YabbaDabbaDo { get; set; }
            }
        }

        [Fact]
        public void Implicit_use_of_a_complex_type_and_an_enum_type_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MixedImplicitComplexOuter1.CountChocula>();
            modelBuilder.Entity<MixedImplicitComplexOuter2.LuckyCharms>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(MixedImplicitComplexOuter2.ComplexCarbs).FullName,
                      typeof(MixedImplicitComplexOuter1.ComplexCarbs).FullName,
                      typeof(MixedImplicitComplexOuter2.ComplexCarbs).Name);
        }

        public class MixedImplicitComplexOuter1
        {
            public class CountChocula
            {
                public int Id { get; set; }
                public ComplexCarbs Carbs { get; set; }
            }

            [ComplexType]
            public class ComplexCarbs
            {
                public string YabbaDabbaDo { get; set; }
            }
        }

        public class MixedImplicitComplexOuter2
        {
            public class LuckyCharms
            {
                public int Id { get; set; }
                public ComplexCarbs Carbs { get; set; }
            }

            public enum ComplexCarbs
            {
                WabbaWobbaWoo
            }
        }

        [Fact]
        public void Implicit_use_of_an_entity_type_and_an_enum_type_with_the_same_simple_name_throws()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<MixedEnumImplicitOuter1.SynonymToastCrunch>();

            Assert.Throws<NotSupportedException>(() => modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo))
                  .ValidateMessage(
                      "SimpleNameCollision",
                      typeof(MixedEnumImplicitOuter2.SynonymToastCrunch).FullName,
                      typeof(MixedEnumImplicitOuter1.SynonymToastCrunch).FullName,
                      typeof(MixedEnumImplicitOuter2.SynonymToastCrunch).Name);
        }

        public class MixedEnumImplicitOuter1
        {
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
                public MixedEnumImplicitOuter2.CountChocula Other { get; set; }
            }
        }

        public class MixedEnumImplicitOuter2
        {
            public class CountChocula
            {
                public int Id { get; set; }
                public SynonymToastCrunch Carbs { get; set; }
            }

            public enum SynonymToastCrunch
            {
                YabbaDabbaDo
            }
        }

        [Fact]
        public void NotMapped_can_be_used_to_choose_which_type_to_use_when_otherwise_there_would_be_a_collision()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<NotMappedOuter1.SynonymToastCrunch>();

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            model.GetConceptualModel().EntityTypes.Single(e => e.Name == "SynonymToastCrunch");
        }

        public class NotMappedOuter1
        {
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
                public NotMappedOuter2.SynonymToastCrunch Other { get; set; }
            }

            [ComplexType]
            public class ComplexCarbs
            {
                public string YabbaDabbaDo { get; set; }
            }
        }

        public class NotMappedOuter2
        {
            [NotMapped]
            public class SynonymToastCrunch
            {
                public int Id { get; set; }
            }
        }

        [Fact]
        public void NotMapped_can_be_used_to_choose_which_enum_to_use_when_otherwise_there_would_be_a_collision()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<NotMappedEnumOuter1.CountChocula>();
            modelBuilder.Entity<NotMappedEnumOuter2.LuckyCharms>();

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Contains("CountChocula", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            Assert.Contains("LuckyCharms", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            model.GetConceptualModel().EnumTypes.Single(e => e.Name == "ComplexCarbs");
        }

        public class NotMappedEnumOuter1
        {
            public class CountChocula
            {
                public int Id { get; set; }
                public ComplexCarbs Carbs { get; set; }
            }

            public enum ComplexCarbs
            {
                YabbaDabbaDo
            }
        }

        public class NotMappedEnumOuter2
        {
            public class LuckyCharms
            {
                public int Id { get; set; }

                [NotMapped]
                public ComplexCarbs Carbs { get; set; }
            }

            public enum ComplexCarbs
            {
                WabbaWobbaWoo
            }
        }

        [Fact]
        public void Type_level_Ignore_can_be_used_to_choose_which_type_to_use_when_otherwise_there_would_be_a_collision()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ImplicitComplexOuter1.CountChocula>();
            modelBuilder.Entity<ImplicitComplexOuter2.LuckyCharms>();
            modelBuilder.Ignore<ImplicitComplexOuter2.ComplexCarbs>();

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Contains("CountChocula", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            Assert.Contains("LuckyCharms", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            model.GetConceptualModel().ComplexTypes.Single(e => e.Name == "ComplexCarbs");
        }

        [Fact]
        public void Property_level_Ignore_can_be_used_to_choose_which_complex_type_to_use_when_otherwise_there_would_be_a_collision()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ImplicitComplexOuter1.CountChocula>();
            modelBuilder.Entity<ImplicitComplexOuter2.LuckyCharms>().Ignore(e => e.Carbs);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Contains("CountChocula", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            Assert.Contains("LuckyCharms", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            model.GetConceptualModel().ComplexTypes.Single(e => e.Name == "ComplexCarbs");
        }

        [Fact]
        public void Property_level_Ignore_can_be_used_to_choose_which_enum_to_use_when_otherwise_there_would_be_a_collision()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<ImplicitEnumOuter1.CountChocula>();
            modelBuilder.Entity<ImplicitEnumOuter2.LuckyCharms>().Ignore(e => e.Carbs);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.Contains("CountChocula", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            Assert.Contains("LuckyCharms", model.GetConceptualModel().EntityTypes.Select(e => e.Name));
            model.GetConceptualModel().EnumTypes.Single(e => e.Name == "ComplexCarbs");
        }
    }

    public class NonNestedType
    {
        public int Id { get; set; }
        public NestedType ANestedType { get; set; }

        public class NestedType
        {
            public int Id { get; set; }
            public ICollection<NonNestedType> NonNestedTypes { get; set; }
        }
    }
}

namespace TheMoon
{
    using System.Collections.Generic;

    public class Cheese
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CheeseInfo Info { get; set; }
        public virtual ICollection<Pickle> Pickles { get; set; }
    }

    public class Pickle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CheeseId { get; set; }
        public virtual Cheese Cheese { get; set; }
    }

    public enum Maturity
    {
        Todler,
        Teenager,
        Engineer,
    }

    public class CheeseInfo
    {
        private CheeseInfo()
        {
        }

        public CheeseInfo(Maturity maturity, byte[] image)
        {
            Maturity = maturity;
            Image = image;
        }

        public Maturity Maturity { get; private set; }
        public byte[] Image { get; private set; }
    }
}

namespace TheEarth
{
    using System.Collections.Generic;

    public class Cheese
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CheeseInfo Info { get; set; }
        public virtual ICollection<Pickle> Pickles { get; set; }
    }

    public class Pickle
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CheeseId { get; set; }
        public virtual Cheese Cheese { get; set; }
    }

    public enum Maturity
    {
        Todler,
        Teenager,
        Engineer,
    }

    public class CheeseInfo
    {
        private CheeseInfo()
        {
        }

        public CheeseInfo(Maturity maturity, byte[] image)
        {
            Maturity = maturity;
            Image = image;
        }

        public Maturity Maturity { get; private set; }
        public byte[] Image { get; private set; }
    }
}
