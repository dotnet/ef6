namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;

    internal sealed class FunctionImportComplexTypeMapping : FunctionImportStructuralTypeMapping
    {
        internal readonly ComplexType ReturnType;

        internal FunctionImportComplexTypeMapping(
            ComplexType returnType, Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
            : base(columnsRenameList, lineInfo)
        {
            ReturnType = returnType;
        }
    }
}