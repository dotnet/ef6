namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Diagnostics;

    /// <summary>
    /// Describes metadata about a table
    /// </summary>
    internal class TableMD
    {
        private readonly List<ColumnMD> m_columns;
        private readonly List<ColumnMD> m_keys;

        private readonly EntitySetBase m_extent; // null for transient tables
        private readonly bool m_flattened;

        /// <summary>
        /// private initializer
        /// </summary>
        /// <param name="extent">the entity set corresponding to this table (if any)</param>
        private TableMD(EntitySetBase extent)
        {
            m_columns = new List<ColumnMD>();
            m_keys = new List<ColumnMD>();
            m_extent = extent;
        }

        /// <summary>
        /// Create a typed-table definition corresponding to an entityset (if specified)
        /// 
        /// The table has exactly one column - the type of the column is specified by 
        /// the "type" parameter. This table is considered to be un-"flattened"
        /// </summary>
        /// <param name="type">type of each element (row) of the table</param>
        /// <param name="extent">entityset corresponding to the table (if any)</param>
        internal TableMD(TypeUsage type, EntitySetBase extent)
            : this(extent)
        {
            m_columns.Add(new ColumnMD("element", type));
            m_flattened = !TypeUtils.IsStructuredType(type);
        }

        /// <summary>
        /// Creates a "flattened" table definition. 
        /// 
        /// The table has one column for each specified property in the "properties" parameter. 
        /// The name and datatype of each table column are taken from the corresponding property.
        /// 
        /// The keys of the table (if any) are those specified in the "keyProperties" parameter
        /// 
        /// The table may correspond to an entity set (if the entityset parameter was non-null)
        /// </summary>
        /// <param name="properties">prperties corresponding to columns of the table</param>
        /// <param name="keyProperties"></param>
        /// <param name="extent">entityset corresponding to the table (if any)</param>
        internal TableMD(
            IEnumerable<EdmProperty> properties, IEnumerable<EdmMember> keyProperties,
            EntitySetBase extent)
            : this(extent)
        {
            var columnMap = new Dictionary<string, ColumnMD>();
            m_flattened = true;

            foreach (var p in properties)
            {
                var newColumn = new ColumnMD(p);
                m_columns.Add(newColumn);
                columnMap[p.Name] = newColumn;
            }
            foreach (var p in keyProperties)
            {
                ColumnMD keyColumn;
                if (!columnMap.TryGetValue(p.Name, out keyColumn))
                {
                    Debug.Assert(false, "keyMember not in columns?");
                }
                else
                {
                    m_keys.Add(keyColumn);
                }
            }
        }

        /// <summary>
        /// The extent metadata (if any)
        /// </summary>
        internal EntitySetBase Extent
        {
            get { return m_extent; }
        }

        /// <summary>
        /// List of columns of this table
        /// </summary>
        internal List<ColumnMD> Columns
        {
            get { return m_columns; }
        }

        /// <summary>
        /// Keys for this table
        /// </summary>
        internal List<ColumnMD> Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        /// Is this table a "flat" table?
        /// </summary>
        internal bool Flattened
        {
            get { return m_flattened; }
        }

        /// <summary>
        /// String form - for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (m_extent != null ? m_extent.Name : "Transient");
        }
    }
}
