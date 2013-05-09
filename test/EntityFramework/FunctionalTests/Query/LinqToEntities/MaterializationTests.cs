// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class MaterializationTests : FunctionalTestBase
    {
        [Fact]
        public void Can_materialize_list_of_entity_properties()
        {
            using (var context = new ArubaContext())
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
        public void Materialize_array_of_entity_properties_throws()
        {
            using (var context = new ArubaContext())
            {
                var query = context.Runs.Where(r => r.Id == 1).Select(r => new int[] { r.Id, r.RunOwner.Id });
                var innerException = Assert.Throws<TargetInvocationException>(() => query.ToList())
                    .InnerException;

                Assert.IsType<InvalidOperationException>(innerException);
                innerException.ValidateMessage(
                          typeof(DbContext).Assembly,
                          "ObjectQuery_UnableToMaterializeArray",
                          null,
                          "System.Int32[]",
                          "System.Collections.Generic.List`1[System.Int32]");
            }
        }

        [Fact]
        public void Materializing_empty_list_throws()
        {
            using (var context = new ArubaContext())
            {
                var query = context.Runs.Select(r => new List<int> { });
                Assert.Throws<NotSupportedException>(() => query.ToList()).
                       ValidateMessage(
                           typeof(DbContext).Assembly,
                           "ELinq_UnsupportedEnumerableType",
                           null,
                           "System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]");
            }
        }

        [Fact]
        public void Can_materialize_list_inside_anonymous_type()
        {
            using (var context = new ArubaContext())
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
            using (var context = new ArubaContext())
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
            using (var context = new ArubaContext())
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
            using (var context = new ArubaContext())
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
	        using (var context = new ArubaContext())
	        {
	            var query = context.People.Where(o => o.Id == 1).Select(m => new MyNonMappedTypeWithNestedNonMappedList
	            {
	                NestedList = m.Children.Select(p => new MyNonMappedType { Id = p.Id, Name = p.Name }).ToList()
	            });
	            var results = query.ToList();
	            var children = context.People.Include(i => i.Children).Single(o => o.Id == 1).Children.Count;
	  
	            Assert.Equal(1, results.Count);
	            Assert.Equal(children, results[0].NestedList.Count);
	        }
	    }

        [Fact]
        public void Can_materialize_nested_anonymous_types()
        {
            using (var context = new ArubaContext())
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
        public void Can_materialize_null_complex_type()
        {
            using (var context = new ArubaContext())
            {
                var query = context.Runs.Select(r => r.Tasks.Where(t => t.Id < 0).Select(t => t.TaskInfo).FirstOrDefault());
                var results = query.ToList();
                Assert.IsType<List<ArubaTaskInfo>>(results);
                foreach (var result in results)
                {
                    Assert.Null(result);
                }
            }
        }

        [Fact]
        public void Materialize_DbSet_throws()
        {
            using (var context = new ArubaContext())
            {
                Assert.Throws<ArgumentException>(() => context.Runs.Select(r => context.Owners));
                context.Runs.Select(r => context.Owners.AsEnumerable()).ToList();
            }
        }

        [Fact]
        public void Materialize_ObjectQuery_throws()
        {
            using(var context = new ArubaContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                var owners = objectContext.CreateQuery<ArubaOwner>("Owners");
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
    }
}