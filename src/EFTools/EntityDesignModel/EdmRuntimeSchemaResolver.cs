// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Xml;

    /// <summary>
    ///     Class to use to resolve references to EDM runtime schemas when building the EdmxSchemaSet.
    /// </summary>
    internal class EdmRuntimeSchemaResolver : XmlUrlResolver
    {
        // keep a map of "relative" names to the URI that we will create for them
        private static readonly Dictionary<string, Uri> ResourceNameToUri = new Dictionary<string, Uri>(11);

        // for some resources, we will load them through a runtime API, so we keep the Version & DataSpace for the URI
        private static readonly Dictionary<Uri, string> UriToResourceName = new Dictionary<Uri, string>(11);

        // Currently, runtime shipped with all xsd versions; so we don't have to worry about loading the correct assembly
        private static readonly Assembly EntityFrameworkAssembly = typeof(EdmItemCollection).Assembly;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static EdmRuntimeSchemaResolver()
        {
            AddUri("System.Data.Resources.CSDLSchema_1.xsd");
            AddUri("System.Data.Resources.CSDLSchema_2.xsd");
            AddUri("System.Data.Resources.CSDLSchema_3.xsd");
            AddUri("System.Data.Resources.CSMSL_1.xsd");
            AddUri("System.Data.Resources.CSMSL_2.xsd");
            AddUri("System.Data.Resources.CSMSL_3.xsd");
            AddUri("System.Data.Resources.SSDLSchema.xsd");
            AddUri("System.Data.Resources.SSDLSchema_2.xsd");
            AddUri("System.Data.Resources.SSDLSchema_3.xsd");
            AddUri("System.Data.Resources.ProviderServices.ProviderManifest.xsd");
            AddUri("System.Data.Resources.EntityStoreSchemaGenerator.xsd");
            AddUri("System.Data.Resources.CodeGenerationSchema.xsd");
            AddUri("System.Data.Resources.AnnotationSchema.xsd");
        }

        private static void AddUri(string name)
        {
            Debug.Assert(!ResourceNameToUri.ContainsKey(name), "Duplicate schema found. Name: " + name);

            var uri = new Uri("res://" + name, UriKind.Absolute);
            Debug.Assert(!UriToResourceName.ContainsKey(uri), "uri can't be in more than one map!");
            ResourceNameToUri.Add(name, uri);
            UriToResourceName.Add(uri, name);
        }

        // returns a stream opened up for the requested schema identified by "res://<relativeUri>"
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            string resourceName;

            if (UriToResourceName.TryGetValue(absoluteUri, out resourceName))
            {
                return EntityFrameworkAssembly.GetManifestResourceStream(resourceName);
            }

            Debug.Fail("Didn't expect GetEntity to be called for absoluteUri " + absoluteUri);
            return base.GetEntity(absoluteUri, role, ofObjectToReturn);
        }

        // returns "res://<relativeUri>" 
        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if (ResourceNameToUri.ContainsKey(relativeUri))
            {
                return ResourceNameToUri[relativeUri];
            }

            Debug.Fail("didn't expect ResolveUri to be called for relativeUri " + relativeUri);
            return base.ResolveUri(baseUri, relativeUri);
        }
    }
}
