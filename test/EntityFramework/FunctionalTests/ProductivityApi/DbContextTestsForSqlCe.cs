// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// DbContext Construction tests for SqlCe
    /// </summary>
    public class DbContextTestsForSqlCe : FunctionalTestBase, IDisposable
    {
        #region Infrastructure/setup

        private readonly IDbConnectionFactory _previousConnectionFactory;

        public DbContextTestsForSqlCe()
        {
            MutableResolver.AddResolver<IDbConnectionFactory>(k => new SqlCeConnectionFactory(
                "System.Data.SqlServerCe.4.0",
                AppDomain.CurrentDomain.BaseDirectory, ""));
        }

        public void Dispose()
        {
            MutableResolver.ClearResolvers();
        }

        #endregion

        #region Positive DbContext Construction

        [Fact]
        public void Verify_DbContext_construction_for_SQLCE_when_constructed_via_parameterless_ctor()
        {
            Verify_DbContext_construction_for_SQLCE(DbContextConstructorArgumentType.Parameterless);
        }

        [Fact]
        public void Verify_DbContext_construction_for_SQLCE_when_constructed_via_DbCompiledModel_ctor()
        {
            Verify_DbContext_construction_for_SQLCE(DbContextConstructorArgumentType.DbCompiledModel);
        }

        [Fact]
        public void Verify_DbContext_construction_for_SQLCE_when_constructed_via_Connection_string_ctor()
        {
            Verify_DbContext_construction_for_SQLCE(DbContextConstructorArgumentType.ConnectionString);
        }

        [Fact]
        public void Verify_DbContext_construction_for_SQLCE_when_constructed_via_Connection_ctor()
        {
            Verify_DbContext_construction_for_SQLCE(DbContextConstructorArgumentType.Connection);
        }

        [Fact]
        public void
            Verify_DbContext_construction_for_SQLCE_when_constructed_via_Connection_string_and_DbCompiledModel_ctor()
        {
            Verify_DbContext_construction_for_SQLCE(DbContextConstructorArgumentType.ConnectionStringAndDbCompiledModel);
        }

        [Fact]
        public void Verify_DbContext_construction_for_SQLCE_when_constructed_via_Connection_and_DbCompiledModel_ctor()
        {
            Verify_DbContext_construction_for_SQLCE(DbContextConstructorArgumentType.ConnectionAndDbCompiledModel);
        }

        private void Verify_DbContext_construction_for_SQLCE(DbContextConstructorArgumentType ctorArguments)
        {
            // Act
            using (
                var context = CreateContext<SimpleModelContext>(
                    ctorArguments,
                    providerName: "System.Data.SqlServerCe.4.0"))
            {
                // Assert
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);

                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();

                Assert.Equal(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleModel.SimpleModelContext.sdf"),
                    context.Database.Connection.Database);
            }
        }

        [Fact]
        public void
            Database_Name_in_CE_is_from_App_config_if_named_connection_string_matches_convention_name_when_using_empty_constructor_on_DbContext
            ()
        {
            Database_Name_is_from_App_Config_if_convention_name_matches_named_connection_string();
        }

        [Fact]
        public void
            Database_Name_in_CE_is_from_App_Config_if_named_connection_string_matches_convention_name_when_using_named_connection_string_when_using_model_constructor_on_DbContext
            ()
        {
            Database_Name_is_from_App_Config_if_convention_name_matches_named_connection_string(
                new DbModelBuilder().Build(ProviderRegistry.SqlCe4_ProviderInfo).Compile());
        }

        private void Database_Name_is_from_App_Config_if_convention_name_matches_named_connection_string(
            DbCompiledModel model = null)
        {
            // Act 
            using (var context = model == null ? new LiveWriterContext() : new LiveWriterContext(model))
            {
                // Assert that name of database is taken from app config rather than the convention way, 
                // which is namespace qualified type name
                Assert.NotEqual("SimpleModel.LiveWriterContext", context.Database.Connection.Database);
                Assert.Equal("LiveWriterDb.sdf", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_empty_DbCompiledModel_on_SqlCe()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(
                DbCompiledModelContents.IsEmpty,
                ProviderRegistry.SqlCe4_ProviderInfo);
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_subset_DbCompiledModel_on_SqlCe()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(
                DbCompiledModelContents.IsSubset,
                ProviderRegistry.SqlCe4_ProviderInfo);
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_superset_DbCompiledModel_on_SqlCe()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(
                DbCompiledModelContents.IsSuperset,
                ProviderRegistry.SqlCe4_ProviderInfo);
        }

        [Fact]
        public void
            Sets_are_initialized_for_DbContext_constructor_when_using_DbCompiledModel_that_matches_the_context_on_SqlCe()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(
                DbCompiledModelContents.Match,
                ProviderRegistry.SqlCe4_ProviderInfo);
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_DbCompiledModel_that_doesnt_match_context_definitions_on_SqlCe
            ()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(
                DbCompiledModelContents.DontMatch,
                ProviderRegistry.SqlCe4_ProviderInfo);
        }

        [Fact]
        public void Model_Tweaking_is_ignored_when_using_model_ctor_on_DbContext_on_SqlCe()
        {
            // Arrange
            var modelBuilder = new DbModelBuilder();
            // Blog has Id as the key defined in OnModelCreating, here we tweak it to use Title as the Key
            modelBuilder.Entity<Blog>().HasKey(o => o.Title);

            // Act
            using (
                var context = new LiveWriterContext(modelBuilder.Build(ProviderRegistry.SqlCe4_ProviderInfo).Compile()))
            {
                Assert.NotNull(context.Blogs);
                context.Assert<Blog>().IsInModel();

                // Assert that Building doesnt have a composite key as defined in OnModelCreating 
                // rather has Details as the only key.
                var type = GetEntityType(context, typeof(Blog));
                Assert.True(type.KeyMembers.Count == 1);
                Assert.Equal(type.KeyMembers.First().Name, "Title");
            }
        }

        [Fact]
        public void Verify_DbContext_construction_using_connection_string_ctor_when_string_is_database_name_on_SqlCe()
        {
            Verify_DbContext_construction_using_connection_string_ctor(
                nameOrConnectionString: "DefaultCEDatabaseNameDb",
                expectedDatabaseName: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DefaultCEDatabaseNameDb.sdf"));
        }

        [Fact]
        public void Verify_DbContext_construction_using_connection_string_ctor_when_string_is_provider_connection_string_on_SqlCe()
        {
            Verify_DbContext_construction_using_connection_string_ctor(
                nameOrConnectionString: SimpleCeConnectionString<SimpleModelContextWithNoData>(),
                expectedDatabaseName:
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleModel.SimpleModelContextWithNoData.sdf"));
        }

        private void Verify_DbContext_construction_using_connection_string_ctor(
            string nameOrConnectionString,
            string expectedDatabaseName)
        {
            using (var context = new SimpleModelContextWithNoData(nameOrConnectionString))
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();

                Assert.Equal(expectedDatabaseName, context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Sets_are_initialized_but_do_not_change_model_using_name_and_model_constructor_on_DbContext_on_SqlCe()
        {
            var model = new DbModelBuilder().Build(ProviderRegistry.SqlCe4_ProviderInfo).Compile();
            using (var context = new SimpleModelContextWithNoData(DefaultDbName<EmptyContext>(), model))
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                context.Assert<Product>().IsNotInModel();
                context.Assert<Category>().IsNotInModel();
            }
        }

        [Fact]
        public void Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_defines_a_subset_of_entities_on_context_on_SqlCe(
            
            )
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.IsSubset);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_defines_a_superset_of_entities_on_context_on_SqlCe()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.IsSuperset);
        }

        [Fact]
        public void Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_matches_the_entities_on_context_on_SqlCe()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.Match);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_has_no_entities_matching_those_on_context_on_SqlCe()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.DontMatch);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_is_empty_on_SqlCe()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.IsEmpty);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_defines_a_subset_of_entities_on_context_on_SqlCe
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.IsSubset);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_defines_a_superset_of_entities_on_context_on_SqlCe
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.IsSuperset);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_matches_the_entities_on_context_on_SqlCe(
            
            )
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.Match);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_has_no_entities_matching_those_on_context_on_SqlCe
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.DontMatch);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_is_empty_on_SqlCe()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.IsEmpty);
        }

        [Fact]
        public void
            DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_defines_a_subset_of_entities_on_context_on_SqlCe
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.IsSubset);
        }

        [Fact]
        public void
            DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_defines_a_superset_of_entities_on_context_on_SqlCe
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.IsSuperset);
        }

        [Fact]
        public void DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_matches_the_entities_on_context_on_SqlCe
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.Match);
        }

        [Fact]
        public void
            DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_has_no_entities_matching_those_on_context_on_SqlCe
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.DontMatch);
        }

        private void DbContext_construction_using_connection_string_and_model_Ctor(
            ConnectionStringFormat connStringFormat, DbCompiledModelContents modelContents)
        {
            // Act
            // Setup connection string
            string connectionString = null;
            string dbName = null;
            switch (connStringFormat)
            {
                case ConnectionStringFormat.DatabaseName:
                    connectionString = dbName = DefaultDbName<SimpleModelContextWithNoData>();
                    break;
                case ConnectionStringFormat.NamedConnectionString:
                    connectionString = "Scenario_Use_SqlCe_AppConfig_connection_string";
                    break;
                case ConnectionStringFormat.ProviderConnectionString:
                    connectionString = SimpleCeConnectionString<SimpleModelContextWithNoData>();
                    break;
                default:
                    throw new ArgumentException("Invalid ConnectionStringFormat enum supplied " + connStringFormat);
            }

            // Setup DbCompiledModel
            var builder = new DbModelBuilder();

            switch (modelContents)
            {
                case DbCompiledModelContents.IsEmpty:
                    // Do nothing as builder has already been initialized
                    break;
                case DbCompiledModelContents.IsSubset:
                    // Product is not defined here
                    builder.Entity<Category>();
                    break;
                case DbCompiledModelContents.IsSuperset:
                    builder.Entity<Category>();
                    builder.Entity<Product>();
                    builder.Entity<Login>();
                    break;
                case DbCompiledModelContents.Match:
                    builder.Entity<Category>();
                    builder.Entity<Product>();
                    break;
                case DbCompiledModelContents.DontMatch:
                    builder.Entity<FeaturedProduct>();
                    builder.Entity<Login>();
                    break;
                default:
                    throw new ArgumentException("Invalid DbCompiledModelContents Arguments passed in, " + modelContents);
            }

            // Act
            using (
                var context = new SimpleModelContextWithNoData(
                    connectionString,
                    builder.Build(ProviderRegistry.SqlCe4_ProviderInfo).
                        Compile()))
            {
                // Assert
                switch (modelContents)
                {
                    case DbCompiledModelContents.IsEmpty:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsNotInModel();
                        context.Assert<Product>().IsNotInModel();
                        break;
                    case DbCompiledModelContents.IsSubset:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel(); // Reachability
                        break;
                    case DbCompiledModelContents.IsSuperset:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel();
                        context.Assert<Login>().IsInModel();
                        break;
                    case DbCompiledModelContents.Match:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel();
                        context.Assert<Login>().IsNotInModel();
                        break;
                    case DbCompiledModelContents.DontMatch:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Products);
                        context.Assert<Login>().IsInModel();
                        context.Assert<FeaturedProduct>().IsInModel();
                        break;
                    default:
                        throw new ArgumentException(
                            "Invalid DbCompiledModelContents Arguments passed in, " +
                            modelContents);
                }
            }
        }

        [Fact]
        public void DbContext_construction_using_existing_connection_and_model_constructor_on_DbContext_where_model_is_empty_on_SqlCe()
        {
            using (var connection = SimpleCeConnection<SimpleModelContextWithNoData>())
            {
                Verify_DbContext_construction_using_connection_and_model_Ctor(
                    connection,
                    DbCompiledModelContents.IsEmpty);
            }
        }

        [Fact]
        public void DbContext_construction_using_existing_connection_and_model_constructor_on_DbContext_where_model_is_a_subset_on_SqlCe()
        {
            using (var connection = SimpleCeConnection<SimpleModelContextWithNoData>())
            {
                Verify_DbContext_construction_using_connection_and_model_Ctor(
                    connection,
                    DbCompiledModelContents.IsSubset);
            }
        }

        [Fact]
        public void
            Sets_are_initialized_but_do_not_change_model_using_existing_connection_and_model_constructor_on_DbContext_where_model_is_a_superset_on_SqlCe
            ()
        {
            using (var connection = SimpleCeConnection<SimpleModelContextWithNoData>())
            {
                Verify_DbContext_construction_using_connection_and_model_Ctor(
                    connection,
                    DbCompiledModelContents.IsSuperset);
            }
        }

        [Fact]
        public void
            Sets_are_initialized_but_do_not_change_model_using_existing_connection_and_model_constructor_on_DbContext_where_model_matches_on_SqlCe
            ()
        {
            using (var connection = SimpleCeConnection<SimpleModelContextWithNoData>())
            {
                Verify_DbContext_construction_using_connection_and_model_Ctor(connection, DbCompiledModelContents.Match);
            }
        }

        [Fact]
        public void
            Sets_are_initialized_but_do_not_change_model_using_existing_connection_and_model_constructor_on_DbContext_where_model_doesnt_match_on_SqlCe
            ()
        {
            using (var connection = SimpleConnection<SimpleModelContextWithNoData>())
            {
                Verify_DbContext_construction_using_connection_and_model_Ctor(
                    connection,
                    DbCompiledModelContents.DontMatch);
            }
        }

        private void Verify_DbContext_construction_using_connection_and_model_Ctor(
            DbConnection connection,
            DbCompiledModelContents contents)
        {
            // Arrange
            // DbCompiledModel creation as appropriate for the various model content options
            var builder = new DbModelBuilder();

            switch (contents)
            {
                case DbCompiledModelContents.IsEmpty:
                    // Do nothing as builder has already been initialized
                    break;
                case DbCompiledModelContents.IsSubset:
                    // Product is not defined here
                    builder.Entity<Category>();
                    break;
                case DbCompiledModelContents.IsSuperset:
                    builder.Entity<Category>();
                    builder.Entity<Product>();
                    builder.Entity<Login>();
                    break;
                case DbCompiledModelContents.Match:
                    builder.Entity<Category>();
                    builder.Entity<Product>();
                    break;
                case DbCompiledModelContents.DontMatch:
                    builder.Entity<FeaturedProduct>();
                    builder.Entity<Login>();
                    break;
                default:
                    throw new ArgumentException("Invalid DbCompiledModelContents Arguments passed in, " + contents);
            }

            var model = builder.Build(connection).Compile();

            // Act
            using (var context = new SimpleModelContextWithNoData(connection, model))
            {
                // Verification as appropriate for the various model content options
                switch (contents)
                {
                    case DbCompiledModelContents.IsEmpty:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsNotInModel();
                        context.Assert<Product>().IsNotInModel();
                        break;
                    case DbCompiledModelContents.IsSubset:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel(); // Reachability
                        break;
                    case DbCompiledModelContents.IsSuperset:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel();
                        context.Assert<Login>().IsInModel();
                        break;
                    case DbCompiledModelContents.Match:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Categories);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel();
                        context.Assert<Login>().IsNotInModel();
                        break;
                    case DbCompiledModelContents.DontMatch:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Products);
                        context.Assert<Login>().IsInModel();
                        context.Assert<FeaturedProduct>().IsInModel();
                        break;
                    default:
                        throw new ArgumentException("Invalid DbCompiledModelContents Arguments passed in, " + contents);
                }
            }
        }

        #endregion

        #region Dispose Semantics of the various constructors for SQL CE

        [Fact]
        public void DbContext_parameterless_ctor_will_dispose_the_underlying_object_context_and_connection_on_SqlCe()
        {
            DbContext_disposal_behavior_wrt_to_object_context_and_connection(
                DbContextConstructorArgumentType.Parameterless);
        }

        [Fact]
        public void DbContext_constructed_with_DbCompiledModel_ctor_will_dispose_the_underlying_object_context_and_connection_on_SqlCe()
        {
            DbContext_disposal_behavior_wrt_to_object_context_and_connection(
                DbContextConstructorArgumentType.DbCompiledModel);
        }

        [Fact]
        public void DbContext_constructed_with_connection_string_ctor_will_dispose_underlying_object_context_and_connection_on_SqlCe()
        {
            DbContext_disposal_behavior_wrt_to_object_context_and_connection(
                DbContextConstructorArgumentType.ConnectionString);
        }

        [Fact]
        public void DbContext_constructed_with_connection_ctor_will_dispose_underlying_object_context_but_not_the_connection_on_SqlCe()
        {
            DbContext_disposal_behavior_wrt_to_object_context_and_connection(DbContextConstructorArgumentType.Connection);
        }

        [Fact]
        public void
            DbContext_constructed_with_connection_and_DbCompiledModel_ctor_will_dispose_underlying_object_context_but_not_the_connection_on_SqlCe
            ()
        {
            DbContext_disposal_behavior_wrt_to_object_context_and_connection(
                DbContextConstructorArgumentType.ConnectionAndDbCompiledModel);
        }

        [Fact]
        public void
            DbContext_constructed_with_connection_string_and_DbCompiledModel_ctor_will_dispose_underlying_object_context_and_the_connection_on_SqlCe
            ()
        {
            DbContext_disposal_behavior_wrt_to_object_context_and_connection(
                DbContextConstructorArgumentType.ConnectionStringAndDbCompiledModel);
        }

        private void DbContext_disposal_behavior_wrt_to_object_context_and_connection(
            DbContextConstructorArgumentType ctorArguments)
        {
            ObjectContext objectContext = null;
            DbConnection storeConnection = null;
            using (
                var context =
                    CreateContext<SimpleModelContext>(ctorArguments, providerName: "System.Data.SqlServerCe.4.0"))
            {
                // Arrange
                var product = context.Products.Find(1);
                Assert.NotNull(product);
                objectContext = GetObjectContext(context);
                storeConnection = context.Database.Connection;
            }

            // Assert
            if (ctorArguments != DbContextConstructorArgumentType.ObjectContext)
            {
                // Assert object context is disposed
                Assert.Throws<ObjectDisposedException>(() => objectContext.SaveChanges()).ValidateMessage(
                    "ObjectContext_ObjectDisposed");
            }

            if (ctorArguments.Equals(DbContextConstructorArgumentType.Connection)
                ||
                ctorArguments.Equals(DbContextConstructorArgumentType.ConnectionAndDbCompiledModel))
            {
                // Assert that connection is closed but not disposed
                Assert.True(
                    storeConnection.State == ConnectionState.Closed &&
                    !storeConnection.ConnectionString.Equals(string.Empty));
            }
            else
            {
                // Assert connection is disposed
                Assert.True(
                    storeConnection.State == ConnectionState.Closed &&
                    storeConnection.ConnectionString.Equals(string.Empty));
            }
        }

        #endregion

        #region Negative DbContext Construction

        [Fact]
        public void DbContext_construction_does_not_throw_but_subsequent_calls_using_connection_throw_for_invalid_SqlCE_connection_string()
        {
            var sqlCeAssembly =
                new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0").CreateConnection("Dummy").GetType().Assembly;
            var context = new SimpleModelContextWithNoData("Data Sourc=Scenario_Use_AppConfig.sdf");
            Assert.Throws<ArgumentException>(() => GetObjectContext(context)).ValidateMessage(
                sqlCeAssembly,
                "ADP_KeywordNotSupported",
                "System.Data.SqlServerCe",
                "data sourc");
        }

        #endregion
    }
}
