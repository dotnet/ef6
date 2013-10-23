// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Model.StandAlone
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    ///     This class will provide an XML model over an in-memory string.
    ///     The URI doesn't need to correspond to a file on disk.
    /// </summary>
    internal class InMemoryXmlModelProvider : VanillaXmlModelProvider
    {
        private readonly Uri _inputUri;
        private readonly string _inputXml;

        internal InMemoryXmlModelProvider(Uri inputUri, string inputXml)
        {
            _inputUri = inputUri;
            _inputXml = inputXml;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override XDocument Build(Uri uri)
        {
            if (uri != _inputUri)
            {
                throw new ArgumentException("specified URI does not match the URI used to create this model provider");
            }
            var builder = new AnnotatedTreeBuilder();
            using (var reader = XmlReader.Create(new StringReader(_inputXml)))
            {
                var doc = builder.Build(reader);
                return doc;
            }
        }
    }
}
