// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ProviderAgnosticModel;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class MaterializationTests
    {
        [Fact]
        public void Can_materialize_list_of_entity_properties()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Runs.Where(r => r.Id == 1).Select(r => new List<int> { r.Id, r.RunOwner.Id });

                // materializing results
                var results = query.ToList();
                var runId = context.Runs.Select(r => r.Id).First(r => r == 1);
                var ownerId = context.Runs.Select(r => r.RunOwner.Id).First(r => r == 1);

                Assert.Equal(1, results.Count);
                Assert.Equal(2, results[0].Count);
                Assert.Equal(runId, results[0][0]);
                Assert.Equal(ownerId, results[0][1]);
            }
        }

        [Fact]
        public void Can_materialize_list_inside_anonymous_type()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Runs.Where(r => r.Id == 1).Select(r => new { r.Id, List = new List<string> { r.Name, "a", "b" } });
                var results = query.ToList();
                var runId = context.Runs.Select(r => r.Id).First(r => r == 1);
                var runName = context.Runs.Where(r => r.Id == 1).Select(r => r.Name).First();
                Assert.Equal(1, results.Count);
                Assert.Equal(runId, results[0].Id);
                Assert.Equal(3, results[0].List.Count);
                Assert.Equal(runName, results[0].List[0]);
                Assert.Equal("a", results[0].List[1]);
                Assert.Equal("b", results[0].List[2]);
            }
        }

        [Fact]
        public void Can_materialize_collection()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Failures.Where(f => f.Id == 1).Select(f => f.Bugs);
                var results = query.ToList();
                var bugs = context.Bugs.Where(b => b.Failure.Id == 1).ToList();

                Assert.Equal(1, results.Count);
                Assert.Equal(bugs.Count, results[0].Count);
            }
        }

        [Fact]
        public void Can_materialize_collection_inside_anonymous_type()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Failures.Where(f => f.Id == 1).Select(f => new { f.Bugs });
                var results = query.ToList();
                var bugs = context.Bugs.Where(b => b.Failure.Id == 1).ToList();

                Assert.Equal(1, results.Count);
                Assert.Equal(bugs.Count, results[0].Bugs.Count);
            }
        }

        [Fact]
        public void Can_materialize_properties_into_non_mapped_type()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Owners.Where(o => o.Id == 1).Select(o => new MyNonMappedType { Id = o.Id, Name = o.FirstName });
                var results = query.ToList();
                var id = context.Owners.Select(o => o.Id).First(r => r == 1);
                var name = context.Owners.Where(o => o.Id == 1).Select(o => o.FirstName).First();

                Assert.Equal(1, results.Count);
                Assert.Equal(id, results[0].Id);
                Assert.Equal(name, results[0].Name);
            }
        }

        [Fact]
	    public void Can_materialize_properties_into_non_mapped_type_with_nested_list()
	    {
            using (var context = new ProviderAgnosticContext())
	        {
                var query = context.Gears.Where(g => g.Nickname == "Marcus").Select(g => new MyNonMappedTypeWithNestedNonMappedList
                {
	                NestedList = g.Reports.Select(r => new MyNonMappedType { Id = r.SquadId, Name = r.Nickname }).ToList()
                });

                var results = query.ToList();
                var reports = context.Gears.Include(g => g.Reports).Single(r => r.Nickname == "Marcus").Reports.Count;

                Assert.Equal(1, results.Count);
                Assert.Equal(reports, results[0].NestedList.Count);
	        }
	    }

        [Fact]
        public void Can_materialize_nested_anonymous_types()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var query = context.Owners.Where(o => o.Id == 1).Select(o => new
                    {
                        o.Id,
                        Name = new { First = o.FirstName, Last = o.LastName }
                    });

                var results = query.ToList();
                var id = context.Owners.Select(o => o.Id).First(r => r == 1);
                var firstName = context.Owners.Where(o => o.Id == 1).Select(o => o.FirstName).First();
                var lastName = context.Owners.Where(o => o.Id == 1).Select(o => o.LastName).First();

                Assert.Equal(1, results.Count);
                Assert.Equal(id, results[0].Id);
                Assert.Equal(firstName, results[0].Name.First);
                Assert.Equal(lastName, results[0].Name.Last);
            }
        }

        [Fact]
        public void Materialize_DbSet_throws()
        {
            using (var context = new ProviderAgnosticContext())
            {
                Assert.Throws<ArgumentException>(() => context.Runs.Select(r => context.Owners));
                context.Runs.Select(r => context.Owners.AsEnumerable()).ToList();
            }
        }

        [Fact]
        public void Materialize_ObjectQuery_throws()
        {
            using (var context = new ProviderAgnosticContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                var owners = objectContext.CreateQuery<Owner>("Owners");
                Assert.Throws<InvalidOperationException>(() => context.Runs.Select(r => owners).ToList());
                context.Runs.Select(r => owners.AsEnumerable()).ToList();
            }
        }

        public class MyNonMappedType
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class MyNonMappedTypeWithNestedNonMappedList
	    {
	        public List<MyNonMappedType> NestedList { get; set; }
	    }

        [Fact] // CodePlex 1566
        public void Collection_instances_are_created_when_lazy_loading_related_collections()
        {
            using (var context = new CollectionsContext())
            {
                var entity = context.LotsOfCollections.Single();
                Assert.Equal(1, entity.AnICollection.Count);
                Assert.Equal(1, entity.AnIList.Count);
                Assert.Equal(1, entity.AnISet.Count);
                Assert.Equal(1, entity.AList.Count);
                Assert.Equal(1, entity.AHashSet.Count);
                Assert.Equal(1, entity.ACustomCollection.Count);
                Assert.Equal(1, entity.AGenericCustomCollection.Count);
            }
        }

        [Fact] // CodePlex 1566
        public void Collection_instances_are_created_when_eager_loading_related_collections()
        {
            using (var context = new CollectionsContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var entity = context
                    .LotsOfCollections
                    .Include(e => e.AnICollection)
                    .Include(e => e.AnIList)
                    .Include(e => e.AnISet)
                    .Include(e => e.AList)
                    .Include(e => e.AHashSet)
                    .Include(e => e.ACustomCollection)
                    .Include(e => e.AGenericCustomCollection)
                    .Single();

                Assert.Equal(1, entity.AnICollection.Count);
                Assert.Equal(1, entity.AnIList.Count);
                Assert.Equal(1, entity.AnISet.Count);
                Assert.Equal(1, entity.AList.Count);
                Assert.Equal(1, entity.AHashSet.Count);
                Assert.Equal(1, entity.ACustomCollection.Count);
                Assert.Equal(1, entity.AGenericCustomCollection.Count);
            }
        }

        [Fact] // CodePlex 1566
        public void Collection_instances_are_created_when_explicitly_loading_related_collections()
        {
            using (var context = new CollectionsContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var entity = context.LotsOfCollections.Single();
                var entry = context.Entry(entity);

                entry.Collection(e => e.AnICollection).Load();
                entry.Collection(e => e.AnIList).Load();
                entry.Collection(e => e.AnISet).Load();
                entry.Collection(e => e.AList).Load();
                entry.Collection(e => e.AHashSet).Load();
                entry.Collection(e => e.ACustomCollection).Load();
                entry.Collection(e => e.AGenericCustomCollection).Load();

                Assert.Equal(1, entity.AnICollection.Count);
                Assert.Equal(1, entity.AnIList.Count);
                Assert.Equal(1, entity.AnISet.Count);
                Assert.Equal(1, entity.AList.Count);
                Assert.Equal(1, entity.AHashSet.Count);
                Assert.Equal(1, entity.ACustomCollection.Count);
                Assert.Equal(1, entity.AGenericCustomCollection.Count);
            }
        }

        public class CollectionsContext : DbContext
        {
            public CollectionsContext()
                : base("CollectionsContext")
            {
            }

            static CollectionsContext()
            {
                Database.SetInitializer(new CollectionsContextInitializer());
            }

            public DbSet<LotsOfCollections> LotsOfCollections { get; set; }
        }

        public class LotsOfCollections
        {
            public int Id { get; set; }
            public virtual ICollection<InLotsOfCollections> AnICollection { get; set; }
            public virtual IList<InLotsOfCollections> AnIList { get; set; }
            public virtual ISet<InLotsOfCollections> AnISet { get; set; }
            public virtual List<InLotsOfCollections> AList { get; set; }
            public virtual HashSet<InLotsOfCollections> AHashSet { get; set; }
            public virtual CustomCollection ACustomCollection { get; set; }
            public virtual CustomCollection<InLotsOfCollections> AGenericCustomCollection { get; set; }
        }

        public class InLotsOfCollections
        {
            public int Id { get; set; }
        }

        public class CustomCollection<TElement> : List<TElement>
        {
        }

        public class CustomCollection : CustomCollection<InLotsOfCollections>
        {
        }

        public class CollectionsContextInitializer : DropCreateDatabaseIfModelChanges<CollectionsContext>
        {
            protected override void Seed(CollectionsContext context)
            {
                context.LotsOfCollections.Add(new LotsOfCollections
                {
                    AnICollection = new CustomCollection { new InLotsOfCollections() },
                    AnIList = new CustomCollection { new InLotsOfCollections() },
                    AnISet = new HashSet<InLotsOfCollections> { new InLotsOfCollections() },
                    AList = new CustomCollection { new InLotsOfCollections() },
                    AHashSet = new HashSet<InLotsOfCollections> { new InLotsOfCollections() },
                    ACustomCollection = new CustomCollection { new InLotsOfCollections() },
                    AGenericCustomCollection = new CustomCollection { new InLotsOfCollections() },
                });
            }
        }
    }
}