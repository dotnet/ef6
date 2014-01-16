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
        private readonly string _tableName;
        private IndexAttribute _index;
        private readonly IDictionary<int, string> _columnNames = new Dictionary<int, string>();

        public ConsolidatedIndex(string tableName, string columnName, IndexAttribute index)
        {
            DebugCheck.NotEmpty(tableName);
            DebugCheck.NotNull(index);

            _tableName = tableName;
            _index = index;
            _columnNames[index.Order] = columnName;
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
                    var consolidated = index.Name == null ? null : allIndexes.FirstOrDefault(i => i.Index.Name == index.Name);
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

        public IEnumerable<string> ColumnNames
        {
            get { return _columnNames.OrderBy(c => c.Key).Select(c => c.Value); }
        }

        public void Add(string columnName, IndexAttribute index)
        {
            DebugCheck.NotEmpty(columnName);
            DebugCheck.NotNull(index);

            Debug.Assert(_index.Name == index.Name);

            if (_columnNames.ContainsKey(index.Order))
            {
                throw new InvalidOperationException(
                    Strings.OrderConflictWhenConsolidating(index.Name, _tableName, index.Order, _columnNames[index.Order], columnName));
            }

            _columnNames[index.Order] = columnName;

            var compat = _index.IsCompatibleWith(index, ignoreOrder: true);
            if (!compat)
            {
                throw new InvalidOperationException(Strings.ConflictWhenConsolidating(index.Name, _tableName, compat.ErrorMessage));
            }

            _index = _index.MergeWith(index, ignoreOrder: true);
        }

        public CreateIndexOperation CreateCreateIndexOperation()
        {
            var columnNames = ColumnNames.ToArray();
            Debug.Assert(columnNames.Length > 0);
            Debug.Assert(_index.Name != null || columnNames.Length == 1);
            
            var operation = new CreateIndexOperation
            {
                Name = _index.Name ?? columnNames[0] + "Index",
                Table = _tableName
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

        protected bool Equals(ConsolidatedIndex other)
        {
            return _tableName == other._tableName
                   && _index.Equals(other._index)
                   && ColumnNames.SequenceEqual(other.ColumnNames);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ConsolidatedIndex)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ColumnNames.Aggregate(
                    (_tableName.GetHashCode() * 397) ^ _index.GetHashCode(),
                    (h, v) => (h * 397) ^ v.GetHashCode());
            }
        }
    }
}
