// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     EdmModel is the top-level container for namespaces and entity containers belonging to the same logical Entity Data Model (EDM) model.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class EdmModel : MetadataItem
    {
        private readonly List<EntityContainer> containersList = new List<EntityContainer>();
        private readonly List<EdmNamespace> namespacesList = new List<EdmNamespace>();

        /// <summary>
        ///     Gets or sets an optional value that indicates the entity model version.
        /// </summary>
        public virtual double Version { get; set; }

        /// <summary>
        ///     Gets or sets the containers declared within the model.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EntityContainer> Containers
        {
            get { return containersList; }
        }

        /// <summary>
        ///     Gets or sets the namespaces declared within the model.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<EdmNamespace> Namespaces
        {
            get { return namespacesList; }
        }

        /// <summary>
        ///     Gets or sets the currently assigned name.
        /// </summary>
        public virtual string Name { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { throw new NotImplementedException(); }
        }

        internal override string Identity
        {
            get { throw new NotImplementedException(); }
        }
    }
}
