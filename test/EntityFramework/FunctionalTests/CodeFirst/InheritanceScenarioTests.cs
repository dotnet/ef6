// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Linq;
    using System.Transactions;
    using FunctionalTests;
    using Xunit;

    public class InheritanceScenarioTests : TestBase
    {
        [Fact] // CodePlex 583
        public void Subclasses_with_different_properties_to_same_column_using_TPH_can_round_trip()
        {
            using (var context = new FunctionalTests.InheritanceScenarioTests.TphPersonContext())
            {
                Assert.Equal("N/A", context.People.OfType<FunctionalTests.InheritanceScenarioTests.Student>().Single(p => p.Name == "Jesse").Career);
                Assert.Equal("Chemistry", context.People.OfType<FunctionalTests.InheritanceScenarioTests.Teacher>().Single(p => p.Name == "Walter").Department);
                Assert.Equal("Laundering", context.People.OfType<FunctionalTests.InheritanceScenarioTests.Lawyer>().Single(p => p.Name == "Saul").Specialty);
                Assert.Equal("DEA", context.People.OfType<FunctionalTests.InheritanceScenarioTests.Officer>().Single(p => p.Name == "Hank").Department);
                Assert.Equal("Skyler", context.People.Single(p => p.Name == "Skyler").Name);

                Assert.IsType<FunctionalTests.InheritanceScenarioTests.CarWash>(context.Covers.OfType<FunctionalTests.InheritanceScenarioTests.CarWash>().Single(p => p.Name == "Skyler's Car Wash"));
                Assert.IsType<FunctionalTests.InheritanceScenarioTests.FastFoodChain>(context.Covers.OfType<FunctionalTests.InheritanceScenarioTests.FastFoodChain>().Single(p => p.Name == "Chickin' Lickin'"));
                Assert.IsType<FunctionalTests.InheritanceScenarioTests.LosPollosHermanos>(context.Covers.OfType<FunctionalTests.InheritanceScenarioTests.FastFoodChain>().Single(p => p.Name == "Chicken Bros"));

                Assert.Equal(1, context.Labs.OfType<FunctionalTests.InheritanceScenarioTests.MobileLab>().Single().Vehicle.Registration);
                Assert.Equal(2, context.Labs.OfType<FunctionalTests.InheritanceScenarioTests.MobileLab>().Single().Vehicle.Info.Depth);
                Assert.Equal(3, context.Labs.OfType<FunctionalTests.InheritanceScenarioTests.MobileLab>().Single().Vehicle.Info.Size);
                Assert.Equal(4, context.Labs.OfType<FunctionalTests.InheritanceScenarioTests.StaticLab>().Single().LabNumber);
                Assert.Equal(5, context.Labs.OfType<FunctionalTests.InheritanceScenarioTests.StaticLab>().Single().LabInfo.Depth);
                Assert.Equal(6, context.Labs.OfType<FunctionalTests.InheritanceScenarioTests.StaticLab>().Single().LabInfo.Size);

                using (context.Database.BeginTransaction())
                {
                    context.People.Local.OfType<FunctionalTests.InheritanceScenarioTests.Teacher>().Single().Department = "Heisenberg";
                    context.Labs.Local.OfType<FunctionalTests.InheritanceScenarioTests.MobileLab>().Single().Vehicle.Registration = 11;
                    context.SaveChanges();

                    Assert.Equal("Heisenberg", context.People.OfType<FunctionalTests.InheritanceScenarioTests.Teacher>().Select(p => p.Department).Single());
                    Assert.Equal(11, context.Labs.OfType<FunctionalTests.InheritanceScenarioTests.MobileLab>().Select(p => p.Vehicle.Registration).Single());
                }
            }
        }

        [Fact]
        public void TPT_model_with_PK_property_to_different_columns_in_different_tables_roundtrips()
        {
            TPT_or_TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips<ContextForPkNamingTPT>();
        }

        [Fact]
        public void TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips()
        {
            TPT_or_TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips<ContextForPkNamingTPC>();
        }

        private void TPT_or_TPC_model_with_PK_property_to_different_columns_in_different_tables_roundtrips<TContext>()
            where TContext : BaseContextForPkNaming, new()
        {
            using (var context = new TContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    var baseEntity = context.Bases.Add(
                        new BaseForPKNaming
                        {
                            Id = 1,
                            Foo = "Foo1"
                        });
                    var derivedEntity =
                        context.Deriveds.Add(
                            new DerivedForPKNaming
                            {
                                Id = 2,
                                Foo = "Foo2",
                                Bar = "Bar2"
                            });

                    context.SaveChanges();

                    context.Entry(baseEntity).State = EntityState.Detached;
                    context.Entry(derivedEntity).State = EntityState.Detached;

                    var foundBase = context.Bases.Single(e => e.Id == baseEntity.Id);
                    var foundDerived = context.Deriveds.Single(e => e.Id == derivedEntity.Id);

                    Assert.Equal("Foo1", foundBase.Foo);
                    Assert.Equal("Foo2", foundDerived.Foo);
                    Assert.Equal("Bar2", foundDerived.Bar);

                    Assert.True(context.Database.SqlQuery<int>("select base_id from base_table").Any());
                    Assert.True(context.Database.SqlQuery<int>("select derived_id from derived_table").Any());

                    if (typeof(TContext)
                        == typeof(ContextForPkNamingTPC))
                    {
                        Assert.True(context.Database.SqlQuery<string>("select base_foo from base_table").Any());
                        Assert.True(context.Database.SqlQuery<string>("select derived_foo from derived_table").Any());
                    }
                }
            }
        }
    }
}
