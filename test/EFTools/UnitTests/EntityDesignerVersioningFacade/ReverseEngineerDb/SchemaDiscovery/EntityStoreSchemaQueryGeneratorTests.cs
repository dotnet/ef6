// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System.Data.Entity.Core.EntityClient;
    using System.Text;
    using Xunit;

    public class EntityStoreSchemaQueryGeneratorTests
    {
        private static ParameterCollectionBuilder CreateParameters(bool optimized = false)
        {
            return new ParameterCollectionBuilder(new EntityCommand().Parameters, optimized);
        }

        [Fact]
        public void AppendComparison_does_not_create_comparison_fragment_or_corresponding_parameter_for_non_value()
        {
            Assert.Empty(
                EntityStoreSchemaQueryGenerator.AppendComparison(
                    new StringBuilder(),
                    string.Empty,
                    string.Empty,
                    /* value */ null,
                    CreateParameters()).ToString());
        }

        [Fact]
        public void AppendComparison_creates_comparison_fragment_and_corresponding_parameter_for_non_null_value()
        {
            var parameters = CreateParameters();

            Assert.Equal(
                "alias.propertyName LIKE @p0",
                EntityStoreSchemaQueryGenerator.AppendComparison(
                    new StringBuilder(),
                    "alias",
                    "propertyName",
                    "Value",
                    parameters).ToString());

            Assert.Equal(1, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("Value", parameters["p0"].Value);
        }

        [Fact]
        public void AppendComparison_creates_parameters_and_adds_AND_for_multiple_comparisons()
        {
            var parameters = CreateParameters();

            var filterBuilder =
                EntityStoreSchemaQueryGenerator.AppendComparison(
                    new StringBuilder(),
                    "alias1",
                    "propertyName1",
                    "Value1",
                    parameters);

            EntityStoreSchemaQueryGenerator.AppendComparison(
                filterBuilder,
                "alias2",
                "propertyName2",
                "Value2",
                parameters);

            Assert.Equal(
                "alias1.propertyName1 LIKE @p0 AND alias2.propertyName2 LIKE @p1",
                filterBuilder.ToString());

            Assert.Equal(2, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("Value1", parameters["p0"].Value);
            Assert.Equal("Value2", parameters["p1"].Value);
        }

        [Fact]
        public void AppendFilterEntry_creates_filter_for_catalog_schema_and_name_if_all_specified()
        {
            var parameters = CreateParameters();

            Assert.Equal(
                "(alias.CatalogName LIKE @p0 AND alias.SchemaName LIKE @p1 AND alias.Name LIKE @p2)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry("catalog", "schema", "name"),
                    parameters).ToString());

            Assert.Equal(3, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("catalog", parameters["p0"].Value);
            Assert.Equal("schema", parameters["p1"].Value);
            Assert.Equal("name", parameters["p2"].Value);
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
                    CreateParameters()).ToString());

            Assert.Equal(
                "(alias.CatalogName LIKE @p0 AND alias.Name LIKE @p1)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry("catalog", null, "name"),
                    CreateParameters()).ToString());

            Assert.Equal(
                "(alias.CatalogName LIKE @p0 AND alias.SchemaName LIKE @p1)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry("catalog", "schema", null),
                    CreateParameters()).ToString());
        }

        [Fact]
        public void AppendFilterEntry_uses_wildcard_parameter_value_if_schema_catalog_and_name_are_null()
        {
            var parameters = CreateParameters();

            Assert.Equal(
                "(alias.Name LIKE @p0)",
                EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                    new StringBuilder(),
                    "alias",
                    new EntityStoreSchemaFilterEntry(null, null, null),
                    parameters).ToString());

            Assert.Equal(1, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("%", parameters["p0"].Value);
        }

        [Fact]
        public void AppendFilterEntry_uses_OR_to_connect_multiple_filters()
        {
            var parameters = CreateParameters();
            var filterBuilder = new StringBuilder();

            EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                filterBuilder,
                "alias",
                new EntityStoreSchemaFilterEntry(null, null, null),
                parameters);

            EntityStoreSchemaQueryGenerator.AppendFilterEntry(
                filterBuilder,
                "alias",
                new EntityStoreSchemaFilterEntry("catalog", "schema", null),
                parameters);

            Assert.Equal(
                "(alias.Name LIKE @p0) OR (alias.CatalogName LIKE @p1 AND alias.SchemaName LIKE @p2)",
                filterBuilder.ToString());

            Assert.Equal(3, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("p1", parameters[1].ParameterName);
            Assert.Equal("%", parameters["p0"].Value);
            Assert.Equal("catalog", parameters["p1"].Value);
            Assert.Equal("schema", parameters["p2"].Value);
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
                    .CreateWhereClause(CreateParameters()).ToString());
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
                    .CreateWhereClause(CreateParameters()).ToString());
        }

        [Fact]
        public void Where_clause_created_for_single_Allow_filter()
        {
            var parameters = CreateParameters();

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
                    .CreateWhereClause(parameters).ToString());

            Assert.Equal(3, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("p1", parameters[1].ParameterName);
            Assert.Equal(parameters["p0"].Value, "catalog");
            Assert.Equal(parameters["p1"].Value, "schema");
            Assert.Equal(parameters["p2"].Value, "name");
        }

        [Fact]
        public void Where_clause_uses_AND_to_connect_multiple_aliases_and_Allow_filter()
        {
            var parameters = CreateParameters();

            Assert.Equal(
                "((alias1.Name LIKE @p0))\r\nAND\r\n((alias2.Name LIKE @p1))",
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
                    .CreateWhereClause(parameters).ToString());

            Assert.Equal(2, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("p1", parameters[1].ParameterName);
            Assert.Equal(parameters["p0"].Value, "name");
            Assert.Equal(parameters["p1"].Value, "name");
        }

        [Fact]
        public void Where_clause_uses_AND_to_connect_multiple_aliases_and_Allow_filter_and_optimizes_parameters()
        {
            var parameters = CreateParameters(optimized: true);

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
                    .CreateWhereClause(parameters).ToString());

            Assert.Equal(1, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal(parameters["p0"].Value, "name");
        }

        [Fact]
        public void Where_clause_created_for_single_Exclude_filter()
        {
            var parameters = CreateParameters();

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
                    .CreateWhereClause(parameters).ToString());

            Assert.Equal(3, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("p1", parameters[1].ParameterName);
            Assert.Equal(parameters["p0"].Value, "catalog");
            Assert.Equal(parameters["p1"].Value, "schema");
            Assert.Equal(parameters["p2"].Value, "name");
        }

        [Fact]
        public void Where_clause_uses_AND_to_connect_multiple_aliases_and_Exclude_filter()
        {
            var parameters = CreateParameters();

            Assert.Equal(
                "NOT ((alias1.Name LIKE @p0))\r\nAND\r\nNOT ((alias2.Name LIKE @p1))",
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
                    .CreateWhereClause(parameters).ToString());

            Assert.Equal(2, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("p1", parameters[1].ParameterName);
            Assert.Equal(parameters["p0"].Value, "name");
            Assert.Equal(parameters["p1"].Value, "name");
        }

        [Fact]
        public void Where_clause_uses_AND_to_connect_multiple_aliases_and_Exclude_filter_and_optimizes_parameters()
        {
            var parameters = CreateParameters(optimized: true);

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
                    .CreateWhereClause(parameters).ToString());

            Assert.Equal(1, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal(parameters["p0"].Value, "name");
        }

        [Fact]
        public void Where_clause_created_when_Allow_and_Exclude_present()
        {
            var parameters = CreateParameters();

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
                    .CreateWhereClause(parameters).ToString());

            Assert.Equal(2, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("p1", parameters[1].ParameterName);
            Assert.Equal(parameters["p0"].Value, "nameAllowed");
            Assert.Equal(parameters["p1"].Value, "nameExcluded");
        }

        [Fact]
        public void GenerateQuery_returns_base_query_if_no_orderby_clause_and_no_applicable_filters()
        {
            var parameters = CreateParameters();

            Assert.Equal(
                "baseQuery",
                new EntityStoreSchemaQueryGenerator(
                    "baseQuery",
                    string.Empty,
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new EntityStoreSchemaFilterEntry[0],
                    new string[0])
                    .GenerateQuery(parameters));

            Assert.Equal(0, parameters.Count);
        }

        [Fact]
        public void GenerateQuery_filters_out_inapplicable_filters()
        {
            var parameters = CreateParameters();

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
                    .GenerateQuery(parameters));

            Assert.Equal(0, parameters.Count);
        }

        [Fact]
        public void GenerateQuery_appends_where_clause()
        {
            var parameters = CreateParameters();

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
                    .GenerateQuery(parameters));

            Assert.Equal(1, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("name", parameters["p0"].Value);
        }

        [Fact]
        public void GenerateQuery_appends_orderby_if_specified()
        {
            var parameters = CreateParameters();

            Assert.Equal(
                "baseQuery\r\norderBy",
                new EntityStoreSchemaQueryGenerator(
                    "baseQuery",
                    "orderBy",
                    EntityStoreSchemaFilterObjectTypes.Table,
                    new EntityStoreSchemaFilterEntry[0],
                    new string[0])
                    .GenerateQuery(parameters));

            Assert.Equal(0, parameters.Count);
        }

        [Fact]
        public void GenerateQuery_appends_orderby_after_where_clause_if_both_are_present()
        {
            var parameters = CreateParameters();

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
                    .GenerateQuery(parameters));

            Assert.Equal(1, parameters.Count);
            Assert.Equal("p0", parameters[0].ParameterName);
            Assert.Equal("name", parameters["p0"].Value);
        }
    }
}
