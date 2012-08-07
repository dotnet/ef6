// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Linq;

    /// <summary>
    ///     EdmModel is the top-level container for namespaces and entity containers belonging to the same logical Entity Data Model (EDM) model.
    /// </summary>
    internal class EdmModel : EdmNamedMetadataItem
    {
        private readonly BackingList<EdmEntityContainer> containersList = new BackingList<EdmEntityContainer>();
        private readonly BackingList<EdmNamespace> namespacesList = new BackingList<EdmNamespace>();

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.Model;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return namespacesList.Concat<EdmMetadataItem>(containersList);
        }

        /// <summary>
        ///     Gets or sets an optional value that indicates the entity model version.
        /// </summary>
        public virtual double Version { get; set; }

        /// <summary>
        ///     Gets or sets the containers declared within the model.
        /// </summary>
        public virtual IList<EdmEntityContainer> Containers
        {
            get { return containersList.EnsureValue(); }
            set { containersList.SetValue(value); }
        }

        internal bool HasContainers
        {
            get { return containersList.HasValue; }
        }

        /// <summary>
        ///     Gets or sets the namespaces declared within the model.
        /// </summary>
        public virtual IList<EdmNamespace> Namespaces
        {
            get { return namespacesList.EnsureValue(); }
            set { namespacesList.SetValue(value); }
        }

        internal bool HasNamespaces
        {
            get { return namespacesList.HasValue; }
        }
    }
}
