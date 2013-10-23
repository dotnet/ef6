// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;

    internal class Documentation : EFElement
    {
        internal static readonly string ElementName = "Documentation";

        private Summary _summary;
        private LongDescription _longDescription;

        internal Documentation(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }

        internal Summary Summary
        {
            get { return _summary; }
            set { _summary = value; }
        }

        internal LongDescription LongDescription
        {
            get { return _longDescription; }
            set { _longDescription = value; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                if (_summary != null)
                {
                    yield return _summary;
                }
                if (_longDescription != null)
                {
                    yield return _longDescription;
                }
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            if (efContainer is Summary)
            {
                _summary = null;
            }
            else if (efContainer is LongDescription)
            {
                _longDescription = null;
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(Summary.ElementName);
            s.Add(LongDescription.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");
            ClearEFObject(_summary);
            _summary = null;
            ClearEFObject(_longDescription);
            _longDescription = null;
            base.PreParse();
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == Summary.ElementName)
            {
                _summary = new Summary(this, elem);
                _summary.Parse(unprocessedElements);
            }
            else if (elem.Name.LocalName == LongDescription.ElementName)
            {
                _longDescription = new LongDescription(this, elem);
                _longDescription.Parse(unprocessedElements);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        internal static void GetInsertPositionForSiblingThatNeedsToBeAfterDocumentationElementButBeforeOtherElements(
            EFContainer parent, out XNode element, out bool insertBefore)
        {
            element = null;
            insertBefore = false;
            var efElement = parent as EFElement;
            if (efElement != null
                && efElement.HasDocumentationElement
                && efElement.DocumentationEFContainer != null)
            {
                // insert after documentation element
                element = efElement.DocumentationEFContainer.XContainer;
                insertBefore = false;
            }
            else if (parent.XContainer.Elements().Any())
            {
                // insert before first element
                element = parent.XContainer.Elements().First();
                insertBefore = true;
            }
        }

        internal override void GetXLinqInsertPosition(EFElement child, out XNode insertAt, out bool insertBefore)
        {
            if (child is Summary)
            {
                insertAt = FirstChildXElementOrNull();
                insertBefore = true;
            }
            else
            {
                base.GetXLinqInsertPosition(child, out insertAt, out insertBefore);
            }
        }
    }
}
