// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// Base class for all Join operations
    /// </summary>
    internal abstract class JoinBaseOp : RelOp
    {
        #region constructors

        internal JoinBaseOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public surface

        /// <summary>
        /// 3 children - left, right, pred
        /// </summary>
        internal override int Arity
        {
            get { return 3; }
        }

        #endregion
    }
}
