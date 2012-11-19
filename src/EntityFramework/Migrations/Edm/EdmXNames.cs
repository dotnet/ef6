// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    internal static class EdmXNames
    {
        private static readonly XNamespace _csdlNamespaceV2
            = XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/edm");

        private static readonly XNamespace _mslNamespaceV2
            = XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/mapping/cs");

        private static readonly XNamespace _ssdlNamespaceV2
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/02/edm/ssdl");

        private static readonly XNamespace _csdlNamespaceV3
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm");

        private static readonly XNamespace _mslNamespaceV3
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs");

        private static readonly XNamespace _ssdlNamespaceV3
            = XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl");

        private static readonly XNamespace _systemNamespace
            = XNamespace.Get("http://schemas.microsoft.com/ado/2012/10/edm/migrations");

        public static readonly XName IsSystemName = _systemNamespace + "IsSystem";

        public static bool IsSystem(this XElement element)
        {
            DebugCheck.NotNull(element);

            return string.Equals("true", (string)element.Attribute(IsSystemName), StringComparison.Ordinal);
        }

        public static string ActionAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Action");
        }

        public static string ColumnNameAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("ColumnName");
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string EntitySetAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("EntitySet");
        }

        public static string NameAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Name");
        }

        public static string EntityTypeAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("EntityType");
        }

        public static string NullableAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Nullable");
        }

        public static string MaxLengthAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("MaxLength");
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string FixedLengthAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("FixedLength");
        }

        public static string PrecisionAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Precision");
        }

        public static string ProviderAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Provider");
        }

        public static string ProviderManifestTokenAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("ProviderManifestToken");
        }

        public static string ScaleAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Scale");
        }

        public static string StoreGeneratedPatternAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("StoreGeneratedPattern");
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string UnicodeAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Unicode");
        }

        public static string RoleAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Role");
        }

        public static string SchemaAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Schema");
        }

        public static string StoreEntitySetAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("StoreEntitySet");
        }

        public static string TableAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Table");
        }

        public static string TypeAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Type");
        }

        public static string TypeNameAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("TypeName");
        }

        public static string ValueAttribute(this XElement element)
        {
            DebugCheck.NotNull(element);

            return (string)element.Attribute("Value");
        }

        public static class Csdl
        {
            public static readonly IEnumerable<XName> AssociationNames = Names("Association");
            public static readonly IEnumerable<XName> ComplexTypeNames = Names("ComplexType");
            public static readonly IEnumerable<XName> EndNames = Names("End");
            public static readonly IEnumerable<XName> EntityContainerNames = Names("EntityContainer");
            public static readonly IEnumerable<XName> EntitySetNames = Names("EntitySet");
            public static readonly IEnumerable<XName> EntityTypeNames = Names("EntityType");
            public static readonly IEnumerable<XName> PropertyNames = Names("Property");
            public static readonly IEnumerable<XName> SchemaNames = Names("Schema");

            private static IEnumerable<XName> Names(string elementName)
            {
                DebugCheck.NotEmpty(elementName);

                return new List<XName>
                           {
                               _csdlNamespaceV3 + elementName,
                               _csdlNamespaceV2 + elementName
                           };
            }
        }

        public static class Msl
        {
            public static readonly IEnumerable<XName> AssociationSetMappingNames = Names("AssociationSetMapping");
            public static readonly IEnumerable<XName> ComplexPropertyNames = Names("ComplexProperty");
            public static readonly IEnumerable<XName> ConditionNames = Names("Condition");
            public static readonly IEnumerable<XName> EntityContainerMappingNames = Names("EntityContainerMapping");
            public static readonly IEnumerable<XName> EntitySetMappingNames = Names("EntitySetMapping");
            public static readonly IEnumerable<XName> EntityTypeMappingNames = Names("EntityTypeMapping");
            public static readonly IEnumerable<XName> MappingNames = Names("Mapping");
            public static readonly IEnumerable<XName> MappingFragmentNames = Names("MappingFragment");
            public static readonly IEnumerable<XName> ScalarPropertyNames = Names("ScalarProperty");

            private static IEnumerable<XName> Names(string elementName)
            {
                DebugCheck.NotEmpty(elementName);

                return new List<XName>
                           {
                               _mslNamespaceV3 + elementName,
                               _mslNamespaceV2 + elementName
                           };
            }
        }

        public static class Ssdl
        {
            public static readonly IEnumerable<XName> AssociationNames = Names("Association");
            public static readonly IEnumerable<XName> DependentNames = Names("Dependent");
            public static readonly IEnumerable<XName> EndNames = Names("End");
            public static readonly IEnumerable<XName> EntityContainerNames = Names("EntityContainer");
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
                DebugCheck.NotEmpty(elementName);

                return new List<XName>
                           {
                               _ssdlNamespaceV3 + elementName,
                               _ssdlNamespaceV2 + elementName
                           };
            }
        }
    }
}
