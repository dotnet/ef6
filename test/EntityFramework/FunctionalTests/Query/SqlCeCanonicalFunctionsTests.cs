// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.Query
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestModels.ArubaCeModel;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// This class test all canonical functions added since EF1 against the SQL Server Compact Providers 
    /// </summary>
    public class SqlCeCanonicalFunctionsTests : FunctionalTestBase
    {
        #region Math Functions
        
        [Fact]
        public void CanonicalFunction_power_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int64>(@"SELECT VALUE Edm.Power(A.c23_bigint, 3) FROM ArubaCeContext.AllTypes AS A");

                Assert.Contains("POWER", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(1000, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_round_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Decimal>(@"SELECT VALUE Edm.Round(A.c7_decimal_28_4) FROM ArubaCeContext.AllTypes AS A");

                Assert.Contains("ROUND", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(10m, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_round_with_digits_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Decimal>(@"SELECT VALUE Edm.Round(A.c7_decimal_28_4,2) FROM ArubaCeContext.AllTypes AS A");

                Assert.Contains("ROUND", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(10.25m, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_truncate_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Decimal>(@"SELECT VALUE Edm.Truncate(A.c7_decimal_28_4,1) FROM ArubaCeContext.AllTypes AS A");

                Assert.Contains("ROUND", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(10.2m, query.First());
            }
        }
        
        #endregion

        #region Aggregate functions
        
        [Fact]
        public void CanonicalFunction_stdevp_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Double>(@"SELECT VALUE Edm.StDevP(A.c7_decimal_28_4) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandExecutionException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_var_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Double>(@"SELECT VALUE Edm.Var(A.c7_decimal_28_4) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandExecutionException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_varp_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Double>(@"SELECT VALUE Edm.VarP(A.c7_decimal_28_4) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandExecutionException>(() => query.First());
            }
        }
        
        #endregion

        #region String functions
        
        [Fact]
        public void CanonicalFunction_contains_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean>(@"SELECT VALUE Edm.Contains(A.c21_ntext, 'uni') FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("LIKE", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("CASE WHEN", query.ToTraceString().ToUpperInvariant());
                Assert.DoesNotContain("CHARINDEX", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(true, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_contains_with_null_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean?>(@"SELECT VALUE Edm.Contains(A.c21_ntext, NULL) FROM ArubaCeContext.AllTypes AS A");
                Assert.DoesNotContain("LIKE", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("CASE WHEN", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(null, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_startswith_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean>(@"SELECT VALUE Edm.StartsWith(A.c21_ntext, 'bbbbbb') FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("LIKE", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("CASE WHEN", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(true, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_startswith_with_null_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean?>(@"SELECT VALUE Edm.StartsWith(A.c21_ntext, NULL) FROM ArubaCeContext.AllTypes AS A");
                Assert.DoesNotContain("LIKE", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("CASE WHEN", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(null, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_endswith_constants_with_space_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean>(@"SELECT VALUE Edm.EndsWith('abcd ', 'cd') FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("LIKE", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("CASE WHEN", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(false, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_endswith_constants_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean>(@"SELECT VALUE Edm.EndsWith('abcd', 'cd') FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("LIKE", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("CASE WHEN", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(true, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_endswith_column_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean>(@"SELECT VALUE Edm.EndsWith(A.c21_ntext, 'corn') FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("LIKE", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("CASE WHEN", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(true, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_endswith_without_constants_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Boolean>(@"SELECT VALUE Edm.EndsWith(A.c21_ntext, A.c21_ntext) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandCompilationException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_left_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<String>(@"SELECT VALUE Edm.Left(A.c21_ntext, 4) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("SUBSTRING", query.ToTraceString().ToUpperInvariant());
                Assert.Equal("bbbb", query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_left_long_length_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<String>(@"SELECT VALUE Edm.Left(A.c21_ntext, 3999) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("SUBSTRING", query.ToTraceString().ToUpperInvariant());
                Assert.Equal("bbbbbbbbbbbbbbbbbbbbbunicorn", query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_left_constant_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<String>(@"SELECT VALUE Edm.Left('unicorn', 4) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("SUBSTRING", query.ToTraceString().ToUpperInvariant());
                Assert.Equal("unic", query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_right_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<String>(@"SELECT VALUE Edm.Right(A.c21_ntext, 4) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("SUBSTRING", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("DATALENGTH", query.ToTraceString().ToUpperInvariant());
                Assert.Equal("corn", query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_right_long_length_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<String>(@"SELECT VALUE Edm.Right(A.c21_ntext, 3999) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("SUBSTRING", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("DATALENGTH", query.ToTraceString().ToUpperInvariant());
                Assert.Equal("bbbbbbbbbbbbbbbbbbbbbunicorn", query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_right_constant_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<String>(@"SELECT VALUE Edm.Right('unicorn', 4) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("SUBSTRING", query.ToTraceString().ToUpperInvariant());
                Assert.Contains("DATALENGTH", query.ToTraceString().ToUpperInvariant());
                Assert.Equal("corn", query.First());
            }
        }

        #endregion

        #region Date and Time functions
        
        [Fact]
        public void CanonicalFunction_addnanoseconds_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddNanoseconds(A.c5_datetime, 100) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandCompilationException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_addmicroseconds_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddMicroseconds(A.c5_datetime, 100) FROM ArubaCeContext.AllTypes AS A");

                Assert.Throws<EntityCommandCompilationException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_addmilliseconds_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddMilliseconds(A.c5_datetime, 100) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(1990, 2, 2, 1, 1, 1, 100), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_addseconds_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddSeconds(A.c5_datetime, 10) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(1990, 2, 2, 1, 1, 11), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_addminutes_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddMinutes(A.c5_datetime, 10) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(1990, 2, 2, 1, 11, 1), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_addhours_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddHours(A.c5_datetime, 10) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(1990, 2, 2, 11, 1, 1), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_adddays_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddDays(A.c5_datetime, 10) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(1990, 2, 12, 1, 1, 1), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_addmonths_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddMonths(A.c5_datetime, 10) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(1990, 12, 2, 1, 1, 1), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_addyears_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.AddYears(A.c5_datetime, 10) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(2000, 2, 2, 1, 1, 1), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_createdatetimeoffset_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.CreateDateTimeOffset(2000, 2, 2, 1, 1, 1, 10) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandCompilationException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_createtime_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.CreateTime(1, 1, 1) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandCompilationException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_createdatetime_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.CreateDateTime(2000, 2, 2, 1, 1, 1) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("CONVERT", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(2000, 2, 2, 1, 1, 1), query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_dayofyear_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DayOfYear(DATETIME'2001-12-31 12:59:59.12345') FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEPART", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(365, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffnanoseconds_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffNanoseconds(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandCompilationException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffmicroseconds_is_not_supported()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffMicroseconds(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Throws<EntityCommandCompilationException>(() => query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffmilliseconds_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffMilliseconds(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEDIFF", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(0, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffseconds_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffSeconds(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEDIFF", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(0, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffminutes_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffMinutes(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEDIFF", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(0, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffhours_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffHours(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEDIFF", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(0, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffdays_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffDays(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEDIFF", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(0, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffmonths_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffMonths(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEDIFF", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(0, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_diffyears_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<Int32>(@"SELECT VALUE Edm.DiffYears(A.c5_datetime, A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEDIFF", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(0, query.First());
            }
        }

        [Fact]
        public void CanonicalFunction_truncatetime_translated_properly_to_sql_function()
        {
            using (var context = GetArubaCeContext())
            {
                var query = context.CreateQuery<DateTime>(@"SELECT VALUE Edm.TruncateTime(A.c5_datetime) FROM ArubaCeContext.AllTypes AS A");
                Assert.Contains("DATEADD(D, DATEDIFF(D,", query.ToTraceString().ToUpperInvariant());
                Assert.Equal(new DateTime(1990, 2, 2, 0, 0, 0), query.First());
            }
        }

        #endregion

        private ObjectContext GetArubaCeContext()
        {
            return (new ArubaCeContext("Scenario_Use_SqlCe_AppConfig_connection_string") as IObjectContextAdapter).ObjectContext;
        }
    }
}

#endif