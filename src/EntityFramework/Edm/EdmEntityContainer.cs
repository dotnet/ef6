// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of an entity container in an Entity Data Model (EDM) <see cref="EdmModel" /> .
    /// </summary>
    internal class EdmEntityContainer
        : EdmNamedMetadataItem
    {
        private readonly BackingList<EdmAssociationSet> associationSetsList = new BackingList<EdmAssociationSet>();
        private readonly BackingList<EdmEntitySet> entitySetsList = new BackingList<EdmEntitySet>();

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.EntityContainer;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return ContainerItems;
        }

        /// <summary>
        ///     Gets all <see cref="EdmEntityContainerItem" /> s declared within the namspace. Includes <see cref="EdmAssociationSet" /> s and <see
        ///      cref="EdmEntitySet" /> s.
        /// </summary>
        public IEnumerable<EdmEntityContainerItem> ContainerItems
        {
            get
            {
                return associationSetsList
                    .Concat<EdmEntityContainerItem>(entitySetsList);
            }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref="EdmAssociationSet" /> s that specifies the association sets within the container.
        /// </summary>
        public virtual IList<EdmAssociationSet> AssociationSets
        {
            get { return associationSetsList.EnsureValue(); }
            set { associationSetsList.SetValue(value); }
        }

        internal bool HasAssociationSets
        {
            get { return associationSetsList.HasValue; }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref="EdmEntitySet" /> s that specifies the entity sets within the container.
        /// </summary>
        public virtual IList<EdmEntitySet> EntitySets
        {
            get { return entitySetsList.EnsureValue(); }
            set { entitySetsList.SetValue(value); }
        }

        internal bool HasEntitySets
        {
            get { return entitySetsList.HasValue; }
        }
    }
}
