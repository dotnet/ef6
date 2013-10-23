// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Database
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Represents the full name of a column on a table on a database
    ///     including the schema
    /// </summary>
    internal struct DatabaseColumn
    {
        internal DatabaseObject Table;
        internal string Column;

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }

            if (typeof(DatabaseColumn) != obj.GetType())
            {
                return false;
            }
            var objAsDatabaseColumn = (DatabaseColumn)obj;

            return (Table.Equals(objAsDatabaseColumn.Table)
                    && Column == objAsDatabaseColumn.Column);
        }

        public override int GetHashCode()
        {
            var columnHashCode = (Column != null ? Column.GetHashCode() : 0);
            return Table.GetHashCode() ^ columnHashCode;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, Resources.DatabaseColumnNameFormat, Table.ToString(), Column);
        }

        internal static DatabaseColumn CreateFromProperty(Property prop)
        {
            var ses = prop.EntityType.EntitySet as StorageEntitySet;
            Debug.Assert(ses != null, "Property " + prop.ToPrettyString() + " does not have S-side EntitySet");
            var tableOrView = DatabaseObject.CreateFromEntitySet(ses);

            var column = new DatabaseColumn();
            column.Table = tableOrView;

            Debug.Assert(prop.LocalName.Value != null, "Property " + prop.ToPrettyString() + " does not have Name");
            column.Column = prop.LocalName.Value;
            return column;
        }
    }

    internal class DatabaseColumnComparer : IComparer<DatabaseColumn>
    {
        private readonly DatabaseObjectComparer _tableComparer = new DatabaseObjectComparer();

        public int Compare(DatabaseColumn x, DatabaseColumn y)
        {
            var compareTables = _tableComparer.Compare(x.Table, y.Table);
            if (compareTables != 0)
            {
                return compareTables;
            }
            else
            {
                return String.Compare(x.Column, y.Column, StringComparison.CurrentCulture);
            }
        }
    }
}
