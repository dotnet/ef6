// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    /// AncillaryOp
    /// </summary>
    internal abstract class AncillaryOp : Op
    {
        #region constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType"> kind of Op </param>
        internal AncillaryOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// AncillaryOp
        /// </summary>
        internal override bool IsAncillaryOp
        {
            get { return true; }
        }

        #endregion
    }
}
