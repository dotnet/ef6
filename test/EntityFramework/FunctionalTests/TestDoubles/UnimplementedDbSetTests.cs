// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.ComponentModel;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SimpleModel;
    using Xunit;

    public class UnimplementedDbSetTests : FunctionalTestBase
    {
        public class UnimplementedDbSet<T> : DbSet<T>
            where T : class
        {
        }

        [Fact]
        public void Find_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Find(1))
                .ValidateMessage("TestDoubleNotImplemented", "Find", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Add_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Add(new Product()))
                .ValidateMessage("TestDoubleNotImplemented", "Add", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void AddRange_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().AddRange(new[] { new Product() }))
                .ValidateMessage("TestDoubleNotImplemented", "AddRange", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Attach_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Attach(new Product()))
                .ValidateMessage("TestDoubleNotImplemented", "Attach", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Remove_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Remove(new Product()))
                .ValidateMessage("TestDoubleNotImplemented", "Remove", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void RemoveRange_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().RemoveRange(new[] { new Product() }))
                .ValidateMessage("TestDoubleNotImplemented", "RemoveRange", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void AddOrUpdate_throws_if_DbSet_does_not_contain_AddOrUpdate_method()
        {
            Assert.Throws<InvalidOperationException>(() => new UnimplementedDbSet<Product>().AddOrUpdate(new[] { new Product() }))
                .ValidateMessage("UnableToDispatchAddOrUpdate", typeof(UnimplementedDbSet<Product>));
        }

        [Fact]
        public void AddOrUpdate_with_expression_throws_if_DbSet_does_not_contain_AddOrUpdate_method()
        {
            Assert.Throws<InvalidOperationException>(
                () => new UnimplementedDbSet<Product>().AddOrUpdate(e => e.Id, new[] { new Product() }))
                .ValidateMessage("UnableToDispatchAddOrUpdate", typeof(UnimplementedDbSet<Product>));
        }

        [Fact]
        public void Getting_results_from_SqlQuery_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().SqlQuery("not a real query").ToList())
                .ValidateMessage("TestDoubleNotImplemented", "GetEnumerator", typeof(DbSqlQuery<Product>).Name, typeof(DbSqlQuery<>).Name);
        }

        [Fact]
        public void Getting_results_from_SqlQuery_with_AsStreaming_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(
                () => new UnimplementedDbSet<Product>().SqlQuery("not a real query").AsStreaming().ToList())
                .ValidateMessage("TestDoubleNotImplemented", "GetEnumerator", typeof(DbSqlQuery<Product>).Name, typeof(DbSqlQuery<>).Name);
        }

        [Fact]
        public void Getting_results_from_SqlQuery_with_NoTracking_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(
                () => new UnimplementedDbSet<Product>().SqlQuery("not a real query").AsNoTracking().ToList())
                .ValidateMessage("TestDoubleNotImplemented", "GetEnumerator", typeof(DbSqlQuery<Product>).Name, typeof(DbSqlQuery<>).Name);
        }

        [Fact]
        public void Create_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Create())
                .ValidateMessage("TestDoubleNotImplemented", "Create", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Generic_Create_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Create<FeaturedProduct>())
                .ValidateMessage("TestDoubleNotImplemented", "Create", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Local_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Local.Select(p => p.Name))
                .ValidateMessage("TestDoubleNotImplemented", "Local", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Getting_results_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().ToList())
                .ValidateMessage("TestDoubleNotImplemented", "IEnumerable<TResult>.GetEnumerator", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Unimplemented_DbSet_does_not_throw_when_ToString_is_used()
        {
            Assert.Equal(typeof(UnimplementedDbSet<Product>).ToString(), new UnimplementedDbSet<Product>().ToString());
        }

        [Fact]
        public void Unimplemented_DbSqlQuery_does_not_throw_when_ToString_is_used()
        {
            Assert.Equal(typeof(DbSqlQuery<Product>).ToString(), new UnimplementedDbSet<Product>().SqlQuery("Foo").ToString());
        }

        [Fact]
        public void LINQ_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().Where(p => p.Name.Contains("e")))
                .ValidateMessage("TestDoubleNotImplemented", "IQueryable.Provider", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Actual_Include_method_is_noop_for_unimplemented_DbSet()
        {
            var set = new UnimplementedDbSet<Product>();
            Assert.Same(set, set.Include(p => p.Category));
        }

        [Fact]
        public void Include_extension_method_is_noop_for_unimplemented_DbSet()
        {
            IQueryable<Product> set = new UnimplementedDbSet<Product>();
            Assert.Same(set, set.Include(p => p.Category));
        }

        [Fact]
        public void Actual_AsNoTracking_method_is_noop_for_unimplemented_DbSet()
        {
            var set = new UnimplementedDbSet<Product>();
            Assert.Same(set, set.AsNoTracking());
        }

        [Fact]
        public void AsNoTracking_extension_method_is_noop_for_unimplemented_DbSet()
        {
            IQueryable<Product> set = new UnimplementedDbSet<Product>();
            Assert.Same(set, set.AsNoTracking());
        }

        [Fact]
        public void Actual_AsStreaming_method_is_noop_for_unimplemented_DbSet()
        {
            var set = new UnimplementedDbSet<Product>();
            Assert.Same(set, set.AsStreaming());
        }

        [Fact]
        public void AsStreaming_extension_method_is_noop_for_unimplemented_DbSet()
        {
            IQueryable<Product> set = new UnimplementedDbSet<Product>();
            Assert.Same(set, set.AsStreaming());
        }

        [Fact]
        public void AsStreaming_is_noop_for_unimplemented_DbSqlQuery()
        {
            var query = new UnimplementedDbSet<Product>().SqlQuery("not a real query");
            Assert.Same(query, query.AsStreaming());
        }

        [Fact]
        public void AsNoTracking_is_noop_for_unimplemented_DbSqlQuery()
        {
            var query = new UnimplementedDbSet<Product>().SqlQuery("not a real query");
            Assert.Same(query, query.AsNoTracking());
        }

        [Fact]
        public void ContainsListCollection_returns_false_for_unimplemented_DbSqlQuery()
        {
            Assert.False(((IListSource)new UnimplementedDbSet<Product>().SqlQuery("not a real query")).ContainsListCollection);
        }

        [Fact]
        public void GetList_throws_as_normal_for_unimplemented_DbSqlQuery()
        {
            var query = new UnimplementedDbSet<Product>().SqlQuery("not a real query");
            Assert.Throws<NotSupportedException>(() => ((IListSource)query).GetList())
                .ValidateMessage("DbQuery_BindingToDbQueryNotSupported");
        }

        [Fact]
        public void ContainsListCollection_returns_false_for_unimplemented_DbSet()
        {
            Assert.False(((IListSource)new UnimplementedDbSet<Product>()).ContainsListCollection);
        }

        [Fact]
        public void GetList_throws_as_normal_for_unimplemented_DbSet()
        {
            Assert.Throws<NotSupportedException>(() => ((IListSource)new UnimplementedDbSet<Product>()).GetList())
                .ValidateMessage("DbQuery_BindingToDbQueryNotSupported");
        }

        [Fact]
        public void Conversion_from_generic_to_non_generic_throws()
        {
            Assert.Throws<NotSupportedException>(() => (DbSet)new UnimplementedDbSet<Product>())
                .ValidateMessage("TestDoublesCannotBeConverted");
        }

#if !NET40
        [Fact]
        public void FindAsync_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().FindAsync(1))
                .ValidateMessage("TestDoubleNotImplemented", "FindAsync", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Getting_async_results_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().ToListAsync())
                .ValidateMessage("TestDoubleNotImplemented", "IDbAsyncEnumerable<TResult>.GetAsyncEnumerator", typeof(UnimplementedDbSet<Product>).Name, typeof(DbSet<>).Name);
        }

        [Fact]
        public void Getting_results_from_async_SqlQuery_throws_for_unimplemented_DbSet()
        {
            Assert.Throws<NotImplementedException>(() => new UnimplementedDbSet<Product>().SqlQuery("not a real query").ToListAsync())
                .ValidateMessage("TestDoubleNotImplemented", "IDbAsyncEnumerable<TElement>.GetAsyncEnumerator", typeof(DbSqlQuery<Product>).Name, typeof(DbSqlQuery<>).Name);
        }
#endif
    }
}
