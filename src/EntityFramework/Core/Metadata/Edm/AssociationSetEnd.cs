// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Class representing a AssociationSet End
    /// </summary>
    public sealed class AssociationSetEnd : MetadataItem
    {
        /// <summary>
        ///     Initializes a new instance of AssocationSetEnd
        /// </summary>
        /// <param name="entitySet"> Entity set that this end refers to </param>
        /// <param name="parentSet"> The association set which this belongs to </param>
        /// <param name="endMember"> The end member of the association set which this is an instance of </param>
        /// <exception cref="System.ArgumentNullException">Thrown if either the role,entitySet, parentSet or endMember arguments are null</exception>
        internal AssociationSetEnd(EntitySet entitySet, AssociationSet parentSet, AssociationEndMember endMember)
        {
            _entitySet = Check.NotNull(entitySet, "entitySet");
            _parentSet = Check.NotNull(parentSet, "parentSet");
            _endMember = Check.NotNull(endMember, "endMember");
        }

        private readonly EntitySet _entitySet;
        private readonly AssociationSet _parentSet;
        private readonly AssociationEndMember _endMember;

        /// <summary>
        ///     Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />
        ///     .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.AssociationSetEnd; }
        }

        /// <summary>
        ///     Gets the parent association set of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSet" /> object that represents the parent association set of this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />
        ///     .
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if Setter is called when the AssociationSetEnd instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.AssociationSet, false)]
        public AssociationSet ParentAssociationSet
        {
            get { return _parentSet; }
        }

        /// <summary>
        ///     Gets the End member that this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" /> object corresponds to.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationEndMember" /> object that represents the End member that this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />
        ///     object corresponds to.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if Setter is called when the AssociationSetEnd instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.AssociationEndMember, false)]
        public AssociationEndMember CorrespondingAssociationEndMember
        {
            get { return _endMember; }
        }

        /// <summary>
        ///     Gets the name of the End for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </summary>
        /// <returns>
        ///     The name of the End for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </returns>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public string Name
        {
            get { return CorrespondingAssociationEndMember.Name; }
        }

        /// <summary>
        ///     Gets the name of the End role for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </summary>
        /// <returns>
        ///     The name of the End role for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown if Setter is called when the AssociationSetEnd instance is in ReadOnly state</exception>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        [Obsolete("This property is going away, please use the Name property instead")]
        public string Role
        {
            get { return Name; }
        }

        /// <summary>Gets the entity set referenced by this End role. </summary>
        /// <returns>
        ///     An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntitySet" /> object that represents the entity set referred by this End role.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EntitySet, false)]
        public EntitySet EntitySet
        {
            get { return _entitySet; }
        }

        /// <summary>
        ///     Gets the identity of this item
        /// </summary>
        internal override string Identity
        {
            get { return Name; }
        }

        /// <summary>
        ///     Returns the name of the End role for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </summary>
        /// <returns>
        ///     The name of the End role for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSetEnd" />.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();

                var parentAssociationSet = ParentAssociationSet;
                if (parentAssociationSet != null)
                {
                    parentAssociationSet.SetReadOnly();
                }

                var endMember = CorrespondingAssociationEndMember;
                if (endMember != null)
                {
                    endMember.SetReadOnly();
                }

                var entitySet = EntitySet;
                if (entitySet != null)
                {
                    entitySet.SetReadOnly();
                }
            }
        }
    }
}
