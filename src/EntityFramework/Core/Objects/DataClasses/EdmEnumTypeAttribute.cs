namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System;

    /// <summary>
    /// Attribute indicating an enum type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [System.AttributeUsage(AttributeTargets.Enum)]
    public sealed class EdmEnumTypeAttribute : EdmTypeAttribute
    {
        /// <summary>
        /// Initializes a new instance of EdmEnumTypeAttribute class.
        /// </summary>
        public EdmEnumTypeAttribute()
        {
        }
    }
}
