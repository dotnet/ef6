// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.ProductivityApi
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.WrappingProvider;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;

    public class DbFunctionScenarios : FunctionalTestBase
    {
        public class StandardDeviation : FunctionalTestBase
        {
            [Fact]
            public void StandardDeviation_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.Decimal)), 2);
                    Assert.Equal(
                        0.71, (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)), 2);
                }
            }

            [Fact]
            public void StandardDeviation_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                    Assert.Equal(
                        0.71,
                        (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)), 2);
                }
            }

            [Fact]
            public void StandardDeviation_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.Double)), 2);
                    Assert.Equal(
                        0.71, (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                }
            }

            [Fact]
            public void StandardDeviation_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableDouble)), 2);
                    Assert.Equal(
                        0.71,
                        (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)), 2);
                }
            }

            [Fact]
            public void StandardDeviation_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.Int)), 2);
                    Assert.Equal(
                        0.71, (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                }
            }

            [Fact]
            public void StandardDeviation_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableInt)), 2);
                    Assert.Equal(
                        0.71, (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)),
                        2);
                }
            }

            [Fact]
            public void StandardDeviation_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.Long)), 2);
                    Assert.Equal(
                        0.71, (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                }
            }

            [Fact]
            public void StandardDeviation_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.71, (double)DbFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableLong)), 2);
                    Assert.Equal(
                        0.71, (double)DbFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)),
                        2);
                }
            }

            [Fact]
            public void StandardDeviation_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        0.71, (double)context.WithRelationships.Select(
                            e => new
                                {
                                    Result = DbFunctions.StandardDeviation(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);

                    Assert.Equal(
                        0.71, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                            e => new
                                {
                                    Result = DbFunctions.StandardDeviation(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);
                }
            }
        }

        public class StandardDeviationP : FunctionalTestBase
        {
            [Fact]
            public void StandardDeviationP_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Decimal)), 2);
                    Assert.Equal(
                        0.5, (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)), 2);
                }
            }

            [Fact]
            public void StandardDeviationP_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                    Assert.Equal(
                        0.5,
                        (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)), 2);
                }
            }

            [Fact]
            public void StandardDeviationP_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Double)), 2);
                    Assert.Equal(
                        0.5, (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                }
            }

            [Fact]
            public void StandardDeviationP_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableDouble)), 2);
                    Assert.Equal(
                        0.5,
                        (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)), 2);
                }
            }

            [Fact]
            public void StandardDeviationP_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Int)), 2);
                    Assert.Equal(
                        0.5, (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                }
            }

            [Fact]
            public void StandardDeviationP_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableInt)), 2);
                    Assert.Equal(
                        0.5, (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)),
                        2);
                }
            }

            [Fact]
            public void StandardDeviationP_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Long)), 2);
                    Assert.Equal(
                        0.5, (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                }
            }

            [Fact]
            public void StandardDeviationP_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableLong)), 2);
                    Assert.Equal(
                        0.5, (double)DbFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)),
                        2);
                }
            }

            [Fact]
            public void StandardDeviationP_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        0.5, (double)context.WithRelationships.Select(
                            e => new
                                {
                                    Result = DbFunctions.StandardDeviationP(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);

                    Assert.Equal(
                        0.5, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                            e => new
                                {
                                    Result = DbFunctions.StandardDeviationP(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);
                }
            }
        }

        public class Var : FunctionalTestBase
        {
            [Fact]
            public void Var_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.Decimal)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)), 2);
                }
            }

            [Fact]
            public void Var_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)), 2);
                }
            }

            [Fact]
            public void Var_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.Double)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                }
            }

            [Fact]
            public void Var_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.NullableDouble)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)), 2);
                }
            }

            [Fact]
            public void Var_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.Int)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                }
            }

            [Fact]
            public void Var_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.NullableInt)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)), 2);
                }
            }

            [Fact]
            public void Var_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.Long)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                }
            }

            [Fact]
            public void Var_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.NullableLong)), 2);
                    Assert.Equal(0.5, (double)DbFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)), 2);
                }
            }

            [Fact]
            public void Var_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        0.5, (double)context.WithRelationships.Select(
                            e => new
                                {
                                    Result = DbFunctions.Var(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);

                    Assert.Equal(
                        0.5, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                            e => new
                                {
                                    Result = DbFunctions.Var(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);
                }
            }
        }

        public class VarP : FunctionalTestBase
        {
            [Fact]
            public void VarP_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.Decimal)), 2);
                    Assert.Equal(0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)), 2);
                }
            }

            [Fact]
            public void VarP_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                    Assert.Equal(
                        0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)), 2);
                }
            }

            [Fact]
            public void VarP_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.Double)), 2);
                    Assert.Equal(0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                }
            }

            [Fact]
            public void VarP_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.NullableDouble)), 2);
                    Assert.Equal(
                        0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)), 2);
                }
            }

            [Fact]
            public void VarP_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.Int)), 2);
                    Assert.Equal(0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                }
            }

            [Fact]
            public void VarP_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.NullableInt)), 2);
                    Assert.Equal(0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)), 2);
                }
            }

            [Fact]
            public void VarP_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.Long)), 2);
                    Assert.Equal(0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                }
            }

            [Fact]
            public void VarP_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(0.25, (double)DbFunctions.VarP(context.WithTypes.Select(e => e.NullableLong)), 2);
                    Assert.Equal(0.25, (double)DbFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)), 2);
                }
            }

            [Fact]
            public void VarP_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        0.25, (double)context.WithRelationships.Select(
                            e => new
                                {
                                    Result = DbFunctions.VarP(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);

                    Assert.Equal(
                        0.25, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                            e => new
                                {
                                    Result = DbFunctions.VarP(e.Types.Select(t => t.Decimal))
                                }).First().Result, 2);
                }
            }
        }

        public class StringFunctions : FunctionalTestBase
        {
            [Fact]
            public void Left_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        "Magic Unic",
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.Left(e.String, 10)).First());

                    Assert.Equal(
                        "Magic Unic",
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(e => DbFunctions.Left(e.String, 10)).First());
                }
            }

            [Fact]
            public void Right_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        "corns Rock",
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.Right(e.String, 10)).First());

                    Assert.Equal(
                        "corns Rock",
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(e => DbFunctions.Right(e.String, 10)).First());
                }
            }

            [Fact]
            public void Reverse_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        "kcoR snrocinU cigaM",
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.Reverse(e.String)).First());

                    Assert.Equal(
                        "kcoR snrocinU cigaM",
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(e => DbFunctions.Reverse(e.String)).First());
                }
            }

            [Fact]
            public void AsUnicode_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        "Magic Unicorns Rock And Roll All Night Long ツ",
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => e.String + DbFunctions.AsUnicode(" And Roll All Night Long ツ")).First());

                    Assert.Equal(
                        "Magic Unicorns Rock And Roll All Night Long ツ",
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => e.String + DbFunctions.AsUnicode(" And Roll All Night Long ツ")).First());
                }
            }

            [Fact]
            public void AsNonUnicode_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        "Magic Unicorns Rock And Roll All Night Long ?",
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => e.String + DbFunctions.AsNonUnicode(" And Roll All Night Long ツ")).First());

                    Assert.Equal(
                        "Magic Unicorns Rock And Roll All Night Long ?",
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => e.String + DbFunctions.AsNonUnicode(" And Roll All Night Long ツ")).First());
                }
            }
        }

        public class DateTimeFunctions : FunctionalTestBase
        {
            private const long EF41Ticks = 634380912600000000;
            private static readonly DateTime _ef43DateTime = new DateTime(2012, 2, 29, 4, 3, 1, 0, DateTimeKind.Utc);
            private static readonly DateTimeOffset _ef43Offset = new DateTimeOffset(2012, 2, 29, 4, 3, 1, 0, new TimeSpan(8, 0, 0));

            [Fact]
            public void GetTotalOffsetMinutes_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        8 * 60,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.GetTotalOffsetMinutes(e.DateTimeOffset)).First());

                    Assert.Equal(
                        8 * 60,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.GetTotalOffsetMinutes(e.DateTimeOffset)).First());
                }
            }

            [Fact]
            public void TruncateTime_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 0, 0, 0, 0, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.TruncateTime(e.DateTimeOffset)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 0, 0, 0, 0, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.TruncateTime(e.DateTimeOffset)).First());
                }
            }

            [Fact]
            public void TruncateTime_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 4, 11, 0, 0, 0, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.TruncateTime(e.DateTime)).First());

                    Assert.Equal(
                        new DateTime(2011, 4, 11, 0, 0, 0, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.TruncateTime(e.DateTime)).First());
                }
            }

            [Fact]
            public void CreateDateTime_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 4, 11, 0, 0, 1, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.CreateDateTime(2011, 4, 11, 0, 0, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2011, 4, 11, 0, 0, 1, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.CreateDateTime(2011, 4, 11, 0, 0, e.Int)).First());
                }
            }

            [Fact]
            public void CreateDateTimeOffset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 0, 0, 1, 0, new TimeSpan(0, 1, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.CreateDateTimeOffset(2011, 4, 11, 0, 0, e.Int, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 0, 0, 1, 0, new TimeSpan(0, 1, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.CreateDateTimeOffset(2011, 4, 11, 0, 0, e.Int, e.Int)).First());
                }
            }

            [Fact]
            public void CreateTime_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new TimeSpan(1, 2, 3),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.CreateTime(e.Int, 2, 3)).First());

                    Assert.Equal(
                        new TimeSpan(1, 2, 3),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.CreateTime(e.Int, 2, 3)).First());
                }
            }

            [Fact]
            public void AddYears_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2012, 4, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddYears(e.DateTimeOffset, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2012, 4, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddYears(e.DateTimeOffset, e.Int)).First());
                }
            }

            [Fact]
            public void AddYears_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2012, 4, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddYears(e.DateTime, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2012, 4, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddYears(e.DateTime, e.Int)).First());
                }
            }

            [Fact]
            public void AddMonths_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 5, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMonths(e.DateTimeOffset, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 5, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMonths(e.DateTimeOffset, e.Int)).First());
                }
            }

            [Fact]
            public void AddMonths_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 5, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMonths(e.DateTime, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2011, 5, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMonths(e.DateTime, e.Int)).First());
                }
            }

            [Fact]
            public void AddDays_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 12, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddDays(e.DateTimeOffset, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 12, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddDays(e.DateTimeOffset, e.Int)).First());
                }
            }

            [Fact]
            public void AddDays_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 4, 12, 4, 1, 0, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddDays(e.DateTime, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2011, 4, 12, 4, 1, 0, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddDays(e.DateTime, e.Int)).First());
                }
            }

            [Fact]
            public void AddHours_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 5, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddHours(e.DateTimeOffset, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 5, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddHours(e.DateTimeOffset, e.Int)).First());
                }
            }

            [Fact]
            public void AddHours_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 4, 11, 5, 1, 0, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddHours(e.DateTime, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2011, 4, 11, 5, 1, 0, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddHours(e.DateTime, e.Int)).First());
                }
            }

            [Fact]
            public void AddHours_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new TimeSpan(5, 1, 0),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddHours(e.TimeSpan, e.Int)).First());

                    Assert.Equal(
                        new TimeSpan(5, 1, 0),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddHours(e.TimeSpan, e.Int)).First());
                }
            }

            [Fact]
            public void AddMinutes_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 4, 2, 0, 0, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMinutes(e.DateTimeOffset, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 4, 2, 0, 0, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMinutes(e.DateTimeOffset, e.Int)).First());
                }
            }

            [Fact]
            public void AddMinutes_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 4, 11, 4, 2, 0, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMinutes(e.DateTime, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2011, 4, 11, 4, 2, 0, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMinutes(e.DateTime, e.Int)).First());
                }
            }

            [Fact]
            public void AddMinutes_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new TimeSpan(4, 2, 0),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMinutes(e.TimeSpan, e.Int)).First());

                    Assert.Equal(
                        new TimeSpan(4, 2, 0),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMinutes(e.TimeSpan, e.Int)).First());
                }
            }

            [Fact]
            public void AddSeconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 4, 1, 1, 0, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddSeconds(e.DateTimeOffset, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 4, 1, 1, 0, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddSeconds(e.DateTimeOffset, e.Int)).First());
                }
            }

            [Fact]
            public void AddSeconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 4, 11, 4, 1, 1, 0, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddSeconds(e.DateTime, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2011, 4, 11, 4, 1, 1, 0, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddSeconds(e.DateTime, e.Int)).First());
                }
            }

            [Fact]
            public void AddSeconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new TimeSpan(4, 1, 1),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddSeconds(e.TimeSpan, e.Int)).First());

                    Assert.Equal(
                        new TimeSpan(4, 1, 1),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddSeconds(e.TimeSpan, e.Int)).First());
                }
            }

            [Fact]
            public void AddMilliseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 4, 1, 0, 1, new TimeSpan(8, 0, 0)),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMilliseconds(e.DateTimeOffset, e.Int)).First());

                    Assert.Equal(
                        new DateTimeOffset(2011, 4, 11, 4, 1, 0, 1, new TimeSpan(8, 0, 0)),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMilliseconds(e.DateTimeOffset, e.Int)).First());
                }
            }

            [Fact]
            public void AddMilliseconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new DateTime(2011, 4, 11, 4, 1, 0, 1, DateTimeKind.Utc),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMilliseconds(e.DateTime, e.Int)).First());

                    Assert.Equal(
                        new DateTime(2011, 4, 11, 4, 1, 0, 1, DateTimeKind.Utc),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMilliseconds(e.DateTime, e.Int)).First());
                }
            }

            [Fact]
            public void AddMilliseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new TimeSpan(0, 4, 1, 0, 1),
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMilliseconds(e.TimeSpan, e.Int)).First());

                    Assert.Equal(
                        new TimeSpan(0, 4, 1, 0, 1),
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMilliseconds(e.TimeSpan, e.Int)).First());
                }
            }

            [Fact]
            public void AddMicroseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        EF41Ticks + 10,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMicroseconds(e.DateTimeOffset, e.Int)).First().
                                Value.Ticks);

                    Assert.Equal(
                        EF41Ticks + 10,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMicroseconds(e.DateTimeOffset, e.Int)).First().Value.Ticks);
                }
            }

            [Fact]
            public void AddMicroseconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        EF41Ticks + 10,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMicroseconds(e.DateTime, e.Int)).First().Value.
                                Ticks);

                    Assert.Equal(
                        EF41Ticks + 10,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMicroseconds(e.DateTime, e.Int)).First().Value.Ticks);
                }
            }

            [Fact]
            public void AddMicroseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new TimeSpan(4, 1, 0).Ticks + 10,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddMicroseconds(e.TimeSpan, e.Int)).First().Value.Ticks);

                    Assert.Equal(
                        new TimeSpan(4, 1, 0).Ticks + 10,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddMicroseconds(e.TimeSpan, e.Int)).First().Value.Ticks);
                }
            }

            [Fact]
            public void AddNanoseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        EF41Ticks + 1,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddNanoseconds(e.DateTimeOffset, e.Int * 100)).
                                First().
                                Value.Ticks);

                    Assert.Equal(
                        EF41Ticks + 1,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddNanoseconds(e.DateTimeOffset, e.Int * 100)).First().Value.Ticks);
                }
            }

            [Fact]
            public void AddNanoseconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        EF41Ticks + 1,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddNanoseconds(e.DateTime, e.Int * 100)).First().
                                Value.
                                Ticks);

                    Assert.Equal(
                        EF41Ticks + 1,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddNanoseconds(e.DateTime, e.Int * 100)).First().Value.Ticks);
                }
            }

            [Fact]
            public void AddNanoseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        new TimeSpan(4, 1, 0).Ticks + 1,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.AddNanoseconds(e.TimeSpan, e.Int * 100)).First().Value.
                                Ticks);

                    Assert.Equal(
                        new TimeSpan(4, 1, 0).Ticks + 1,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.AddNanoseconds(e.TimeSpan, e.Int * 100)).First().Value.Ticks);
                }
            }

            [Fact]
            public void DiffYears_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1, context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffYears(e.DateTimeOffset, _ef43Offset)).First());

                    Assert.Equal(
                        1,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffYears(e.DateTimeOffset, _ef43Offset)).First());
                }
            }

            [Fact]
            public void DiffYears_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1, context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffYears(e.DateTime, _ef43DateTime)).First());

                    Assert.Equal(
                        1,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffYears(e.DateTime, _ef43DateTime)).First());
                }
            }

            [Fact]
            public void DiffMonths_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        10,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffMonths(e.DateTimeOffset, _ef43Offset)).First());

                    Assert.Equal(
                        10,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMonths(e.DateTimeOffset, _ef43Offset)).First());
                }
            }

            [Fact]
            public void DiffMonths_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        10, context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffMonths(e.DateTime, _ef43DateTime)).First());

                    Assert.Equal(
                        10,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMonths(e.DateTime, _ef43DateTime)).First());
                }
            }

            [Fact]
            public void DiffDays_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        324,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffDays(e.DateTimeOffset, _ef43Offset)).First());

                    Assert.Equal(
                        324,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffDays(e.DateTimeOffset, _ef43Offset)).First());
                }
            }

            [Fact]
            public void DiffDays_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        324, context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffDays(e.DateTime, _ef43DateTime)).First());

                    Assert.Equal(
                        324,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffDays(e.DateTime, _ef43DateTime)).First());
                }
            }

            [Fact]
            public void DiffHours_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        7776,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffHours(e.DateTimeOffset, _ef43Offset)).First());

                    Assert.Equal(
                        7776,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffHours(e.DateTimeOffset, _ef43Offset)).First());
                }
            }

            [Fact]
            public void DiffHours_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        7776, context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffHours(e.DateTime, _ef43DateTime)).First());

                    Assert.Equal(
                        7776,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffHours(e.DateTime, _ef43DateTime)).First());
                }
            }

            [Fact]
            public void DiffHours_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffHours(e.TimeSpan, new TimeSpan(5, 1, 0))).First());

                    Assert.Equal(
                        1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffHours(e.TimeSpan, new TimeSpan(5, 1, 0))).First());
                }
            }

            [Fact]
            public void DiffMinutes_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        466562,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffMinutes(e.DateTimeOffset, _ef43Offset)).First());

                    Assert.Equal(
                        466562,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMinutes(e.DateTimeOffset, _ef43Offset)).First());
                }
            }

            [Fact]
            public void DiffMinutes_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        466562,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffMinutes(e.DateTime, _ef43DateTime)).First());

                    Assert.Equal(
                        466562,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMinutes(e.DateTime, _ef43DateTime)).First());
                }
            }

            [Fact]
            public void DiffMinutes_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMinutes(e.TimeSpan, new TimeSpan(4, 2, 0))).First());

                    Assert.Equal(
                        1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMinutes(e.TimeSpan, new TimeSpan(4, 2, 0))).First());
                }
            }

            [Fact]
            public void DiffSeconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        27993721,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffSeconds(e.DateTimeOffset, _ef43Offset)).First());

                    Assert.Equal(
                        27993721,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffSeconds(e.DateTimeOffset, _ef43Offset)).First());
                }
            }

            [Fact]
            public void DiffSeconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        27993721,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffSeconds(e.DateTime, _ef43DateTime)).First());

                    Assert.Equal(
                        27993721,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffSeconds(e.DateTime, _ef43DateTime)).First());
                }
            }

            [Fact]
            public void DiffSeconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1,
                        context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.DiffSeconds(e.TimeSpan, new TimeSpan(4, 1, 1))).First());

                    Assert.Equal(
                        1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffSeconds(e.TimeSpan, new TimeSpan(4, 1, 1))).First());
                }
            }

            [Fact]
            public void DiffMilliseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        100,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e =>
                            DbFunctions.DiffMilliseconds(
                                e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());

                    Assert.Equal(
                        100,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e =>
                            DbFunctions.DiffMilliseconds(
                                e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());
                }
            }

            [Fact]
            public void DiffMilliseconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        100,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMilliseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc))).First());

                    Assert.Equal(
                        100,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMilliseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc))).First());
                }
            }

            [Fact]
            public void DiffMilliseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMilliseconds(e.TimeSpan, new TimeSpan(0, 4, 1, 0, 1))).First());

                    Assert.Equal(
                        1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMilliseconds(e.TimeSpan, new TimeSpan(0, 4, 1, 0, 1))).First());
                }
            }

            [Fact]
            public void DiffMicroseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        100000,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e =>
                            DbFunctions.DiffMicroseconds(
                                e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());

                    Assert.Equal(
                        100000,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e =>
                            DbFunctions.DiffMicroseconds(
                                e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());
                }
            }

            [Fact]
            public void DiffMicroseconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        100000,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMicroseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc))).First());

                    Assert.Equal(
                        100000,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMicroseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc))).First());
                }
            }

            [Fact]
            public void DiffMicroseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    var ticks = new TimeSpan(4, 1, 0).Ticks + 10;
                    Assert.Equal(
                        1, context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMicroseconds(e.TimeSpan, new TimeSpan(ticks))).First());

                    Assert.Equal(
                        1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffMicroseconds(e.TimeSpan, new TimeSpan(ticks))).First());
                }
            }

            [Fact]
            public void DiffNanoseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        100000000,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e =>
                            DbFunctions.DiffNanoseconds(
                                e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());

                    Assert.Equal(
                        100000000,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e =>
                            DbFunctions.DiffNanoseconds(
                                e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());
                }
            }

            [Fact]
            public void DiffNanoseconds_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        100000000,
                        context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffNanoseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc))).First());

                    Assert.Equal(
                        100000000,
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffNanoseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc))).First());
                }
            }

            [Fact]
            public void DiffNanoseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    var ticks = new TimeSpan(4, 1, 0).Ticks + 1;
                    Assert.Equal(
                        100, context.WithTypes.OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffNanoseconds(e.TimeSpan, new TimeSpan(ticks))).First());

                    Assert.Equal(
                        100, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.DiffNanoseconds(e.TimeSpan, new TimeSpan(ticks))).First());
                }
            }
        }

        public class Truncate : FunctionalTestBase
        {
            [Fact]
            public void Truncate_on_double_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1.0, (double)context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.Truncate(e.Double, e.Int)).First(), 7);

                    Assert.Equal(
                        1.0, (double)GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.Truncate(e.Double, e.Int)).First(), 7);
                }
            }

            [Fact]
            public void Truncate_on_decimal_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1, (decimal)context.WithTypes.OrderBy(e => e.Id).Select(e => DbFunctions.Truncate(e.Decimal, e.Int)).First(), 7);

                    Assert.Equal(
                        1, (decimal)GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => DbFunctions.Truncate(e.Decimal, e.Int)).First(), 7);
                }
            }
        }

        public class MathTruncate : FunctionalTestBase
        {
            [Fact]
            public void MathTruncate_on_double_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1.0, (double)context.WithTypes.OrderBy(e => e.Id).Select(e => Math.Truncate(e.Double)).First(), 7);

                    Assert.Equal(
                        1.0, (double)GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => Math.Truncate(e.Double)).First(), 7);
                }
            }

            [Fact]
            public void MathTruncate_on_decimal_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        1, (decimal)context.WithTypes.OrderBy(e => e.Id).Select(e => Math.Truncate(e.Decimal)).First(), 7);

                    Assert.Equal(
                        1, (decimal)GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                            e => Math.Truncate(e.Decimal)).First(), 7);
                }
            }
        }

#pragma warning disable 612,618
        /// <summary>
        /// Tests for the proxy implementations of the <see cref="DbFunctions" /> functions
        /// in the <see cref="EntityFunctions" /> class.
        /// </summary>
        public class EntityFunctionsProxies
        {
            public class StandardDeviation : FunctionalTestBase
            {
                [Fact]
                public void StandardDeviation_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.Decimal)), 2);
                        Assert.Equal(
                            0.71, (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviation_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                        Assert.Equal(
                            0.71,
                            (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviation_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.Double)), 2);
                        Assert.Equal(
                            0.71, (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                    }
                }

                [Fact]
                public void StandardDeviation_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableDouble)), 2);
                        Assert.Equal(
                            0.71,
                            (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviation_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.Int)), 2);
                        Assert.Equal(
                            0.71, (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                    }
                }

                [Fact]
                public void StandardDeviation_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableInt)), 2);
                        Assert.Equal(
                            0.71,
                            (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviation_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.Long)), 2);
                        Assert.Equal(
                            0.71, (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                    }
                }

                [Fact]
                public void StandardDeviation_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.71, (double)EntityFunctions.StandardDeviation(context.WithTypes.Select(e => e.NullableLong)), 2);
                        Assert.Equal(
                            0.71,
                            (double)EntityFunctions.StandardDeviation(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviation_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            0.71, (double)context.WithRelationships.Select(
                                e => new
                                    {
                                        Result = EntityFunctions.StandardDeviation(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);

                        Assert.Equal(
                            0.71, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                                e => new
                                    {
                                        Result = EntityFunctions.StandardDeviation(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);
                    }
                }
            }

            public class StandardDeviationP : FunctionalTestBase
            {
                [Fact]
                public void StandardDeviationP_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Decimal)), 2);
                        Assert.Equal(
                            0.5, (double)EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviationP_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                        Assert.Equal(
                            0.5,
                            (double)
                            EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)), 2);
                    }
                }

                [Fact]
                public void StandardDeviationP_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Double)), 2);
                        Assert.Equal(
                            0.5, (double)EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                    }
                }

                [Fact]
                public void StandardDeviationP_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableDouble)), 2);
                        Assert.Equal(
                            0.5,
                            (double)EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviationP_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Int)), 2);
                        Assert.Equal(
                            0.5, (double)EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                    }
                }

                [Fact]
                public void StandardDeviationP_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableInt)), 2);
                        Assert.Equal(
                            0.5,
                            (double)EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviationP_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.Long)), 2);
                        Assert.Equal(
                            0.5, (double)EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                    }
                }

                [Fact]
                public void StandardDeviationP_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.StandardDeviationP(context.WithTypes.Select(e => e.NullableLong)), 2);
                        Assert.Equal(
                            0.5,
                            (double)EntityFunctions.StandardDeviationP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)),
                            2);
                    }
                }

                [Fact]
                public void StandardDeviationP_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            0.5, (double)context.WithRelationships.Select(
                                e => new
                                    {
                                        Result = EntityFunctions.StandardDeviationP(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);

                        Assert.Equal(
                            0.5, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                                e => new
                                    {
                                        Result = EntityFunctions.StandardDeviationP(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);
                    }
                }
            }

            public class Var : FunctionalTestBase
            {
                [Fact]
                public void Var_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.Decimal)), 2);
                        Assert.Equal(0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)), 2);
                    }
                }

                [Fact]
                public void Var_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                        Assert.Equal(
                            0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)), 2);
                    }
                }

                [Fact]
                public void Var_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.Double)), 2);
                        Assert.Equal(0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                    }
                }

                [Fact]
                public void Var_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.NullableDouble)), 2);
                        Assert.Equal(
                            0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)), 2);
                    }
                }

                [Fact]
                public void Var_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.Int)), 2);
                        Assert.Equal(0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                    }
                }

                [Fact]
                public void Var_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.NullableInt)), 2);
                        Assert.Equal(0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)), 2);
                    }
                }

                [Fact]
                public void Var_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.Long)), 2);
                        Assert.Equal(0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                    }
                }

                [Fact]
                public void Var_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.5, (double)EntityFunctions.Var(context.WithTypes.Select(e => e.NullableLong)), 2);
                        Assert.Equal(
                            0.5, (double)EntityFunctions.Var(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)), 2);
                    }
                }

                [Fact]
                public void Var_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            0.5, (double)context.WithRelationships.Select(
                                e => new
                                    {
                                        Result = EntityFunctions.Var(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);

                        Assert.Equal(
                            0.5, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                                e => new
                                    {
                                        Result = EntityFunctions.Var(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);
                    }
                }
            }

            public class VarP : FunctionalTestBase
            {
                [Fact]
                public void VarP_for_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.Decimal)), 2);
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Decimal)), 2);
                    }
                }

                [Fact]
                public void VarP_for_nullable_decimal_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.NullableDecimal)), 2);
                        Assert.Equal(
                            0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDecimal)), 2);
                    }
                }

                [Fact]
                public void VarP_for_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.Double)), 2);
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Double)), 2);
                    }
                }

                [Fact]
                public void VarP_for_nullable_double_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.NullableDouble)), 2);
                        Assert.Equal(
                            0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableDouble)), 2);
                    }
                }

                [Fact]
                public void VarP_for_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.Int)), 2);
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Int)), 2);
                    }
                }

                [Fact]
                public void VarP_for_nullable_int_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.NullableInt)), 2);
                        Assert.Equal(
                            0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableInt)), 2);
                    }
                }

                [Fact]
                public void VarP_for_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.Long)), 2);
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.Long)), 2);
                    }
                }

                [Fact]
                public void VarP_for_nullable_long_can_be_bootstrapped_from_DbContext_or_ObjectContext()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(0.25, (double)EntityFunctions.VarP(context.WithTypes.Select(e => e.NullableLong)), 2);
                        Assert.Equal(
                            0.25, (double)EntityFunctions.VarP(GetObjectSet<EntityWithTypes>(context).Select(e => e.NullableLong)), 2);
                    }
                }

                [Fact]
                public void VarP_can_be_used_on_nested_collection_with_ObjectQuery_or_DbQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            0.25, (double)context.WithRelationships.Select(
                                e => new
                                    {
                                        Result = EntityFunctions.VarP(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);

                        Assert.Equal(
                            0.25, (double)GetObjectSet<EntityWithRelationship>(context).Select(
                                e => new
                                    {
                                        Result = EntityFunctions.VarP(e.Types.Select(t => t.Decimal))
                                    }).First().Result, 2);
                    }
                }
            }

            public class StringFunctions : FunctionalTestBase
            {
                [Fact]
                public void Left_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            "Magic Unic",
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.Left(e.String, 10)).First());

                        Assert.Equal(
                            "Magic Unic",
                            GetObjectSet<EntityWithTypes>(context)
                                .OrderBy(e => e.Id)
                                .Select(e => EntityFunctions.Left(e.String, 10))
                                .First());
                    }
                }

                [Fact]
                public void Right_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            "corns Rock",
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.Right(e.String, 10)).First());

                        Assert.Equal(
                            "corns Rock",
                            GetObjectSet<EntityWithTypes>(context)
                                .OrderBy(e => e.Id)
                                .Select(e => EntityFunctions.Right(e.String, 10))
                                .First());
                    }
                }

                [Fact]
                public void Reverse_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            "kcoR snrocinU cigaM",
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.Reverse(e.String)).First());

                        Assert.Equal(
                            "kcoR snrocinU cigaM",
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(e => EntityFunctions.Reverse(e.String)).First());
                    }
                }

                [Fact]
                public void AsUnicode_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            "Magic Unicorns Rock And Roll All Night Long ツ",
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => e.String + EntityFunctions.AsUnicode(" And Roll All Night Long ツ")).First());

                        Assert.Equal(
                            "Magic Unicorns Rock And Roll All Night Long ツ",
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => e.String + EntityFunctions.AsUnicode(" And Roll All Night Long ツ")).First());
                    }
                }

                [Fact]
                public void AsNonUnicode_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            "Magic Unicorns Rock And Roll All Night Long ?",
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => e.String + EntityFunctions.AsNonUnicode(" And Roll All Night Long ツ")).First());

                        Assert.Equal(
                            "Magic Unicorns Rock And Roll All Night Long ?",
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => e.String + EntityFunctions.AsNonUnicode(" And Roll All Night Long ツ")).First());
                    }
                }
            }

            public class DateTimeFunctions : FunctionalTestBase
            {
                private const long EF41Ticks = 634380912600000000;
                private static readonly DateTime _ef43DateTime = new DateTime(2012, 2, 29, 4, 3, 1, 0, DateTimeKind.Utc);
                private static readonly DateTimeOffset _ef43Offset = new DateTimeOffset(2012, 2, 29, 4, 3, 1, 0, new TimeSpan(8, 0, 0));

                [Fact]
                public void GetTotalOffsetMinutes_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            8 * 60,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.GetTotalOffsetMinutes(e.DateTimeOffset))
                                   .First());

                        Assert.Equal(
                            8 * 60,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.GetTotalOffsetMinutes(e.DateTimeOffset)).First());
                    }
                }

                [Fact]
                public void TruncateTime_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 0, 0, 0, 0, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.TruncateTime(e.DateTimeOffset)).First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 0, 0, 0, 0, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.TruncateTime(e.DateTimeOffset)).First());
                    }
                }

                [Fact]
                public void TruncateTime_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 4, 11, 0, 0, 0, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.TruncateTime(e.DateTime)).First());

                        Assert.Equal(
                            new DateTime(2011, 4, 11, 0, 0, 0, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.TruncateTime(e.DateTime)).First());
                    }
                }

                [Fact]
                public void CreateDateTime_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 4, 11, 0, 0, 1, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.CreateDateTime(2011, 4, 11, 0, 0, e.Int))
                                   .First());

                        Assert.Equal(
                            new DateTime(2011, 4, 11, 0, 0, 1, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.CreateDateTime(2011, 4, 11, 0, 0, e.Int)).First());
                    }
                }

                [Fact]
                public void CreateDateTimeOffset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 0, 0, 1, 0, new TimeSpan(0, 1, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.CreateDateTimeOffset(2011, 4, 11, 0, 0, e.Int, e.Int)).First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 0, 0, 1, 0, new TimeSpan(0, 1, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.CreateDateTimeOffset(2011, 4, 11, 0, 0, e.Int, e.Int)).First());
                    }
                }

                [Fact]
                public void CreateTime_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new TimeSpan(1, 2, 3),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.CreateTime(e.Int, 2, 3)).First());

                        Assert.Equal(
                            new TimeSpan(1, 2, 3),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.CreateTime(e.Int, 2, 3)).First());
                    }
                }

                [Fact]
                public void AddYears_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2012, 4, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddYears(e.DateTimeOffset, e.Int)).First());

                        Assert.Equal(
                            new DateTimeOffset(2012, 4, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddYears(e.DateTimeOffset, e.Int)).First());
                    }
                }

                [Fact]
                public void AddYears_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2012, 4, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddYears(e.DateTime, e.Int)).First());

                        Assert.Equal(
                            new DateTime(2012, 4, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddYears(e.DateTime, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMonths_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 5, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddMonths(e.DateTimeOffset, e.Int)).First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 5, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMonths(e.DateTimeOffset, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMonths_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 5, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddMonths(e.DateTime, e.Int)).First());

                        Assert.Equal(
                            new DateTime(2011, 5, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMonths(e.DateTime, e.Int)).First());
                    }
                }

                [Fact]
                public void AddDays_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 12, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddDays(e.DateTimeOffset, e.Int)).First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 12, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddDays(e.DateTimeOffset, e.Int)).First());
                    }
                }

                [Fact]
                public void AddDays_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 4, 12, 4, 1, 0, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddDays(e.DateTime, e.Int)).First());

                        Assert.Equal(
                            new DateTime(2011, 4, 12, 4, 1, 0, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddDays(e.DateTime, e.Int)).First());
                    }
                }

                [Fact]
                public void AddHours_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 5, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddHours(e.DateTimeOffset, e.Int)).First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 5, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddHours(e.DateTimeOffset, e.Int)).First());
                    }
                }

                [Fact]
                public void AddHours_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 4, 11, 5, 1, 0, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddHours(e.DateTime, e.Int)).First());

                        Assert.Equal(
                            new DateTime(2011, 4, 11, 5, 1, 0, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddHours(e.DateTime, e.Int)).First());
                    }
                }

                [Fact]
                public void AddHours_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new TimeSpan(5, 1, 0),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddHours(e.TimeSpan, e.Int)).First());

                        Assert.Equal(
                            new TimeSpan(5, 1, 0),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddHours(e.TimeSpan, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMinutes_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 4, 2, 0, 0, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddMinutes(e.DateTimeOffset, e.Int)).First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 4, 2, 0, 0, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMinutes(e.DateTimeOffset, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMinutes_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 4, 11, 4, 2, 0, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddMinutes(e.DateTime, e.Int)).First());

                        Assert.Equal(
                            new DateTime(2011, 4, 11, 4, 2, 0, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMinutes(e.DateTime, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMinutes_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new TimeSpan(4, 2, 0),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddMinutes(e.TimeSpan, e.Int)).First());

                        Assert.Equal(
                            new TimeSpan(4, 2, 0),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMinutes(e.TimeSpan, e.Int)).First());
                    }
                }

                [Fact]
                public void AddSeconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 4, 1, 1, 0, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddSeconds(e.DateTimeOffset, e.Int)).First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 4, 1, 1, 0, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddSeconds(e.DateTimeOffset, e.Int)).First());
                    }
                }

                [Fact]
                public void AddSeconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 4, 11, 4, 1, 1, 0, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddSeconds(e.DateTime, e.Int)).First());

                        Assert.Equal(
                            new DateTime(2011, 4, 11, 4, 1, 1, 0, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddSeconds(e.DateTime, e.Int)).First());
                    }
                }

                [Fact]
                public void AddSeconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new TimeSpan(4, 1, 1),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddSeconds(e.TimeSpan, e.Int)).First());

                        Assert.Equal(
                            new TimeSpan(4, 1, 1),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddSeconds(e.TimeSpan, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMilliseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 4, 1, 0, 1, new TimeSpan(8, 0, 0)),
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.AddMilliseconds(e.DateTimeOffset, e.Int))
                                   .First());

                        Assert.Equal(
                            new DateTimeOffset(2011, 4, 11, 4, 1, 0, 1, new TimeSpan(8, 0, 0)),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMilliseconds(e.DateTimeOffset, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMilliseconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new DateTime(2011, 4, 11, 4, 1, 0, 1, DateTimeKind.Utc),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddMilliseconds(e.DateTime, e.Int)).First());

                        Assert.Equal(
                            new DateTime(2011, 4, 11, 4, 1, 0, 1, DateTimeKind.Utc),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMilliseconds(e.DateTime, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMilliseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new TimeSpan(0, 4, 1, 0, 1),
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddMilliseconds(e.TimeSpan, e.Int)).First());

                        Assert.Equal(
                            new TimeSpan(0, 4, 1, 0, 1),
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMilliseconds(e.TimeSpan, e.Int)).First());
                    }
                }

                [Fact]
                public void AddMicroseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            EF41Ticks + 10,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.AddMicroseconds(e.DateTimeOffset, e.Int))
                                   .First()
                                   .
                                    Value.Ticks);

                        Assert.Equal(
                            EF41Ticks + 10,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMicroseconds(e.DateTimeOffset, e.Int)).First().Value.Ticks);
                    }
                }

                [Fact]
                public void AddMicroseconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            EF41Ticks + 10,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.AddMicroseconds(e.DateTime, e.Int))
                                   .First()
                                   .Value.
                                    Ticks);

                        Assert.Equal(
                            EF41Ticks + 10,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMicroseconds(e.DateTime, e.Int)).First().Value.Ticks);
                    }
                }

                [Fact]
                public void AddMicroseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new TimeSpan(4, 1, 0).Ticks + 10,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.AddMicroseconds(e.TimeSpan, e.Int))
                                   .First()
                                   .Value.Ticks);

                        Assert.Equal(
                            new TimeSpan(4, 1, 0).Ticks + 10,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddMicroseconds(e.TimeSpan, e.Int)).First().Value.Ticks);
                    }
                }

                [Fact]
                public void AddNanoseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            EF41Ticks + 1,
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.AddNanoseconds(e.DateTimeOffset, e.Int * 100)).
                                    First().
                                    Value.Ticks);

                        Assert.Equal(
                            EF41Ticks + 1,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddNanoseconds(e.DateTimeOffset, e.Int * 100)).First().Value.Ticks);
                    }
                }

                [Fact]
                public void AddNanoseconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            EF41Ticks + 1,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.AddNanoseconds(e.DateTime, e.Int * 100))
                                   .First()
                                   .
                                    Value.
                                    Ticks);

                        Assert.Equal(
                            EF41Ticks + 1,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddNanoseconds(e.DateTime, e.Int * 100)).First().Value.Ticks);
                    }
                }

                [Fact]
                public void AddNanoseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            new TimeSpan(4, 1, 0).Ticks + 1,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.AddNanoseconds(e.TimeSpan, e.Int * 100))
                                   .First()
                                   .Value.
                                    Ticks);

                        Assert.Equal(
                            new TimeSpan(4, 1, 0).Ticks + 1,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.AddNanoseconds(e.TimeSpan, e.Int * 100)).First().Value.Ticks);
                    }
                }

                [Fact]
                public void DiffYears_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffYears(e.DateTimeOffset, _ef43Offset))
                                   .First());

                        Assert.Equal(
                            1,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffYears(e.DateTimeOffset, _ef43Offset)).First());
                    }
                }

                [Fact]
                public void DiffYears_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1,
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.DiffYears(e.DateTime, _ef43DateTime)).First());

                        Assert.Equal(
                            1,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffYears(e.DateTime, _ef43DateTime)).First());
                    }
                }

                [Fact]
                public void DiffMonths_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            10,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffMonths(e.DateTimeOffset, _ef43Offset))
                                   .First());

                        Assert.Equal(
                            10,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMonths(e.DateTimeOffset, _ef43Offset)).First());
                    }
                }

                [Fact]
                public void DiffMonths_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            10,
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.DiffMonths(e.DateTime, _ef43DateTime)).First());

                        Assert.Equal(
                            10,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMonths(e.DateTime, _ef43DateTime)).First());
                    }
                }

                [Fact]
                public void DiffDays_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            324,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffDays(e.DateTimeOffset, _ef43Offset))
                                   .First());

                        Assert.Equal(
                            324,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffDays(e.DateTimeOffset, _ef43Offset)).First());
                    }
                }

                [Fact]
                public void DiffDays_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            324,
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.DiffDays(e.DateTime, _ef43DateTime)).First());

                        Assert.Equal(
                            324,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffDays(e.DateTime, _ef43DateTime)).First());
                    }
                }

                [Fact]
                public void DiffHours_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            7776,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffHours(e.DateTimeOffset, _ef43Offset))
                                   .First());

                        Assert.Equal(
                            7776,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffHours(e.DateTimeOffset, _ef43Offset)).First());
                    }
                }

                [Fact]
                public void DiffHours_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            7776,
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.DiffHours(e.DateTime, _ef43DateTime)).First());

                        Assert.Equal(
                            7776,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffHours(e.DateTime, _ef43DateTime)).First());
                    }
                }

                [Fact]
                public void DiffHours_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffHours(e.TimeSpan, new TimeSpan(5, 1, 0)))
                                   .First());

                        Assert.Equal(
                            1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffHours(e.TimeSpan, new TimeSpan(5, 1, 0))).First());
                    }
                }

                [Fact]
                public void DiffMinutes_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            466562,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffMinutes(e.DateTimeOffset, _ef43Offset))
                                   .First());

                        Assert.Equal(
                            466562,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMinutes(e.DateTimeOffset, _ef43Offset)).First());
                    }
                }

                [Fact]
                public void DiffMinutes_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            466562,
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.DiffMinutes(e.DateTime, _ef43DateTime)).First());

                        Assert.Equal(
                            466562,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMinutes(e.DateTime, _ef43DateTime)).First());
                    }
                }

                [Fact]
                public void DiffMinutes_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMinutes(e.TimeSpan, new TimeSpan(4, 2, 0))).First());

                        Assert.Equal(
                            1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMinutes(e.TimeSpan, new TimeSpan(4, 2, 0))).First());
                    }
                }

                [Fact]
                public void DiffSeconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            27993721,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffSeconds(e.DateTimeOffset, _ef43Offset))
                                   .First());

                        Assert.Equal(
                            27993721,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffSeconds(e.DateTimeOffset, _ef43Offset)).First());
                    }
                }

                [Fact]
                public void DiffSeconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            27993721,
                            context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.DiffSeconds(e.DateTime, _ef43DateTime)).First());

                        Assert.Equal(
                            27993721,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffSeconds(e.DateTime, _ef43DateTime)).First());
                    }
                }

                [Fact]
                public void DiffSeconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1,
                            context.WithTypes.OrderBy(e => e.Id)
                                   .Select(e => EntityFunctions.DiffSeconds(e.TimeSpan, new TimeSpan(4, 1, 1)))
                                   .First());

                        Assert.Equal(
                            1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffSeconds(e.TimeSpan, new TimeSpan(4, 1, 1))).First());
                    }
                }

                [Fact]
                public void DiffMilliseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            100,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e =>
                                EntityFunctions.DiffMilliseconds(
                                    e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());

                        Assert.Equal(
                            100,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e =>
                                EntityFunctions.DiffMilliseconds(
                                    e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());
                    }
                }

                [Fact]
                public void DiffMilliseconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            100,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMilliseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc)))
                                   .First());

                        Assert.Equal(
                            100,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMilliseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc)))
                                                                  .First());
                    }
                }

                [Fact]
                public void DiffMilliseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMilliseconds(e.TimeSpan, new TimeSpan(0, 4, 1, 0, 1))).First());

                        Assert.Equal(
                            1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMilliseconds(e.TimeSpan, new TimeSpan(0, 4, 1, 0, 1))).First());
                    }
                }

                [Fact]
                public void DiffMicroseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            100000,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e =>
                                EntityFunctions.DiffMicroseconds(
                                    e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());

                        Assert.Equal(
                            100000,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e =>
                                EntityFunctions.DiffMicroseconds(
                                    e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());
                    }
                }

                [Fact]
                public void DiffMicroseconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            100000,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMicroseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc)))
                                   .First());

                        Assert.Equal(
                            100000,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMicroseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc)))
                                                                  .First());
                    }
                }

                [Fact]
                public void DiffMicroseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        var ticks = new TimeSpan(4, 1, 0).Ticks + 10;
                        Assert.Equal(
                            1, context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMicroseconds(e.TimeSpan, new TimeSpan(ticks))).First());

                        Assert.Equal(
                            1, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffMicroseconds(e.TimeSpan, new TimeSpan(ticks))).First());
                    }
                }

                [Fact]
                public void DiffNanoseconds_on_offset_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            100000000,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e =>
                                EntityFunctions.DiffNanoseconds(
                                    e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());

                        Assert.Equal(
                            100000000,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e =>
                                EntityFunctions.DiffNanoseconds(
                                    e.DateTimeOffset, new DateTimeOffset(EF41Ticks + 1000000, new TimeSpan(8, 0, 0)))).First());
                    }
                }

                [Fact]
                public void DiffNanoseconds_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            100000000,
                            context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffNanoseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc)))
                                   .First());

                        Assert.Equal(
                            100000000,
                            GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffNanoseconds(e.DateTime, new DateTime(EF41Ticks + 1000000, DateTimeKind.Utc)))
                                                                  .First());
                    }
                }

                [Fact]
                public void DiffNanoseconds_on_time_span_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        var ticks = new TimeSpan(4, 1, 0).Ticks + 1;
                        Assert.Equal(
                            100, context.WithTypes.OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffNanoseconds(e.TimeSpan, new TimeSpan(ticks))).First());

                        Assert.Equal(
                            100, GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.DiffNanoseconds(e.TimeSpan, new TimeSpan(ticks))).First());
                    }
                }
            }

            public class Truncate : FunctionalTestBase
            {
                [Fact]
                public void Truncate_on_double_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1.0, (double)context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.Truncate(e.Double, e.Int)).First(),
                            7);

                        Assert.Equal(
                            1.0, (double)GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.Truncate(e.Double, e.Int)).First(), 7);
                    }
                }

                [Fact]
                public void Truncate_on_decimal_can_be_used_in_DbQuery_or_ObjectQuery()
                {
                    using (var context = new EntityFunctionContext())
                    {
                        Assert.Equal(
                            1, (decimal)context.WithTypes.OrderBy(e => e.Id).Select(e => EntityFunctions.Truncate(e.Decimal, e.Int)).First(),
                            7);

                        Assert.Equal(
                            1, (decimal)GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(
                                e => EntityFunctions.Truncate(e.Decimal, e.Int)).First(), 7);
                    }
                }
            }
        }
#pragma warning restore 612,618

        public class EdmFunctionUsage : FunctionalTestBase
        {
#pragma warning disable 612,618
            [EdmFunction("Edm", "Left")]
#pragma warning restore 612,618
            public static String Left(String stringArgument, long? length)
            {
                throw new NotSupportedException();
            }

            [Fact]
            public void Function_attributed_with_EdmFunctionAttribute_can_be_used_in_DbQuery_or_ObjectQuery()
            {
                using (var context = new EntityFunctionContext())
                {
                    Assert.Equal(
                        "Magic Unic",
                        context.WithTypes.OrderBy(e => e.Id).Select(e => Left(e.String, 10)).First());

                    Assert.Equal(
                        "Magic Unic",
                        GetObjectSet<EntityWithTypes>(context).OrderBy(e => e.Id).Select(e => Left(e.String, 10)).First());
                }
            }
        }

        [Fact] // CodePlex 5
        public void Bootstrapping_from_DbQuery_with_DbQuery_as_parameter_only_hits_the_database_once()
        {
            try
            {
                WrappingAdoNetProvider<SqlClientFactory>.WrapProviders();

                using (var context = new EntityFunctionContext())
                {
                    context.Database.Initialize(force: false);

                    var log = WrappingAdoNetProvider<SqlClientFactory>.Instance.Log;
                    log.Clear();

                    Assert.Equal(0.5, (double)DbFunctions.Var(context.WithTypes.Select(e => e.Id)), 2);

                    var queries = log.Where(l => l.Method == "ExecuteReader").Select(l => l.Details.StripFormatting());
                    Assert.Equal(1, queries.Count());
                    Assert.Equal(
                        @"SELECT [GroupBy1].[A1] AS [C1] FROM ( SELECT  VAR([Extent1].[Id]) AS [A1] FROM [dbo].[EntityWithTypes] AS [Extent1])  AS [GroupBy1]"
                            .StripFormatting(),
                        queries.Single());
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

        public static ObjectSet<TEntity> GetObjectSet<TEntity>(DbContext context) where TEntity : class
        {
            return ((IObjectContextAdapter)context).ObjectContext.CreateObjectSet<TEntity>();
        }
    }

    public class EntityFunctionContext : DbContext
    {
        static EntityFunctionContext()
        {
            Database.SetInitializer(new EntityFunctionInitializer());
        }

        public DbSet<EntityWithTypes> WithTypes { get; set; }
        public DbSet<EntityWithRelationship> WithRelationships { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<EntityWithTypes>()
                .Property(e => e.DateTime)
                .HasColumnType("datetime2");
        }
    }

    public class EntityWithTypes
    {
        public int Id { get; set; }

        public decimal Decimal { get; set; }
        public decimal? NullableDecimal { get; set; }

        public double Double { get; set; }
        public double? NullableDouble { get; set; }

        public int Int { get; set; }
        public int? NullableInt { get; set; }

        public long Long { get; set; }
        public long? NullableLong { get; set; }

        public DateTime? DateTime { get; set; }
        public DateTimeOffset? DateTimeOffset { get; set; }
        public TimeSpan? TimeSpan { get; set; }

        public string String { get; set; }

        public int RelationshipId { get; set; }
        public EntityWithRelationship Relationship { get; set; }
    }

    public class EntityWithRelationship
    {
        public int Id { get; set; }

        public ICollection<EntityWithTypes> Types { get; set; }
    }

    public class EntityFunctionInitializer : DropCreateDatabaseAlways<EntityFunctionContext>
    {
        protected override void Seed(EntityFunctionContext context)
        {
            var entityWithRelationship = new EntityWithRelationship();
            new List<EntityWithTypes>
                {
                    new EntityWithTypes
                        {
                            Decimal = 1.0001m,
                            NullableDecimal = 1m,
                            Double = 1.0001,
                            NullableDouble = 1.0,
                            Int = 1,
                            NullableInt = 1,
                            Long = 1,
                            NullableLong = 1,
                            String = "Magic Unicorns Rock",
                            DateTime = new DateTime(2011, 4, 11, 4, 1, 0, 0, DateTimeKind.Utc),
                            DateTimeOffset = new DateTimeOffset(2011, 4, 11, 4, 1, 0, 0, new TimeSpan(8, 0, 0)),
                            TimeSpan = new TimeSpan(4, 1, 0),
                            Relationship = entityWithRelationship
                        },
                    new EntityWithTypes
                        {
                            Decimal = 2m,
                            NullableDecimal = 2m,
                            Double = 2.0,
                            NullableDouble = 2.0,
                            Int = 2,
                            NullableInt = 2,
                            Long = 2,
                            NullableLong = 2,
                            String = "Magic Unicorns Roll",
                            DateTime = new DateTime(2012, 2, 29, 4, 3, 1, 0, DateTimeKind.Utc),
                            DateTimeOffset = new DateTimeOffset(2012, 2, 29, 4, 3, 1, 0, new TimeSpan(8, 0, 0)),
                            TimeSpan = new TimeSpan(4, 3, 1),
                            Relationship = entityWithRelationship
                        }
                }.ForEach(e => context.WithTypes.Add(e));
        }
    }
}
