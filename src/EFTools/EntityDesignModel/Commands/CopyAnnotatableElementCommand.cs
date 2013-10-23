// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal abstract class CopyAnnotatableElementCommand : Command
    {
        protected virtual void AddAnnotations(AnnotatableElementClipboardFormat clipboardFormat, EFElement element)
        {
            foreach (var t in clipboardFormat.AdditionalAttributes)
            {
                var name = t.Item1;
                var value = t.Item2;
                var xn = XName.Get(name);
                var xa = new XAttribute(xn, value);
                element.XElement.Add(xa);
            }

            foreach (var s in clipboardFormat.AdditionalElements)
            {
                try
                {
                    var d = XDocument.Parse(s, LoadOptions.PreserveWhitespace);
                    var xe = d.Root;
                    xe.Remove();
                    element.XElement.Add(xe);
                }
                catch (XmlException)
                {
                    // ignore an XmlException.  There was probalby a problem parsing.
                }
            }
        }
    }
}
