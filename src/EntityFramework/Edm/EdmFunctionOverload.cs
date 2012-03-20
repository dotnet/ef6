#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Linq;

    /// <summary>
    /// Allow the construction and modification of a function overload group in an Entity Data Model (EDM) <see cref="EdmFunctionGroup"/>. Each function overload in a group must contain a unique combination of parameter and return types.
    /// </summary>
    internal class EdmFunctionOverload : EdmMetadataItem
    {
        private readonly BackingList<EdmFunctionParameter> parametersList = new BackingList<EdmFunctionParameter>();

        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.FunctionOverload;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return this.parametersList.Concat<EdmMetadataItem>(Yield(this.ReturnType));
        }

        /// <summary>
        /// Gets or sets an <see cref="EdmTypeReference"/> that specifies the type of the function overload's return parameter.
        /// </summary>
        public virtual EdmTypeReference ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="EdmFunctionParameter"/>s that specifies the parameters defined by the function overload.
        /// </summary>
        public virtual IList<EdmFunctionParameter> Parameters { get { return this.parametersList.EnsureValue(); } set { this.parametersList.SetValue(value); } }

        internal bool HasParameters { get { return this.parametersList.HasValue; } }

        /// <summary>
        /// Gets or sets an optional value that provides the function definition if the function overload is a Model Defined Function.
        /// </summary>
        public virtual string DefiningQuery { get; set; }
    }
}

#endif