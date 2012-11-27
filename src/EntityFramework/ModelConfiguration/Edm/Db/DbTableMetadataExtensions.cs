// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Utilities;

    internal static class DbTableMetadataExtensions
    {
        private const string TableNameAnnotation = "TableName";
        private const string KeyNamesTypeAnnotation = "KeyNamesType";

        public static void AddColumn(this EntityType table, EdmProperty column)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(column);

            column.SetPreferredName(column.Name);
            column.Name = table.Properties.UniquifyName(column.Name);

            table.AddMember(column);
        }

        public static void SetConfiguration(this EntityType table, object configuration)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(configuration);

            table.Annotations.SetConfiguration(configuration);
        }

        public static DatabaseName GetTableName(this EntityType table)
        {
            DebugCheck.NotNull(table);

            return (DatabaseName)table.Annotations.GetAnnotation(TableNameAnnotation);
        }

        public static void SetTableName(this EntityType table, DatabaseName tableName)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(tableName);

            table.Annotations.SetAnnotation(TableNameAnnotation, tableName);
        }

        public static EntityType GetKeyNamesType(this EntityType table)
        {
            DebugCheck.NotNull(table);

            return (EntityType)table.Annotations.GetAnnotation(KeyNamesTypeAnnotation);
        }

        public static void SetKeyNamesType(this EntityType table, EntityType entityType)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(entityType);

            table.Annotations.SetAnnotation(KeyNamesTypeAnnotation, entityType);
        }
    }
}
