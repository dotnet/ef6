// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;

    /// <summary>
    /// A convention that operates on the conceptual section of the model after the model is created.
    /// </summary>
    /// <typeparam name="T">The type of metadata item that this convention operates on.</typeparam>
    public interface IConceptualModelConvention<T> : IConvention
        where T : MetadataItem
    {
        /// <summary>
        /// Applies this convention to an item in the model.
        /// </summary>
        /// <param name="item">The item to apply the convention to.</param>
        /// <param name="model">The model.</param>
        void Apply(T item, DbModel model);
    }
}
