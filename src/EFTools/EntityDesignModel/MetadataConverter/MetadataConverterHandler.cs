// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    ///     Base class of Metadata converter
    /// </summary>
    internal abstract class MetadataConverterHandler
    {
        private MetadataConverterHandler _successor;

        internal void SetNextHandler(MetadataConverterHandler successor)
        {
            _successor = successor;
        }

        internal XmlDocument HandleConversion(XmlDocument doc)
        {
            // TODO: do we want to continue if one of the chain operation failed?
            var resultDocument = DoHandleConversion(doc);

            Debug.Assert(resultDocument != null, "The document returned by metadata converter is null");

            if (_successor != null
                && resultDocument != null)
            {
                resultDocument = _successor.HandleConversion(resultDocument);
            }
            return resultDocument;
        }

        protected abstract XmlDocument DoHandleConversion(XmlDocument doc);
    }
}
