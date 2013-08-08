// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// Describes a column of a table
    /// </summary>
    internal sealed class ColumnVar : Var
    {
        private readonly ColumnMD m_columnMetadata;
        private readonly Table m_table;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ColumnVar(int id, Table table, ColumnMD columnMetadata)
            : base(id, VarType.Column, columnMetadata.Type)
        {
            m_table = table;
            m_columnMetadata = columnMetadata;
        }

        /// <summary>
        /// The table instance containing this column reference
        /// </summary>
        internal Table Table
        {
            get { return m_table; }
        }

        /// <summary>
        /// The column metadata for this column
        /// </summary>
        internal ColumnMD ColumnMetadata
        {
            get { return m_columnMetadata; }
        }

        /// <summary>
        /// Get the name of this column var
        /// </summary>
        internal override bool TryGetName(out string name)
        {
            name = m_columnMetadata.Name;
            return true;
        }
    }
}
