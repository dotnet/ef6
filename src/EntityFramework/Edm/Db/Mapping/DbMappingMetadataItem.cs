// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    ///     DbMappingMetadataItem is the base for all types in the EDM-to-Database Mapping construction and modification API that support annotation using <see
    ///      cref="DataModelAnnotation" /> .
    /// </summary>
    internal abstract class DbMappingMetadataItem
        : DbMappingModelItem, IAnnotatedDataModelItem
    {
        private readonly BackingList<DataModelAnnotation> annotationsList = new BackingList<DataModelAnnotation>();

        #region Implementation of IAnnotatedDataModelItem

        IList<DataModelAnnotation> IAnnotatedDataModelItem.Annotations
        {
            get { return Annotations; }
            set { Annotations = value; }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the currently assigned annotations.
        /// </summary>
        internal virtual IList<DataModelAnnotation> Annotations
        {
            get { return annotationsList.EnsureValue(); }
            set { annotationsList.SetValue(value); }
        }
    }
}
