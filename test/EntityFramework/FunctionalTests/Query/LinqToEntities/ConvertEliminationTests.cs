// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;

    public class ConvertEliminationTests : FunctionalTestBase
    {
        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 2 = [Extent1].[c38_shortenum]";

                var query = context.AllTypes.Where(a => a.c38_shortenum == ArubaShortEnum.ShortEnumValue2).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 2 = [Extent1].[c38_shortenum]";

                var query = context.AllTypes.Where(a => ArubaShortEnum.ShortEnumValue2 == a.c38_shortenum).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_parameter_with_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c38_shortenum] = @p__linq__0";

                var parameter = ArubaShortEnum.ShortEnumValue2;
                var query = context.AllTypes.Where(a => a.c38_shortenum == parameter).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_parameter_with_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE @p__linq__0 = [Extent1].[c38_shortenum]";

                var parameter = ArubaShortEnum.ShortEnumValue2;
                var query = context.AllTypes.Where(a => parameter == a.c38_shortenum).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_constant_with_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 1 = [Extent1].[c2_smallint]";

                var query = context.AllTypes.Where(a => a.c2_smallint == (short)1).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_constant_with_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 1 = [Extent1].[c2_smallint]";

                var query = context.AllTypes.Where(a => (short)1 == a.c2_smallint).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_parameter_with_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c2_smallint] = @p__linq__0";

                short parameter = 1;
                var query = context.AllTypes.Where(a => a.c2_smallint == parameter).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_parameter_with_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE @p__linq__0 = [Extent1].[c2_smallint]";

                short parameter = 1;
                var query = context.AllTypes.Where(a => parameter == a.c2_smallint).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r == 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_not_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 2 <> [Extent1].[c38_shortenum]";

                var query = context.AllTypes.Where(a => a.c38_shortenum != ArubaShortEnum.ShortEnumValue2).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_not_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 2 <> [Extent1].[c38_shortenum]";

                var query = context.AllTypes.Where(a => ArubaShortEnum.ShortEnumValue2 != a.c38_shortenum).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_parameter_with_not_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c38_shortenum] <> @p__linq__0";

                var parameter = ArubaShortEnum.ShortEnumValue2;
                var query = context.AllTypes.Where(a => a.c38_shortenum != parameter).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_parameter_with_not_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE @p__linq__0 <> [Extent1].[c38_shortenum]";

                var parameter = ArubaShortEnum.ShortEnumValue2;
                var query = context.AllTypes.Where(a => parameter != a.c38_shortenum).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_constant_with_not_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 1 <> [Extent1].[c2_smallint]";

                var query = context.AllTypes.Where(a => a.c2_smallint != (short)1).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_constant_with_not_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE 1 <> [Extent1].[c2_smallint]";

                var query = context.AllTypes.Where(a => (short)1 != a.c2_smallint).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_parameter_with_not_equal()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c2_smallint] <> @p__linq__0";

                short parameter = 1;
                var query = context.AllTypes.Where(a => a.c2_smallint != parameter).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_parameter_with_not_equal_flipped()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE @p__linq__0 <> [Extent1].[c2_smallint]";

                short parameter = 1;
                var query = context.AllTypes.Where(a => parameter != a.c2_smallint).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r != 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_less_than()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c38_shortenum] < 2";

                var query = context.AllTypes.Where(a => a.c38_shortenum < ArubaShortEnum.ShortEnumValue2).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r=> r < ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_parameter_with_less_than()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c38_shortenum] < @p__linq__0";

                var parameter = ArubaShortEnum.ShortEnumValue2;
                var query = context.AllTypes.Where(a => a.c38_shortenum < parameter).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r => r < ArubaShortEnum.ShortEnumValue2));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_constant_with_less_than()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c2_smallint] < 1";

                var query = context.AllTypes.Where(a => a.c2_smallint < (short)1).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r => r < 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_property_and_parameter_with_less_than()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c2_smallint] AS [c2_smallint]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c2_smallint] < @p__linq__0";

                short parameter = 1;
                var query = context.AllTypes.Where(a => a.c2_smallint < parameter).Select(a => a.c2_smallint);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r => r < 1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_less_than_or_equal_to()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c38_shortenum] <= 1";

                var query = context.AllTypes.Where(a => a.c38_shortenum <= ArubaShortEnum.ShortEnumValue1).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r => r <= ArubaShortEnum.ShortEnumValue1));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_greater_than()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c38_shortenum] > 0";

                var query = context.AllTypes.Where(a => a.c38_shortenum > ArubaShortEnum.ShortEnumValue0).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r => r > ArubaShortEnum.ShortEnumValue0));
            }
        }

        [Fact]
        public void No_casts_when_comparing_short_enum_property_and_constant_with_greater_than_or_equal_to()
        {
            using (var context = new ArubaContext())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                const string expectedSql =
                    @"SELECT 
	 [Extent1].[c38_shortenum] AS [c38_shortenum]
	 FROM [dbo].[ArubaAllTypes] AS [Extent1]
	 WHERE [Extent1].[c38_shortenum] >= 1";

                var query = context.AllTypes.Where(a => a.c38_shortenum >= ArubaShortEnum.ShortEnumValue1).Select(a => a.c38_shortenum);

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                Assert.True(query.ToList().All(r => r >= ArubaShortEnum.ShortEnumValue1));
            }
        }
    }
}
