// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class EnumTests : FunctionalTestBase
    {
        [Fact]
        public void Cast_property_to_byte_enum()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
[Extent1].[c34_byteenum] AS [c34_byteenum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
WHERE 2 =  CAST( [Extent1].[c34_byteenum] AS int)";

                var query = context.AllTypes.Where(a => a.c34_byteenum == ArubaByteEnum.ByteEnumValue2).Select(a => a.c34_byteenum);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that correct enums are filtered out (verify count and value)
                var results = query.ToList();
                var expectedCount = context.AllTypes.ToList().Count(a => a.c34_byteenum == ArubaByteEnum.ByteEnumValue2);
                Assert.True(results.All(r => r == ArubaByteEnum.ByteEnumValue2));
                Assert.Equal(expectedCount, results.Count);
            }
        }

        [Fact]
        public void Cast_constant_to_byte_enum()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
[Extent1].[c34_byteenum] AS [c34_byteenum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
WHERE 2 =  CAST( [Extent1].[c34_byteenum] AS int)";

                var query = context.AllTypes.Where(a => a.c34_byteenum == (ArubaByteEnum)2).Select(a => a.c34_byteenum);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that correct enums are filtered out (verify count and value)
                var results = query.ToList();
                var expectedCount = context.AllTypes.ToList().Count(a => a.c34_byteenum == (ArubaByteEnum)2);
                Assert.True(results.All(r => r == ArubaByteEnum.ByteEnumValue2));
                Assert.Equal(expectedCount, results.Count);
            }
        }

        [Fact]
        public void Is_on_enum_not_supported()
        {
            using (var context = new ArubaContext())
            {
                Assert.Throws<NotSupportedException>(
                    () => context.AllTypes.Select(a => (ArubaEnum?)a.c33_enum is ArubaEnum).ToList()).ValidateMessage(
                    typeof(DbContext).Assembly(),
                    "ELinq_UnsupportedIsOrAs",
                    null, 
                    "TypeIs",
                    "System.Nullable`1[[System.Data.Entity.TestModels.ArubaModel.ArubaEnum, EntityFramework.FunctionalTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a]]",
                    "System.Data.Entity.TestModels.ArubaModel.ArubaEnum");
            }
        }

        [Fact]
        public void As_on_enum_not_supported()
        {
            using (var context = new ArubaContext())
            {
                Assert.Throws<NotSupportedException>(
                    () => context.AllTypes.Select(a => a.c33_enum as ArubaEnum?).ToList()).ValidateMessage(
                    typeof(DbContext).Assembly(),
                    "ELinq_UnsupportedIsOrAs",
                    null, 
                    "TypeAs", 
                    "System.Data.Entity.TestModels.ArubaModel.ArubaEnum",
                    "System.Nullable`1[[System.Data.Entity.TestModels.ArubaModel.ArubaEnum, EntityFramework.FunctionalTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a]]");
            }
        }

        [Fact]
        public void Byte_based_enum_in_OrderByDescending_clause()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
                    @"SELECT 
[Extent1].[c34_byteenum] AS [c34_byteenum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
ORDER BY [Extent1].[c34_byteenum] DESC";

                var query = context.AllTypes.OrderByDescending(a => a.c34_byteenum).Select(a => a.c34_byteenum);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify order is correct
                var results = query.ToList();
                var lowest = (int)results[0];
                foreach (var result in results)
                {
                    Assert.True((int)result <= lowest);
                    lowest = (int)result;
                }
            }
        }

        [Fact]
        public void Enum_in_OrderByThenBy_clause()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
                    @"SELECT 
[Extent1].[c33_enum] AS [c33_enum], 
[Extent1].[c34_byteenum] AS [c34_byteenum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
ORDER BY [Extent1].[c33_enum] ASC, [Extent1].[c34_byteenum] DESC";

                var query = context.AllTypes.OrderBy(p => p.c33_enum).ThenByDescending(a => a.c34_byteenum).Select(
                    a => new
                        {
                            a.c33_enum,
                            a.c34_byteenum
                        });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.AllTypes.Select(
                    a => new
                        {
                            a.c33_enum,
                            a.c34_byteenum
                        }).ToList()
                                      .OrderBy(p => p.c33_enum).ThenByDescending(a => a.c34_byteenum).ToList();

                Assert.Equal(results.Count, expected.Count());
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(results[i].c33_enum, expected[i].c33_enum);
                    Assert.Equal(results[i].c34_byteenum, expected[i].c34_byteenum);
                }
            }
        }

        [Fact]
        public void Byte_based_enum_in_GroupBy_clause()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
                    @"SELECT 
1 AS [C1], 
[GroupBy1].[K1] AS [c34_byteenum], 
[GroupBy1].[A1] AS [C2]
FROM ( SELECT 
	[Extent1].[c34_byteenum] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[ArubaAllTypes] AS [Extent1]
	GROUP BY [Extent1].[c34_byteenum]
)  AS [GroupBy1]";

                var query = context.AllTypes.GroupBy(p => p.c34_byteenum).Select(
                    g => new
                        {
                            g.Key,
                            Count = g.Count()
                        });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList().OrderBy(r => r.Key).ToList();
                var expected = context.AllTypes.ToList().GroupBy(a => a.c34_byteenum).Select(
                    g => new
                        {
                            g.Key,
                            Count = g.Count()
                        }).OrderBy(r => r.Key).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Key == i.Key && o.Count == i.Count);
            }
        }

        [Fact]
        public void Enum_with_arithmetic_operations()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
[Extent1].[c1_int] AS [c1_int]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
WHERE ( CAST( [Extent1].[c34_byteenum] AS int) <>  CAST(  CAST(  CAST( [Extent1].[c34_byteenum] AS int) + 2 AS tinyint) AS int)) AND ( CAST(  CAST( [Extent1].[c33_enum] AS int) + 1 AS int) <>  CAST(  CAST( [Extent1].[c33_enum] AS int) - 2 AS int))";

                var query = context.AllTypes
                                   .Where(a => a.c34_byteenum != a.c34_byteenum + 2)
                                   .Where(a => a.c33_enum + 1 != a.c33_enum - 2)
                                   .Select(a => a.c1_int);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that no results are filtered out (sanity)
                var results = query.ToList();
                var allTypesCount = context.AllTypes.Select(a => a.c1_int).ToList().Count();
                Assert.Equal(allTypesCount, results.Count);
            }
        }

        [Fact]
        public void Enum_with_bitwise_operations()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
[Extent1].[c33_enum] AS [c33_enum], 
[Extent1].[c34_byteenum] AS [c34_byteenum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
WHERE ( CAST(  CAST( ( CAST( [Extent1].[c34_byteenum] AS int)) & (1) AS tinyint) AS int) > 0) AND (3 =  CAST( ( CAST( [Extent1].[c33_enum] AS int)) | (1) AS int))";

                var query = context.AllTypes
                                   .Where(a => (a.c34_byteenum & ArubaByteEnum.ByteEnumValue1) > 0)
                                   .Where(a => (a.c33_enum | (ArubaEnum)1) == (ArubaEnum)3)
                                   .Select(
                                       a => new
                                           {
                                               a.c33_enum,
                                               a.c34_byteenum
                                           });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that correct enums are filtered out
                var results = query.ToList();
                foreach (var result in results)
                {
                    Assert.Equal((ArubaEnum)3, result.c33_enum);
                    Assert.Equal(ArubaByteEnum.ByteEnumValue1, result.c34_byteenum);
                }
            }
        }
    }
}
