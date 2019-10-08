// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;
    using SimpleModel;
    using Xunit;

    public class UnimplementedNonGenericDbSetTests : FunctionalTestBase
    {
        public class UnimplementedNonGenericDbSet : DbSet
        {
        }

        [Fact]
        public void Find_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().Find(1))
                .ValidateMessage("TestDoubleNotImplemented", "Find", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Add_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().Add(new Product()))
                .ValidateMessage("TestDoubleNotImplemented", "Add", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void AddRange_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().AddRange(new[] { new Product() }))
                .ValidateMessage("TestDoubleNotImplemented", "AddRange", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Attach_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().Attach(new Product()))
                .ValidateMessage("TestDoubleNotImplemented", "Attach", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Remove_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().Remove(new Product()))
                .ValidateMessage("TestDoubleNotImplemented", "Remove", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void RemoveRange_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().RemoveRange(new[] { new Product() }))
                .ValidateMessage("TestDoubleNotImplemented", "RemoveRange", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Getting_results_from_SqlQuery_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().SqlQuery("not a real query").ToList<Product>())
                .ValidateMessage("TestDoubleNotImplemented", "GetEnumerator", typeof(DbSqlQuery).Name, typeof(DbSqlQuery).Name);
        }

#pragma warning disable 612, 618
        [Fact]
        public void Getting_results_from_SqlQuery_with_AsStreaming_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(
                () => new UnimplementedNonGenericDbSet().SqlQuery("not a real query").AsStreaming().ToList<Product>())
                .ValidateMessage("TestDoubleNotImplemented", "GetEnumerator", typeof(DbSqlQuery).Name, typeof(DbSqlQuery).Name);
        }
#pragma warning restore 612, 618

        [Fact]
        public void Getting_results_from_SqlQuery_with_NoTracking_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(
                () => new UnimplementedNonGenericDbSet().SqlQuery("not a real query").AsNoTracking().ToList<Product>())
                .ValidateMessage("TestDoubleNotImplemented", "GetEnumerator", typeof(DbSqlQuery).Name, typeof(DbSqlQuery).Name);
        }

        [Fact]
        public void Create_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().Create())
                .ValidateMessage("TestDoubleNotImplemented", "Create", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Generic_Create_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().Create(typeof(FeaturedProduct)))
                .ValidateMessage("TestDoubleNotImplemented", "Create", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Local_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().Local)
                .ValidateMessage("TestDoubleNotImplemented", "Local", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Getting_results_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedNonGenericDbSet().ToList<Product>())
                .ValidateMessage(
                    "TestDoubleNotImplemented", "IEnumerable.GetEnumerator", typeof(UnimplementedNonGenericDbSet).Name,
                    typeof(DbSet).Name);
        }

        [Fact]
        public void Unimplemented_DbSet_does_not_throw_when_ToString_is_used()
        {
            Assert.Equal(typeof(UnimplementedNonGenericDbSet).ToString(), new UnimplementedNonGenericDbSet().ToString());
        }

        [Fact]
        public void Unimplemented_DbSqlQuery_does_not_throw_when_ToString_is_used()
        {
            Assert.Equal(typeof(DbSqlQuery).ToString(), new UnimplementedNonGenericDbSet().SqlQuery("Foo").ToString());
        }

        [Fact]
        public void Actual_Include_method_is_noop_for_unimplemented_DbSet()
        {
            var set = new UnimplementedNonGenericDbSet();
            Assert.Same(set, set.Include("Category"));
        }

        [Fact]
        public void Include_extension_method_is_noop_for_unimplemented_DbSet()
        {
            IQueryable set = new UnimplementedNonGenericDbSet();
            Assert.Same(set, set.Include("Category"));
        }

        [Fact]
        public void Actual_AsNoTracking_method_is_noop_for_unimplemented_DbSet()
        {
            var set = new UnimplementedNonGenericDbSet();
            Assert.Same(set, set.AsNoTracking());
        }

        [Fact]
        public void AsNoTracking_extension_method_is_noop_for_unimplemented_DbSet()
        {
            IQueryable set = new UnimplementedNonGenericDbSet();
            Assert.Same(set, set.AsNoTracking());
        }

#pragma warning disable 612, 618
        [Fact]
        public void Actual_AsStreaming_method_is_noop_for_unimplemented_DbSet()
        {
            var set = new UnimplementedNonGenericDbSet();
            Assert.Same(set, set.AsStreaming());
        }

        [Fact]
        public void AsStreaming_extension_method_is_noop_for_unimplemented_DbSet()
        {
            IQueryable set = new UnimplementedNonGenericDbSet();
            Assert.Same(set, set.AsStreaming());
        }

        [Fact]
        public void AsStreaming_is_noop_for_unimplemented_DbSqlQuery()
        {
            var query = new UnimplementedNonGenericDbSet().SqlQuery("not a real query");
            Assert.Same(query, query.AsStreaming());
        }
#pragma warning restore 612, 618

        [Fact]
        public void AsNoTracking_is_noop_for_unimplemented_DbSqlQuery()
        {
            var query = new UnimplementedNonGenericDbSet().SqlQuery("not a real query");
            Assert.Same(query, query.AsNoTracking());
        }

        [Fact]
        public void ContainsListCollection_returns_false_for_unimplemented_DbSqlQuery()
        {
            Assert.False(((IListSource)new UnimplementedNonGenericDbSet().SqlQuery("not a real query")).ContainsListCollection);
        }

        [Fact]
        public void GetList_throws_as_normal_for_unimplemented_DbSqlQuery()
        {
            var query = new UnimplementedNonGenericDbSet().SqlQuery("not a real query");
            Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList())
                .ValidateMessage("DbQuery_BindingToDbQueryNotSupported");
        }

        [Fact]
        public void ContainsListCollection_returns_false_for_unimplemented_DbSet()
        {
            Assert.False(((IListSource)new UnimplementedNonGenericDbSet()).ContainsListCollection);
        }

        [Fact]
        public void GetList_throws_as_normal_for_unimplemented_DbSet()
        {
            Assert.Throws<NotSupportedException>(() => ((IListSource)new UnimplementedNonGenericDbSet()).GetList())
                .ValidateMessage("DbQuery_BindingToDbQueryNotSupported");
        }

        [Fact]
        public void Conversion_from_non_generic_to_generic_throws()
        {
            Assert.Throws<NotSupportedException>(() => new UnimplementedNonGenericDbSet().Cast<Product>())
                .ValidateMessage("TestDoublesCannotBeConverted");
        }

#if !NET40
        [Fact]
        public async Task FindAsync_throws_for_unimplemented_DbSet()
        {
            (await Assert.ThrowsAsync<NotImplementedException>(() => new UnimplementedNonGenericDbSet().FindAsync(1)))
                .ValidateMessage("TestDoubleNotImplemented", "FindAsync", typeof(UnimplementedNonGenericDbSet).Name, typeof(DbSet).Name);
        }

        [Fact]
        public void Getting_async_results_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<AggregateException>(() => new UnimplementedNonGenericDbSet().ToListAsync().Result)
                .InnerException
                .ValidateMessage(
                    "TestDoubleNotImplemented", "IDbAsyncEnumerable.GetAsyncEnumerator", typeof(UnimplementedNonGenericDbSet).Name,
                    typeof(DbSet).Name);
        }

        [Fact]
        public void Getting_results_from_async_SqlQuery_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<AggregateException>(() => new UnimplementedNonGenericDbSet().SqlQuery("not a real query").ToListAsync().Result)
                .InnerException
                .ValidateMessage(
                    "TestDoubleNotImplemented", "IDbAsyncEnumerable.GetAsyncEnumerator", typeof(DbSqlQuery).Name,
                    typeof(DbSqlQuery).Name);
        }
#endif
    }
}
