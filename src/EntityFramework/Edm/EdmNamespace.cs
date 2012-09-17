// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of a namespace in an <see cref="EdmModel" /> .
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class EdmNamespace : EdmQualifiedNameMetadataItem
    {
        private readonly BackingList<EdmAssociationType> associationTypesList = new BackingList<EdmAssociationType>();
        private readonly BackingList<EdmComplexType> complexTypesList = new BackingList<EdmComplexType>();
        private readonly BackingList<EdmEntityType> entityTypesList = new BackingList<EdmEntityType>();
        private readonly BackingList<EdmEnumType> enumTypesList = new BackingList<EdmEnumType>();

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.Namespace;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return NamespaceItems;
        }

        /// <summary>
        ///     Gets all <see cref="EdmNamespaceItem" /> s declared within the namspace. Includes <see cref="EdmAssociationType" /> s, <see
        ///      cref="EdmComplexType" /> s, <see cref="EdmEntityType" /> s.
        /// </summary>
        public IEnumerable<EdmNamespaceItem> NamespaceItems
        {
            get
            {
                return associationTypesList
                    .Concat<EdmNamespaceItem>(complexTypesList)
                    .Concat(entityTypesList)
                    .Concat(enumTypesList);
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="EdmAssociationType" /> s declared within the namespace.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EdmAssociationType> AssociationTypes
        {
            get { return associationTypesList.EnsureValue(); }
            set { associationTypesList.SetValue(value); }
        }

        internal bool HasAssociationTypes
        {
            get { return associationTypesList.HasValue; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="EdmComplexType" /> s declared within the namespace.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EdmComplexType> ComplexTypes
        {
            get { return complexTypesList.EnsureValue(); }
            set { complexTypesList.SetValue(value); }
        }

        internal bool HasComplexTypes
        {
            get { return complexTypesList.HasValue; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="EdmEntityType" /> s declared within the namespace.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EdmEntityType> EntityTypes
        {
            get { return entityTypesList.EnsureValue(); }
            set { entityTypesList.SetValue(value); }
        }

        internal bool HasEntityTypes
        {
            get { return entityTypesList.HasValue; }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EdmEnumType> EnumTypes
        {
            get { return enumTypesList.EnsureValue(); }
            set { enumTypesList.SetValue(value); }
        }

        internal bool HasEnumTypes
        {
            get { return enumTypesList.HasValue; }
        }
    }
}
