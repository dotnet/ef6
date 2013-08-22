// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlServerCe;
    using System.Linq;

    public class InfoContext : DbContext
    {
        private readonly bool _supportsSchema;

        static InfoContext()
        {
            Database.SetInitializer<InfoContext>(null);
        }

        public InfoContext(DbConnection connection, bool supportsSchema = true)
            : base(connection, true)
        {
            _supportsSchema = supportsSchema;
        }

        public DbQuery<TableInfo> Tables
        {
            get { return Set<TableInfo>().AsNoTracking(); }
        }

        public DbQuery<ColumnInfo> Columns
        {
            get { return Set<ColumnInfo>().AsNoTracking(); }
        }

        public DbQuery<TableConstraintInfo> TableConstraints
        {
            get { return Set<TableConstraintInfo>().AsNoTracking(); }
        }

        public DbQuery<KeyColumnUsageInfo> KeyColumnUsages
        {
            get { return Set<KeyColumnUsageInfo>().AsNoTracking(); }
        }

        public bool ColumnExists(string tableName, string columnName)
        {
            var tuple = ParseTableName(tableName);
            var candidates = Columns.Where(c => c.Table.Name == tuple.Item2 && c.Name == columnName).Include(c => c.Table).ToList();

            if (!candidates.Any())
            {
                return false;
            }

            return candidates.Any(c => SchemaEquals(tuple.Item1, c.Table));
        }

        public int GetColumnIndex(string tableName, string columnName)
        {
            var tuple = ParseTableName(tableName);
            var columnNames = Columns.Where(c => c.Table.Name == tuple.Item2).Select(c => c.Name).ToList();
            return columnNames.IndexOf(columnName);
        }

        public bool TableExists(string name)
        {
            var tuple = ParseTableName(name);
            var candidates = Tables.Where(t => t.Name == tuple.Item2).ToList();

            if (!candidates.Any())
            {
                return false;
            }

            return candidates.Any(t => SchemaEquals(tuple.Item1, t));
        }

        public override int SaveChanges()
        {
            throw new InvalidOperationException("This context is read-only.");
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var table = modelBuilder.Entity<TableInfo>();
            table.ToTable("TABLES", "INFORMATION_SCHEMA");
            table.Property(t => t.Schema).HasColumnName("TABLE_SCHEMA");
            table.Property(t => t.Name).HasColumnName("TABLE_NAME");
            table.HasKey(
                t => new
                         {
                             t.Schema,
                             t.Name
                         });
            table.HasMany(t => t.Columns).WithRequired(c => c.Table).HasForeignKey(
                c => new
                         {
                             c.TableSchema,
                             c.TableName
                         });
            table.HasMany(t => t.Constraints).WithRequired(tc => tc.Table).HasForeignKey(
                c => new
                         {
                             c.TableSchema,
                             c.TableName
                         });

            var column = modelBuilder.Entity<ColumnInfo>();
            column.ToTable("COLUMNS", "INFORMATION_SCHEMA");
            column.Property(c => c.TableSchema).HasColumnName("TABLE_SCHEMA");
            column.Property(c => c.TableName).HasColumnName("TABLE_NAME");
            column.Property(c => c.Name).HasColumnName("COLUMN_NAME");
            column.Property(c => c.Position).HasColumnName("ORDINAL_POSITION");
            column.Property(c => c.Default).HasColumnName("COLUMN_DEFAULT");
            column.Property(c => c.IsNullable).HasColumnName("IS_NULLABLE");
            column.Property(c => c.Type).HasColumnName("DATA_TYPE");
            column.Property(c => c.MaxLength).HasColumnName("CHARACTER_MAXIMUM_LENGTH");
            column.Property(c => c.NumericPrecision).HasColumnName("NUMERIC_PRECISION");
            column.Property(c => c.Scale).HasColumnName("NUMERIC_SCALE");
            column.Property(c => c.DateTimePrecision).HasColumnName("DATETIME_PRECISION");
            column.HasKey(
                c => new
                         {
                             c.TableSchema,
                             c.TableName,
                             c.Name
                         });

            var tableConstraint = modelBuilder.Entity<TableConstraintInfo>();
            tableConstraint.ToTable("TABLE_CONSTRAINTS", "INFORMATION_SCHEMA");
            tableConstraint.Property(tc => tc.Schema).HasColumnName("CONSTRAINT_SCHEMA");
            tableConstraint.Property(tc => tc.Name).HasColumnName("CONSTRAINT_NAME");
            tableConstraint.Property(tc => tc.TableSchema).HasColumnName("TABLE_SCHEMA");
            tableConstraint.Property(tc => tc.TableName).HasColumnName("TABLE_NAME");
            tableConstraint.HasKey(
                tc => new
                          {
                              tc.Schema,
                              tc.Name
                          });

            var uniqueConstraint = modelBuilder.Entity<UniqueConstraintInfo>();
            uniqueConstraint.Map(m => m.Requires("CONSTRAINT_TYPE").HasValue("UNIQUE"));

            var primaryKeyConstraint = modelBuilder.Entity<PrimaryKeyConstraintInfo>();
            primaryKeyConstraint.Map(m => m.Requires("CONSTRAINT_TYPE").HasValue("PRIMARY KEY"));

            var foreignKeyConstraint = modelBuilder.Entity<ForeignKeyConstraintInfo>();
            foreignKeyConstraint.Map(m => m.Requires("CONSTRAINT_TYPE").HasValue("FOREIGN KEY"));

            var referentialConstraint = modelBuilder.Entity<ReferentialConstraintInfo>();
            referentialConstraint.ToTable("REFERENTIAL_CONSTRAINTS", "INFORMATION_SCHEMA");
            referentialConstraint.Property(rc => rc.UniqueConstraintSchema).HasColumnName("UNIQUE_CONSTRAINT_SCHEMA");
            referentialConstraint.Property(rc => rc.UniqueConstraintName).HasColumnName("UNIQUE_CONSTRAINT_NAME");
            referentialConstraint.Property(rc => rc.DeleteRule).HasColumnName("DELETE_RULE");
            referentialConstraint.HasRequired(rc => rc.UniqueConstraint).WithMany(uc => uc.ReferentialConstraints).HasForeignKey(
                rc => new
                          {
                              rc.UniqueConstraintSchema,
                              rc.UniqueConstraintName
                          });

            var keyColumnUsage = modelBuilder.Entity<KeyColumnUsageInfo>();
            keyColumnUsage.ToTable("KEY_COLUMN_USAGE", "INFORMATION_SCHEMA");
            keyColumnUsage.Property(kcu => kcu.ConstraintSchema).HasColumnName("CONSTRAINT_SCHEMA");
            keyColumnUsage.Property(kcu => kcu.ConstraintName).HasColumnName("CONSTRAINT_NAME");
            keyColumnUsage.Property(kcu => kcu.ColumnTableSchema).HasColumnName("TABLE_SCHEMA");
            keyColumnUsage.Property(kcu => kcu.ColumnTableName).HasColumnName("TABLE_NAME");
            keyColumnUsage.Property(kcu => kcu.ColumnName).HasColumnName("COLUMN_NAME");
            keyColumnUsage.Property(kcu => kcu.Position).HasColumnName("ORDINAL_POSITION");
            keyColumnUsage.HasKey(
                kcu => new
                           {
                               kcu.ConstraintSchema,
                               kcu.ConstraintName,
                               kcu.ColumnTableSchema,
                               kcu.ColumnTableName,
                               kcu.ColumnName
                           });
            keyColumnUsage.HasRequired(kcu => kcu.Constraint).WithMany(kc => kc.KeyColumnUsages).HasForeignKey(
                kcu => new
                           {
                               kcu.ConstraintSchema,
                               kcu.ConstraintName
                           });
            keyColumnUsage.HasRequired(kcu => kcu.Column).WithMany(c => c.KeyColumnUsages).HasForeignKey(
                kcu => new
                           {
                               kcu.ColumnTableSchema,
                               kcu.ColumnTableName,
                               kcu.ColumnName
                           });

            if (Database.Connection is SqlCeConnection)
            {
                column.Property(c => c.NumericPrecision).HasColumnType("smallint");
                column.Property(c => c.DateTimePrecision).HasColumnType("int");
            }
            else
            {
                column.Property(c => c.Scale).HasColumnType("int");
            }
        }

        private static Tuple<string, string> ParseTableName(string name)
        {
            var lastDot = name.LastIndexOf('.');

            if (lastDot == -1)
            {
                return new Tuple<string, string>(null, name);
            }

            return new Tuple<string, string>(
                name.Substring(0, lastDot),
                name.Substring(lastDot + 1));
        }

        private bool SchemaEquals(string schema, TableInfo table)
        {
            if (!_supportsSchema
                || string.IsNullOrWhiteSpace(schema))
            {
                return true;
            }

            return table.Schema == schema;
        }
    }

    #region Entity types

    public class TableInfo
    {
        public TableInfo()
        {
            Columns = new HashSet<ColumnInfo>();
            Constraints = new HashSet<TableConstraintInfo>();
        }

        public string Schema { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ColumnInfo> Columns { get; protected set; }
        public virtual ICollection<TableConstraintInfo> Constraints { get; protected set; }
    }

    public class ColumnInfo
    {
        public ColumnInfo()
        {
            KeyColumnUsages = new HashSet<KeyColumnUsageInfo>();
        }

        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public virtual TableInfo Table { get; set; }

        public string Name { get; set; }
        public int Position { get; set; }
        public string Default { get; set; }
        public string IsNullable { get; set; }
        public string Type { get; set; }
        public int? MaxLength { get; set; }
        public byte? NumericPrecision { get; set; }
        public short? Scale { get; set; }
        public short? DateTimePrecision { get; set; }

        public virtual ICollection<KeyColumnUsageInfo> KeyColumnUsages { get; protected set; }
    }

    public class TableConstraintInfo
    {
        public string Schema { get; set; }
        public string Name { get; set; }

        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public virtual TableInfo Table { get; set; }
    }

    public abstract class KeyConstraintInfo : TableConstraintInfo
    {
        public KeyConstraintInfo()
        {
            KeyColumnUsages = new HashSet<KeyColumnUsageInfo>();
        }

        public virtual ICollection<KeyColumnUsageInfo> KeyColumnUsages { get; protected set; }
    }

    public abstract class UniqueConstraintInfoBase : KeyConstraintInfo
    {
        protected UniqueConstraintInfoBase()
        {
            ReferentialConstraints = new HashSet<ReferentialConstraintInfo>();
        }

        public virtual ICollection<ReferentialConstraintInfo> ReferentialConstraints { get; protected set; }
    }

    public class UniqueConstraintInfo : UniqueConstraintInfoBase
    {
    }

    public class PrimaryKeyConstraintInfo : UniqueConstraintInfoBase
    {
    }

    public abstract class ForeignKeyConstraintInfo : KeyConstraintInfo
    {
    }

    public class ReferentialConstraintInfo : ForeignKeyConstraintInfo
    {
        public string UniqueConstraintSchema { get; set; }
        public string UniqueConstraintName { get; set; }
        public virtual UniqueConstraintInfoBase UniqueConstraint { get; set; }

        public string DeleteRule { get; set; }
    }

    public class KeyColumnUsageInfo
    {
        public string ConstraintSchema { get; set; }
        public string ConstraintName { get; set; }
        public KeyConstraintInfo Constraint { get; set; }

        public string ColumnTableSchema { get; set; }
        public string ColumnTableName { get; set; }
        public string ColumnName { get; set; }
        public ColumnInfo Column { get; set; }

        public int Position { get; set; }
    }

    #endregion
}
