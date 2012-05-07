namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Navigate a relationship, and get the reference(s) of the target end
    /// </summary>
    internal sealed class NavigateOp : ScalarOp
    {
        #region private state

        private readonly RelProperty m_property;

        #endregion

        #region constructors

        internal NavigateOp(TypeUsage type, RelProperty relProperty)
            : base(OpType.Navigate, type)
        {
            m_property = relProperty;
        }

        private NavigateOp()
            : base(OpType.Navigate)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NavigateOp Pattern = new NavigateOp();

        /// <summary>
        /// 1 child - entity instance
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        /// The rel property that describes this nvaigation
        /// </summary>
        internal RelProperty RelProperty
        {
            get { return m_property; }
        }

        /// <summary>
        /// The relationship we're traversing
        /// </summary>
        internal RelationshipType Relationship
        {
            get { return m_property.Relationship; }
        }

        /// <summary>
        /// The starting point of the traversal
        /// </summary>
        internal RelationshipEndMember FromEnd
        {
            get { return m_property.FromEnd; }
        }

        /// <summary>
        /// The end-point of the traversal
        /// </summary>
        internal RelationshipEndMember ToEnd
        {
            get { return m_property.ToEnd; }
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
