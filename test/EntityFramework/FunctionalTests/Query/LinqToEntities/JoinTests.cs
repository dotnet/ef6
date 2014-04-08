// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Xunit;    

    public class JoinTests
    {
        public class CodePlex2093 : FunctionalTestBase
        {
            public class A
            {
                [MaxLength(5)]
                public string Id { get; set; }
                public DateTime Date { get; set; }
            }

            public class B
            {
                [MaxLength(10)]
                public string Id { get; set; }
                public DateTime Date { get; set; }
            }

            public class Context : DbContext
            {
                static Context()
                {
                    Database.SetInitializer<Context>(null);
                }

                public DbSet<A> As { get; set; }
                public DbSet<B> Bs { get; set; }
            }

            [Fact]
            public void Join_with_anonymous_types_works_as_expected()
            {
                var expectedSql = 
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Date] AS [Date]
    FROM  [dbo].[A] AS [Extent1]
    INNER JOIN [dbo].[B] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[Id]) AND ([Extent1].[Date] = [Extent2].[Date])";

                using (var ctx = new Context())
                {
                    var query =
                        from a in ctx.As
                        join b in ctx.Bs
                            on new { a.Id, a.Date }
                            equals new { b.Id, b.Date }
                        select a;

                    QueryTestHelpers.VerifyQuery(query, expectedSql);
                }
            }
        }
    }
}
