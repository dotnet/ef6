// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Threading;

    /// <summary>
    ///     Represents a end of a Association Type
    /// </summary>
    public sealed class AssociationEndMember : RelationshipEndMember
    {
        /// <summary>
        ///     Initializes a new instance of AssociationEndMember
        /// </summary>
        /// <param name="name"> name of the association end member </param>
        /// <param name="endRefType"> Ref type that this end refers to </param>
        /// <param name="multiplicity"> multiplicity of the end </param>
        internal AssociationEndMember(
            string name,
            RefType endRefType,
            RelationshipMultiplicity multiplicity)
            : base(name, endRefType, multiplicity)
        {
        }

        internal AssociationEndMember(string name, EntityType entityType)
            : base(name, new RefType(entityType), default(RelationshipMultiplicity))
        {
        }

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.AssociationEndMember; }
        }

        private Func<RelationshipManager, RelatedEnd, RelatedEnd> _getRelatedEndMethod;

        /// <summary>
        ///     cached dynamic method to set a CLR property value on a CLR instance
        /// </summary>
        internal Func<RelationshipManager, RelatedEnd, RelatedEnd> GetRelatedEnd
        {
            get { return _getRelatedEndMethod; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _getRelatedEndMethod, value, null);
            }
        }

        /// <summary>
        /// Creates a read-only AssociationEndMember instance.
        /// </summary>
        /// <param name="name">The name of the association end member.</param>
        /// <param name="endRefType">The reference type for the end.</param>
        /// <param name="multiplicity">The multiplicity of the end.</param>
        /// <param name="deleteAction">Flag that indicates the delete behavior of the end.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the instance.</param>
        /// <returns>The newly created AssociationEndMember instance.</returns>
        /// <exception cref="ArgumentException">The specified name is null or empty.</exception>
        /// <exception cref="ArgumentNullException">The specified reference type is null.</exception>
        public static AssociationEndMember Create(
            string name, 
            RefType endRefType, 
            RelationshipMultiplicity multiplicity,
            OperationAction deleteAction,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(endRefType, "endRefType");

            var instance = new AssociationEndMember(name, endRefType, multiplicity);
            instance.DeleteBehavior = deleteAction;

            if (metadataProperties != null)
            {
                instance.AddMetadataProperties(metadataProperties.ToList());
            }

            instance.SetReadOnly();

            return instance;
        }
    }
}
