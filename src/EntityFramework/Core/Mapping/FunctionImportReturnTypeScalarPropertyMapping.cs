// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Maps a function import return type property to a table column.
    /// </summary>
    public sealed class FunctionImportReturnTypeScalarPropertyMapping : FunctionImportReturnTypePropertyMapping
    {
        private readonly string _propertyName;
        private readonly string _columnName;

        /// <summary>
        /// Initializes a new FunctionImportReturnTypeScalarPropertyMapping instance.
        /// </summary>
        /// <param name="propertyName">The mapped property name.</param>
        /// <param name="columnName">The mapped column name.</param>
        public FunctionImportReturnTypeScalarPropertyMapping(
            string propertyName, string columnName)
            : this(
                Check.NotNull(propertyName, "propertyName"),
                Check.NotNull(columnName, "columnName"),
                LineInfo.Empty)
        {
        }

        internal FunctionImportReturnTypeScalarPropertyMapping(
            string propertyName, string columnName, LineInfo lineInfo)
            : base(lineInfo)
        {
            DebugCheck.NotNull(propertyName);
            DebugCheck.NotNull(columnName);

            _propertyName = propertyName;
            _columnName = columnName;
        }

        /// <summary>
        /// Gets the mapped property name.
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        internal override string CMember
        {
            get { return PropertyName; }
        }

        /// <summary>
        /// Gets the mapped column name.
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
        }

        internal override string SColumn
        {
            get { return ColumnName; }
        }
    }
}
