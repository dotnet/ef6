namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Xml.XPath;

    internal sealed class FunctionImportEntityTypeMappingConditionValue : FunctionImportEntityTypeMappingCondition
    {
        internal FunctionImportEntityTypeMappingConditionValue(string columnName, XPathNavigator columnValue, LineInfo lineInfo)
            : base(columnName, lineInfo)
        {
            Contract.Requires(columnValue != null);

            _xPathValue = columnValue;
            _convertedValues = new Memoizer<Type, object>(GetConditionValue, null);
        }

        private readonly XPathNavigator _xPathValue;
        private readonly Memoizer<Type, object> _convertedValues;

        internal override ValueCondition ConditionValue
        {
            get { return new ValueCondition(_xPathValue.Value); }
        }

        internal override bool ColumnValueMatchesCondition(object columnValue)
        {
            if (null == columnValue
                || Convert.IsDBNull(columnValue))
            {
                // only FunctionImportEntityTypeMappingConditionIsNull can match a null
                // column value
                return false;
            }

            var columnValueType = columnValue.GetType();

            // check if we've interpreted this column type yet
            var conditionValue = _convertedValues.Evaluate(columnValueType);
            return ByValueEqualityComparer.Default.Equals(columnValue, conditionValue);
        }

        private object GetConditionValue(Type columnValueType)
        {
            return GetConditionValue(
                columnValueType,
                handleTypeNotComparable:
                    () =>
                        {
                            throw new EntityCommandExecutionException(Strings.Mapping_FunctionImport_UnsupportedType(ColumnName, columnValueType.FullName));
                        },
                handleInvalidConditionValue:
                    () =>
                        {
                            throw new EntityCommandExecutionException(Strings.Mapping_FunctionImport_ConditionValueTypeMismatch(
                                StorageMslConstructs.FunctionImportMappingElement, ColumnName, columnValueType.FullName));
                        });
        }

        internal object GetConditionValue(Type columnValueType, Action handleTypeNotComparable, Action handleInvalidConditionValue)
        {
            // Check that the type is supported and comparable.
            PrimitiveType primitiveType;
            if (!ClrProviderManifest.Instance.TryGetPrimitiveType(columnValueType, out primitiveType)
                ||
                !StorageMappingItemLoader.IsTypeSupportedForCondition(primitiveType.PrimitiveTypeKind))
            {
                handleTypeNotComparable();
                return null;
            }

            try
            {
                return _xPathValue.ValueAs(columnValueType);
            }
            catch (FormatException)
            {
                handleInvalidConditionValue();
                return null;
            }
        }
    }
}