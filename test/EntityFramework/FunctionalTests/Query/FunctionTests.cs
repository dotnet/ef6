// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class FunctionTests
    {
        private static readonly MetadataWorkspace workspace = QueryTestHelpers.CreateMetadataWorkspace(ProductModel.csdl, ProductModel.ssdl, ProductModel.msl);

        [Fact]
        public void Inline_function_count_Products()
        {
            var query =
@"Function CountProducts(products Collection(ProductModel.Product)) as 
    (count(select value 1 from products))

select gkey, CountProducts(GroupPartition(P))
FROM ProductContainer.Products as P
Group By P.ProductName as gkey";

            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ProductName], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[ProductName] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ProductName]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Inline_functions_MaxProductId_and_MinProductId()
        {
            var query =
@"Function MinProductId(products Collection(ProductModel.Product)) as 
    (min(select value pp.ProductId from products as pp))
Function MaxProductId(products Collection(ProductModel.Product)) as 
    (max(select value pp.ProductId from products as pp))

select gkey, MinProductId(GroupPartition(P)), MaxProductId(GroupPartition(P))
FROM ProductContainer.Products as P
Group By P.ProductName as gkey";

            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ProductName], 
[GroupBy1].[A1] AS [C2], 
[GroupBy1].[A2] AS [C3]
FROM ( SELECT 
	[Extent1].[ProductName] AS [K1], 
	MIN([Extent1].[ProductID]) AS [A1], 
	MAX([Extent1].[ProductID]) AS [A2]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ProductName]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Inline_function_MaxMinProductId()
        {
            var query =
@"Function MaxMinProductId(products Collection(ProductModel.Product)) as 
(
    max(select value pp.ProductId from products as pp) - 
    min(select value pp.ProductId from products as pp)
)

select gkey, MaxMinProductId(GroupPartition(p))
FROM ProductContainer.Products as P
Group By P.ProductName as gkey";

            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ProductName], 
[GroupBy1].[A1] - [GroupBy1].[A2] AS [C2]
FROM ( SELECT 
	[Extent1].[ProductName] AS [K1], 
	MAX([Extent1].[ProductID]) AS [A1], 
	MIN([Extent1].[ProductID]) AS [A2]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ProductName]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Inline_function_one_level_above()
        {
            var query =
@"Function MaxProductId(products Collection(ProductModel.Product)) as 
(
    max(select value pp.ProductId from products as pp)
)

select i.gkey, (MaxProductId(i.groupProducts))
from (
    select gkey as gkey, GroupPartition(P) as groupProducts
    FROM ProductContainer.Products as P
    Group By P.ProductName as gkey
) as i";

            var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [ProductName], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[ProductName] AS [K1], 
	MAX([Extent1].[ProductID]) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	GROUP BY [Extent1].[ProductName]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Inline_function_MaxInt()
        {
            var query =
@"Function MaxInt(i Collection(Int32)) as (max(i))

select gkey, MaxInt(groupPartition(a))
FROM {1,1,2,2,2} as a
Group By a as gkey";

            var expectedSql =
@"SELECT 
[GroupBy1].[K1] AS [C1], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[UnionAll4].[C1] AS [K1], 
	MAX([UnionAll4].[C1]) AS [A1]
	FROM  (SELECT 
		[UnionAll3].[C1] AS [C1]
		FROM  (SELECT 
			[UnionAll2].[C1] AS [C1]
			FROM  (SELECT 
				[UnionAll1].[C1] AS [C1]
				FROM  (SELECT 
					1 AS [C1]
					FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
				UNION ALL
					SELECT 
					1 AS [C1]
					FROM  ( SELECT 1 AS X ) AS [SingleRowTable2]) AS [UnionAll1]
			UNION ALL
				SELECT 
				2 AS [C1]
				FROM  ( SELECT 1 AS X ) AS [SingleRowTable3]) AS [UnionAll2]
		UNION ALL
			SELECT 
			2 AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable4]) AS [UnionAll3]
	UNION ALL
		SELECT 
		2 AS [C1]
		FROM  ( SELECT 1 AS X ) AS [SingleRowTable5]) AS [UnionAll4]
	GROUP BY [UnionAll4].[C1]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Inline_aggregate_funtion_MinProductId()
        {
            var query =
@"Function MinProductId(products Collection(ProductModel.Product)) as 
(
    anyelement(select value min(pp.ProductId) from products as pp)
)

select gkey, MinProductId(GroupPartition(P))
FROM ProductContainer.Products as P
Group By P.ProductName as gkey";

            var expectedSql =
@"SELECT 
1 AS [C1], 
[Project2].[ProductName] AS [ProductName], 
[Project2].[C1] AS [C2]
FROM ( SELECT 
	[Distinct1].[ProductName] AS [ProductName], 
	(SELECT 
		MIN([Extent2].[ProductID]) AS [A1]
		FROM [dbo].[Products] AS [Extent2]
		WHERE ([Extent2].[Discontinued] IN (0,1)) AND (([Distinct1].[ProductName] = [Extent2].[ProductName]) OR (([Distinct1].[ProductName] IS NULL) AND ([Extent2].[ProductName] IS NULL)))) AS [C1]
	FROM ( SELECT DISTINCT 
		[Extent1].[ProductName] AS [ProductName]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
	)  AS [Distinct1]
)  AS [Project2]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }
    }
}