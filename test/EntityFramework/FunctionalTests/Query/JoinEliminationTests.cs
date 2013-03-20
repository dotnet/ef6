// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
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
                context.Configuration.UseCSharpNullComparisonBehavior = false;

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
                context.Configuration.UseCSharpNullComparisonBehavior = false;

                var query = from p in context.Posts where p.Id == 1 select new { p.Id, p.Text, p.ParentPost.Blog.Name };

                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }
    }
}
