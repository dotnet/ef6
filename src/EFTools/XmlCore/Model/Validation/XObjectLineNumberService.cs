// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    internal class XObjectLineNumberService
    {
        private readonly XmlModelProvider _xmlModelProvider;

        internal XObjectLineNumberService(XmlModelProvider xmlModelProvider)
        {
            Debug.Assert(xmlModelProvider != null, "xmlModelProvider != null");

            _xmlModelProvider = xmlModelProvider;
        }

        internal int GetLineNumber(XObject xobject, Uri uri)
        {
            var textSpan = _xmlModelProvider.GetTextSpanForXObject(xobject, uri);
            return textSpan.iStartLine;
        }

        internal int GetColumnNumber(XObject xobject, Uri uri)
        {
            var textSpan = _xmlModelProvider.GetTextSpanForXObject(xobject, uri);
            return textSpan.iStartIndex;
        }
    }
}
