// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    internal class LazyInitialModelContentsFactory : IInitialModelContentsFactory
    {
        private readonly string _fileContentsTemplate;
        private readonly IDictionary<string, string> _replacementsDictionary;
        private string _initialModelContents;

        public LazyInitialModelContentsFactory(
            string fileContentsTemplate,
            IDictionary<string, string> replacementsDictionary)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(fileContentsTemplate), "fileContentsTemplate is null or empty.");
            Debug.Assert(replacementsDictionary != null, "replacementsDictionary is null.");

            _fileContentsTemplate = fileContentsTemplate;
            _replacementsDictionary = replacementsDictionary;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public string GetInitialModelContents(Version targetSchemaVersion)
        {
            Debug.Assert(
                EntityFrameworkVersion.IsValidVersion(targetSchemaVersion),
                "invalid schema version");

            if (_initialModelContents == null)
            {
                AddSchemaSpecificReplacements(_replacementsDictionary, targetSchemaVersion);

                var sb = new StringBuilder(_fileContentsTemplate);
                foreach (var pair in _replacementsDictionary)
                {
                    sb.Replace(pair.Key, pair.Value);
                }

                _initialModelContents = sb.ToString();
            }

            return _initialModelContents;
        }

        public static void AddSchemaSpecificReplacements(IDictionary<string, string> replacementsDictionary, Version schemaVersion)
        {
            Debug.Assert(replacementsDictionary != null, "replacementsDictionary is null.");
            Debug.Assert(schemaVersion != null, "schemaVersion is null.");
            Debug.Assert(!replacementsDictionary.ContainsKey("$edmxversion$"), "replacementsDictionary contains key '$edmxversion$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$edmxnamespacename$"), "replacementsDictionary contains key '$edmxnamespacename$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$ssdlnamespacename$"), "replacementsDictionary contains key '$ssdlnamespacename$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$csdlnamespacename$"), "replacementsDictionary contains key '$csdlnamespacename$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$msdlnamespacename$"), "replacementsDictionary contains key '$msdlnamespacename$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$essgnamespacename$"), "replacementsDictionary contains key '$essgnamespacename$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$cgnamespacename$"), "replacementsDictionary contains key '$cgnamespacename$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$annotationnamespace$"), "replacementsDictionary contains key '$annotationnamespace$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$lazyloadingattribute$"),
                "replacementsDictionary contains key '$lazyloadingattribute$'");
            Debug.Assert(
                !replacementsDictionary.ContainsKey("$useStrongSpatialTypesAttribute$"),
                "replacementsDictionary contains key '$useStrongSpatialTypesAttribute$'");

            // Set the namespace names (EDMX, CodeGen, ESSG, CSDL, MSDL, and SSDL)
            replacementsDictionary.Add("$edmxversion$", schemaVersion.ToString(2)); // only print Major.Minor version information
            replacementsDictionary.Add("$edmxnamespacename$", SchemaManager.GetEDMXNamespaceName(schemaVersion));
            replacementsDictionary.Add("$ssdlnamespacename$", SchemaManager.GetSSDLNamespaceName(schemaVersion));
            replacementsDictionary.Add("$csdlnamespacename$", SchemaManager.GetCSDLNamespaceName(schemaVersion));
            replacementsDictionary.Add("$msdlnamespacename$", SchemaManager.GetMSLNamespaceName(schemaVersion));
            replacementsDictionary.Add(
                "$essgnamespacename$",
                SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName());
            replacementsDictionary.Add(
                "$cgnamespacename$",
                SchemaManager.GetCodeGenerationNamespaceName());

            if (EdmFeatureManager.GetLazyLoadingFeatureState(schemaVersion).IsEnabled()
                || EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(schemaVersion).IsEnabled())
            {
                replacementsDictionary.Add(
                    "$annotationnamespace$",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "xmlns:annotation=\"{0}\"",
                        SchemaManager.GetAnnotationNamespaceName()));
            }
            else
            {
                replacementsDictionary.Add("$annotationnamespace$", string.Empty);
            }

            if (EdmFeatureManager.GetLazyLoadingFeatureState(schemaVersion).IsEnabled())
            {
                replacementsDictionary.Add("$lazyloadingattribute$", "annotation:LazyLoadingEnabled=\"true\"");
            }
            else
            {
                replacementsDictionary.Add("$lazyloadingattribute$", string.Empty);
            }

            // set UseStrongSpatialTypes to false as runtime will throw exception if true (as of V3 - to be updated in later version of runtime)
            if (EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(schemaVersion).IsEnabled())
            {
                replacementsDictionary.Add(
                    "$useStrongSpatialTypesAttribute$",
                    "annotation:UseStrongSpatialTypes=\"false\"");
            }
            else
            {
                replacementsDictionary.Add("$useStrongSpatialTypesAttribute$", string.Empty);
            }
        }
    }
}
