// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class MetadataConverterDriver
    {
        private static MetadataConverterDriver _instance;
        private static MetadataConverterDriver _sqlCeInstance;

        private readonly IDictionary<string, MetadataConverterHandler> _handlers;

        public MetadataConverterDriver()
        {
            _handlers = new Dictionary<string, MetadataConverterHandler>();
        }

        /// <summary>
        ///     Returns the static instance of the MetadataConverterDriver
        /// </summary>
        internal static MetadataConverterDriver Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MetadataConverterDriver();
                }

                return _instance;
            }
        }

        /// <summary>
        ///     Returns the static instance of the MetadataConverterDriver used for SQL CE upgrades
        /// </summary>
        internal static MetadataConverterDriver SqlCeInstance
        {
            get
            {
                if (_sqlCeInstance == null)
                {
                    _sqlCeInstance = new MetadataConverterDriver();
                }

                return _sqlCeInstance;
            }
        }

        /// <summary>
        ///     Convert the EDMX content according to target framework
        /// </summary>
        /// <returns>converted document or null</returns>
        /// <remarks>Virtual to allow mocking.</remarks>
        internal virtual XmlDocument Convert(XmlDocument doc, Version targetSchemaVersion)
        {
            var schemaVersion = GetDocumentSchemaVersion(doc);
            var handler = GetConverterHandler(schemaVersion, targetSchemaVersion);

            if (handler != null)
            {
                return handler.HandleConversion(doc);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     Convert the EDMX content where the conversion does not depend on target framework
        ///     (currently only SQL CE upgrade)
        /// </summary>
        /// <param name="xmlDocPath"></param>
        /// <returns>converted document or null</returns>
        internal XmlDocument Convert(XmlDocument doc)
        {
            Debug.Assert(doc != null, "doc != null");

            var handler = CreateConverterHandler(GetDocumentSchemaVersion(doc));
            return handler != null ? handler.HandleConversion(doc) : null;
        }

        private MetadataConverterHandler GetConverterHandler(Version sourceSchemaVersion, Version targetSchemaVersion)
        {
            // if one of the versions is null, return immediately
            if (sourceSchemaVersion == null
                || targetSchemaVersion == null)
            {
                return null;
            }
                // if the versions are equal, no conversion will take place
            else if (sourceSchemaVersion == targetSchemaVersion)
            {
                return null;
            }
            else
            {
                // check the cache first
                // the key should be Source schema Version + target Schema Version
                var key = String.Format(
                    CultureInfo.InvariantCulture, "{0}{1}", sourceSchemaVersion.ToString(2), targetSchemaVersion.ToString(2));
                if (!_handlers.ContainsKey(key))
                {
                    _handlers[key] = CreateConverterHandler(sourceSchemaVersion, targetSchemaVersion);
                }
                return _handlers[key];
            }
        }

        private static Version GetDocumentSchemaVersion(XmlDocument document)
        {
            Version schemaVersion = null;

            // Make sure that the root's local element is "edmx"
            var element = document.DocumentElement;
            var isRootElementEdmxNode = String.Equals(element.LocalName, "Edmx", StringComparison.Ordinal);
            Debug.Assert(
                isRootElementEdmxNode,
                String.Format(CultureInfo.InvariantCulture, "Not valid root element name. Expected: 'edmx', found: {0}", element.LocalName));

            if (isRootElementEdmxNode)
            {
                // Get the xml namespace for the root element
                var namespaceName = element.NamespaceURI;
                Debug.Assert(!String.IsNullOrEmpty(namespaceName), "Could not find edmx namespace name");

                if (!String.IsNullOrEmpty(namespaceName))
                {
                    schemaVersion = SchemaManager.GetSchemaVersion(XNamespace.Get(namespaceName));
                }
            }

            return schemaVersion;
        }

        private MetadataConverterHandler CreateConverterHandler(Version sourceSchemaVersion)
        {
            if (this == _sqlCeInstance)
            {
                MetadataConverterHandler ssdlProviderAttributesConverter = new SsdlProviderAttributesHandler(sourceSchemaVersion);
                return ssdlProviderAttributesConverter;
            }

            return null;
        }

        private MetadataConverterHandler CreateConverterHandler(Version sourceSchemaVersion, Version targetSchemaVersion)
        {
            if (this == _instance)
            {
                var namespaceConverter = new NamespaceConverterHandler(sourceSchemaVersion, targetSchemaVersion);
                var versionConverter = new VersionConverterHandler(targetSchemaVersion);
                var useStrongSpatialTypesConverter = new UseStrongSpatialTypesHandler(targetSchemaVersion);

                namespaceConverter.SetNextHandler(versionConverter);
                versionConverter.SetNextHandler(useStrongSpatialTypesConverter);
                return namespaceConverter;
            }

            return null;
        }
    }
}
