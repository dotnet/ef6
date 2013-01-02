// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Xunit;

    public class LeftOuterJoinEliminationContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentDetail> DocumentDetails { get; set; }
    }

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

    public class JoinEliminationTests
    {
        [Fact]
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

            using (var context = new LeftOuterJoinEliminationContext())
            {
                var query = context.DocumentDetails.Where(x => x.Document.Customer.PersonId == 1 || x.Document.Customer.PersonId == 2);
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }
    }
}
