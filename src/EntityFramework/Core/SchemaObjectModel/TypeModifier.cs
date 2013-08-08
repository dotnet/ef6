// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    /// <summary>
    /// Return value from StructuredProperty RemoveTypeModifier
    /// </summary>
    internal enum TypeModifier
    {
        /// <summary>
        /// Type string has no modifier
        /// </summary>
        None,

        /// <summary>
        /// Type string was of form Array(...)
        /// </summary>
        Array,

        /// <summary>
        /// Type string was of form Set(...)
        /// </summary>
        Set,

        /// <summary>
        /// Type string was of form Table(...)
        /// </summary>
        Table,
    }
}
