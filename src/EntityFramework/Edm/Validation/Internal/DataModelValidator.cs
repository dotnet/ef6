namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Validation.Internal.EdmModel;

    /// <summary>
    ///     Data Model Validator
    /// </summary>
    internal class DataModelValidator
    {
        public event EventHandler<DataModelErrorEventArgs> OnError;

        /// <summary>
        ///     Validate the <see cref = "EdmModel" /> and all of its properties given certain version.
        /// </summary>
        /// <param name = "root"> The root of the model to be validated </param>
        /// <param name = "validateSyntax"> True to validate the syntax, otherwise false </param>
        internal void Validate(Edm.EdmModel root, bool validateSyntax)
        {
            // Build up the validation context
            var context = new EdmModelValidationContext(validateSyntax);
            context.OnError += OnError;
            context.Validate(root);
        }
    }
}