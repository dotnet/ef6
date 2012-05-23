namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Data.Entity.Core.Objects.Internal;
    using System.Linq.Expressions;

    /// <summary>
    /// Type returned by the Translator visitor; allows us to put the logic
    /// to ensure a specific return type in a single place, instead of in 
    /// each Visit method.
    /// </summary>
    internal class TranslatorResult
    {
        private readonly Expression ReturnedExpression;
        private readonly Type RequestedType;

        internal TranslatorResult(Expression returnedExpression, Type requestedType)
        {
            RequestedType = requestedType;
            ReturnedExpression = returnedExpression;
        }

        /// <summary>
        /// Return the expression; wrapped with the appropriate cast/convert
        /// logic to guarantee it's type.
        /// </summary>
        internal Expression Expression
        {
            get
            {
                var result = Translator.Emit_EnsureType(ReturnedExpression, RequestedType);
                return result;
            }
        }

        /// <summary>
        /// Return the expression without attempting to cast/convert to the requested type.
        /// </summary>
        internal Expression UnconvertedExpression
        {
            get { return ReturnedExpression; }
        }

        /// <summary>
        /// Checks if the expression represents an wrapped entity and if so creates an expression
        /// that extracts the raw entity from the wrapper.
        /// </summary>
        internal Expression UnwrappedExpression
        {
            get
            {
                if (!typeof(IEntityWrapper).IsAssignableFrom(ReturnedExpression.Type))
                {
                    return ReturnedExpression;
                }
                return Translator.Emit_UnwrapAndEnsureType(ReturnedExpression, RequestedType);
            }
        }
    }
}
