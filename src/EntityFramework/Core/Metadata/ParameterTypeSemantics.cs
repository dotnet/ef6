namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// The enumeration defining the type semantics used to resolve function overloads. 
    /// These flags are defined in the provider manifest per function definition.
    /// </summary>
    public enum ParameterTypeSemantics
    {
        /// <summary>
        /// Allow Implicit Conversion between given and formal argument types (default).
        /// </summary>
        AllowImplicitConversion = 0,

        /// <summary>
        /// Allow Type Promotion between given and formal argument types.
        /// </summary>
        AllowImplicitPromotion = 1,

        /// <summary>
        /// Use strict Equivalence only.
        /// </summary>
        ExactMatchOnly = 2
    }
}