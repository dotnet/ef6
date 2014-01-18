// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.XmlDesignerBase.Model.StandAlone;

    // <summary>
    //     This XmlModelProvider uses a strategy pattern to accept 'Loaders' which
    //     define how to build an XDocument from a given URI. In addition, it can discriminate
    //     between a parent URI and children URI. For example, the parent URI for the Entity Designer
    //     would be an .edmx file while child URIs would include the .edmx.diagram file.
    // </summary>
    internal class StandaloneXmlModelProvider : VanillaXmlModelProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public StandaloneXmlModelProvider(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null, "serviceProvider != null");

            _serviceProvider = serviceProvider;
        }

        private List<ExtensionError> _extensionErrors;

        internal IList<ExtensionError> ExtensionErrors
        {
            get { return _extensionErrors; }
        }

        protected override XDocument Build(Uri uri)
        {
            Debug.Assert(uri != null, "uri != null");

            return
                EntityDesignArtifact.ExtensionEdmx.Equals(
                    Path.GetExtension(uri.LocalPath),
                    StringComparison.OrdinalIgnoreCase)
                    ? CreateAnnotatedXDocument(ReadEdmxContents(uri))
                    : CreateAnnotatedXDocument(uri);
        }

        // virtual to allow mocking
        protected virtual string ReadEdmxContents(Uri uri)
        {
            Debug.Assert(uri != null, "uri != null");

            // Try to load the artifact string via extensions, if that doesn't work fall back to using the buffer on disk
            var inputDocument = File.ReadAllText(uri.LocalPath);
            var projectItem = VsUtils.GetProjectItemForDocument(uri.LocalPath, _serviceProvider);
            string documentToLoad;
            return
                TryGetBufferViaExtensions(projectItem, inputDocument, out documentToLoad, out _extensionErrors)
                    ? documentToLoad
                    : inputDocument;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static XDocument CreateAnnotatedXDocument(string documentContents)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(documentContents), "invalid edmx contents");

            using (var stringReader = new StringReader(documentContents))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    return CreateAnnotatedXDocument(reader);
                }
            }
        }

        private static XDocument CreateAnnotatedXDocument(Uri uri)
        {
            Debug.Assert(uri != null, "uri != null");

            using (var reader = XmlReader.Create(uri.LocalPath))
            {
                return CreateAnnotatedXDocument(reader);
            }
        }

        private static XDocument CreateAnnotatedXDocument(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");

            return new AnnotatedTreeBuilder().Build(reader);
        }

        internal static bool TryGetBufferViaExtensions(
            ProjectItem projectItem, string fileContents,
            out string documentViaExtensions, out List<ExtensionError> errors)
        {
            var converters = EscherExtensionPointManager.LoadModelConversionExtensions();
            var serializers = EscherExtensionPointManager.LoadModelTransformExtensions();

            if (projectItem == null
                || !VsUtils.EntityFrameworkSupportedInProject(projectItem.ContainingProject, PackageManager.Package, allowMiscProject: false)
                || (serializers.Length == 0 && converters.Length == 0))
            {
                errors = new List<ExtensionError>();
                documentViaExtensions = "";
                return false;
            }

            return TryGetBufferViaExtensions(
                PackageManager.Package, projectItem, fileContents,
                converters, serializers, out documentViaExtensions, out errors);
        }

        // TODO: Refactor this method when fixing https://entityframework.codeplex.com/workitem/1371
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static bool TryGetBufferViaExtensions(
            IServiceProvider serviceProvider, ProjectItem projectItem, string fileContents,
            Lazy<IModelConversionExtension, IEntityDesignerConversionData>[] converters,
            Lazy<IModelTransformExtension>[] serializers, out string documentViaExtensions,
            out List<ExtensionError> errors)
        {
            Debug.Assert(serviceProvider != null, "serviceProvider != null");
            Debug.Assert(projectItem != null, "projectItem != null");
            Debug.Assert(VsUtils.EntityFrameworkSupportedInProject(projectItem.ContainingProject, serviceProvider, false));
            Debug.Assert(serializers != null && converters != null, "extensions must not be null");
            Debug.Assert(serializers.Any() || converters.Any(), "at least one extension expected");

            errors = new List<ExtensionError>();
            documentViaExtensions = "";

            ModelConversionContextImpl conversionContext = null;
            ModelTransformContextImpl transformContext = null;

            try
            {
                var targetSchemaVersion =
                    EdmUtils.GetEntityFrameworkVersion(projectItem.ContainingProject, serviceProvider);

                Debug.Assert(targetSchemaVersion != null, "should not get here for a Misc project");

                // get the extension of the file being loaded (might not be EDMX); this API will include the preceeding "."
                var filePath = projectItem.get_FileNames(1);
                var fileExtension = Path.GetExtension(filePath);
                XDocument originalDocument = null;

                // see if we are loading an EDMX file or not, and if we have any converters
                if (!string.Equals(
                    fileExtension, EntityDesignArtifact.ExtensionEdmx,
                    StringComparison.OrdinalIgnoreCase))
                {
                    conversionContext = new ModelConversionContextImpl(
                        projectItem.ContainingProject, projectItem, new FileInfo(filePath),
                        targetSchemaVersion, fileContents);

                    // we aren't loading an EDMX file, so call the extensions who can process this file extension
                    // when this finishes, then output should be a valid EDMX document
                    VSArtifact.DispatchToConversionExtensions(converters, fileExtension, conversionContext, true);

                    // we are done with the non-EDMX extensions so CurrentDocument will be a valid EDMX document
                    // create the serialization context for further extensions to act on
                    transformContext = new ModelTransformContextImpl(
                        projectItem, targetSchemaVersion, conversionContext.CurrentDocument);
                }
                else
                {
                    // we are loading an EDMX file, we can parse file contents into an XDocument
                    try
                    {
                        originalDocument = XDocument.Parse(fileContents, LoadOptions.PreserveWhitespace);
                        transformContext = new ModelTransformContextImpl(
                            projectItem, targetSchemaVersion, originalDocument);
                    }
                    catch (XmlException)
                    {
                        // If there's an error here, don't do anything. We will want to gracefully step out of the extension loading
                        // since the designer itself won't load.
                    }
                }

                if (transformContext != null
                    && originalDocument != null)
                {
                    // now dispatch to those that want to work on EDMX files
                    VSArtifact.DispatchToSerializationExtensions(serializers, transformContext, true);

                    // TODO: this does not seem to be correct if severity is Message or Warning
                    if (transformContext.Errors.Count == 0)
                    {
                        // see if any extension changed things. Note that we need to compare the serialization of
                        // the XDocuments together since the original buffer may have different whitespace after creating the XDocument.
                        // TODO: Why not use XNode.DeepEquals()?
                        string newBufferContents;
                        using (var currentDocWriter = new Utf8StringWriter())
                        {
                            transformContext.CurrentDocument.Save(currentDocWriter, SaveOptions.None);
                            newBufferContents = currentDocWriter.ToString();
                        }

                        string originalBufferContents;
                        using (var originalDocWriter = new Utf8StringWriter())
                        {
                            originalDocument.Save(originalDocWriter, SaveOptions.None);
                            originalBufferContents = originalDocWriter.ToString();
                        }

                        if (!string.Equals(originalBufferContents, newBufferContents, StringComparison.Ordinal))
                        {
                            documentViaExtensions = newBufferContents;
                            return true;
                        }
                    }
                    else
                    {
                        errors.AddRange(transformContext.Errors);
                        return false;
                    }
                }
            }
            finally
            {
                var errorList = ErrorListHelper.GetExtensionErrorList(serviceProvider);
                errorList.Clear();

                // log any errors                   
                if (conversionContext != null
                    && conversionContext.Errors.Count > 0)
                {
                    ErrorListHelper.LogExtensionErrors(conversionContext.Errors, projectItem);
                }

                if (transformContext != null
                    && transformContext.Errors.Count > 0)
                {
                    ErrorListHelper.LogExtensionErrors(transformContext.Errors, projectItem);
                }
            }

            return false;
        }
    }
}
