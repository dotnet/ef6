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
            var parameterMap = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var alias in _filterAliases)
            {
                var allows = new StringBuilder();
                var excludes = new StringBuilder();
                foreach (var entry in _filters)
                {
                    if (entry.Effect == EntityStoreSchemaFilterEffect.Allow)
                    {
                        AppendFilterEntry(allows, alias, entry, parameterMap);
                    }
                    else
                    {
                        Debug.Assert(entry.Effect == EntityStoreSchemaFilterEffect.Exclude, "did you add new value?");
                        AppendFilterEntry(excludes, alias, entry, parameterMap);
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

            foreach(var entry in parameterMap)
            {
                parameters.AddWithValue(entry.Value, entry.Key);
            }

            return whereClause;
        }

        // internal to allow unit testing
        internal static StringBuilder AppendFilterEntry(
            StringBuilder segment, string alias, EntityStoreSchemaFilterEntry entry, Dictionary<string, string> parameterMap)
        {
            Debug.Assert(segment != null, "segment != null");
            Debug.Assert(alias != null, "alias != null");
            Debug.Assert(entry != null, "entry != null");
            Debug.Assert(parameterMap != null, "parameters != null");

            var filterText = new StringBuilder();
            AppendComparison(filterText, alias, "CatalogName", entry.Catalog, parameterMap);
            AppendComparison(filterText, alias, "SchemaName", entry.Schema, parameterMap);
            AppendComparison(
                filterText, alias, "Name",
                entry.Catalog == null && entry.Schema == null && entry.Name == null ? "%" : entry.Name,
                parameterMap);
            segment
                .AppendIfNotEmpty(" OR ")
                .Append("(")
                .Append(filterText)
                .Append(")");

            return segment;
        }

        // internal to allow unit testing
        internal static StringBuilder AppendComparison(
            StringBuilder filterFragment, string alias, string propertyName, string value, Dictionary<string, string> parameterMap)
        {
            Debug.Assert(filterFragment != null, "filterFragment != null");
            Debug.Assert(alias != null, "alias != null");
            Debug.Assert(propertyName != null, "propertyName != null");
            Debug.Assert(parameterMap != null, "parameters != null");

            if (value != null)
            {
                string parameterName = null;                
                if (!parameterMap.TryGetValue(value, out parameterName))
                { 
                    parameterName = GetParameterName(parameterMap.Count);
                    parameterMap.Add(value, parameterName);
                }
                AppendComparisonFragment(filterFragment, alias, propertyName, parameterName);
            }

            return filterFragment;
        }

        private static string GetParameterName(int position)
        {
            return "p" + position.ToString(CultureInfo.InvariantCulture);
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
