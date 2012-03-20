namespace System.Data.Entity.Migrations.Edm
{
    using System.Diagnostics.Contracts;
    using System.Xml.Linq;
    using System.Collections.Generic;

    internal static class EdmXNames
    {
        private static readonly XNamespace CsdlNamespaceV2
            = XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/edm");

        private static readonly XNamespace MslNamespaceV2
            = XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/mapping/cs");

        private static readonly XNamespace SsdlNamespaceV2
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/02/edm/ssdl");

        private static readonly XNamespace CsdlNamespaceV3
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm");

        private static readonly XNamespace MslNamespaceV3
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs");

        private static readonly XNamespace SsdlNamespaceV3
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl");

        public static string ActionAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Action");
        }

        public static string ColumnNameAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("ColumnName");
        }

        public static string EntitySetAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("EntitySet");
        }

        public static string NameAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Name");
        }

        public static string EntityTypeAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("EntityType");
        }

        public static string NullableAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Nullable");
        }

        public static string MaxLengthAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("MaxLength");
        }

        public static string FixedLengthAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("FixedLength");
        }

        public static string PrecisionAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Precision");
        }

        public static string ProviderAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Provider");
        }

        public static string ProviderManifestTokenAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("ProviderManifestToken");
        }

        public static string ScaleAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Scale");
        }

        public static string StoreGeneratedPatternAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("StoreGeneratedPattern");
        }

        public static string UnicodeAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Unicode");
        }

        public static string RoleAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Role");
        }

        public static string SchemaAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Schema");
        }

        public static string StoreEntitySetAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("StoreEntitySet");
        }

        public static string TableAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Table");
        }

        public static string TypeAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Type");
        }

        public static string TypeNameAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("TypeName");
        }

        public static string ValueAttribute(this XElement element)
        {
            Contract.Requires(element != null);

            return (string)element.Attribute("Value");
        }

        public static class Csdl
        {
            public static readonly IEnumerable<XName> AssociationNames = Names("Association");
            public static readonly IEnumerable<XName> ComplexTypeNames = Names("ComplexType");
            public static readonly IEnumerable<XName> EndNames = Names("End");
            public static readonly IEnumerable<XName> EntityTypeNames = Names("EntityType");
            public static readonly IEnumerable<XName> PropertyNames = Names("Property");
            public static readonly IEnumerable<XName> SchemaNames = Names("Schema");

            private static IEnumerable<XName> Names(string elementName)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(elementName));

                return new List<XName> { CsdlNamespaceV3 + elementName, CsdlNamespaceV2 + elementName };
            }
        }

        public static class Msl
        {
            public static readonly IEnumerable<XName> AssociationSetMappingNames = Names("AssociationSetMapping");
            public static readonly IEnumerable<XName> ComplexPropertyNames = Names("ComplexProperty");
            public static readonly IEnumerable<XName> ConditionNames = Names("Condition");
            public static readonly IEnumerable<XName> EntityTypeMappingNames = Names("EntityTypeMapping");
            public static readonly IEnumerable<XName> MappingNames = Names("Mapping");
            public static readonly IEnumerable<XName> MappingFragmentNames = Names("MappingFragment");
            public static readonly IEnumerable<XName> ScalarPropertyNames = Names("ScalarProperty");

            private static IEnumerable<XName> Names(string elementName)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(elementName));

                return new List<XName> { MslNamespaceV3 + elementName, MslNamespaceV2 + elementName };
            }
        }

        public static class Ssdl
        {
            public static readonly IEnumerable<XName> AssociationNames = Names("Association");
            public static readonly IEnumerable<XName> DependentNames = Names("Dependent");
            public static readonly IEnumerable<XName> EndNames = Names("End");
            public static readonly IEnumerable<XName> EntitySetNames = Names("EntitySet");
            public static readonly IEnumerable<XName> EntityTypeNames = Names("EntityType");
            public static readonly IEnumerable<XName> KeyNames = Names("Key");
            public static readonly IEnumerable<XName> OnDeleteNames = Names("OnDelete");
            public static readonly IEnumerable<XName> PrincipalNames = Names("Principal");
            public static readonly IEnumerable<XName> PropertyNames = Names("Property");
            public static readonly IEnumerable<XName> PropertyRefNames = Names("PropertyRef");
            public static readonly IEnumerable<XName> SchemaNames = Names("Schema");

            private static IEnumerable<XName> Names(string elementName)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(elementName));

                return new List<XName> { SsdlNamespaceV3 + elementName, SsdlNamespaceV2 + elementName };
            }
        }
    }
}