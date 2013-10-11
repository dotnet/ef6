// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents a mapping condition for a function import result.
    /// </summary>
    public abstract class FunctionImportEntityTypeMappingCondition : MappingItem
    {
        private readonly string _columnName;
        internal FunctionImportEntityTypeMappingCondition(string columnName, LineInfo lineInfo)
        {
            DebugCheck.NotNull(columnName);

            _columnName = columnName;
            LineInfo = lineInfo;
        }

        /// <summary>
        /// Gets the name of the column used to evaluate the condition.
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
        }

        internal readonly LineInfo LineInfo;

        internal abstract ValueCondition ConditionValue { get; }

        internal abstract bool ColumnValueMatchesCondition(object columnValue);

        /// <inheritdoc />
        public override string ToString()
        {
            return ConditionValue.ToString();
        }
    }
}
