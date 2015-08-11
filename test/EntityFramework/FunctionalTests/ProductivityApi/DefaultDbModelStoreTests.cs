// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Xunit;

    /// <summary>
    /// Functional tests for DefaultDbModelStore methods.
    /// </summary>
    public class DefaultDbModelStoreTests : FunctionalTestBase
    {
        private readonly string _location;
        private readonly DefaultDbModelStore _store;

        public DefaultDbModelStoreTests()
        {
            _location = Path.GetTempPath();

            _store = new DefaultDbModelStore(_location);
        }

        [Fact]
        public void TryLoad_returns_null_when_file_does_not_exist()
        {
            Assert.False(File.Exists(_location + typeof(DbContext).FullName + ".edmx"));

            Assert.Null(_store.TryLoad(typeof(DbContext)));
        }

        [Fact]
        public void TryGetEdmx_returns_null_when_file_does_not_exist()
        {
            Assert.False(File.Exists(_location + typeof(DbContext).FullName + ".edmx"));

            Assert.Null(_store.TryGetEdmx(typeof(DbContext)));
        }

        #region Edmx file invalidation
        [Fact]
        public void TryLoad_deletes_edmx_if_context_lastWriteTimeUtc_is_later_edmx_lastwrite()
        {
            var edmxFilePath = WriteMockContextEdmx();

            var assemblyCreationTime = File.GetLastWriteTimeUtc(typeof(MockContext).Assembly.Location);

            //set assembly lastWriteTime later than assembly creation time
            File.SetLastWriteTimeUtc(edmxFilePath, assemblyCreationTime.AddMilliseconds(1));

            var loadedFile = _store.TryLoad(typeof(MockContext));
            Assert.NotNull(loadedFile);

            Assert.True(File.Exists(edmxFilePath), "edmx should remain");

            //set assembly lastWriteTime earlier than assembly creation time
            File.SetLastWriteTimeUtc(edmxFilePath, assemblyCreationTime.AddMilliseconds(-1));

            loadedFile = _store.TryLoad(typeof(MockContext));
            Assert.Null(loadedFile);

            Assert.False(File.Exists(edmxFilePath), "edmx should have been deleted");
        }

        [Fact]
        public void TryGetEdmx_deletes_edmx_if_context_lastWriteTimeUtc_later_edmx_lastwrite()
        {
            MutableResolver.ClearResolvers();

            var edmxFilePath = WriteMockContextEdmx();

            var assemblyCreationTime = File.GetLastWriteTimeUtc(typeof(MockContext).Assembly.Location);

            //set assembly lastWriteTime later than assembly creation time
            File.SetLastWriteTimeUtc(edmxFilePath, assemblyCreationTime.AddMilliseconds(1));

            var compiledModelFromCache = _store.TryGetEdmx(typeof(MockContext));
            Assert.NotNull(compiledModelFromCache);

            Assert.True(File.Exists(edmxFilePath), "edmx should remain");

            //set assembly lastWriteTime earlier than assembly creation time
            File.SetLastWriteTimeUtc(edmxFilePath, assemblyCreationTime.AddMilliseconds(-1));

            compiledModelFromCache = _store.TryGetEdmx(typeof(MockContext));
            Assert.Null(compiledModelFromCache);

            Assert.False(File.Exists(edmxFilePath), "edmx should have been deleted");
        }

        private string WriteMockContextEdmx()
        {
            var edmxFilePath = _location + typeof(MockContext).FullName + ".edmx";
            using (var context = new MockContext())
            {
                context.Database.Initialize(false);

                using (var writer = XmlWriter.Create(edmxFilePath))
                {
                    EdmxWriter.WriteEdmx(new MockContext(), writer);
                }
            }
            return edmxFilePath;
        }

        public class MockContext : DbContext
        {
        }
        #endregion

        [Fact]
        public void DefaultDbModelStore_saves_and_loads_DbContext_with_DropCreateAlwaysInitializer()
        {
            try
            {
                var dependencyResolver = new SingletonDependencyResolver<DbModelStore>(_store);
                MutableResolver.AddResolver<DbModelStore>(dependencyResolver);

                Assert.False(File.Exists(_location + typeof(ModelContext).FullName + ".edmx"), "edmx should not exist yet");

                using (var context = new ModelContext())
                {
                    context.Database.Initialize(true);
                }

                Assert.True(File.Exists(_location + typeof(ModelContext).FullName + ".edmx"), "edmx should have been written to _location");

                var xdocFromStore = _store.TryGetEdmx(typeof(ModelContext));
                Assert.NotNull(xdocFromStore);

                var compiledModelFromCache = _store.TryLoad(typeof(ModelContext));
                Assert.NotNull(compiledModelFromCache);

                using (var context = new ModelContext(compiledModelFromCache))
                {
                    Assert.False(context.Models.Any(prd => true), "should access without error");
                }
            }
            finally //clean up
            {
                MutableResolver.ClearResolvers();
                if (File.Exists(_location + typeof(ModelContext).FullName + ".edmx"))
                {
                    File.Delete(_location + typeof(ModelContext).FullName + ".edmx");
                }
            }
        }

        [Fact]
        public void DefaultDbModelStore_saves_and_loads_DbContext_with_DbFunction_StoreModelConvention()
        {
            try
            {
                var dependencyResolver = new SingletonDependencyResolver<DbModelStore>(_store);
                MutableResolver.AddResolver<DbModelStore>(dependencyResolver);

                Assert.False(File.Exists(_location + typeof(ScalarFunctionDbContext).FullName + ".edmx"), "edmx should not exist yet");

                using (var context = new ScalarFunctionDbContext())
                {
                    context.Models.Add(new Model { Id = 1 });
                    context.SaveChanges();

                    Assert.True(
                        context.Set<Model>().Any(model => ScalarFunction.GetSomething("inValue") == "inValue"),
                        "Value passed in should be returned from db function without error");
                }

                Assert.True(
                    File.Exists(_location + typeof(ScalarFunctionDbContext).FullName + ".edmx"), "edmx should be written to _location");

                var xdocFromStore = _store.TryGetEdmx(typeof(ScalarFunctionDbContext));
                Assert.NotNull(xdocFromStore);

                var compiledModelFromCache = _store.TryLoad(typeof(ScalarFunctionDbContext));
                Assert.NotNull(compiledModelFromCache);
                using (var context = new ScalarFunctionDbContext(compiledModelFromCache))
                {
                    Assert.True(
                        context.Set<Model>().Any(prd => ScalarFunction.GetSomething("inValue") == "inValue"),
                        "Value passed in should be returned from db function without error");
                }
            }
            finally //clean up
            {
                MutableResolver.ClearResolvers();
                if (File.Exists(_location + typeof(ScalarFunctionDbContext).FullName + ".edmx"))
                {
                    File.Delete(_location + typeof(ScalarFunctionDbContext).FullName + ".edmx");
                }
            }
        }

        #region ScalarFunctionDbContext
        public class ScalarFunctionDbContextInitializer : CreateDatabaseIfNotExists<ScalarFunctionDbContext>
        {
            protected override void Seed(ScalarFunctionDbContext context)
            {
                context.Database.ExecuteSqlCommand(CreateFunctionScript);
            }

            private const string CreateFunctionScript =
                @"CREATE FUNCTION [dbo].[GetSomething]
                    (
	                    @inValue varchar(100)
                    )
                    RETURNS varchar(100)
                    AS BEGIN
                        RETURN @inValue;
                    END";
        }
        public class ScalarFunctionDbContext : DbContext
        {
            public ScalarFunctionDbContext()
            {
                Database.SetInitializer(new ScalarFunctionDbContextInitializer());
                Configuration.AutoDetectChangesEnabled = false;
                Configuration.LazyLoadingEnabled = false;
                Configuration.ProxyCreationEnabled = false;
            }

            public ScalarFunctionDbContext(DbCompiledModel model)
                : base(model)
            {
                Database.SetInitializer(new ScalarFunctionDbContextInitializer());
                Configuration.AutoDetectChangesEnabled = false;
                Configuration.LazyLoadingEnabled = false;
                Configuration.ProxyCreationEnabled = false;
            }

            public DbSet<Model> Models { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Conventions.Add(new ScalarFunctionConvention());
                base.OnModelCreating(modelBuilder);
            }
        }

        public static class ScalarFunction
        {
            [DbFunction("CodeFirstDatabaseSchema", "GetSomething")]
            public static string GetSomething(string inValue)
            {
                throw new NotSupportedException();
            }
        }

        public class ScalarFunctionConvention : IStoreModelConvention<EdmModel>
        {
            private DbModel _model;

            public void Apply(EdmModel item, DbModel model)
            {
                _model = model;

                var attribute = typeof(ScalarFunction)
                    .GetMethod("GetSomething")
                    .GetCustomAttributes(false).OfType<DbFunctionAttribute>().FirstOrDefault();
                
                MapScalarFunction(attribute);
            }

            private void MapScalarFunction(DbFunctionAttribute dbfuncAttr)
            {
                var stringType = _model
                    .ProviderManifest
                    .GetStoreType(
                        TypeUsage.CreateDefaultTypeUsage(
                            PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))).EdmType;
                
                var edmParams = new List<FunctionParameter>
                {
                    FunctionParameter.Create("inValue", stringType, ParameterMode.In)
                };

                var edmReturnParams = new List<FunctionParameter>
                {
                    FunctionParameter.Create("result", stringType, ParameterMode.ReturnValue)
                };

                var functionPayload = new EdmFunctionPayload
                {
                    StoreFunctionName = dbfuncAttr.FunctionName,
                    Parameters = edmParams,
                    ReturnParameters = edmReturnParams,
                    Schema = "dbo"
                };

                var function = EdmFunction.Create(dbfuncAttr.FunctionName, dbfuncAttr.NamespaceName,
                    _model.StoreModel.DataSpace, functionPayload, null);

                _model.StoreModel.AddItem(function);
            }
        }

        #endregion

        #region ModelContext
        public class Model
        {
            public int Id { get; set; }
        }

        public class ModelContext : DbContext
        {
            public ModelContext()
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<ModelContext>());
            }

            public ModelContext(DbCompiledModel model)
                : base(model)
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<ModelContext>());
            }

            public DbSet<Model> Models { get; set; }
        }
        #endregion
    }
}
