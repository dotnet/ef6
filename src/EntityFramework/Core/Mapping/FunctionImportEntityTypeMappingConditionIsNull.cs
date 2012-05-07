namespace System.Data.Entity.Core.Mapping
{
    internal sealed class FunctionImportEntityTypeMappingConditionIsNull : FunctionImportEntityTypeMappingCondition
    {
        internal FunctionImportEntityTypeMappingConditionIsNull(string columnName, bool isNull, LineInfo lineInfo)
            : base(columnName, lineInfo)
        {
            IsNull = isNull;
        }

        internal readonly bool IsNull;

        internal override ValueCondition ConditionValue
        {
            get { return IsNull ? ValueCondition.IsNull : ValueCondition.IsNotNull; }
        }

        internal override bool ColumnValueMatchesCondition(object columnValue)
        {
            var valueIsNull = null == columnValue || Convert.IsDBNull(columnValue);
            return valueIsNull == IsNull;
        }
    }
}