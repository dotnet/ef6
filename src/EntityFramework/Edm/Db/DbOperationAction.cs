// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    ///     Specifies the action to take on a given operation.
    /// </summary>
    public enum DbOperationAction
    {
        /// <summary>
        ///     Default behavior
        /// </summary>
        None = 0,

        /// <summary>
        ///     Restrict the operation
        /// </summary>
        Restrict = 1,

        /// <summary>
        ///     Cascade the operation
        /// </summary>
        Cascade = 2,
    }
}
