// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Xunit;

    public class JoinEliminationTests : FunctionalTestBase
    {
        public class Codeplex655Context : DbContext
        {
            public DbSet<Customer> Customers { get; set; }
            public DbSet<Document> Documents { get; set; }
            public DbSet<DocumentDetail> DocumentDetails { get; set; }

            public class Customer
            {
                [Key]
                public int? CustomerId { get; set; }
                public int PersonId { get; set; }
            }

            public class Document
            {
                [Key]
                public int? DocumentId { get; set; }
                public int? CustomerId { get; set; }
                [ForeignKey("CustomerId")]
                public Customer Customer { get; set; }
            }

            public class DocumentDetail
            {
                [Key]
                public int? DocumentDetailId { get; set; }
                public int? DocumentId { get; set; }
                [ForeignKey("DocumentId")]
                public Document Document { get; set; }
            }
        }

        [Fact] // Codeplex #655
        public static void LeftOuterJoin_duplicates_are_eliminated()
        {
            const string expectedSql =
@"SELECT 
[Extent1].[DocumentDetailId] AS [DocumentDetailId], 
[Extent1].[DocumentId] AS [DocumentId] 
FROM [dbo].[DocumentDetails] AS [Extent1] 
LEFT OUTER JOIN [dbo].[Documents] AS [Extent2] ON [Extent1].[DocumentId] = [Extent2].[DocumentId] 
LEFT OUTER JOIN [dbo].[Customers] AS [Extent3] ON [Extent2].[CustomerId] = [Extent3].[CustomerId] 
WHERE [Extent3].[PersonId] IN (1,2)";

            Database.SetInitializer<Codeplex655Context>(null);

            using (var context = new Codeplex655Context())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = context.DocumentDetails.Where(x => x.Document.Customer.PersonId == 1 || x.Document.Customer.PersonId == 2);

                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        public class Codeplex960Context : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId { get; set; }
                public int? ParentPostId { get; set; }
                public string Text { get; set; }

                [ForeignKey("BlogId")]
                public virtual Blog Blog { get; set; }
                [ForeignKey("ParentPostId")]
                public virtual Post ParentPost { get; set; }
            }
        }

        [Fact] // Codeplex #960
        public static void LeftOuterJoin_is_not_turned_into_inner_join_if_nullable_foreign_key()
        {
            const string expectedSql = 
@"SELECT
[Extent1].[Id] AS [Id],
[Extent1].[Text] AS [Text],
[Extent3].[Name] AS [Name]
FROM   [dbo].[Posts] AS [Extent1]
LEFT OUTER JOIN [dbo].[Posts] AS [Extent2] ON [Extent1].[ParentPostId] = [Extent2].[Id]
LEFT OUTER JOIN [dbo].[Blogs] AS [Extent3] ON [Extent2].[BlogId] = [Extent3].[Id]
WHERE 1 = [Extent1].[Id]";

            Database.SetInitializer<Codeplex960Context>(null);

            using (var context = new Codeplex960Context())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = from p in context.Posts where p.Id == 1 select new { p.Id, p.Text, p.ParentPost.Blog.Name };

                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        public partial class Codeplex199Context : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<ProductModel> ProductModels { get; set; }
            public DbSet<String> Strings { get; set; }
            public DbSet<StringInstrument> StringInstruments { get; set; }

            public partial class Product
            {
                public int ProductID { get; set; }
                public string Name { get; set; }
                public Nullable<int> ProductModelID { get; set; }
                public virtual ProductModel ProductModel { get; set; }
            }

            public partial class ProductModel
            {
                public ProductModel()
                {
                    this.Products = new List<Product>();
                }

                public int ProductModelID { get; set; }
                public DateTime ModifiedDate { get; set; }
                public virtual ICollection<Product> Products { get; set; }
            }

            public partial class String
            {
                public int StringId { get; set; }
                public string Name { get; set; }
                public int StringInstrumentId { get; set; }
                public virtual StringInstrument StringInstrument { get; set; }
            }

            public partial class StringInstrument
            {
                public StringInstrument()
                {
                    this.Strings = new List<String>();
                }

                public int StringInstrumentId { get; set; }
                public DateTime ProductionDate { get; set; }
                public virtual ICollection<String> Strings { get; set; }
            }
        }

        [Fact]
        public static void Making_use_of_variable_multiple_times_doesnt_cause_redundant_joins()
        {
            const string expectedSqlProducts =
@"SELECT
    [Extent1].[Name] AS [Name]
    FROM   [dbo].[Products] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ProductModels] AS [Extent2] ON [Extent1].[ProductModelID] = [Extent2].[ProductModelID]
    WHERE ([Extent2].[ModifiedDate] >= @p__linq__0) AND ([Extent2].[ModifiedDate] <= @p__linq__1)";

            const string expectedSqlStrings = 
@"SELECT
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Strings] AS [Extent1]
    INNER JOIN [dbo].[StringInstruments] AS [Extent2] ON [Extent1].[StringInstrumentId] = [Extent2].[StringInstrumentId]
    WHERE ([Extent2].[ProductionDate] >= @p__linq__0) AND ([Extent2].[ProductionDate] <= @p__linq__1)";

            Database.SetInitializer<Codeplex199Context>(null);

            using (var context = new Codeplex199Context())
            {
                context.Configuration.UseDatabaseNullSemantics = true;
                context.Configuration.LazyLoadingEnabled = false;

                var MinDate = new DateTime(2011, 02, 03);
                var MaxDate = new DateTime(2011, 03, 04);

                var query = context.Products
                                   .Where(p => p.ProductModel.ModifiedDate >= MinDate
                                               && p.ProductModel.ModifiedDate <= MaxDate)
                                   .Select(p => p.Name);

                QueryTestHelpers.VerifyQuery(query, expectedSqlProducts);

                query = context.Strings
                               .Where(s => s.StringInstrument.ProductionDate >= MinDate
                                           && s.StringInstrument.ProductionDate <= MaxDate)
                               .Select(s => s.Name);

                QueryTestHelpers.VerifyQuery(query, expectedSqlStrings);
            }
        }
    }
}
