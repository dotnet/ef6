namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal static class EdmStructuralMemberExtensions
    {
        public static PropertyInfo GetClrPropertyInfo(this EdmStructuralMember property)
        {
            Contract.Requires(property != null);

            return property.Annotations.GetClrPropertyInfo();
        }

        public static void SetClrPropertyInfo(this EdmStructuralMember property, PropertyInfo propertyInfo)
        {
            Contract.Requires(property != null);

            property.Annotations.SetClrPropertyInfo(propertyInfo);
        }

        public static IEnumerable<T> GetClrAttributes<T>(this EdmStructuralMember property) where T : Attribute
        {
            Contract.Requires(property != null);

            var clrAttributes = property.Annotations.GetClrAttributes();
            return clrAttributes != null ? clrAttributes.OfType<T>() : Enumerable.Empty<T>();
        }
    }
}