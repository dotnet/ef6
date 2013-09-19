// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// Represents all physical operators
    /// </summary>
    internal abstract class PhysicalOp : Op
    {
        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType"> the op type </param>
        internal PhysicalOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// This is a physical Op
        /// </summary>
        internal override bool IsPhysicalOp
        {
            get { return true; }
        }

        #endregion
    }
}
