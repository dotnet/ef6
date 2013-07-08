// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;

    /// <summary>
    /// An interface to get the underlying store and conceptual model for a <see cref="DbModel"/>.
    /// </summary>
    public interface IEdmModelAdapter
    {
        /// <summary>
        /// Gets the conceptual model.
        /// </summary>
        EdmModel ConceptualModel { get; }

        /// <summary>
        /// Gets the store model.
        /// </summary>
        EdmModel StoreModel { get; }
    }
}
