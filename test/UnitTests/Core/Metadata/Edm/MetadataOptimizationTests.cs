// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class MetadataOptimizationTests
    {
        private static int Count = 5;

        [Fact]
        public void GetCSpaceAssociationTypes_assigns_indexes_and_creates_array_once()
        {
            var metadataWorkspace = CreateMetadataWorkspace();

            Assert.NotNull(metadataWorkspace.MetadataOptimization);

            var associationTypes = metadataWorkspace.GetItemCollection(DataSpace.CSpace)
                .GetItems<AssociationType>().ToArray();

            for (var i = 0; i < associationTypes.Length; i++)
            {
                Assert.False(associationTypes[i].Index >= 0);
            }

            var associationTypes1 = metadataWorkspace.MetadataOptimization.GetCSpaceAssociationTypes();

            Assert.Equal(associationTypes.Length, associationTypes1.Length);
            for (var i = 0; i < associationTypes.Length; i++)
            {
                Assert.Equal(i, associationTypes[i].Index);
                Assert.Same(associationTypes[i], associationTypes1[i]);                
            }

            var associationTypes2 = metadataWorkspace.MetadataOptimization.GetCSpaceAssociationTypes();

            Assert.Same(associationTypes1, associationTypes2);
        }

        [Fact]
        public void GetCSpaceAssociationTypeToSetsMap_creates_array_once()
        {
            var metadataWorkspace = CreateMetadataWorkspace();

            Assert.NotNull(metadataWorkspace.MetadataOptimization);

            var associationSets = metadataWorkspace.GetItemCollection(DataSpace.CSpace)
                .GetItems<EntityContainer>().Single().AssociationSets.ToArray();

            var associationSets1 = metadataWorkspace.MetadataOptimization.GetCSpaceAssociationTypeToSetsMap();

            Assert.Equal(associationSets.Length, associationSets1.Length);
            for (var i = 0; i < associationSets.Length; i++)
            {
                Assert.Same(associationSets[i], associationSets1[i]);
            }

            var associationSets2 = metadataWorkspace.MetadataOptimization.GetCSpaceAssociationTypeToSetsMap();

            Assert.Same(associationSets1, associationSets2);
        }

        [Fact]
        public void GetOSpaceAssociationTypes_creates_array_with_nulls_once()
        {
            var metadataWorkspace = CreateMetadataWorkspace();

            Assert.NotNull(metadataWorkspace.MetadataOptimization);

            var csAssociationTypes = metadataWorkspace.GetItemCollection(DataSpace.CSpace)
                .GetItems<AssociationType>().ToArray();

            var osAssociationTypes1 = metadataWorkspace.MetadataOptimization.GetOSpaceAssociationTypes();

            Assert.Equal(csAssociationTypes.Length, osAssociationTypes1.Length);
            for (var i = 0; i < osAssociationTypes1.Length; i++)
            {
                Assert.Null(osAssociationTypes1[i]);
            }

            var osAssociationTypes2 = metadataWorkspace.MetadataOptimization.GetOSpaceAssociationTypes();

            Assert.Same(osAssociationTypes1, osAssociationTypes2);
        }

        [Fact]
        public void GetOSpaceAssociationType_assigns_index_and_sets_array_element_once()
        {
            var metadataWorkspace = CreateMetadataWorkspace();

            Assert.NotNull(metadataWorkspace.MetadataOptimization);

            var index = Count / 2;
            var csAssociationType = metadataWorkspace.GetItemCollection(DataSpace.CSpace)
                .GetItems<AssociationType>()[index];

            Assert.False(csAssociationType.Index >= 0);

            var osAssociationType1 = new AssociationType("AssociationType1", "Namespace", false, DataSpace.OSpace);
            var osAssociationType2 = new AssociationType("AssociationType2", "Namespace", false, DataSpace.OSpace);

            Assert.False(osAssociationType1.Index >= 0);
            Assert.False(osAssociationType2.Index >= 0);

            var osAssociationType = metadataWorkspace.MetadataOptimization.GetOSpaceAssociationType(
                csAssociationType, () => osAssociationType1);

            Assert.True(osAssociationType1.Index >= 0);
            Assert.Equal(csAssociationType.Index, osAssociationType1.Index);
            Assert.Same(osAssociationType1, osAssociationType);

            osAssociationType = metadataWorkspace.MetadataOptimization.GetOSpaceAssociationType(
                csAssociationType, () => osAssociationType2);

            Assert.False(osAssociationType2.Index >= 0);
            Assert.Same(osAssociationType1, osAssociationType);
        }

        [Fact]
        public void GetCSpaceAssociationType_retrieves_element_at_corresponding_index()
        {
            var metadataWorkspace = CreateMetadataWorkspace();

            Assert.NotNull(metadataWorkspace.MetadataOptimization);

            var index = Count / 2;
            var osAssociationType = new AssociationType("AssociationType", "Namespace", false, DataSpace.OSpace);
            osAssociationType.Index = index;

            var csAssociationTypes = metadataWorkspace.MetadataOptimization.GetCSpaceAssociationTypes();
            var csAssociationType = metadataWorkspace.MetadataOptimization.GetCSpaceAssociationType(osAssociationType);

            Assert.Equal(index, csAssociationType.Index);
            Assert.Same(csAssociationType, csAssociationTypes[index]);            
        }

        [Fact]
        public void FindCSpaceAssociationSet_returns_matching_set_SEST()
        {
            var conceptualModel = CreateConceptualModel();

            var index = Count / 2;
            var associationType = conceptualModel.AssociationTypes.ElementAt(index);
            var associationSet = conceptualModel.GetAssociationSet(associationType);
            var sourceSet = associationSet.SourceSet;
            var targetSet = associationSet.TargetSet;

            var metadataWorkspace = new MetadataWorkspace(() => new EdmItemCollection(conceptualModel), () => null, () => null);

            Assert.NotNull(metadataWorkspace.MetadataOptimization);

            var associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "SourceEnd" + index, sourceSet);
            Assert.Same(associationSet, associationSetFound);

            associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "TargetEnd" + index, targetSet);
            Assert.Same(associationSet, associationSetFound);

            EntitySet endEntitySet;
            associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "SourceEnd" + index, sourceSet.Name, sourceSet.EntityContainer.Name, out endEntitySet);
            Assert.Same(associationSet, associationSetFound);
            Assert.Same(sourceSet, endEntitySet);

            associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "TargetEnd" + index, targetSet.Name, targetSet.EntityContainer.Name, out endEntitySet);
            Assert.Same(associationSet, associationSetFound);
            Assert.Same(targetSet, endEntitySet);

            associationSet = metadataWorkspace.MetadataOptimization
                .GetCSpaceAssociationTypeToSetsMap()[index] as AssociationSet;

            Assert.NotNull(associationSet);
            Assert.Same(associationSetFound, associationSet);
        }

        [Fact]
        public void FindCSpaceAssociationSet_returns_matching_set_MEST()
        {
            var conceptualModel = CreateConceptualModel();

            // Add second association set for an association type.

            var index = Count / 2;
            var associationType = conceptualModel.AssociationTypes.ElementAt(index);

            var sourceSet = conceptualModel.AddEntitySet(
                "SourceEntitySetX" + index,
                associationType.SourceEnd.GetEntityType());
            var targetSet = conceptualModel.AddEntitySet(
                "TargetEntitySetX" + index,
                associationType.TargetEnd.GetEntityType());

            var associationSet = AssociationSet.Create(
                "AssociationSetX" + index,
                associationType,
                sourceSet,
                targetSet,
                Enumerable.Empty<MetadataProperty>());
            conceptualModel.AddAssociationSet(associationSet);

            var metadataWorkspace = new MetadataWorkspace(() => new EdmItemCollection(conceptualModel), () => null, () => null);

            Assert.NotNull(metadataWorkspace.MetadataOptimization);

            var associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "SourceEnd" + index, sourceSet);
            Assert.Same(associationSet, associationSetFound);

            associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "TargetEnd" + index, targetSet);
            Assert.Same(associationSet, associationSetFound);

            EntitySet endEntitySet;
            associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "SourceEnd" + index, sourceSet.Name, sourceSet.EntityContainer.Name, out endEntitySet);
            Assert.Same(associationSet, associationSetFound);
            Assert.Same(sourceSet, endEntitySet);

            associationSetFound = metadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet(
                associationType, "TargetEnd" + index, targetSet.Name, targetSet.EntityContainer.Name, out endEntitySet);
            Assert.Same(associationSet, associationSetFound);
            Assert.Same(targetSet, endEntitySet);

            var associationSets = metadataWorkspace.MetadataOptimization
                .GetCSpaceAssociationTypeToSetsMap()[index] as AssociationSet[];
            
            Assert.NotNull(associationSets);
            Assert.Equal(2, associationSets.Length);
            Assert.NotSame(associationSets[0], associationSets[1]);
            Assert.Same(associationSetFound, associationSets[1]);
        }

        private static MetadataWorkspace CreateMetadataWorkspace()
        {
            var conceptualModel = CreateConceptualModel();

            return new MetadataWorkspace(() => new EdmItemCollection(conceptualModel), () => null, () => null);
        }

        private static EdmModel CreateConceptualModel()
        {
            var conceptualModel = EdmModel.CreateConceptualModel(new EntityContainer("Container", DataSpace.CSpace));

            for (var i = 0; i < Count; i++)
            {
                var associationType = CreateCSpaceAssociationType(i);
                conceptualModel.AddAssociationType(associationType);

                var sourceSet = conceptualModel.AddEntitySet(
                    "SourceEntitySet" + i,
                    associationType.SourceEnd.GetEntityType());
                var targetSet = conceptualModel.AddEntitySet(
                    "TargetEntitySet" + i,
                    associationType.TargetEnd.GetEntityType());

                var associationSet = AssociationSet.Create(
                    "AssociationSet" + i,
                    associationType,
                    sourceSet,
                    targetSet,
                    Enumerable.Empty<MetadataProperty>());
                conceptualModel.AddAssociationSet(associationSet);
            }

            return conceptualModel;
        }

        private static AssociationType CreateCSpaceAssociationType(int index)
        {
            var sourceProperty = new EdmProperty("SourceProperty");
            var targetProperty = new EdmProperty("TargetProperty");

            var sourceEntityType = EntityType.Create(
                "SourceEntityType" + index, 
                "Namespace",
                DataSpace.CSpace,
                new [] { "SourceProperty" },
                new[] { sourceProperty }, 
                Enumerable.Empty<MetadataProperty>());
            var targetEntityType = EntityType.Create(
                "TargetEntityType" + index,
                "Namespace",
                DataSpace.CSpace,
                new[] { "TargetProperty" },
                new[] { targetProperty },
                Enumerable.Empty<MetadataProperty>());

            var sourceEnd = new AssociationEndMember("SourceEnd" + index, sourceEntityType);
            var targetEnd = new AssociationEndMember("TargetEnd" + index, targetEntityType);

            var constraint =
                new ReferentialConstraint(
                    sourceEnd,
                    targetEnd,
                    new[] { sourceProperty },
                    new[] { targetProperty });

            var associationType =
                AssociationType.Create(
                    "AssociationType" + index,
                    "Namespace",
                    true,
                    DataSpace.CSpace,
                    sourceEnd,
                    targetEnd,
                    constraint,
                    Enumerable.Empty<MetadataProperty>());

            return associationType;
        }

        [Fact]
        public void GetEntitySetMappingCache_returns_unique_instance_per_metadataworkspace_per_contexttype()
        {
            var metadataWorkspace1 = CreateMetadataWorkspace();
            var metadataWorkspace2 = CreateMetadataWorkspace();

            var entitySetMappings1 = metadataWorkspace1.MetadataOptimization.EntitySetMappingCache;
            var entitySetMappings2 = metadataWorkspace2.MetadataOptimization.EntitySetMappingCache;

            Assert.False(
                ReferenceEquals(entitySetMappings1, entitySetMappings2),
                "Metadata workspace should produce a unique entitysetmapping dictionary per metadata workspace instance");
            Assert.True(
                ReferenceEquals(entitySetMappings1, metadataWorkspace1.MetadataOptimization.EntitySetMappingCache),
                "Metadata workspace should provide the cached instance of the entitysetmapping dictionary for the provided context type");
        }

        public class Blog
        {
            public int BlogId { get; set; }
            public string Title { get; set; }
            public virtual ICollection<Post> Posts { get; set; }
        }

        public class Post
        {
            public int PostId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public Blog Blog { get; set; }
        }

        public class BlogDbContext : DbContext
        {
            public virtual DbSet<Blog> Blogs { get; set; }
            public virtual DbSet<Post> Posts { get; set; }
        }

        public class Picture
        {
            public int PictureId { get; set; }
            public string Name { get; set; }
            public byte[] Data { get; set; }
        }

        public class BlogDbContextVersion2 : DbContext
        {
            public virtual DbSet<Blog> Blogs { get; set; }
            public virtual DbSet<Post> Posts { get; set; }
            public virtual DbSet<Picture> Pictures { get; set; }
        }

        [Fact]
        public void GetEntitySetMappingCache_produces_multiple_entitysets_for_single_clr_type_present_in_multiple_models()
        {
            var blogType = typeof(Blog);
            Internal.EntitySetTypePair blog1PairForBlogType;
            using (var blog1 = new BlogDbContext())
            {
                using (var blog2 = new BlogDbContextVersion2())
                {
                    //obtain the type to entityset dictionaries and update the mappings
                    var workspace1 = ((IObjectContextAdapter)blog1).ObjectContext.MetadataWorkspace.MetadataOptimization;
                    var entitySetMappingCache1 = workspace1.EntitySetMappingCache;
                    workspace1.TryUpdateEntitySetMappingsForType(blogType);

                    var workspace2 = ((IObjectContextAdapter)blog2).ObjectContext.MetadataWorkspace.MetadataOptimization;
                    var entitySetMappingCache2 = workspace2.EntitySetMappingCache;
                    workspace2.TryUpdateEntitySetMappingsForType(blogType);

                    //check that the same clr type maps to different entity sets on different metadata workspaces
                    blog1PairForBlogType = entitySetMappingCache1[blogType];
                    Assert.NotNull(blog1PairForBlogType);
                    Assert.False(
                        ReferenceEquals(blog1PairForBlogType, entitySetMappingCache2[blogType]),
                        "Single CLR type on two different metadata workspaces erroneously maps to the same entity set instance");
                }
            }

            using (var blog1Bis = new BlogDbContext())
            {
                var workspace1Bis = ((IObjectContextAdapter)blog1Bis).ObjectContext.MetadataWorkspace.MetadataOptimization;
                var entitySetMappingCache1Bis = workspace1Bis.EntitySetMappingCache;
                workspace1Bis.TryUpdateEntitySetMappingsForType(blogType);

                Assert.True(
                        blog1PairForBlogType.Equals(entitySetMappingCache1Bis[blogType]),
                        "Metadata workspace should produce same entity set for same CLR type on different context instances");

                Assert.True(
                        blog1PairForBlogType.Equals(entitySetMappingCache1Bis[new Blog().GetType()]),
                        "Metadata workspace should produce the same entity set for the same CLR type using different instances of the same type");
            }
        }
    }
}
