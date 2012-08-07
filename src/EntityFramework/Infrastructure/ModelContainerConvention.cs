// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     This <see cref="DbModelBuilder" /> convention uses the name of the derived
    ///     <see cref="DbContext" /> class as the container for the conceptual model built by
    ///     Code First.
    /// </summary>
    public class ModelContainerConvention : IEdmConvention
    {
        #region Fields and constructors

        private readonly string _containerName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelContainerConvention" /> class.
        /// </summary>
        /// <param name="containerName"> The model container name. </param>
        internal ModelContainerConvention(string containerName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(containerName));

            _containerName = containerName;
        }

        #endregion

        #region Convention Apply

        /// <summary>
        ///     Applies the convention to the given model.
        /// </summary>
        /// <param name="model"> The model. </param>
        void IEdmConvention.Apply(EdmModel model)
        {
            model.Containers.Single().Name = _containerName;
        }

        #endregion
    }
}
