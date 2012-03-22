namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     The context for DataModel Validation
    /// </summary>
    internal abstract class DataModelValidationContext
    {
        internal bool ValidateSyntax { get; set; }
        internal double ValidationContextVersion { get; set; }

        internal abstract void AddError(DataModelItem item, string propertyName, string errorMessage, int errorCode);

        internal abstract void AddWarning(DataModelItem item, string propertyName, string errorMessage, int errorCode);
    }
}
