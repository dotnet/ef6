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

        [Fact] // CodePlex 826
        public void Evaluating_if_projection_of_reference_is_null_is_handled_correctly()
        {
            var expectedSql =
@"SELECT 
    1 AS [C1], 
    CASE WHEN ([Extent2].[Id] IS NOT NULL) THEN 1 END AS [C2], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent2].[DetailedDescription] AS [DetailedDescription], 
    CASE WHEN ([Extent2].[Id] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C3]
    FROM  [dbo].[Products] AS [Extent1]
    LEFT OUTER JOIN [dbo].[Categories] AS [Extent2] ON [Extent1].[CategoryId] = [Extent2].[Id]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND ([Extent2].[Id] IS NOT NULL)";

            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Select(p =>
                    new
                    {
                        ProductId = p.Id,
                        Category = (p.Category != null) 
                            ? new { p.Category.Id, p.Category.DetailedDescription } 
                            : null
                    });

                var query = products
                    .Where(p => p.Category != null)
                    .Select(p => new { p.Category, CategoryIsNull = p.Category == null });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
                Assert.Equal(7, query.Count());

                var firstOrDefault = query.FirstOrDefault();

                Assert.False(firstOrDefault != null && firstOrDefault.CategoryIsNull);
            }
        }

        public class MeIzNamed
        {
            public int Id { get; set; }
        }
    }
}
