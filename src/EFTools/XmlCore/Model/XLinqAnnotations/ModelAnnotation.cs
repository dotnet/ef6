// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.XLinqAnnotations
{
    using System.Xml.Linq;

    internal class ModelAnnotation
    {
        internal long NextIdentity { get; set; }

        internal static long GetNextIdentity(XObject element)
        {
            var annotation = element.Annotation<ModelAnnotation>();
            if (annotation == null)
            {
                annotation = new ModelAnnotation();
                element.AddAnnotation(annotation);
            }

            var nextIdentity = annotation.NextIdentity;
            annotation.NextIdentity = ++nextIdentity;

            return nextIdentity;
        }
    }
}
