#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    /// <summary>
    /// Allow the construction and modification of a function group in an Entity Data Model (EDM) <see cref="EdmNamespace"/>. Function groups contain one or more overloads of a function with a specific name.
    /// </summary>
    internal class EdmFunctionGroup : EdmNamespaceItem
    {
        private readonly BackingList<EdmFunctionOverload> overloadsList = new BackingList<EdmFunctionOverload>();

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.FunctionGroup;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return this.overloadsList;
        }

        /// <summary>
        /// Gets or sets the current <see cref="EdmFunctionOverload"/>s defined for this function group.
        /// </summary>
        public virtual IList<EdmFunctionOverload> Overloads { get { return this.overloadsList.EnsureValue(); } set { this.overloadsList.SetValue(value); } }

        internal bool HasOverloads { get { return this.overloadsList.HasValue; } }
    }
}

#endif