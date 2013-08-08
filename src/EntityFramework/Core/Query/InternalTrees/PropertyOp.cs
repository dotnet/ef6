// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a property access
    /// </summary>
    internal sealed class PropertyOp : ScalarOp
    {
        #region private state

        private readonly EdmMember m_property;

        #endregion

        #region constructors

        internal PropertyOp(TypeUsage type, EdmMember property)
            : base(OpType.Property, type)
        {
            Debug.Assert(
                (property is EdmProperty) || (property is RelationshipEndMember) || (property is NavigationProperty),
                "Unexpected EdmMember type");
            m_property = property;
        }

        private PropertyOp()
            : base(OpType.Property)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Used for patterns in transformation rules
        /// </summary>
        internal static readonly PropertyOp Pattern = new PropertyOp();

        /// <summary>
        /// 1 child - the instance
        /// </summary>
        internal override int Arity
        {
            get { return 1; }
        }

        /// <summary>
        /// The property metadata
        /// </summary>
        internal EdmMember PropertyInfo
        {
            get { return m_property; }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        /// <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v"> The visitor </param>
        /// <param name="n"> The node in question </param>
        /// <returns> An instance of TResultType </returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
