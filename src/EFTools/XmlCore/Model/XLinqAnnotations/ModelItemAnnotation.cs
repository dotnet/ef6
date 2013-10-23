// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.XLinqAnnotations
{
    using System.Xml.Linq;

    internal static class ModelItemAnnotation
    {
        internal static EFObject GetModelItem(XObject xobject)
        {
            var mia = xobject.Annotation<EFObject>();
            while (mia == null
                   && xobject.Parent != null)
            {
                mia = xobject.Parent.Annotation<EFObject>();
                xobject = xobject.Parent;
            }

            return mia;
        }

        internal static void SetModelItem(XObject xobject, EFObject efobject)
        {
            if (xobject.Annotation<EFObject>() == null)
            {
                xobject.AddAnnotation(efobject);
            }
            else
            {
                xobject.RemoveAnnotations<EFObject>();
                xobject.AddAnnotation(efobject);
            }
        }
    }
}
