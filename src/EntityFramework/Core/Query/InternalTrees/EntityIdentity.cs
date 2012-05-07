namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// Abstract base class representing entity identity. Used by both
    /// EntityColumnMap and RefColumnMap.
    /// An EntityIdentity captures two pieces of information - the list of keys
    /// that uniquely identify an entity within an entityset, and the the entityset
    /// itself.
    /// </summary>
    internal abstract class EntityIdentity
    {
        private readonly SimpleColumnMap[] m_keys; // list of keys

        /// <summary>
        /// Simple constructor - gets a list of key columns
        /// </summary>
        /// <param name="keyColumns"></param>
        internal EntityIdentity(SimpleColumnMap[] keyColumns)
        {
            Debug.Assert(keyColumns != null, "Must specify column maps for key columns");
            m_keys = keyColumns;
        }

        /// <summary>
        /// Get the key columns
        /// </summary>
        internal SimpleColumnMap[] Keys
        {
            get { return m_keys; }
        }
    }
}