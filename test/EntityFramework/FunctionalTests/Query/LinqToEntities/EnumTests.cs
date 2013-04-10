// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.TestModels.ArubaModel;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class EnumTests : FunctionalTestBase
    {
        [Fact]
        public void Cast_property_to_enum()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
[Extent1].[c33_enum] AS [c33_enum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
WHERE 1 =  CAST( [Extent1].[c33_enum] AS int)";

                var query = context.AllTypes.Where(a => a.c33_enum == ArubaEnum.EnumValue1).Select(a => a.c33_enum);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that correct enums are filtered out (verify count and value)
                var results = query.ToList();
                var expectedCount = context.AllTypes.ToList().Count(a => a.c33_enum == ArubaEnum.EnumValue1);
                Assert.True(results.All(r => r == ArubaEnum.EnumValue1));
                Assert.Equal(expectedCount, results.Count);
            }
        }

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
        public void Cast_constant_to_enum()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
[Extent1].[c33_enum] AS [c33_enum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
WHERE 1 =  CAST( [Extent1].[c33_enum] AS int)";

                var query = context.AllTypes.Where(a => a.c33_enum == (ArubaEnum)1).Select(a => a.c33_enum);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that correct enums are filtered out (verify count and value)
                var results = query.ToList();
                var expectedCount = context.AllTypes.ToList().Count(a => a.c33_enum == (ArubaEnum)1);
                Assert.True(results.All(r => r == ArubaEnum.EnumValue1));
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
                    typeof(DbContext).Assembly,
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
                    typeof(DbContext).Assembly,
                    "ELinq_UnsupportedIsOrAs",
                    null, 
                    "TypeAs", 
                    "System.Data.Entity.TestModels.ArubaModel.ArubaEnum",
                    "System.Nullable`1[[System.Data.Entity.TestModels.ArubaModel.ArubaEnum, EntityFramework.FunctionalTests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a]]");
            }
        }

        [Fact]
        public void Unnamed_enum_constant_in_Where_clause()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
[Extent1].[c33_enum] AS [c33_enum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
WHERE 42 =  CAST( [Extent1].[c33_enum] AS int)";

                var query = context.AllTypes.Where(a => a.c33_enum == (ArubaEnum)42).Select(p => p.c33_enum);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify all results are filtered out
                var results = query.ToList();
                Assert.False(results.Any());
            }
        }

        [Fact]
        public void Enum_in_OrderBy_clause()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
                    @"SELECT 
[Extent1].[c33_enum] AS [c33_enum]
FROM [dbo].[ArubaAllTypes] AS [Extent1]
ORDER BY [Extent1].[c33_enum] ASC";

                var query = context.AllTypes.OrderBy(a => a.c33_enum).Select(a => a.c33_enum);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify order is correct
                var results = query.ToList();
                var highest = (int)results[0];
                foreach (var result in results)
                {
                    Assert.True((int)result >= highest);
                    highest = (int)result;
                }
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
        public void Enum_in_GroupBy_clause()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
                    @"SELECT 
[GroupBy1].[K1] AS [c33_enum], 
[GroupBy1].[A1] AS [C1]
FROM ( SELECT 
	[Extent1].[c33_enum] AS [K1], 
	COUNT(1) AS [A1]
	FROM [dbo].[ArubaAllTypes] AS [Extent1]
	GROUP BY [Extent1].[c33_enum]
)  AS [GroupBy1]";

                var query = context.AllTypes.GroupBy(a => a.c33_enum).Select(
                    g => new
                        {
                            g.Key,
                            Count = g.Count()
                        });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList().OrderBy(r => r.Key).ToList();
                var expected = context.AllTypes.ToList().GroupBy(a => a.c33_enum).Select(
                    g => new
                        {
                            g.Key,
                            Count = g.Count()
                        }).OrderBy(r => r.Key).ToList();
                QueryTestHelpers.VerifyQueryResult(expected, results, (o, i) => o.Key == i.Key && o.Count == i.Count);
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
        public void Enum_in_Join_clause()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
                    @"SELECT 
[Limit1].[c1_int] AS [c1_int], 
[Limit1].[c33_enum] AS [c33_enum], 
[Extent2].[c1_int] AS [c1_int1], 
[Extent2].[c33_enum] AS [c33_enum1]
FROM   (SELECT TOP (1) [c].[c1_int] AS [c1_int], [c].[c33_enum] AS [c33_enum]
	FROM [dbo].[ArubaAllTypes] AS [c] ) AS [Limit1]
INNER JOIN [dbo].[ArubaAllTypes] AS [Extent2] ON [Limit1].[c33_enum] = [Extent2].[c33_enum]";

                var query = context.AllTypes.Take(1).Join(
                    context.AllTypes, o => o.c33_enum, i => i.c33_enum, (o, i) => new
                        {
                            OuterKey = o.c1_int,
                            OuterEnum = o.c33_enum,
                            InnerKey = i.c1_int,
                            InnerEnum = i.c33_enum
                        });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all all entities with matching enum values are joined
                // and that correct enum values get joined
                var firstEnum = context.AllTypes.ToList().Take(1).First().c33_enum;
                var allTypesWithFirstEnumIds = context.AllTypes.ToList().Where(a => a.c33_enum == firstEnum).Select(a => a.c1_int).ToList();
                var results = query.ToList();
                Assert.Equal(allTypesWithFirstEnumIds.Count(), results.Count());
                foreach (var result in results)
                {
                    Assert.Equal(firstEnum, result.InnerEnum);
                    Assert.Equal(firstEnum, result.OuterEnum);
                    Assert.True(allTypesWithFirstEnumIds.Contains(result.OuterKey));
                }
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

        [Fact]
        public void Enum_with_coalesce_operator()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var expectedSql =
                    @"SELECT 
CASE WHEN (CASE WHEN (0 = ([Extent1].[c1_int] % 2)) THEN [Extent1].[c33_enum] END IS NULL) THEN 1 WHEN (0 = ([Extent1].[c1_int] % 2)) THEN [Extent1].[c33_enum] END AS [C1]
FROM [dbo].[ArubaAllTypes] AS [Extent1]";

                var query = context.AllTypes.Select(p => p.c1_int % 2 == 0 ? p.c33_enum : (ArubaEnum?)null)
                                   .Select(a => a ?? ArubaEnum.EnumValue1);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                var expected = context.AllTypes.ToList().Select(p => p.c1_int % 2 == 0 ? p.c33_enum : (ArubaEnum?)null)
                                      .Select(a => a ?? ArubaEnum.EnumValue1).ToList();

                Assert.Equal(expected.Count, results.Count);
                for (var i = 0; i < results.Count; i++)
                {
                    Assert.Equal(expected[i], results[i]);
                }
            }
        }

        [Fact]
        public void Casting_to_enum_undeclared_in_model_works()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
                    @"SELECT 
[Extent1].[c1_int] AS [c1_int]
FROM [dbo].[ArubaAllTypes] AS [Extent1]";

                var query = context.AllTypes.Select(a => (FileAccess)a.c1_int);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // not much to verify here, just make sure we don't throw
                query.ToList();
            }
        }
    }
}
