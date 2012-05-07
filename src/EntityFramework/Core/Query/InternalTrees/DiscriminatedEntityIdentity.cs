namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// This class also represents entity identity. However, this class addresses
    /// those scenarios where the entityset for the entity is not uniquely known
    /// a priori. Instead, the query is annotated with information, and based on
    /// the resulting information, the appropriate entityset is identified.
    /// Specifically, the specific entityset is represented as a SimpleColumnMap
    /// in the query. The value of that column is used to look up a dictionary,
    /// and then identify the appropriate entity set.
    /// It is entirely possible that no entityset may be located for the entity
    /// instance - this represents a transient entity instance
    /// </summary>
    internal class DiscriminatedEntityIdentity : EntityIdentity
    {
        private readonly SimpleColumnMap m_entitySetColumn; // (optional) column map representing the entity set
        private readonly EntitySet[] m_entitySetMap; // optional dictionary that maps values to entitysets

        /// <summary>
        /// Simple constructor
        /// </summary>
        /// <param name="entitySetColumn">column map representing the entityset</param>
        /// <param name="entitySetMap">Map from value -> the appropriate entityset</param>
        /// <param name="keyColumns">list of key columns</param>
        internal DiscriminatedEntityIdentity(
            SimpleColumnMap entitySetColumn, EntitySet[] entitySetMap,
            SimpleColumnMap[] keyColumns)
            : base(keyColumns)
        {
            Debug.Assert(entitySetColumn != null, "Must specify a column map to identify the entity set");
            Debug.Assert(entitySetMap != null, "Must specify a dictionary to look up entitysets");
            m_entitySetColumn = entitySetColumn;
            m_entitySetMap = entitySetMap;
        }

        /// <summary>
        /// Get the column map representing the entityset
        /// </summary>
        internal SimpleColumnMap EntitySetColumnMap
        {
            get { return m_entitySetColumn; }
        }

        /// <summary>
        /// Return the entityset map
        /// </summary>
        internal EntitySet[] EntitySetMap
        {
            get { return m_entitySetMap; }
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var separator = String.Empty;
            sb.AppendFormat(CultureInfo.InvariantCulture, "[(Keys={");
            foreach (var c in Keys)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, c);
                separator = ",";
            }
            sb.AppendFormat(CultureInfo.InvariantCulture, "})]");
            return sb.ToString();
        }
    }
}