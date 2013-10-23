// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    internal class EntityStoreSchemaQueryGenerator
    {
        private readonly string _baseQuery;
        private readonly string _orderByClause;
        private readonly EntityStoreSchemaFilterEntry[] _filters;
        private readonly string[] _filterAliases;

        public EntityStoreSchemaQueryGenerator(
            string baseQuery, string orderByClause, EntityStoreSchemaFilterObjectTypes queryTypes,
            IEnumerable<EntityStoreSchemaFilterEntry> filters, string[] filterAliases)
        {
            _baseQuery = baseQuery;
            _orderByClause = orderByClause;
            _filters = filters.Where(entry => (queryTypes & entry.Types) != 0).ToArray();
            _filterAliases = filterAliases;
        }

        public string GenerateQuery(EntityParameterCollection parameters)
        {
            Debug.Assert(parameters != null, "parameters != null");

            if (string.IsNullOrWhiteSpace(_orderByClause)
                && !_filters.Any())
            {
                return _baseQuery;
            }

            var sqlStatement = new StringBuilder(_baseQuery);

            var whereClause = CreateWhereClause(parameters);

            if (whereClause.Length != 0)
            {
                sqlStatement
                    .Append(Environment.NewLine)
                    .Append("WHERE")
                    .Append(Environment.NewLine)
                    .Append(whereClause);
            }

            if (!string.IsNullOrEmpty(_orderByClause))
            {
                sqlStatement.Append(Environment.NewLine);
                sqlStatement.Append(_orderByClause);
            }

            return sqlStatement.ToString();
        }

        // internal to allow unit testing
        internal StringBuilder CreateWhereClause(EntityParameterCollection parameters)
        {
            Debug.Assert(parameters != null, "parameters != null");

            var whereClause = new StringBuilder();
            foreach (var alias in _filterAliases)
            {
                var allows = new StringBuilder();
                var excludes = new StringBuilder();
                foreach (var entry in _filters)
                {
                    if (entry.Effect == EntityStoreSchemaFilterEffect.Allow)
                    {
                        AppendFilterEntry(allows, alias, entry, parameters);
                    }
                    else
                    {
                        Debug.Assert(entry.Effect == EntityStoreSchemaFilterEffect.Exclude, "did you add new value?");
                        AppendFilterEntry(excludes, alias, entry, parameters);
                    }
                }

                if (allows.Length != 0)
                {
                    // AND appended if this is not the first condition
                    whereClause
                        .AppendIfNotEmpty(Environment.NewLine)
                        .AppendIfNotEmpty("AND")
                        .AppendIfNotEmpty(Environment.NewLine);

                    whereClause
                        .Append("(")
                        .Append(allows)
                        .Append(")");
                }

                if (excludes.Length != 0)
                {
                    // AND appended if this is not the first condition
                    whereClause
                        .AppendIfNotEmpty(Environment.NewLine)
                        .AppendIfNotEmpty("AND")
                        .AppendIfNotEmpty(Environment.NewLine);

                    whereClause
                        .Append("NOT (")
                        .Append(excludes)
                        .Append(")");
                }
            }
            return whereClause;
        }

        // internal to allow unit testing
        internal static StringBuilder AppendFilterEntry(
            StringBuilder segment, string alias, EntityStoreSchemaFilterEntry entry, EntityParameterCollection parameters)
        {
            Debug.Assert(segment != null, "segment != null");
            Debug.Assert(alias != null, "alias != null");
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(parameters != null, "parameters != null");

            var filterText = new StringBuilder();
            AppendComparison(filterText, alias, "CatalogName", entry.Catalog, parameters);
            AppendComparison(filterText, alias, "SchemaName", entry.Schema, parameters);
            AppendComparison(
                filterText, alias, "Name",
                entry.Catalog == null && entry.Schema == null && entry.Name == null ? "%" : entry.Name,
                parameters);
            segment
                .AppendIfNotEmpty(" OR ")
                .Append("(")
                .Append(filterText)
                .Append(")");

            return segment;
        }

        // internal to allow unit testing
        internal static StringBuilder AppendComparison(
            StringBuilder filterFragment, string alias, string propertyName, string value, EntityParameterCollection parameters)
        {
            Debug.Assert(filterFragment != null, "filterFragment != null");
            Debug.Assert(alias != null, "alias != null");
            Debug.Assert(propertyName != null, "propertyName != null");
            Debug.Assert(parameters != null, "parameters != null");

            if (value != null)
            {
                var parameterName = "p" + parameters.Count.ToString(CultureInfo.InvariantCulture);

                AppendComparisonFragment(filterFragment, alias, propertyName, parameterName);
                parameters.Add(
                    new EntityParameter
                        {
                            ParameterName = parameterName,
                            Value = value
                        });
            }

            return filterFragment;
        }

        private static void AppendComparisonFragment(
            StringBuilder filterFragment, string alias, string propertyName, string parameterName)
        {
            Debug.Assert(filterFragment != null, "filterFragment != null");
            Debug.Assert(alias != null, "alias != null");
            Debug.Assert(propertyName != null, "propertyName != null");

            filterFragment
                .AppendIfNotEmpty(" AND ")
                .Append(alias)
                .Append(".")
                .Append(propertyName)
                .Append(" LIKE @")
                .Append(parameterName);
        }
    }
}
