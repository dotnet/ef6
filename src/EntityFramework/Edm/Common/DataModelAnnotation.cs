// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Common
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows the construction and modification of a user-specified annotation (name-value pair) on a <see
    ///      cref="DataModelItem" /> instance.
    /// </summary>
    public class DataModelAnnotation
        : INamedDataModelItem
    {
        /// <summary>
        ///     Gets or sets an optional namespace that can be used to distinguish the annotation from others with the same <see
        ///      cref="Name" /> value.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Namespace")]
        public virtual string Namespace { get; set; }

        /// <summary>
        ///     Gets or sets the name of the annotation.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     Gets or sets the value of the annotation.
        /// </summary>
        public virtual object Value { get; set; }
    }
}
