namespace System.Data.Entity.Core.Mapping
{
    internal abstract class FunctionImportReturnTypePropertyMapping
    {
        internal readonly string CMember;
        internal readonly string SColumn;
        internal readonly LineInfo LineInfo;

        internal FunctionImportReturnTypePropertyMapping(string cMember, string sColumn, LineInfo lineInfo)
        {
            CMember = cMember;
            SColumn = sColumn;
            LineInfo = lineInfo;
        }
    }
}