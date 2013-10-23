// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     Strongly typed DataTable for TableDetails
    /// </summary>
    internal sealed class TableDetailsRow : DataRow
    {
        private readonly TableDetailsCollection _tableTableDetails;

        [DebuggerNonUserCode]
        internal TableDetailsRow(DataRowBuilder rb)
            : base(rb)
        {
            _tableTableDetails = ((TableDetailsCollection)(base.Table));
        }

        /// <summary>
        ///     Gets a strongly typed table
        /// </summary>
        public new TableDetailsCollection Table
        {
            get { return _tableTableDetails; }
        }

        /// <summary>
        ///     Gets the Catalog column value
        /// </summary>
        public string Catalog
        {
            get
            {
                try
                {
                    return ((string)(this[_tableTableDetails.CatalogColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.CatalogColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.CatalogColumn] = value; }
        }

        /// <summary>
        ///     Gets the Schema column value
        /// </summary>
        public string Schema
        {
            get
            {
                try
                {
                    return ((string)(this[_tableTableDetails.SchemaColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.SchemaColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.SchemaColumn] = value; }
        }

        /// <summary>
        ///     Gets the TableName column value
        /// </summary>
        public string TableName
        {
            get
            {
                try
                {
                    return ((string)(this[_tableTableDetails.TableNameColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.TableNameColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.TableNameColumn] = value; }
        }

        /// <summary>
        ///     Gets the ColumnName column value
        /// </summary>
        public string ColumnName
        {
            get
            {
                try
                {
                    return ((string)(this[_tableTableDetails.ColumnNameColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.ColumnNameColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.ColumnNameColumn] = value; }
        }

        /// <summary>
        ///     Gets the IsNullable column value
        /// </summary>
        public bool IsNullable
        {
            get
            {
                try
                {
                    return ((bool)(this[_tableTableDetails.IsNullableColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.IsNullableColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.IsNullableColumn] = value; }
        }

        /// <summary>
        ///     Gets the DataType column value
        /// </summary>
        public string DataType
        {
            get
            {
                try
                {
                    return ((string)(this[_tableTableDetails.DataTypeColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.DataTypeColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.DataTypeColumn] = value; }
        }

        /// <summary>
        ///     Gets the MaximumLength column value
        /// </summary>
        public int MaximumLength
        {
            get
            {
                try
                {
                    return ((int)(this[_tableTableDetails.MaximumLengthColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.MaximumLengthColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.MaximumLengthColumn] = value; }
        }

        /// <summary>
        ///     Gets the DateTime Precision column value
        /// </summary>
        public int DateTimePrecision
        {
            get
            {
                try
                {
                    return ((int)(this[_tableTableDetails.DateTimePrecisionColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.DateTimePrecisionColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.DateTimePrecisionColumn] = value; }
        }

        /// <summary>
        ///     Gets the Precision column value
        /// </summary>
        public int Precision
        {
            get
            {
                try
                {
                    return ((int)(this[_tableTableDetails.PrecisionColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.PrecisionColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.PrecisionColumn] = value; }
        }

        /// <summary>
        ///     Gets the Scale column value
        /// </summary>
        public int Scale
        {
            get
            {
                try
                {
                    return ((int)(this[_tableTableDetails.ScaleColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.ScaleColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.ScaleColumn] = value; }
        }

        /// <summary>
        ///     Gets the IsServerGenerated column value
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                try
                {
                    return ((bool)(this[_tableTableDetails.IsIdentityColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.IsIdentityColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.IsIdentityColumn] = value; }
        }

        /// <summary>
        ///     Gets the IsServerGenerated column value
        /// </summary>
        public bool IsServerGenerated
        {
            get
            {
                try
                {
                    return ((bool)(this[_tableTableDetails.IsServerGeneratedColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.IsServerGeneratedColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.IsServerGeneratedColumn] = value; }
        }

        /// <summary>
        ///     Gets the IsPrimaryKey column value
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                try
                {
                    return ((bool)(this[_tableTableDetails.IsPrimaryKeyColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableTableDetails.IsPrimaryKeyColumn.ColumnName,
                            _tableTableDetails.TableName),
                        e);
                }
            }
            set { this[_tableTableDetails.IsPrimaryKeyColumn] = value; }
        }

        /// <summary>
        ///     Determines if the Catalog column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsCatalogNull()
        {
            return IsNull(_tableTableDetails.CatalogColumn);
        }

        /// <summary>
        ///     Determines if the Schema column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsSchemaNull()
        {
            return IsNull(_tableTableDetails.SchemaColumn);
        }

        /// <summary>
        ///     Determines if the DataType column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsDataTypeNull()
        {
            return IsNull(_tableTableDetails.DataTypeColumn);
        }

        /// <summary>
        ///     Determines if the MaximumLength column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsMaximumLengthNull()
        {
            return IsNull(_tableTableDetails.MaximumLengthColumn);
        }

        /// <summary>
        ///     Determines if the Precision column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPrecisionNull()
        {
            return IsNull(_tableTableDetails.PrecisionColumn);
        }

        /// <summary>
        ///     Determines if the DateTime Precision column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsDateTimePrecisionNull()
        {
            return IsNull(_tableTableDetails.DateTimePrecisionColumn);
        }

        /// <summary>
        ///     Determines if the Scale column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsScaleNull()
        {
            return IsNull(_tableTableDetails.ScaleColumn);
        }

        /// <summary>
        ///     Determines if the IsIdentity column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsIsIdentityNull()
        {
            return IsNull(_tableTableDetails.IsIdentityColumn);
        }

        /// <summary>
        ///     Determines if the IsIdentity column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsIsServerGeneratedNull()
        {
            return IsNull(_tableTableDetails.IsServerGeneratedColumn);
        }

        public string GetMostQualifiedTableName()
        {
            var name = string.Empty;
            if (!IsCatalogNull())
            {
                name = Catalog;
            }

            if (!IsSchemaNull())
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name += ".";
                }
                name += Schema;
            }

            if (!string.IsNullOrEmpty(name))
            {
                name += ".";
            }

            // TableName is not allowed to be null
            name += TableName;

            return name;
        }
    }
}
