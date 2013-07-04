// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     This <see cref="DbModelBuilder" /> convention uses the name of the derived
    ///     <see cref="DbContext" /> class as the container for the conceptual model built by
    ///     Code First.
    /// </summary>
    public class ModelContainerConvention : IModelConvention<EntityContainer>
    {
        #region Fields and constructors

        private readonly string _containerName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelContainerConvention" /> class.
        /// </summary>
        /// <param name="containerName"> The model container name. </param>
        internal ModelContainerConvention(string containerName)
        {
            DebugCheck.NotEmpty(containerName);

            _containerName = containerName;
        }

        #endregion

        #region Convention Apply

        /// <summary>
        ///     Applies the convention to the given model.
        /// </summary>
        /// <param name="model"> The model. </param>
        public virtual void Apply(EntityContainer item, DbModel model)
        {
            Check.NotNull(model, "model");

            item.Name = _containerName;
        }

        #endregion
    }
}
