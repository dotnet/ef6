// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Xml.Linq;

    /// <summary>
    /// Base class for persisted model cache.
    /// </summary>
    public abstract class DbModelStore
    {
        /// <summary>
        /// Loads a model from the store.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <returns>The loaded metadata model.</returns>
        public abstract DbCompiledModel TryLoad(Type contextType);

        /// <summary>
        /// Retrieves an edmx XDocument version of the model from the store.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <returns>The loaded XDocument edmx.</returns>
        public abstract XDocument TryGetEdmx(Type contextType);

        /// <summary>
        /// Saves a model to the store.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <param name="model">The metadata model to save.</param>
        public abstract void Save(Type contextType, DbModel model);

        /// <summary>
        /// Gets the default database schema used by a model.
        /// </summary>
        /// <param name="contextType">The type of context representing the model.</param>
        /// <returns>The default database schema.</returns>
        protected virtual string GetDefaultSchema(Type contextType)
        {
            return EdmModelExtensions.DefaultSchema;
        }
    }
}
