namespace System.Data.Entity.Core.Mapping
{
    using System.Diagnostics.Contracts;

    internal abstract class FunctionImportEntityTypeMappingCondition
    {
        protected FunctionImportEntityTypeMappingCondition(string columnName, LineInfo lineInfo)
        {
            Contract.Requires(columnName != null);

            ColumnName = columnName;
            LineInfo = lineInfo;
        }

        internal readonly string ColumnName;
        internal readonly LineInfo LineInfo;

        internal abstract ValueCondition ConditionValue { get; }

        internal abstract bool ColumnValueMatchesCondition(object columnValue);

        public override string ToString()
        {
            return ConditionValue.ToString();
        }
    }
}