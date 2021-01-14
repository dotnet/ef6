// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.SqlServer;
    using System.Linq;

    public class SqlServerMigrationSqlGeneratorWtihCollations : SqlServerMigrationSqlGenerator
    {
        private CollationAttribute _tableCollation;

        protected override void Generate(ColumnModel column, IndentedTextWriter writer)
        {
            writer.Write(Quote(column.Name));
            writer.Write(" ");
            writer.Write(BuildColumnType(column));

            var collation = TryGetCollation(column.Annotations);
            if (collation != null && column.ClrType == typeof(string))
            {
                writer.Write(" COLLATE " + collation.CollationName + " ");
            }

            if ((column.IsNullable != null)
                && !column.IsNullable.Value)
            {
                writer.Write(" NOT NULL");
            }

            if (column.DefaultValue != null)
            {
                writer.Write(" DEFAULT ");
                writer.Write(Generate((dynamic)column.DefaultValue));
            }
            else if (!string.IsNullOrWhiteSpace(column.DefaultValueSql))
            {
                writer.Write(" DEFAULT ");
                writer.Write(column.DefaultValueSql);
            }
            else if (column.IsIdentity)
            {
                if ((column.Type == PrimitiveTypeKind.Guid)
                    && (column.DefaultValue == null))
                {
                    writer.Write(" DEFAULT " + GuidColumnDefault);
                }
                else
                {
                    writer.Write(" IDENTITY");
                }
            }
        }

        protected override void Generate(AlterColumnOperation alterColumnOperation)
        {
            var column = alterColumnOperation.Column;

            if ((column.DefaultValue != null)
                || !string.IsNullOrWhiteSpace(column.DefaultValueSql))
            {
                using (var writer = Writer())
                {
                    DropDefaultConstraint(alterColumnOperation.Table, column.Name, writer);

                    writer.Write("ALTER TABLE ");
                    writer.Write(Name(alterColumnOperation.Table));
                    writer.Write(" ADD CONSTRAINT ");
                    writer.Write(Quote("DF_" + alterColumnOperation.Table + "_" + column.Name));
                    writer.Write(" DEFAULT ");
                    writer.Write(
                        (column.DefaultValue != null)
                            ? Generate((dynamic)column.DefaultValue)
                            : column.DefaultValueSql
                        );
                    writer.Write(" FOR ");
                    writer.Write(Quote(column.Name));

                    Statement(writer);
                }
            }

            using (var writer = Writer())
            {
                writer.Write("ALTER TABLE ");
                writer.Write(Name(alterColumnOperation.Table));
                writer.Write(" ALTER COLUMN ");
                writer.Write(Quote(column.Name));
                writer.Write(" ");
                writer.Write(BuildColumnType(column));

                var collation = TryGetCollation(alterColumnOperation.Column.Annotations);
                if (collation != null && column.ClrType == typeof(string))
                {
                    writer.Write(" COLLATE " + collation.CollationName + " ");
                }

                if ((column.IsNullable != null)
                    && !column.IsNullable.Value)
                {
                    writer.Write(" NOT");
                }

                writer.Write(" NULL");

                Statement(writer);
            }
        }

        protected override void Generate(CreateTableOperation createTableOperation)
        {
            _tableCollation = createTableOperation.Annotations.ContainsKey(CollationAttribute.AnnotationName)
                ? (CollationAttribute)createTableOperation.Annotations[CollationAttribute.AnnotationName]
                : null;

            base.Generate(createTableOperation);

            _tableCollation = null;
        }

        protected override void Generate(AlterTableOperation alterTableOperation)
        {
            _tableCollation = alterTableOperation.Annotations.ContainsKey(CollationAttribute.AnnotationName)
                ? (CollationAttribute)alterTableOperation.Annotations[CollationAttribute.AnnotationName].NewValue
                : null;

            if (_tableCollation != null)
            {
                // Need to alter any column that doesn't have explicitly set collation
                foreach (var column in alterTableOperation.Columns.Where(
                    c => c.ClrType == typeof(string)
                         && !c.Annotations.ContainsKey(CollationAttribute.AnnotationName)))
                {
                    Generate(new AlterColumnOperation(alterTableOperation.Name, column, false));
                }
            }

            _tableCollation = null;
        }

        private CollationAttribute TryGetCollation(IDictionary<string, AnnotationValues> annotations)
        {
            return annotations.ContainsKey(CollationAttribute.AnnotationName)
                ? (CollationAttribute)annotations[CollationAttribute.AnnotationName].NewValue
                : _tableCollation;
        }
    }
}
