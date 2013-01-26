// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class FuncletizationTests : FunctionalTestBase
    {
        [Fact]
        public void Funcletize_ICollection_count_when_passed_as_parameter()
        {
            var localList = new List<int> { 1, 2, 3 };
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
@p__linq__0 AS [C1]
FROM [dbo].[ArubaTasks] AS [Extent1]

/*
Int32 p__linq__0 = 3
*/";
                var query = context.Tasks.Select(t => localList.Count);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that correct value gets projected
                var results = query.ToList();
                Assert.True(results.All(r => r == 3));
            }
        }

        [Fact]
        public void Funcletize_byte_array()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[ArubaTasks] AS [Extent1]
WHERE  0x010203  =  0x010203";

                var query = context.Tasks.Where(c => new byte[] { 1, 2, 3 } == new byte[] { 1, 2, 3 }).Select(c => c.Id);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that filter is a no-op
                var results = query.ToList();
                var expectedCount = context.Tasks.Count();
                Assert.Equal(expectedCount, results.Count);
            }
        }

        [Fact]
        public void Funcletize_decimal_constructors()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
1.5 AS [C2], 
2.5 AS [C3], 
cast(5 as decimal(18)) AS [C4], 
cast(7 as decimal(18)) AS [C5], 
cast(9 as decimal(18)) AS [C6], 
cast(11 as decimal(18)) AS [C7], 
0.0013 AS [C8]
FROM [dbo].[ArubaTasks] AS [Extent1]";

                var query = context.Tasks.Select(c => new
                    {
                        a = new Decimal(1.5), 
                        b = new Decimal((float)2.5), 
                        c = new Decimal(5), 
                        e = new Decimal((long)7),
                        f = new Decimal((uint)9),
                        g = new Decimal((ulong)11),
                        h = new Decimal(13, 0, 0, false, 4),
                    });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var results = query.ToList();
                foreach (var result in results)
                {
                    Assert.Equal((decimal)1.5, result.a);
                    Assert.Equal((decimal)2.5, result.b);
                    Assert.Equal(5, result.c);
                    Assert.Equal(7, result.e);
                    Assert.Equal(9, result.f);
                    Assert.Equal(11, result.g);
                    Assert.Equal((decimal)0.0013, result.h);
                }
            }
        }

        [Fact]
        public void Funcletize_string_constructors()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
N'aaaaa' AS [C2]
FROM [dbo].[ArubaTasks] AS [Extent1]";

                var query = context.Tasks.Select(c => new
                {
                    b = new String('a', 5),
                });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var result = query.ToList();
                Assert.True(result.All(r => r.b == "aaaaa"));
            }
        }

        [Fact]
        public void Funcletize_static_field()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
@p__linq__0 AS [C1]
FROM [dbo].[ArubaTasks] AS [Extent1]

/*
String p__linq__0 = ""staticField""
*/";

                var query = context.Tasks.Select(c => StubClass.StaticField);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var result = query.ToList();
                Assert.True(result.All(r => r == "staticField"));
            }
        }

        [Fact]
        public void Funcletize_static_property()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
@p__linq__0 AS [C1]
FROM [dbo].[ArubaTasks] AS [Extent1]

/*
String p__linq__0 = ""StaticProperty""
*/";

                var query = context.Tasks.Select(c => StubClass.StaticProperty);
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var result = query.ToList();
                Assert.True(result.All(r => r == "StaticProperty"));
            }
        }

        public static class StubClass
        {
            public static string StaticField = "staticField";

            public static string StaticProperty { get; set; }

            static StubClass()
            {
                StaticProperty = "StaticProperty";
            }
        }
    }
}