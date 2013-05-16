// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Represents the edm member class
    /// </summary>
    public abstract class EdmMember : MetadataItem, INamedDataModelItem
    {
        private StructuralType _declaringType;
        private TypeUsage _typeUsage;
        private string _name;
        private string _identity;

        internal EdmMember()
        {
            // for testing
        }

        /// <summary>
        ///     Initializes a new instance of EdmMember class
        /// </summary>
        /// <param name="name"> name of the member </param>
        /// <param name="memberTypeUsage"> type information containing info about member's type and its facet </param>
        internal EdmMember(string name, TypeUsage memberTypeUsage)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(memberTypeUsage, "memberTypeUsage");

            _name = name;
            _typeUsage = memberTypeUsage;
        }

        string INamedDataModelItem.Identity
        {
            get { return Identity; }
        }

        /// <summary>
        ///     Returns the identity of the member
        /// </summary>
        internal override string Identity
        {
            get { return _identity ?? Name; }
        }

        /// <summary>Gets the name of this member.</summary>
        /// <returns>The name of this member.</returns>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public virtual string Name
        {
            get { return _name; }
            set
            {
                Check.NotEmpty(value, "value");
                Util.ThrowIfReadOnly(this);

                if (!string.Equals(_name, value, StringComparison.Ordinal))
                {
                    _name = value;

                    if (_declaringType != null)
                    {
                        if (_declaringType
                            .Members.Except(new[] { this })
                            .Any(c => string.Equals(Identity, c.Identity, StringComparison.Ordinal)))
                        {
                            // Duplicate configured name, uniquify the identity so that
                            // a validation exception can be generated later on. For valid
                            // models, we sync it back up in SetReadOnly()
                            _identity = _declaringType.Members.Select(i => i.Identity).Uniquify(Identity);
                        }

                        _declaringType.NotifyItemIdentityChanged();
                    }
                }
            }
        }

        /// <summary>Gets the type on which this member is declared.</summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.StructuralType" /> object that represents the type on which this member is declared.
        /// </returns>
        public virtual StructuralType DeclaringType
        {
            get { return _declaringType; }
        }

        /// <summary>
        ///     Gets the instance of the <see cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" /> class that contains both the type of the member and facets for the type.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" /> object that contains both the type of the member and facets for the type.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
        public virtual TypeUsage TypeUsage
        {
            get { return _typeUsage; }
            protected set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                _typeUsage = value;
            }
        }

        /// <summary>Returns the name of this member.</summary>
        /// <returns>The name of this member.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     Sets the member to read only mode. Once this is done, there are no changes
        ///     that can be done to this class
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();

                var currentIdentity = _identity;
                _identity = Name;

                if (_declaringType != null
                    && currentIdentity != null
                    && !string.Equals(currentIdentity, _identity, StringComparison.Ordinal))
                {
                    _declaringType.NotifyItemIdentityChanged();
                }

                // TypeUsage is always readonly, no need to set it
            }
        }

        /// <summary>
        ///     Change the declaring type without doing fixup in the member collection
        /// </summary>
        internal void ChangeDeclaringTypeWithoutCollectionFixup(StructuralType newDeclaringType)
        {
            _declaringType = newDeclaringType;
        }

        /// <summary>
        ///     Tells whether this member is marked as a Computed member in the EDM definition
        /// </summary>
        internal bool IsStoreGeneratedComputed
        {
            get
            {
                Facet item;
                if (TypeUsage.Facets.TryGetValue(EdmProviderManifest.StoreGeneratedPatternFacetName, false, out item))
                {
                    return ((StoreGeneratedPattern)item.Value) == StoreGeneratedPattern.Computed;
                }

                return false;
            }
        }

        /// <summary>
        ///     Tells whether this member's Store generated pattern is marked as Identity in the EDM definition
        /// </summary>
        public bool IsStoreGeneratedIdentity
        {
            get
            {
                Facet item;
                if (TypeUsage.Facets.TryGetValue(EdmProviderManifest.StoreGeneratedPatternFacetName, false, out item))
                {
                    return ((StoreGeneratedPattern)item.Value) == StoreGeneratedPattern.Identity;
                }

                return false;
            }
        }

        internal virtual bool IsPrimaryKeyColumn
        {
            get
            {
                var entityTypeBase = _declaringType as EntityTypeBase;

                return (entityTypeBase != null)
                       && entityTypeBase.KeyMembers.Contains(this);
            }
        }
    }
}
