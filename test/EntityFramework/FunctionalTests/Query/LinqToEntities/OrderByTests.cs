// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class OrderByTests : FunctionalTestBase
    {
        public class CodePlex2274 : FunctionalTestBase
        {
            public class MyContext : DbContext
            {
                static MyContext()
                {
                    Database.SetInitializer<MyContext>(null);
                }

                public DbSet<A> As { get; set; }
            }

            public class A
            {
                public A()
                {
                    Bs = new List<B>();
                }

                public int Id { get; set; }

                public ICollection<B> Bs { get; set; }
            }

            public class B
            {
                public int Id { get; set; }
                public int P1 { get; set; }
                public string P2 { get; set; }
            }

            [Fact]
            public void Generated_SQL_for_query_with_nested_FirstOrDefault_in_filter_honors_OrderBy()
            {
                using (var context = new MyContext())
                {
                    var query =
                        from a in context.As
                        where
                            a.Id == 1
                            && a.Bs.OrderBy(b => b.P1).FirstOrDefault(b => b.P2 == "") != null
                        select a;

                    QueryTestHelpers.VerifyQuery(
                        query,
@"SELECT 
    [Extent1].[Id] AS [Id]
    FROM  [dbo].[A] AS [Extent1]
    CROSS APPLY  (SELECT TOP (1) [Project1].[Id] AS [Id]
        FROM ( SELECT 
            [Extent2].[Id] AS [Id], 
            [Extent2].[P1] AS [P1]
            FROM [dbo].[B] AS [Extent2]
            WHERE ([Extent1].[Id] = [Extent2].[A_Id]) AND (N'' = [Extent2].[P2])
        )  AS [Project1]
        ORDER BY [Project1].[P1] ASC ) AS [Limit1]
    WHERE (1 = [Extent1].[Id]) AND ([Limit1].[Id] IS NOT NULL)");
                }               
            }

            [Fact]
            public void Generated_SQL_for_query_with_nested_FirstOrDefault_in_subquery_honors_OrderBy()
            {
                using (var context = new MyContext())
                {
                    var query =
                        from a in context.As
                        select new
                        {
                            A = a,
                            B = a.Bs.OrderBy(b => b.P1).FirstOrDefault(b => b.P2 == "")
                        };

                    QueryTestHelpers.VerifyQuery(
                        query,
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Limit1].[Id] AS [Id1], 
    [Limit1].[P1] AS [P1], 
    [Limit1].[P2] AS [P2], 
    [Limit1].[A_Id] AS [A_Id]
    FROM  [dbo].[A] AS [Extent1]
    OUTER APPLY  (SELECT TOP (1) [Project1].[Id] AS [Id], [Project1].[P1] AS [P1], [Project1].[P2] AS [P2], [Project1].[A_Id] AS [A_Id]
        FROM ( SELECT 
            [Extent2].[Id] AS [Id], 
            [Extent2].[P1] AS [P1], 
            [Extent2].[P2] AS [P2], 
            [Extent2].[A_Id] AS [A_Id]
            FROM [dbo].[B] AS [Extent2]
            WHERE ([Extent1].[Id] = [Extent2].[A_Id]) AND (N'' = [Extent2].[P2])
        )  AS [Project1]
        ORDER BY [Project1].[P1] ASC ) AS [Limit1]");
                }
            }
        }
    }
}
