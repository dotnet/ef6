// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;

    internal static class DatabaseMetadataQueryTool
    {
        internal static ICollection<EntityStoreSchemaFilterEntry> GetTablesFilterEntries(
            ModelBuilderSettings settings, DoWorkEventArgs args)
        {
            var entries = ExecuteDatabaseMetadataQuery(SelectTablesESqlQuery, EntityStoreSchemaFilterObjectTypes.Table, settings, args);
            return entries;
        }

        internal static ICollection<EntityStoreSchemaFilterEntry> GetViewFilterEntries(ModelBuilderSettings settings, DoWorkEventArgs args)
        {
            var entries = ExecuteDatabaseMetadataQuery(SelectViewESqlQuery, EntityStoreSchemaFilterObjectTypes.View, settings, args);
            return entries;
        }

        internal static ICollection<EntityStoreSchemaFilterEntry> GetFunctionsFilterEntries(
            ModelBuilderSettings settings, DoWorkEventArgs args)
        {
            ICollection<EntityStoreSchemaFilterEntry> entries;

            if (EdmFeatureManager.GetComposableFunctionImportFeatureState(settings.TargetSchemaVersion).IsEnabled())
            {
                entries = ExecuteDatabaseMetadataQuery(
                    SelectFunctionsESqlQuery, EntityStoreSchemaFilterObjectTypes.Function, settings, args);
            }
            else
            {
                entries = ExecuteDatabaseMetadataQuery(
                    SelectFunctionsESqlQueryBeforeV3, EntityStoreSchemaFilterObjectTypes.Function, settings, args);
            }

            return entries;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "The only SQL passed to this method consists of pre-defined queries over which the user has no control")]
        private static ICollection<EntityStoreSchemaFilterEntry> ExecuteDatabaseMetadataQuery(
            string esqlQuery, EntityStoreSchemaFilterObjectTypes types, ModelBuilderSettings settings, DoWorkEventArgs args)
        {
            var filterEntries = new List<EntityStoreSchemaFilterEntry>();

            EntityConnection ec = null;
            try
            {
                Version actualEntityFrameworkConnectionVersion;
                ec = new StoreSchemaConnectionFactory().Create(
                    DependencyResolver.Instance,
                    settings.RuntimeProviderInvariantName,
                    settings.DesignTimeConnectionString,
                    settings.TargetSchemaVersion,
                    out actualEntityFrameworkConnectionVersion);

                // if the provider does not support V3 and we are querying for Functions then switch to the pre-V3 query
                if (actualEntityFrameworkConnectionVersion < EntityFrameworkVersion.Version3
                    && SelectFunctionsESqlQuery.Equals(esqlQuery, StringComparison.Ordinal))
                {
                    esqlQuery = SelectFunctionsESqlQueryBeforeV3;
                }

                using (var command = new EntityCommand(null, ec, DependencyResolver.Instance))
                {
                    // NOTE:  DO NOT set the the command.CommandTimeout value.  Some providers don't support a non-zero value, and will throw (eg, SqlCE provider). 
                    // The System.Data.SqlClient's default value is 15, so we will still get a timeout for sql server. 

                    command.CommandType = CommandType.Text;
                    command.CommandText = esqlQuery;
                    ec.Open();
                    DbDataReader reader = null;
                    try
                    {
                        reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                        while (reader.Read())
                        {
                            if (args != null
                                && args.Cancel)
                            {
                                break;
                            }

                            if (reader.FieldCount == 3)
                            {
                                // the types coming back through the reader may not be a string 
                                // (eg, SqlCE returns System.DbNull for catalogName & schemaName), so cast carefully
                                var catalogName = reader.GetValue(0) as String;
                                var schemaName = reader.GetValue(1) as String;
                                var name = reader.GetValue(2) as String;

                                if (String.IsNullOrEmpty(name) == false)
                                {
                                    filterEntries.Add(
                                        new EntityStoreSchemaFilterEntry(
                                            catalogName, schemaName, name, types, EntityStoreSchemaFilterEffect.Allow));
                                }
                            }
                            else
                            {
                                Debug.Fail("Unexpected field count in reader");
                            }
                        }
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            try
                            {
                                reader.Close();
                                reader.Dispose();
                            }
                            catch (Exception)
                            {
                                Debug.Fail(
                                    "Could not close the DbDataReader in ExecuteDatabaseMetadataQuery(). If this is the result of a connection to a database file, it will leave a read lock on the file.");
                            }
                        }
                    }
                }
            }
            finally
            {
                if (ec != null)
                {
                    try
                    {
                        ec.Close();
                        ec.Dispose();
                    }
                    catch (Exception)
                    {
                        Debug.Fail(
                            "Could not close the EntityConnection in ExecuteDatabaseMetadataQuery(). If this is a connection to a database file, it will leave a read lock on the file.");
                    }
                }
            }

            return filterEntries;
        }

        private const string SelectTablesESqlQuery = @"
            SELECT 
                t.CatalogName
            ,   t.SchemaName                    
            ,   t.Name
            FROM
                SchemaInformation.Tables as t
            ORDER BY
                t.SchemaName
            ,   t.Name
            ";

        private const string SelectViewESqlQuery = @"
            SELECT 
                t.CatalogName
            ,   t.SchemaName
            ,   t.Name
            FROM
                SchemaInformation.Views as t
            ORDER BY
                t.SchemaName
            ,   t.Name
            ";

        // in the query below we select procedures & scalar-functions only.  We exclude table-valued functions. (Used for schemas before V3 - TVFs were not supported).
        private const string SelectFunctionsESqlQueryBeforeV3 = @"
            SELECT
                  sp.CatalogName
                , sp.SchemaName
                , sp.Name
            FROM
            (
                (SELECT
                    sf.CatalogName as CatalogName
                 ,  sf.SchemaName as SchemaName
                 ,  sf.Name as Name
                FROM
                    OfType(SchemaInformation.Functions, Store.ScalarFunction) as sf)

                UNION ALL

                (SELECT
                    sproc.CatalogName as CatalogName
                 ,  sproc.SchemaName as SchemaName
                 ,  sproc.Name as Name
                FROM
                    SchemaInformation.Procedures as sproc)

            ) as sp
            ORDER BY
                sp.SchemaName
            ,   sp.Name
            ";

        // in the query below we select procedures, scalar-functions and table-valued functions (used for V3+ schemas - before that TVFs were not supported)
        private const string SelectFunctionsESqlQuery = @"
            SELECT
                  sp.CatalogName
                , sp.SchemaName
                , sp.Name
            FROM
            (
                (SELECT
                    sf.CatalogName as CatalogName
                 ,  sf.SchemaName as SchemaName
                 ,  sf.Name as Name
                FROM
                    OfType(SchemaInformation.Functions, Store.ScalarFunction) as sf)

                UNION ALL

                (SELECT
                    tvf.CatalogName as CatalogName
                 ,  tvf.SchemaName as SchemaName
                 ,  tvf.Name as Name
                FROM
                    OfType(SchemaInformation.Functions, Store.TableValuedFunction) as tvf)

                UNION ALL

                (SELECT
                    sproc.CatalogName as CatalogName
                 ,  sproc.SchemaName as SchemaName
                 ,  sproc.Name as Name
                FROM
                    SchemaInformation.Procedures as sproc)

            ) as sp
            ORDER BY
                sp.SchemaName
            ,   sp.Name
            ";
    }
}
