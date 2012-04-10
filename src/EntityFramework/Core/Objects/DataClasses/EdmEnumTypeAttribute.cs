namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Attribute indicating an enum type.
    /// </summary>
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
