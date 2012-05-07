namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// Base class for set operations - union, intersect, except
    /// </summary>
    internal abstract class SetOp : RelOp
    {
        #region private state

        private readonly VarMap[] m_varMap;
        private readonly VarVec m_outputVars;

        #endregion

        #region constructors

        internal SetOp(OpType opType, VarVec outputs, VarMap left, VarMap right)
            : this(opType)
        {
            m_varMap = new VarMap[2];
            m_varMap[0] = left;
            m_varMap[1] = right;
            m_outputVars = outputs;
        }

        protected SetOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// 2 children - left, right
        /// </summary>
        internal override int Arity
        {
            get { return 2; }
        }

        /// <summary>
        /// Map of result vars to the vars of each branch of the setOp
        /// </summary>
        internal VarMap[] VarMap
        {
            get { return m_varMap; }
        }

        /// <summary>
        /// Get the set of output vars produced
        /// </summary>
        internal VarVec Outputs
        {
            get { return m_outputVars; }
        }

        #endregion
    }
}