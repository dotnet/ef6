// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.TestHelpers;
    using System.IO;
    using System.Linq;
    using Moq;
    using Xunit;

    public class MetadataCacheTests : TestBase
    {
        static MetadataCacheTests()
        {
            DbConfiguration.SetConfiguration(new FunctionalTestsConfiguration());

            for (var i = 0; i < MetadataFiles.Length; i += 2)
            {
                using (var file = File.CreateText(MetadataFiles[i]))
                {
                    file.Write(MetadataFiles[i + 1]);
                }
            }
        }

        public class GetArtifactLoader : MetadataCacheTests
        {
            [Fact]
            public void GetArtifactLoader_creates_loader_for_file_paths_in_SSDL_MSL_CSDL_order()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);

                var loader = (MetadataArtifactLoaderComposite)new MetadataCache().GetArtifactLoader(options.Object);

                var loaders = loader.ToList<MetadataArtifactLoader>();
                var loadedPaths = loader.GetPaths();
                var originalPaths = loader.GetOriginalPaths();

                Assert.Equal(3, loaders.Count);
                Assert.Equal(3, loadedPaths.Count);
                Assert.Equal(3, originalPaths.Count);

                for (var i = 0; i < loaders.Count; i++)
                {
                    Assert.IsType<MetadataArtifactLoaderFile>(loaders[i]);
                    Assert.Equal(Path.GetFullPath(MetadataFiles[i * 2]), loadedPaths[i]);
                    Assert.Equal(Path.GetFullPath(MetadataFiles[i * 2]), originalPaths[i]);
                }
            }

            [Fact]
            public void GetArtifactLoader_returns_cached_loaders_when_provided_same_input()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);
                var cache = new MetadataCache();

                var loaders1 = ((MetadataArtifactLoaderComposite)cache.GetArtifactLoader(options.Object)).ToList<MetadataArtifactLoader>();
                var loaders2 = ((MetadataArtifactLoaderComposite)cache.GetArtifactLoader(options.Object)).ToList<MetadataArtifactLoader>();

                Assert.Equal(3, loaders1.Count);
                Assert.Equal(3, loaders2.Count);

                for (var i = 0; i < loaders1.Count; i++)
                {
                    Assert.Same(loaders1[i], loaders2[i]);
                }
            }

            [Fact]
            public void GetArtifactLoader_returns_empty_composite_loader_when_given_no_paths()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns("");

                var loader = (MetadataArtifactLoaderComposite)new MetadataCache().GetArtifactLoader(options.Object);

                Assert.Equal(0, loader.GetPaths().Count);
                Assert.Equal(0, loader.GetOriginalPaths().Count);
                Assert.Equal(0, loader.Count());
            }

            [Fact]
            public void GetArtifactLoader_creates_loader_for_embedded_resources_in_SSDL_MSL_CSDL_order()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(EmbdeddedMetadataPaths);

                var loader = (MetadataArtifactLoaderComposite)new MetadataCache().GetArtifactLoader(options.Object);

                var loaders = loader.ToList<MetadataArtifactLoader>();
                var loadedPaths = loader.GetPaths();
                var originalPaths = loader.GetOriginalPaths();

                Assert.Equal(3, loaders.Count);
                Assert.Equal(3, loadedPaths.Count);
                Assert.Equal(3, originalPaths.Count);

                for (var i = 0; i < loaders.Count; i++)
                {
                    Assert.IsType<MetadataArtifactLoaderResource>(loaders[i]);
                    Assert.True(loadedPaths[i].StartsWith(@"res://"));
                    Assert.True(loadedPaths[i].Contains(@"/System.Data.Entity.Core.Metadata.Edm.MetadataCacheTests."));
                    Assert.True(originalPaths[i].StartsWith(@"res://"));
                    Assert.True(originalPaths[i].Contains(@"/System.Data.Entity.Core.Metadata.Edm.MetadataCacheTests."));
                }

                Assert.True(loadedPaths[0].EndsWith(".ssdl"));
                Assert.True(originalPaths[0].EndsWith(".ssdl"));
                Assert.True(loadedPaths[1].EndsWith(".msl"));
                Assert.True(originalPaths[1].EndsWith(".msl"));
                Assert.True(loadedPaths[2].EndsWith(".csdl"));
                Assert.True(originalPaths[2].EndsWith(".csdl"));
            }

            [Fact]
            public void GetArtifactLoader_creates_loader_for_directories_containing_SSDL_MSL_CSDL_possibly_not_in_order()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataDirs);

                var loader = (MetadataArtifactLoaderComposite)new MetadataCache().GetArtifactLoader(options.Object);

                var loaders = loader.ToList<MetadataArtifactLoader>();
                var loadedPaths = loader.GetPaths();
                var originalPaths = loader.GetOriginalPaths();

                Assert.Equal(3, loaders.Count);
                Assert.Equal(3, loadedPaths.Count);
                Assert.Equal(3, originalPaths.Count);

                for (var i = 0; i < loaders.Count; i++)
                {
                    Assert.IsType<MetadataArtifactLoaderCompositeFile>(loaders[i]);
                    Assert.Equal(Directory.GetCurrentDirectory() + "\\", originalPaths[i]);
                    Assert.Contains(Path.GetFullPath(MetadataFiles[i * 2]), loadedPaths);
                }
            }

            [Fact]
            public void GetArtifactLoader_does_not_use_cached_loaders_when_metadata_directory_is_used_for_loading()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataDirs);
                var cache = new MetadataCache();

                var loaders1 = ((MetadataArtifactLoaderComposite)cache.GetArtifactLoader(options.Object)).ToList<MetadataArtifactLoader>();
                var loaders2 = ((MetadataArtifactLoaderComposite)cache.GetArtifactLoader(options.Object)).ToList<MetadataArtifactLoader>();

                Assert.Equal(3, loaders1.Count);
                Assert.Equal(3, loaders2.Count);

                for (var i = 0; i < loaders1.Count; i++)
                {
                    Assert.NotSame(loaders1[i], loaders2[i]);
                }
            }
        }

        public class GetMetadataWorkspace : MetadataCacheTests
        {
            [Fact]
            public void GetMetadataWorkspace_uses_given_artifact_loader_to_create_workspace()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);

                var loader = new MetadataCache().GetArtifactLoader(options.Object);
                var workspace = new MetadataCache().GetMetadataWorkspace("Key Lime", loader);

                var edmItemCollection = (EdmItemCollection)workspace.GetItemCollection(DataSpace.CSpace);
                Assert.NotNull(edmItemCollection.GetItem<EntityType>("MetadataCacheTests.BlogEdmx"));

                var storeItemCollection = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);
                Assert.NotNull(storeItemCollection.GetItem<EntityType>("CodeFirstDatabaseSchema.BlogEdmx"));

                Assert.IsType<StorageMappingItemCollection>(workspace.GetItemCollection(DataSpace.CSSpace));
            }

            [Fact]
            public void GetMetadataWorkspace_returns_cached_workspace_based_on_cache_key()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);

                var loader1 = new MetadataCache().GetArtifactLoader(options.Object);
                var loader2 = new MetadataCache().GetArtifactLoader(options.Object);

                var cache = new MetadataCache();

                Assert.Same(
                    cache.GetMetadataWorkspace("Key Lime", loader1),
                    cache.GetMetadataWorkspace("Key Lime", loader2));
            }

            [Fact]
            public void GetMetadataWorkspace_caches_based_on_artifact_paths_and_store_provider()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);
                options.Setup(m => m["provider"]).Returns("Some Provider");

                var cache = new MetadataCache();

                var workspace = cache.GetMetadataWorkspace(options.Object);
                Assert.Same(workspace, cache.GetMetadataWorkspace(options.Object));

                options.Setup(m => m["provider"]).Returns("Another Provider");
                Assert.NotSame(workspace, cache.GetMetadataWorkspace(options.Object));

                options.Setup(m => m["metadata"]).Returns(EmbdeddedMetadataPaths);
                options.Setup(m => m["provider"]).Returns("Some Provider");
                Assert.NotSame(workspace, cache.GetMetadataWorkspace(options.Object));

                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);
                options.Setup(m => m["provider"]).Returns("Some Provider");
                Assert.Same(workspace, cache.GetMetadataWorkspace(options.Object));
            }
        }

        public class Clear : MetadataCacheTests
        {
            [Fact]
            public void Clear_clears_all_cached_workspaces()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);
                options.Setup(m => m["provider"]).Returns("Some Provider");

                var cache = new MetadataCache();

                var workspace = cache.GetMetadataWorkspace(options.Object);

                cache.Clear();

                Assert.NotSame(workspace, cache.GetMetadataWorkspace(options.Object));
            }

            [Fact]
            public void Clear_clears_all_cached_artifact_loaders()
            {
                var options = new Mock<DbConnectionOptions>();
                options.Setup(m => m["metadata"]).Returns(FileMetadataPaths);
                var cache = new MetadataCache();

                var loaders1 = ((MetadataArtifactLoaderComposite)cache.GetArtifactLoader(options.Object)).ToList<MetadataArtifactLoader>();

                cache.Clear();

                var loaders2 = ((MetadataArtifactLoaderComposite)cache.GetArtifactLoader(options.Object)).ToList<MetadataArtifactLoader>();

                Assert.Equal(3, loaders1.Count);
                Assert.Equal(3, loaders2.Count);

                for (var i = 0; i < loaders1.Count; i++)
                {
                    Assert.NotSame(loaders1[i], loaders2[i]);
                }
            }
        }

        private const string FileMetadataDirs = @".\|.\|.\";

        private const string FileMetadataPaths = @".\MetadataCacheTests.csdl|.\MetadataCacheTests.ssdl|.\MetadataCacheTests.msl";

        private const string EmbdeddedMetadataPaths =
            @"res://EntityFramework.UnitTests/System.Data.Entity.Core.Metadata.Edm.MetadataCacheTests.csdl"
            + @"|res://EntityFramework.UnitTests/System.Data.Entity.Core.Metadata.Edm.MetadataCacheTests.ssdl"
            + @"|res://EntityFramework.UnitTests/System.Data.Entity.Core.Metadata.Edm.MetadataCacheTests.msl";

        private static readonly string[] MetadataFiles = new[]
            {
                @".\MetadataCacheTests.ssdl", Ssdl,
                @".\MetadataCacheTests.msl", Msl,
                @".\MetadataCacheTests.csdl", Csdl
            };

        private const string Csdl =
            @"<Schema Namespace='MetadataCacheTests' Alias='Self' p4:UseStrongSpatialTypes='false' xmlns:p4='http://schemas.microsoft.com/ado/2009/02/edm/annotation' xmlns='http://schemas.microsoft.com/ado/2009/11/edm'>
                <EntityType Name='BlogEdmx'>
                  <Key>
                    <PropertyRef Name='Id' />
                  </Key>
                  <Property Name='Id' Type='Int32' Nullable='false' p4:StoreGeneratedPattern='Identity' />
                </EntityType>
                <EntityContainer Name='ContextEdmx'>
                  <EntitySet Name='Blogs' EntityType='Self.BlogEdmx' />
                </EntityContainer>
              </Schema>";

        private const string Msl =
            @"<Mapping Space='C-S' xmlns='http://schemas.microsoft.com/ado/2009/11/mapping/cs'>
                <EntityContainerMapping StorageEntityContainer='CodeFirstDatabase' CdmEntityContainer='ContextEdmx'>
                  <EntitySetMapping Name='Blogs'>
                    <EntityTypeMapping TypeName='MetadataCacheTests.BlogEdmx'>
                      <MappingFragment StoreEntitySet='BlogEdmx'>
                        <ScalarProperty Name='Id' ColumnName='Id' />
                      </MappingFragment>
                    </EntityTypeMapping>
                  </EntitySetMapping>
                </EntityContainerMapping>
              </Mapping>";

        private const string Ssdl =
            @"<Schema Namespace='CodeFirstDatabaseSchema' Provider='System.Data.SqlClient' ProviderManifestToken='2008' Alias='Self' xmlns='http://schemas.microsoft.com/ado/2009/11/edm/ssdl'>
                <EntityType Name='BlogEdmx'>
                  <Key>
                    <PropertyRef Name='Id' />
                  </Key>
                  <Property Name='Id' Type='int' StoreGeneratedPattern='Identity' Nullable='false' />
                </EntityType>
                <EntityContainer Name='CodeFirstDatabase'>
                  <EntitySet Name='BlogEdmx' EntityType='Self.BlogEdmx' Schema='dbo' Table='BlogEdmxes' />
                </EntityContainer>
              </Schema>";
    }
}
