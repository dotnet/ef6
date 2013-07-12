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
            public void ExplicitLoadFromAssembly_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);

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
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);
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
            public void ImplicitLoadAllReferencedAssemblies_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);

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
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);
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
            public void ImplicitLoadAssemblyForType_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);

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
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);
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
            public void LoadFromAssembly_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);

                objectItemCollection.LoadFromAssembly(typeof(FactAttribute).Assembly);

                KnownAssemblyEntry _;
                mockKnownAssemblies.Verify(
                    m => m.TryGetKnownAssembly(typeof(FactAttribute).Assembly, It.IsAny<object>(), It.IsAny<EdmItemCollection>(), out _));
            }

            [Fact]
            public void Two_arg_LoadFromAssembly_does_perform_o_space_lookup_if_o_space_types_not_already_loaded()
            {
                var mockKnownAssemblies = new Mock<KnownAssembliesSet>();
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);

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
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);

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
                var objectItemCollection = new ObjectItemCollection(mockKnownAssemblies.Object);
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
