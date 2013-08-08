// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// The concurrency mode for properties.
    /// </summary>
    public enum ConcurrencyMode
    {
        /// <summary>
        /// Default concurrency mode: the property is never validated
        /// at write time
        /// </summary>
        None,

        /// <summary>
        /// Fixed concurrency mode: the property is always validated at
        /// write time
        /// </summary>
        Fixed,
    }
}
