// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Common
{
    using System.Collections.Generic;

    /// <summary>
    ///     IAnnotatedDataModelItem is implemented by model-specific base types for all types with an <see cref="Annotations" /> property. <seealso
    ///      cref="EdmDataModelItem" />
    /// </summary>
    internal interface IAnnotatedDataModelItem
    {
        /// <summary>
        ///     Gets or sets the currently assigned annotations.
        /// </summary>
        IList<DataModelAnnotation> Annotations { get; set; }
    }
}
