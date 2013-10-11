// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents a mapping condition for the result of a function import
    /// evaluated by checking null or not null.
    /// </summary>
    public sealed class FunctionImportEntityTypeMappingConditionIsNull : FunctionImportEntityTypeMappingCondition
    {
        private readonly bool _isNull;

        /// <summary>
        /// Initializes a new FunctionImportEntityTypeMappingConditionIsNull instance.
        /// </summary>
        /// <param name="columnName">The name of the column used to evaluate the condition.</param>
        /// <param name="isNull">Flag that indicates whether a null or not null check is performed.</param>
        public FunctionImportEntityTypeMappingConditionIsNull(string columnName, bool isNull)
            : this(Check.NotNull(columnName, "columnName"), isNull, LineInfo.Empty)
        {
        }

        internal FunctionImportEntityTypeMappingConditionIsNull(string columnName, bool isNull, LineInfo lineInfo)
            : base(columnName, lineInfo)
        {
            DebugCheck.NotNull(columnName);

            _isNull = isNull;
        }

        /// <summary>
        /// Gets a flag that indicates whether a null or not null check is performed.
        /// </summary>
        public bool IsNull
        {
            get { return _isNull; }
        }

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
