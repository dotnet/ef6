﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Common;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;

#if NETFRAMEWORK
    using System.Data.Entity.SqlServerCompact;
    using System.Data.SqlServerCe;
    using System.IO;
#endif

    public abstract class TestDatabase
    {
        public string ConnectionString { get; set; }
        public string ProviderName { get; protected set; }
        public string ProviderManifestToken { get; protected set; }
        public MigrationSqlGenerator SqlGenerator { get; protected set; }
        public virtual InfoContext Info { get; protected set; }

        public abstract bool Exists();

        public abstract void EnsureDatabase();

        public abstract void ResetDatabase();

        public abstract void DropDatabase();

        public abstract DbConnection CreateConnection(string connectionString);

        protected static InfoContext CreateInfoContext(DbConnection connection, bool supportsSchema = true)
        {
            var info = new InfoContext(connection, true, supportsSchema);
            info.Database.Initialize(force: false);

            return info;
        }

        public void ExecuteNonQuery(string commandText, string connectionString = null)
        {
            Execute(commandText, c => c.ExecuteNonQuery(), connectionString);
        }

        protected T ExecuteScalar<T>(string commandText, string connectionString = null)
        {
            return Execute(commandText, c => (T)c.ExecuteScalar(), connectionString);
        }

        private T Execute<T>(string commandText, Func<DbCommand, T> action, string connectionString = null)
        {
            connectionString = connectionString ?? ConnectionString;

            using (var connection = CreateConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = commandText;

                    return action(command);
                }
            }
        }
    }

    public class SqlTestDatabase : TestDatabase
    {
        private readonly string _name;

        public SqlTestDatabase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("'" + nameof(name) + "' can not be null or empty.");
            }

            _name = name;

            ConnectionString = ModelHelpers.SimpleConnectionString(name);
            ProviderName = SqlProviderServices.ProviderInvariantName;
            ProviderManifestToken = "2008";
            SqlGenerator = new SqlServerMigrationSqlGenerator();
            Info = CreateInfoContext(new SqlConnection(ConnectionString));
        }

        public override void EnsureDatabase()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    var databaseExistsSql = "SELECT Count(*) FROM sys.databases WHERE name = N'" + _name + "'";
                    var databaseExists = ExecuteScalar<int>(databaseExistsSql, ModelHelpers.SimpleConnectionString("master")) == 1;
                    if (!databaseExists)
                    {
                        var createDatabaseSql = "CREATE DATABASE [" + _name + "]";
                        ExecuteNonQuery(createDatabaseSql, ModelHelpers.SimpleConnectionString("master"));

                        // Previously invalid connections may now be valid
                        SqlConnection.ClearPool(new SqlConnection(ConnectionString));
                    }

                    ResetDatabase();
                });
        }

        public override void ResetDatabase()
        {
            ExecuteNonQuery(
                @"DECLARE @sql NVARCHAR(1024);

                  DECLARE @constraint_name NVARCHAR(256),
                          @table_schema NVARCHAR(100),
                          @table_name NVARCHAR(100);
                 
                  DECLARE constraint_cursor CURSOR FOR
                  SELECT constraint_name, table_schema, table_name
                  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                  WHERE constraint_catalog = '" + _name + @"'
                  AND constraint_type = 'FOREIGN KEY'
                 
                  OPEN constraint_cursor;
                  FETCH NEXT FROM constraint_cursor INTO @constraint_name, @table_schema, @table_name;
                  WHILE @@FETCH_STATUS = 0
                  BEGIN
                      SELECT @sql = 'ALTER TABLE [' + REPLACE(@table_schema, ']', ']]') + '].[' + REPLACE(@table_name, ']', ']]') + '] 
                                     DROP CONSTRAINT [' + REPLACE(@constraint_name, ']', ']]') + ']';
                      EXEC sp_executesql @sql; 
                      FETCH NEXT FROM constraint_cursor INTO @constraint_name, @table_schema, @table_name;
                  END
                  CLOSE constraint_cursor;
                  DEALLOCATE constraint_cursor;

                  DECLARE table_cursor CURSOR FOR
                  SELECT 'DROP TABLE [' + REPLACE(SCHEMA_NAME(schema_id), ']', ']]') + '].[' + REPLACE(object_name(object_id), ']', ']]') + '];'
                  FROM sys.objects
                  WHERE TYPE = 'U'
                  
                  OPEN table_cursor;
                  FETCH NEXT FROM table_cursor INTO @sql;
                  WHILE @@FETCH_STATUS = 0
                  BEGIN
                      EXEC sp_executesql @sql;
                      FETCH NEXT FROM table_cursor INTO @sql;
                  END
                  CLOSE table_cursor;
                  DEALLOCATE table_cursor;

                  DECLARE sproc_cursor CURSOR FOR
                  SELECT 'DROP PROCEDURE [' + REPLACE(SCHEMA_NAME(schema_id), ']', ']]') + '].[' + REPLACE(object_name(object_id), ']', ']]') + '];'
                  FROM sys.objects
                  WHERE TYPE = 'P'
                  
                  OPEN sproc_cursor;
                  FETCH NEXT FROM sproc_cursor INTO @sql;
                  WHILE @@FETCH_STATUS = 0
                  BEGIN
                      EXEC sp_executesql @sql;
                      FETCH NEXT FROM sproc_cursor INTO @sql;
                  END
                  CLOSE sproc_cursor;
                  DEALLOCATE sproc_cursor;");
        }

        public override void DropDatabase()
        {
            SqlConnection.ClearAllPools();
            if (DatabaseTestHelpers.IsSqlAzure(ConnectionString))
            {
                string azureConnectionString = ConnectionString + ";Database=Master;";
                ExecuteNonQuery(
                @"DROP DATABASE [" + _name + "]", azureConnectionString);
            }
            else
            {
                ExecuteNonQuery(
                @"ALTER DATABASE [" + _name
                + "] SET OFFLINE WITH ROLLBACK IMMEDIATE;ALTER DATABASE [" + _name
                + "] SET ONLINE;DROP DATABASE [" + _name + "]");   
            }                       
        }

        public override bool Exists()
        {
            return Database.Exists(ConnectionString);
        }

        public override DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }

#if NETFRAMEWORK
    public class SqlCeTestDatabase : TestDatabase
    {
        private readonly string _name;
        public const int NtextLength = (1 << 29) - 1;

        public SqlCeTestDatabase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("'" + name + "' can not be null or empty.");
            }

            _name = name;

            ConnectionString = ModelHelpers.SimpleCeConnectionString(name);
            ProviderName = SqlCeProviderServices.ProviderInvariantName;
            ProviderManifestToken = "4.0";
            SqlGenerator = new SqlCeMigrationSqlGenerator();
            Info = CreateInfoContext(new SqlCeConnection(ConnectionString), false);
        }

        public override InfoContext Info
        {
            get
            {
                // This is not ideal, but the SqlCe provider does not support schemas. In order
                // to map to these special schema-qualified views, we need to create wrappers for them.
                SyncInfoWrappers();

                return base.Info;
            }
            protected set { base.Info = value; }
        }

        public override void EnsureDatabase()
        {
            if (!File.Exists(_name + ".sdf"))
            {
                using (var engine = new SqlCeEngine
                                        {
                                            LocalConnectionString = ConnectionString
                                        })
                {
                    engine.CreateDatabase();
                }
            }
        }

        public override bool Exists()
        {
            return File.Exists(_name + ".sdf");
        }

        public override void ResetDatabase()
        {
            DropDatabase();
            EnsureDatabase();
        }

        public override void DropDatabase()
        {
            File.Delete(_name + ".sdf");
        }

        public override DbConnection CreateConnection(string connectionString)
        {
            return new SqlCeConnection(connectionString);
        }

        private void SyncInfoWrappers()
        {
            if (!Exists())
            {
                return;
            }

            if (!TableExists("TABLES"))
            {
                ExecuteNonQuery(
                    @"CREATE TABLE TABLES (
    TABLE_SCHEMA nvarchar(128),
    TABLE_NAME nvarchar(128),
    PRIMARY KEY (TABLE_SCHEMA, TABLE_NAME)
)");
            }

            if (!TableExists("COLUMNS"))
            {
                ExecuteNonQuery(
                    @"CREATE TABLE COLUMNS (
    TABLE_SCHEMA nvarchar(128),
    TABLE_NAME nvarchar(128),
    COLUMN_NAME nvarchar(128),
    ORDINAL_POSITION int,
    COLUMN_DEFAULT nvarchar(4000),
    IS_NULLABLE nvarchar(3),
    DATA_TYPE nvarchar(128),
    CHARACTER_MAXIMUM_LENGTH int,
    NUMERIC_PRECISION smallint,
    NUMERIC_SCALE smallint,
    DATETIME_PRECISION int,
    PRIMARY KEY (TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME)
)");
            }

            if (!TableExists("TABLE_CONSTRAINTS"))
            {
                ExecuteNonQuery(
                    @"CREATE TABLE TABLE_CONSTRAINTS (
    CONSTRAINT_SCHEMA nvarchar(128),
    CONSTRAINT_NAME nvarchar(128),
    TABLE_SCHEMA nvarchar(128),
    TABLE_NAME nvarchar(128),
    CONSTRAINT_TYPE nvarchar(128),
    PRIMARY KEY (CONSTRAINT_SCHEMA, CONSTRAINT_NAME)
)");
            }

            if (!TableExists("REFERENTIAL_CONSTRAINTS"))
            {
                ExecuteNonQuery(
                    @"CREATE TABLE REFERENTIAL_CONSTRAINTS (
    CONSTRAINT_SCHEMA nvarchar(128),
    CONSTRAINT_NAME nvarchar(128),
    UNIQUE_CONSTRAINT_SCHEMA nvarchar(128),
    UNIQUE_CONSTRAINT_NAME nvarchar(128),
    DELETE_RULE nvarchar(128),
    PRIMARY KEY (CONSTRAINT_SCHEMA, CONSTRAINT_NAME)
)");
            }

            if (!TableExists("KEY_COLUMN_USAGE"))
            {
                ExecuteNonQuery(
                    @"CREATE TABLE KEY_COLUMN_USAGE (
    CONSTRAINT_SCHEMA nvarchar(128),
    CONSTRAINT_NAME nvarchar(128),
    TABLE_SCHEMA nvarchar(128),
    TABLE_NAME nvarchar(128),
    COLUMN_NAME nvarchar(128),
    ORDINAL_POSITION int,
    PRIMARY KEY (CONSTRAINT_SCHEMA, CONSTRAINT_NAME, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME)
)");
            }

            ExecuteNonQuery("DELETE TABLES");
            ExecuteNonQuery(
                @"INSERT TABLES
SELECT '' TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES");

            ExecuteNonQuery("DELETE COLUMNS");
            ExecuteNonQuery(
                @"INSERT COLUMNS
SELECT
    '' TABLE_SCHEMA,
    TABLE_NAME,
    COLUMN_NAME,
    ORDINAL_POSITION,
    COLUMN_DEFAULT,
    IS_NULLABLE,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    DATETIME_PRECISION
FROM INFORMATION_SCHEMA.COLUMNS");

            ExecuteNonQuery("DELETE TABLE_CONSTRAINTS");
            ExecuteNonQuery(
                @"INSERT TABLE_CONSTRAINTS
SELECT '' CONSTRAINT_SCHEMA, CONSTRAINT_NAME, '' TABLE_SCHEMA, TABLE_NAME, CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS");

            ExecuteNonQuery("DELETE REFERENTIAL_CONSTRAINTS");
            ExecuteNonQuery(
                @"INSERT REFERENTIAL_CONSTRAINTS
SELECT '' CONSTRAINT_SCHEMA, CONSTRAINT_NAME, '' UNIQUE_CONSTRAINT_SCHEMA, UNIQUE_CONSTRAINT_NAME, DELETE_RULE
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS");

            ExecuteNonQuery("DELETE KEY_COLUMN_USAGE");
            ExecuteNonQuery(
                @"INSERT KEY_COLUMN_USAGE
SELECT '' CONSTRAINT_SCHEMA, CONSTRAINT_NAME, '' TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE");
        }

        private bool TableExists(string name)
        {
            return ExecuteScalar<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + name + "'") != 0;
        }
    }
#endif
}
