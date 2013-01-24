// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using SimpleModel;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class FuncletizationTests : FunctionalTestBase
    {
        [Fact]
        public void Funcletize_ICollection_count_when_passed_as_parameter()
        {
            var localList = new List<int> { 1, 2, 3 };
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
@p__linq__0 AS [C1]
FROM [dbo].[Categories] AS [Extent1]

/*
Int32 p__linq__0 = 3
*/";
                var query = context.Categories.Select(c => localList.Count);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Funcletize_byte_array()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id]
FROM [dbo].[Categories] AS [Extent1]
WHERE  0x010203  =  0x010203";

                var query = context.Categories.Where(c => new byte[] { 1, 2, 3 } == new byte[] { 1, 2, 3 }).Select(c => c.Id);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Funcletize_decimal_constructors()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
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
FROM [dbo].[Categories] AS [Extent1]";

                var query = context.Categories.Select(c => new
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
            }
        }

        [Fact]
        public void Funcletize_string_constructors()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
1 AS [C1], 
N'aaaaa' AS [C2]
FROM [dbo].[Categories] AS [Extent1]";

                var query = context.Categories.Select(c => new
                {
                    b = new String('a', 5),
                });

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
            
        }

        [Fact]
        public void Funcletize_static_field()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
@p__linq__0 AS [C1]
FROM [dbo].[Categories] AS [Extent1]

/*
String p__linq__0 = ""staticField""
*/";

                var query = context.Categories.Select(c => StubClass.StaticField);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Funcletize_static_property()
        {
            using (var context = new SimpleLocalDbModelContextWithNoData())
            {
                var expectedSql =
@"SELECT 
@p__linq__0 AS [C1]
FROM [dbo].[Categories] AS [Extent1]

/*
String p__linq__0 = ""StaticProperty""
*/";

                var query = context.Categories.Select(c => StubClass.StaticProperty);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
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