#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Linq;

    /// <summary>
    /// Allows the construction and modification of an Entity Data Model (EDM) function import. Function imports allow a storage model function to appear in (and optionally contribute to an entity set in) an entity model.
    /// </summary>
    internal class EdmFunctionImport : EdmEntityContainerItem
    {
        private readonly BackingList<EdmFunctionParameter> parametersList = new BackingList<EdmFunctionParameter>();

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.FunctionImport;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return this.parametersList.Concat<EdmMetadataItem>(Yield(this.ReturnType));
        }

        /// <summary>
        /// Gets or sets an <see cref="EdmTypeReference"/> that specifies the type of the function's return parameter.
        /// </summary>
        public virtual EdmTypeReference ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="EdmFunctionParameter"/>s that specifies the parameters defined by the function.
        /// </summary>
        public virtual IList<EdmFunctionParameter> Parameters { get { return this.parametersList.EnsureValue(); } set { this.parametersList.SetValue(value); } }

        /// <summary>
        /// Gets or sets an optional value that indicates the entity set in which entity instances produced by the function import should be considered members.
        /// </summary>
        public virtual EdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether invocations of the imported function can be composed with other operations.
        /// </summary>
        public virtual bool IsComposable { get; set; }
    }
}

#endif