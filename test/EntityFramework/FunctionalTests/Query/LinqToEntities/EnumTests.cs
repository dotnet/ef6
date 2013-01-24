// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Text;
    using System.Xml;
    using SimpleModel;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.IO;
    using System.Linq;
    using Xunit;

    public enum Category
    {
        Beverages = 1,
        Condiments = 2,
        Confections = 3,
        DairyProducts = 4,
        GrainsCereals = 5,
        MeatPoultry = 6,
        Produce = 7,
    };

    public enum ByteBasedColor : byte
    {
        Red = 1,
        Blue = 2,
        Green = 3,
        Black = 4,
        White = 5,
    };

    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public byte Code { get; set; }
        public Category ProductCategory { get; set; }
        public ByteBasedColor Color { get; set; }
    }

    public class ProductWithEnumsContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
    }

    public class EnumTests : FunctionalTestBase
    {
        [Fact]
        public void Cast_property_to_enum()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE 1 =  CAST( [Extent1].[ProductId] AS int)";

                var query = context.Products.Where(p => (Category)p.ProductId == Category.Beverages).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Cast_property_to_byte_enum()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE 4 =  CAST( [Extent1].[Code] AS int)";

                var query = context.Products.Where(p => (ByteBasedColor)p.Code == ByteBasedColor.Black).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Cast_constant_to_enum()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE 1 =  CAST( [Extent1].[ProductCategory] AS int)";

                var query = context.Products.Where(p => p.ProductCategory == (Category)1).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Cast_constant_to_byte_enum()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE 2 =  CAST( [Extent1].[Color] AS int)";

                var query = context.Products.Where(p => p.Color == (ByteBasedColor)2).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Is_on_enum_not_supported()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var message = Assert.Throws<NotSupportedException>(() =>
                    context.Products.Select(p => (Category?)p.ProductCategory is Category).ToList()).Message;

                Assert.True(
                    message.Contains(
                        Strings.ELinq_UnsupportedIsOrAs("TypeIs", "System.Nullable`1", "System.Data.Entity.Query.LinqToEntities.Category")));
            }
        }

        [Fact]
        public void As_on_enum_not_supported()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var message = Assert.Throws<NotSupportedException>(() =>
                    context.Products.Select(p => p.ProductCategory as Nullable<Category>).ToList()).Message;

                Assert.True(
                    message.Contains(
                        Strings.ELinq_UnsupportedIsOrAs("TypeAs", "System.Data.Entity.Query.LinqToEntities.Category", "System.Nullable`1")));
            }
        }

        [Fact]
        public void Enum_property_in_Where_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE 1 =  CAST( [Extent1].[ProductCategory] AS int)";

                var query = context.Products.Where(p => p.ProductCategory == Category.Beverages).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Byte_enum_property_in_Where_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE 4 =  CAST( [Extent1].[Color] AS int)";

                var query = context.Products.Where(p => p.Color == ByteBasedColor.Black).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Unnamed_enum_constant_in_Where_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE 42 =  CAST( [Extent1].[ProductCategory] AS int)";

                var query = context.Products.Where(p => p.ProductCategory == (Category)42).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Enum_in_OrderBy_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
ORDER BY [Extent1].[ProductCategory] ASC";

                var query = context.Products.OrderBy(p => p.ProductCategory).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Byte_based_enum_in_OrderByDescending_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
ORDER BY [Extent1].[Color] DESC";

                var query = context.Products.OrderByDescending(p => p.Color).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Enum_in_OrderByThenBy_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
ORDER BY [Extent1].[ProductCategory] ASC, [Extent1].[Color] DESC";

                var query = context.Products.OrderBy(p => p.ProductCategory).ThenByDescending(p => p.Color).Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Enum_in_GroupBy_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[GroupBy1].[K1] AS [ProductCategory], 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[ProductCategory] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	GROUP BY [Extent1].[ProductCategory]
)  AS [GroupBy1]";

                var query = context.Products.GroupBy(p => p.ProductCategory).Select(g => new { g.Key, Count = g.Count() });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Byte_based_enum_in_GroupBy_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [Color], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[Color] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[Products] AS [Extent1]
	GROUP BY [Extent1].[Color]
)  AS [GroupBy1]";

                var query = context.Products.GroupBy(p => p.Color).Select(g => new { g.Key, Count = g.Count() });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Enum_in_Join_clause()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Limit1].[ProductCategory] AS [ProductCategory], 
[Limit1].[ProductName] AS [ProductName], 
[Extent2].[Color] AS [Color]
FROM   (SELECT TOP (1) [c].[ProductName] AS [ProductName], [c].[ProductCategory] AS [ProductCategory]
	FROM [dbo].[Products] AS [c] ) AS [Limit1]
INNER JOIN [dbo].[Products] AS [Extent2] ON [Limit1].[ProductCategory] = [Extent2].[ProductCategory]";

                var query = context.Products.Take(1).Join(context.Products, o => o.ProductCategory, i => i.ProductCategory, (o, i) => new { o.ProductName, i.Color });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Enum_with_arithmetic_operations()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
                    // TODO: too many casts here!
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE ( CAST(  CAST(  CAST( [Extent1].[Color] AS int) - 1 AS tinyint) AS int) <>  CAST(  CAST(  CAST( [Extent1].[Color] AS int) + 2 AS tinyint) AS int)) AND ( CAST(  CAST( [Extent1].[ProductCategory] AS int) + 1 AS int) <>  CAST(  CAST( [Extent1].[ProductCategory] AS int) - 2 AS int))";

                var query = context.Products
                    .Where(p => p.Color - 1 != p.Color + 2)
                    .Where(p => p.ProductCategory + 1 != p.ProductCategory - 2)
                    .Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Enum_with_bitwise_operations()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductName] AS [ProductName]
FROM [dbo].[Products] AS [Extent1]
WHERE ( CAST(  CAST( ( CAST( [Extent1].[Color] AS int)) & (2) AS tinyint) AS int) > 0) AND (6 =  CAST( ( CAST( [Extent1].[ProductCategory] AS int)) | (1) AS int))";

                var query = context.Products
                    .Where(p => (p.Color & ByteBasedColor.Blue) > 0)
                    .Where(p => (p.ProductCategory | (Category)1) == (Category)6)
                    .Select(p => p.ProductName);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Enum_with_coalesce_operator()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
CASE WHEN (CASE WHEN (0 = ([Extent1].[ProductId] % 2)) THEN [Extent1].[ProductCategory] END IS NULL) THEN 3 WHEN (0 = ([Extent1].[ProductId] % 2)) THEN [Extent1].[ProductCategory] END AS [C1]
FROM [dbo].[Products] AS [Extent1]";

                var query = context.Products.Select(p => p.ProductId % 2 == 0 ? p.ProductCategory : (Category?)null)
                    .Select(p => p ?? Category.Confections); 

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Casting_to_enum_undeclared_in_model_works()
        {
            using (var context = new ProductWithEnumsContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[ProductId] AS [ProductId]
FROM [dbo].[Products] AS [Extent1]";

                var query = context.Products.Select(p => (FileAccess)p.ProductId);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }
    }
}