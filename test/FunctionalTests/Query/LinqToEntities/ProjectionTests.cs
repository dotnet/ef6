// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Collections.Generic;
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

        [Fact] // CodePlex #2595
        public void Projecting_collection_of_DTOs_inside_DTO_is_null_comparable()
        {
            var expectedSql =
@"SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[Id] AS [Id], 
    [Project1].[C2] AS [C2], 
    [Project1].[Reference1_Id] AS [Reference1_Id], 
    [Project1].[C3] AS [C3], 
    [Project1].[Reference2_Id] AS [Reference2_Id], 
    [Project1].[C4] AS [C4], 
    [Project1].[Id1] AS [Id1]
    FROM ( SELECT 
        [Extent1].[Id] AS [Id], 
        [Extent1].[Reference1_Id] AS [Reference1_Id], 
        [Extent1].[Reference2_Id] AS [Reference2_Id], 
        1 AS [C1], 
        cast(0 as bit) AS [C2], 
        cast(0 as bit) AS [C3], 
        [Extent2].[Id] AS [Id1], 
        CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C4]
        FROM  [dbo].[Entity1] AS [Extent1]
        LEFT OUTER JOIN [dbo].[Entity3] AS [Extent2] ON [Extent2].[Entity2_Id] = [Extent1].[Reference1_Id]
    )  AS [Project1]
    ORDER BY [Project1].[Id] ASC, [Project1].[C4] ASC";

            using (var ctx = new Issue2595Context())
            {
                ctx.Database.Delete();

                var entity1 = new Entity1
                {
                    Id = "1",
                    Reference1 = new Entity2
                    {
                        Id = "1.1",
                        Collection = new List<Entity3>()
                        {
                            new Entity3 { Id = "1.1.1" },
                            new Entity3 { Id = "1.1.2" }
                        }
                    },
                    Reference2 = new Entity4 { Id = "1.2" }
                };

                var entity2 = new Entity1 { Id = "2", Reference1 = null, Reference2 = new Entity4 { Id = "2.2" } };
                var entity3 = new Entity1 { Id = "3", Reference1 = null, Reference2 = null };

                ctx.Entities1.Add(entity1);
                ctx.Entities1.Add(entity2);
                ctx.Entities1.Add(entity3);
                ctx.SaveChanges();
            }

            using (var ctx = new Issue2595Context())
            {
                var query = ctx.Entities1.Select(e => new DTO1
                {
                    Id = e.Id,
                    Reference1 = new DTO2
                    {
                        Id = e.Reference1.Id,
                        Collection = e.Reference1.Collection.Select(c => new DTO3 { Id = c.Id })
                    },
                    Reference2 = new DTO4 {  Id = e.Reference2.Id }
                }).Select(r => new { r.Id, IsNull1 = r.Reference1 == null, r.Reference1, IsNull2 = r.Reference2 == null, r.Reference2 });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
                var results = query.ToList();

                Assert.Equal(3, results.Count);
                Assert.True(results.Select(r => r.IsNull1).All(r => !r));
                Assert.True(results.Select(r => r.IsNull2).All(r => !r));
                Assert.True(results.Select(r => r.Reference1).All(r => r != null));
                Assert.True(results.Select(r => r.Reference2).All(r => r != null));
            }
        }

        [Fact] // CodePlex #2595
        public void Projecting_collection_inside_entity_is_null_comparable()
        {
            var expectedSql =
@"SELECT 
    1 AS [C1], 
    [Extent1].[Id] AS [Id], 
    CASE WHEN ([Extent1].[Reference1_Id] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C2], 
    [Extent1].[Reference1_Id] AS [Reference1_Id], 
    CASE WHEN ([Extent1].[Reference2_Id] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END AS [C3], 
    [Extent1].[Reference2_Id] AS [Reference2_Id]
    FROM [dbo].[Entity1] AS [Extent1]";

            using (var ctx = new Issue2595Context())
            {
                ctx.Database.Delete();

                var entity1 = new Entity1
                {
                    Id = "1",
                    Reference1 = new Entity2
                    {
                        Id = "1.1",
                        Collection = new List<Entity3>()
                        {
                            new Entity3 { Id = "1.1.1" },
                            new Entity3 { Id = "1.1.2" }
                        }
                    },
                    Reference2 = new Entity4 { Id = "1.2" }
                };

                var entity2 = new Entity1 { Id = "2", Reference1 = null, Reference2 = new Entity4 { Id = "2.2" } };
                var entity3 = new Entity1 { Id = "3", Reference1 = null, Reference2 = null };

                ctx.Entities1.Add(entity1);
                ctx.Entities1.Add(entity2);
                ctx.Entities1.Add(entity3);
                ctx.SaveChanges();
            }

            using (var ctx = new Issue2595Context())
            {
                var query = ctx.Entities1.Include(e => e.Reference1).Include("Reference.Collection").Include(e => e.Reference2)
                    .Select(r => new { r.Id, IsNull1 = r.Reference1 == null, r.Reference1, IsNull2 = r.Reference2 == null, r.Reference2 });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        public class Entity1
        {
            public string Id { get; set; }
            public Entity2 Reference1 { get; set; }
            public Entity4 Reference2 { get; set; }
        }

        public class Entity2
        {
            public string Id { get; set; }
            public ICollection<Entity3> Collection { get; set; }
        }

        public class Entity3
        {
            public string Id { get; set; }
        }

        public class Entity4
        {
            public string Id { get; set; }
        }

        public class DTO1
        {
            public string Id { get; set; }
            public DTO2 Reference1 { get; set; }
            public DTO4 Reference2 { get; set; }
        }

        public class DTO2
        {
            public string Id { get; set; }
            public IEnumerable<DTO3> Collection { get; set; }
        }

        public class DTO3
        {
            public string Id { get; set; }
        }

        public class DTO4
        {
            public string Id { get; set; }
        }

        public class Issue2595Context : DbContext
        {
            public DbSet<Entity1> Entities1 { get; set; }
        }
    }
}
