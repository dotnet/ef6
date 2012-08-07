// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    ///     All relational operators - filter, project, join etc.
    /// </summary>
    internal abstract class RelOp : Op
    {
        #region constructors

        /// <summary>
        ///     Basic constructor.
        /// </summary>
        /// <param name="opType"> kind of Op </param>
        internal RelOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        ///     RelOp
        /// </summary>
        internal override bool IsRelOp
        {
            get { return true; }
        }

        #endregion
    }
}
