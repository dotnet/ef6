// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    /// The different ways that new objects loaded from the database can be merged with existing objects already in memory.
    /// </summary>
    public enum MergeOption
    {
        /// <summary>
        ///     Will only append new (top level-unique) rows.  This is the default behavior.
        /// </summary>
        AppendOnly = 0,

        /// <summary>
        ///     Same behavior as LoadOption.OverwriteChanges.
        /// </summary>
        OverwriteChanges = LoadOption.OverwriteChanges,

        /// <summary>
        ///     Same behavior as LoadOption.PreserveChanges.
        /// </summary>
        PreserveChanges = LoadOption.PreserveChanges,

        /// <summary>
        ///     Will not modify cache.
        /// </summary>
        NoTracking = 3,
    }
}
