// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Data.Sql
{
    using System;
    using System.Data;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.VisualStudio.Data.Services.RelationalObjectModel;

    internal class DataSchemaColumn : DataSchemaObject, IDataSchemaColumn
    {
        private const int MaxPrecisionValue = 38;
        private readonly IVsDataColumn _column;

        public DataSchemaColumn(DataSchemaServer server, IVsDataColumn column)
            : base(server, column)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            _column = column;
        }

        #region IDataSchemaColumn Members

        public Type UrtType
        {
            get { return _column.FrameworkDataType; }
        }

        public uint? Size
        {
            get
            {
                // If the column support "size" property and the value is valid.
                if (HasSize(UrtType)
                    && _column.Length > 0)
                {
                    return (uint?)_column.Length;
                }
                return null;
            }
        }

        public bool IsNullable
        {
            get { return _column.IsNullable; }
        }

        public DbType DbType
        {
            get { return (DbType)_column.AdoDotNetDbType; }
        }

        public uint? Precision
        {
            get
            {
                // If the column support "Precision" property and the value is valid.
                if (HasPrecision(UrtType)
                    && IsValidPrecisionValue(_column.Precision))
                {
                    return (uint?)_column.Precision;
                }
                return null;
            }
        }

        public uint? Scale
        {
            get
            {
                // If the column support "Scale" property and the value is valid.
                if (HasScale(UrtType)
                    && IsValidScaleValue(_column.Scale))
                {
                    return (uint?)_column.Scale;
                }
                return null;
            }
        }

        public int ProviderDataType
        {
            get { return _column.AdoDotNetDataType; }
        }

        public string NativeDataType
        {
            get { return _column.NativeDataType; }
        }

        #endregion

        #region Helper methods

        // <summary>
        //     Return true if the column type support/has size property
        // </summary>
        private static bool HasSize(Type type)
        {
            if (type == typeof(string)
                || type == typeof(Byte[]))
            {
                return true;
            }
            return false;
        }

        // <summary>
        //     Return true if the column type support/has precision property.
        // </summary>
        private static bool HasPrecision(Type type)
        {
            if (type == typeof(Decimal)
                || type == typeof(DateTime))
            {
                return true;
            }
            return false;
        }

        // <summary>
        //     Return true if the column type support/has scale property.
        // </summary>
        private static bool HasScale(Type type)
        {
            if (type == typeof(Decimal))
            {
                return true;
            }
            return false;
        }

        // <summary>
        //     The value has to be a positive integer value between 1 - 38
        // </summary>
        private static bool IsValidPrecisionValue(int value)
        {
            return (value > 0 && value <= MaxPrecisionValue);
        }

        // <summary>
        //     The value has to be a positive integer value less than or equal to  column precision value (including 0).
        // </summary>
        private bool IsValidScaleValue(int value)
        {
            return (value >= 0 && value <= _column.Precision);
        }

        #endregion
    }
}
