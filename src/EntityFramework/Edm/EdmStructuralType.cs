namespace System.Data.Entity.Edm
{
    /// <summary>
    ///     The base for all all Entity Data Model (EDM) types that represent a structured type from the EDM type system.
    /// </summary>
    internal abstract class EdmStructuralType
        : EdmDataModelType
    {
        public abstract EdmStructuralTypeMemberCollection Members { get; }
    }
}
