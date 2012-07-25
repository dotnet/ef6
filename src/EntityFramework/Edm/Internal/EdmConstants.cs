// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Internal
{
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;

    /// <summary>
    ///     Contains constant values that apply to the EDM model, regardless of source (for CSDL specific constants see <see cref = "CsdlConstants" /> ).
    /// </summary>
    internal static class EdmConstants
    {
        internal const string ClrPrimitiveTypeNamespace = "System";
        internal const string EdmNamespace = "Edm";
        internal const string TransientNamespace = "Transient";

        internal const int Max_DecimalPrecision = 38;
        internal const int Max_DecimalScale = 38;

        internal const string Property_Annotations = "Annotations";
        internal const string Property_BaseType = "BaseType";
        internal const string Property_CollectionKind = "CollectionKind";
        internal const string Property_DefaultValue = "DefaultValue";
        internal const string Property_Documentation = "Documentation";
        internal const string Property_EndKind = "EndKind";
        internal const string Property_IsAbstract = "IsAbstract";
        internal const string Property_IsFixedLength = "IsFixedLength";
        internal const string Property_IsMaxLength = "IsMaxLength";
        internal const string Property_IsNullable = "IsNullable";
        internal const string Property_IsUnicode = "IsUnicode";
        internal const string Property_Name = "Name";
        internal const string Property_PrimitiveType = "PrimitiveType";

        internal const string Value_CollectionType = "CollectionType";
    }
}
