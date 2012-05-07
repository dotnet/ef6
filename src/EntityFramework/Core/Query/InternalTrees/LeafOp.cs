namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// LeafOp - matches any subtree 
    /// </summary>
    internal sealed class LeafOp : RulePatternOp
    {
        /// <summary>
        /// The singleton instance of this class
        /// </summary>
        internal static readonly LeafOp Instance = new LeafOp();

        internal static readonly LeafOp Pattern = Instance;

        /// <summary>
        /// 0 children
        /// </summary>
        internal override int Arity
        {
            get { return 0; }
        }

        #region constructors

        /// <summary>
        /// Niladic constructor
        /// </summary>
        private LeafOp()
            : base(OpType.Leaf)
        {
        }

        #endregion
    }
}
