// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// Base class for all Apply Ops
    /// </summary>
    internal abstract class ApplyBaseOp : RelOp
    {
        #region constructors

        internal ApplyBaseOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public surface

        /// <summary>
        /// 2 children - left, right
        /// </summary>
        internal override int Arity
        {
            get { return 2; }
        }

        #endregion
    }
}
