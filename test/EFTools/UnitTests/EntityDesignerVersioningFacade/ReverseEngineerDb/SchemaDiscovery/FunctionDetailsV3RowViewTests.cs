// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Linq;
    using Xunit;

    public class FunctionDetailsV3RowViewTests
    {
        [Fact]
        public void String_properties_exposed_correctly()
        {
            var row =
                new object[]
                    {
                        "catalog",
                        "schema",
                        "name",
                        "retType",
                        null,
                        null,
                        null,
                        null,
                        null,
                        "paramName",
                        "paramType",
                        "paramDirection"
                    };

            var view = new FunctionDetailsV3RowView(row);

            Assert.Equal("catalog", view.Catalog);
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
            const int isAggregateIndex = 4;

            var row = new object[12];
            var view = new FunctionDetailsV3RowView(row);

            row[isAggregateIndex] = false;
            Assert.False(view.IsIsAggregate);
            row[isAggregateIndex] = true;
            Assert.True(view.IsIsAggregate);
        }

        [Fact]
        public void IsComposable_property_exposed_correctly()
        {
            const int isComposableIndex = 5;
            var row = new object[12];
            var view = new FunctionDetailsV3RowView(row);

            row[isComposableIndex] = false;
            Assert.False(view.IsComposable);
            row[isComposableIndex] = true;
            Assert.True(view.IsComposable);
        }

        [Fact]
        public void IsBuiltIn_property_exposed_correctly()
        {
            const int isBuiltInIndex = 6;
            var row = new object[12];
            var view = new FunctionDetailsV3RowView(row);

            row[isBuiltInIndex] = false;
            Assert.False(view.IsBuiltIn);
            row[isBuiltInIndex] = true;
            Assert.True(view.IsBuiltIn);
        }

        [Fact]
        public void IsNiladic_property_exposed_correctly()
        {
            const int isNiladicIndex = 7;

            var row = new object[12];
            var view = new FunctionDetailsV3RowView(row);

            row[isNiladicIndex] = false;
            Assert.False(view.IsNiladic);
            row[isNiladicIndex] = true;
            Assert.True(view.IsNiladic);
        }

        [Fact]
        public void IsTvf_property_exposed_correctly()
        {
            const int isTvfIndex = 8;

            var row = new object[12];
            var view = new FunctionDetailsV3RowView(row);

            row[isTvfIndex] = false;
            Assert.False(view.IsTvf);
            row[isTvfIndex] = true;
            Assert.True(view.IsTvf);
        }

        [Fact]
        public void DbNull_converted_to_default_values()
        {
            var view =
                new FunctionDetailsV3RowView(Enumerable.Repeat(DBNull.Value, 12).ToArray());

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
                new FunctionDetailsV3RowView(Enumerable.Repeat(DBNull.Value, 12).ToArray());

            Assert.True(view.IsParameterNameNull);
            Assert.True(view.IsParameterTypeNull);
            Assert.True(view.IsParameterModeNull);
        }

        [Fact]
        public void IsParameterXXXNull_properties_return_false_for_non_DBNull_values()
        {
            var view =
                new FunctionDetailsV3RowView(Enumerable.Repeat("test", 12).ToArray());

            Assert.False(view.IsParameterNameNull);
            Assert.False(view.IsParameterTypeNull);
            Assert.False(view.IsParameterModeNull);
        }

        [Fact]
        public void GetMostQualifiedFunctionName_returns_correct_function_name()
        {
            Assert.Equal("function", GetMostQualifiedFunctionName(catalog: DBNull.Value, schema: DBNull.Value, procedureName: "function"));
            Assert.Equal("dbo.function", GetMostQualifiedFunctionName(catalog: DBNull.Value, schema: "dbo", procedureName: "function"));
            Assert.Equal(
                "catalog.function", GetMostQualifiedFunctionName(catalog: "catalog", schema: DBNull.Value, procedureName: "function"));
            Assert.Equal("catalog.dbo.function", GetMostQualifiedFunctionName(catalog: "catalog", schema: "dbo", procedureName: "function"));
        }

        private static string GetMostQualifiedFunctionName(object catalog, object schema, object procedureName)
        {
            var row =
                new[]
                    {
                        catalog, schema, procedureName, null, null, null, null, null, null, null, null, null
                    };

            return new FunctionDetailsV3RowView(row).GetMostQualifiedFunctionName();
        }
    }
}
