// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     This <see cref="DbModelBuilder" /> convention uses the namespace of the derived
    ///     <see cref="DbContext" /> class as the namespace of the conceptual model built by
    ///     Code First.
    /// </summary>
    public class ModelNamespaceConvention : IConfigurationConvention
    {
        private readonly string _modelNamespace;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelNamespaceConvention" /> class.
        /// </summary>
        /// <param name="modelNamespace"> The model namespace. </param>
        internal ModelNamespaceConvention(string modelNamespace)
        {
            DebugCheck.NotEmpty(modelNamespace);

            _modelNamespace = modelNamespace;
        }

        /// <summary>
        ///     Applies the convention to the given model.
        /// </summary>
        /// <param name="model"> The model. </param>
        public void Apply(ModelConfiguration modelConfiguration)
        {
            modelConfiguration.ModelNamespace = _modelNamespace;
        }
    }
}
