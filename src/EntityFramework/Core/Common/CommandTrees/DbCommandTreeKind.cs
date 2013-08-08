// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    /// <summary>
    /// Describes the different "kinds" (classes) of command trees.
    /// </summary>
    public enum DbCommandTreeKind
    {
        /// <summary>
        /// A query to retrieve data
        /// </summary>
        Query,

        /// <summary>
        /// Update existing data
        /// </summary>
        Update,

        /// <summary>
        /// Insert new data
        /// </summary>
        Insert,

        /// <summary>
        /// Deleted existing data
        /// </summary>
        Delete,

        /// <summary>
        /// Call a function
        /// </summary>
        Function,
    }
}
