
#if IncludeUnusedEdmCode

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;

    /// <summary>
    /// Allows the construction and modification of a parameter to an Entity Data Model (EDM) function, including <see cref="EdmFunctionOverload"/> and <see cref="EdmFunctionImport"/>.
    /// </summary>
    internal class EdmFunctionParameter : EdmNamedMetadataItem
    {
        internal override EdmItemKind GetItemKind()
        {
            return EdmItemKind.FunctionParameter;
        }

        protected override IEnumerable<EdmMetadataItem> GetChildItems()
        {
            return Yield(this.ParameterType);
        }

        /// <summary>
        /// Gets or sets an <see cref="EdmTypeReference"/> that specifies the parameter type.
        /// </summary>
        public virtual EdmTypeReference ParameterType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the <see cref="EdmParameterMode"/> of the parameter.
        /// </summary>
        public virtual EdmParameterMode Mode { get; set; }
    }
}

#endif
