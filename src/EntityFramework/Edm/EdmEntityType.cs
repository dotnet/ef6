namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of an entity type in an Entity Data Model (EDM) <see cref = "EdmNamespace" /> .
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    [DebuggerDisplay("{Name}")]
    internal class EdmEntityType
        : EdmStructuralType
    {
        private readonly BackingList<EdmProperty> declaredPropertiesList = new BackingList<EdmProperty>();
        private readonly BackingList<EdmProperty> declaredKeyPropertiesList = new BackingList<EdmProperty>();

        private readonly BackingList<EdmNavigationProperty> declaredNavigationPropertiesList =
            new BackingList<EdmNavigationProperty>();

        private EdmEntityType baseEntityType;
        private bool isAbstract;

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.EntityType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return declaredPropertiesList.Concat<EdmMetadataItem>(declaredNavigationPropertiesList);
        }

        public override EdmStructuralTypeMemberCollection Members
        {
            get
            {
                return new EdmStructuralTypeMemberCollection(
                    () => Properties.Concat<EdmStructuralMember>(NavigationProperties));
            }
        }

        /// <summary>
        ///     Gets or sets the optional <see cref = "EdmEntityType" /> that indicates the base entity type of the entity type.
        /// </summary>
        public new virtual EdmEntityType BaseType
        {
            get { return baseEntityType; }
            set
            {
                base.BaseType = value;
                baseEntityType = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the entity type is abstract.
        /// </summary>
        public new virtual bool IsAbstract
        {
            get { return isAbstract; }
            set
            {
                base.IsAbstract = value;
                isAbstract = value;
            }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "EdmProperty" /> s that specifies the properties declared by the entity type.
        /// </summary>
        public virtual IList<EdmProperty> DeclaredProperties
        {
            get { return declaredPropertiesList.EnsureValue(); }
            set { declaredPropertiesList.SetValue(value); }
        }

        internal bool HasDeclaredProperties
        {
            get { return declaredPropertiesList.HasValue; }
        }

        public IEnumerable<EdmProperty> Properties
        {
            get
            {
                foreach (var declaringType in this.ToHierarchy().Reverse())
                {
                    foreach (var declaredProp in declaringType.declaredPropertiesList)
                    {
                        yield return declaredProp;
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the collection of <see cref = "EdmProperty" /> s that indicates which properties from the <see cref = "DeclaredProperties" /> collection are part of the entity key.
        /// </summary>
        public virtual IList<EdmProperty> DeclaredKeyProperties
        {
            get { return declaredKeyPropertiesList.EnsureValue(); }
            set { declaredKeyPropertiesList.SetValue(value); }
        }

        internal bool HasDeclaredKeyProperties
        {
            get { return declaredKeyPropertiesList.HasValue; }
        }

        /// <summary>
        ///     Gets or sets the optional collection of <see cref = "EdmNavigationProperty" /> s that specifies the navigation properties declared by the entity type.
        /// </summary>
        public virtual IList<EdmNavigationProperty> DeclaredNavigationProperties
        {
            get { return declaredNavigationPropertiesList.EnsureValue(); }
            set { declaredNavigationPropertiesList.SetValue(value); }
        }

        internal bool HasDeclaredNavigationProperties
        {
            get { return declaredNavigationPropertiesList.HasValue; }
        }

        public IEnumerable<EdmNavigationProperty> NavigationProperties
        {
            get
            {
                foreach (var declaringType in this.ToHierarchy().Reverse())
                {
                    foreach (var declaredNavProp in declaringType.declaredNavigationPropertiesList)
                    {
                        yield return declaredNavProp;
                    }
                }
            }
        }
    }
}
