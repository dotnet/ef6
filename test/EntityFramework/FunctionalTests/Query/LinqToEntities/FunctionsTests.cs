// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;

    public class FunctionsTests : FunctionalTestBase
    {
        public class StringFunctions : FunctionalTestBase
        {
            [Fact]
            public void IsNullOrEmpty_translated_properly_to_expression()
            {
                using (var context = new ArubaContext())
                {
                    var expectedSql =
@"SELECT 
    CASE WHEN (([Extent1].[LastName] IS NULL) OR (( CAST(LEN([Extent1].[LastName]) AS int)) = 0)) THEN cast(1 as bit) WHEN ( NOT (([Extent1].[LastName] IS NULL) OR ((( CAST(LEN([Extent1].[LastName]) AS int)) = 0) AND ( CAST(LEN([Extent1].[LastName]) AS int) IS NOT NULL)))) THEN cast(0 as bit) END AS [C1]
    FROM [dbo].[ArubaOwners] AS [Extent1]";

                    var query = context.Owners.Select(o => string.IsNullOrEmpty(o.LastName));
                    QueryTestHelpers.VerifyDbQuery(query, expectedSql);
                }
            }

            [Fact]
            public void String_Contains_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.Owners.Select(o => o.LastName.Contains("Name"));
                    Assert.Contains("LIKE N'%NAME%'", query.ToString().ToUpperInvariant()); 
                }
            }

            [Fact]
            public void String_EndsWith_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.Owners.Select(o => o.LastName.EndsWith("Name"));
                    Assert.Contains("LIKE N'%NAME'", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void String_StartsWith_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.Owners.Select(o => o.LastName.StartsWith("Name"));
                    Assert.Contains("LIKE N'NAME%'", query.ToString().ToUpperInvariant());
                }
            }
        }

        public class DateTimeFunctions : FunctionalTestBase
        {
            [Fact]
            public void DateTime_Now_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.Owners.Select(o => DateTime.Now);
                    Assert.Contains("SYSDATETIME", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTime_UtcNow_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.Owners.Select(o => DateTime.UtcNow);
                    Assert.Contains("SYSUTCDATETIME", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Day_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c30_datetimeoffset.Day);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("DAY", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Hour_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c30_datetimeoffset.Hour);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("HOUR", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Millisecond_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c30_datetimeoffset.Millisecond);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("MILLISECOND", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Minute_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c30_datetimeoffset.Minute);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("MINUTE", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Month_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c30_datetimeoffset.Month);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("MONTH", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Second_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c30_datetimeoffset.Second);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("SECOND", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Year_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c30_datetimeoffset.Year);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("YEAR", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DateTimeOffest_Now_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => DateTimeOffset.Now);
                    Assert.Contains("SYSDATETIMEOFFSET", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void Timespan_Hours_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c27_time.Hours);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("HOUR", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void Timespan_Milliseconds_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c27_time.Milliseconds);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("MILLISECOND", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void Timespan_Minutes_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c27_time.Minutes);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("MINUTE", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void Timespan_Seconds_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => a.c27_time.Seconds);
                    Assert.Contains("DATEPART", query.ToString().ToUpperInvariant());
                    Assert.Contains("SECOND", query.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void DbFunctions_CreateDateTime_does_not_throw_if_year_does_not_have_4_digits()
            {
                using (var ctx = new ArubaContext())
                {
                    var query = ctx.AllTypes.Select(
                        a => a.c5_datetime == DbFunctions.CreateDateTime(8, 2, 29, 1, 1, 1));
                    Assert.DoesNotThrow(() => query.Count());

                    query = ctx.AllTypes.Select(
                        a => a.c6_smalldatetime == DbFunctions.CreateDateTime(8, 2, 29, 1, 1, 1));
                    Assert.DoesNotThrow(() => query.Count());

                    query = ctx.AllTypes.Select(
                        a => a.c28_date == DbFunctions.CreateDateTime(8, 2, 29, null, null, null));
                    Assert.DoesNotThrow(() => query.Count());

                    query = ctx.AllTypes.Select(
                        a => a.c29_datetime2 == DbFunctions.CreateDateTime(8, 2, 29, 1, 1, 1));
                    Assert.DoesNotThrow(() => query.Count());
                }
            }

            [Fact]
            public void DbFunctions_CreateDateTimeOffset_does_not_throw_if_year_does_not_have_4_digits()
            {
                using (var ctx = new ArubaContext())
                {
                    var query = ctx.AllTypes.Select(
                        a => a.c30_datetimeoffset == DbFunctions.CreateDateTimeOffset(8, 2, 29, 1, 1, 1, 1));
                    Assert.DoesNotThrow(() => query.Count());
                }
            }
        }

        public class MathFunctions : FunctionalTestBase
        {
            [Fact]
            public void Rounds_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query1 = context.AllTypes.Select(a => Math.Round(a.c7_decimal_28_4));
                    Assert.Contains("ROUND", query1.ToString().ToUpperInvariant());

                    var query2 = context.AllTypes.Select(a => Math.Round(a.c7_decimal_28_4, 2));
                    Assert.Contains("ROUND", query2.ToString().ToUpperInvariant());
                    Assert.Contains("2", query2.ToString());

                    var query3 = context.AllTypes.Select(a => Math.Round(a.c10_float));
                    Assert.Contains("ROUND", query3.ToString().ToUpperInvariant());

                    var query4 = context.AllTypes.Select(a => Math.Round(a.c10_float, 2));
                    Assert.Contains("ROUND", query4.ToString().ToUpperInvariant());
                    Assert.Contains("2", query2.ToString());
                }
            }

            [Fact]
            public void Abs_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query1 = context.AllTypes.Select(a => Math.Abs(a.c1_int));
                    Assert.Contains("ABS", query1.ToString().ToUpperInvariant());

                    var query2 = context.AllTypes.Select(a => Math.Abs(a.c2_smallint));
                    Assert.Contains("ABS", query2.ToString().ToUpperInvariant());

                    var query3 = context.AllTypes.Select(a => Math.Abs(a.c3_tinyint));
                    Assert.Contains("ABS", query3.ToString().ToUpperInvariant());

                    var query4 = context.AllTypes.Select(a => Math.Abs(a.c23_bigint));
                    Assert.Contains("ABS", query4.ToString().ToUpperInvariant());

                    var query5 = context.AllTypes.Select(a => Math.Abs(a.c7_decimal_28_4));
                    Assert.Contains("ABS", query5.ToString().ToUpperInvariant());

                    var query6 = context.AllTypes.Select(a => Math.Abs(a.c9_real));
                    Assert.Contains("ABS", query6.ToString().ToUpperInvariant());

                    var query7 = context.AllTypes.Select(a => Math.Abs(a.c10_float));
                    Assert.Contains("ABS", query7.ToString().ToUpperInvariant());
                }
            }

            [Fact]
            public void Power_properly_translated_to_function()
            {
                using (var context = new ArubaContext())
                {
                    var query = context.AllTypes.Select(a => Math.Pow(a.c10_float, a.c10_float));
                    Assert.Contains("POWER", query.ToString().ToUpperInvariant());
                }
            }
        }

        [Fact]
        public void NewGuid_translated_to_correct_function_in_database()
        {
            using (var context = new ArubaContext())
            {
                var query = context.Owners.Select(o => Guid.NewGuid());
                Assert.Contains("NEWID", query.ToString().ToUpperInvariant());
            }
        }

        [Fact]
        public void SqlFunctions_scalar_function_translated_properly_to_sql_function()
        {
            using (var context = new ArubaContext())
            {
                var query1 = context.AllTypes.Select(a => SqlFunctions.Acos(a.c7_decimal_28_4));
                var query2 = context.AllTypes.Select(a => SqlFunctions.Acos(a.c10_float));
                Assert.Contains("ACOS", query1.ToString().ToUpperInvariant());
                Assert.Contains("ACOS", query2.ToString().ToUpperInvariant());
            }
        }

        [Fact]
        public void SqlFunctions_aggregate_function_translated_properly_to_sql_function()
        {
            using (var context = new ArubaContext())
            {
                var query = context.AllTypes.Select(a => SqlFunctions.ChecksumAggregate(context.AllTypes.Select(i => i.c1_int)));
                Assert.Contains("CHECKSUM_AGG", query.ToString().ToUpperInvariant());
            }
        }

        [Fact]
        public void SqlFunction_passing_function_as_argument_to_another_works()
        {
            using (var context = new ArubaContext())
            {
                var query = context.AllTypes.Select(a => SqlFunctions.Asin(SqlFunctions.Acos(a.c10_float)));
                Assert.Contains("ASIN", query.ToString().ToUpperInvariant());
                Assert.Contains("ACOS", query.ToString().ToUpperInvariant());
            }
        }
    }
}
