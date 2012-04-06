using System;

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Attribute identifying the Edm base class
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
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
