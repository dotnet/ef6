// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    ///     The pattern for Server Generated Properties.
    /// </summary>
    public enum StoreGeneratedPattern
    {
        /// <summary>
        ///     Not a Server Generated Property. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        ///     A value is generated on INSERT, and remains unchanged on update.
        /// </summary>
        Identity = 1,

        /// <summary>
        ///     A value is generated on both INSERT and UPDATE.
        /// </summary>
        Computed = 2,
    }
}
