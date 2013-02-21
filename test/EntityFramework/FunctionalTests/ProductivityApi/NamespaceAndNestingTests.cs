// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using TheMoon;
    using Xunit;
    using CheeseInfo = TheEarth.CheeseInfo;

    public class NamespaceAndNestingTests : FunctionalTestBase
    {
        [Fact]
        public void Code_First_can_use_types_that_have_the_same_name_but_different_namespaces()
        {
            // This test excerises queries and updates using models with a variety of types
            // including enums and complex types where the types have the same names but
            // are in different namespaces.

            using (var moon = new MoonContext())
            {
                var cheese = moon.Cheeses.Single();

                Assert.Equal("Wensleydale", cheese.Name);
                Assert.Equal(Maturity.Engineer, cheese.Info.Maturity);
                Assert.Equal(64, cheese.Info.Image.Length);
                Assert.Equal(new[] { "Branston", "Piccalilli" }, cheese.Pickles.Select(p => p.Name).OrderBy(n => n));

                using (moon.Database.BeginTransaction())
                {
                    cheese.Pickles.Add(
                        new Pickle
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
                                Info = new CheeseInfo(TheEarth.Maturity.Todler, new byte[32]),
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

            public DbSet<Cheese> Cheeses { get; set; }
            public DbSet<Pickle> Pickles { get; set; }
        }

        public class MoonInitializer : DropCreateDatabaseIfModelChanges<MoonContext>
        {
            protected override void Seed(MoonContext context)
            {
                context.Cheeses.Add(
                    new Cheese
                        {
                            Name = "Wensleydale",
                            Info = new TheMoon.CheeseInfo(Maturity.Engineer, new byte[64]),
                            Pickles = new List<Pickle>
                                {
                                    new Pickle
                                        {
                                            Name = "Branston"
                                        },
                                    new Pickle
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
                            Info = new CheeseInfo(TheEarth.Maturity.Teenager, new byte[64]),
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
