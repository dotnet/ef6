// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;

    internal static class DbTableMetadataExtensions
    {
        private const string TableNameAnnotation = "TableName";
        private const string KeyNamesTypeAnnotation = "KeyNamesType";

        public static void AddColumn(this EntityType table, EdmProperty column)
        {
            Contract.Requires(table != null);
            Contract.Requires(column != null);

            column.SetPreferredName(column.Name);
            column.Name = table.Properties.UniquifyName(column.Name);

            table.AddMember(column);
        }

        public static void SetConfiguration(this EntityType table, object configuration)
        {
            Contract.Requires(table != null);
            Contract.Requires(configuration != null);

            table.Annotations.SetConfiguration(configuration);
        }

        public static DatabaseName GetTableName(this EntityType table)
        {
            Contract.Requires(table != null);

            return (DatabaseName)table.Annotations.GetAnnotation(TableNameAnnotation);
        }

        public static void SetTableName(this EntityType table, DatabaseName tableName)
        {
            Contract.Requires(table != null);
            Contract.Requires(tableName != null);

            table.Annotations.SetAnnotation(TableNameAnnotation, tableName);
        }

        public static EntityType GetKeyNamesType(this EntityType table)
        {
            Contract.Requires(table != null);

            return (EntityType)table.Annotations.GetAnnotation(KeyNamesTypeAnnotation);
        }

        public static void SetKeyNamesType(this EntityType table, EntityType entityType)
        {
            Contract.Requires(table != null);
            Contract.Requires(entityType != null);

            table.Annotations.SetAnnotation(KeyNamesTypeAnnotation, entityType);
        }
    }
}
