// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The base for all all Database Metadata types that support annotation using <see cref="DataModelAnnotation" /> .
    /// </summary>
    public abstract class DbMetadataItem
        : DbDataModelItem
    {
        private IList<DataModelAnnotation> annotationsList = new List<DataModelAnnotation>();

        /// <summary>
        ///     Gets or sets the currently assigned annotations.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DataModelAnnotation> Annotations
        {
            get { return annotationsList; }
            set { annotationsList = value; }
        }
    }
}
