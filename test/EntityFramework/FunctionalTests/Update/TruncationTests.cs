// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Update
{
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.TestModels.ArubaCeModel;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;
    using Xunit.Extensions;

    public class TruncationTests : FunctionalTestBase
    {
        public TruncationTests()
        {
            using (var context = new ArubaContext())
            {
                context.Database.Initialize(force: false);
            }
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_relies_on_old_behavior_to_truncate_continues_to_work_by_default()
        {
            InsertAndUpdateWithDecimals(
                9.88888888888888888888888888888888m, 9.8888m,
                11.88888888888888888888888888888888m, 11.8888m);
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_relies_on_old_behavior_to_truncate_behaves_differently_when_flag_is_changed()
        {
            RunWithTruncateFlag(
                () => InsertAndUpdateWithDecimals(
                    9.88888888888888888888888888888888m, 9.8889m,
                    11.88888888888888888888888888888888m, 11.8889m));
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_relies_on_old_behavior_to_round_continues_to_work_by_default()
        {
            InsertAndUpdateWithDecimals(
                9.11111111111111111111111111111111m + 0.00005m, 9.1111m,
                11.11111111111111111111111111111111m + 0.00005m, 11.1111m);
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_relies_on_old_behavior_to_round_behaves_differently_when_flag_is_changed()
        {
            RunWithTruncateFlag(
                () => InsertAndUpdateWithDecimals(
                    9.11111111111111111111111111111111m + 0.00005m, 9.1112m,
                    11.11111111111111111111111111111111m + 0.00005m, 11.1112m));
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_explicitly_rounds_continues_to_work_by_default()
        {
            InsertAndUpdateWithDecimals(
                Math.Round(9.88888888888888888888888888888888m, 4), 9.8889m,
                Math.Round(11.88888888888888888888888888888888m, 4), 11.8889m);
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_explicitly_rounds_continues_to_work_when_flag_is_changed()
        {
            RunWithTruncateFlag(
                () => InsertAndUpdateWithDecimals(
                    Math.Round(9.88888888888888888888888888888888m, 4), 9.8889m,
                    Math.Round(11.88888888888888888888888888888888m, 4), 11.8889m));
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_explicitly_truncates_continues_to_work_by_default()
        {
            InsertAndUpdateWithDecimals(
                Math.Truncate(9.88888888888888888888888888888888m * 10000) / 10000, 9.8888m,
                Math.Truncate(11.88888888888888888888888888888888m * 10000) / 10000, 11.8888m);
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Legacy_code_that_explicitly_truncates_continues_to_work_when_flag_is_changed()
        {
            RunWithTruncateFlag(
                () => InsertAndUpdateWithDecimals(
                    Math.Truncate(9.88888888888888888888888888888888m * 10000) / 10000, 9.8888m,
                    Math.Truncate(11.88888888888888888888888888888888m * 10000) / 10000, 11.8888m));
        }

        private static void RunWithTruncateFlag(Action test)
        {
            var oldValue = SqlProviderServices.TruncateDecimalsToScale;
            SqlProviderServices.TruncateDecimalsToScale = false;
            try
            {
                test();
            }
            finally
            {
                SqlProviderServices.TruncateDecimalsToScale = oldValue;
            }
        }

        private static void InsertAndUpdateWithDecimals(
            decimal insertValue, decimal insertExpected, decimal updateValue, decimal updateExpected)
        {
            using (var context = new ArubaContext())
            {
                // Insert
                var allTypes = context.AllTypes.Add(CreateArubaAllTypes(insertValue));
                context.SaveChanges();

                ValidateSavedValues(context, allTypes, insertExpected);

                // Update
                UpdateArubaAllTypes(allTypes, updateValue);
                context.SaveChanges();

                ValidateSavedValues(context, allTypes, updateExpected);
            }
        }

        private static void ValidateSavedValues(ArubaContext context, ArubaAllTypes allTypes, decimal value)
        {
            var saved = context.AllTypes.AsNoTracking().Single(t => t.c1_int == allTypes.c1_int);

            Assert.Equal(value, saved.c7_decimal_28_4);
            Assert.Equal(value, saved.c8_numeric_28_4);
            Assert.Equal(value, saved.c11_money);
            Assert.Equal(value, saved.c12_smallmoney);
        }

        private static ArubaAllTypes CreateArubaAllTypes(decimal value)
        {
            return new ArubaAllTypes
                {
                    c7_decimal_28_4 = value,
                    c8_numeric_28_4 = value,
                    c11_money = value,
                    c12_smallmoney = value,
                    c5_datetime = DateTime.Now,
                    c6_smalldatetime = DateTime.Now
                };
        }

        private static void UpdateArubaAllTypes(ArubaAllTypes allTypes, decimal value)
        {
            allTypes.c7_decimal_28_4 = value;
            allTypes.c8_numeric_28_4 = value;
            allTypes.c11_money = value;
            allTypes.c12_smallmoney = value;
        }

        [Fact]
        public void SQL_Compact_does_not_truncate_decimals_on_insert_or_update()
        {
            using (var context = new ArubaCeContext("Scenario_Use_SqlCe_AppConfig_connection_string"))
            {
                context.Database.Initialize(force: false);

                using (context.Database.BeginTransaction())
                {
                    // Insert
                    var allTypes = context.AllTypes.Add(
                        new ArubaAllCeTypes
                            {
                                c7_decimal_28_4 = 9.88888888888888888888888888888888m,
                                c8_numeric_28_4 = 9.88888888888888888888888888888888m,
                                c5_datetime = DateTime.Now,
                            });
                    context.SaveChanges();

                    ValidateSavedValues(context, allTypes, 9.8889m);

                    // Update
                    allTypes.c7_decimal_28_4 = 11.88888888888888888888888888888888m;
                    allTypes.c8_numeric_28_4 = 11.88888888888888888888888888888888m;
                    context.SaveChanges();

                    ValidateSavedValues(context, allTypes, 11.8889m);
                }
            }
        }

        private static void ValidateSavedValues(ArubaCeContext context, ArubaAllCeTypes allTypes, decimal value)
        {
            var saved = context.AllTypes.AsNoTracking().Single(t => t.c1_int == allTypes.c1_int);

            Assert.Equal(value, saved.c7_decimal_28_4);
            Assert.Equal(value, saved.c8_numeric_28_4);
        }

        [Fact]
        public void SQL_Compact_always_truncates_money_on_insert_or_update()
        {
            using (var context = new ArubaCeContext("Scenario_Use_SqlCe_AppConfig_connection_string"))
            {
                context.Database.Initialize(force: false);

                using (context.Database.BeginTransaction())
                {
                    // Insert
                    var allTypes = context.AllTypes.Add(
                        new ArubaAllCeTypes
                            {
                                c11_money = 9.88888888888888888888888888888888m,
                                c5_datetime = DateTime.Now,
                            });
                    context.SaveChanges();

                    Assert.Equal(9.8888m, context.AllTypes.AsNoTracking().Single(t => t.c1_int == allTypes.c1_int).c11_money);

                    // Update
                    allTypes.c11_money = 11.88888888888888888888888888888888m;
                    context.SaveChanges();

                    Assert.Equal(11.8888m, context.AllTypes.AsNoTracking().Single(t => t.c1_int == allTypes.c1_int).c11_money);
                }
            }
        }
    }
}
