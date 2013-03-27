// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Initializes a new instance of the RelationshipEndMember class
    /// </summary>
    public abstract class RelationshipEndMember : EdmMember
    {
        /// <summary>
        ///     Initializes a new instance of RelationshipEndMember
        /// </summary>
        /// <param name="name"> name of the relationship end member </param>
        /// <param name="endRefType"> Ref type that this end refers to </param>
        /// <param name="multiplicity"> The multiplicity of this relationship end </param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or endRefType arguments is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal RelationshipEndMember(
            string name,
            RefType endRefType,
            RelationshipMultiplicity multiplicity)
            : base(name,
                TypeUsage.Create(
                    endRefType, new FacetValues
                        {
                            Nullable = false
                        }))
        {
            _relationshipMultiplicity = multiplicity;
            _deleteBehavior = OperationAction.None;
        }

        private OperationAction _deleteBehavior;
        private RelationshipMultiplicity _relationshipMultiplicity;

        /// <summary>
        ///     Returns the operational behaviour for this end
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.OperationAction, true)]
        public OperationAction DeleteBehavior
        {
            get { return _deleteBehavior; }
            set
            {
                Util.ThrowIfReadOnly(this);
                _deleteBehavior = value;
            }
        }

        /// <summary>
        ///     Returns the multiplicity for this relationship end
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.RelationshipMultiplicity, false)]
        public RelationshipMultiplicity RelationshipMultiplicity
        {
            get { return _relationshipMultiplicity; }
            set
            {
                Util.ThrowIfReadOnly(this);

                _relationshipMultiplicity = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public EntityType GetEntityType()
        {
            if (TypeUsage == null)
            {
                return null;
            }

            return (EntityType)((RefType)TypeUsage.EdmType).ElementType;
        }
    }
}
