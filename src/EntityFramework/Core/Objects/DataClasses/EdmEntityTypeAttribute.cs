using System;

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Attribute identifying the Edm base class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EdmEntityTypeAttribute : EdmTypeAttribute
    {
        /// <summary>
        /// Attribute identifying the Edm base class
        /// </summary>
        public EdmEntityTypeAttribute()
        {
        }
    }
}
