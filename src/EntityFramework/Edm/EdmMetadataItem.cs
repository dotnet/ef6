// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     The base for all all Entity Data Model (EDM) types that support annotation using <see cref="DataModelAnnotation" /> .
    /// </summary>
    internal abstract class EdmMetadataItem
        : EdmDataModelItem, IAnnotatedDataModelItem
    {
        private readonly BackingList<DataModelAnnotation> annotationsList = new BackingList<DataModelAnnotation>();

        /// <summary>
        ///     Gets or sets the currently assigned annotations.
        /// </summary>
        public virtual IList<DataModelAnnotation> Annotations
        {
            get { return annotationsList.EnsureValue(); }
            set { annotationsList.SetValue(value); }
        }

        internal bool HasAnnotations
        {
            get { return annotationsList.HasValue; }
        }

        /// <summary>
        ///     Returns all EdmItem children directly contained by this EdmItem.
        /// </summary>
        public virtual IEnumerable<EdmMetadataItem> ChildItems
        {
            get { return GetChildItems(); }
        }

        protected abstract IEnumerable<EdmMetadataItem> GetChildItems();

        internal static IEnumerable<EdmMetadataItem> Yield(params EdmMetadataItem[] items)
        {
            Contract.Assert(items != null, "Yielding null items list?");
            return items;
        }
    }
}
