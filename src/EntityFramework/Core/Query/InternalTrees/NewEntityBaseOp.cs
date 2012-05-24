namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Base class for DiscriminatedNewEntityOp and NewEntityOp
    /// </summary>
    internal abstract class NewEntityBaseOp : ScalarOp
    {
        #region private state

        private readonly bool m_scoped;
        private readonly EntitySet m_entitySet;
        private readonly List<RelProperty> m_relProperties; // list of relationship properties for which we have values

        #endregion

        #region constructors

        internal NewEntityBaseOp(OpType opType, TypeUsage type, bool scoped, EntitySet entitySet, List<RelProperty> relProperties)
            : base(opType, type)
        {
            Debug.Assert(scoped || entitySet == null, "entitySet cann't be set of constructor isn't scoped");
            Debug.Assert(relProperties != null, "expected non-null list of rel-properties");
            m_scoped = scoped;
            m_entitySet = entitySet;
            m_relProperties = relProperties;
        }

        protected NewEntityBaseOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public APIs

        /// <summary>
        /// True if the entity constructor is scoped to a particular entity set or null (scoped as "unscoped").
        /// False if the scope is not yet known. Scope is determined in PreProcessor.
        /// </summary>
        internal bool Scoped
        {
            get { return m_scoped; }
        }

        /// <summary>
        /// Get the entityset (if any) associated with this constructor
        /// </summary>
        internal EntitySet EntitySet
        {
            get { return m_entitySet; }
        }

        /// <summary>
        /// get the list of relationship properties (if any) specified for this constructor
        /// </summary>
        internal List<RelProperty> RelationshipProperties
        {
            get { return m_relProperties; }
        }

        #endregion
    }
}
