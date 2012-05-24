namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an enumeration type that has a reference to the backing CLR type.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal sealed class ClrEnumType : EnumType
    {
        /// <summary>cached CLR type handle, allowing the Type reference to be GC'd</summary>
        private readonly RuntimeTypeHandle _type;

        private readonly string _cspaceTypeName;

        /// <summary>
        /// Initializes a new instance of ClrEnumType class with properties from the CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type to construct from.</param>
        /// <param name="cspaceNamespaceName">CSpace namespace name.</param>
        /// <param name="cspaceTypeName">CSpace type name.</param>
        internal ClrEnumType(Type clrType, string cspaceNamespaceName, string cspaceTypeName)
            : base(clrType)
        {
            Debug.Assert(clrType != null, "clrType != null");
            Debug.Assert(clrType.IsEnum, "enum type expected");
            Debug.Assert(
                !String.IsNullOrEmpty(cspaceNamespaceName) && !String.IsNullOrEmpty(cspaceTypeName),
                "Mapping information must never be null");

            _type = clrType.TypeHandle;
            _cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
        }

        /// <summary>
        /// Gets the clr type backing this enum type.
        /// </summary>
        internal override Type ClrType
        {
            get { return Type.GetTypeFromHandle(_type); }
        }

        /// <summary>
        /// Get the full CSpaceTypeName for this enum type.
        /// </summary>
        internal string CSpaceTypeName
        {
            get { return _cspaceTypeName; }
        }
    }
}
