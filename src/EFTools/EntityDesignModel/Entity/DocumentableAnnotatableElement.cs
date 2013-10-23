// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Xml.Linq;

    internal abstract class DocumentableAnnotatableElement : EFDocumentableItem
    {
        internal DocumentableAnnotatableElement(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        // This will be called from the child EFObject's constructor, so not all of the member variables may be hooked up yet. 
        // be careful.  referencing certain fields may cause null-reference exceptions 
        internal override void GetXLinqInsertPosition(EFElement child, out XNode insertAt, out bool insertBefore)
        {
            if (child is Documentation)
            {
                // base class will return correct position to insert the Documentation element - these always need to go first
                base.GetXLinqInsertPosition(child, out insertAt, out insertBefore);
            }
            else
            {
                AnnotatableElement.GetInsertPointForAnnotatableElements(this, out insertAt, out insertBefore);
            }
        }
    }

    internal abstract class NameableAnnotatableElement : EFNameableItem
    {
        internal NameableAnnotatableElement(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        // This will be called from the child EFObject's constructor, so not all of the member variables may be hooked up yet. 
        // be careful.  referencing certain fields may cause null-reference exceptions 
        internal override void GetXLinqInsertPosition(EFElement child, out XNode insertAt, out bool insertBefore)
        {
            AnnotatableElement.GetInsertPointForAnnotatableElements(this, out insertAt, out insertBefore);
        }
    }
}
