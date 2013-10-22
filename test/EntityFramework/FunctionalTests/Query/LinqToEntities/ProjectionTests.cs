// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using SimpleModel;
    using Xunit;

    public class ProjectionTests : FunctionalTestBase
    {
        [Fact] // CodePlex 1751
        public void Joining_to_anonymous_type_projection_is_handled_correctly()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Select(p => new { p.Id });

                var query = context.Categories.Select(
                    c => c.Products
                             .Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Equal(4, query.ToList().Count);
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_is_handled_correctly()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Select(p => new MeIzNamed { Id = p.Id });

                var query = context.Categories.Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Equal(4, query.ToList().Count);
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_typed_as_DbQuery_is_handled_correctly()
        {
            using (var context = new SimpleModelContext())
            {
                var products = (DbQuery<MeIzNamed>)context.Products.Select(p => new MeIzNamed { Id = p.Id });

                var query = context.Categories.Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Equal(4, query.ToList().Count);
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_anonymous_type_projection_using_ObjectQuery_is_handled_correctly()
        {
            using (var context = new SimpleModelContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var products = objectContext.CreateObjectSet<Product>().Select(p => new { p.Id });

                var query = objectContext.CreateObjectSet<Category>().Select(
                    c => c.Products
                             .Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Equal(4, query.ToList().Count);
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_using_ObjectQuery_is_handled_correctly()
        {
            using (var context = new SimpleModelContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var products = objectContext.CreateObjectSet<Product>().Select(p => new MeIzNamed { Id = p.Id });

                var query =
                    objectContext.CreateObjectSet<Category>()
                        .Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Equal(4, query.ToList().Count);
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_typed_as_ObjectQuery_is_handled_correctly()
        {
            using (var context = new SimpleModelContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var products = (ObjectQuery<MeIzNamed>)objectContext.CreateObjectSet<Product>().Select(p => new MeIzNamed { Id = p.Id });

                var query =
                    objectContext.CreateObjectSet<Category>()
                        .Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Equal(4, query.ToList().Count);
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_anonymous_type_projection_of_wrong_IQueryable_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.ToList().Select(p => new { p.Id }).AsQueryable();

                var query = context.Categories.Select(
                    c => c.Products
                             .Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => query.ToString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "Anonymous type");
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_of_wrong_IQueryable_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.ToList().Select(p => new MeIzNamed { Id = p.Id }).AsQueryable();

                var query = context.Categories.Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => query.ToString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "System.Data.Entity.Query.LinqToEntities.ProjectionTests+MeIzNamed");
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_anonymous_type_projection_of_wrong_IQueryable_using_ObjectQuery_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var products = objectContext.CreateObjectSet<Product>().ToList().Select(p => new { p.Id }).AsQueryable();

                var query = objectContext.CreateObjectSet<Category>().Select(
                    c => c.Products
                             .Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => ((ObjectQuery)query).ToTraceString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "Anonymous type");
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_of_wrong_IQueryable_using_ObjectQuery_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var products = objectContext.CreateObjectSet<Product>().ToList().Select(p => new MeIzNamed { Id = p.Id }).AsQueryable();

                var query =
                    objectContext.CreateObjectSet<Category>()
                        .Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => ((ObjectQuery)query).ToTraceString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "System.Data.Entity.Query.LinqToEntities.ProjectionTests+MeIzNamed");
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_anonymous_type_projection_of_IEnumerable_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.ToList().Select(p => new { p.Id });

                var query = context.Categories.Select(
                    c => c.Products
                             .Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => query.ToString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "Anonymous type");
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_of_IEnumerable_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.ToList().Select(p => new MeIzNamed { Id = p.Id });

                var query = context.Categories.Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => query.ToString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "System.Data.Entity.Query.LinqToEntities.ProjectionTests+MeIzNamed");
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_anonymous_type_projection_of_IEnumerable_using_ObjectQuery_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var products = objectContext.CreateObjectSet<Product>().ToList().Select(p => new { p.Id });

                var query = objectContext.CreateObjectSet<Category>().Select(
                    c => c.Products
                             .Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => ((ObjectQuery)query).ToTraceString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "Anonymous type");
            }
        }

        [Fact] // CodePlex 1751
        public void Joining_to_named_type_projection_of_IEnumerable_using_ObjectQuery_throws()
        {
            using (var context = new SimpleModelContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;

                var products = objectContext.CreateObjectSet<Product>().ToList().Select(p => new MeIzNamed { Id = p.Id });

                var query =
                    objectContext.CreateObjectSet<Category>()
                        .Select(c => c.Products.Join(products, o => o.Id, i => i.Id, (o, i) => new { o, i }));

                Assert.Throws<NotSupportedException>(() => ((ObjectQuery)query).ToTraceString())
                    .ValidateMessage("ELinq_UnsupportedConstant", "System.Data.Entity.Query.LinqToEntities.ProjectionTests+MeIzNamed");
            }
        }

        public class MeIzNamed
        {
            public int Id { get; set; }
        }
    }
}
