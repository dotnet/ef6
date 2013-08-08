// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// The pattern used to generate values for a property in the database.
    /// </summary>
    public enum DatabaseGeneratedOption
    {
        /// <summary>
        /// The database does not generate values.
        /// </summary>
        None,

        /// <summary>
        /// The database generates a value when a row is inserted.
        /// </summary>
        Identity,

        /// <summary>
        /// The database generates a value when a row is inserted or updated.
        /// </summary>
        Computed
    }
}

#endif
