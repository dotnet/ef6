// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    internal enum EFElementState
    {
        /// <summary>
        ///     Initialize state
        /// </summary>
        None = 0,

        /// <summary>
        ///     Indicates that Parse was attempted on the model element, but the model element is not Parsed
        /// </summary>
        ParseAttempted,

        /// <summary>
        ///     An item sets itself to this state once it has been parsed from the Source
        /// </summary>
        Parsed,

        /// <summary>
        ///     Indicates that Normalize was attempted on the model element, but the model element is not Normalized.  An item should be re-normalized
        ///     if its normalized name changed during editing.
        /// </summary>
        NormalizeAttempted,

        /// <summary>
        ///     An item sets itself to this state once it has added its normalized name to the symbol table
        /// </summary>
        Normalized,

        /// <summary>
        ///     Indicates that Resolve pass was attempted on the model element, but the model element is not resolved.  This could be because
        ///     the element references an item that does not exist.
        /// </summary>
        ResolveAttempted,

        /// <summary>
        ///     The item's bindings have been resolved across the model
        /// </summary>
        Resolved
    }
}
