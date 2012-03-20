namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Linq;

    /// <summary>
    ///     Allows the construction and modification of a complex type in an Entity Data Model (EDM) <see cref = "EdmNamespace" /> .
    /// </summary>
    internal class EdmComplexType
        : EdmStructuralType
    {
        private readonly BackingList<EdmProperty> declaredPropertiesList = new BackingList<EdmProperty>();

        private EdmComplexType baseComplexType;
        private bool isAbstract;

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.ComplexType;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return declaredPropertiesList;
        }

        public override EdmStructuralTypeMemberCollection Members
        {
            get { return new EdmStructuralTypeMemberCollection(() => Properties, () => declaredPropertiesList); }
        }

        /// <summary>
        ///     Gets or sets the optional <see cref = "EdmComplexType" /> that indicates the base complex type of the complex type.
        /// </summary>
        public new virtual EdmComplexType BaseType
        {
            get { return baseComplexType; }
            set
            {
                base.BaseType = value;
                baseComplexType = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the complex type is abstract.
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
        ///     Gets or sets the collection of <see cref = "EdmProperty" /> instances that describe the (scalar or complex) properties of the complex type.
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
    }
}