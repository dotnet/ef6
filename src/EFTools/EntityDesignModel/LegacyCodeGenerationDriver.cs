// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyCodegen;

    internal class LegacyCodeGenerationDriver
    {
        private readonly LanguageOption _language;
        private readonly Version _targetEntityFrameworkVersion;

        public LegacyCodeGenerationDriver(LanguageOption language, Version targetEntityFrameworkVersion)
        {
            Debug.Assert(
                EntityFrameworkVersion.IsValidVersion(targetEntityFrameworkVersion),
                "invalid targetEntityFrameworkVersion");

            _language = language;
            _targetEntityFrameworkVersion = targetEntityFrameworkVersion;
        }

        public IList<EdmSchemaError> GenerateCode(EFArtifact artifact, string defaultNamespace, TextWriter outputWriter)
        {
            Debug.Assert(outputWriter != null, "OutputWriter parameter is null");
            Debug.Assert(artifact != null, "artifact parameter is null");

            // create a specialized xml reader that piggybacks off of XNodeReader and keeps track 
            // of line numbers using the Xml Model
            using (var xmlModelReader = CreateXmlReader(artifact, artifact.ConceptualModel().XElement))
            {
                var codeGenerator = CreateCodeGenerator(_language, _targetEntityFrameworkVersion);

                Debug.Assert(artifact.ConceptualModel() != null, "Artifact ConceptuaModel is null");

                if (defaultNamespace != null && artifact.ConceptualModel() != null)
                {
                    codeGenerator.AddNamespaceMapping(artifact.ConceptualModel().Namespace.Value, defaultNamespace);
                }

                return codeGenerator.GenerateCode(xmlModelReader, outputWriter);
            }
        }

        private static XmlReader CreateXmlReader(EFArtifact artifact, XElement xobject)
        {
            var baseReader = xobject.CreateReader();
            var lineNumberService = new XNodeReaderLineNumberService(artifact.XmlModelProvider, baseReader, artifact.Uri);
            var proxyReader = new XmlReaderProxy(baseReader, artifact.Uri, lineNumberService);
            return proxyReader;
        }

        // protected virtual to allow mocking
        protected virtual CodeGeneratorBase CreateCodeGenerator(LanguageOption language, Version targetEntityFrameworkVersion)
        {
            return CodeGeneratorBase.Create(language, targetEntityFrameworkVersion);
        }
    }
}
