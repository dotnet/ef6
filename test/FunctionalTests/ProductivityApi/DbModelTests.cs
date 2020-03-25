// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CodeFirstTest
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Functional tests for ModelBuilder and DbCompiledModel.
    /// </summary>
    public class DbModelTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public DbModelTests()
        {
            CreateMetadataFilesForSimpleModel();
        }

        #endregion

        #region Positive CreateObjectContext tests

        [Fact]
        public void CreateObjectContext_loads_o_space_metadata()
        {
            var builder = SimpleModelContext.CreateBuilder();

            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                using (var context = builder.Build(connection).Compile().CreateObjectContext<ObjectContext>(connection))
                {
                    var objectItemCollection =
                        (ObjectItemCollection)context.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
                    var ospaceTypes = context.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);
                    Assert.True(ospaceTypes.Any(t => objectItemCollection.GetClrType(t) == typeof(Product)));
                    Assert.True(ospaceTypes.Any(t => objectItemCollection.GetClrType(t) == typeof(Category)));
                }
            }
        }

        [Fact]
        public void CreateObjectContext_uses_cached_MetadataWorkspace()
        {
            var builder = new DbModelBuilder();
            var model = builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();

            MetadataWorkspace workspace1;
            MetadataWorkspace workspace2;

            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                using (var context = model.CreateObjectContext<ObjectContext>(connection))
                {
                    workspace1 = context.MetadataWorkspace;
                }
            }

            using (var connection = SimpleConnection<SimpleModelContext>())
            {
                using (var context = model.CreateObjectContext<ObjectContext>(connection))
                {
                    workspace2 = context.MetadataWorkspace;
                }
            }

            Assert.NotNull(workspace1);
            Assert.NotNull(workspace2);
            Assert.Same(workspace1, workspace2);
        }

        #endregion

        #region Positive empty model tests

        [Fact]
        public void CreateModel_on_new_DbModelBuilder_creates_empty_model()
        {
            using (var connection = SimpleConnection<EmptyContext>())
            {
                using (
                    var context =
                        new DbModelBuilder().Build(connection).Compile().CreateObjectContext<ObjectContext>(connection))
                {
                    AssertEmptyModel(context);
                }
            }
        }

        private void AssertEmptyModel(ObjectContext context)
        {
            Assert.Equal(0, GetItemCollection(context, DataSpace.OSpace).Count());
            Assert.Equal(1, GetItemCollection(context, DataSpace.CSpace).Count());
        }

        private static IEnumerable<GlobalItem> GetItemCollection(ObjectContext context, DataSpace dataSpace)
        {
            return
                context.MetadataWorkspace.GetItemCollection(dataSpace).Where(
                    t => t.BuiltInTypeKind != BuiltInTypeKind.PrimitiveType &&
                         t.BuiltInTypeKind != BuiltInTypeKind.EdmFunction);
        }

        #endregion

        #region IncludeMetadataInModel tests

        public class DefaultIncludeMetadataInModelContext : DbContext
        {
            public DefaultIncludeMetadataInModelContext()
            {
                Database.SetInitializer<DefaultIncludeMetadataInModelContext>(null);
            }

            public DefaultIncludeMetadataInModelContext(DbCompiledModel model)
                : base(model)
            {
                Database.SetInitializer<DefaultIncludeMetadataInModelContext>(null);
            }
        }

        [Fact]
        public void IncludeMetadataConvention_is_not_used_by_default()
        {
            using (var context = new DefaultIncludeMetadataInModelContext())
            {
#pragma warning disable 612,618
                context.Assert<EdmMetadata>().IsNotInModel();
#pragma warning restore 612,618
            }
        }

        public class IncludeMetadataInModelContext : DbContext
        {
            public IncludeMetadataInModelContext()
            {
                Database.SetInitializer<IncludeMetadataInModelContext>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
#pragma warning disable 612,618
                modelBuilder.Entity<EdmMetadata>().ToTable("EdmMetadata");
#pragma warning restore 612,618
            }
        }

        [Fact]
        public void IncludeMetadataInModel_can_be_switched_on_in_OnModelCreating()
        {
            using (var context = new IncludeMetadataInModelContext())
            {
#pragma warning disable 612,618
                context.Assert<EdmMetadata>().IsInModel();
#pragma warning restore 612,618
            }
        }

        [Fact]
        public void IncludeMetadataInModel_on_standalone_ModelBuilder_is_off_by_default()
        {
            var builder = new DbModelBuilder();

            using (
                var context =
                    new DefaultIncludeMetadataInModelContext(
                        builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile()))
            {
#pragma warning disable 612,618
                context.Assert<EdmMetadata>().IsNotInModel();
#pragma warning restore 612,618
            }
        }

        [Fact]
        public void IncludeMetadataInModel_on_standalone_DbModelBuilder_can_be_turned_on()
        {
            var builder = new DbModelBuilder();

#pragma warning disable 612,618
            builder.Entity<EdmMetadata>().ToTable("EdmMetadata");
#pragma warning restore 612,618

            using (
                var context =
                    new DefaultIncludeMetadataInModelContext(
                        builder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile()))
            {
#pragma warning disable 612,618
                context.Assert<EdmMetadata>().IsInModel();
#pragma warning restore 612,618
            }
        }

        #endregion

        #region Tests for reading the model hash from a Code First context

        [Fact]
        public void Model_hash_can_be_obtained_before_context_is_initialized()
        {
            Model_hash_can_be_obtained_from_Code_First_context(initializeContext: false);
        }

        [Fact]
        public void Model_hash_can_be_obtained_after_context_is_initialized()
        {
            Model_hash_can_be_obtained_from_Code_First_context(initializeContext: true);
        }

        private void Model_hash_can_be_obtained_from_Code_First_context(bool initializeContext)
        {
            using (var context = new SimpleModelContext())
            {
                if (initializeContext)
                {
                    context.Database.Initialize(force: false);
                }

#pragma warning disable 612,618
                var hash = EdmMetadata.TryGetModelHash(context);
#pragma warning restore 612,618

                Assert.NotNull(hash);
            }
        }

        [Fact]
        public void Model_hash_can_be_obtained_when_using_existing_model_before_context_is_initialized()
        {
            Model_hash_can_be_obtained_from_Code_First_context_when_using_existing_model(initializeContext: false);
        }

        [Fact]
        public void Model_hash_can_be_obtained_when_using_existing_model_after_context_is_initialized()
        {
            Model_hash_can_be_obtained_from_Code_First_context_when_using_existing_model(initializeContext: true);
        }

        private void Model_hash_can_be_obtained_from_Code_First_context_when_using_existing_model(bool initializeContext)
        {
            var model = SimpleModelContext.CreateBuilder().Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();
            using (var context = new SimpleModelContext(model))
            {
                if (initializeContext)
                {
                    context.Database.Initialize(force: false);
                }

#pragma warning disable 612,618
                var hash = EdmMetadata.TryGetModelHash(context);
#pragma warning restore 612,618

                Assert.NotNull(hash);
            }
        }

        public class NoMetadataInModelContext : DbContext
        {
            public NoMetadataInModelContext()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<NoMetadataInModelContext>());
            }
        }

        [Fact]
        public void Model_hash_can_be_obtained_from_Code_First_context_even_when_EdmMetadata_is_not_mapped()
        {
            using (var context = new DefaultIncludeMetadataInModelContext())
            {
#pragma warning disable 612,618
                Assert.False(String.IsNullOrWhiteSpace(EdmMetadata.TryGetModelHash(context)));
#pragma warning restore 612,618
            }
        }

        #endregion

        #region Negative tests for cases where EDMX writing is not supported.

        [Fact]
        public void TryGetModelHash_returns_null_when_used_with_DbContext_created_from_existing_ObjectContext()
        {
            using (var outerContext = new SimpleModelContext())
            {
                using (var context = new SimpleModelContext(GetObjectContext(outerContext)))
                {
#pragma warning disable 612,618
                    Assert.Null(EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618
                }
            }
        }

        [Fact]
        public void TryGetModelHash_returns_null_when_used_with_Model_First_DbContext()
        {
            using (var context = new SimpleModelContext(new EntityConnection(SimpleModelEntityConnectionString)))
            {
#pragma warning disable 612,618
                Assert.Null(EdmMetadata.TryGetModelHash(context));
#pragma warning restore 612,618
            }
        }

        #endregion

        #region Model creation with a disposed connection

        public class ContextForDisposedConnectionFailure : DbContext
        {
            public ContextForDisposedConnectionFailure(DbConnection connection)
                : base(connection, contextOwnsConnection: true)
            {
                Database.SetInitializer<ContextForDisposedConnectionFailure>(null);
            }

            public DbSet<Product> Products { get; set; }
        }

        // See bug Dev11 139975
        [Fact]
        public void Model_creation_failure_due_to_a_bad_connection_does_not_preclude_model_creation_with_retry()
        {
            var disposedConnection = SimpleConnection<ContextForDisposedConnectionFailure>();
            disposedConnection.Dispose();

            // Make model initialization fail once
            using (var context = new ContextForDisposedConnectionFailure(disposedConnection))
            {
                try
                {
                    context.Database.Initialize(force: false);
                    Assert.True(false);
                }
                catch (ProviderIncompatibleException)
                {
                }
            }

            // Retry--this time it should work
            using (
                var context =
                    new ContextForDisposedConnectionFailure(SimpleConnection<ContextForDisposedConnectionFailure>()))
            {
                context.Database.Initialize(force: false);
                context.Assert<Product>().IsInModel();
            }
        }

        #endregion
    }
}
