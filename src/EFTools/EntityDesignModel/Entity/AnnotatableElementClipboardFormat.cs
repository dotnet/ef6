// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;

    [Serializable]
    internal abstract class AnnotatableElementClipboardFormat : EFElementClipboardFormat
    {
        private readonly List<String> _additionalElements = new List<String>();
        private readonly List<Tuple<String, String>> _additionalAttributes = new List<Tuple<String, String>>();

        internal AnnotatableElementClipboardFormat(EFElement efElement)
            : base(efElement)
        {
            // scan through the XML and identify any "extra" attributes we want to include in the copy
            foreach (var xo in ModelHelper.GetStructuredAnnotationsForElement(efElement))
            {
                var xa = xo as XAttribute;
                var xe = xo as XElement;
                if (xa != null)
                {
                    var t = new Tuple<string, string>(xa.Name.ToString(), xa.Value);
                    _additionalAttributes.Add(t);
                }
                else if (xe != null)
                {
                    _additionalElements.Add(xe.ToString(SaveOptions.None));
                }
                else
                {
                    Debug.Fail("unexepected type of XObject returned from GetAnnotationsForElement()");
                }
            }
        }

        internal IEnumerable<string> AdditionalElements
        {
            get { return _additionalElements; }
        }

        internal IEnumerable<Tuple<String, String>> AdditionalAttributes
        {
            get { return _additionalAttributes; }
        }
    }
}
