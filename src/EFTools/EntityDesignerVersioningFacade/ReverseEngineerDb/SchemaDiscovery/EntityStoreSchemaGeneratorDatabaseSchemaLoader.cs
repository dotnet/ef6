// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Responsible for Loading Database Schema Information
    /// </summary>
    internal class EntityStoreSchemaGeneratorDatabaseSchemaLoader
    {
        private readonly EntityConnection _connection;
        private readonly Version _storeSchemaModelVersion;

        private const string ProviderNameSqlclient = "System.Data.SqlClient";

        public EntityStoreSchemaGeneratorDatabaseSchemaLoader(EntityConnection entityConnection, Version storeSchemaModelVersion)
        {
            Debug.Assert(entityConnection != null, "entityConnection != null");
            Debug.Assert(entityConnection.State == ConnectionState.Closed, "expected closed connection");
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(storeSchemaModelVersion), "invalid version");

            _connection = entityConnection;
            _storeSchemaModelVersion = storeSchemaModelVersion;
        }

        internal class QueryTraceOn9481Interceptor : DbCommandInterceptor
        {
            [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
                Justification = "The only SQL passed to this method consists of pre-defined queries over which the user has no control")]
            public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
            {
                // Enables the 9481 trace flag to deal with Sql Server 2014 performance regression
                // See Codeplex #2445
                command.CommandText = command.CommandText + " OPTION (QUERYTRACEON 9481)";
                base.ReaderExecuting(command, interceptionContext);
            }
        }

        // virtual for testing
        public virtual StoreSchemaDetails LoadStoreSchemaDetails(IList<EntityStoreSchemaFilterEntry> filters)
        {
            Debug.Assert(filters != null, "filters != null");

            QueryTraceOn9481Interceptor interceptor = null;
            try
            {
                _connection.Open();

                interceptor = CreateAndRegisterTrace9481InterceptorIfSqlServer();

                return new StoreSchemaDetails(
                    LoadTableDetails(filters),
                    LoadViewDetails(filters),
                    LoadRelationships(filters),
                    LoadFunctionDetails(filters),
                    _storeSchemaModelVersion == EntityFrameworkVersion.Version3
                        ? LoadFunctionReturnTableDetails(filters)
                        : Enumerable.Empty<TableDetailsRow>());
            }
            finally
            {
                if (interceptor != null)
                {
                    DbInterception.Remove(interceptor);
                }
                _connection.Close();
            }
        }

        private QueryTraceOn9481Interceptor CreateAndRegisterTrace9481InterceptorIfSqlServer()
        {
            QueryTraceOn9481Interceptor interceptor = null;
            var providerInvariantName = _connection.StoreProviderFactory.GetProviderInvariantName();
            if (string.CompareOrdinal(providerInvariantName, ProviderNameSqlclient) == 0)
            {
                interceptor = new QueryTraceOn9481Interceptor();
                DbInterception.Add(interceptor);
            }
            return interceptor;
        }

        // internal virtual for testing
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal virtual IEnumerable<TableDetailsRow> LoadTableDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            var table = new TableDetailsCollection();
            return LoadDataTable<TableDetailsRow>(
                TableDetailSql,
                rows =>
                rows
                    .OrderBy(r => r.Field<string>("SchemaName"))
                    .ThenBy(r => r.Field<string>("TableName"))
                    .ThenBy(r => r.Field<int>("Ordinal")),
                table,
                EntityStoreSchemaFilterObjectTypes.Table,
                filters,
                TableDetailAlias);
        }

        // internal virtual for testing
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal virtual IEnumerable<TableDetailsRow> LoadViewDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            var views = new TableDetailsCollection();
            return LoadDataTable<TableDetailsRow>(
                ViewDetailSql,
                rows =>
                rows
                    .OrderBy(r => r.Field<string>("SchemaName"))
                    .ThenBy(r => r.Field<string>("TableName"))
                    .ThenBy(r => r.Field<int>("Ordinal")),
                views,
                EntityStoreSchemaFilterObjectTypes.View,
                filters,
                ViewDetailAlias);
        }

        // internal virtual for testing
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal virtual IEnumerable<RelationshipDetailsRow> LoadRelationships(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            var table = new RelationshipDetailsCollection();
            return LoadDataTable<RelationshipDetailsRow>(
                RelationshipDetailSql,
                rows =>
                rows
                    .OrderBy(r => r.Field<string>("RelationshipName"))
                    .ThenBy(r => r.Field<string>("RelationshipId"))
                    .ThenBy(r => r.Field<int>("Ordinal")),
                table,
                EntityStoreSchemaFilterObjectTypes.Table,
                filters,
                RelationshipDetailFromTableAlias,
                RelationshipDetailToTableAlias);
        }

        // internal virtual for testing
        internal virtual IEnumerable<FunctionDetailsRowView> LoadFunctionDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            var functionDetailsRows = new List<FunctionDetailsRowView>();

            using (var command = CreateFunctionDetailsCommand(filters))
            {
                using (var reader = new FunctionDetailsReader(command, _storeSchemaModelVersion))
                {
                    while (reader.Read())
                    {
                        functionDetailsRows.Add(reader.CurrentRow);
                    }
                }
            }
            return functionDetailsRows;
        }

        // internal virtual for testing
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal virtual IEnumerable<TableDetailsRow> LoadFunctionReturnTableDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            Debug.Assert(
                _storeSchemaModelVersion >= EntityFrameworkVersion.Version3,
                "_storeSchemaModelVersion >= EntityFrameworkVersions.Version3");

            var table = new TableDetailsCollection();
            return LoadDataTable<TableDetailsRow>(
                FunctionReturnTableDetailSql,
                rows =>
                rows
                    .OrderBy(r => r.Field<string>("SchemaName"))
                    .ThenBy(r => r.Field<string>("TableName"))
                    .ThenBy(r => r.Field<int>("Ordinal")),
                table,
                EntityStoreSchemaFilterObjectTypes.Function,
                filters,
                FunctionReturnTableDetailAlias);
        }

        private IEnumerable<T> LoadDataTable<T>(
            string sql, Func<IEnumerable<T>, IEnumerable<T>> orderByFunc, DataTable table, EntityStoreSchemaFilterObjectTypes queryTypes,
            IEnumerable<EntityStoreSchemaFilterEntry> filters, params string[] filterAliases)
            where T : DataRow

        {
            using (var command = CreateFilteredCommand(sql, null, queryTypes, filters.ToList(), filterAliases))
            {
                using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    var values = new object[table.Columns.Count];
                    while (reader.Read())
                    {
                        reader.GetValues(values);
                        table.Rows.Add(values);
                    }

                    return orderByFunc(((IEnumerable<T>)table));
                }
            }
        }

        // virtual for testing
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "All SQL comes from private constant queries that the user cannot change")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal virtual EntityCommand CreateFilteredCommand(
            string sql, string orderByClause, EntityStoreSchemaFilterObjectTypes queryTypes, List<EntityStoreSchemaFilterEntry> filters,
            string[] filterAliases)
        {
            var command =
                new EntityCommand(null, _connection, DependencyResolver.Instance)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = 0
                    };

            command.CommandText =
                new EntityStoreSchemaQueryGenerator(sql, orderByClause, queryTypes, filters, filterAliases)
                    .GenerateQuery(command.Parameters);

            return command;
        }

        internal EntityCommand CreateFunctionDetailsCommand(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            Debug.Assert(filters != null, "filters != null");

            return CreateFilteredCommand(
                _storeSchemaModelVersion >= EntityFrameworkVersion.Version3
                    ? FunctionDetailsV3Query
                    : FunctionDetailsV1Query,
                FunctionOrderByClause,
                EntityStoreSchemaFilterObjectTypes.Function,
                filters.ToList(),
                new[] { FunctionDetailAlias });
        }

        private const string ViewDetailAlias = "v";
        private const string ViewDetailSql = @"
              SELECT 
                  v.CatalogName
              ,   v.SchemaName                           
              ,   v.Name
              ,   v.ColumnName
              ,   v.Ordinal
              ,   v.IsNullable
              ,   v.TypeName
              ,   v.MaxLength
              ,   v.Precision
              ,   v.DateTimePrecision
              ,   v.Scale
              ,   v.IsIdentity
              ,   v.IsStoreGenerated
              ,   CASE WHEN pk.IsPrimaryKey IS NULL THEN false ELSE pk.IsPrimaryKey END as IsPrimaryKey
            FROM (
              SELECT
                  v.CatalogName
              ,   v.SchemaName                           
              ,   v.Name
              ,   c.Id as ColumnId
              ,   c.Name as ColumnName
              ,   c.Ordinal
              ,   c.IsNullable
              ,   c.ColumnType.TypeName as TypeName
              ,   c.ColumnType.MaxLength as MaxLength
              ,   c.ColumnType.Precision as Precision
              ,   c.ColumnType.DateTimePrecision as DateTimePrecision
              ,   c.ColumnType.Scale as Scale
              ,   c.IsIdentity
              ,   c.IsStoreGenerated
              FROM
                  SchemaInformation.Views as v 
                  cross apply 
                  v.Columns as c ) as v 
            LEFT OUTER JOIN (
              SELECT 
                  true as IsPrimaryKey
                , pkc.Id
              FROM
                  OfType(SchemaInformation.ViewConstraints, Store.PrimaryKeyConstraint) as pk
                  CROSS APPLY pk.Columns as pkc) as pk
            ON v.ColumnId = pk.Id                   
             ";

        private const string TableDetailAlias = "t";
        private const string TableDetailSql = @"
              SELECT 
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   t.ColumnName
              ,   t.Ordinal
              ,   t.IsNullable
              ,   t.TypeName
              ,   t.MaxLength
              ,   t.Precision
              ,   t.DateTimePrecision
              ,   t.Scale
              ,   t.IsIdentity
              ,   t.IsStoreGenerated
              ,   CASE WHEN pk.IsPrimaryKey IS NULL THEN false ELSE pk.IsPrimaryKey END as IsPrimaryKey
            FROM (
              SELECT
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   c.Id as ColumnId
              ,   c.Name as ColumnName
              ,   c.Ordinal
              ,   c.IsNullable
              ,   c.ColumnType.TypeName as TypeName
              ,   c.ColumnType.MaxLength as MaxLength
              ,   c.ColumnType.Precision as Precision
              ,   c.ColumnType.DateTimePrecision as DateTimePrecision
              ,   c.ColumnType.Scale as Scale
              ,   c.IsIdentity
              ,   c.IsStoreGenerated
              FROM
                  SchemaInformation.Tables as t 
                  cross apply 
                  t.Columns as c ) as t 
            LEFT OUTER JOIN (
              SELECT 
                  true as IsPrimaryKey
                , pkc.Id
              FROM
                  OfType(SchemaInformation.TableConstraints, Store.PrimaryKeyConstraint) as pk
                  CROSS APPLY pk.Columns as pkc) as pk
            ON t.ColumnId = pk.Id                   
            ";

        private const string FunctionReturnTableDetailAlias = "tvf";
        private const string FunctionReturnTableDetailSql = @"
              SELECT 
                  tvf.CatalogName
              ,   tvf.SchemaName                           
              ,   tvf.Name
              ,   tvf.ColumnName
              ,   tvf.Ordinal
              ,   tvf.IsNullable
              ,   tvf.TypeName
              ,   tvf.MaxLength
              ,   tvf.Precision
              ,   tvf.DateTimePrecision
              ,   tvf.Scale
              ,   false as IsIdentity
              ,   false as IsStoreGenerated
              ,   false as IsPrimaryKey
            FROM (
              SELECT
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   c.Id as ColumnId
              ,   c.Name as ColumnName
              ,   c.Ordinal
              ,   c.IsNullable
              ,   c.ColumnType.TypeName as TypeName
              ,   c.ColumnType.MaxLength as MaxLength
              ,   c.ColumnType.Precision as Precision
              ,   c.ColumnType.DateTimePrecision as DateTimePrecision
              ,   c.ColumnType.Scale as Scale
              FROM
                  OfType(SchemaInformation.Functions, Store.TableValuedFunction) as t 
                  cross apply 
                  t.Columns as c ) as tvf
            ";

        private const string RelationshipDetailFromTableAlias = "r.FromTable";
        private const string RelationshipDetailToTableAlias = "r.ToTable";
        private const string RelationshipDetailSql = @"
              SELECT
                 r.ToTable.CatalogName as ToTableCatalog
               , r.ToTable.SchemaName as ToTableSchema
               , r.ToTable.Name as ToTableName
               , r.ToColumnName
               , r.FromTable.CatalogName as FromTableCatalog
               , r.FromTable.SchemaName as FromTableSchema
               , r.FromTable.Name as FromTableName
               , r.FromColumnName
               , r.Ordinal
               , r.RelationshipName
               , r.RelationshipId
               , r.IsCascadeDelete
              FROM (
               SELECT 
                    fks.ToColumn.Parent as ToTable
               ,    fks.ToColumn.Name as ToColumnName
               ,    c.Parent as FromTable
               ,    fks.FromColumn.Name as FromColumnName
               ,    fks.Ordinal as Ordinal
               ,    c.Name as RelationshipName
               ,    c.Id as RelationshipId
               ,    c.DeleteRule = 'CASCADE' as IsCascadeDelete
            FROM 
                OfType(SchemaInformation.TableConstraints, Store.ForeignKeyConstraint) as c,
                ( SELECT 
                   Ref(fk.Constraint) as cRef
                 ,  fk.ToColumn
                 , fk.FromColumn
                 , fk.Ordinal
                FROM
                   c.ForeignKeys as fk) as fks
                WHERE fks.cRef = Ref(c)) as r
                ";

        private const string FunctionDetailAlias = "sp";

        private const string FunctionDetailsV1Query = @"
            SELECT
                  sp.SchemaName
                , sp.Name 
                , sp.ReturnTypeName
                , sp.IsAggregate
                , sp.IsComposable
                , sp.IsBuiltIn
                , sp.IsNiladic
                , sp.ParameterName
                , sp.ParameterType
                , sp.Mode
            FROM (  
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   r.ReturnType.TypeName as ReturnTypeName
              ,   r.IsAggregate as IsAggregate
              ,   true as IsComposable
              ,   r.IsBuiltIn as IsBuiltIn
              ,   r.IsNiladic as IsNiladic
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                OfType(SchemaInformation.Functions, Store.ScalarFunction) as r 
                 OUTER APPLY
                r.Parameters as p)
            UNION ALL
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   CAST(NULL as string) as ReturnTypeName
              ,   false as IsAggregate
              ,   false as IsComposable
              ,   false as IsBuiltIn
              ,   false as IsNiladic
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                SchemaInformation.Procedures as r 
                 OUTER APPLY
                r.Parameters as p)) as sp
            ";

        private const string FunctionDetailsV3Query = @"
            Function IsTvf(f Store.Function) as (f is of (Store.TableValuedFunction))
            SELECT
                  sp.CatalogName
                , sp.SchemaName
                , sp.Name 
                , sp.ReturnTypeName
                , sp.IsAggregate
                , sp.IsComposable
                , sp.IsBuiltIn
                , sp.IsNiladic
                , sp.IsTvf
                , sp.ParameterName
                , sp.ParameterType
                , sp.Mode
            FROM (  
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   TREAT(r as Store.ScalarFunction).ReturnType.TypeName as ReturnTypeName
              ,   TREAT(r as Store.ScalarFunction).IsAggregate as IsAggregate
              ,   true as IsComposable
              ,   r.IsBuiltIn as IsBuiltIn
              ,   r.IsNiladic as IsNiladic
              ,   IsTvf(r) as IsTvf
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                SchemaInformation.Functions as r 
                 OUTER APPLY
                r.Parameters as p)
            UNION ALL
            (SELECT
                  r.CatalogName as CatalogName
              ,   r.SchemaName as SchemaName
              ,   r.Name as Name
              ,   CAST(NULL as string) as ReturnTypeName
              ,   false as IsAggregate
              ,   false as IsComposable
              ,   false as IsBuiltIn
              ,   false as IsNiladic
              ,   false as IsTvf
              ,   p.Name as ParameterName
              ,   p.ParameterType.TypeName as ParameterType
              ,   p.Mode as Mode
              ,   p.Ordinal as Ordinal
            FROM
                SchemaInformation.Procedures as r 
                 OUTER APPLY
                r.Parameters as p)) as sp
            ";

        private const string FunctionOrderByClause = @" 
            ORDER BY
                sp.SchemaName
            ,   sp.Name
            ,   sp.Ordinal
            ";
    }
}
