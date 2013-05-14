// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

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

        private readonly ReadOnlyMetadataCollection<AssociationSetEnd> _associationSetEnds
            = new ReadOnlyMetadataCollection<AssociationSetEnd>(new MetadataCollection<AssociationSetEnd>());

        /// <summary>
        ///     Gets the association related to this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSet" />.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationType" /> object that represents the association related to this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSet" />
        ///     .
        /// </returns>
        public new AssociationType ElementType
        {
            get { return (AssociationType)base.ElementType; }
        }

        /// <summary>
        ///     Gets the ends of this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSet" />.
        /// </summary>
        /// <returns>
        ///     A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the ends of this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSet" />
        ///     .
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.AssociationSetEnd, true)]
        public ReadOnlyMetadataCollection<AssociationSetEnd> AssociationSetEnds
        {
            get { return _associationSetEnds; }
        }

        internal EntitySet SourceSet
        {
            get
            {
                var associationSetEnd = AssociationSetEnds.FirstOrDefault();

                return (associationSetEnd != null)
                           ? associationSetEnd.EntitySet
                           : null;
            }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);
                Debug.Assert(ElementType.SourceEnd != null);

                var associationSetEnd = new AssociationSetEnd(value, this, ElementType.SourceEnd);

                if (AssociationSetEnds.Count == 0)
                {
                    AddAssociationSetEnd(associationSetEnd);
                }
                else
                {
                    AssociationSetEnds.Source[0] = associationSetEnd;
                }
            }
        }

        internal EntitySet TargetSet
        {
            get
            {
                var associationSetEnd = AssociationSetEnds.ElementAtOrDefault(1);

                return (associationSetEnd != null)
                           ? associationSetEnd.EntitySet
                           : null;
            }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);
                Debug.Assert(AssociationSetEnds.Any());
                Debug.Assert(ElementType.TargetEnd != null);

                var associationSetEnd = new AssociationSetEnd(value, this, ElementType.TargetEnd);

                if (AssociationSetEnds.Count == 1)
                {
                    AddAssociationSetEnd(associationSetEnd);
                }
                else
                {
                    AssociationSetEnds.Source[1] = associationSetEnd;
                }
            }
        }

        internal AssociationEndMember SourceEnd
        {
            get
            {
                var associationSetEnd = AssociationSetEnds.FirstOrDefault();
                return
                    associationSetEnd != null
                        ? ElementType.KeyMembers.OfType<AssociationEndMember>().SingleOrDefault(e => e.Name == associationSetEnd.Name)
                        : null;
            }
        }

        internal AssociationEndMember TargetEnd
        {
            get
            {
                var associationSetEnd = AssociationSetEnds.ElementAtOrDefault(1);
                return
                    associationSetEnd != null
                        ? ElementType.KeyMembers.OfType<AssociationEndMember>().SingleOrDefault(e => e.Name == associationSetEnd.Name)
                        : null;
            }
        }

        /// <summary>
        ///     Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSet" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents built-in type kind for this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationSet" />
        ///     .
        /// </returns>
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
        internal void AddAssociationSetEnd(AssociationSetEnd associationSetEnd)
        {
            AssociationSetEnds.Source.Add(associationSetEnd);
        }

        /// <summary>
        ///     Creates a read-only AssociationSet instance from the specified parameters.
        /// </summary>
        /// <param name="name">The name of the association set.</param>
        /// <param name="type">The association type of the elements in the association set.</param>
        /// <param name="sourceSet">The entity set for the source association set end.</param>
        /// <param name="targetSet">The entity set for the target association set end.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the instance.</param>
        /// <returns>The newly created AssociationSet instance.</returns>
        /// <exception cref="System.ArgumentException">The specified name is null or empty.</exception>
        /// <exception cref="System.ArgumentNullException">The specified association type is null.</exception>
        /// <exception cref="System.ArgumentException">
        ///     The entity type of one of the ends of the specified
        ///     association type does not match the entity type of the corresponding entity set end.
        /// </exception>
        public static AssociationSet Create(
            string name,
            AssociationType type,
            EntitySet sourceSet,
            EntitySet targetSet,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(type, "type");

            if (!CheckEntitySetAgainstEndMember(sourceSet, type.SourceEnd)
                || !CheckEntitySetAgainstEndMember(targetSet, type.TargetEnd))
            {
                throw new ArgumentException(Strings.AssociationSet_EndEntityTypeMismatch);
            }

            var instance = new AssociationSet(name, type);

            if (sourceSet != null)
            {
                instance.SourceSet = sourceSet;
            }

            if (targetSet != null)
            {
                instance.TargetSet = targetSet;
            }

            if (metadataProperties != null)
            {
                instance.AddMetadataProperties(metadataProperties.ToList());
            }

            instance.SetReadOnly();

            return instance;
        }

        private static bool CheckEntitySetAgainstEndMember(EntitySet entitySet, AssociationEndMember endMember)
        {
            return (entitySet == null && endMember == null)
                || (entitySet != null && endMember != null && entitySet.ElementType == endMember.GetEntityType());
        }
    }
}
