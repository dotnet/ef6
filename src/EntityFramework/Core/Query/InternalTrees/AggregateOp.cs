namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Basic Aggregates
    /// </summary>
    internal sealed class AggregateOp : ScalarOp
    {
        #region private state

        private readonly EdmFunction m_aggFunc;
        private readonly bool m_distinctAgg;

        #endregion

        #region constructors

        internal AggregateOp(EdmFunction aggFunc, bool distinctAgg)
            : base(OpType.Aggregate, aggFunc.ReturnParameter.TypeUsage)
        {
            m_aggFunc = aggFunc;
            m_distinctAgg = distinctAgg;
        }

        private AggregateOp()
            : base(OpType.Aggregate)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly AggregateOp Pattern = new AggregateOp();

        /// <summary>
        /// The Aggregate function's metadata
        /// </summary>
        internal EdmFunction AggFunc
        {
            get { return m_aggFunc; }
        }

        /// <summary>
        /// Is this a "distinct" aggregate
        /// </summary>
        internal bool IsDistinctAggregate
        {
            get { return m_distinctAgg; }
        }

        /// <summary>
        /// Yes; this is an aggregate
        /// </summary>
        internal override bool IsAggregateOp
        {
            get { return true; }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}