// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Data.Entity.ViewGeneration;

[assembly: DbMappingViewCacheType(typeof(PregenContext), typeof(PregenContextViews))]
[assembly: DbMappingViewCacheType(typeof(PregenContextEdmx), typeof(PregenContextEdmxViews))]
[assembly: DbMappingViewCacheType(typeof(PregenObjectContext), typeof(PregenObjectContextViews))]

namespace System.Data.Entity.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;    
    using System.Data.SqlClient;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class PregeneratedViewsTests : FunctionalTestBase
    {
        [Fact]
        public void Pregenerated_views_are_found_for_Code_First_model()
        {
            using (var context = new PregenContext())
            {
                var _ = context.Blogs.ToString(); // Trigger view loading

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
                var _ = context.Blogs.ToString(); // Trigger view loading

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
                var _ = context.Blogs.ToTraceString(); // Trigger view loading

                Assert.True(PregenObjectContextViews.View0Accessed);
                Assert.True(PregenObjectContextViews.View1Accessed);
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
                return this.CreateObjectSet<PregenBlog>();
            }
        }
    }
}
