// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class FunctionDetailsV1RowViewTests
    {
        [Fact]
        public void String_properties_exposed_correctly()
        {
            var row =
                new object[]
                    {
                        "schema",
                        "name",
                        "retType",
                        null,
                        null,
                        null,
                        null,
                        "paramName",
                        "paramType",
                        "paramDirection"
                    };

            var view = new FunctionDetailsV1RowView(row);

            Assert.Equal("schema", view.Schema);
            Assert.Equal("name", view.ProcedureName);
            Assert.Equal("retType", view.ReturnType);
            Assert.Equal("paramName", view.ParameterName);
            Assert.Equal("paramType", view.ParameterType);
            Assert.Equal("paramDirection", view.ProcParameterMode);
        }

        [Fact]
        public void IsAggregate_property_exposed_correctly()
        {
            const int isAggregateIndex = 3;

            var row = new object[10];
            var view = new FunctionDetailsV1RowView(row);

            row[isAggregateIndex] = false;
            Assert.False(view.IsIsAggregate);
            row[isAggregateIndex] = true;
            Assert.True(view.IsIsAggregate);
        }

        [Fact]
        public void IsComposable_property_exposed_correctly()
        {
            const int isComposableIndex = 4;
            var row = new object[10];
            var view = new FunctionDetailsV1RowView(row);

            row[isComposableIndex] = false;
            Assert.False(view.IsComposable);
            row[isComposableIndex] = true;
            Assert.True(view.IsComposable);
        }

        [Fact]
        public void IsBuiltIn_property_exposed_correctly()
        {
            const int isBuiltInIndex = 5;
            var row = new object[10];
            var view = new FunctionDetailsV1RowView(row);

            row[isBuiltInIndex] = false;
            Assert.False(view.IsBuiltIn);
            row[isBuiltInIndex] = true;
            Assert.True(view.IsBuiltIn);
        }

        [Fact]
        public void IsNiladic_property_exposed_correctly()
        {
            const int isNiladicIndex = 6;

            var row = new object[10];
            var view = new FunctionDetailsV1RowView(row);

            row[isNiladicIndex] = false;
            Assert.False(view.IsNiladic);
            row[isNiladicIndex] = true;
            Assert.True(view.IsNiladic);
        }

        [Fact]
        public void Catalog_and_IsTvf_return_default_values()
        {
            var view =
                new FunctionDetailsV1RowView(Enumerable.Repeat(new object(), 10).ToArray());

            Assert.Null(view.Catalog);
            Assert.False(view.IsTvf);
        }

        [Fact]
        public void DbNull_converted_to_default_values()
        {
            var view =
                new FunctionDetailsV1RowView(Enumerable.Repeat(DBNull.Value, 10).ToArray());

            Assert.Null(view.Catalog);
            Assert.Null(view.Schema);
            Assert.Null(view.ProcedureName);
            Assert.Null(view.ReturnType);
            Assert.False(view.IsIsAggregate);
            Assert.False(view.IsBuiltIn);
            Assert.False(view.IsComposable);
            Assert.False(view.IsNiladic);
            Assert.False(view.IsTvf);
            Assert.Null(view.ParameterName);
            Assert.Null(view.ParameterType);
            Assert.Null(view.ProcParameterMode);
        }

        [Fact]
        public void IsParameterXXXNull_properties_return_true_for_DBNull_values()
        {
            var view =
                new FunctionDetailsV1RowView(Enumerable.Repeat(DBNull.Value, 10).ToArray());

            Assert.True(view.IsParameterNameNull);
            Assert.True(view.IsParameterTypeNull);
            Assert.True(view.IsParameterModeNull);
        }

        [Fact]
        public void IsParameterXXXNull_properties_return_false_for_non_DBNull_values()
        {
            var view =
                new FunctionDetailsV1RowView(Enumerable.Repeat("test", 10).ToArray());

            Assert.False(view.IsParameterNameNull);
            Assert.False(view.IsParameterTypeNull);
            Assert.False(view.IsParameterModeNull);
        }

        [Fact]
        public void TryGetParameterMode_returns_ParameterMode_corresponding_to_the_given_string_parameter_mode()
        {
            TryGetParameterMode_test_helper(DBNull.Value, false, (ParameterMode)(-1));
            TryGetParameterMode_test_helper(string.Empty, false, (ParameterMode)(-1));
            TryGetParameterMode_test_helper("foo", false, (ParameterMode)(-1));
            TryGetParameterMode_test_helper("IN", true, ParameterMode.In);
            TryGetParameterMode_test_helper("OUT", true, ParameterMode.Out);
            TryGetParameterMode_test_helper("INOUT", true, ParameterMode.InOut);
        }

        private static void TryGetParameterMode_test_helper(object paramDirection, bool successExpected, ParameterMode expectedMode)
        {
            var row =
                new[]
                    {
                        "schema",
                        "name",
                        "retType",
                        null,
                        null,
                        null,
                        null,
                        "paramName",
                        "paramType",
                        paramDirection
                    };

            ParameterMode actualMode;
            var success = new FunctionDetailsV1RowView(row).TryGetParameterMode(out actualMode);
            Assert.Equal(successExpected, success);
            Assert.Equal(expectedMode, actualMode);
        }
    }
}
