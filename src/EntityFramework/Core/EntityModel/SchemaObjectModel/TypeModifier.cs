namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Return value from StructuredProperty RemoveTypeModifier
    /// </summary>
    internal enum TypeModifier
    {
        /// <summary>Type string has no modifier</summary>
        None,

        /// <summary>Type string was of form Array(...)</summary>
        Array,

        /// <summary>Type string was of form Set(...)</summary>
        Set,

        /// <summary>Type string was of form Table(...)</summary>
        Table,
    }
}
