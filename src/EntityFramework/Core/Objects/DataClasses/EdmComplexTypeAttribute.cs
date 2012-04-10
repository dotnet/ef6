namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// attribute for complex types
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class)]
    public sealed class EdmComplexTypeAttribute: EdmTypeAttribute
    {
        /// <summary>
        /// attribute for complex types
        /// </summary>
        public EdmComplexTypeAttribute()
        {
        }
    }
}
