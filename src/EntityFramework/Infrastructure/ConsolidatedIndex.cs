// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal class ConsolidatedIndex
    {
        private readonly string _table;
        private IndexAttribute _index;
        private readonly IDictionary<int, string> _columns = new Dictionary<int, string>();

        public ConsolidatedIndex(string table, IndexAttribute index)
        {
            DebugCheck.NotEmpty(table);
            DebugCheck.NotNull(index);

            _table = table;
            _index = index;
        }

        public ConsolidatedIndex(string table, string column, IndexAttribute index)
            : this(table, index)
        {
            DebugCheck.NotEmpty(table);
            DebugCheck.NotEmpty(column);
            DebugCheck.NotNull(index);

            _columns[index.Order] = column;
        }

        public static IEnumerable<ConsolidatedIndex> BuildIndexes(string tableName, IEnumerable<Tuple<string, EdmProperty>> columns)
        {
            DebugCheck.NotEmpty(tableName);
            DebugCheck.NotNull(columns);

            var allIndexes = new List<ConsolidatedIndex>();

            foreach (var column in columns)
            {
                foreach (var index in column.Item2.Annotations.Where(a => a.Name == XmlConstants.IndexAnnotationWithPrefix)
                    .Select(a => a.Value)
                    .OfType<IndexAnnotation>()
                    .SelectMany(a => a.Indexes))
                {
                    
                    var consolidatedCandidates = allIndexes.Where(
                        i => (index.Identity != null && i.Index.Identity == index.Identity) 
                            || (index.Name != null && i.Index.Name == index.Name));

                    if (consolidatedCandidates.Count() > 1)
                    {
                        throw Error.ConflictingIndexAttributeMatches(index.Identity, index.Name);
                    }

                    var consolidated = consolidatedCandidates.SingleOrDefault();
                    
                    if (consolidated == null)
                    {
                        allIndexes.Add(new ConsolidatedIndex(tableName, column.Item1, index));
                    }
                    else
                    {
                        consolidated.Add(column.Item1, index);
                    }
                }
            }

            return allIndexes;
        }

        public IndexAttribute Index
        {
            get { return _index; }
        }

        public IEnumerable<string> Columns
        {
            get { return _columns.OrderBy(c => c.Key).Select(c => c.Value); }
        }

        public string Table
        {
            get { return _table; }
        }

        public void Add(string columnName, IndexAttribute index)
        {
            DebugCheck.NotEmpty(columnName);
            DebugCheck.NotNull(index);

            Debug.Assert(_index.Identity == index.Identity || _index.Name == index.Name);

            if (_columns.ContainsKey(index.Order))
            {
                throw new InvalidOperationException(
                    Strings.OrderConflictWhenConsolidating(index.Name, _table, index.Order, _columns[index.Order], columnName));
            }

            _columns[index.Order] = columnName;

            var compat = _index.IsCompatibleWith(index, ignoreOrder: true);
            if (!compat)
            {
                throw new InvalidOperationException(Strings.ConflictWhenConsolidating(index.Name, _table, compat.ErrorMessage));
            }

            _index = _index.MergeWith(index, ignoreOrder: true);
        }

        public CreateIndexOperation CreateCreateIndexOperation()
        {
            var columnNames = Columns.ToArray();
            Debug.Assert(columnNames.Length > 0);
            Debug.Assert(_index.Identity != null || _index.Name != null || columnNames.Length == 1);

            var operation = new CreateIndexOperation
            {
                Name = _index.Name ?? IndexOperation.BuildDefaultName(columnNames),
                Table = _table
            };

            foreach (var columnName in columnNames)
            {
                operation.Columns.Add(columnName);
            }

            if (_index.IsClusteredConfigured)
            {
                operation.IsClustered = _index.IsClustered;
            }

            if (_index.IsUniqueConfigured)
            {
                operation.IsUnique = _index.IsUnique;
            }

            return operation;
        }

        public DropIndexOperation CreateDropIndexOperation()
        {
            return (DropIndexOperation)CreateCreateIndexOperation().Inverse;
        }
    }
}
