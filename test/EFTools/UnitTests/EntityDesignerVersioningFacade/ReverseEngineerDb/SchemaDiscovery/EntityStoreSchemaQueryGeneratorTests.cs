// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class EntityStoreSchemaQueryGeneratorTests
    {
        [Fact]
        public void AppendComparison_does_not_create_comparison_fragment_or_corresponding_parameter_for_non_value()
        {
            Assert.Empty(
                EntityStoreSchemaQueryGenerator.AppendComparison(
                    new StringBuilder(),
                    string.Empty,
                    string.Empty,
                    /* value */ null,
                    new ParameterCollectionBuilder()).ToString());
        }

        [Fact]
        public void AppendComparison_creates_comparison_fragment_and_corresponding_parameter_for_non_null_value()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "alias.propertyName LIKE @p0",
                EntityStoreSchemaQueryGenerator.AppendComparison(
                    new StringBuilder(),
                    "alias",
                    "propertyName",
                    "Value",
                    parameterBuilder).ToString());

            Assert.Equal(1, parameterBuilder.ParameterCollection.Count);
            Assert.Equal("Value", parameterBuilder.ParameterCollection["p0"].Value);
        }

        [Fact]
        public void AppendComparison_creates_parameters_and_adds_AND_for_multiple_comparisons()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            var filterBuilder =
                EntityStoreSchemaQueryGenerator.AppendComparison(
                    new StringBuilder(),
                    "alias1",
                    "propertyName1",
                    "Value1",
                    parameterBuilder);

            EntityStoreSchemaQueryGenerator.AppendComparison(
                filterBuilder,
                "alias2",
                "propertyName2",
                "Value2",
                parameterBuilder);

            Assert.Equal(
                "alias1.propertyName1 LIKE @p0 AND alias2.propertyName2 LIKE @p1",
                filterBuilder.ToString());

            Assert.Equal(2, parameterBuilder.ParameterCollection.Count);
            Assert.Equal("Value1", parameterBuilder.ParameterCollection["p0"].Value);
            Assert.Equal("Value2", parameterBuilder.ParameterCollection["p1"].Value);
        }

        [Fact]
        public void AppendFilterEntry_creates_filter_for_catalog_schema_and_name_if_all_specified()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "(alias.CatalogName LIKE @p0 AND alias.SchemaName LIKE @p1 AND alias.Name LIKE @p2)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry("catalog", "schema", "name"),
                    parameterBuilder).ToString());

            Assert.Equal(3, parameterMap.Count);
            Assert.Equal("catalog", parameterBuilder.ParameterCollection["p0"].Value);
            Assert.Equal("schema", parameterBuilder.ParameterCollection["p1"].Value);
            Assert.Equal("name", parameterBuilder.ParameterCollection["p2"].Value);
        }

        [Fact]
        public void AppendFilterEntry_does_not_create_comparison_for_missing_catalog_schema_or_name_if_any_specified()
        {
            Assert.Equal(
                "(alias.SchemaName LIKE @p0 AND alias.Name LIKE @p1)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry(null, "schema", "name"),
                    new ParameterCollectionBuilder()).ToString());

            Assert.Equal(
                "(alias.CatalogName LIKE @p0 AND alias.Name LIKE @p1)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry("catalog", null, "name"),
                    new ParameterCollectionBuilder()).ToString());

            Assert.Equal(
                "(alias.CatalogName LIKE @p0 AND alias.SchemaName LIKE @p1)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry("catalog", "schema", null),
                    new ParameterCollectionBuilder()).ToString());
        }

        [Fact]
        public void AppendFilterEntry_uses_wildcard_parameter_value_if_schema_catalog_and_name_are_null()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "(alias.Name LIKE @p0)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry(null, null, null),
                    parameterBuilder).ToString());

            Assert.Equal(1, parameterBuilder.ParameterCollection.Count);
            Assert.Equal("%", parameterBuilder.ParameterCollection["p0"].Value);
        }

        [Fact]
        public void AppendFilterEntry_uses_OR_to_connect_multiple_filters()
        {
            var parameterBuilder = new ParameterCollectionBuilder();
            var filterBuilder = new StringBuilder();

            EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                filterBuilder,
                "alias",
                new EntityStoreSchemaFilterEntry(null, null, null),
                parameterBuilder);

            EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                filterBuilder,
                "alias",
                new EntityStoreSchemaFilterEntry("catalog", "schema", null),
                parameterBuilder);

            Assert.Equal(
                "(alias.Name LIKE @p0) OR (alias.CatalogName LIKE @p1 AND alias.SchemaName LIKE @p2)",
                filterBuilder.ToString());

            Assert.Equal(3, parameterBuilder.ParameterCollection.Count);
            Assert.Equal("%", parameterBuilder.ParameterCollection["p0"].Value);
            Assert.Equal("catalog", parameterBuilder.ParameterCollection["p1"].Value);
            Assert.Equal("schema", parameterBuilder.ParameterCollection["p2"].Value);
        }

        [Fact]
        public void Where_clause_not_created_for_empty_filter_aliases()
        {
            Assert.Equal(
                string.Empty,
                new EntityStoreSchemaQueryGenerator(
                    string.Empty,
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null, null, null,
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Allow)
                        },
                    filterAliases: new string[0])
                    .CreateWhereClause(new ParameterCollectionBuilder()).ToString());
        }

        [Fact]
        public void Where_clause_not_created_for_empty_filters()
        {
            Assert.Equal(
                string.Empty,
                new EntityStoreSchemaQueryGenerator(
                    string.Empty,
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new EntityStoreSchemaFilterEntry[0],
                    new[] { "alias" })
                    .CreateWhereClause(new ParameterCollectionBuilder()).ToString());
        }

        [Fact]
        public void Where_clause_created_for_single_Allow_filter()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "((alias.CatalogName LIKE @p0 AND alias.SchemaName LIKE @p1 AND alias.Name LIKE @p2))",
                new EntityStoreSchemaQueryGenerator(
                    string.Empty,
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                "catalog",
                                "schema",
                                "name",
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Allow)
                        },
                    new[] { "alias" })
                    .CreateWhereClause(parameterBuilder).ToString());

            Assert.Equal(3, parameterBuilder.Count);
            Assert.Equal(parameterBuilder.ParameterCollection["p0"].Value, "catalog");
            Assert.Equal(parameterBuilder.ParameterCollection["p1"].Value, "schema");
            Assert.Equal(parameterBuilder.ParameterCollection["p2"].Value, "name");
        }

        [Fact]
        public void Where_clause_uses_AND_to_connect_multiple_aliases_and_Allow_filter()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "((alias1.Name LIKE @p0))\r\nAND\r\n((alias2.Name LIKE @p0))",
                new EntityStoreSchemaQueryGenerator(
                    string.Empty,
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                "name",
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Allow),
                        },
                    new[] { "alias1", "alias2" })
                    .CreateWhereClause(parameterBuilder).ToString());

            Assert.Equal(1, parameterBuilder.ParameterCollection.Count);
            Assert.Equal(parameterBuilder.ParameterCollection["p0"].Value, "name");
        }

        [Fact]
        public void Where_clause_created_for_single_Exclude_filter()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "NOT ((alias.CatalogName LIKE @p0 AND alias.SchemaName LIKE @p1 AND alias.Name LIKE @p2))",
                new EntityStoreSchemaQueryGenerator(
                    string.Empty,
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                "catalog",
                                "schema",
                                "name",
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Exclude)
                        },
                    new[] { "alias" })
                    .CreateWhereClause(parameterBuilder).ToString());

            Assert.Equal(3, parameterBuilder.ParameterCollection.Count);
            Assert.Equal(parameterBuilder.ParameterCollection["p0"].Value, "catalog");
            Assert.Equal(parameterBuilder.ParameterCollection["p1"].Value, "schema");
            Assert.Equal(parameterBuilder.ParameterCollection["p2"].Value, "name");
        }

        [Fact]
        public void Where_clause_uses_AND_to_connect_multiple_aliases_and_Exclude_filter()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "NOT ((alias1.Name LIKE @p0))\r\nAND\r\nNOT ((alias2.Name LIKE @p0))",
                new EntityStoreSchemaQueryGenerator(
                    string.Empty,
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                "name",
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Exclude),
                        },
                    new[] { "alias1", "alias2" })
                    .CreateWhereClause(parameterBuilder).ToString());

            Assert.Equal(1, parameterBuilder.ParameterCollection.Count);
            Assert.Equal(parameterBuilder.ParameterCollection["p0"].Value, "name");
        }

        [Fact]
        public void Where_clause_created_when_Allow_and_Exclude_present()
        {
            var parameterBuilder = new ParameterCollectionBuilder();

            Assert.Equal(
                "NOT ((alias.Name LIKE @p0) OR (alias.Name LIKE @p1))",
                new EntityStoreSchemaQueryGenerator(
                    string.Empty,
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                "nameAllowed",
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Exclude),
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                "nameExcluded",
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Exclude)
                        },
                    new[] { "alias" })
                    .CreateWhereClause(parameterBuilder).ToString());

            Assert.Equal(2, parameterBuilder.ParameterCollection.Count);
            Assert.Equal(parameterBuilder.ParameterCollection["p0"].Value, "nameAllowed");
            Assert.Equal(parameterBuilder.ParameterCollection["p1"].Value, "nameExcluded");
        }

        [Fact]
        public void GenerateQuery_returns_base_query_if_no_orderby_clause_and_no_applicable_filters()
        {
            var parameters = new EntityCommand().Parameters;

            Assert.Equal(
                "baseQuery",
                new EntityStoreSchemaQueryGenerator(
                    "baseQuery",
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new EntityStoreSchemaFilterEntry[0],
                    new string[0])
                    .GenerateQuery(parameterBuilder.ParameterCollection), false);

            Assert.Equal(0, parameterBuilder.ParameterCollection.Count);
        }

        [Fact]
        public void GenerateQuery_filters_out_inapplicable_filters()
        {
            var parameters = new EntityCommand().Parameters;

            Assert.Equal(
                "baseQuery",
                new EntityStoreSchemaQueryGenerator(
                    "baseQuery",
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                "name",
                                EntityStoreSchemaFilterObjectTypes.Function,
                                EntityStoreSchemaFilterEffect.Exclude),
                        },
                    new string[0])
                    .GenerateQuery(parameters, false));

            Assert.Equal(0, parameters.Count);
        }

        [Fact]
        public void GenerateQuery_appends_where_clause()
        {
            var parameters = new EntityCommand().Parameters;

            Assert.Equal(
                "baseQuery\r\nWHERE\r\nNOT ((alias.Name LIKE @p0))",
                new EntityStoreSchemaQueryGenerator(
                    "baseQuery",
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Function,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                "name",
                                EntityStoreSchemaFilterObjectTypes.Function,
                                EntityStoreSchemaFilterEffect.Exclude),
                        },
                    new[] { "alias" })
                    .GenerateQuery(parameters, false));

            Assert.Equal(1, parameters.Count);
            Assert.Equal(parameters["p0"].Value, "name");
        }

        [Fact]
        public void GenerateQuery_appends_orderby_if_specified()
        {
            var parameters = new EntityCommand().Parameters;

            Assert.Equal(
                "baseQuery\r\norderBy",
                new EntityStoreSchemaQueryGenerator(
                    "baseQuery",
                    "orderBy",
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new EntityStoreSchemaFilterEntry[0],
                    new string[0])
                    .GenerateQuery(parameters, false));

            Assert.Equal(0, parameters.Count);
        }

        [Fact]
        public void GenerateQuery_appends_orderby_after_where_clause_if_both_are_present()
        {
            var parameters = new EntityCommand().Parameters;

            Assert.Equal(
                "baseQuery\r\nWHERE\r\nNOT ((alias.Name LIKE @p0))\r\norderby",
                new EntityStoreSchemaQueryGenerator(
                    "baseQuery",
                    "orderby",
                    EntityStoreSchemaFilterObjectTypes.Function,
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                "name",
                                EntityStoreSchemaFilterObjectTypes.Function,
                                EntityStoreSchemaFilterEffect.Exclude),
                        },
                    new[] { "alias" })
                    .GenerateQuery(parameters, false));

            Assert.Equal(1, parameters.Count);
            Assert.Equal(parameters["p0"].Value, "name");
        }
    }
}
