// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Abstract base class for tree expressions (unary as in Not, n-ary
    /// as in And or Or). Duplicate elements are trimmed at construction
    /// time (algorithms applied to these trees rely on the assumption
    /// of uniform children).
    /// </summary>
    /// <typeparam name="T_Identifier"> The type of leaf term identifiers in this expression. </typeparam>
    internal abstract class TreeExpr<T_Identifier> : BoolExpr<T_Identifier>
    {
        private readonly Set<BoolExpr<T_Identifier>> _children;
        private readonly int _hashCode;

        /// <summary>
        /// Initialize a new tree expression with the given children.
        /// </summary>
        /// <param name="children"> Child expressions </param>
        protected TreeExpr(IEnumerable<BoolExpr<T_Identifier>> children)
        {
            DebugCheck.NotNull(children);
            _children = new Set<BoolExpr<T_Identifier>>(children);
            _children.MakeReadOnly();
            _hashCode = _children.GetElementsHashCode();
        }

        /// <summary>
        /// Gets the children of this expression node.
        /// </summary>
        internal Set<BoolExpr<T_Identifier>> Children
        {
            get { return _children; }
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("use only typed Equals");
            return base.Equals(obj as BoolExpr<T_Identifier>);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant("{0}({1})", ExprType, _children);
        }

        protected override bool EquivalentTypeEquals(BoolExpr<T_Identifier> other)
        {
            return ((TreeExpr<T_Identifier>)other).Children.SetEquals(Children);
        }
    }
}
