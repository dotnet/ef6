// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ModelDefinedFunctionTests : FunctionalTestBase
    {
        private static readonly MetadataWorkspace workspace = QueryTestHelpers.CreateMetadataWorkspace(
            ProductModel.CsdlWithFunctions, ProductModel.Ssdl, ProductModel.Msl);

        [Fact]
        public void Exception_thrown_for_function_with_no_body()
        {
            var query = "ProductModel.F_NoBody()";
            QueryTestHelpers.VerifyThrows<InvalidOperationException>(query, workspace, "Cqt_UDF_FunctionHasNoDefinition", "ProductModel.F_NoBody()");
        }

        [Fact]
        public void Exception_thrown_if_invalid_parameter_passed_to_function()
        {
            var query = "ProductModel.F_I(10)";
            QueryTestHelpers.VerifyThrows<InvalidOperationException>(query, workspace, "Cqt_UDF_FunctionDefinitionResultTypeMismatch", "Edm.Int32", "ProductModel.F_I", "Edm.Int16");
        }

        [Fact]
        public void Project_function_referencing_other_functions()
        {
            var query = "ProductModel.F_A() - ProductModel.F_B() - ProductModel.F_C()";
            var expectedSql =
                @"SELECT 
((1 - (1 + 2)) - 1) - (1 + 2) AS [C1]
FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_that_takes_itself_as_argument()
        {
            var query = "ProductModel.F_J(ProductModel.F_J(ProductModel.F_J(1)))";
            var expectedSql =
                @"SELECT 
1 AS [C1]
FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Exception_thrown_for_function_with_direct_reference_to_itself_in_definition()
        {
            var query = "ProductModel.F_D()";
            QueryTestHelpers.VerifyThrows<EntityCommandCompilationException>(query, workspace, "Cqt_UDF_FunctionDefinitionWithCircularReference", "ProductModel.F_D");
        }

        [Fact]
        public void Exception_thrown_for_function_with_indirect_reference_to_itself_in_definition()
        {
            var query1 = "ProductModel.F_E()";
            QueryTestHelpers.VerifyThrows<EntityCommandCompilationException>(query1, workspace, "Cqt_UDF_FunctionDefinitionWithCircularReference", "ProductModel.F_E");

            var query2 = "ProductModel.F_F()";
            QueryTestHelpers.VerifyThrows<EntityCommandCompilationException>(query2, workspace, "Cqt_UDF_FunctionDefinitionWithCircularReference", "ProductModel.F_F");
        }

        [Fact]
        public void Exception_thrown_for_function_that_references_inline_function_in_its_body()
        {
            var query =
                @"using ProductModel;
function F_H() as (1)
F_G()";

            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query, 
                workspace, 
                "CannotResolveNameToTypeOrFunction", 
                s => s.Replace(" Near simple identifier, line 3, column 7.", ""), 
                "F_H");
        }

        [Fact]
        public void Function_returning_scalar()
        {
            var query = "ProductModel.F_Ret_ST() + 11";
            var expectedSql =
                @"SELECT 
[GroupBy1].[A1] + 11 AS [C1]
FROM ( SELECT 
	COUNT(3) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_returning_collection_of_scalars()
        {
            var query = "select max(s) as maxs from ProductModel.F_Ret_ColST() as s";
            var expectedSql =
                @"SELECT 
1 AS [C1], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	MAX([Filter1].[A1]) AS [A1]
	FROM ( SELECT 
		[Extent1].[ProductID] - 3 AS [A1]
		FROM [dbo].[Products] AS [Extent1]
		WHERE [Extent1].[Discontinued] IN (0,1)
	)  AS [Filter1]
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_returning_entity()
        {
            var query = "ProductModel.F_Ret_ET().ProductID + 12";
            var expectedSql =
                @"SELECT 
[Limit1].[ProductID] + 12 AS [C1]
FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
LEFT OUTER JOIN  (SELECT TOP (1) [Extent1].[ProductID] AS [ProductID]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	ORDER BY [Extent1].[ProductID] ASC ) AS [Limit1] ON 1 = 1";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_returning_collection_of_entities()
        {
            var query = "select p.ProductID + 5 as z from ProductModel.F_Ret_ColET() as p order by p.ProductID DESC";
            var expectedSql =
                @"SELECT 
[Project1].[ProductID] AS [ProductID], 
[Project1].[C1] AS [C1]
FROM ( SELECT TOP (5) 
	[Extent1].[ProductID] AS [ProductID], 
	[Extent1].[ProductID] + 5 AS [C1]
	FROM [dbo].[Products] AS [Extent1]
	WHERE [Extent1].[Discontinued] IN (0,1)
	ORDER BY [Extent1].[ProductID] ASC
)  AS [Project1]
ORDER BY [Project1].[ProductID] DESC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_returning_complex_type()
        {
            var query = "ProductModel.F_Ret_CT().City + ' ' + ProductModel.F_Ret_CT().Country";
            var expectedSql =
                @"SELECT 
[Limit1].[City] + ' ' + [Limit2].[Country] AS [C1]
FROM    ( SELECT 1 AS X ) AS [SingleRowTable1]
LEFT OUTER JOIN  (SELECT TOP (1) [Extent1].[City] AS [City]
	FROM [dbo].[Customers] AS [Extent1]
	ORDER BY [Extent1].[CustomerID] ASC ) AS [Limit1] ON 1 = 1
LEFT OUTER JOIN  (SELECT TOP (1) [Extent2].[Country] AS [Country]
	FROM [dbo].[Customers] AS [Extent2]
	ORDER BY [Extent2].[CustomerID] ASC ) AS [Limit2] ON 1 = 1";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_returning_collection_of_complex_types()
        {
            var query = "select ct from (ProductModel.F_Ret_ColCT()) as ct order by ct.City";
            var expectedSql =
                @"SELECT 
[Limit1].[C1] AS [C1], 
[Limit1].[HomeAddress] AS [HomeAddress], 
[Limit1].[City] AS [City], 
[Limit1].[Region] AS [Region], 
[Limit1].[PostalCode] AS [PostalCode], 
[Limit1].[Country] AS [Country]
FROM ( SELECT TOP (5) [Project1].[HomeAddress] AS [HomeAddress], [Project1].[City] AS [City], [Project1].[Region] AS [Region], [Project1].[PostalCode] AS [PostalCode], [Project1].[Country] AS [Country], [Project1].[C1] AS [C1]
	FROM ( SELECT 
		[Extent1].[CustomerID] AS [CustomerID], 
		[Extent1].[HomeAddress] AS [HomeAddress], 
		[Extent1].[City] AS [City], 
		[Extent1].[Region] AS [Region], 
		[Extent1].[PostalCode] AS [PostalCode], 
		[Extent1].[Country] AS [Country], 
		1 AS [C1]
		FROM [dbo].[Customers] AS [Extent1]
	)  AS [Project1]
	ORDER BY [Project1].[CustomerID] ASC
)  AS [Limit1]
ORDER BY [Limit1].[City] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_taking_collection_of_scalars_as_argument_and_returns_collection_of_entities()
        {
            var query =
                "SELECT top(5) et FROM ProductModel.F_In_ColST_Ret_ColET(SELECT VALUE c.CustomerID from ProductContainer.Customers as c) as et";

            var expectedSql =
                @"SELECT TOP (5) 
[Project1].[C1] AS [C1], 
[Project1].[CustomerID] AS [CustomerID], 
[Project1].[HomeAddress] AS [HomeAddress], 
[Project1].[City] AS [City], 
[Project1].[Region] AS [Region], 
[Project1].[PostalCode] AS [PostalCode], 
[Project1].[Country] AS [Country]
FROM ( SELECT 
	[Extent1].[CustomerID] AS [CustomerID], 
	[Extent1].[HomeAddress] AS [HomeAddress], 
	[Extent1].[City] AS [City], 
	[Extent1].[Region] AS [Region], 
	[Extent1].[PostalCode] AS [PostalCode], 
	[Extent1].[Country] AS [Country], 
	1 AS [C1]
	FROM [dbo].[Customers] AS [Extent1]
)  AS [Project1]
WHERE  EXISTS (SELECT 
	1 AS [C1]
	FROM [dbo].[Customers] AS [Extent2]
	WHERE [Project1].[CustomerID] = [Extent2].[CustomerID]
)";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_taking_collection_of_complex_types_as_argument_and_returns_a_scalar()
        {
            var query = "ProductModel.F_In_ColCT_Ret_ST(select value c.Address from ProductContainer.Customers as c)";

            var expectedSql =
                @"SELECT 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	MIN([Extent1].[City]) AS [A1]
	FROM [dbo].[Customers] AS [Extent1]
	WHERE [Extent1].[Country] = 'Mexico'
)  AS [GroupBy1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_taking_collection_of_entities_as_argument_and_returns_collection_of_entities()
        {
            var query = "select top(5) et FROM ProductModel.F_In_ColET_Ret_ColET(select value c from ProductContainer.Customers as c) as et";

            var expectedSql =
                @"SELECT 
[Limit1].[C1] AS [C1], 
[Limit1].[CustomerID] AS [CustomerID], 
[Limit1].[HomeAddress] AS [HomeAddress], 
[Limit1].[City] AS [City], 
[Limit1].[Region] AS [Region], 
[Limit1].[PostalCode] AS [PostalCode], 
[Limit1].[Country] AS [Country]
FROM ( SELECT TOP (5) 
	[Extent1].[CustomerID] AS [CustomerID], 
	[Extent1].[HomeAddress] AS [HomeAddress], 
	[Extent1].[City] AS [City], 
	[Extent1].[Region] AS [Region], 
	[Extent1].[PostalCode] AS [PostalCode], 
	[Extent1].[Country] AS [Country], 
	1 AS [C1]
	FROM [dbo].[Customers] AS [Extent1]
	WHERE [Extent1].[Country] = 'Mexico'
)  AS [Limit1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_taking_collection_of_complex_types_as_argument_and_returns_collection_of_complex_types()
        {
            var query =
                "SELECT TOP(5) ct FROM ProductModel.F_In_ColCT_Ret_ColCT(select value c.Address from ProductContainer.Customers as c) as ct";

            var expectedSql =
                @"SELECT 
[Limit1].[C1] AS [C1], 
[Limit1].[HomeAddress] AS [HomeAddress], 
[Limit1].[City] AS [City], 
[Limit1].[Region] AS [Region], 
[Limit1].[PostalCode] AS [PostalCode], 
[Limit1].[Country] AS [Country]
FROM ( SELECT TOP (5) 
	[Extent1].[HomeAddress] AS [HomeAddress], 
	[Extent1].[City] AS [City], 
	[Extent1].[Region] AS [Region], 
	[Extent1].[PostalCode] AS [PostalCode], 
	[Extent1].[Country] AS [Country], 
	1 AS [C1]
	FROM [dbo].[Customers] AS [Extent1]
	WHERE [Extent1].[Country] = 'Mexico'
)  AS [Limit1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_taking_collection_of_scalars_as_argument_and_returns_collection_of_scalars()
        {
            var query =
                "SELECT top(5) s FROM ProductModel.F_In_ColST_Ret_ColST(SELECT VALUE c.CustomerID from ProductContainer.Customers as c) as s";

            var expectedSql =
                @"SELECT TOP (5) 
1 AS [C1], 
[c].[CustomerID] AS [C2]
FROM  (SELECT 
	[Extent1].[CustomerID] AS [CustomerID]
	FROM [dbo].[Customers] AS [Extent1]
INTERSECT
	SELECT 
	[UnionAll2].[C1] AS [C1]
	FROM  (SELECT 
		[UnionAll1].[C1] AS [C1]
		FROM  (SELECT 
			'a' AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
		UNION ALL
			SELECT 
			'b' AS [C1]
			FROM  ( SELECT 1 AS X ) AS [SingleRowTable2]) AS [UnionAll1]
	UNION ALL
		SELECT 
		'c' AS [C1]
		FROM  ( SELECT 1 AS X ) AS [SingleRowTable3]) AS [UnionAll2]) AS [c]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_taking_row_as_argument_and_returns_a_row()
        {
            var query = "SELECT TOP(5) ProductModel.F_In_RT_Ret_RT(c) as r FROM (SELECT cust FROM ProductContainer.Customers as cust) as c";

            var expectedSql =
                @"SELECT 
[Limit1].[C1] AS [C1], 
LEN([Limit1].[CustomerID]) AS [C2], 
[Limit1].[City] AS [City], 
[Limit1].[HomeAddress] AS [HomeAddress], 
[Limit1].[Region] AS [Region], 
[Limit1].[PostalCode] AS [PostalCode], 
[Limit1].[Country] AS [Country]
FROM ( SELECT TOP (5) 
	[Extent1].[CustomerID] AS [CustomerID], 
	[Extent1].[HomeAddress] AS [HomeAddress], 
	[Extent1].[City] AS [City], 
	[Extent1].[Region] AS [Region], 
	[Extent1].[PostalCode] AS [PostalCode], 
	[Extent1].[Country] AS [Country], 
	1 AS [C1]
	FROM [dbo].[Customers] AS [Extent1]
)  AS [Limit1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Function_taking_collection_of_rows_as_argument_and_returns_collection_of_rows()
        {
            var query = "SELECT TOP(5) r FROM ProductModel.F_In_ColRT_Ret_ColRT(SELECT c FROM ProductContainer.Customers as c) as r";

            var expectedSql =
                @"SELECT 
[Limit1].[C1] AS [C1], 
[Limit1].[C2] AS [C2], 
[Limit1].[City] AS [City], 
[Limit1].[HomeAddress] AS [HomeAddress], 
[Limit1].[Region] AS [Region], 
[Limit1].[PostalCode] AS [PostalCode], 
[Limit1].[Country] AS [Country]
FROM ( SELECT TOP (5) 
	[Extent1].[HomeAddress] AS [HomeAddress], 
	[Extent1].[City] AS [City], 
	[Extent1].[Region] AS [Region], 
	[Extent1].[PostalCode] AS [PostalCode], 
	[Extent1].[Country] AS [Country], 
	1 AS [C1], 
	LEN([Extent1].[CustomerID]) AS [C2]
	FROM [dbo].[Customers] AS [Extent1]
)  AS [Limit1]";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Row_field_names_are_ignored_during_function_resolution()
        {
            var query =
                @"select r.A as W, r.B as V from
ProductModel.F_RowFieldNamessIgnored(
{
    row(1 as z, '1' as y),
    row(4 as r, '4' as q)
}) as r
order by W";

            var expectedSql =
                @"SELECT 
[Project3].[C1] AS [C1], 
[Project3].[C2] AS [C2], 
[Project3].[C3] AS [C3]
FROM ( SELECT 
	[UnionAll1].[C1] AS [C1], 
	(CASE WHEN ([UnionAll1].[C1] = 0) THEN 1 ELSE 4 END) + 1 AS [C2], 
	CASE WHEN ([UnionAll1].[C1] = 0) THEN '1' ELSE '4' END + '1' AS [C3]
	FROM  (SELECT 
		0 AS [C1]
		FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
		WHERE ((CASE WHEN (0 = 0) THEN 1 ELSE 4 END) = 1) AND ((CASE WHEN (0 = 0) THEN '1' ELSE '4' END) = '1')
	UNION ALL
		SELECT 
		1 AS [C1]
		FROM  ( SELECT 1 AS X ) AS [SingleRowTable2]
		WHERE ((CASE WHEN (1 = 0) THEN 1 ELSE 4 END) = 1) AND ((CASE WHEN (1 = 0) THEN '1' ELSE '4' END) = '1')) AS [UnionAll1]
)  AS [Project3]
ORDER BY [Project3].[C2] ASC";

            QueryTestHelpers.VerifyQuery(query, workspace, expectedSql);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_integer_as_argument()
        {
            var query1 = "ProductModel.F_In_Number(CAST(1 as Byte))";
            var expectedSql1 = "SELECT '16' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 = "ProductModel.F_In_Number(CAST(1 as Int32))";
            var expectedSql2 = "SELECT '32' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);

            var query3 = "ProductModel.F_In_Number(CAST(1 as Int64))";
            var expectedSql3 = "SELECT 'Decimal' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query3, workspace, expectedSql3);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_two_integers_as_arguments()
        {
            var query1 = "ProductModel.F_In_ST_1(CAST(1 as Int16), CAST(1 AS Single))";
            var expectedSql1 = "SELECT 1 AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 = "ProductModel.F_In_ST_1(CAST(1 as Int32), CAST(1 AS Int32))";
            var expectedSql2 = "SELECT 2 AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);

            var query3 = "ProductModel.F_In_ST_1(CAST(1 as Single), CAST(1 AS Int16))";
            var expectedSql3 = "SELECT 3 AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query3, workspace, expectedSql3);

            var query4 = "ProductModel.F_In_ST_2(CAST(1 as Byte), CAST(1 AS Int32))";
            var expectedSql4 = "SELECT 1 AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query4, workspace, expectedSql4);

            var query5 = "ProductModel.F_In_ST_2(CAST(1 as Int16), CAST(1 AS Int32))";
            var expectedSql5 = "SELECT 2 AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query5, workspace, expectedSql5);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_two_integers_as_arguments_negative()
        {
            var query1 = "ProductModel.F_In_ST_1(CAST(1 as Int16), CAST(1 AS Int32))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query1, 
                workspace, 
                "AmbiguousFunctionArguments", 
                s => s.Replace(" Near function 'ProductModel.F_In_ST_1()', line 1, column 14.", ""));

            var query2 = "ProductModel.F_In_ST_1(CAST(1 as Int16), CAST(1 AS Int16))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query2, 
                workspace, 
                "AmbiguousFunctionArguments",
                s => s.Replace(" Near function 'ProductModel.F_In_ST_1()', line 1, column 14.", ""));

            var query3 = "ProductModel.F_In_ST_1(CAST(1 as Int64), CAST(1 AS Int16))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query3, 
                workspace, 
                "AmbiguousFunctionArguments",
                s => s.Replace(" Near function 'ProductModel.F_In_ST_1()', line 1, column 14.", ""));

            var query4 = "ProductModel.F_In_ST_1(CAST(1 as Double), CAST(1 AS Double))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query4,
                workspace,
                "NoFunctionOverloadMatch",
                s => s.Replace(" Near function 'F_In_ST_1()', line 1, column 14.", ""),
                "ProductModel", 
                "F_In_ST_1", 
                "F_In_ST_1(Edm.Double, Edm.Double)");
        }

        [Fact]
        public void Overload_resolution_for_function_taking_entity_as_argument()
        {
            var query1 = "ProductModel.F_In_Entity(anyelement(ProductContainer.Products))";
            var expectedSql1 = "SELECT 'Product' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 =
                "ProductModel.F_In_Entity(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p))";
            var expectedSql2 = "SELECT 'DiscontinuedProduct' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);

            var query3 =
                "ProductModel.F_In_Entity2(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p))";
            var expectedSql3 = "SELECT 'Product' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query3, workspace, expectedSql3);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_entity_and_integer_as_arguments()
        {
            var query1 = "ProductModel.F_In_ProdNumber(anyelement(ProductContainer.Products), CAST(1 as Int32))";
            var expectedSql1 = "SELECT 'Prod-32' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 = "ProductModel.F_In_ProdNumber(anyelement(ProductContainer.Products), CAST(1 as Int16))";
            var expectedSql2 = "SELECT 'Prod-32' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);

            var query3 =
                "ProductModel.F_In_ProdNumber(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p), CAST(1 as Int32))";
            var expectedSql3 = "SELECT 'Prod-32' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query3, workspace, expectedSql3);

            var query4 =
                "ProductModel.F_In_ProdNumber(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p), CAST(1 as Int16))";
            var expectedSql4 = "SELECT 'DiscProd-16' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query4, workspace, expectedSql4);

            var query5 =
                "ProductModel.F_In_ProdNumber2(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p), CAST(1 as Int64))";
            var expectedSql5 = "SELECT 'DiscProd-Decimal' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query5, workspace, expectedSql5);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_entity_and_integer_as_arguments_negative()
        {
            var query1 = "ProductModel.F_In_ProdNumber2(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p), CAST(1 as Int32))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query1, 
                workspace, 
                "AmbiguousFunctionArguments",
                s => s.Replace(" Near function 'ProductModel.F_In_ProdNumber2()', line 1, column 14.", ""));

            var query2 = "ProductModel.F_In_ProdNumber2(anyelement(ProductContainer.Products), CAST(1 as Decimal))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query2, 
                workspace, 
                "NoFunctionOverloadMatch",
                s => s.Replace(" Near function 'F_In_ProdNumber2()', line 1, column 14.", ""),
                "ProductModel", 
                "F_In_ProdNumber2", 
                "F_In_ProdNumber2(ProductModel.Product, Edm.Decimal)");
        }

        [Fact]
        public void Overload_resolution_for_function_taking_entity_reference_as_argument()
        {
            var query1 = "ProductModel.F_In_Ref(REF(anyelement(ProductContainer.Products)))";
            var expectedSql1 = "SELECT 'Ref(Product)' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 =
                "ProductModel.F_In_Ref(REF(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p)))";
            var expectedSql2 = "SELECT 'Ref(DiscontinuedProduct)' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);

            var query3 =
                "ProductModel.F_In_Ref2(REF(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p)))";
            var expectedSql3 = "SELECT 'Ref(Product)' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query3, workspace, expectedSql3);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_row_as_argument()
        {
            var query1 =
                "ProductModel.F_In_Row(Row(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p) as x, CAST(1 as Int64) as y))";
            var expectedSql1 = "SELECT 'Row(DiscProd,Decimal)' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 = "ProductModel.F_In_Row(Row(anyelement(ProductContainer.Products) as x, CAST(1 as Int16) as y))";
            var expectedSql2 = "SELECT 'Row(Prod,32)' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_row_as_argument_negative()
        {
            var query1 =
                "ProductModel.F_In_Row(Row(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p) as x, CAST(1 as Int32) as y))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query1,
                workspace,
                "AmbiguousFunctionArguments",
                s => s.Replace(" Near function 'ProductModel.F_In_Row()', line 1, column 14.", ""));

            var query2 = "ProductModel.F_In_Row(Row(anyelement(ProductContainer.Products) as x, CAST(1 as Decimal) as y))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query2,
                workspace,
                "NoFunctionOverloadMatch",
                s => s.Replace(
                    " Near function 'F_In_Row()', line 1, column 14.", ""),
                    "ProductModel",
                    "F_In_Row",
                    "F_In_Row(Transient.rowtype[(x,ProductModel.Product(Nullable=True,DefaultValue=)),(y,Edm.Decimal(Nullable=True,DefaultValue=,Precision=,Scale=))])");
        }

        [Fact]
        public void Overload_resolution_for_function_taking_collection_of_rows_as_argument()
        {
            var query1 =
                "ProductModel.F_In_ColRow({Row(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p) as x, CAST(1 as Int64) as y)})";
            var expectedSql1 = "SELECT 'Col(Row(DiscProd,Decimal))' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 = "ProductModel.F_In_ColRow({Row(anyelement(ProductContainer.Products) as x, CAST(1 as Int16) as y)})";
            var expectedSql2 = "SELECT 'Col(Row(Prod,32))' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);
        }

        [Fact]
        public void Overload_resolution_for_function_taking_collection_of_rows_as_argument_negative()
        {
            var query1 =
                "ProductModel.F_In_ColRow({Row(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p) as x, CAST(1 as Int32) as y)})";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query1,
                workspace,
                "AmbiguousFunctionArguments",
                s => s.Replace(" Near function 'ProductModel.F_In_ColRow()', line 1, column 14.", ""));

            var query2 = "ProductModel.F_In_ColRow({Row(anyelement(ProductContainer.Products) as x, CAST(1 as Decimal) as y)})";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query2,
                workspace,
                "NoFunctionOverloadMatch",
                s => s.Replace(" Near function 'F_In_ColRow()', line 1, column 14.", ""),
                "ProductModel",
                "F_In_ColRow",
                "F_In_ColRow(Transient.collection[Transient.rowtype[(x,ProductModel.Product(Nullable=True,DefaultValue=)),(y,Edm.Decimal(Nullable=True,DefaultValue=,Precision=,Scale=))](Nullable=True,DefaultValue=)])");
        }

        [Fact]
        public void Overload_resolution_for_functions_with_nulls_as_arguments()
        {
            var query1 = "ProductModel.F_In_ProdNumber2(null, CAST(1 as Int64))";
            var expectedSql1 = "SELECT 'DiscProd-Decimal' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query1, workspace, expectedSql1);

            var query2 =
                "ProductModel.F_In_ProdNumber3(anyelement(select value treat(p as ProductModel.DiscontinuedProduct) from ProductContainer.Products as p), null)";
            var expectedSql2 = "SELECT 'Prod-32' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query2, workspace, expectedSql2);

            var query3 = "ProductModel.F_In_Row2(CAST(1 as Int16), null, CAST(1 as Int64))";
            var expectedSql3 = "SELECT 'Decimal-r-Decimal' AS [C1] FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]";
            QueryTestHelpers.VerifyQuery(query3, workspace, expectedSql3);
        }

        [Fact]
        public void Overload_resolution_for_functions_with_nulls_as_arguments_negative()
        {
            var query1 = "ProductModel.F_In_Number(null)";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query1,
                workspace,
                "AmbiguousFunctionArguments",
                s => s.Replace(" Near function 'ProductModel.F_In_Number()', line 1, column 14.", ""));

            var query2 = "ProductModel.F_In_ColRow2(CAST(1 as Int16), null, CAST(1 as Int64))";
            QueryTestHelpers.VerifyThrows<EntitySqlException>(
                query2, 
                workspace, 
                "NoFunctionOverloadMatch",
                s => s.Replace(" Near function 'F_In_ColRow2()', line 1, column 14.", ""),
                "ProductModel", 
                "F_In_ColRow2", 
                "F_In_ColRow2(Edm.Int16, NULL, Edm.Int64)");
        }
    }
}
