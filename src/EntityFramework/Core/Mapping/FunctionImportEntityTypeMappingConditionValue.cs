// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Xml.XPath;

    /// <summary>
    /// Represents a mapping condition for the result of a function import,
    /// evaluated by comparison with a specified value.
    /// </summary>
    public sealed class FunctionImportEntityTypeMappingConditionValue : FunctionImportEntityTypeMappingCondition
    {
        private readonly object _value;

        /// <summary>
        /// Initializes a new FunctionImportEntityTypeMappingConditionValue instance.
        /// </summary>
        /// <param name="columnName">The name of the column used to evaluate the condition.</param>
        /// <param name="value">The value to compare with.</param>
        public FunctionImportEntityTypeMappingConditionValue(string columnName, object value)
            : base(Check.NotNull(columnName, "columnName"), LineInfo.Empty)
        {
            Check.NotNull(value, "value");

            _value = value;
            _convertedValues = new Memoizer<Type, object>(GetConditionValue, null);
        }

        internal FunctionImportEntityTypeMappingConditionValue(string columnName, XPathNavigator columnValue, LineInfo lineInfo)
            : base(columnName, lineInfo)
        {
            DebugCheck.NotNull(columnValue);

            _xPathValue = columnValue;
            _convertedValues = new Memoizer<Type, object>(GetConditionValue, null);
        }

        /// <summary>
        /// Gets the value used for comparison.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        private readonly XPathNavigator _xPathValue;
        private readonly Memoizer<Type, object> _convertedValues;

        internal override ValueCondition ConditionValue
        {
            get { return new ValueCondition(_value != null ? _value.ToString() : _xPathValue.Value); }
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
                            throw new EntityCommandExecutionException(
                                Strings.Mapping_FunctionImport_UnsupportedType(ColumnName, columnValueType.FullName));
                        },
                handleInvalidConditionValue:
                    () =>
                        {
                            throw new EntityCommandExecutionException(
                                Strings.Mapping_FunctionImport_ConditionValueTypeMismatch(
                                    MslConstructs.FunctionImportMappingElement, ColumnName, columnValueType.FullName));
                        });
        }

        internal object GetConditionValue(Type columnValueType, Action handleTypeNotComparable, Action handleInvalidConditionValue)
        {
            // Check that the type is supported and comparable.
            PrimitiveType primitiveType;
            if (!ClrProviderManifest.Instance.TryGetPrimitiveType(columnValueType, out primitiveType)
                ||
                !MappingItemLoader.IsTypeSupportedForCondition(primitiveType.PrimitiveTypeKind))
            {
                handleTypeNotComparable();
                return null;
            }

            if (_value != null)
            {
                if (_value.GetType() == columnValueType)
                {
                    return _value;
                }

                handleInvalidConditionValue();
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
