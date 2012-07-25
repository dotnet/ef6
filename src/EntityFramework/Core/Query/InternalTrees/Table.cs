// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Represents one instance of a table. Contains the table metadata
    /// </summary>
    internal class Table
    {
        private readonly TableMD m_tableMetadata;
        private readonly VarList m_columns;
        private readonly VarVec m_referencedColumns;
        private readonly VarVec m_keys;
        private readonly VarVec m_nonnullableColumns;
        private readonly int m_tableId;

        internal Table(Command command, TableMD tableMetadata, int tableId)
        {
            m_tableMetadata = tableMetadata;
            m_columns = Command.CreateVarList();
            m_keys = command.CreateVarVec();
            m_nonnullableColumns = command.CreateVarVec();
            m_tableId = tableId;

            var columnVarMap = new Dictionary<string, ColumnVar>();
            foreach (var c in tableMetadata.Columns)
            {
                var v = command.CreateColumnVar(this, c);
                columnVarMap[c.Name] = v;
                if (!c.IsNullable)
                {
                    m_nonnullableColumns.Set(v);
                }
            }

            foreach (var c in tableMetadata.Keys)
            {
                var v = columnVarMap[c.Name];
                m_keys.Set(v);
            }

            m_referencedColumns = command.CreateVarVec(m_columns);
        }

        /// <summary>
        /// Metadata for the table instance
        /// </summary>
        internal TableMD TableMetadata
        {
            get { return m_tableMetadata; }
        }

        /// <summary>
        /// List of column references
        /// </summary>
        internal VarList Columns
        {
            get { return m_columns; }
        }

        /// <summary>
        /// Get the list of all referenced columns. 
        /// </summary>
        internal VarVec ReferencedColumns
        {
            get { return m_referencedColumns; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal VarVec NonNullableColumns
        {
            get { return m_nonnullableColumns; }
        }

        /// <summary>
        /// List of keys
        /// </summary>
        internal VarVec Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        /// (internal) id for this table instance
        /// </summary>
        internal int TableId
        {
            get { return m_tableId; }
        }

        /// <summary>
        /// String form - for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}::{1}", m_tableMetadata, TableId);
            ;
        }
    }
}
