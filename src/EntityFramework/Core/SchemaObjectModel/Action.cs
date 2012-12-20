// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    /// <summary>
    ///     Valid actions in an On&lt;Operation&gt; element
    /// </summary>
    internal enum Action
    {
        /// <summary>
        ///     no action
        /// </summary>
        None,

        /// <summary>
        ///     Cascade to other ends
        /// </summary>
        Cascade,
    }
}
