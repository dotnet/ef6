// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal abstract class AnnotatableElement : EFElement
    {
        internal AnnotatableElement(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        // This will be called from the child EFObject's constructor, so not all of the member variables may be hooked up yet. 
        // be careful.  referencing certain fields may cause null-reference exceptions 
        internal override void GetXLinqInsertPosition(EFElement child, out XNode insertAt, out bool insertBefore)
        {
            GetInsertPointForAnnotatableElements(this, out insertAt, out insertBefore);
        }

        internal static void GetInsertPointForAnnotatableElements(EFElement parent, out XNode insertPosition, out bool insertBefore)
        {
            if (parent.XContainer.Elements().Count() == 0)
            {
                insertPosition = null;
                insertBefore = false;
            }
            else
            {
                insertPosition = GetLastSiblingOfMyNamespace(parent);
                insertBefore = false;
                if (insertPosition == null)
                {
                    insertPosition = parent.XContainer.Elements().First();
                    insertBefore = true;
                }
            }
        }

        /// <summary>
        ///     this will always return the preceding sibling that appears before the first sibling with a different namespace.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private static XElement GetLastSiblingOfMyNamespace(EFElement parent)
        {
            string expectedNamespace;
            if (ModelHelper.GetBaseModelRoot(parent).IsCSDL)
            {
                expectedNamespace = SchemaManager.GetCSDLNamespaceName(parent.Artifact.SchemaVersion);
            }
            else
            {
                expectedNamespace = SchemaManager.GetSSDLNamespaceName(parent.Artifact.SchemaVersion);
            }

            var c = parent.XContainer;
            XElement predecessor = null;
            foreach (var e in c.Elements())
            {
                if (e.Name.NamespaceName.Equals(expectedNamespace, StringComparison.OrdinalIgnoreCase))
                {
                    predecessor = e;
                }
                else
                {
                    break;
                }
            }
            return predecessor;
        }
    }
}
