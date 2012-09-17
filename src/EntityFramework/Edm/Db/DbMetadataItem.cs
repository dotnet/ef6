// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The base for all all Database Metadata types that support annotation using <see cref="DataModelAnnotation" /> .
    /// </summary>
    public abstract class DbMetadataItem
        : DbDataModelItem, IAnnotatedDataModelItem
    {
        private readonly BackingList<DataModelAnnotation> annotationsList = new BackingList<DataModelAnnotation>();

        /// <summary>
        ///     Gets or sets the currently assigned annotations.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DataModelAnnotation> Annotations
        {
            get { return annotationsList.EnsureValue(); }
            set { annotationsList.SetValue(value); }
        }

        internal bool HasAnnotations
        {
            get { return annotationsList.HasValue; }
        }
    }
}
