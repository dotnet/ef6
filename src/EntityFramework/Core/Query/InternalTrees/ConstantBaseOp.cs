// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Base class for all constant Ops
    /// </summary>
    internal abstract class ConstantBaseOp : ScalarOp
    {
        #region private state

        private readonly object m_value;

        #endregion

        #region constructors

        protected ConstantBaseOp(OpType opType, TypeUsage type, object value)
            : base(opType, type)
        {
            m_value = value;
        }

        /// <summary>
        ///     Constructor overload for rules
        /// </summary>
        protected ConstantBaseOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public properties and methods

        /// <summary>
        ///     Get the constant value
        /// </summary>
        internal virtual Object Value
        {
            get { return m_value; }
        }

        /// <summary>
        ///     0 children
        /// </summary>
        internal override int Arity
        {
            get { return 0; }
        }

        /// <summary>
        ///     Two CostantBaseOps are equivalent if they are of the same
        ///     derived type and have the same type and value.
        /// </summary>
        /// <param name="other"> the other Op </param>
        /// <returns> true, if these are equivalent (not a strict equality test) </returns>
        internal override bool IsEquivalent(Op other)
        {
            var otherConstant = other as ConstantBaseOp;
            return
                otherConstant != null &&
                OpType == other.OpType &&
                otherConstant.Type.EdmEquals(Type) &&
                ((otherConstant.Value == null && Value == null) || otherConstant.Value.Equals(Value));
        }

        #endregion
    }
}
