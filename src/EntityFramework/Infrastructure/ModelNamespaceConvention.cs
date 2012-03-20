namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     This <see cref = "DbModelBuilder" /> convention uses the namespace of the derived
    ///     <see cref = "DbContext" /> class as the namespace of the conceptual model built by
    ///     Code First.
    /// </summary>
    public class ModelNamespaceConvention : IEdmConvention
    {
        #region Fields and constructors

        private readonly string _modelNamespace;

        /// <summary>
        ///     Initializes a new instance of the <see cref = "ModelNamespaceConvention" /> class.
        /// </summary>
        /// <param name = "modelNamespace">The model namespace.</param>
        internal ModelNamespaceConvention(string modelNamespace)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(modelNamespace));

            _modelNamespace = modelNamespace;
        }

        #endregion

        #region Convention Apply

        /// <summary>
        ///     Applies the convention to the given model.
        /// </summary>
        /// <param name = "model">The model.</param>
        void IEdmConvention.Apply(EdmModel model)
        {
            model.Namespaces.Single().Name = _modelNamespace;
        }

        #endregion
    }
}