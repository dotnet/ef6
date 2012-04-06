//****************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//****************************************************************************

using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;
using SampleEntityFrameworkProvider;
using System.Data.Common;

namespace Microsoft.Samples.VisualStudio.Data.ExtendedProvider
{
    /// <summary>
    /// Represents a custom data object selector to supplement or replace
    /// the schema collections supplied by the .NET Framework Data Provider
    /// for SQL Server.  Many of the enumerations here are required for full
    /// support of the built in data design scenarios.
    /// </summary>
    internal class SqlObjectSelector : DataObjectSelector
    {
        #region Protected Methods

        protected override IList<string> GetRequiredRestrictions(
            string typeName, object[] parameters)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            // All types except the root require a restriction on the database
            // because it is hard to enumerate objects on SQL Server across
            // multiple databases at the same time.  Also, the method used to
            // enumerate stored procedure columns only works to retrieve
            // columns for a single stored procedure at a time.
            if (!typeName.Equals(SqlObjectTypes.Root,
                StringComparison.OrdinalIgnoreCase))
            {
                if (typeName.Equals(SqlObjectTypes.StoredProcedureColumn,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return new string[] {
                        "Database",
                        "Schema",
                        "StoredProcedure"
                    };
                }
                return new string[] {
                    "Database"
                };
            }

            return base.GetRequiredRestrictions(typeName, parameters);
        }

        protected override IVsDataReader SelectObjects(string typeName,
            object[] restrictions, string[] properties, object[] parameters)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            // Execute a SQL statement to get the property values
            SampleConnection conn = Site.GetLockedProviderObject() as SampleConnection;
            Debug.Assert(conn != null, "Invalid provider object.");
            if (conn == null)
            {
                // This should never occur
                throw new NotSupportedException();
            }
            try
            {
                // Ensure the connection is open
                if (Site.State != DataConnectionState.Open)
                {
                    Site.Open();
                }

                // Create a command object
                SampleCommand comm = conn.CreateCommand() as SampleCommand;

                // Choose and format SQL based on the type
                if (typeName.Equals(SqlObjectTypes.Root,
                        StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = rootEnumerationSql;
                }
                else if (restrictions.Length == 0 ||
                    !(restrictions[0] is string))
                {
                    throw new ArgumentException(
                        "Missing required restriction(s).");
                }
                else if (typeName.Equals(SqlObjectTypes.Index,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        indexEnumerationSql,
                        restrictions,
                        indexEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.IndexColumn,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        indexColumnEnumerationSql,
                        restrictions,
                        indexColumnEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.ForeignKey,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        foreignKeyEnumerationSql,
                        restrictions,
                        foreignKeyEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.ForeignKeyColumn,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        foreignKeyColumnEnumerationSql,
                        restrictions,
                        foreignKeyColumnEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.StoredProcedure,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        storedProcedureEnumerationSql,
                        restrictions,
                        storedProcedureEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.StoredProcedureParameter,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        storedProcedureParameterEnumerationSql,
                        restrictions,
                        storedProcedureParameterEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.StoredProcedureColumn,
                    StringComparison.OrdinalIgnoreCase))
                {
                    if (restrictions.Length < 3 ||
                        !(restrictions[0] is string) ||
                        !(restrictions[1] is string) ||
                        !(restrictions[2] is string))
                    {
                        throw new ArgumentException(
                            "Missing required restriction(s).");
                    }

                    //
                    // In order to implement stored procedure columns we
                    // execute the stored procedure in schema only mode
                    // and intepret the resulting schema table.
                    //

                    // Format the command type and text
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.CommandText = String.Format(
                        CultureInfo.CurrentCulture,
                        "[{0}].[{1}].[{2}]",
                        (restrictions[0] as string).Replace("]", "]]"),
                        (restrictions[1] as string).Replace("]", "]]"),
                        (restrictions[2] as string).Replace("]", "]]"));

                    // Get the schema of the stored procedure
                    DataTable schemaTable = null;
                    DbDataReader reader = null;
                    try
                    {
                        // SqlCommandBuilder.DeriveParameters(comm);
                        reader = comm.ExecuteReader(CommandBehavior.SchemaOnly);
                        schemaTable = reader.GetSchemaTable();
                    }
                    catch (SqlException)
                    {
                        // The DeriveParameters and GetSchemaTable calls can
                        // be flaky; catch SqlException here because we would
                        // rather return an empty result set than an error.
                    }
                    catch (InvalidOperationException)
                    {
                        // DeriveParameters sometimes throws this as well
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }

                    // Build a different data table to contain the right
                    // information (must have full identifier)
                    DataTable dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.CurrentCulture;
                    dataTable.Columns.Add("Database", typeof(string));
                    dataTable.Columns.Add("Schema", typeof(string));
                    dataTable.Columns.Add("StoredProcedure", typeof(string));
                    dataTable.Columns.Add("Name", typeof(string));
                    dataTable.Columns.Add("Ordinal", typeof(int));
                    dataTable.Columns.Add("ProviderType", typeof(int));
                    dataTable.Columns.Add("FrameworkType", typeof(Type));
                    dataTable.Columns.Add("MaxLength", typeof(int));
                    dataTable.Columns.Add("Precision", typeof(short));
                    dataTable.Columns.Add("Scale", typeof(short));
                    dataTable.Columns.Add("IsNullable", typeof(bool));

                    // Populate the data table if a schema table was returned
                    if (schemaTable != null)
                    {
                        foreach (DataRow row in schemaTable.Rows)
                        {
                            dataTable.Rows.Add(
                                restrictions[0],
                                restrictions[1],
                                restrictions[2],
                                row["ColumnName"],
                                row["ColumnOrdinal"],
                                row["ProviderType"],
                                row["DataType"],
                                row["ColumnSize"],
                                row["NumericPrecision"],
                                row["NumericScale"],
                                row["AllowDBNull"]);
                        }
                    }

                    return new AdoDotNetTableReader(dataTable);
                }
                else if (typeName.Equals(SqlObjectTypes.Function,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        functionEnumerationSql,
                        restrictions,
                        functionEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.FunctionParameter,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        functionParameterEnumerationSql,
                        restrictions,
                        functionParameterEnumerationDefaults);
                }
                else if (typeName.Equals(SqlObjectTypes.FunctionColumn,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comm.CommandText = FormatSqlString(
                        functionColumnEnumerationSql,
                        restrictions,
                        functionColumnEnumerationDefaults);
                }
                else
                {
                    throw new NotSupportedException();
                }

                return new AdoDotNetReader(comm.ExecuteReader());
            }
            finally
            {
                Site.UnlockProviderObject();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method formats a SQL string by specifying format arguments
        /// based on restrictions.  All enumerations require at least a
        /// database restriction, which is specified twice with different
        /// escape characters.  This is followed by each restriction in turn
        /// with the quote character escaped.  Where there is no restriction,
        /// a default restriction value is added to ensure the SQL statement
        /// is still valid.
        /// </summary>
        private static string FormatSqlString(string sql,
            object[] restrictions, object[] defaultRestrictions)
        {
            Debug.Assert(sql != null);
            Debug.Assert(restrictions != null);
            Debug.Assert(restrictions.Length > 0);
            Debug.Assert(restrictions[0] is string);
            Debug.Assert(defaultRestrictions != null);
            Debug.Assert(defaultRestrictions.Length >= restrictions.Length);

            object[] formatArgs = new object[defaultRestrictions.Length + 1];
            formatArgs[0] = (restrictions[0] as string).Replace("]", "]]");
            for (int i = 0; i < defaultRestrictions.Length; i++)
            {
                if (restrictions.Length > i && restrictions[i] != null)
                {
                    formatArgs[i+1] = "N'" +
                        restrictions[i].ToString().Replace("'", "''") + "'";
                }
                else
                {
                    formatArgs[i+1] = defaultRestrictions[i];
                }
            }
            return String.Format(CultureInfo.CurrentCulture, sql, formatArgs);
        }

        #endregion

        #region Private Constants

        private const string rootEnumerationSql =
            "SELECT" +
            "   [Server] = SERVERPROPERTY('ServerName')," +
            "   [Instance] = SERVERPROPERTY('InstanceName')," +
            "   [Login] = SUSER_SNAME(SUSER_SID())," +
            "   [Database] = DB_NAME()," +
            "   [User] = USER_NAME()," +
            "   [Schema] = SCHEMA_NAME()";

        private const string indexEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Table] = OBJECT_NAME(o.object_id)," +
            "   [Name] = i.name," +
            "   [IsUnique] = i.is_unique," +
            "   [IsPrimary] = i.is_primary_key" +
            " FROM" +
            "   [{0}].sys.indexes i INNER JOIN" +
            "   [{0}].sys.objects o ON i.object_id = o.object_id INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   i.type <> 0 AND" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3} AND" +
            "   i.name = {4}" +
            " ORDER BY" +
            "   1,2,3,4";
        private static string[] indexEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)",
            "i.name"
        };

        private const string indexColumnEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Table] = OBJECT_NAME(o.object_id)," +
            "   [Index] = i.name," +
            "   [Name] = c.name," +
            "   [Ordinal] = ic.key_ordinal" +
            " FROM" +
            "   [{0}].sys.index_columns ic INNER JOIN" +
            "   [{0}].sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id INNER JOIN" +
            "   [{0}].sys.indexes i ON c.object_id = i.object_id AND ic.index_id = i.index_id INNER JOIN" +
            "   [{0}].sys.objects o ON i.object_id = o.object_id INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   ic.column_id > 0 AND" +
            "   i.type <> 0 AND" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3} AND" +
            "   i.name = {4} AND" +
            "   c.name = {5}" +
            " ORDER BY" +
            "   1,2,3,4,6";
        private static string[] indexColumnEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)",
            "i.name",
            "c.name"
        };

        private const string foreignKeyEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Table] = OBJECT_NAME(o.object_id)," +
            "   [Name] = fk.name," +
            "   [ReferencedTableSchema] = SCHEMA_NAME(rk.schema_id)," +
            "   [ReferencedTableName] = OBJECT_NAME(rk.object_id)," +
            "   [UpdateAction] = fk.update_referential_action," +
            "   [DeleteAction] = fk.delete_referential_action" +
            " FROM" +
            "   [{0}].sys.foreign_keys fk INNER JOIN" +
            "   [{0}].sys.objects rk ON fk.referenced_object_id = rk.object_id INNER JOIN" +
            "   [{0}].sys.objects o ON fk.parent_object_id = o.object_id INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3} AND" +
            "   fk.name = {4}" +
            " ORDER BY" +
            "   1,2,3,4";
        private static string[] foreignKeyEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)",
            "fk.name"
        };

        private const string foreignKeyColumnEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Table] = OBJECT_NAME(o.object_id)," +
            "   [ForeignKey] = fk.name," +
            "   [Name] = fc.name," +
            "   [Ordinal] = fkc.constraint_column_id," +
            "   [ReferencedColumnName] = rc.name" +
            " FROM" +
            "   [{0}].sys.foreign_key_columns fkc INNER JOIN" +
            "   [{0}].sys.columns fc ON fkc.parent_object_id = fc.object_id AND fkc.parent_column_id = fc.column_id INNER JOIN" +
            "   [{0}].sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id INNER JOIN" +
            "   [{0}].sys.foreign_keys fk ON fkc.constraint_object_id = fk.object_id INNER JOIN" +
            "   [{0}].sys.objects rk ON fk.referenced_object_id = rk.object_id INNER JOIN" +
            "   [{0}].sys.objects o ON fk.parent_object_id = o.object_id INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3} AND" +
            "   fk.name = {4} AND" +
            "   fc.name = {5}" +
            " ORDER BY" +
            "   1,2,3,4,6";
        private static string[] foreignKeyColumnEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)",
            "fk.name",
            "fc.name"
        };

        private const string storedProcedureEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Name] = o.name" +
            " FROM" +
            "   [{0}].sys.objects o INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   o.type IN ('P', 'PC') AND" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3}" +
            " ORDER BY" +
            "   1,2,3";
        private static string[] storedProcedureEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)"
        };

        private const string storedProcedureParameterEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [StoredProcedure] = o.name," +
            "   [Name] = p.name," +
            "   [Ordinal] = p.parameter_id," +
            "   [DataType] = t.name," +
            "   [MaxLength] = CASE WHEN t.name IN (N'nchar', N'nvarchar') THEN p.max_length/2 ELSE p.max_length END," +
            "   [Precision] = p.precision," +
            "   [Scale] = p.scale," +
            "   [IsOutput] = p.is_output" +
            " FROM" +
            "   [{0}].sys.parameters p INNER JOIN" +
            "   [{0}].sys.types t ON p.system_type_id = t.user_type_id INNER JOIN" +
            "   [{0}].sys.objects o ON p.object_id = o.object_id INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   o.type IN ('P', 'PC') AND" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3} AND" +
            "   p.name = {4}" +
            " ORDER BY" +
            "   1,2,3,5";
        private static string[] storedProcedureParameterEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)",
            "p.name"
        };

        private const string functionEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Name] = o.name," +
            "   [Type] = o.type" +
            " FROM" +
            "   [{0}].sys.objects o INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   o.type IN ('AF', 'FN', 'FS', 'FT', 'IF', 'TF') AND" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3}" +
            " ORDER BY" +
            "   1,2,3";
        private static string[] functionEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)"
        };

        private const string functionParameterEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Function] = o.name," +
            "   [Name] = CASE WHEN p.parameter_id = 0 THEN N'@RETURN_VALUE' ELSE p.name END," +
            "   [Ordinal] = p.parameter_id," +
            "   [DataType] = t.name," +
            "   [MaxLength] = CASE WHEN t.name IN (N'nchar', N'nvarchar') THEN p.max_length/2 ELSE p.max_length END," +
            "   [Precision] = p.precision," +
            "   [Scale] = p.scale," +
            "   [IsOutput] = p.is_output" +
            " FROM" +
            "   [{0}].sys.parameters p INNER JOIN" +
            "   [{0}].sys.types t ON p.system_type_id = t.user_type_id INNER JOIN" +
            "   [{0}].sys.objects o ON p.object_id = o.object_id INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   o.type IN ('AF', 'FN', 'FS', 'FT', 'IF', 'TF') AND" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3} AND" +
            "   p.name = {4}" +
            " ORDER BY" +
            "   1,2,3,5";
        private static string[] functionParameterEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)",
            "p.name"
        };

        private const string functionColumnEnumerationSql =
            "SELECT" +
            "   [Database] = d.name," +
            "   [Schema] = SCHEMA_NAME(o.schema_id)," +
            "   [Function] = o.name," +
            "   [Name] = c.name," +
            "   [Ordinal] = c.column_id," +
            "   [DataType] = t.name," +
            "   [MaxLength] = CASE WHEN t.name IN (N'nchar', N'nvarchar') THEN c.max_length/2 ELSE c.max_length END," +
            "   [Precision] = c.precision," +
            "   [Scale] = c.scale" +
            " FROM" +
            "   [{0}].sys.columns c INNER JOIN" +
            "   [{0}].sys.types t ON c.system_type_id = t.user_type_id INNER JOIN" +
            "   [{0}].sys.objects o ON c.object_id = o.object_id AND o.type IN ('AF', 'FN', 'FS', 'FT', 'IF', 'TF') INNER JOIN" +
            "   master.sys.databases d ON d.name = {1}" +
            " WHERE" +
            "   SCHEMA_NAME(o.schema_id) = {2} AND" +
            "   OBJECT_NAME(o.object_id) = {3} AND" +
            "   c.name = {4}" +
            " ORDER BY" +
            "   1,2,3,5";
        private static string[] functionColumnEnumerationDefaults =
        {
            "d.name",
            "SCHEMA_NAME(o.schema_id)",
            "OBJECT_NAME(o.object_id)",
            "c.name"
        };

        #endregion
    }
}
