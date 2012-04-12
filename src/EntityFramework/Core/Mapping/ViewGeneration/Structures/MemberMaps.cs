namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Diagnostics;

    // This class manages the different maps used in the view generation
    // process. These maps keep track of indexes of memberpaths, domains of
    // member paths, etc
    internal class MemberMaps
    {
        #region Fields

        private readonly MemberProjectionIndex m_projectedSlotMap;
        private readonly MemberDomainMap m_queryDomainMap;
        private readonly MemberDomainMap m_updateDomainMap;
        private readonly ViewTarget m_viewTarget;

        #endregion

        #region Constructors

        internal MemberMaps(
            ViewTarget viewTarget, MemberProjectionIndex projectedSlotMap,
            MemberDomainMap queryDomainMap, MemberDomainMap updateDomainMap)
        {
            m_projectedSlotMap = projectedSlotMap;
            m_queryDomainMap = queryDomainMap;
            m_updateDomainMap = updateDomainMap;

            Debug.Assert(m_queryDomainMap != null);
            Debug.Assert(m_updateDomainMap != null);
            Debug.Assert(m_projectedSlotMap != null);
            m_viewTarget = viewTarget;
        }

        #endregion

        #region Properties

        internal MemberProjectionIndex ProjectedSlotMap
        {
            get { return m_projectedSlotMap; }
        }

        internal MemberDomainMap QueryDomainMap
        {
            get { return m_queryDomainMap; }
        }

        internal MemberDomainMap UpdateDomainMap
        {
            get { return m_updateDomainMap; }
        }

        internal MemberDomainMap RightDomainMap
        {
            get { return m_viewTarget == ViewTarget.QueryView ? m_updateDomainMap : m_queryDomainMap; }
        }

        internal MemberDomainMap LeftDomainMap
        {
            get { return m_viewTarget == ViewTarget.QueryView ? m_queryDomainMap : m_updateDomainMap; }
        }

        #endregion
    }
}
