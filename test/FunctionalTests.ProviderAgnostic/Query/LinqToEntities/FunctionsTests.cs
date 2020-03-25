// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestModels.ProviderAgnosticModel;
    using System.Linq;
    using Xunit;

    public class FunctionsTests
    {
        public class StringFunctions
        {
            [Fact]
            public void String_Concat_translated_properly_to_plus_operator()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => string.Concat(o.LastName, o.FirstName)).ToList();
                    var actual = orderedOwners.Select(o => string.Concat(o.LastName, o.FirstName)).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void String_Length_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.LastName.Length).ToList();
                    var actual = orderedOwners.Select(o => o.LastName.Length).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void IndexOf_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.LastName.IndexOf("N")).ToList();
                    var actual = orderedOwners.Select(o => o.LastName.IndexOf("N")).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void String_Insert_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.LastName.Insert(2, "Foo")).ToList();
                    var actual = orderedOwners.Select(o => o.LastName.Insert(2, "Foo")).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void String_Remove_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.LastName.Remove(2, 3)).ToList();
                    var actual = orderedOwners.Select(o => o.LastName.Remove(2, 3)).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void String_Replace_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.LastName.Replace("Name", "Foo")).ToList();
                    var actual = orderedOwners.Select(o => o.LastName.Replace("Name", "Foo")).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void Substring_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected1 = orderedOwners.ToList().Select(o => o.LastName.Substring(1)).ToList();
                    var expected2 = orderedOwners.ToList().Select(o => o.LastName.Substring(1, 2)).ToList();
                    var actual1 = orderedOwners.Select(o => o.LastName.Substring(1)).ToList();
                    var actual2 = orderedOwners.Select(o => o.LastName.Substring(1, 2)).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected1, actual1, (e, a) => e == a);
                    QueryTestHelpers.VerifyQueryResult(expected2, actual2, (e, a) => e == a);
                }
            }

            [Fact]
            public void ToLower_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.LastName.ToLower()).ToList();
                    var actual = orderedOwners.Select(o => o.LastName.ToLower()).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void ToUpper_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.LastName.ToUpper()).ToList();
                    var actual = orderedOwners.Select(o => o.LastName.ToUpper()).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void Trim_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.Alias.Trim()).ToList();
                    var actual = orderedOwners.Select(o => o.Alias.Trim()).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void TrimStart_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.Alias.TrimStart()).ToList();
                    var actual = orderedOwners.Select(o => o.Alias.TrimStart()).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void TrimEnd_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedOwners = context.Owners.OrderBy(o => o.Id);
                    var expected = orderedOwners.ToList().Select(o => o.Alias.TrimEnd()).ToList();
                    var actual = orderedOwners.Select(o => o.Alias.TrimEnd()).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }
        }

        public class DateTimeFunctions
        {
            [Fact]
            public void DateTime_Day_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedFailures = context.Failures.OrderBy(f => f.Id);
                    var expected = orderedFailures.ToList().Select(o => o.Changed.Day).ToList();
                    var actual = orderedFailures.Select(o => o.Changed.Day).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void DateTime_Hour_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedFailures = context.Failures.OrderBy(f => f.Id);
                    var expected = orderedFailures.ToList().Select(o => o.Changed.Hour).ToList();
                    var actual = orderedFailures.Select(o => o.Changed.Hour).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void DateTime_Millisecond_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedFailures = context.Failures.OrderBy(f => f.Id);
                    var expected = orderedFailures.ToList().Select(o => o.Changed.Millisecond).ToList();
                    var actual = orderedFailures.Select(o => o.Changed.Millisecond).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void DateTime_Minute_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedFailures = context.Failures.OrderBy(f => f.Id);
                    var expected = orderedFailures.ToList().Select(o => o.Changed.Minute).ToList();
                    var actual = orderedFailures.Select(o => o.Changed.Minute).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void DateTime_Month_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedFailures = context.Failures.OrderBy(f => f.Id);
                    var expected = orderedFailures.ToList().Select(o => o.Changed.Month).ToList();
                    var actual = orderedFailures.Select(o => o.Changed.Month).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void DateTime_Second_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedFailures = context.Failures.OrderBy(f => f.Id);
                    var expected = orderedFailures.ToList().Select(o => o.Changed.Second).ToList();
                    var actual = orderedFailures.Select(o => o.Changed.Second).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void DateTime_Year_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedFailures = context.Failures.OrderBy(f => f.Id);
                    var expected = orderedFailures.ToList().Select(o => o.Changed.Year).ToList();
                    var actual = orderedFailures.Select(o => o.Changed.Year).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }
        }

        public class MathFunctions
        {
            [Fact]
            public void Ceiling_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedAllTypes = context.AllTypes.OrderBy(a => a.Id);
                    var expected = orderedAllTypes.ToList().Select(a => Math.Ceiling(a.DecimalProperty)).ToList();
                    var actual = orderedAllTypes.Select(a => Math.Ceiling(a.DecimalProperty)).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void Floor_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedAllTypes = context.AllTypes.OrderBy(a => a.Id);
                    var expected = orderedAllTypes.ToList().Select(a => Math.Floor(a.DecimalProperty)).ToList();
                    var actual = orderedAllTypes.Select(a => Math.Floor(a.DecimalProperty)).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected, actual, (e, a) => e == a);
                }
            }

            [Fact]
            public void Truncates_properly_translated_to_function()
            {
                using (var context = new ProviderAgnosticContext())
                {
                    var orderedAllTypes = context.AllTypes.OrderBy(a => a.Id);

                    var expected1 = orderedAllTypes.ToList().Select(a => Math.Truncate(a.DecimalProperty)).ToList();
                    var expected2 = orderedAllTypes.ToList().Select(a => Math.Truncate(a.FloatProperty)).ToList();
                    var actual1 = orderedAllTypes.Select(a => Math.Truncate(a.DecimalProperty)).ToList();
                    var actual2 = orderedAllTypes.Select(a => Math.Truncate(a.FloatProperty)).ToList();

                    QueryTestHelpers.VerifyQueryResult(expected1, actual1, (e, a) => e == a);
                    QueryTestHelpers.VerifyQueryResult(expected2, actual2, (e, a) => e == a);
                }
            }
        }
    }
}
