// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    ///     Class for representing an Association set
    /// </summary>
    public sealed class AssociationSet : RelationshipSet
    {
        /// <summary>
        ///     Initializes a new instance of AssocationSet with the given name and the association type
        /// </summary>
        /// <param name="name"> The name of the Assocation set </param>
        /// <param name="associationType"> The association type of the entities that this associationship set type contains </param>
        internal AssociationSet(string name, AssociationType associationType)
            : base(name, null, null, null, associationType)
        {
        }

        private readonly ReadOnlyMetadataCollection<AssociationSetEnd> _associationSetEnds =
            new ReadOnlyMetadataCollection<AssociationSetEnd>(new MetadataCollection<AssociationSetEnd>());

        /// <summary>
        ///     Returns the association type associated with this association set
        /// </summary>
        public new AssociationType ElementType
        {
            get { return (AssociationType)base.ElementType; }
        }

        /// <summary>
        ///     Returns the ends of the association set
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.AssociationSetEnd, true)]
        public ReadOnlyMetadataCollection<AssociationSetEnd> AssociationSetEnds
        {
            get { return _associationSetEnds; }
        }

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.AssociationSet; }
        }

        /// <summary>
        ///     Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                AssociationSetEnds.Source.SetReadOnly();
            }
        }

        /// <summary>
        ///     Adds the given end to the collection of ends
        /// </summary>
        /// <param name="associationSetEnd"> </param>
        internal void AddAssociationSetEnd(AssociationSetEnd associationSetEnd)
        {
            AssociationSetEnds.Source.Add(associationSetEnd);
        }
    }
}
