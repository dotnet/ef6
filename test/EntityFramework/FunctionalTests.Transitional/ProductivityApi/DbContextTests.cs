// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Validation;
    using System.Globalization;
    using System.Linq;
    using System.Transactions;
    using AdvancedPatternsModel;
    using AllTypeKeysModel;
    using ConcurrencyModel;
    using DaFunc;
    using SimpleModel;
    using Xunit;
    using Xunit.Extensions;

    /// <summary>
    ///     Tests for the primary methods on DbContext.
    /// </summary>
    public class DbContextTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public DbContextTests()
        {
            CreateMetadataFilesForSimpleModel();
        }

        #endregion

        #region Positive constructor tests

        [Fact]
        public void Sets_are_initialized_using_empty_constructor_on_DbContext()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        [Fact]
        public void Sets_are_initialized_using_name_constructor_on_DbContext()
        {
            using (var context = new SimpleModelContext(DefaultDbName<SimpleModelContext>()))
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        [Fact]
        public void Sets_are_initialized_but_do_not_change_model_using_name_and_model_constructor_on_DbContext()
        {
            var model = new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();
            using (var context = new SimpleModelContextWithNoData(DefaultDbName<EmptyContext>(), model))
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                context.Assert<Product>().IsNotInModel();
                context.Assert<Category>().IsNotInModel();
            }
        }

        [Fact]
        public void Sets_are_initialized_using_existing_connection_constructor_on_DbContext()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                using (var context = new SimpleModelContext(connection))
                {
                    Assert.NotNull(context.Products);
                    Assert.NotNull(context.Categories);
                    context.Assert<Product>().IsInModel();
                    context.Assert<Category>().IsInModel();
                }
            }
        }

        [Fact]
        public void
            Sets_are_initialized_but_do_not_change_model_using_existing_connection_and_model_constructor_on_DbContext()
        {
            var model = new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();
            using (var connection = SimpleConnection<EmptyContext>())
            {
                using (var context = new SimpleModelContextWithNoData(connection, model))
                {
                    Assert.NotNull(context.Products);
                    Assert.NotNull(context.Categories);
                    context.Assert<Product>().IsNotInModel();
                    context.Assert<Category>().IsNotInModel();
                }
            }
        }

        [Fact]
        public void Sets_are_initialized_using_object_context_constructor_on_DbContext()
        {
            using (var context = new SimpleModelContext())
            {
                using (var context2 = new SimpleModelContext(GetObjectContext(context)))
                {
                    Assert.NotNull(context2.Products);
                    Assert.NotNull(context2.Categories);
                    context.Assert<Product>().IsInModel();
                    context.Assert<Category>().IsInModel();
                }
            }
        }

        [Fact]
        public void Database_Name_is_formed_by_convention_ie_namespace_qualified_class_name_when_using_empty_constructor_on_DbContext()
        {
            Database_Name_is_formed_by_convention_ie_namespace_qualified_class_name();
        }

        [Fact]
        public void Database_Name_is_formed_by_convention_ie_namespace_qualified_class_name_when_using_model_constructor_on_DbContext()
        {
            // Arrange
            var builder = SimpleModelContext.CreateBuilder();

            // Act- Assert
            Database_Name_is_formed_by_convention_ie_namespace_qualified_class_name(
                builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile());
        }

        private void Database_Name_is_formed_by_convention_ie_namespace_qualified_class_name(
            DbCompiledModel model = null)
        {
            // Act
            using (var context = model == null ? new SimpleModelContext() : new SimpleModelContext(model))
            {
                // Assert
                Assert.Equal("SimpleModel.SimpleModelContext", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void
            Database_Name_is_from_App_Config_if_convention_name_matches_named_connection_string_when_using_model_constructor_on_DbContext()
        {
            var builder = AllTypeKeysContext.CreateBuilder();
            Database_Name_is_from_App_Config_if_convention_name_matches_named_connection_string(
                builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile());
        }

        [Fact]
        public void
            Database_Name_is_from_App_config_if_named_connection_string_matches_convention_name_when_using_empty_constructor_on_DbContext()
        {
            Database_Name_is_from_App_Config_if_convention_name_matches_named_connection_string();
        }

        private void Database_Name_is_from_App_Config_if_convention_name_matches_named_connection_string(
            DbCompiledModel model = null)
        {
            // Act 
            using (var context = model == null ? new AllTypeKeysContext() : new AllTypeKeysContext(model))
            {
                // Assert that name of database is taken from app config rather than the convention way,
                // namespace qualified type name
                Assert.NotEqual("AllTypeKeysModel.AllTypeKeysContext", context.Database.Connection.Database);
                Assert.Equal("AllTypeKeysDb", context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_empty_DbCompiledModel()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(DbCompiledModelContents.IsEmpty);
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_subset_DbCompiledModel()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(DbCompiledModelContents.IsSubset);
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_superset_DbCompiledModel()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(DbCompiledModelContents.IsSuperset);
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_DbCompiledModel_that_matches_the_context()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(DbCompiledModelContents.Match);
        }

        [Fact]
        public void Sets_are_initialized_for_DbContext_constructor_when_using_DbCompiledModel_that_doesnt_match_context_definitions()
        {
            VerifySetsAreInitialized<SimpleModelContextWithNoData>(DbCompiledModelContents.DontMatch);
        }

        [Fact]
        public void Model_Tweaking_is_ignored_when_using_model_ctor_on_DbContext()
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

                // Assert that Blog doesnt have Id as the key but rather has Title as the Key
                var type = GetEntityType(context, typeof(Blog));
                Assert.True(type.KeyMembers.Count == 1);
                Assert.Equal(type.KeyMembers.First().Name, "Title");
            }
        }

        [Fact]
        public void Verify_DbContext_construction_using_connection_string_ctor_when_string_is_database_name()
        {
            Verify_DbContext_construction_using_connection_string_ctor(
                nameOrConnectionString: "DefaultDatabaseNameDb",
                expectedDatabaseName: "DefaultDatabaseNameDb");
        }

        [Fact]
        public void Verify_DbContext_construction_using_connection_string_ctor_when_string_is_provider_connection_string()
        {
            Verify_DbContext_construction_using_connection_string_ctor(
                nameOrConnectionString: SimpleConnectionString<SimpleModelContextWithNoData>());
        }

        [Fact]
        public void Verify_DbContext_construction_using_connection_string_ctor_when_string_is_named_connection_string()
        {
            Verify_DbContext_construction_using_connection_string_ctor(
                nameOrConnectionString: "SimpleModelWithNoDataFromAppConfig",
                expectedDatabaseName: "SimpleModel.SimpleModelWithNoData");
        }

        [Fact]
        public void Verify_DbContext_construction_using_connection_string_ctor_when_string_is_named_connection_string_using_name_keyword()
        {
            Verify_DbContext_construction_using_connection_string_ctor(
                nameOrConnectionString: "Name=SimpleModelWithNoDataFromAppConfig",
                expectedDatabaseName: "SimpleModel.SimpleModelWithNoData");
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_connection_string_ctor_when_string_is_named_connection_string_and_its_last_token_exists_in_config
            ()
        {
            Verify_DbContext_construction_using_connection_string_ctor(
                nameOrConnectionString: "X.Y.Z.R.SimpleModelWithNoDataFromAppConfig",
                expectedDatabaseName: "SimpleModel.SimpleModelWithNoData");
        }

        private void Verify_DbContext_construction_using_connection_string_ctor(
            string nameOrConnectionString,
            string expectedDatabaseName =
                "SimpleModel.SimpleModelContextWithNoData")
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
        public void Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_defines_a_subset_of_entities_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.IsSubset);
        }

        [Fact]
        public void Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_defines_a_superset_of_entities_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.IsSuperset);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_matches_the_entities_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.Match);
        }

        [Fact]
        public void Verify_DbContext_construction_using_db_name_and_model_Ctor_where_model_has_no_entities_matching_those_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.DatabaseName,
                DbCompiledModelContents.DontMatch);
        }

        [Fact]
        public void Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_is_empty()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.IsEmpty);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_defines_a_subset_of_entities_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.IsSubset);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_defines_a_superset_of_entities_on_context
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.IsSuperset);
        }

        [Fact]
        public void Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_matches_the_entities_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.Match);
        }

        [Fact]
        public void
            Verify_DbContext_construction_using_valid_connection_string_and_model_Ctor_where_model_has_no_entities_matching_those_on_context
            ()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.ProviderConnectionString, DbCompiledModelContents.DontMatch);
        }

        [Fact]
        public void Verify_DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_is_empty()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.IsEmpty);
        }

        [Fact]
        public void DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_defines_a_subset_of_entities_on_context(

            )
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.IsSubset);
        }

        [Fact]
        public void
            DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_defines_a_superset_of_entities_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.IsSuperset);
        }

        [Fact]
        public void DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_matches_the_entities_on_context()
        {
            DbContext_construction_using_connection_string_and_model_Ctor(
                ConnectionStringFormat.NamedConnectionString,
                DbCompiledModelContents.Match);
        }

        [Fact]
        public void
            DbContext_construction_using_named_connection_string_and_model_Ctor_where_model_has_no_entities_matching_those_on_context()
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
                    connectionString = "SimpleModelWithNoDataFromAppConfig";
                    dbName = "SimpleModel.SimpleModelWithNoData";
                    break;
                case ConnectionStringFormat.ProviderConnectionString:
                    connectionString = SimpleConnectionString<SimpleModelContextWithNoData>();
                    dbName = "SimpleModel.SimpleModelContextWithNoData";
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
                    builder.Build(ProviderRegistry.Sql2008_ProviderInfo).
                        Compile()))
            {
                // Assert
                Assert.Equal(context.Database.Connection.Database, dbName);
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
                        context.Assert<Product>().IsInModel(); // reachability
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
        public void Database_name_generated_from_generic_DbContext_class_name_works()
        {
            using (var context = new GenericFuncy<string, GT<string, int?>>())
            {
                context.Database.Delete();

                context.Database.Initialize(force: false);

                Assert.True(context.Database.Exists());
            }
        }

        [Fact]
        public void Database_name_generated_from_DbContext_class_nested_in_generic_class_name_works()
        {
            using (var context = new GT<string, int?>.Funcy())
            {
                context.Database.Delete();

                context.Database.Initialize(force: false);

                Assert.True(context.Database.Exists());
            }
        }

        [Fact]
        public void Database_name_generated_from_generic_DbContext_class_nested_in_generic_class_name_works()
        {
            using (var context = new GT<NT, NT>.GenericFuncy<GT<GT<NT, NT>, NT>, NT>())
            {
                context.Database.Delete();

                context.Database.Initialize(force: false);

                Assert.True(context.Database.Exists());
            }
        }

        #endregion

        #region Negative constructor tests

        [Fact]
        public void DbContext_construction_does_not_throw_but_subsequent_calls_using_connection_throw_for_invalid_sql_connection_string()
        {
            var context = new SimpleModelContextWithNoData("InvalidKeywordConnectionString");
            Assert.Throws<ArgumentException>(() => GetObjectContext(context)).ValidateMessage(
                typeof(DbConnection).Assembly, "ADP_KeywordNotSupported", null);
        }

        [Fact]
        public void DbContext_construction_does_not_throw_but_subsequent_calls_using_connection_throw_for_invalid_provider_keyword()
        {
            var context = new SimpleModelContextWithNoData("InvalidProviderNameConnectionString");
            Assert.Throws<ArgumentException>(() => GetObjectContext(context)).ValidateMessage("EntityClient_InvalidStoreProvider");
        }

        [Fact]
        public void DbContext_construction_does_not_throw_but_subsequent_calls_using_connection_throw_for_nonexistent_connection_string()
        {
            var context = new SimpleModelContextWithNoData("Name=NonexistentConnectionString");
            Assert.Throws<InvalidOperationException>(() => GetObjectContext(context)).ValidateMessage(
                "DbContext_ConnectionStringNotFound", "NonexistentConnectionString");
        }

        [Fact]
        public void DbContext_caches_models_for_two_providers()
        {
            // Ensure that the model is in use with a SQL connection
            using (var context = new SimpleModelContext())
            {
                context.Database.Initialize(force: false);
            }

            // Now try to use it with a CE connection; would throw in EF 4.1, will not now throw.
            var sqlCeConnectionFactory = new SqlCeConnectionFactory(
                "System.Data.SqlServerCe.4.0",
                AppDomain.CurrentDomain.BaseDirectory, "");
            using (var connection = sqlCeConnectionFactory.CreateConnection(DefaultDbName<SimpleModelContext>()))
            {
                using (var context = new SimpleModelContext(connection))
                {
                    context.Database.Initialize(force: false);
                }
            }
        }

        #endregion

        #region Positive SaveChanges tests

        [Fact]
        [AutoRollback]
        public void SaveChanges_saves_Added_Modified_Deleted_entities()
        {
            SaveChanges_saves_Added_Modified_Deleted_entities_implementation((c) => c.SaveChanges());
        }

#if !NET40

        [Fact]
        [AutoRollback]
        public void SaveChangesAsync_saves_Added_Modified_Deleted_entities()
        {
            SaveChanges_saves_Added_Modified_Deleted_entities_implementation((c) => c.SaveChangesAsync().Result);
        }

#endif

        private void SaveChanges_saves_Added_Modified_Deleted_entities_implementation(Func<DbContext, int> saveChanges)
        {
            using (var context = new SimpleModelContext())
            {
                // Modified
                var product1 = context.Products.Find(1);
                product1.Name = "Smarties";

                // Deleted
                var product2 = context.Products.Find(2);
                context.Products.Remove(product2);

                // Added
                var product3 = new Product
                                   {
                                       Name = "Branston Pickle"
                                   };
                context.Products.Add(product3);

                // Validate state before Save
                Assert.Equal(3, GetStateEntries(context).Count());
                Assert.Equal(EntityState.Modified, GetStateEntry(context, product1).State);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, product2).State);
                Assert.Equal(EntityState.Added, GetStateEntry(context, product3).State);

                // Save
                var savedCount = saveChanges(context);
                Assert.Equal(3, savedCount);

                // Validate state after Save
                Assert.Equal(2, GetStateEntries(context).Count());
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product1).State);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product3).State);

                using (var context2 = new SimpleModelContext())
                {
                    var product1s = context2.Products.Find(product1.Id);
                    var product2s = context2.Products.Find(product2.Id);
                    var product3s = context2.Products.Find(product3.Id);

                    Assert.NotNull(product1s);
                    Assert.Null(product2s);
                    Assert.NotNull(product3s);

                    Assert.Equal("Smarties", product1s.Name);
                    Assert.Equal("Branston Pickle", product3s.Name);

                    Assert.Equal(2, GetStateEntries(context).Count());
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context2, product1s).State);
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context2, product3s).State);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void SaveChanges_performs_DetectChanges()
        {
            SaveChanges_performs_DetectChanges_implementation((c) => c.SaveChanges());
        }

#if !NET40

        [Fact]
        [AutoRollback]
        public void SaveChangesAsync_performs_DetectChanges()
        {
            SaveChanges_performs_DetectChanges_implementation((c) => c.SaveChangesAsync().Result);
        }

#endif

        private void SaveChanges_performs_DetectChanges_implementation(Func<DbContext, int> saveChanges)
        {
            // NOTE: This is split out into a seperate test from the above test because 
            //       it is important no other APIs are called between the modification 
            //       and calling SaveChanges due to other APIs calling DetectChanges implicitly

            using (var context = new SimpleModelContext())
            {
                var prod = context.Products.Find(1);
                prod.Name = "Cascade Draught";
                var savedCount = saveChanges(context);
                Assert.Equal(1, savedCount);
            }
        }

        [Fact]
        public void SaveChanges_on_uninitialized_context_does_not_throw()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal(0, context.SaveChanges());
            }
        }

#if !NET40

        [Fact]
        public void SaveChangesAsync_on_uninitialized_context_does_not_throw()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal(0, context.SaveChangesAsync().Result);
            }
        }

#endif

        public class SaveChangesDoesntInitializeContext : DbContext
        {
            public SaveChangesDoesntInitializeContext()
            {
                Database.SetInitializer<SaveChangesDoesntInitializeContext>(null);
            }

            public bool InitializationHappened { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                InitializationHappened = true;
            }
        }

        [Fact]
        public void SaveChanges_on_uninitialized_context_does_not_cause_context_to_be_initialized()
        {
            using (var context = new SaveChangesDoesntInitializeContext())
            {
                context.SaveChanges();

                Assert.False(context.InitializationHappened);

                var _ = ((IObjectContextAdapter)context).ObjectContext;

                Assert.True(context.InitializationHappened);
            }
        }

#if !NET40

        public class SaveChangesAsyncDoesntInitializeContext : DbContext
        {
            public SaveChangesAsyncDoesntInitializeContext()
            {
                Database.SetInitializer<SaveChangesAsyncDoesntInitializeContext>(null);
            }

            public bool InitializationHappened { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                InitializationHappened = true;
            }
        }

        [Fact]
        public void SaveChangesAsync_on_uninitialized_context_does_not_cause_context_to_be_initialized()
        {
            using (var context = new SaveChangesAsyncDoesntInitializeContext())
            {
                context.SaveChangesAsync().Wait();

                Assert.False(context.InitializationHappened);

                var _ = ((IObjectContextAdapter)context).ObjectContext;

                Assert.True(context.InitializationHappened);
            }
        }

#endif

        [Fact]
        public void SaveChanges_is_virtual()
        {
            using (DbContext context = new SimpleModelContextWithNoData())
            {
                Assert.Equal(0, context.SaveChanges());
                Assert.True(((SimpleModelContextWithNoData)context).SaveChangesCalled);
            }
        }

#if !NET40

        [Fact]
        public void SaveChangesAsync_is_virtual()
        {
            using (DbContext context = new SimpleModelContextWithNoData())
            {
                Assert.Equal(0, context.SaveChangesAsync().Result);
                Assert.True(((SimpleModelContextWithNoData)context).SaveChangesCalled);
            }
        }

#endif

        #endregion

        #region Negative SaveChanges tests

        [Fact]
        public void SaveChanges_bubbles_presave_exception()
        {
            SaveChanges_bubbles_presave_exception_implementation((c) => c.SaveChanges());
        }

#if !NET40

        [Fact]
        public void SaveChangesAsync_bubbles_presave_exception()
        {
            SaveChanges_bubbles_presave_exception_implementation(
                (c) => ExceptionHelpers.UnwrapAggregateExceptions(() => c.SaveChangesAsync().Result));
        }

#endif

        private void SaveChanges_bubbles_presave_exception_implementation(Func<DbContext, int> saveChanges)
        {
            EnsureDatabaseInitialized(() => new AdvancedPatternsMasterContext());

            using (new TransactionScope())
            {
                using (var context = new AdvancedPatternsMasterContext())
                {
                    var emp = new CurrentEmployee
                                  {
                                      EmployeeId = 4
                                  };
                    var ord = new WorkOrder
                                  {
                                      WorkOrderId = 2,
                                      EmployeeId = 4
                                  };
                    context.Employees.Attach(emp);
                    context.WorkOrders.Attach(ord);

                    // Create a conceptual null
                    GetObjectContext(context).ObjectStateManager.ChangeObjectState(emp, EntityState.Deleted);

                    Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).ValidateMessage(
                        "ObjectContext_CommitWithConceptualNull");
                }
            }
        }

        [Fact]
        public void SaveChanges_bubbles_UpdateException()
        {
            SaveChanges_bubbles_UpdateException_implementation((c) => c.SaveChanges());
        }

#if !NET40

        [Fact]
        public void SaveChangesAsync_bubbles_UpdateException()
        {
            SaveChanges_bubbles_UpdateException_implementation(
                (c) => ExceptionHelpers.UnwrapAggregateExceptions(() => c.SaveChangesAsync().Result));
        }

#endif

        private void SaveChanges_bubbles_UpdateException_implementation(Func<DbContext, int> saveChanges)
        {
            using (var context = new SimpleModelContext())
            {
                using (context.Database.BeginTransaction())
                {
                    var prod = new Product
                                   {
                                       Name = "Wallaby Sausages",
                                       CategoryId = "AUSSIE FOODS"
                                   };
                    context.Products.Add(prod);

                    Assert.Throws<DbUpdateException>(() => saveChanges(context)).ValidateMessage(
                        "Update_GeneralExecutionException");
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void SaveChanges_bubbles_exception_during_AcceptChanges()
        {
            SaveChanges_bubbles_exception_during_AcceptChanges_implementation((c) => c.SaveChanges());
        }

#if !NET40

        [Fact]
        [AutoRollback]
        public void SaveChangesAsync_bubbles_exception_during_AcceptChanges()
        {
            SaveChanges_bubbles_exception_during_AcceptChanges_implementation(
                (c) => ExceptionHelpers.UnwrapAggregateExceptions(() => c.SaveChangesAsync().Result));
        }

#endif

        private void SaveChanges_bubbles_exception_during_AcceptChanges_implementation(Func<DbContext, int> saveChanges)
        {
            using (var context = new SimpleModelContext())
            {
                var cat1 = new Category
                               {
                                   Id = "AUSSIE FOODS"
                               };
                var cat2 = new Category
                               {
                                   Id = "AUSSIE FOODS"
                               };

                context.Categories.Attach(cat1);
                context.Categories.Add(cat2);

                // Accept will fail because of PK violation
                // (cat1 doesn't actually exist in the store so update pipeline will succeed)
                Assert.Throws<InvalidOperationException>(() => saveChanges(context)).ValidateMessage(
                    "ObjectContext_AcceptAllChangesFailure");
            }
        }

        [Fact]
        public void SaveChanges_throws_on_validation_errors()
        {
            SaveChanges_throws_on_validation_errors_implementation((c) => c.SaveChanges());
        }

#if !NET40

        [Fact]
        public void SaveChangesAsync_throws_on_validation_errors()
        {
            SaveChanges_throws_on_validation_errors_implementation(
                (c) => ExceptionHelpers.UnwrapAggregateExceptions(() => c.SaveChangesAsync().Result));
        }

#endif

        public void SaveChanges_throws_on_validation_errors_implementation(Func<DbContext, int> saveChanges)
        {
            using (var context = new ValidationTestContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    context.ValidateEntityFunc =
                        (entry) => { return new DbEntityValidationResult(entry, new[] { new DbValidationError("Id", "error") }); };
                    context.Categories.Add(new Category("FOOD"));
                    Assert.Throws<DbEntityValidationException>(() => context.SaveChanges()).ValidateMessage(
                        "DbEntityValidationException_ValidationFailed");
                }
            }
        }

        #endregion

        #region Positive ObjectContext tests

        [Fact]
        public void ObjectContext_property_returns_EF_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);

                Assert.NotNull(GetObjectContext(context));
                Assert.NotNull(GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(product));
            }
        }

        [Fact]
        public void ObjectContext_property_returns_EF_context_even_before_DbContext_is_initialized()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.NotNull(GetObjectContext(context));
                var product = GetObjectContext(context).CreateObjectSet<Product>().Where(p => p.Id == 1).Single();
                Assert.Equal(1, product.Id);
            }
        }

        #endregion

        #region Positive Connection property tests

        [Fact]
        public void Connection_property_returns_connection()
        {
            using (var context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);

                Assert.NotNull(context.Database.Connection);
                Assert.Equal(DefaultDbName<SimpleModelContext>(), context.Database.Connection.Database);
            }
        }

        [Fact]
        public void Connection_property_returns_connection_even_before_DbContext_is_initialized()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.NotNull(context.Database.Connection);
                Assert.Equal(DefaultDbName<SimpleModelContext>(), context.Database.Connection.Database);
            }
        }

        #endregion

        #region Positive set detection and initialization tests

        public class DbSetVariant : EmptyContext
        {
            public DbSetVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> UnicornProducts { get; set; }
            public DbSet<Category> UnicornCategories { get; set; }
        }

        [Fact]
        public void DbSet_properties_are_detected_and_initialized()
        {
            using (var context = new DbSetVariant())
            {
                Assert.NotNull(context.UnicornProducts);
                Assert.NotNull(context.UnicornCategories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        [Fact]
        public void Entity_set_names_are_taken_from_DbSet_property_names()
        {
            using (var context = new DbSetVariant())
            {
                Assert.Equal("UnicornProducts", GetEntitySetName(context, typeof(Product)));
                Assert.Equal("UnicornCategories", GetEntitySetName(context, typeof(Category)));
            }
        }

        public class IDbSetVariant : EmptyContext
        {
            public IDbSetVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public IDbSet<Product> UnicornProducts { get; set; }
            public IDbSet<Category> UnicornCategories { get; set; }
        }

        [Fact]
        public void IDbSet_properties_are_detected_and_initialized()
        {
            using (var context = new IDbSetVariant())
            {
                Assert.NotNull(context.UnicornProducts);
                Assert.NotNull(context.UnicornCategories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        [Fact]
        public void Entity_set_names_are_taken_from_DbSet_property_names_when_using_IDbSet_properties()
        {
            using (var context = new IDbSetVariant())
            {
                Assert.Equal("UnicornProducts", GetEntitySetName(context, typeof(Product)));
                Assert.Equal("UnicornCategories", GetEntitySetName(context, typeof(Category)));
            }
        }

        public class IQueryableVariant : EmptyContext
        {
            public IQueryableVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public IQueryable<Product> Products { get; set; }
            public IQueryable<Category> Categories { get; set; }
        }

        [Fact]
        public void IQueryable_properties_are_not_detected_or_initialized()
        {
            using (var context = new IQueryableVariant())
            {
                Assert.Null(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsNotInModel();
                context.Assert<Category>().IsNotInModel();
            }
        }

        public class DbQueryVariant : EmptyContext
        {
            public DbQueryVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public DbQuery<Product> Products { get; set; }
            public DbQuery<Category> Categories { get; set; }
        }

        [Fact]
        public void DbQuery_properties_are_not_initialized()
        {
            using (var context = new DbQueryVariant())
            {
                Assert.Null(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsNotInModel();
                context.Assert<Category>().IsNotInModel();
            }
        }

        public class SetOfDerivedTypeVariant : EmptyContext
        {
            public SetOfDerivedTypeVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> UnicornProducts { get; set; }
            public DbSet<FeaturedProduct> FeaturedProducts { get; set; }
            public DbSet<Category> UnicornCategories { get; set; }
        }

        [Fact]
        public void Set_of_derived_type_initialized()
        {
            using (var context = new SetOfDerivedTypeVariant())
            {
                Assert.NotNull(context.UnicornProducts);
                Assert.NotNull(context.FeaturedProducts);
                Assert.NotNull(context.UnicornCategories);
                context.Assert<Product>().IsInModel();
                context.Assert<FeaturedProduct>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        [Fact]
        public void Entity_set_names_for_a_derived_type_come_from_the_base_type_DbSet_property_names()
        {
            using (var context = new SetOfDerivedTypeVariant())
            {
                Assert.Equal("UnicornProducts", GetEntitySetName(context, typeof(Product)));
                Assert.Equal("UnicornProducts", GetEntitySetName(context, typeof(FeaturedProduct)));
                Assert.Equal("UnicornCategories", GetEntitySetName(context, typeof(Category)));
            }
        }

        public class PrivateSettersVariant : EmptyContext
        {
            public PrivateSettersVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products { get; private set; }
            public IDbSet<Category> Categories { get; private set; }
        }

        [Fact]
        public void Set_properties_with_private_setters_are_detected_but_not_initialized()
        {
            using (var context = new PrivateSettersVariant())
            {
                Assert.Null(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class InternalSettersVariant : EmptyContext
        {
            public InternalSettersVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products { get; internal set; }
            public IDbSet<Category> Categories { get; internal set; }
        }

        [Fact]
        public void Set_properties_with_internal_setters_are_detected_but_not_initialized()
        {
            using (var context = new InternalSettersVariant())
            {
                Assert.Null(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class ProtectedSettersVariant : EmptyContext
        {
            public ProtectedSettersVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products { get; protected set; }
            public IDbSet<Category> Categories { get; protected set; }
        }

        [Fact]
        public void Set_properties_with_protected_setters_are_detected_but_not_initialized()
        {
            using (var context = new ProtectedSettersVariant())
            {
                Assert.Null(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class NoSettersVariant : EmptyContext
        {
            public NoSettersVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products
            {
                get { return null; }
            }

            public IDbSet<Category> Categories
            {
                get { return null; }
            }
        }

        [Fact]
        public void Set_properties_with_no_setters_are_detected_but_not_initialized()
        {
            using (var context = new NoSettersVariant())
            {
                Assert.Null(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class FullyPrivateVariant : EmptyContext
        {
            public FullyPrivateVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            private DbSet<Product> Products { get; set; }
            private IDbSet<Category> Categories { get; set; }

            public IQueryable<Product> GetProducts()
            {
                return Products;
            }

            public IQueryable<Category> GetCategories()
            {
                return Categories;
            }
        }

        [Fact]
        public void Fully_private_properties_are_detected_but_not_initialized()
        {
            using (var context = new FullyPrivateVariant())
            {
                Assert.Null(context.GetProducts());
                Assert.Null(context.GetCategories());
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class MethodsAndFieldsVariant : EmptyContext
        {
            public MethodsAndFieldsVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public IQueryable<Product> Products;
            public IDbSet<Category> Categories;
            public DbSet<Login> Logins;

            public IQueryable<Product> GetProducts()
            {
                return Products;
            }

            public IDbSet<Category> GetCategories()
            {
                return Categories;
            }

            public DbSet<Login> GetLogins()
            {
                return Logins;
            }
        }

        [Fact]
        public void IQueryable_and_DbSet_methods_and_fields_are_ignored()
        {
            using (var context = new MethodsAndFieldsVariant())
            {
                Assert.Null(context.GetProducts());
                Assert.Null(context.GetCategories());
                Assert.Null(context.GetLogins());
                context.Assert<Product>().IsNotInModel();
                context.Assert<Category>().IsNotInModel();
                context.Assert<Login>().IsNotInModel();
            }
        }

        [Fact]
        public void Derived_context_with_no_sets_is_okay()
        {
            using (var context = new EmptyContext())
            {
                // Asking for the ObjectContext causes the DbContext to be initialized even with no types.
                Assert.NotNull(GetObjectContext(context));
            }
        }

        public class NonGenericIQueryableVariant : EmptyContext
        {
            public NonGenericIQueryableVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public IQueryable Stuff { get; set; }
        }

        [Fact]
        public void Set_properties_of_non_generic_IQueryable_are_ignored()
        {
            using (var context = new NonGenericIQueryableVariant())
            {
                Assert.Null(context.Stuff);

                // Asking for the ObjectContext causes the DbContext to be initialized even with no types.
                Assert.NotNull(GetObjectContext(context));
            }
        }

        public class IEnumerableVariant : EmptyContext
        {
            public IEnumerableVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public IEnumerable<Category> Categories { get; set; }
        }

        [Fact]
        public void Set_properties_defined_as_IEnumerable_are_ignored()
        {
            using (var context = new IEnumerableVariant())
            {
                Assert.Null(context.Categories);
                context.Assert<Category>().IsNotInModel();
            }
        }

        public class SetsDerivedFromValidSetTypeVariant : EmptyContext
        {
            public SetsDerivedFromValidSetTypeVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public ObjectSet<Product> Products { get; set; }
            public HashSetBasedDbSet<Category> Categories { get; set; }
        }

        [Fact]
        public void Set_properties_derived_from_valid_set_type_are_ignored()
        {
            using (var ctx = new SetsDerivedFromValidSetTypeVariant())
            {
                Assert.Null(ctx.Products);
                Assert.Null(ctx.Categories);
                ctx.Assert<Product>().IsNotInModel();
                ctx.Assert<Category>().IsNotInModel();
            }
        }

        public class DerivedDerivedContextNoExtraSetsVariant : SimpleModelContext
        {
        }

        [Fact]
        public void Derived_derived_context_with_no_extra_sets()
        {
            using (var context = new DerivedDerivedContextNoExtraSetsVariant())
            {
                Assert.NotNull(context.Categories);
                Assert.NotNull(context.Products);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class DerivedDerivedContextExtraSetsVariant : SimpleModelContext
        {
            public DbSet<ExtraEntity> ExtraSet { get; set; }
        }

        [Fact]
        public void Derived_derived_context_with_extra_sets()
        {
            using (var context = new DerivedDerivedContextExtraSetsVariant())
            {
                Assert.NotNull(context.Categories);
                Assert.NotNull(context.Products);
                Assert.NotNull(context.ExtraSet);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
                context.Assert<ExtraEntity>().IsInModel();
            }
        }

        #endregion

        #region Negative set detection and initialization tests

        public class InterfaceVariant : EmptyContext
        {
            public InterfaceVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public DbSet<ICollection> Collections { get; set; }
        }

        [Fact]
        public void Set_properties_for_interface_types_throw()
        {
            try
            {
                new InterfaceVariant();
                Assert.True(false);
            }
            catch (InvalidOperationException ex)
            {
                var resourceLookup = new AssemblyResourceLookup(
                    EntityFrameworkAssembly,
                    "System.Data.Entity.Properties.Resources");
                var messageTemplate = resourceLookup.LookupString("InvalidEntityType");

                var message = String.Format(CultureInfo.InvariantCulture, messageTemplate, typeof(ICollection));

                Assert.Equal(message, ex.Message);
            }
        }

        public class GenericVariant : EmptyContext
        {
            public GenericVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public IDbSet<List<Product>> ProductCollections { get; set; }
        }

        [Fact]
        public void Set_properties_for_generic_types_throw()
        {
            try
            {
                new GenericVariant();
                Assert.True(false);
            }
            catch (InvalidOperationException ex)
            {
                var resourceLookup = new AssemblyResourceLookup(
                    EntityFrameworkAssembly,
                    "System.Data.Entity.Properties.Resources");
                var messageTemplate = resourceLookup.LookupString("InvalidEntityType");

                var message = String.Format(CultureInfo.InvariantCulture, messageTemplate, typeof(List<Product>));

                Assert.Equal(message, ex.Message);
            }
        }

        public class NonEntityVariant : EmptyContext
        {
            public NonEntityVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public DbSet<ObsoleteAttribute> ObsoleteAttributes { get; set; }
        }

        [Fact]
        public void Set_properties_for_non_entity_types_throw()
        {
            using (var context = new NonEntityVariant())
            {
                Assert.NotNull(context.ObsoleteAttributes);
                Assert.Throws<ModelValidationException>(() => GetObjectContext(context));
            }
        }

        public class DuplicateDbSetVariant : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Category> CategoriesAgain { get; set; }
        }

        [Fact]
        public void Duplicate_DbSet_content_types_throw_Anti_MEST()
        {
            using (var context = new DuplicateDbSetVariant())
            {
                Assert.Throws<InvalidOperationException>(() => context.Categories.FirstOrDefault()).ValidateMessage(
                    "Mapping_MESTNotSupported", "Categories", "CategoriesAgain", "SimpleModel.Category");
            }
        }

        public class DuplicateIQueryableSetVariant : EmptyContext
        {
            public DuplicateIQueryableSetVariant()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public IQueryable<Product> Products1 { get; set; }
            public IQueryable<Product> Products2 { get; set; }
        }

        [Fact]
        public void Duplicate_IQueryable_content_types_do_not_throw_Anti_MEST()
        {
            using (var context = new DuplicateIQueryableSetVariant())
            {
                Assert.Null(context.Products1);
                Assert.Null(context.Products2);
                context.Assert<Product>().IsNotInModel();
            }
        }

        public class DuplicateSetWithDifferentSetTypesVariant : EmptyContext
        {
            public DuplicateSetWithDifferentSetTypesVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public IQueryable<Category> CategoriesAgain { get; set; }
        }

        [Fact]
        public void Duplicate_set_content_types_where_one_type_is_IQueryable_does_not_throw_Anti_MEST()
        {
            using (var context = new DuplicateSetWithDifferentSetTypesVariant())
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                Assert.Null(context.CategoriesAgain);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class ExceptionFromSetSetterVariant : EmptyContext
        {
            public ExceptionFromSetSetterVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Category> Categories { get; set; }

            public DbSet<Product> Products
            {
                get { return null; }
                set { throw new Exception("SETTER"); }
            }
        }

        [Fact]
        public void Exception_from_set_setter()
        {
            try
            {
                var ctx = new ExceptionFromSetSetterVariant();
                Assert.True(false, "Exception in set setter was swallowed.");
            }
            catch (Exception ex)
            {
                Assert.Equal("SETTER", ex.Message);
            }
        }

        #endregion

        #region Positive tests for tweaking set discovery using attributes at class level

        [SuppressDbSetInitialization]
        private class ClassLevelSetDiscoverOnlyVariant : EmptyContext
        {
            public ClassLevelSetDiscoverOnlyVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
        }

        [Fact]
        public void
            SuppressDbSetInitializationAttribute_on_context_class_results_in_entities_in_model_but_sets_not_initialized()
        {
            using (var context = new ClassLevelSetDiscoverOnlyVariant())
            {
                Assert.Null(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        #endregion

        #region Positive tests for tweaking set discovery using attributes at property level

        private class PropertyLevelSetDiscoverOnlyVariant : EmptyContext
        {
            public PropertyLevelSetDiscoverOnlyVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products { get; set; }

            [SuppressDbSetInitialization]
            public DbSet<Category> Categories { get; set; }
        }

        [Fact]
        public void SuppressDbSetInitializationAttribute_on_property_results_in_that_entity_in_model_but_its_set_not_initialized()
        {
            using (var context = new PropertyLevelSetDiscoverOnlyVariant())
            {
                Assert.NotNull(context.Products);
                Assert.Null(context.Categories);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        #endregion

        #region Positive Set tests

        [Fact]
        public void Can_create_DbSet()
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set<Category>();
                Assert.IsType<DbSet<Category>>(set);

                var entity = set.FirstOrDefault();
                Assert.IsType<Category>(entity);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, entity).State);
            }
        }

        [Fact]
        public void Can_create_non_generic_DbSet()
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set(typeof(Category));
                Assert.Equal(typeof(Category), set.ElementType);

                var entity = set.Cast<Category>().FirstOrDefault();
                Assert.IsType<Category>(entity);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, entity).State);
            }
        }

        [Fact]
        public void Can_create_DbSet_for_base_type()
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set<Product>();
                Assert.IsType<DbSet<Product>>(set);

                var entity = set.FirstOrDefault();
                Assert.IsType<Product>(entity);
            }
        }

        [Fact]
        public void Can_create_non_generic_DbSet_for_base_type()
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set(typeof(Product));
                Assert.Equal(typeof(Product), set.ElementType);

                var entity = set.Cast<Product>().FirstOrDefault();
                Assert.IsType<Product>(entity);
            }
        }

        [Fact]
        public void Can_create_DbSet_for_abstract_base_type()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var set = context.Set<Employee>();
                Assert.IsType<DbSet<Employee>>(set);

                var entity = set.FirstOrDefault();
                Assert.IsAssignableFrom<Employee>(entity);
            }
        }

        [Fact]
        public void Can_create_non_generic_DbSet_for_abstract_base_type()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var set = context.Set(typeof(Employee));
                Assert.Equal(typeof(Employee), set.ElementType);

                var entity = set.Cast<Employee>().FirstOrDefault();
                Assert.IsAssignableFrom<Employee>(entity);
            }
        }

        [Fact]
        public void Can_create_DbSet_for_derived_type()
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set<FeaturedProduct>();
                Assert.IsType<DbSet<FeaturedProduct>>(set);

                var entity = set.FirstOrDefault();
                Assert.IsType<FeaturedProduct>(entity);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, entity).State);
            }
        }

        [Fact]
        public void Can_create_non_generic_DbSet_for_derived_type()
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set(typeof(FeaturedProduct));
                Assert.Equal(typeof(FeaturedProduct), set.ElementType);

                var entity = set.Cast<FeaturedProduct>().FirstOrDefault();
                Assert.IsType<FeaturedProduct>(entity);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, entity).State);
            }
        }

        [Fact]
        public void Set_method_returns_the_same_instance_each_invocation()
        {
            using (var context = new SimpleModelContext())
            {
                var set1 = context.Set<Product>();
                var set2 = context.Set<Product>();

                Assert.Same(set1, set2);
                Assert.Same(set1, context.Products);
            }
        }

        [Fact]
        public void Non_generic_Set_method_returns_the_same_instance_each_invocation()
        {
            using (var context = new SimpleModelContext())
            {
                var set1 = context.Set(typeof(Product));
                var set2 = context.Set(typeof(Product));

                Assert.Same(set1, set2);
            }
        }

        #endregion

        #region Positive OnModelCreating tests

        public class TweakingModelVariant : EmptyContext
        {
            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Category>().HasKey(c => c.Id);
                modelBuilder.Entity<Category>().HasMany(c => c.Products).WithOptional(p => p.Category).HasForeignKey(
                    p => p.CategoryId);
                base.OnModelCreating(modelBuilder);
            }
        }

        [Fact]
        public void Model_can_be_tweaked_in_OnModelCreating()
        {
            using (var context = new TweakingModelVariant())
            {
                Assert.NotNull(context.Products);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
            }
        }

        public class OnModelCreatingVariant : DbContext
        {
            public DbSet<Product> Products { get; set; }

            public static int OnModelCreatingCount { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                OnModelCreatingCount++;
            }
        }

        [Fact]
        public void OnModelCreating_is_only_called_once_by_default()
        {
            using (var context = new OnModelCreatingVariant())
            {
                context.Assert<Product>().IsInModel();
            }
            Assert.Equal(1, OnModelCreatingVariant.OnModelCreatingCount);

            using (var context = new OnModelCreatingVariant())
            {
                context.Assert<Product>().IsInModel();
            }
            Assert.Equal(1, OnModelCreatingVariant.OnModelCreatingCount);

            using (var context = new OnModelCreatingVariant())
            {
                context.Assert<Product>().IsInModel();
            }
            Assert.Equal(1, OnModelCreatingVariant.OnModelCreatingCount);
        }

        public class SillyCallToOnModelCreatingVariant : EmptyContext
        {
            public SillyCallToOnModelCreatingVariant()
                : base(DefaultDbName<EmptyContext>())
            {
                OnModelCreating(new DbModelBuilder());
            }
        }

        [Fact]
        public void Calling_OnModelCreating_explicitly_is_noop()
        {
            using (var context = new SillyCallToOnModelCreatingVariant())
            {
            }
        }

        [Fact]
        public void Can_use_context_during_construction_with_tweaking()
        {
            using (var context = new UseContextInCtorModelTweakingVariant())
            {
                Assert.NotNull(context.Products);
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();

                Assert.False(GetObjectContext(context).ContextOptions.LazyLoadingEnabled);
                Assert.False(context.Configuration.LazyLoadingEnabled);
            }
        }

        public class UseContextInCtorModelTweakingVariant : EmptyContext
        {
            public UseContextInCtorModelTweakingVariant()
            {
                ((IObjectContextAdapter)this).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Category>().HasKey(c => c.Id);
                modelBuilder.Entity<Category>().HasMany(c => c.Products).WithOptional(p => p.Category).HasForeignKey(
                    p => p.CategoryId);
                base.OnModelCreating(modelBuilder);
            }
        }

        #endregion

        #region Negative OnModelCreating tests

        public class UseModeWhileCreatingVariant : EmptyContext
        {
            public UseModeWhileCreatingVariant()
                : base("CommonProductCategoryDatabase")
            {
            }

            public DbSet<Product> Products { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                Products.FirstOrDefault();
                base.OnModelCreating(modelBuilder);
            }
        }

        [Fact]
        public void Using_context_during_OnModelCreating_throws()
        {
            using (var context = new UseModeWhileCreatingVariant())
            {
                Assert.Throws<InvalidOperationException>(() => context.Products.FirstOrDefault()).ValidateMessage(
                    "DbContext_ContextUsedInModelCreating");
            }
        }

        #endregion

        #region Positive non-derived context tests using an existing model

        [Fact]
        public void Model_passed_to_DbContext_string_constructor_is_used_to_back_the_context()
        {
            Model_passed_to_DbContext_constructor_is_used_to_back_the_context(
                model => new DbContext(DefaultDbName<SimpleModelContext>(), model));
        }

        [Fact]
        public void Model_passed_to_DbContext_DbConnection_constructor_is_used_to_back_the_context()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                Model_passed_to_DbContext_constructor_is_used_to_back_the_context(
                    model => new DbContext(connection, model, contextOwnsConnection: false));
            }
        }

        private void Model_passed_to_DbContext_constructor_is_used_to_back_the_context(
            Func<DbCompiledModel, DbContext> factory)
        {
            using (var context = factory(CreateSimpleModel()))
            {
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
                context.Assert<Login>().IsNotInModel();

                // Verify that we can use sets and we are connected to the correct database
                Assert.Equal("Marmite", context.Set<Product>().Find(1).Name);
                Assert.Equal("Foods", context.Set<Category>().Find("Foods").Id);
            }
        }

        private static DbCompiledModel CreateSimpleModel()
        {
            return SimpleModelContext.CreateBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();
        }

        #endregion

        #region Positive derived context tests using existing model

        private class DerivedContextWithMismatchedModel : DbContext
        {
            public DerivedContextWithMismatchedModel(DbCompiledModel model)
                : base(model)
            {
            }

            public DerivedContextWithMismatchedModel(string nameOrConnectionString, DbCompiledModel model)
                : base(nameOrConnectionString, model)
            {
            }

            public DerivedContextWithMismatchedModel(DbConnection existingConnection, DbCompiledModel model)
                : base(existingConnection, model, contextOwnsConnection: false)
            {
            }

            public DbSet<Login> Logins { get; set; }
        }

        [Fact]
        public void Model_passed_to_derived_DbContext_constructor_is_used_and_the_sets_on_context_are_not_used()
        {
            // In this case we can't control the name of the database, so it won't be the normal SimpleModel database
            using (var context = new DerivedContextWithMismatchedModel(CreateSimpleModel()))
            {
                context.Assert<Product>().IsInModel();
                context.Assert<Category>().IsInModel();
                context.Assert<Login>().IsNotInModel();

                Assert.Throws<InvalidOperationException>(() => context.Logins.ToList()).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", "Login");
            }
        }

        [Fact]
        public void Model_passed_to_derived_DbContext_string_constructor_is_used_and_the_sets_on_context_are_not_used()
        {
            Model_passed_to_DbContext_constructor_is_used_to_back_the_context(
                model => new DerivedContextWithMismatchedModel(DefaultDbName<SimpleModelContext>(), model));
        }

        [Fact]
        public void
            Model_passed_to_derived_DbContext_DbConnection_constructor_is_used_and_the_sets_on_context_are_not_used()
        {
            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                Model_passed_to_DbContext_constructor_is_used_to_back_the_context(
                    model => new DerivedContextWithMismatchedModel(connection, model));
            }
        }

        #endregion

        #region Lazy loading tests

        private void ValidateLazyLoading(F1Context context, bool lazyLoadingEnabled)
        {
            Assert.Equal(lazyLoadingEnabled, context.Configuration.LazyLoadingEnabled);
            Assert.Equal(lazyLoadingEnabled, GetObjectContext(context).ContextOptions.LazyLoadingEnabled);

            context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);

            var team = context.Drivers.First().Team;
            if (lazyLoadingEnabled)
            {
                Assert.NotNull(team);
            }
            else
            {
                Assert.Null(team);
            }
        }

        private static DbCompiledModel CreateF1Model()
        {
            var builder = new DbModelBuilder();

            F1Context.AdditionalModelConfiguration(builder);

            return builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_empty_constructor()
        {
            using (var context = new F1Context())
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_String_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>()))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(CreateF1Model()))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_String_and_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>(), CreateF1Model()))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_DbConnection_constructor()
        {
            using (var context = new F1Context(SimpleConnection<F1Context>(), contextOwnsConnection: true))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_DbConnection_and_DbCompiledModel_constructor()
        {
            using (
                var context = new F1Context(SimpleConnection<F1Context>(), CreateF1Model(), contextOwnsConnection: true)
                )
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_entity_connection_string_in_constructor()
        {
            using (var context = new DbContext(SimpleModelEntityConnectionString))
            {
                Assert.True(context.Configuration.LazyLoadingEnabled);
                var objectContext = GetObjectContext(context);
                Assert.True(objectContext.ContextOptions.LazyLoadingEnabled);
            }
        }

        [Fact]
        public void Lazy_loading_is_on_by_default_when_using_EntityConnection_object_in_constructor()
        {
            using (
                var context = new DbContext(
                    new EntityConnection(SimpleModelEntityConnectionString),
                    contextOwnsConnection: true))
            {
                Assert.True(context.Configuration.LazyLoadingEnabled);
                var objectContext = GetObjectContext(context);
                Assert.True(objectContext.ContextOptions.LazyLoadingEnabled);
            }
        }

        [Fact]
        public void Lazy_loading_flag_can_be_inherited_from_ObjectContext_as_true_when_using_ObjectContext_constructor()
        {
            Lazy_loading_flag_is_inherited_from_ObjectContext_when_using_ObjectContext_constructor(
                lazyLoadingEnabled: true);
        }

        [Fact]
        public void Lazy_loading_flag_can_be_inherited_from_ObjectContext_as_false_when_using_ObjectContext_constructor()
        {
            Lazy_loading_flag_is_inherited_from_ObjectContext_when_using_ObjectContext_constructor(
                lazyLoadingEnabled: false);
        }

        private void Lazy_loading_flag_is_inherited_from_ObjectContext_when_using_ObjectContext_constructor(
            bool lazyLoadingEnabled)
        {
            var objectContext = GetObjectContext(new F1Context());
            objectContext.ContextOptions.LazyLoadingEnabled = lazyLoadingEnabled;

            using (var context = new F1Context(objectContext, dbContextOwnsObjectContext: true))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled);
            }
        }

        [Fact]
        public void Lazy_loading_can_be_switched_off_in_constructor_which_calls_to_base_empty_constructor()
        {
            using (var context = new F1Context(lazyLoadingEnabled: false))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: false);
            }
        }

        [Fact]
        public void Lazy_loading_can_be_switched_off_in_constructor_which_calls_to_base_String_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>(), lazyLoadingEnabled: false))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: false);
            }
        }

        [Fact]
        public void Lazy_loading_can_be_switched_off_in_constructor_which_calls_to_base_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(CreateF1Model(), lazyLoadingEnabled: false))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: false);
            }
        }

        [Fact]
        public void
            Lazy_loading_can_be_switched_off_in_constructor_which_calls_to_base_String_and_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>(), CreateF1Model(), lazyLoadingEnabled: false))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: false);
            }
        }

        [Fact]
        public void Lazy_loading_can_be_switched_off_in_constructor_which_calls_to_base_DbConnection_constructor()
        {
            using (
                var context = new F1Context(
                    SimpleConnection<F1Context>(), contextOwnsConnection: true,
                    lazyLoadingEnabled: false))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: false);
            }
        }

        [Fact]
        public void Lazy_loading_can_be_switched_off_in_constructor_which_calls_to_base_DbConnection_and_DbCompiledModel_constructor()
        {
            using (
                var context = new F1Context(
                    SimpleConnection<F1Context>(), CreateF1Model(), contextOwnsConnection: true,
                    lazyLoadingEnabled: false))
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: false);
            }
        }

        [Fact]
        public void Lazy_loading_can_be_switched_off_in_constructor_which_calls_to_base_ObjectContext_constructor()
        {
            var objectContext = GetObjectContext(new F1Context());
            objectContext.ContextOptions.LazyLoadingEnabled = true;

            using (
                var context = new F1Context(objectContext, dbContextOwnsObjectContext: true, lazyLoadingEnabled: false))
            {
                ValidateLazyLoading(context, false);
            }
        }

        public class LazyLoadingFlagTestContext : EmptyContext
        {
            public LazyLoadingFlagTestContext()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public LazyLoadingFlagTestContext(string connectionString)
                : base(connectionString)
            {
            }

            public LazyLoadingFlagTestContext(DbConnection connection)
                : base(connection, contextOwnsConnection: true)
            {
            }

            public bool ModelCreated { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                ModelCreated = true;
                base.OnModelCreating(modelBuilder);
            }
        }

        [Fact]
        public void Lazy_loading_can_be_changed_after_DbContext_is_created_but_before_initialization_when_using_entity_connection_string()
        {
            Lazy_loading_can_be_changed_after_DbContext_is_created_but_before_initialization(
                () => new LazyLoadingFlagTestContext(SimpleModelEntityConnectionString), expectOnModelCreation: false);
        }

        [Fact]
        public void Lazy_loading_can_be_changed_after_DbContext_is_created_but_before_initialization_when_using_EntityConnection()
        {
            Lazy_loading_can_be_changed_after_DbContext_is_created_but_before_initialization(
                () => new LazyLoadingFlagTestContext(new EntityConnection(SimpleModelEntityConnectionString)),
                expectOnModelCreation: false);
        }

        [Fact]
        public void
            Lazy_loading_can_be_changed_after_DbContext_is_created_without_causing_ObjectContext_initialization_when_using_code_first()
        {
            Lazy_loading_can_be_changed_after_DbContext_is_created_but_before_initialization(
                () => new LazyLoadingFlagTestContext(), expectOnModelCreation: true);
        }

        private void Lazy_loading_can_be_changed_after_DbContext_is_created_but_before_initialization(
            Func<LazyLoadingFlagTestContext> createContext, bool expectOnModelCreation)
        {
            using (var context = createContext())
            {
                Assert.True(context.Configuration.LazyLoadingEnabled);

                context.Configuration.LazyLoadingEnabled = false;
                Assert.False(context.Configuration.LazyLoadingEnabled);

                context.Configuration.LazyLoadingEnabled = true;
                Assert.True(context.Configuration.LazyLoadingEnabled);

                context.Configuration.LazyLoadingEnabled = false;
                Assert.False(context.ModelCreated);

                Assert.Equal(false, GetObjectContext(context).ContextOptions.LazyLoadingEnabled);
                Assert.Equal(expectOnModelCreation, context.ModelCreated);
            }
        }

        [Fact]
        public void
            Changing_lazy_loading_flag_after_ObjectContext_is_initialized_causes_lazy_loading_flag_in_DbContext_and_ObjectContext_to_change(

            )
        {
            using (var context = new F1Context())
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);

                context.Configuration.LazyLoadingEnabled = false;
                ValidateLazyLoading(context, lazyLoadingEnabled: false);

                context.Configuration.LazyLoadingEnabled = true;
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Changing_lazy_loading_flag_in_ObjectContext_causes_lazy_loading_flag_in_DbContext_and_ObjectContext_to_change()
        {
            using (var context = new F1Context())
            {
                ValidateLazyLoading(context, lazyLoadingEnabled: true);

                GetObjectContext(context).ContextOptions.LazyLoadingEnabled = false;
                ValidateLazyLoading(context, lazyLoadingEnabled: false);

                GetObjectContext(context).ContextOptions.LazyLoadingEnabled = true;
                ValidateLazyLoading(context, lazyLoadingEnabled: true);
            }
        }

        [Fact]
        public void Entity_reference_does_not_get_lazily_loaded_if_LazyLoadingEnabled_is_set_to_false()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var driver = context.Drivers.FirstOrDefault();

                Assert.Null(driver.Team);
            }
        }

        [Fact]
        public void Entity_collection_does_not_get_lazily_loaded_if_LazyLoadingEnabled_is_set_to_false()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var team = context.Teams.FirstOrDefault();

                Assert.Equal(0, team.Drivers.Count);
            }
        }

        [Fact]
        public void Lazy_loading_throws_when_done_outside_context_scope()
        {
            Team team;
            using (var context = new F1Context())
            {
                team = context.Teams.FirstOrDefault();
            }

            Assert.Throws<ObjectDisposedException>(() => team.Engine);
            Assert.Throws<ObjectDisposedException>(() => team.Drivers);
        }

        [Fact]
        public void Lazy_loading_wires_up_references_navigations_correctly()
        {
            using (var context = new F1Context())
            {
                // get driver and engine supplier that would navigate to the same engine
                var query = context.Drivers.Select(d => new { DriverId = d.Id, EngineSupplierId = d.Team.Engine.EngineSupplier.Id }).AsNoTracking();
                var tuple = query.FirstOrDefault();
                var driverId = tuple.DriverId;
                var engineSupplierId = tuple.EngineSupplierId;

                var driver = context.Drivers.Where(d => d.Id == driverId).Single();
                var team = driver.Team;

                var engineSupplier = context.EngineSuppliers.Where(es => es.Id == engineSupplierId).Single();
                var engine = engineSupplier.Engines;

                context.Configuration.LazyLoadingEnabled = false;

                Assert.NotNull(team.Engine);
            }
        }

        [Fact]
        public void Lazy_loading_wires_up_collection_navigations_correctly()
        {
            using (var context = new F1Context())
            {
                var query = context.Sponsors.SelectMany(s => s.Teams, (s, t) => new { SponsorId = s.Id, EngineId = t.Engine.Id }).AsNoTracking();
                var tuple = query.FirstOrDefault();
                var sponsor = context.Sponsors.Where(s => s.Id == tuple.SponsorId).Single();
                var sponsorTeams = sponsor.Teams;

                var engine = context.Engines.Where(e => e.Id == tuple.EngineId).Single();

                context.Configuration.LazyLoadingEnabled = false;

                Assert.True(engine.Teams.Count > 0);
                bool engineTeamsContainSponsorTeam = false;
                foreach (var sponsorTeam in sponsorTeams)
                {
                    if (engine.Teams.Contains(sponsorTeam))
                    {
                        engineTeamsContainSponsorTeam = true;
                    }
                }

                Assert.True(engineTeamsContainSponsorTeam);

                foreach (var engineTeam in engine.Teams)
                {
                    Assert.True(sponsorTeams.Contains(engineTeam));
                }
            }
        }

        [Fact]
        public void Lazy_loading_many_to_many_navigation_works_properly()
        {
            using (var context = new F1Context())
            {
                var teamId = context.Teams.OrderBy(t => t.Id).AsNoTracking().FirstOrDefault().Id;
                var sponsorsId = context.Teams.Where(t => t.Id == teamId).SelectMany(t => t.Sponsors).AsNoTracking().Select(s => s.Id).ToList();

                var team = context.Teams.Where(t => t.Id == teamId).Single();
                var sponsors = team.Sponsors;

                context.Configuration.LazyLoadingEnabled = false;

                foreach (var sponsor in sponsors)
                {
                    Assert.True(sponsorsId.Contains(sponsor.Id));
                }
            }
        }

        #endregion

        #region Proxy creation tests

        private void ValidateProxyCreation(F1Context context, bool proxyCreationEnabled)
        {
            Assert.Equal(proxyCreationEnabled, context.Configuration.ProxyCreationEnabled);
            Assert.Equal(proxyCreationEnabled, GetObjectContext(context).ContextOptions.ProxyCreationEnabled);

            context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);

            var driver = context.Drivers.First();
            if (proxyCreationEnabled)
            {
                Assert.NotEqual(typeof(Driver), driver.GetType());
            }
            else
            {
                Assert.Equal(typeof(Driver), driver.GetType());
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_empty_constructor()
        {
            using (var context = new F1Context())
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_String_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>()))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(CreateF1Model()))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_String_and_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>(), CreateF1Model()))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_DbConnection_constructor()
        {
            using (var context = new F1Context(SimpleConnection<F1Context>(), contextOwnsConnection: true))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_DbConnection_and_DbCompiledModel_constructor()
        {
            using (
                var context = new F1Context(SimpleConnection<F1Context>(), CreateF1Model(), contextOwnsConnection: true)
                )
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_entity_connection_string_in_constructor()
        {
            using (var context = new DbContext(SimpleModelEntityConnectionString))
            {
                Assert.True(context.Configuration.ProxyCreationEnabled);
                var objectContext = GetObjectContext(context);
                Assert.True(objectContext.ContextOptions.ProxyCreationEnabled);
            }
        }

        [Fact]
        public void Proxy_creation_is_on_by_default_when_using_EntityConnection_object_in_constructor()
        {
            using (
                var context = new DbContext(
                    new EntityConnection(SimpleModelEntityConnectionString),
                    contextOwnsConnection: true))
            {
                Assert.True(context.Configuration.ProxyCreationEnabled);
                var objectContext = GetObjectContext(context);
                Assert.True(objectContext.ContextOptions.ProxyCreationEnabled);
            }
        }

        [Fact]
        public void Proxy_creation_flag_can_be_inherited_from_ObjectContext_as_true_when_using_ObjectContext_constructor()
        {
            Proxy_creation_flag_is_inherited_from_ObjectContext_when_using_ObjectContext_constructor(
                proxyCreationEnabled: true);
        }

        [Fact]
        public void
            Proxy_creation_flag_can_be_inherited_from_ObjectContext_as_false_when_using_ObjectContext_constructor()
        {
            Proxy_creation_flag_is_inherited_from_ObjectContext_when_using_ObjectContext_constructor(
                proxyCreationEnabled: false);
        }

        private void Proxy_creation_flag_is_inherited_from_ObjectContext_when_using_ObjectContext_constructor(
            bool proxyCreationEnabled)
        {
            var objectContext = GetObjectContext(new F1Context());
            objectContext.ContextOptions.ProxyCreationEnabled = proxyCreationEnabled;

            using (var context = new F1Context(objectContext, dbContextOwnsObjectContext: true))
            {
                ValidateProxyCreation(context, proxyCreationEnabled);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_switched_off_in_constructor_which_calls_to_base_empty_constructor()
        {
            using (var context = new F1Context(proxyCreationEnabled: false))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: false);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_switched_off_in_constructor_which_calls_to_base_String_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>(), proxyCreationEnabled: false))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: false);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_switched_off_in_constructor_which_calls_to_base_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(CreateF1Model(), proxyCreationEnabled: false))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: false);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_switched_off_in_constructor_which_calls_to_base_String_and_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>(), CreateF1Model(), proxyCreationEnabled: false)
                )
            {
                ValidateProxyCreation(context, proxyCreationEnabled: false);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_switched_off_in_constructor_which_calls_to_base_DbConnection_constructor()
        {
            using (
                var context = new F1Context(
                    SimpleConnection<F1Context>(), contextOwnsConnection: true,
                    proxyCreationEnabled: false))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: false);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_switched_off_in_constructor_which_calls_to_base_DbConnection_and_DbCompiledModel_constructor()
        {
            using (
                var context = new F1Context(
                    SimpleConnection<F1Context>(), CreateF1Model(), contextOwnsConnection: true,
                    proxyCreationEnabled: false))
            {
                ValidateProxyCreation(context, proxyCreationEnabled: false);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_switched_off_in_constructor_which_calls_to_base_ObjectContext_constructor()
        {
            var objectContext = GetObjectContext(new F1Context());
            objectContext.ContextOptions.ProxyCreationEnabled = true;

            using (
                var context = new F1Context(objectContext, dbContextOwnsObjectContext: true, proxyCreationEnabled: false)
                )
            {
                ValidateProxyCreation(context, false);
            }
        }

        public class ProxyCreationFlagTestContext : EmptyContext
        {
            public ProxyCreationFlagTestContext()
                : base(DefaultDbName<EmptyContext>())
            {
            }

            public ProxyCreationFlagTestContext(string connectionString)
                : base(connectionString)
            {
            }

            public ProxyCreationFlagTestContext(DbConnection connection)
                : base(connection, contextOwnsConnection: true)
            {
            }

            public bool ModelCreated { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                ModelCreated = true;
                base.OnModelCreating(modelBuilder);
            }
        }

        [Fact]
        public void Proxy_creation_can_be_changed_after_DbContext_is_created_but_before_initialization_when_using_entity_connection_string()
        {
            Proxy_creation_can_be_changed_after_DbContext_is_created_but_before_initialization(
                () => new ProxyCreationFlagTestContext(SimpleModelEntityConnectionString), expectOnModelCreation: false);
        }

        [Fact]
        public void Proxy_creation_can_be_changed_after_DbContext_is_created_but_before_initialization_when_using_EntityConnection()
        {
            Proxy_creation_can_be_changed_after_DbContext_is_created_but_before_initialization(
                () => new ProxyCreationFlagTestContext(new EntityConnection(SimpleModelEntityConnectionString)),
                expectOnModelCreation: false);
        }

        [Fact]
        public void
            Proxy_creation_can_be_changed_after_DbContext_is_created_without_causing_ObjectContext_initialization_when_using_code_first()
        {
            Proxy_creation_can_be_changed_after_DbContext_is_created_but_before_initialization(
                () => new ProxyCreationFlagTestContext(), expectOnModelCreation: true);
        }

        private void Proxy_creation_can_be_changed_after_DbContext_is_created_but_before_initialization(
            Func<ProxyCreationFlagTestContext> createContext, bool expectOnModelCreation)
        {
            using (var context = createContext())
            {
                Assert.True(context.Configuration.ProxyCreationEnabled);

                context.Configuration.ProxyCreationEnabled = false;
                Assert.False(context.Configuration.ProxyCreationEnabled);

                context.Configuration.ProxyCreationEnabled = true;
                Assert.True(context.Configuration.ProxyCreationEnabled);

                context.Configuration.ProxyCreationEnabled = false;
                Assert.False(context.ModelCreated);

                Assert.Equal(false, GetObjectContext(context).ContextOptions.ProxyCreationEnabled);
                Assert.Equal(expectOnModelCreation, context.ModelCreated);
            }
        }

        [Fact]
        public void
            Changing_proxy_creation_flag_after_ObjectContext_is_initialized_causes_proxy_creation_flag_in_DbContext_and_ObjectContext_to_change
            ()
        {
            using (var context = new F1Context())
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);

                context.Configuration.ProxyCreationEnabled = false;
                ValidateProxyCreation(context, proxyCreationEnabled: false);

                context.Configuration.ProxyCreationEnabled = true;
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        [Fact]
        public void Changing_proxy_creation_flag_in_ObjectContext_causes_proxy_creation_flag_in_DbContext_and_ObjectContext_to_change()
        {
            using (var context = new F1Context())
            {
                ValidateProxyCreation(context, proxyCreationEnabled: true);

                GetObjectContext(context).ContextOptions.ProxyCreationEnabled = false;
                ValidateProxyCreation(context, proxyCreationEnabled: false);

                GetObjectContext(context).ContextOptions.ProxyCreationEnabled = true;
                ValidateProxyCreation(context, proxyCreationEnabled: true);
            }
        }

        #endregion

        #region DetectChanges tests

        [Fact]
        public void AutoDetectChangesEnabled_is_on_by_default_and_can_be_changed()
        {
            using (var context = new F1Context())
            {
                Assert.True(context.Configuration.AutoDetectChangesEnabled);

                context.Configuration.AutoDetectChangesEnabled = false;

                Assert.False(context.Configuration.AutoDetectChangesEnabled);
            }
        }

        [Fact]
        public void Explicitly_calling_DetectChanges_results_in_change_detection()
        {
            TestDetectChanges(c => c.ChangeTracker.DetectChanges(), autoDetectChanges: true);
        }

        [Fact]
        public void
            Explicitly_calling_DetectChanges_results_in_change_detection_even_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.ChangeTracker.DetectChanges(), autoDetectChanges: false, expectDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Find_on_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Teams.Find(Team.Williams), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Add_on_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Drivers.Add(new Driver()), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Attach_on_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Drivers.Attach(new Driver()), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Remove_on_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            Driver driver = null;
            TestDetectChanges(
                c => { driver = c.Drivers.Add(new Driver()); }, c => c.Drivers.Remove(driver),
                autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Local_on_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => { var _ = c.Drivers.Local; }, autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Find_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Set(typeof(Team)).Find(Team.Williams), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Add_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Set(typeof(Driver)).Add(new Driver()), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Attach_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Set(typeof(Driver)).Attach(new Driver()), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Remove_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            Driver driver = null;
            TestDetectChanges(
                c => { driver = c.Drivers.Add(new Driver()); }, c => c.Set(typeof(Driver)).Remove(driver),
                autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_Local_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => { var _ = c.Set(typeof(Driver)).Local; }, autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_non_generic_Entry_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Entry((object)new Driver()), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_generic_Entry_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.Entry(new Driver()), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_non_generic_Entries_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.ChangeTracker.Entries(), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_generic_Entries_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChanges(c => c.ChangeTracker.Entries<Driver>(), autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Find_on_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Teams.Find(Team.Williams), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Add_on_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Drivers.Add(new Driver()), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Attach_on_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Drivers.Attach(new Driver()), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Remove_on_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            Driver driver = null;
            TestDetectChanges(
                c => { driver = c.Drivers.Add(new Driver()); }, c => c.Drivers.Remove(driver),
                autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Local_on_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => { var _ = c.Drivers.Local; }, autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Find_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Set(typeof(Team)).Find(Team.Williams), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Add_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Set(typeof(Driver)).Add(new Driver()), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Attach_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Set(typeof(Driver)).Attach(new Driver()), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Remove_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            Driver driver = null;
            TestDetectChanges(
                c => { driver = c.Drivers.Add(new Driver()); }, c => c.Set(typeof(Driver)).Remove(driver),
                autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_Local_on_non_generic_DbSet_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => { var _ = c.Set(typeof(Driver)).Local; }, autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_non_generic_Entry_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Entry((object)new Driver()), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_generic_Entry_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.Entry(new Driver()), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_non_generic_Entries_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.ChangeTracker.Entries(), autoDetectChanges: false);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_generic_Entries_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChanges(c => c.ChangeTracker.Entries<Driver>(), autoDetectChanges: false);
        }

        private void TestDetectChanges(
            Action<F1Context> actOnContext, bool autoDetectChanges,
            bool? expectDetectChanges = null)
        {
            TestDetectChanges(c => { }, actOnContext, autoDetectChanges, expectDetectChanges);
        }

        private void TestDetectChanges(
            Action<F1Context> setupContext, Action<F1Context> actOnContext,
            bool autoDetectChanges, bool? expectDetectChanges = null)
        {
            using (var context = new F1Context())
            {
                context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;

                setupContext(context);

                var mclaren = context.Teams.Find(Team.McLaren);
                var larryEntry = context.Entry(
                    new Driver
                        {
                            Name = "Larry David"
                        });
                mclaren.Drivers.Add(larryEntry.Entity);

                actOnContext(context);

                Assert.Equal(
                    expectDetectChanges ?? autoDetectChanges ? EntityState.Added : EntityState.Detached,
                    larryEntry.State);
            }
        }

        [Fact]
        public void DetectChanges_is_called_by_SaveChanges_when_AutoDetectChangesEnabled_is_on()
        {
            TestDetectChangesWithSaveChanges(autoDetectChanges: true);
        }

        [Fact]
        public void DetectChanges_is_not_called_by_SaveChanges_when_AutoDetectChangesEnabled_is_off()
        {
            TestDetectChangesWithSaveChanges(autoDetectChanges: false);
        }

        private void TestDetectChangesWithSaveChanges(bool autoDetectChanges)
        {
            using (var context = new F1Context())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    context.Configuration.AutoDetectChangesEnabled = autoDetectChanges;

                    var mclaren = context.Teams.Find(Team.McLaren);
                    var larryEntry = context.Entry(
                        new Driver
                            {
                                Name = "Larry David"
                            });
                    mclaren.Drivers.Add(larryEntry.Entity);

                    Assert.Equal(autoDetectChanges ? EntityState.Added : EntityState.Detached, larryEntry.State);

                    context.SaveChanges();

                    Assert.Equal(autoDetectChanges ? EntityState.Unchanged : EntityState.Detached, larryEntry.State);
                }
            }
        }

        #endregion

        #region ValidateOnSaveEnabled tests

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_empty_constructor()
        {
            using (var context = new F1Context())
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_String_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>()))
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(CreateF1Model()))
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_String_and_DbCompiledModel_constructor()
        {
            using (var context = new F1Context(DefaultDbName<F1Context>(), CreateF1Model()))
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_DbConnection_constructor()
        {
            using (var context = new F1Context(SimpleConnection<F1Context>(), contextOwnsConnection: true))
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_DbConnection_and_DbCompiledModel_constructor()
        {
            using (
                var context = new F1Context(SimpleConnection<F1Context>(), CreateF1Model(), contextOwnsConnection: true)
                )
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_entity_connection_string_in_constructor()
        {
            using (var context = new DbContext(SimpleModelEntityConnectionString))
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_is_on_by_default_when_using_EntityConnection_object_in_constructor()
        {
            using (
                var context = new DbContext(
                    new EntityConnection(SimpleModelEntityConnectionString),
                    contextOwnsConnection: true))
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_flag_is_on_by_default_when_using_ObjectContext_constructor()
        {
            var objectContext = GetObjectContext(new F1Context());
            using (var context = new F1Context(objectContext, dbContextOwnsObjectContext: true))
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        [Fact]
        public void ValidateOnSaveEnabled_can_be_changed()
        {
            using (var context = new F1Context())
            {
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
                context.Configuration.ValidateOnSaveEnabled = false;
                Assert.False(context.Configuration.ValidateOnSaveEnabled);
                context.Configuration.ValidateOnSaveEnabled = true;
                Assert.True(context.Configuration.ValidateOnSaveEnabled);
            }
        }

        public class ValidationTestContext : DbContext
        {
            public DbSet<Category> Categories { get; set; }

            public Func<DbEntityEntry, DbEntityValidationResult> ValidateEntityFunc { get; set; }

            protected override bool ShouldValidateEntity(DbEntityEntry entityEntry)
            {
                return true;
            }

            protected override DbEntityValidationResult ValidateEntity(
                DbEntityEntry entityEntry,
                IDictionary<object, object> items)
            {
                if (ValidateEntityFunc != null)
                {
                    return ValidateEntityFunc(entityEntry);
                }
                return new DbEntityValidationResult(entityEntry, Enumerable.Empty<DbValidationError>());
            }
        }

        [Fact]
        public void ValidateEntity_is_called_by_SaveChanges_when_ValidateOnSaveEnabled_is_on()
        {
            TestValidateEntityWithSaveChanges(validateOnSaveEnabled: true);
        }

        [Fact]
        public void ValidateEntity_is_not_called_by_SaveChanges_when_ValidateOnSaveEnabled_is_off()
        {
            TestValidateEntityWithSaveChanges(validateOnSaveEnabled: false);
        }

        private void TestValidateEntityWithSaveChanges(bool validateOnSaveEnabled)
        {
            using (var context = new ValidationTestContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    context.Configuration.ValidateOnSaveEnabled = validateOnSaveEnabled;
                    var validateCalled = false;

                    context.ValidateEntityFunc = (entry) =>
                                                     {
                                                         validateCalled = true;
                                                         return new DbEntityValidationResult(
                                                             entry,
                                                             Enumerable.Empty
                                                                 <DbValidationError>());
                                                     };
                    context.Categories.Add(new Category("FOOD"));
                    context.SaveChanges();

                    Assert.True(validateOnSaveEnabled == validateCalled);
                }
            }
        }

        [Fact]
        public void DetectChanges_is_called_by_SaveChanges_once_when_ValidateOnSaveEnabled_is_on()
        {
            TestDetectChangesWithSaveChangesAndValidation(validateOnSaveEnabled: true);
        }

        [Fact]
        public void DetectChanges_is_called_by_SaveChanges_once_when_ValidateOnSaveEnabled_is_off()
        {
            TestDetectChangesWithSaveChangesAndValidation(validateOnSaveEnabled: false);
        }

        private void TestDetectChangesWithSaveChangesAndValidation(bool validateOnSaveEnabled)
        {
            using (var context = new ValidationTestContext())
            {
                context.Database.Initialize(force: false);

                using (new TransactionScope())
                {
                    context.Configuration.ValidateOnSaveEnabled = validateOnSaveEnabled;

                    var food = context.Entry(new Category("FOOD"));
                    context.Categories.Add(food.Entity);
                    context.SaveChanges();

                    Assert.Equal(null, food.Entity.DetailedDescription);
                    Assert.Equal(EntityState.Unchanged, food.State);

                    food.Entity.DetailedDescription = "foo";
                    Assert.Equal(EntityState.Unchanged, food.State);

                    context.ValidateEntityFunc = (entry) =>
                                                     {
                                                         Assert.Equal(
                                                             validateOnSaveEnabled
                                                                 ? EntityState.Modified
                                                                 : EntityState.Unchanged, entry.State);
                                                         entry.State = EntityState.Unchanged;

                                                         food.Entity.DetailedDescription = "bar";
                                                         Assert.Equal(EntityState.Unchanged, entry.State);

                                                         return new DbEntityValidationResult(
                                                             entry,
                                                             Enumerable.Empty
                                                                 <DbValidationError>());
                                                     };

                    context.SaveChanges();
                    Assert.Equal(validateOnSaveEnabled ? "bar" : "foo", food.Entity.DetailedDescription);

                    food.Reload();
                    Assert.Equal(validateOnSaveEnabled ? null : "foo", food.Entity.DetailedDescription);
                }
            }
        }

        #endregion

        #region Replace connection tests

        [Fact]
        public void Can_replace_connection()
        {
            using (var context = new ReplaceConnectionContext())
            {
                using (var newConnection = new LazyInternalConnection(
                    new DbConnectionInfo(
                        SimpleConnectionString("NewReplaceConnectionContextDatabase"),
                        "System.Data.SqlClient")))
                {
                    Can_replace_connection_implementation(context, newConnection);
                }
            }
        }

        [Fact]
        public void Can_replace_connection_with_different_provider()
        {
            using (var context = new ReplaceConnectionContext())
            {
                using (var newConnection = new LazyInternalConnection(
                    new DbConnectionInfo(
                        "Data Source=NewReplaceConnectionContextDatabase.sdf",
                        "System.Data.SqlServerCe.4.0")))
                {
                    Can_replace_connection_implementation(context, newConnection);
                }
            }
        }

        private void Can_replace_connection_implementation(
            ReplaceConnectionContext context,
            LazyInternalConnection newConnection)
        {
            Database.Delete(newConnection.Connection);
            Database.Delete(typeof(ReplaceConnectionContext).DatabaseName());

            context.InternalContext.OverrideConnection(newConnection);

            context.Entities.Add(
                new PersistEntity
                    {
                        Name = "Testing"
                    });
            context.SaveChanges();

            Assert.Same(newConnection.Connection, context.Database.Connection);
            Assert.True(Database.Exists(newConnection.Connection));
            Assert.False(Database.Exists(typeof(ReplaceConnectionContext).DatabaseName()));

            // By pass EF just to make sure everything targetted the correct database
            var cmd = newConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM PersistEntities";
            cmd.Connection.Open();
            Assert.Equal(1, cmd.ExecuteScalar());
            cmd.Connection.Close();
        }

        public class ReplaceConnectionContext : DbContext
        {
            public DbSet<PersistEntity> Entities { get; set; }
        }

        public class PersistEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion

        #region Test EntityConnection-Store Connection state correlation when opening EntityConnection implicitly through context

        [Fact]
        public void Implicit_EntityConnection_throws_if_close_underlying_StoreConnection()
        {
            using (var context = new SimpleModelContext())
            {
                EntityConnection entityConnection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;

                Assert.True(context.Products.Count() >= 2, "Need at least 2 product entries for test to work below");

                ConnectionEventsTracker dbConnectionTracker = new ConnectionEventsTracker(entityConnection.StoreConnection);
                ConnectionEventsTracker entityConnectionTracker = new ConnectionEventsTracker(entityConnection);

                var query = from p in context.Products
                            select p.Name;

                query = query.AsStreaming();

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                IEnumerator<string> enumerator = query.GetEnumerator();
                enumerator.MoveNext();

                // close the underlying store connection without explicitly closing entityConnection
                // (but entityConnection state is updated automatically)
                entityConnection.StoreConnection.Close();
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // verify that the open and close events have been fired once and only once on both EntityConnection and underlying DbConnection
                dbConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();
                entityConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();

                // check that we throw when we attempt to use the implicitly-opened entityConnection with closed underlying store connection
                Assert.Throws<EntityCommandExecutionException>(() => enumerator.MoveNext()).ValidateMessage("ADP_DataReaderClosed", "Read");

                enumerator.Dispose();
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // verify that the open and close events are not fired again by the second MoveNext() above
                dbConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();
                entityConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();

                // prove that can still re-use the connection even after the above
                Assert.True(context.Products.Count() > 0); // this will check that the query will still execute

                // and show that the entity connection and the store connection are once again closed
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
            }
        }

        [Fact]
        public void Implicit_EntityConnection_throws_if_close_EntityConnection_during_query()
        {
            using (var context = new SimpleModelContext())
            {
                EntityConnection entityConnection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;

                Assert.True(context.Products.Count() >= 2, "Need at least 2 product entries for test to work below");

                ConnectionEventsTracker dbConnectionTracker = new ConnectionEventsTracker(entityConnection.StoreConnection);
                ConnectionEventsTracker entityConnectionTracker = new ConnectionEventsTracker(entityConnection);

                var query = from p in context.Products
                            select p.Name;

                query = query.AsStreaming();

                Assert.Equal(ConnectionState.Closed, entityConnection.State); // entityConnection state
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State); // underlying storeConnection state

                IEnumerator<string> enumerator = query.GetEnumerator();
                enumerator.MoveNext();

                // close the entity connection explicitly (i.e. not through context) in middle of query
                entityConnection.Close();
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // verify that the open and close events have been fired once and only once on both EntityConnection and underlying DbConnection
                dbConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();
                entityConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();

                // check that we throw when we attempt to use the implicitly-opened entityConnection
                Assert.Throws<EntityCommandExecutionException>(() => enumerator.MoveNext()).ValidateMessage("ADP_DataReaderClosed", "Read");

                enumerator.Dispose();
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);

                // verify that the open and close events are not fired again by the second MoveNext() above
                dbConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();
                entityConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();

                // prove that can still re-use the connection even after the above
                Assert.True(context.Products.Count() > 0); // this will check that the query will still execute

                // and show that the entity connection and the store connection are once again closed
                Assert.Equal(ConnectionState.Closed, entityConnection.State);
                Assert.Equal(ConnectionState.Closed, entityConnection.StoreConnection.State);
            }
        }

        [Fact]
        public void EntityConnection_StateChangeEvents_are_fired_when_state_changes()
        {
            using (var context = new SimpleModelContext())
            {
                EntityConnection entityConnection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;

                var dbConnectionTracker = new ConnectionEventsTracker(entityConnection.StoreConnection);
                var entityConnectionTracker = new ConnectionEventsTracker(entityConnection);

                // verify that the open and close events have not been fired yet
                dbConnectionTracker.VerifyNoConnectionEventsWereFired();
                entityConnectionTracker.VerifyNoConnectionEventsWereFired();

                var storeConnection = entityConnection.StoreConnection;
                storeConnection.Open();

                // verify the open event has been fired on the store connection and the EntityConnection
                // was subscribed so it updated too
                dbConnectionTracker.VerifyConnectionOpenedEventWasFired();
                entityConnectionTracker.VerifyConnectionOpenedEventWasFired();

                storeConnection.Close();

                // verify the close event has been fired on the store connection and the EntityConnection
                // was subscribed so it updated too
                dbConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();
                entityConnectionTracker.VerifyConnectionOpenCloseEventsWereFired();
            }
        }
        #endregion
    }
}
