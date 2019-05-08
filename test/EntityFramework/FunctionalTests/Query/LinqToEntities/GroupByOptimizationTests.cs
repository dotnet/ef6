// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.Core;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;

    public class GroupByOptimizationTests : FunctionalTestBase
    {
        [Fact]
        public void GroupBy_is_optimized_when_projecting_group_key()
        {
            var expectedSql =
@"SELECT 
[Distinct1].[FirstName] AS [FirstName]
FROM ( SELECT DISTINCT 
	[Extent1].[FirstName] AS [FirstName]
	FROM [dbo].[ArubaOwners] AS [Extent1]
)  AS [Distinct1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.FirstName).Select(g => g.Key);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.FirstName).Select(g => g.Key).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);

            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_group_count()
        {
            var expectedSql =
@"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[FirstName] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	GROUP BY [Extent1].[FirstName]
)  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.FirstName).Select(g => g.Count());
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.FirstName).Select(g => g.Count()).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_expression_containing_group_key()
        {
            var expectedSql =
@"SELECT 
[Extent1].[Id] * 2 AS [C1]
FROM [dbo].[ArubaOwners] AS [Extent1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.Id).Select(g => g.Key * 2);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.Id).Select(g => g.Key * 2).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_aggregate_on_the_group()
        {
            var expectedSql =
@"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[FirstName] AS [K1], 
	MAX([Extent1].[Id]) AS [A1]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	GROUP BY [Extent1].[FirstName]
)  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.FirstName).Select(g => g.Max(p => p.Id));
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.FirstName).Select(g => g.Max(p => p.Id)).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_group_aggregate()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [FirstName], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[FirstName] AS [K1], 
	MAX([Extent1].[Id]) AS [A1]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	GROUP BY [Extent1].[FirstName]
)  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.FirstName).Select(g => new { Key = g.Key, Aggregate = g.Max(p => p.Id) });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.FirstName).Select(g => new { Key = g.Key, Aggregate = g.Max(p => p.Id) }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Key == i.Key && o.Aggregate == i.Aggregate);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_multiple_group_aggregates()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [FirstName], 
[GroupBy1].[A1] AS [C2], 
[GroupBy1].[A2] AS [C3]
FROM ( SELECT 
	[Extent1].[K1] AS [K1], 
	MAX([Extent1].[A1]) AS [A1], 
	MIN([Extent1].[A2]) AS [A2]
	FROM ( SELECT 
		[Extent1].[FirstName] AS [K1], 
		[Extent1].[Id] AS [A1], 
		[Extent1].[Id] + 2 AS [A2]
		FROM [dbo].[ArubaOwners] AS [Extent1]
	)  AS [Extent1]
	GROUP BY [K1]
)  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.FirstName).Select(g => new { key1 = g.Key, key2 = g.Key, max = g.Max(p => p.Id), min = g.Min(s => s.Id + 2) });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.FirstName).Select(g => new { key1 = g.Key, key2 = g.Key, max = g.Max(p => p.Id), min = g.Min(s => s.Id + 2) }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.key1 == i.key1 && o.key2 == i.key2 && o.max == i.max && o.min == i.min);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_conditional_expression_containing_group_key()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
CASE WHEN ([Distinct1].[FirstName] IS NULL) THEN N'is null' ELSE N'not null' END AS [C2], 
CASE WHEN (((@p__linq__0 = 1) AND (@p__linq__1 = 1)) OR ((@p__linq__2 = 1) AND (@p__linq__3 = 1))) THEN cast(1 as bit) WHEN ( NOT (((@p__linq__0 = 1) AND (@p__linq__1 = 1)) OR ((@p__linq__2 = 1) AND (@p__linq__3 = 1)))) THEN cast(0 as bit) END AS [C3]
FROM ( SELECT DISTINCT 
	[Extent1].[FirstName] AS [FirstName]
	FROM [dbo].[ArubaOwners] AS [Extent1]
)  AS [Distinct1]";

            bool a = true;
            bool b = false;
            bool c = true;

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.FirstName).Select(g => new { keyIsNull = g.Key == null ? "is null" : "not null", logicExpression = (a && b || b && c) });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.FirstName).Select(g => new { keyIsNull = g.Key == null ? "is null" : "not null", logicExpression = (a && b || b && c) }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.keyIsNull == i.keyIsNull && o.logicExpression == i.logicExpression);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_filerting_and_projecting_anonymous_type_with_group_key_and_function_aggregate()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [FirstName], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[FirstName] AS [K1], 
	AVG( CAST( [Extent1].[Id] AS float)) AS [A1]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	WHERE [Extent1].[Id] > 5
	GROUP BY [Extent1].[FirstName]
)  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.Where(o => o.Id > 5).GroupBy(o => o.FirstName).Select(g => new { FirstName = g.Key, AverageId = g.Average(p => p.Id) });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList().OrderBy(r => r.AverageId).ToList();
                var expected = context.Owners.ToList().Where(o => o.Id > 5).GroupBy(o => o.FirstName).Select(g => new { FirstName = g.Key, AverageId = g.Average(p => p.Id) }).OrderBy(r => r.AverageId).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.FirstName == i.FirstName && o.AverageId == i.AverageId);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_function_aggregate_with_expression()
        {
            var expectedSql =
@"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[K1] AS [K1], 
	MAX([Extent1].[A1]) AS [A1]
	FROM ( SELECT 
		[Extent1].[FirstName] AS [K1], 
		[Extent1].[Id] * 2 AS [A1]
		FROM [dbo].[ArubaOwners] AS [Extent1]
	)  AS [Extent1]
	GROUP BY [K1]
)  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(p => p.FirstName).Select(g => g.Max(p => p.Id * 2));
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(p => p.FirstName).Select(g => g.Max(p => p.Id * 2)).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_projecting_expression_with_multiple_function_aggregates()
        {
            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[A1] - [GroupBy1].[A2] AS [C2]
FROM ( SELECT 
	[Extent1].[FirstName] AS [K1], 
	MAX([Extent1].[Id]) AS [A1], 
	MIN([Extent1].[Id]) AS [A2]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	GROUP BY [Extent1].[FirstName]
)  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o.FirstName).Select(g => new { maxMinusMin = g.Max(p => p.Id) - g.Min(s => s.Id) });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o.FirstName).Select(g => new { maxMinusMin = g.Max(p => p.Id) - g.Min(s => s.Id) }).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.maxMinusMin == i.maxMinusMin);
            }
        }

        [Fact]
        public void GroupBy_is_optimized_when_grouping_by_row_and_projecting_column_of_the_key_row()
        {
            var expectedSql =
@"SELECT 
[Distinct1].[FirstName] AS [FirstName]
FROM ( SELECT DISTINCT 
	[Extent1].[FirstName] AS [FirstName]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	WHERE [Extent1].[Id] < 4
)  AS [Distinct1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.Where(o => o.Id < 4).GroupBy(g => new { g.FirstName }).Select(g => g.Key.FirstName);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().Where(o => o.Id < 4).GroupBy(g => new { g.FirstName }).Select(g => g.Key.FirstName).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);
            }
        }

        [Fact]
        public void Grouping_by_all_columns_doesnt_produce_a_groupby_statement()
        {
            var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias]
    FROM [dbo].[ArubaOwners] AS [Extent1]";

            using (var context = new ArubaContext())
            {
                var query = context.Owners.GroupBy(o => o).Select(g => g.Key);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o).Select(g => g.Key).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);

            }
        }

        [Fact]
        public void Grouping_by_all_columns_with_aggregate_function_works()
        {
            var expectedSqlA =
@"SELECT 
    (SELECT 
        COUNT(1) AS [A1]
        FROM [dbo].[ArubaOwners] AS [Extent2]
        WHERE [Extent1].[Id] = [Extent2].[Id]) AS [C1]
    FROM [dbo].[ArubaOwners] AS [Extent1]";

            var expectedSqlB =
@"SELECT 
    [GroupBy1].[K1] AS [Id], 
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        [Extent1].[Id] AS [K1], 
        [Extent1].[FirstName] AS [K2], 
        [Extent1].[LastName] AS [K3], 
        [Extent1].[Alias] AS [K4], 
        COUNT(1) AS [A1]
        FROM [dbo].[ArubaOwners] AS [Extent1]
        GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
    )  AS [GroupBy1]";

            var expectedSqlC =
@"SELECT 
    [GroupBy1].[K1] AS [Id], 
    [GroupBy1].[K4] AS [Alias],
    [GroupBy1].[A1] AS [C1]
    FROM ( SELECT 
        [Extent1].[Id] AS [K1], 
        [Extent1].[FirstName] AS [K2], 
        [Extent1].[LastName] AS [K3], 
        [Extent1].[Alias] AS [K4], 
        COUNT(1) AS [A1]
        FROM [dbo].[ArubaOwners] AS [Extent1]
        GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
    )  AS [GroupBy1]";

            var expectedSqlD =
@"SELECT 
    [GroupBy1].[K1] AS [Id], 
    [GroupBy1].[A1] AS [C1], 
    [GroupBy1].[A2] AS [C2]
    FROM ( SELECT 
        [Extent1].[Id] AS [K1], 
        [Extent1].[FirstName] AS [K2], 
        [Extent1].[LastName] AS [K3], 
        [Extent1].[Alias] AS [K4], 
        SUM([Extent1].[Id]) AS [A1], 
        COUNT(1) AS [A2]
        FROM [dbo].[ArubaOwners] AS [Extent1]
        GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
    )  AS [GroupBy1]";

            using (var context = new ArubaContext())
            {
                var query1 = context.Owners.GroupBy(o => new { o.Id, o.FirstName, o.LastName, o.Alias }, c => new { c.LastName, c.FirstName }, (k, g) => g.Count());
                QueryTestHelpers.VerifyDbQuery(query1, expectedSqlA);

                query1 = context.Owners.GroupBy(o => o, c => new { c.LastName, c.FirstName }, (k, g) => g.Count());
                QueryTestHelpers.VerifyDbQuery(query1, expectedSqlA);

                query1 = context.Owners.GroupBy(o => o, c => c, (k, g) => g.Count());
                QueryTestHelpers.VerifyDbQuery(query1, expectedSqlA);

                var results = query1.ToList();
                var expected = context.Owners.ToList().GroupBy(o => o, c => c, (k, g) => g.Count()).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o == i);

                var query2 = context.Owners.GroupBy(o => o, c => c, (k, g) => new { Count = g.Count() });
                QueryTestHelpers.VerifyDbQuery(query2, expectedSqlB);

                var query3 = context.Owners.GroupBy(o => o, c => c, (k, g) => new { Id = k.Id, Count = g.Count() });
                QueryTestHelpers.VerifyDbQuery(query3, expectedSqlB);

                var query4 = context.Owners.GroupBy(o => o, c => c, (k, g) => new { Id = k.Id, Alias = k.Alias, Count = g.Count() });
                QueryTestHelpers.VerifyDbQuery(query4, expectedSqlC);

                var query5 = from o in context.Owners
                             group o by o into g
                             select g.Count();
                QueryTestHelpers.VerifyDbQuery(query5, expectedSqlA);

                var query6 = from o in context.Owners
                             group o by o into g
                             select new { Id = g.Key.Id, Count = g.Count() };
                QueryTestHelpers.VerifyDbQuery(query6, expectedSqlB);

                var query7 = from o in context.Owners
                             group o by o into g
                             select new { Id = g.Key.Id, Alias = g.Key.Alias, Count = g.Count() };
                QueryTestHelpers.VerifyDbQuery(query7, expectedSqlC);

                var query8 = from o in context.Owners
                             group o by o into g
                             select new { Id = g.Key.Id, Sum = g.Sum(x => x.Id), Count = g.Count() };
                QueryTestHelpers.VerifyDbQuery(query8, expectedSqlD);

            }
        }


        [Fact]
        public void GroupBy_with_aggregate_does_not_throw_for_types_incompatible_with_GroupBy()
        {
            using (var context = new ImageContext())
            {
                var query2 = context.Users.GroupBy(o => o, u => new { u.Image, u.Name }, (k, g) => g.Count());

                query2.ToList();
            }
        }

        [Fact]
        public void GroupBy_fails_when_image_column_is_included()
        {
            using (var context = new ImageContext())
            {
                var query0 = from u in context.Users
                    group u by u.Image
                    into g
                    select new { Image = g.Key };

                Assert.Throws<EntityCommandExecutionException>(() => query0.ToList());   
            }            
        }

        public class ImageContext : DbContext
        {
            public DbSet<User> Users { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<User>().Property(u => u.Image).HasColumnType("image");
            }
        }

        public class User
        {
            public int Id { get; set; }
            public byte[] Image { get; set; }
            public string Name { get; set; }
        }
    }
}
