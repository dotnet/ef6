// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Globalization;
    using System.IO;
    using FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns;
    using FunctionalTests.ProductivityApi.TemplateModels.CsMonsterModel;
    using ProductivityApiTests;
    using SimpleModel;
    using Xunit;

    /// <summary>
    ///     Base class for Productivity API tests that sets up a simple model and some data.
    ///     Call ClassInitializeBase from the ClassInitialize method of your test class to ensure
    ///     that the test data is configured.
    /// </summary>
    public class FunctionalTestBase : TestBase
    {
        #region Test database setup

        protected int AdvancedModelOfficeCount
        {
            get { return 4; }
        }

        protected int AdvancedModelBuildingCount
        {
            get { return 2; }
        }

        protected int AdvancedModelWhiteboardCount
        {
            get { return 3; }
        }

        /// <summary>
        ///     Ensures the database for the context is created and seeded.  This is typically used
        ///     when a test is going to use a transaction to ensure that the DDL happens outside of
        ///     the transaction.
        /// </summary>
        /// <param name="createContext"> A func to create the context. </param>
        protected static void EnsureDatabaseInitialized(Func<DbContext> createContext)
        {
            using (var context = createContext())
            {
                context.Database.Initialize(force: false);
            }
        }

        /// <summary>
        ///     Drops the database that would be used for the context. Usually used to avoid errors during initialization.
        /// </summary>
        /// <param name="createContext"> A func to create the context. </param>
        protected static void DropDatabase(Func<DbContext> createContext)
        {
            using (var context = createContext())
            {
                context.Database.Delete();
            }
        }

        /// <summary>
        ///     Drops and then initializes the database that will be used for the context.
        /// </summary>
        /// <param name="createContext"> A func to create the context. </param>
        protected static void ResetDatabase(Func<DbContext> createContext)
        {
            DropDatabase(createContext);
            EnsureDatabaseInitialized(createContext);
        }

        /// <summary>
        ///     Initializes the metadata files and creates databases for existing CSDL/EDMX files.
        /// </summary>
        protected static void InitializeModelFirstDatabases()
        {
            const string prefix = "FunctionalTests.ProductivityApi.TemplateModels.Schemas.";
            ResourceUtilities.CopyEmbeddedResourcesToCurrentDir(
                typeof(TemplateTests).Assembly,
                prefix,
                /*overwrite*/ true,
                "AdvancedPatterns.edmx",
                "MonsterModel.csdl",
                "MonsterModel.msl",
                "MonsterModel.ssdl");

            // Extract the csdl, msl, and ssdl from the edmx so that they can be referenced in the connection string.
            ModelHelpers.WriteMetadataFiles(File.ReadAllText(@".\AdvancedPatterns.edmx"), @".\AdvancedPatterns");

            using (var context = new AdvancedPatternsModelFirstContext())
            {
                context.Database.Initialize(force: false);
            }

            using (var context = new MonsterModel())
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<MonsterModel>());
                context.Database.Initialize(force: false);
            }
        }

        private static bool _metadataForSimpleModelCreated;

        /// <summary>
        ///     Creates the metadata files (CSDL/SSDL/MSL) for the SimpleModelContext.
        /// </summary>
        protected static void CreateMetadataFilesForSimpleModel()
        {
            if (!_metadataForSimpleModelCreated)
            {
                var builder = SimpleModelContext.CreateBuilder();
                ModelHelpers.WriteMetadataFiles(builder, @".\SimpleModel");

                using (var connection = SimpleConnection<SimpleModelContext>())
                {
                    new SimpleModelContext(connection, builder.Build(connection).Compile()).Database.Initialize(false);
                }

                _metadataForSimpleModelCreated = true;
            }
        }

        private static string _simpleModelEntityConnectionString;

        /// <summary>
        ///     An entity connection string for the SimpleModelContext.
        /// </summary>
        protected static string SimpleModelEntityConnectionString
        {
            get
            {
                const string baseConnectionString =
                    @"metadata=.\SimpleModel.csdl|.\SimpleModel.ssdl|.\SimpleModel.msl;
                                                  provider=System.Data.SqlClient;provider connection string='{0}'";
                return _simpleModelEntityConnectionString ??
                       (_simpleModelEntityConnectionString =
                        String.Format(
                            CultureInfo.InvariantCulture, baseConnectionString,
                            SimpleConnectionString<SimpleModelContext>()));
            }
        }

        #endregion

        #region Construction helpers

        protected static TContext CreateContext<TContext>(
            DbContextConstructorArgumentType arguments,
            string providerName = "System.Data.SqlClient")
            where TContext : DbContext
        {
            DbConnection connection = null;
            if (arguments == DbContextConstructorArgumentType.Connection
                ||
                arguments == DbContextConstructorArgumentType.ConnectionAndDbCompiledModel)
            {
                if (providerName == "System.Data.SqlClient")
                {
                    connection = SimpleConnection<TContext>();
                }
                else if (providerName == "System.Data.SqlServerCe.4.0")
                {
                    connection = SimpleCeConnection<TContext>();
                }
                else
                {
                    throw new ArgumentException("Invalid provider specified, " + providerName);
                }
            }

            string connectionString = null;
            if (arguments == DbContextConstructorArgumentType.ConnectionString
                ||
                arguments == DbContextConstructorArgumentType.ConnectionStringAndDbCompiledModel)
            {
                if (providerName == "System.Data.SqlClient")
                {
                    connectionString = SimpleConnectionString<TContext>();
                }
                else if (providerName == "System.Data.SqlServerCe.4.0")
                {
                    connectionString = SimpleCeConnectionString<TContext>();
                }
                else
                {
                    throw new ArgumentException("Invalid provider specified, " + providerName);
                }
            }

            var providerInfo
                = (providerName == "System.Data.SqlServerCe.4.0")
                      ? ProviderRegistry.SqlCe4_ProviderInfo
                      : ProviderRegistry.Sql2008_ProviderInfo;

            TContext context = null;
            switch (arguments)
            {
                case DbContextConstructorArgumentType.Parameterless:
                    context = (TContext)Activator.CreateInstance(typeof(TContext));
                    break;
                case DbContextConstructorArgumentType.DbCompiledModel:
                    context =
                        (TContext)
                        Activator.CreateInstance(
                            typeof(TContext),
                            SimpleModelContext.CreateBuilder().Build(providerInfo).Compile());
                    break;
                case DbContextConstructorArgumentType.Connection:
                    context = (TContext)Activator.CreateInstance(typeof(TContext), connection, false);
                    break;
                case DbContextConstructorArgumentType.ConnectionString:
                    context = (TContext)Activator.CreateInstance(typeof(TContext), connectionString);
                    break;
                case DbContextConstructorArgumentType.ConnectionAndDbCompiledModel:
                    context =
                        (TContext)
                        Activator.CreateInstance(
                            typeof(TContext), connection,
                            SimpleModelContext.CreateBuilder().Build(connection).Compile(), false);
                    break;
                case DbContextConstructorArgumentType.ConnectionStringAndDbCompiledModel:
                    context =
                        (TContext)
                        Activator.CreateInstance(
                            typeof(TContext), connectionString,
                            SimpleModelContext.CreateBuilder().Build(providerInfo).Compile());
                    break;
                default:
                    throw new ArgumentException("Invalid DbContext constructor arguments " + arguments);
            }

            return context;
        }

        protected void VerifySetsAreInitialized<TContext>(
            DbCompiledModelContents contents,
            DbProviderInfo providerInfo = null)
            where TContext : SimpleModelContextWithNoData
        {
            providerInfo = providerInfo ?? ProviderRegistry.Sql2008_ProviderInfo;

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

            var model = builder.Build(providerInfo).Compile();

            // Act
            using (var context = (TContext)Activator.CreateInstance(typeof(TContext), model))
            {
                // Verification as appropriate for the various model content options
                switch (contents)
                {
                    case DbCompiledModelContents.IsEmpty:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Products);
                        context.Assert<Category>().IsNotInModel();
                        context.Assert<Product>().IsNotInModel();
                        break;
                    case DbCompiledModelContents.IsSubset:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Products);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel(); // reachability
                        break;
                    case DbCompiledModelContents.IsSuperset:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Products);
                        context.Assert<Category>().IsInModel();
                        context.Assert<Product>().IsInModel();
                        context.Assert<Login>().IsInModel();
                        break;
                    case DbCompiledModelContents.Match:
                        Assert.NotNull(context.Categories);
                        Assert.NotNull(context.Products);
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
    }
}
