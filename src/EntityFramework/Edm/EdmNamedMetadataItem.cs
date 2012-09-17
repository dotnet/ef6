// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     The base for all all Entity Data Model (EDM) item types that with a <see cref="Name" /> property.
    /// </summary>
    public abstract class EdmNamedMetadataItem
        : EdmMetadataItem, INamedDataModelItem
    {
        /// <summary>
        ///     Gets or sets the currently assigned name.
        /// </summary>
        public virtual string Name { get; set; }
    }
}
