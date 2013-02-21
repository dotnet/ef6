// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using Moq;
    using Xunit;

    public class ObjectItemCollectionTests
    {
        public class ExplicitLoadFromAssembly : TestBase
        {
            [Fact]
            public void ExplicitLoadFromAssembly_checks_only_given_assembly_for_views()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.ExplicitLoadFromAssembly(
                    typeof(object).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()), null);

                mockCache.Verify(m => m.CheckAssembly(typeof(object).Assembly, false), Times.Once());
            }

            [Fact]
            public void ExplicitLoadFromAssembly_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);

                objectItemCollection.ExplicitLoadFromAssembly(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()), null);

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(typeof(FactAttribute).Assembly, It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _));
            }

            [Fact]
            public void ExplicitLoadFromAssembly_does_not_perform_o_space_lookup_if_o_space_types_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);
                objectItemCollection.OSpaceTypesLoaded = true;

                objectItemCollection.ExplicitLoadFromAssembly(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()), null);

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(It.IsAny<Assembly>(), It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _),
                    Times.Never());
            }
        }

        public class ImplicitLoadAllReferencedAssemblies : TestBase
        {
            [Fact]
            public void ImplicitLoadAllReferencedAssemblies_checks_assembly_and_references_for_views_if_assembly_not_filtered()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.ImplicitLoadAllReferencedAssemblies(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                mockCache.Verify(m => m.CheckAssembly(typeof(FactAttribute).Assembly, true), Times.Once());
            }

            [Fact]
            public void ImplicitLoadAllReferencedAssemblies_does_not_check_assembly_for_views_if_assembly_filtered()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.ImplicitLoadAllReferencedAssemblies(
                    typeof(object).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                mockCache.Verify(m => m.CheckAssembly(It.IsAny<Assembly>(), It.IsAny<bool>()), Times.Never());
            }

            [Fact]
            public void ImplicitLoadAllReferencedAssemblies_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);

                objectItemCollection.ImplicitLoadAllReferencedAssemblies(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(typeof(FactAttribute).Assembly, It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _));
            }

            [Fact]
            public void ImplicitLoadAllReferencedAssemblies_does_not_perform_o_space_lookup_if_o_space_types_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);
                objectItemCollection.OSpaceTypesLoaded = true;

                objectItemCollection.ImplicitLoadAllReferencedAssemblies(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(It.IsAny<Assembly>(), It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _),
                    Times.Never());
            }
        }

        public class ImplicitLoadAssemblyForType : TestBase
        {
            [Fact]
            public void ImplicitLoadAssemblyForType_checks_only_given_assembly_for_views_if_assembly_not_filtered()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.ImplicitLoadAssemblyForType(
                    typeof(FactAttribute), new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                mockCache.Verify(m => m.CheckAssembly(typeof(FactAttribute).Assembly, false), Times.Once());
            }

            [Fact]
            public void ImplicitLoadAssemblyForType_does_not_check_assembly_for_views_if_assembly_filtered()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.ImplicitLoadAssemblyForType(
                    typeof(object), new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                mockCache.Verify(m => m.CheckAssembly(It.IsAny<Assembly>(), It.IsAny<bool>()), Times.Never());
            }

            [Fact]
            public void ImplicitLoadAssemblyForType_checks_assemblies_of_generics_for_views_if_assembly_not_filtered()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.ImplicitLoadAssemblyForType(
                    typeof(Dictionary<FactAttribute, FactAttribute>), new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                mockCache.Verify(m => m.CheckAssembly(typeof(FactAttribute).Assembly, false), Times.Exactly(2));
            }

            [Fact]
            public void ImplicitLoadAssemblyForType_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);

                objectItemCollection.ImplicitLoadAssemblyForType(
                    typeof(Dictionary<FactAttribute, FactAttribute>), new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(typeof(FactAttribute).Assembly, It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _));
            }

            [Fact]
            public void ImplicitLoadAssemblyForType_does_not_perform_o_space_lookup_if_o_space_types_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);
                objectItemCollection.OSpaceTypesLoaded = true;

                objectItemCollection.ImplicitLoadAssemblyForType(
                    typeof(Dictionary<FactAttribute, FactAttribute>), new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(It.IsAny<Assembly>(), It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _),
                    Times.Never());
            }
        }

        public class LoadFromAssembly : TestBase
        {
            [Fact]
            public void LoadFromAssembly_checks_only_given_assembly_for_views()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.LoadFromAssembly(typeof(object).Assembly);

                mockCache.Verify(m => m.CheckAssembly(typeof(object).Assembly, false), Times.Once());
            }

            [Fact]
            public void LoadFromAssembly_two_arg_overload_checks_only_given_assembly_for_views()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.LoadFromAssembly(
                    typeof(object).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                mockCache.Verify(m => m.CheckAssembly(typeof(object).Assembly, false), Times.Once());
            }

            [Fact]
            public void LoadFromAssembly_three_arg_overload_checks_only_given_assembly_for_views()
            {
                var mockCache = new Mock<IViewAssemblyCache>();
                var objectItemCollection = new ObjectItemCollection(mockCache.Object);

                objectItemCollection.LoadFromAssembly(
                    typeof(object).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()), s => { });

                mockCache.Verify(m => m.CheckAssembly(typeof(object).Assembly, false), Times.Once());
            }

            [Fact]
            public void LoadFromAssembly_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);

                objectItemCollection.LoadFromAssembly(typeof(FactAttribute).Assembly);

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(typeof(FactAttribute).Assembly, It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _));
            }

            [Fact]
            public void Two_arg_LoadFromAssembly_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);

                objectItemCollection.LoadFromAssembly(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()));

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(typeof(FactAttribute).Assembly, It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _));
            }

            [Fact]
            public void Three_arg_LoadFromAssembly_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);

                objectItemCollection.LoadFromAssembly(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()), s => { });

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(typeof(FactAttribute).Assembly, It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _));
            }

            [Fact]
            public void LoadFromAssembly_does_not_perform_o_space_lookup_if_o_space_types_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(new Mock<IViewAssemblyCache>().Object, mockKnownAssemblies.Object);
                objectItemCollection.OSpaceTypesLoaded = true;

                objectItemCollection.LoadFromAssembly(typeof(FactAttribute).Assembly);
                objectItemCollection.LoadFromAssembly(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()));
                objectItemCollection.LoadFromAssembly(
                    typeof(FactAttribute).Assembly, new EdmItemCollection(Enumerable.Empty<XmlReader>()), s => { });

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(It.IsAny<Assembly>(), It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _),
                    Times.Never());
            }
        }
    }
}
