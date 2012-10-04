// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;

    [DebuggerDisplay("{Table.Name}")]
    internal class TableMapping
    {
        private readonly DbTableMetadata _table;
        private readonly SortedEntityTypeIndex _entityTypes;
        private readonly List<ColumnMapping> _columns;

        public TableMapping(DbTableMetadata table)
        {
            Contract.Requires(table != null);
            _table = table;
            _entityTypes = new SortedEntityTypeIndex();
            _columns = new List<ColumnMapping>();
        }

        public DbTableMetadata Table
        {
            get { return _table; }
        }

        public SortedEntityTypeIndex EntityTypes
        {
            get { return _entityTypes; }
        }

        public IEnumerable<ColumnMapping> ColumnMappings
        {
            get { return _columns; }
        }

        public void AddEntityTypeMappingFragment(
            EntitySet entitySet, EntityType entityType, DbEntityTypeMappingFragment fragment)
        {
            Contract.Assert(fragment.Table == Table);

            _entityTypes.Add(entitySet, entityType);

            var defaultDiscriminatorColumn = fragment.GetDefaultDiscriminator();
            DbColumnCondition defaultDiscriminatorCondition = null;
            if (defaultDiscriminatorColumn != null)
            {
                defaultDiscriminatorCondition =
                    fragment.ColumnConditions.SingleOrDefault(cc => cc.Column == defaultDiscriminatorColumn);
            }

            foreach (var pm in fragment.PropertyMappings)
            {
                var columnMapping = FindOrCreateColumnMapping(pm.Column);
                columnMapping.AddMapping(
                    entityType,
                    pm.PropertyPath,
                    fragment.ColumnConditions.Where(cc => cc.Column == pm.Column),
                    defaultDiscriminatorColumn == pm.Column);
            }

            // Add any column conditions that aren't mapped to properties
            foreach (
                var cc in
                    fragment.ColumnConditions.Where(cc => !fragment.PropertyMappings.Any(pm => pm.Column == cc.Column)))
            {
                var columnMapping = FindOrCreateColumnMapping(cc.Column);
                columnMapping.AddMapping(entityType, null, new[] { cc }, defaultDiscriminatorColumn == cc.Column);
            }
        }

        private ColumnMapping FindOrCreateColumnMapping(DbTableColumnMetadata column)
        {
            var columnMapping = _columns.SingleOrDefault(c => c.Column == column);
            if (columnMapping == null)
            {
                columnMapping = new ColumnMapping(column);
                _columns.Add(columnMapping);
            }

            return columnMapping;
        }
    }
}
