namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using EdmModel = System.Data.Entity.Edm.EdmModel;

    /// <summary>
    ///     Edm Model Validator
    /// </summary>
    internal static class EdmModelValidator
    {
        /// <summary>
        ///     validate the <see cref = "EdmModel" /> from the root with the context
        /// </summary>
        /// <param name = "validateRoot"> The root to validate from </param>
        /// <param name = "context"> The validation context </param>
        internal static void Validate(EdmModel validateRoot, EdmModelValidationContext context)
        {
            // build up the rule set and the visitor
            var edmModelValidationRuleSet = EdmModelRuleSet.CreateEdmModelRuleSet(context.ValidationContextVersion, context.ValidateSyntax);

            var modelVisitor = new EdmModelValidationVisitor(context, edmModelValidationRuleSet);

            modelVisitor.Visit(validateRoot);
        }
    }
}