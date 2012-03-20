namespace System.Data.Entity.Edm.Internal
{
    using System.Collections.Generic;
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

        #region Documentation, Annotations

        protected virtual void VisitAnnotations(DataModelItem item, IEnumerable<DataModelAnnotation> annotations)
        {
            VisitCollection(annotations, VisitAnnotation);
        }

        protected virtual void VisitAnnotation(DataModelAnnotation item)
        {
        }

        #endregion

        protected virtual void VisitDataModelItem(DataModelItem item)
        {
        }
    }
}