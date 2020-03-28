// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Data.Entity.ViewGeneration;

[assembly: DbMappingViewCacheType(typeof(PregenContext), typeof(PregenContextViews))]
[assembly: DbMappingViewCacheType(typeof(PregenContextEdmx), typeof(PregenContextEdmxViews))]
[assembly: DbMappingViewCacheType(typeof(PregenObjectContext), typeof(PregenObjectContextViews))]

[assembly: DbMappingViewCacheType(typeof(ContextWithHashMissmatch), typeof(ViewCacheWithHashMissmatch))]
[assembly: DbMappingViewCacheType(typeof(ContextWithInvalidView), typeof(ViewCacheWithInvalidView))]
[assembly: DbMappingViewCacheType(typeof(ContextWithNullView), typeof(ViewCacheWithNullView))]

namespace System.Data.Entity.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;    
    using System.Data.SqlClient;
    using System.Xml.Linq;
    using Xunit;

    public class PregeneratedViewsTests : FunctionalTestBase
    {
        [Fact]
        public void Pregenerated_views_are_found_for_Code_First_model()
        {
            using (var context = new PregenContext())
            {
                // Trigger view loading
                var _ = context.Blogs.ToString(); 

                Assert.True(PregenContextViews.View0Accessed);
                Assert.True(PregenContextViews.View1Accessed);
            }
        }

        [Fact]
        public void Pregenerated_views_are_found_for_EDMX_model()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(PregenContextEdmx.Csdl).CreateReader() });
            var storeItemCollection = new StoreItemCollection(new[] { XDocument.Parse(PregenContextEdmx.Ssdl).CreateReader() });

            IList<EdmSchemaError> errors;
            var storageMappingItemCollection = StorageMappingItemCollection.Create(
                edmItemCollection,
                storeItemCollection,
                new[] { XDocument.Parse(PregenContextEdmx.Msl).CreateReader() },
                null,
                out errors);

            var workspace = new MetadataWorkspace(
                () => edmItemCollection,
                () => storeItemCollection,
                () => storageMappingItemCollection);

            using (var context = new PregenContextEdmx(workspace))
            {
                // Trigger view loading
                var _ = context.Blogs.ToString(); 

                Assert.True(PregenContextEdmxViews.View0Accessed);
                Assert.True(PregenContextEdmxViews.View1Accessed);
            }
        }

        [Fact]
        public void Pregenerated_views_are_found_for_object_context()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<PregenBlog>();
            var compiledModel = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2008")).Compile();
            var connection = new SqlConnection();

            using (var context = compiledModel.CreateObjectContext<PregenObjectContext>(connection))
            {
                // Trigger view loading
                var _ = context.Blogs.ToTraceString(); 

                Assert.True(PregenObjectContextViews.View0Accessed);
                Assert.True(PregenObjectContextViews.View1Accessed);
            }
        }

        [Fact]
        public void Exception_is_thrown_if_pregenerated_view_cache_mapping_hash_does_not_match()
        {
            using (var context = new ContextWithHashMissmatch())
            {
                var exception =
                    Assert.Throws<EntityCommandCompilationException>(
                        () => context.Blogs.ToString());

                Assert.NotNull(exception.InnerException);
                exception.InnerException.ValidateMessage("ViewGen_HashOnMappingClosure_Not_Matching", "ViewCacheWithHashMissmatch");
            }
        }

        [Fact]
        public void Exception_is_thrown_if_pregenerated_view_cache_returns_view_with_invalid_esql()
        {
            using (var context = new ContextWithInvalidView())
            {
                var exception =
                    Assert.Throws<EntityCommandCompilationException>(
                        () => context.Blogs.ToString());

                Assert.NotNull(exception.InnerException);
                exception.InnerException.ValidateMessage("CouldNotResolveIdentifier", false, "Invalid");
            }
        }

        [Fact]
        public void Exception_is_thrown_if_pregenerated_view_cache_returns_null_view_for_EntitySetBase_that_requires_esql()
        {
            using (var context = new ContextWithNullView())
            {
                var exception =
                    Assert.Throws<EntityCommandCompilationException>(
                        () => context.Blogs.ToString());

                Assert.NotNull(exception.InnerException);
                exception.InnerException.ValidateMessage("Mapping_Views_For_Extent_Not_Generated", "EntitySet", "Blogs");
            }
        }
    }

    public class PregenBlog
    {
        public int Id { get; set; }
    }

    public class PregenContext : DbContext
    {
        static PregenContext()
        {
            Database.SetInitializer<PregenContext>(null);
        }

        public DbSet<PregenBlog> Blogs { get; set; }
    }

    public class PregenBlogEdmx
    {
        public int Id { get; set; }
    }

    public class PregenContextEdmx : DbContext
    {
        static PregenContextEdmx()
        {
            Database.SetInitializer<PregenContextEdmx>(null);
        }

        public PregenContextEdmx(MetadataWorkspace workspace)
            : base(
                new EntityConnection(workspace, new SqlConnection(), entityConnectionOwnsStoreConnection: true),
                contextOwnsConnection: true)
        {
        }

        public DbSet<PregenBlogEdmx> Blogs { get; set; }

        public const string Csdl =
            @"<Schema Namespace='System.Data.Entity.ViewGeneration' Alias='Self' p4:UseStrongSpatialTypes='false' xmlns:p4='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>
                <EntityType Name='PregenBlogEdmx'>
                  <Key>
                    <PropertyRef Name='Id' />
                  </Key>
                  <Property Name='Id' Type='Int32' Nullable='false' p4:StoreGeneratedPattern='Identity' />
                </EntityType>
                <EntityContainer Name='PregenContextEdmx'>
                  <EntitySet Name='Blogs' EntityType='Self.PregenBlogEdmx' />
                </EntityContainer>
              </Schema>";

        public const string Msl =
            @"<Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2009/11/mapping/cs'>
                <EntityContainerMapping StorageEntityContainer='CodeFirstDatabase' CdmEntityContainer='PregenContextEdmx'>
                  <EntitySetMapping Name='Blogs'>
                    <EntityTypeMapping TypeName='System.Data.Entity.ViewGeneration.PregenBlogEdmx'>
                      <MappingFragment StoreEntitySet='PregenBlogEdmx'>
                        <ScalarProperty Name='Id' ColumnName='Id' />
                      </MappingFragment>
                    </EntityTypeMapping>
                  </EntitySetMapping>
                </EntityContainerMapping>
              </Mapping>";

        public const string Ssdl =
            @"<Schema Namespace='CodeFirstDatabaseSchema' Provider='System.Data.SqlClient' ProviderManifestToken='2008' Alias='Self' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>
                <EntityType Name='PregenBlogEdmx'>
                  <Key>
                    <PropertyRef Name='Id' />
                  </Key>
                  <Property Name='Id' Type='int' StoreGeneratedPattern='Identity' Nullable='false' />
                </EntityType>
                <EntityContainer Name='CodeFirstDatabase'>
                  <EntitySet Name='PregenBlogEdmx' EntityType='Self.PregenBlogEdmx' Schema='dbo' Table='PregenBlogEdmxes' />
                </EntityContainer>
              </Schema>";
    }

    public class PregenObjectContext : ObjectContext
    {
        public PregenObjectContext(EntityConnection connection)
            : base(connection)
        {
        }

        public ObjectSet<PregenBlog> Blogs
        {
            get
            {
                return CreateObjectSet<PregenBlog>();
            }
        }
    }

    public class ContextWithHashMissmatch : DbContext
    {
        static ContextWithHashMissmatch()
        {
            Database.SetInitializer<ContextWithHashMissmatch>(null);
        }

        public DbSet<PregenBlog> Blogs { get; set; }
    }

    public class ViewCacheWithHashMissmatch : DbMappingViewCache
    {
        public override string MappingHashValue
        {
            get { return "Missmatch"; }
        }

        public override DbMappingView GetView(EntitySetBase extent)
        {
            throw new NotImplementedException();
        }
    }

    public class ContextWithInvalidView : DbContext
    {
        static ContextWithInvalidView()
        {
            Database.SetInitializer<ContextWithInvalidView>(null);
        }

        public DbSet<PregenBlog> Blogs { get; set; }
    }

    public class ViewCacheWithInvalidView : DbMappingViewCache
    {
        public override string MappingHashValue
        {
            get { return "15d7c7e9868caaf4966b8ef383979b6dbbeccffe1a12e3c1427de366f557cbe1"; }
        }

        public override DbMappingView GetView(EntitySetBase extent)
        {
            return new DbMappingView("Invalid");
        }
    }

    public class ContextWithNullView : DbContext
    {
        static ContextWithNullView()
        {
            Database.SetInitializer<ContextWithNullView>(null);
        }

        public DbSet<PregenBlog> Blogs { get; set; }
    }

    public class ViewCacheWithNullView : DbMappingViewCache
    {
        public override string MappingHashValue
        {
            get { return "073bf2d4f3ff4b869adf2adf400785826ba08e02a178374c013f35992856df6a"; }
        }

        public override DbMappingView GetView(EntitySetBase extent)
        {
            if (extent.Name == "PregenBlog")
            {
                return new DbMappingView(@"
SELECT VALUE -- Constructing PregenBlog
    [CodeFirstDatabaseSchema.PregenBlog](T1.PregenBlog_Id)
FROM (
    SELECT 
        T.Id AS PregenBlog_Id, 
        True AS _from0
    FROM ContextWithNullView.Blogs AS T
) AS T1");
            }

            return null;
        }
    }
}
