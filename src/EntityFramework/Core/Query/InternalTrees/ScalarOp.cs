// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// All scalars fall into this category
    /// </summary>
    internal abstract class ScalarOp : Op
    {
        #region private state

        private TypeUsage m_type;

        #endregion

        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType"> kind of Op </param>
        /// <param name="type"> type of value produced by this Op </param>
        internal ScalarOp(OpType opType, TypeUsage type)
            : this(opType)
        {
            DebugCheck.NotNull(type);
            m_type = type;
        }

        protected ScalarOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// ScalarOp
        /// </summary>
        internal override bool IsScalarOp
        {
            get { return true; }
        }

        /// <summary>
        /// Two scalarOps are equivalent (usually) if their OpTypes and types are the
        /// same. Obviously, their arguments need to be equivalent as well - but that's
        /// checked elsewhere
        /// </summary>
        /// <param name="other"> The other Op to compare against </param>
        /// <returns> true, if the Ops are indeed equivalent </returns>
        internal override bool IsEquivalent(Op other)
        {
            return (other.OpType == OpType && TypeSemantics.IsStructurallyEqual(Type, other.Type));
        }

        /// <summary>
        /// Datatype of result
        /// </summary>
        internal override TypeUsage Type
        {
            get { return m_type; }
            set { m_type = value; }
        }

        /// <summary>
        /// Is this an Aggregate
        /// </summary>
        internal virtual bool IsAggregateOp
        {
            get { return false; }
        }

        #endregion
    }
}
