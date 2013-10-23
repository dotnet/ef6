// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    ///     Strongly typed RelationshipDetail row
    /// </summary>
    internal sealed class RelationshipDetailsRow : DataRow
    {
        private readonly RelationshipDetailsCollection _tableRelationshipDetails;

        [DebuggerNonUserCode]
        internal RelationshipDetailsRow(DataRowBuilder rb)
            : base(rb)
        {
            _tableRelationshipDetails = ((RelationshipDetailsCollection)(base.Table));
        }

        /// <summary>
        ///     Gets a strongly typed table
        /// </summary>
        public new RelationshipDetailsCollection Table
        {
            get { return _tableRelationshipDetails; }
        }

        /// <summary>
        ///     Gets the PkCatalog column value
        /// </summary>
        public string PKCatalog
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.PKCatalogColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.PKCatalogColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.PKCatalogColumn] = value; }
        }

        /// <summary>
        ///     Gets the PkSchema column value
        /// </summary>
        public string PKSchema
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.PKSchemaColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.PKSchemaColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.PKSchemaColumn] = value; }
        }

        /// <summary>
        ///     Gets the PkTable column value
        /// </summary>
        public string PKTable
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.PKTableColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.PKTableColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.PKTableColumn] = value; }
        }

        /// <summary>
        ///     Gets the PkColumn column value
        /// </summary>
        public string PKColumn
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.PKColumnColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.PKColumnColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.PKColumnColumn] = value; }
        }

        /// <summary>
        ///     Gets the FkCatalog column value
        /// </summary>
        public string FKCatalog
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.FKCatalogColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.FKCatalogColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.FKCatalogColumn] = value; }
        }

        /// <summary>
        ///     Gets the FkSchema column value
        /// </summary>
        public string FKSchema
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.FKSchemaColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.FKSchemaColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.FKSchemaColumn] = value; }
        }

        /// <summary>
        ///     Gets the FkTable column value
        /// </summary>
        public string FKTable
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.FKTableColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.FKTableColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.FKTableColumn] = value; }
        }

        /// <summary>
        ///     Gets the FkColumn column value
        /// </summary>
        public string FKColumn
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.FKColumnColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.FKColumnColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.FKColumnColumn] = value; }
        }

        /// <summary>
        ///     Gets the Ordinal column value
        /// </summary>
        public int Ordinal
        {
            get
            {
                try
                {
                    return ((int)(this[_tableRelationshipDetails.OrdinalColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.OrdinalColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.OrdinalColumn] = value; }
        }

        /// <summary>
        ///     Gets the RelationshipName column value
        /// </summary>
        public string RelationshipName
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.RelationshipNameColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.RelationshipNameColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.RelationshipNameColumn] = value; }
        }

        /// <summary>
        ///     Gets the RelationshipName column value
        /// </summary>
        public string RelationshipId
        {
            get
            {
                try
                {
                    return ((string)(this[_tableRelationshipDetails.RelationshipIdColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.RelationshipIdColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.RelationshipIdColumn] = value; }
        }

        /// <summary>
        ///     Gets the IsCascadeDelete column value
        /// </summary>
        public bool RelationshipIsCascadeDelete
        {
            get
            {
                try
                {
                    return ((bool)(this[_tableRelationshipDetails.RelationshipIsCascadeDeleteColumn]));
                }
                catch (InvalidCastException e)
                {
                    throw new StrongTypingException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources_VersioningFacade.StronglyTypedAccessToNullValue,
                            _tableRelationshipDetails.RelationshipIsCascadeDeleteColumn.ColumnName,
                            _tableRelationshipDetails.TableName),
                        e);
                }
            }
            set { this[_tableRelationshipDetails.RelationshipIsCascadeDeleteColumn] = value; }
        }

        /// <summary>
        ///     Determines if the PkCatalog column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKCatalogNull()
        {
            return IsNull(_tableRelationshipDetails.PKCatalogColumn);
        }

        /// <summary>
        ///     Determines if the PkSchema column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKSchemaNull()
        {
            return IsNull(_tableRelationshipDetails.PKSchemaColumn);
        }

        /// <summary>
        ///     Determines if the PkTable column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKTableNull()
        {
            return IsNull(_tableRelationshipDetails.PKTableColumn);
        }

        /// <summary>
        ///     Determines if the PkColumn column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKColumnNull()
        {
            return IsNull(_tableRelationshipDetails.PKColumnColumn);
        }

        /// <summary>
        ///     Determines if the FkCatalog column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKCatalogNull()
        {
            return IsNull(_tableRelationshipDetails.FKCatalogColumn);
        }

        /// <summary>
        ///     Determines if the FkSchema column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKSchemaNull()
        {
            return IsNull(_tableRelationshipDetails.FKSchemaColumn);
        }

        /// <summary>
        ///     Determines if the FkTable column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKTableNull()
        {
            return IsNull(_tableRelationshipDetails.FKTableColumn);
        }

        /// <summary>
        ///     Determines if the FkColumn column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKColumnNull()
        {
            return IsNull(_tableRelationshipDetails.FKColumnColumn);
        }

        /// <summary>
        ///     Determines if the Ordinal column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsOrdinalNull()
        {
            return IsNull(_tableRelationshipDetails.OrdinalColumn);
        }

        /// <summary>
        ///     Determines if the RelationshipName column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsRelationshipNameNull()
        {
            return IsNull(_tableRelationshipDetails.RelationshipNameColumn);
        }

        public string GetMostQualifiedPrimaryKey()
        {
            var builder = new StringBuilder();
            var separator = string.Empty;

            if (!IsPKCatalogNull())
            {
                builder.Append(PKCatalog);
                separator = ".";
            }

            if (!IsPKSchemaNull())
            {
                builder.Append(separator);
                builder.Append(PKSchema);
                separator = ".";
            }

            // PKTable cannot be null.
            builder.Append(separator);
            builder.Append(PKTable);

            return builder.ToString();
        }

        public string GetMostQualifiedForeignKey()
        {
            var builder = new StringBuilder();
            var separator = string.Empty;

            if (!IsFKCatalogNull())
            {
                builder.Append(FKCatalog);
                separator = ".";
            }

            if (!IsFKSchemaNull())
            {
                builder.Append(separator);
                builder.Append(FKSchema);
                separator = ".";
            }

            // FKTable cannot be null.
            builder.Append(separator);
            builder.Append(FKTable);

            return builder.ToString();
        }
    }
}
