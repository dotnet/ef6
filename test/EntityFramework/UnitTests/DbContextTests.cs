// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Linq;
    using Moq;
    using Moq.Protected;
    using Xunit;

    #region Context types for testing database name generation

    internal class SimpleContextClass : DbContext
    {
        internal class NestedContextClass : DbContext
        {
            internal class DouubleNestedContextClass : DbContext
            {
            }
        }

        internal class GenericContextClass<T> : DbContext
        {
        }

        internal class DoubleGenericContextClass<T1, T2> : DbContext
        {
        }
    }

    internal class GenericContextClass<T> : DbContext
    {
        internal class NestedContextClass : DbContext
        {
        }
    }

    internal class DoubleGenericContextClass<T1, T2> : DbContext
    {
        internal class NestedContextClass : DbContext
        {
        }

        internal class DoubleGenericNestedContextClass<T3, T4> : DbContext
        {
            internal class DoubleGenericDoubleNestedContextClass<T5, T6> : DbContext
            {
            }
        }
    }

    #endregion

    /// <summary>
    ///     Unit tests for DbContext.
    /// </summary>
    public class DbContextTests : TestBase
    {
        static DbContextTests()
        {
            // Ensure the basic-auth test user exists

            using (var connection = new SqlConnection(SimpleConnectionString("master")))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText
                        = @"IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'EFTestUser')
BEGIN
  CREATE LOGIN [EFTestUser] WITH PASSWORD=N'Password1', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
  EXEC sys.sp_addsrvrolemember @loginame = N'EFTestUser', @rolename = N'sysadmin'
END";
                    command.ExecuteNonQuery();
                }
            }
        }

        #region Using EF connection string/EntityConnection in combination with DbCompiledModel

        private const string EntityConnectionString =
            @"metadata=.\Foo.csdl|.\Foo.ssdl|.\Foo.msl;provider=System.Data.SqlClient;provider connection string='Server=.\Foo;Database=Bar'";

        private static DbCompiledModel _emptyModel;

        private static DbCompiledModel EmptyModel
        {
            get { return _emptyModel ?? (_emptyModel = new DbModelBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile()); }
        }

        [Fact]
        public void Entity_connection_string_and_DbCompiledModel_used_together_throw()
        {
            using (var context = new DbContext(EntityConnectionString, EmptyModel))
            {
                Assert.Equal(
                    Strings.DbContext_ConnectionHasModel,
                    Assert.Throws<InvalidOperationException>(() => context.Set<FakeEntity>().Load()).Message);
            }
        }

        [Fact]
        public void Named_entity_connection_string_and_DbCompiledModel_used_together_throw()
        {
            using (var context = new DbContext("EntityConnectionString", EmptyModel))
            {
                Assert.Equal(
                    Strings.DbContext_ConnectionHasModel,
                    Assert.Throws<InvalidOperationException>(() => context.Set<FakeEntity>().Load()).Message);
            }
        }

        [Fact]
        public void Named_entity_connection_string_using_name_keyword_and_DbCompiledModel_used_together_throw()
        {
            using (var context = new DbContext("name=EntityConnectionString", EmptyModel))
            {
                Assert.Equal(
                    Strings.DbContext_ConnectionHasModel,
                    Assert.Throws<InvalidOperationException>(() => context.Set<FakeEntity>().Load()).Message);
            }
        }

        [Fact]
        public void EntitConnection_object_and_DbCompiledModel_used_together_throw()
        {
            using (var context = new DbContext(new EntityConnection(EntityConnectionString), EmptyModel, contextOwnsConnection: true))
            {
                Assert.Equal(
                    Strings.DbContext_ConnectionHasModel,
                    Assert.Throws<InvalidOperationException>(() => context.Set<FakeEntity>().Load()).Message);
            }
        }

        #endregion

        #region Bad connection string tests

        [Fact]
        public void Using_name_keyword_throws_if_name_not_found_in_app_config()
        {
            using (var context = new FakeDerivedDbContext("name=MissingConnectionString"))
            {
                Assert.Equal(
                    Strings.DbContext_ConnectionStringNotFound("MissingConnectionString"),
                    Assert.Throws<InvalidOperationException>(() => context.Set<FakeEntity>().Load()).Message);
            }
        }

        [Fact]
        public void Using_name_keyword_throws_if_name_not_found_in_app_config_even_if_DbCompiledModel_passed()
        {
            using (var context = new DbContext("name=MissingConnectionString", EmptyModel))
            {
                Assert.Equal(
                    Strings.DbContext_ConnectionStringNotFound("MissingConnectionString"),
                    Assert.Throws<InvalidOperationException>(() => context.Set<FakeEntity>().Load()).Message);
            }
        }

        #endregion

        #region Negative constructor tests

        [Fact]
        public void Constructing_DbContext_with_null_nameOrConnectionString_throws()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                Assert.Throws<ArgumentException>(() => new FakeDerivedDbContext((string)null)).Message);
        }

        [Fact]
        public void Constructing_DbContext_with_empty_nameOrConnectionString_throws()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                Assert.Throws<ArgumentException>(() => new FakeDerivedDbContext("")).Message);
        }

        [Fact]
        public void Constructing_DbContext_with_whitespace_nameOrConnectionString_throws()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"),
                Assert.Throws<ArgumentException>(() => new FakeDerivedDbContext(" ")).Message);
        }

        [Fact]
        public void Constructing_DbContext_with_null_existingConnection_throws()
        {
            Assert.Equal(
                "existingConnection", Assert.Throws<ArgumentNullException>(() => new FakeDerivedDbContext((DbConnection)null)).ParamName);
        }

        [Fact]
        public void Constructing_DbContext_with_null_objectContext_throws()
        {
            Assert.Equal(
                "objectContext",
                Assert.Throws<ArgumentNullException>(() => new DbContext(null, dbContextOwnsObjectContext: false)).ParamName);
        }

        [Fact]
        public void Constructing_DbContext_with_null_model_using_model_only_constructor_throws()
        {
            Assert.Equal("model", Assert.Throws<ArgumentNullException>(() => new FakeDerivedDbContext((DbCompiledModel)null)).ParamName);
        }

        [Fact]
        public void Constructing_DbContext_with_null_model_using_nameOrConnectionString_constructor_throws()
        {
            Assert.Equal("model", Assert.Throws<ArgumentNullException>(() => new DbContext("Johnny Rotten", null)).ParamName);
        }

        [Fact]
        public void Constructing_DbContext_with_null_model_using_existingConnection_constructor_throws()
        {
            Assert.Equal(
                "model",
                Assert.Throws<ArgumentNullException>(() => new DbContext(new SqlConnection(), null, contextOwnsConnection: false)).ParamName);
        }

        #endregion

        #region Virtual Dispose tests

        [Fact]
        public void Dispose_can_be_overriden_in_a_derived_DbContext()
        {
            var mockContext = new Mock<DbContext>();
            mockContext.Protected().Setup("Dispose", true).Verifiable();

            mockContext.Object.Dispose();

            mockContext.Verify();
        }

        #endregion

        #region Tests for DbModelBuilderVersionAttribute

        [Fact]
        public void DbModelBuilderVersion_Latest_is_used_if_no_attribute_is_provided()
        {
            var internalContext = new LazyInternalContext(
                new Mock<DbContext>().Object,
                new Mock<IInternalConnection>().Object, null);

            var builder = internalContext.CreateModelBuilder();

            Assert.Equal(DbModelBuilderVersion.Latest, builder.Version);
        }

        [Fact]
        public void Can_read_version_from_DbModelBuilderVersionAttribute()
        {
            var attribute = new DbModelBuilderVersionAttribute(DbModelBuilderVersion.Latest);

            Assert.Equal(DbModelBuilderVersion.Latest, attribute.Version);
        }

        [DbModelBuilderVersion(DbModelBuilderVersion.V4_1)]
        public class FakeWithAttribute : DbContext
        {
        }

        [Fact]
        public void Version_in_DbModelBuilderVersionAttribute_is_used_if_provided()
        {
            var internalContext = new LazyInternalContext(new FakeWithAttribute(), new Mock<IInternalConnection>().Object, null);

            var builder = internalContext.CreateModelBuilder();

            Assert.Equal(DbModelBuilderVersion.V4_1, builder.Version);
        }

        [DbModelBuilderVersion(DbModelBuilderVersion.Latest)]
        public class FakeWithAttributeDerived : FakeWithAttribute
        {
        }

        [Fact]
        public void Version_from_DbModelBuilderVersionAttribute_on_derived_context_overrides_version_from_base()
        {
            var internalContext = new LazyInternalContext(new FakeWithAttributeDerived(), new Mock<IInternalConnection>().Object, null);

            var builder = internalContext.CreateModelBuilder();

            Assert.Equal(DbModelBuilderVersion.Latest, builder.Version);
        }

        public class FakeWithNoAttributeDerived : FakeWithAttribute
        {
        }

        [Fact]
        public void Version_from_DbModelBuilderVersionAttribute_on_base_context_is_used_if_no_attribute_is_found_on_derived_context()
        {
            var internalContext = new LazyInternalContext(new FakeWithNoAttributeDerived(), new Mock<IInternalConnection>().Object, null);

            var builder = internalContext.CreateModelBuilder();

            Assert.Equal(DbModelBuilderVersion.V4_1, builder.Version);
        }

        #endregion

        #region Tests for creating database names from context class names

        [Fact]
        public void Simple_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.SimpleContextClass",
                typeof(SimpleContextClass).DatabaseName());
        }

        [Fact]
        public void Nested_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.SimpleContextClass+NestedContextClass",
                typeof(SimpleContextClass.NestedContextClass).DatabaseName());
        }

        [Fact]
        public void Double_nested_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.SimpleContextClass+NestedContextClass+DouubleNestedContextClass",
                typeof(SimpleContextClass.NestedContextClass.DouubleNestedContextClass).DatabaseName());
        }

        [Fact]
        public void Generic_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.GenericContextClass`1[System.String]",
                typeof(GenericContextClass<string>).DatabaseName());
        }

        [Fact]
        public void Double_generic_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.DoubleGenericContextClass`2[System.String,System.Int32]",
                typeof(DoubleGenericContextClass<string, int>).DatabaseName());
        }

        [Fact]
        public void Nested_in_generic_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.GenericContextClass`1+NestedContextClass[System.String]",
                typeof(GenericContextClass<string>.NestedContextClass).DatabaseName());
        }

        [Fact]
        public void Nested_in_double_generic_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.DoubleGenericContextClass`2+NestedContextClass[System.String,System.Int32]",
                typeof(DoubleGenericContextClass<string, int>.NestedContextClass).DatabaseName());
        }

        [Fact]
        public void Double_generic_double_nested_in_double_generic_context_class_can_be_used_to_create_database_name()
        {
            Assert.Equal(
                "ProductivityApiUnitTests.DoubleGenericContextClass`2+DoubleGenericNestedContextClass`2+DoubleGenericDoubleNestedContextClass`2[System.String,System.Int32,System.Collections.Generic.ICollection`1[System.String],System.Random,System.Collections.Generic.Dictionary`2[System.String,System.Int32],System.Nullable`1[System.Int32]]",
                typeof(
                    DoubleGenericContextClass<string, int>.DoubleGenericNestedContextClass<ICollection<string>, Random>.
                        DoubleGenericDoubleNestedContextClass<Dictionary<string, int>, int?>).DatabaseName());
        }

        #endregion

        #region PersistSecurityInfo tests

        private class PersistSecurityInfoContext : DbContext
        {
            static PersistSecurityInfoContext()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<PersistSecurityInfoContext>());
            }

            public PersistSecurityInfoContext(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }

            public PersistSecurityInfoContext(DbConnection existingConnection, bool contextOwnsConnection)
                : base(existingConnection, contextOwnsConnection)
            {
            }

            public PersistSecurityInfoContext(ObjectContext objectContext, bool dbContextOwnsObjectContext)
                : base(objectContext, dbContextOwnsObjectContext)
            {
            }

            public IDbSet<PersistEntity> Entities { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<PersistEntity>().ToTable(DateTime.Now.Ticks.ToString());
            }
        }

        public class PersistEntity
        {
            public int Id { get; set; }
        }

        [Fact]
        public void Can_initialize_database_when_using_secure_connection_string_with_sql_server_authentication_and_lazy_connection()
        {
            var connectionString
                = SimpleConnectionStringWithCredentials(
                    "PersistSecurityInfoContext",
                    "EFTestUser",
                    "Password1");

            var context = new PersistSecurityInfoContext(connectionString);

            try
            {
                context.Database.Initialize(true);
            }
            finally
            {
                context.Database.Delete();
            }

            Assert.Equal(
                new SqlConnectionStringBuilder(connectionString).Password,
                new SqlConnectionStringBuilder(context.Database.Connection.ConnectionString).Password);
        }

        [Fact]
        public void Can_initialize_database_when_using_secure_connection_string_with_sql_server_authentication_and_eager_connection()
        {
            var connectionString
                = SimpleConnectionStringWithCredentials(
                    "PersistSecurityInfoContext",
                    "EFTestUser",
                    "Password1");

            var context = new PersistSecurityInfoContext(new SqlConnection(connectionString), true);

            try
            {
                context.Database.Delete();

                context.Database.Initialize(true);
            }
            finally
            {
                context.Database.Delete();
            }

            Assert.Equal(
                new SqlConnectionStringBuilder(connectionString).Password,
                new SqlConnectionStringBuilder(context.Database.Connection.ConnectionString).Password);
        }

        [Fact]
        public void Can_use_ddl_ops_when_using_secure_connection_string_with_sql_server_authentication_and_eager_context()
        {
            var connectionString
                = SimpleConnectionStringWithCredentials(
                    "PersistSecurityInfoContext",
                    "EFTestUser",
                    "Password1");

            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<PersistEntity>().ToTable(DateTime.Now.Ticks.ToString());
            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);
            var entityConnection
                = new EntityConnection(model.DatabaseMapping.ToMetadataWorkspace(), new SqlConnection(connectionString));

            var objectContext = new ObjectContext(entityConnection);

            var context = new PersistSecurityInfoContext(objectContext, true);

            try
            {
                context.Database.Delete();

                context.Database.Create();
                
                Assert.Equal(
                    new SqlConnectionStringBuilder(connectionString).Password,
                    new SqlConnectionStringBuilder(context.Database.Connection.ConnectionString).Password);

                context.Set<PersistEntity>().ToList();

                context.Database.Delete();
            }
            finally
            {
                context.Database.Delete();
            }
        }

        #endregion

        #region Replace connection tests

        [Fact]
        public void Can_replace_connection()
        {
            var intializer = new ReplaceConnectionInitializer();
            Database.SetInitializer(intializer);

            using (var context = new ReplaceConnectionContext())
            {
                using (var newConnection =
                    new LazyInternalConnection(
                        new DbConnectionInfo(
                            @"Server=.\SQLEXPRESS;Database=NewReplaceConnectionContextDatabase;Trusted_Connection=True;",
                            "System.Data.SqlClient")))
                {
                    context.InternalContext.OverrideConnection(newConnection);

                    Assert.IsType<SqlConnection>(context.Database.Connection);
                    Assert.Equal("NewReplaceConnectionContextDatabase", context.Database.Connection.Database);

                    context.Database.Initialize(force: true);
                    Assert.Equal(
                        "NewReplaceConnectionContextDatabase",
                        intializer.DatabaseNameUsedDuringInitialization);
                }
            }
        }

        [Fact]
        public void Can_replace_connection_with_different_provider()
        {
            var intializer = new ReplaceConnectionInitializer();
            Database.SetInitializer(intializer);

            using (var context = new ReplaceConnectionContext())
            {
                using (var newConnection =
                    new LazyInternalConnection(
                        new DbConnectionInfo(
                            "Data Source=NewReplaceConnectionContextDatabase.sdf",
                            "System.Data.SqlServerCe.4.0")))
                {
                    context.InternalContext.OverrideConnection(newConnection);

                    Assert.IsType<SqlCeConnection>(context.Database.Connection);
                    Assert.Equal(
                        "NewReplaceConnectionContextDatabase.sdf",
                        context.Database.Connection.Database);

                    context.Database.Initialize(force: true);
                    Assert.Equal(
                        "NewReplaceConnectionContextDatabase.sdf",
                        intializer.DatabaseNameUsedDuringInitialization);
                }
            }
        }

        [Fact]
        public void Exception_replacing_DbConnection_with_EntityConnection()
        {
            using (var context = new ReplaceConnectionContext())
            {
                using (var newConnection = new EagerInternalConnection(new EntityConnection(), connectionOwned: true))
                {
                    Assert.Equal(
                        Strings.LazyInternalContext_CannotReplaceDbConnectionWithEfConnection,
                        Assert.Throws<InvalidOperationException>(() => context.InternalContext.OverrideConnection(newConnection)).Message);
                }
            }
        }

        [Fact]
        public void Exception_replacing_EntityConnection_with_DbConnection()
        {
            using (var context = new ReplaceConnectionContext("name=EntityConnectionString"))
            {
                using (var newConnection = new LazyInternalConnection("ByConventionName"))
                {
                    Assert.Equal(
                        Strings.LazyInternalContext_CannotReplaceEfConnectionWithDbConnection,
                        Assert.Throws<InvalidOperationException>(() => context.InternalContext.OverrideConnection(newConnection)).Message);
                }
            }
        }

        [Fact]
        public void Exception_replacing_connection_on_eager_context()
        {
            using (var connection = new SqlConnection(@"Server=.\SQLEXPRESS;Database=MigrationInitFromConfig;Trusted_Connection=True;"))
            {
                var ctx = new DbModelBuilder()
                    .Build(connection)
                    .Compile()
                    .CreateObjectContext<ObjectContext>(connection);

                using (var context = new DbContext(ctx, dbContextOwnsObjectContext: true))
                {
                    using (var newConnection = new LazyInternalConnection("ByConventionName"))
                    {
                        Assert.Equal(
                            Strings.EagerInternalContext_CannotSetConnectionInfo,
                            Assert.Throws<InvalidOperationException>(() => context.InternalContext.OverrideConnection(newConnection)).
                                   Message);
                    }
                }
            }
        }

        [Fact]
        public void Replacing_connection_does_not_initialize_either_connection()
        {
            using (var origConnection = new LazyInternalConnection("OrigName"))
            {
                using (var newConnection = new LazyInternalConnection("NewName"))
                {
                    var context = new LazyInternalContext(new ReplaceConnectionContext(), origConnection, null);
                    context.OverrideConnection(newConnection);

                    Assert.False(origConnection.IsInitialized);
                    Assert.False(newConnection.IsInitialized);
                }
            }
        }

        public class ReplaceConnectionContext : DbContext
        {
            public ReplaceConnectionContext()
            {
            }

            public ReplaceConnectionContext(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }
        }

        public class ReplaceConnectionInitializer : IDatabaseInitializer<ReplaceConnectionContext>
        {
            public string DatabaseNameUsedDuringInitialization { get; set; }

            public void InitializeDatabase(ReplaceConnectionContext context)
            {
                if (DatabaseNameUsedDuringInitialization != null)
                {
                    throw new Exception("Initialization already performed!");
                }

                DatabaseNameUsedDuringInitialization = context.Database.Connection.Database;
            }
        }

        #endregion

        #region Provider tests

        [Fact]
        public void ProviderName_gets_name_from_DbConnection_when_eager_context_is_used()
        {
            var mockContext = new Mock<EagerInternalContext>(new Mock<DbContext>().Object)
                                  {
                                      CallBase = true
                                  };
            mockContext.Setup(m => m.Connection).Returns(new SqlConnection());

            Assert.Equal("System.Data.SqlClient", mockContext.Object.ProviderName);
            mockContext.Verify(m => m.Connection, Times.Once());
        }

        [Fact]
        public void ProviderName_gets_name_from_internal_connection_ProviderName_when_lazy_context_is_used()
        {
            var mockConnection = new Mock<IInternalConnection>();
            var mockContext = new Mock<LazyInternalContext>(new Mock<DbContext>().Object, mockConnection.Object, null, null, null, null)
                                  {
                                      CallBase = true
                                  };
            mockConnection.Setup(m => m.ProviderName).Returns("SomeLazyProvider");

            Assert.Equal("SomeLazyProvider", mockContext.Object.ProviderName);
            mockContext.Verify(m => m.Connection, Times.Never());
        }

        [Fact]
        public void Can_get_metadata_with_minimal_DbConnection_implementation()
        {
            // Required for use with MVC Scaffolding that uses Microsoft.VisualStudio.Web.Mvc.Scaffolding.BuiltIn.ScaffoldingDbConnection
            // which is a very minimal implementation of DbConnection.
            using (var connection = new FakeSqlConnection
                                        {
                                            ConnectionString = ""
                                        })
            {
                using (var context = new EmptyContext(connection))
                {
                    var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
                    Assert.NotNull(metadata);
                }
            }
        }

        #endregion

        #region Set tests

        [Fact]
        public void Passing_null_type_to_Non_generic_Set_method_throws()
        {
            var context = new Mock<InternalContextForMock>
                              {
                                  CallBase = true
                              }.Object.Owner;
            Assert.Equal("entityType", Assert.Throws<ArgumentNullException>(() => context.Set(null)).ParamName);
        }

        #endregion

        #region Other tests
        public class UseCSharpNullComparisonBehaviorDbContext : DbContext
        {
        }

        [Fact]
        public static void UseCSharpNullComparisonBehavior_is_retrieved_and_set_correctly()
        {
            Database.SetInitializer<UseCSharpNullComparisonBehaviorDbContext>(null);

            using (var dbContext = new UseCSharpNullComparisonBehaviorDbContext())
            {
                var objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;

                Assert.True(dbContext.Configuration.UseDatabaseNullSemantics);
                Assert.False(objectContext.ContextOptions.UseCSharpNullComparisonBehavior);

                dbContext.Configuration.UseDatabaseNullSemantics = false;

                Assert.False(dbContext.Configuration.UseDatabaseNullSemantics);
                Assert.True(objectContext.ContextOptions.UseCSharpNullComparisonBehavior);

                dbContext.Configuration.UseDatabaseNullSemantics = true;

                Assert.True(dbContext.Configuration.UseDatabaseNullSemantics);
                Assert.False(objectContext.ContextOptions.UseCSharpNullComparisonBehavior);
            }
        }
        #endregion
    }
}
