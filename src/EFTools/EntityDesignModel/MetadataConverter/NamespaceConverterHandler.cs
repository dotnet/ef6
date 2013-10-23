// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Xsl;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal sealed class NamespaceConverterHandler : MetadataConverterHandler
    {
        private readonly XslCompiledTransform _xsltTransform;

        // TODO: change to pass the namespaces as the parameters.
        private const string XsltTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
               <xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
               <xsl:output method=""xml"" version=""1.0"" indent=""no"" omit-xml-declaration=""no""/>

                <!-- EDMX -->
                <xsl:template match=""*[namespace-uri() = '{0}' ]"">
                       <xsl:element name=""{{name()}}"" namespace=""{1}"">
                            <xsl:apply-templates select=""@* | node()"" />
                        </xsl:element>
                 </xsl:template>
                 <!-- CSDL -->
                 <xsl:template match=""*[namespace-uri() = '{2}']"">
                       <xsl:element name=""{{name()}}"" namespace=""{3}"">
                            <xsl:apply-templates select=""@* | node()"" />
                        </xsl:element>
                 </xsl:template>
                 <!-- SSDL -->
                 <xsl:template match=""*[namespace-uri() = '{4}']"">
                       <xsl:element name=""{{name()}}"" namespace=""{5}"">
                            <xsl:apply-templates select=""@* | node()"" />
                        </xsl:element>
                 </xsl:template>
                 <!-- MSL -->
                 <xsl:template match=""*[namespace-uri() = '{6}']"">
                       <xsl:element name=""{{name()}}"" namespace=""{7}"">
                            <xsl:apply-templates select=""@* | node()"" />
                        </xsl:element>
                 </xsl:template>
                 <xsl:template match=""node() | @*"">
                    <xsl:copy>
                        <xsl:apply-templates select=""node() | @*"" />
                    </xsl:copy> 
                 </xsl:template>
             </xsl:stylesheet>";

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal NamespaceConverterHandler(Version sourceSchemaVersion, Version targetSchemaVersion)
        {
            var template = String.Format(
                CultureInfo.InvariantCulture, XsltTemplate,
                SchemaManager.GetEDMXNamespaceName(sourceSchemaVersion), SchemaManager.GetEDMXNamespaceName(targetSchemaVersion),
                SchemaManager.GetCSDLNamespaceName(sourceSchemaVersion), SchemaManager.GetCSDLNamespaceName(targetSchemaVersion),
                SchemaManager.GetSSDLNamespaceName(sourceSchemaVersion), SchemaManager.GetSSDLNamespaceName(targetSchemaVersion),
                SchemaManager.GetMSLNamespaceName(sourceSchemaVersion), SchemaManager.GetMSLNamespaceName(targetSchemaVersion));

            _xsltTransform = new XslCompiledTransform();

            using (var reader = XmlReader.Create(new StringReader(template)))
            {
                _xsltTransform.Load(reader);
            }
        }

        /// <summary>
        ///     Change csdl, ssdl, msl and EDMX namespaces
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        protected override XmlDocument DoHandleConversion(XmlDocument doc)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream, _xsltTransform.OutputSettings))
                {
                    _xsltTransform.Transform(doc.CreateNavigator(), null, xmlWriter);
                }

                memoryStream.Position = 0;
                using (var reader = XmlReader.Create(memoryStream))
                {
                    var resultDocument = new XmlDocument { PreserveWhitespace = true };
                    resultDocument.Load(reader);
                    return resultDocument;
                }
            }
        }
    }
}
