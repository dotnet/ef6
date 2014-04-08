// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    // <summary>
    //     Translate System.Version to XNamespaces used by the Entity Framework.
    // </summary>
    internal static class SchemaManager
    {
        private static readonly IDictionary<Version, XNamespace> CsdlNamespaces;
        private static readonly IDictionary<Version, XNamespace> MslNamespaces;
        private static readonly IDictionary<Version, XNamespace> SsdlNamespaces;
        private static readonly IDictionary<Version, XNamespace> EdmxNamespaces;
        private static readonly IDictionary<XNamespace, Version> NamespaceToVersionReverseLookUp;

        private static readonly string[] CsdlNamespaceNames;
        private static readonly string[] SsdlNamespaceNames;
        private static readonly string[] MslNamespaceNames;
        private static readonly string[] EdmxNamespaceNames;

        public const string EntityStoreSchemaGeneratorNamespace =
            "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";

        public const string AnnotationNamespace = "http://schemas.microsoft.com/ado/2009/02/edm/annotation";
        public const string CodeGenerationNamespace = "http://schemas.microsoft.com/ado/2006/04/codegeneration";
        public const string ProviderManifestNamespace = "http://schemas.microsoft.com/ado/2006/04/edm/providermanifest";

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static SchemaManager()
        {
            const short arraySize = 3;

            CsdlNamespaces = new Dictionary<Version, XNamespace>(arraySize);
            CsdlNamespaces.Add(EntityFrameworkVersion.Version1, XNamespace.Get("http://schemas.microsoft.com/ado/2006/04/edm"));
            CsdlNamespaces.Add(EntityFrameworkVersion.Version2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/edm"));
            CsdlNamespaces.Add(EntityFrameworkVersion.Version3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm"));
            CsdlNamespaceNames = CsdlNamespaces.Select(n => n.Value.NamespaceName).ToArray();

            MslNamespaces = new Dictionary<Version, XNamespace>(arraySize);
            MslNamespaces.Add(EntityFrameworkVersion.Version1, XNamespace.Get("urn:schemas-microsoft-com:windows:storage:mapping:CS"));
            MslNamespaces.Add(EntityFrameworkVersion.Version2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/mapping/cs"));
            MslNamespaces.Add(EntityFrameworkVersion.Version3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/mapping/cs"));
            MslNamespaceNames = MslNamespaces.Select(n => n.Value.NamespaceName).ToArray();

            SsdlNamespaces = new Dictionary<Version, XNamespace>(arraySize);
            SsdlNamespaces.Add(EntityFrameworkVersion.Version1, XNamespace.Get("http://schemas.microsoft.com/ado/2006/04/edm/ssdl"));
            SsdlNamespaces.Add(EntityFrameworkVersion.Version2, XNamespace.Get("http://schemas.microsoft.com/ado/2009/02/edm/ssdl"));
            SsdlNamespaces.Add(EntityFrameworkVersion.Version3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edm/ssdl"));
            SsdlNamespaceNames = SsdlNamespaces.Select(n => n.Value.NamespaceName).ToArray();

            EdmxNamespaces = new Dictionary<Version, XNamespace>(arraySize);
            EdmxNamespaces.Add(EntityFrameworkVersion.Version1, XNamespace.Get("http://schemas.microsoft.com/ado/2007/06/edmx"));
            EdmxNamespaces.Add(EntityFrameworkVersion.Version2, XNamespace.Get("http://schemas.microsoft.com/ado/2008/10/edmx"));
            EdmxNamespaces.Add(EntityFrameworkVersion.Version3, XNamespace.Get("http://schemas.microsoft.com/ado/2009/11/edmx"));
            EdmxNamespaceNames = EdmxNamespaces.Select(n => n.Value.NamespaceName).ToArray();

            NamespaceToVersionReverseLookUp = new Dictionary<XNamespace, Version>();
            foreach (var kvp in CsdlNamespaces.Concat(MslNamespaces).Concat(SsdlNamespaces).Concat(EdmxNamespaces))
            {
                NamespaceToVersionReverseLookUp.Add(kvp.Value, kvp.Key);
            }
        }

        // <summary>
        //     Get CSDL namespace name for a schema version
        // </summary>
        internal static string GetCSDLNamespaceName(Version schemaVersion)
        {
            Debug.Assert(schemaVersion != null, "schemaVersion != null");

            return GetNamespaceName(schemaVersion, CsdlNamespaces);
        }

        // <summary>
        //     Return all CSDL Namespace names
        // </summary>
        internal static string[] GetCSDLNamespaceNames()
        {
            return CsdlNamespaceNames;
        }

        // <summary>
        //     Return MSL namespace name for a schema version
        // </summary>
        internal static string GetMSLNamespaceName(Version schemaVersion)
        {
            Debug.Assert(schemaVersion != null, "schemaVersion != null");

            return GetNamespaceName(schemaVersion, MslNamespaces);
        }

        // <summary>
        //     Return all MSL Namespace names
        // </summary>
        internal static string[] GetMSLNamespaceNames()
        {
            return MslNamespaceNames;
        }

        // <summary>
        //     Return SSDL namespace name for a schema version
        // </summary>
        internal static string GetSSDLNamespaceName(Version schemaVersion)
        {
            Debug.Assert(schemaVersion != null, "schemaVersion != null");

            return GetNamespaceName(schemaVersion, SsdlNamespaces);
        }

        // <summary>
        //     Return all SSDL Namespace names
        // </summary>
        internal static string[] GetSSDLNamespaceNames()
        {
            return SsdlNamespaceNames;
        }

        // <summary>
        //     Return EDMX namespace name for a schema version
        // </summary>
        internal static string GetEDMXNamespaceName(Version schemaVersion)
        {
            Debug.Assert(schemaVersion != null, "schemaVersion != null");

            return GetNamespaceName(schemaVersion, EdmxNamespaces);
        }

        // <summary>
        //     Return all EDMX Namespace names
        // </summary>
        internal static string[] GetEDMXNamespaceNames()
        {
            return EdmxNamespaceNames;
        }

        // <summary>
        //     Return Entity Store namespace name for a schema version
        // </summary>
        internal static string GetEntityStoreSchemaGeneratorNamespaceName()
        {
            return EntityStoreSchemaGeneratorNamespace;
        }

        // <summary>
        //     Return Code generation namespace name for a schema version
        // </summary>
        internal static string GetCodeGenerationNamespaceName()
        {
            return CodeGenerationNamespace;
        }

        // <summary>
        //     Return Annotation namespace name for a schema version
        // </summary>
        internal static string GetAnnotationNamespaceName()
        {
            return AnnotationNamespace;
        }

        // <summary>
        //     Return Provider namespace name for a schema version
        // </summary>
        internal static string GetProviderManifestNamespaceName()
        {
            return ProviderManifestNamespace;
        }

        // <summary>
        //     Get All namespaces used for the specific version EDMX files
        // </summary>
        internal static string[] GetAllNamespacesForVersion(Version schemaVersion)
        {
            return new[]
                {
                    GetEDMXNamespaceName(schemaVersion),
                    GetCSDLNamespaceName(schemaVersion),
                    GetSSDLNamespaceName(schemaVersion),
                    GetProviderManifestNamespaceName(),
                    GetCodeGenerationNamespaceName(),
                    GetMSLNamespaceName(schemaVersion),
                    GetAnnotationNamespaceName()
                };
        }

        // <summary>
        //     Given a namespace, determine the schema version
        // </summary>
        internal static Version GetSchemaVersion(XNamespace xNamespace)
        {
            // TODO: Returing V1 if the namespace not found feels wrong. Investigate where it is used and return null (or throw?)
            // Note that throwing from this method can crash VS if the exception is not handled.

            Version schemaVersion;
            return xNamespace != null && NamespaceToVersionReverseLookUp.TryGetValue(xNamespace, out schemaVersion)
                       ? schemaVersion
                       : EntityFrameworkVersion.Version1;
        }

        private static string GetNamespaceName(Version schemaVersion, IDictionary<Version, XNamespace> xNamespaces)
        {
            Debug.Assert(schemaVersion != null, "schemaVersion != null");
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version.");
            Debug.Assert(xNamespaces != null, "xNamespaces != null");
            Debug.Assert(xNamespaces.ContainsKey(schemaVersion), "The requested namespace is not found");

            return xNamespaces[schemaVersion].NamespaceName;
        }

        internal static XmlNamespaceManager GetEdmxNamespaceManager(XmlNameTable xmlNameTable, Version schemaVersion)
        {
            Debug.Assert(xmlNameTable != null, "xmlNameTable != null");
            Debug.Assert(schemaVersion != null, "schemaVersion != null");

            var nsMgr = new XmlNamespaceManager(xmlNameTable);
            nsMgr.AddNamespace("edmx", GetEDMXNamespaceName(schemaVersion));
            nsMgr.AddNamespace("csdl", GetCSDLNamespaceName(schemaVersion));
            nsMgr.AddNamespace("essg", GetEntityStoreSchemaGeneratorNamespaceName());
            nsMgr.AddNamespace("ssdl", GetSSDLNamespaceName(schemaVersion));
            nsMgr.AddNamespace("msl", GetMSLNamespaceName(schemaVersion));
            return nsMgr;
        }
    }
}
