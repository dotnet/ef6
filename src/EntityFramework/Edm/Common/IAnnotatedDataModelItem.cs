// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Common
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     IAnnotatedDataModelItem is implemented by model-specific base types for all types with an <see cref="Annotations" /> property. <seealso
    ///      cref="EdmDataModelItem" />
    /// </summary>
    public interface IAnnotatedDataModelItem
    {
        /// <summary>
        ///     Gets or sets the currently assigned annotations.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<DataModelAnnotation> Annotations { get; set; }
    }
}
