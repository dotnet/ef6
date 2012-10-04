// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Common;

    internal abstract class DataModelItemVisitor
    {
        protected static void VisitCollection<T>(IEnumerable<T> collection, Action<T> visitMethod)
        {
            if (collection != null)
            {
                foreach (var element in collection)
                {
                    visitMethod(element);
                }
            }
        }

        protected virtual void VisitAnnotations(DataModelItem item, IEnumerable<DataModelAnnotation> annotations)
        {
            VisitCollection(annotations, VisitAnnotation);
        }

        protected virtual void VisitAnnotations(MetadataItem item, IEnumerable<DataModelAnnotation> annotations)
        {
            VisitCollection(annotations, VisitAnnotation);
        }

        protected virtual void VisitAnnotation(DataModelAnnotation item)
        {
        }

        protected virtual void VisitDataModelItem(DataModelItem item)
        {
        }
    }
}
