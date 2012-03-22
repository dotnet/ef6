namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class EdmComplexTypeExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmProperty AddComplexProperty(
            this EdmComplexType complexType, string name, EdmComplexType targetComplexType)
        {
            Contract.Requires(complexType != null);
            Contract.Requires(complexType.Properties != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(targetComplexType != null);

            var property = new EdmProperty
                               {
                                   Name = name
                               }.AsComplex(targetComplexType);

            complexType.DeclaredProperties.Add(property);

            return property;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static object GetConfiguration(this EdmComplexType complexType)
        {
            Contract.Requires(complexType != null);

            return complexType.Annotations.GetConfiguration();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static void SetConfiguration(this EdmComplexType complexType, object configuration)
        {
            Contract.Requires(complexType != null);

            complexType.Annotations.SetConfiguration(configuration);
        }

        public static Type GetClrType(this EdmComplexType complexType)
        {
            Contract.Requires(complexType != null);

            return complexType.Annotations.GetClrType();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static void SetClrType(this EdmComplexType complexType, Type type)
        {
            Contract.Requires(complexType != null);
            Contract.Requires(type != null);

            complexType.Annotations.SetClrType(type);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmProperty AddPrimitiveProperty(this EdmComplexType complexType, string name)
        {
            Contract.Requires(complexType != null);
            Contract.Requires(complexType.Properties != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var property = new EdmProperty().AsPrimitive();
            property.Name = name;

            complexType.DeclaredProperties.Add(property);

            return property;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmProperty GetPrimitiveProperty(this EdmComplexType complexType, string name)
        {
            Contract.Requires(complexType != null);
            Contract.Requires(complexType.Properties != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            return complexType.Properties.SingleOrDefault(p => p.Name == name);
        }
    }
}
