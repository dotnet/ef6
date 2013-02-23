// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Moq;
    using Xunit;

    public class CodeFirstOSpaceLoaderTests : TestBase
    {
        [Fact]
        public void LoadTypes_processes_entity_types()
        {
            LoadTypes_processes_given_type(SetupEdmType(new EntityType("E", "N", DataSpace.CSpace), typeof(Random)));
        }

        [Fact]
        public void LoadTypes_processes_complex_types()
        {
            LoadTypes_processes_given_type(SetupEdmType(new ComplexType(typeof(Random).Name), typeof(Random)));
        }

        [Fact]
        public void LoadTypes_processes_enum_types()
        {
            LoadTypes_processes_given_type(
                SetupEdmType(
                    new EnumType(
                        typeof(Random).Name,
                        typeof(Random).Namespace,
                        PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32),
                        false,
                        DataSpace.CSpace), typeof(Random)));
        }

        private void LoadTypes_processes_given_type(EdmType edmType)
        {
            var edmItemCollection = new EdmItemCollection();
            edmItemCollection.AddInternal(edmType);
            var mockFactory = CreateMockFactory();

            new CodeFirstOSpaceLoader(mockFactory.Object).LoadTypes(edmItemCollection, new ObjectItemCollection());

            mockFactory.Verify(m => m.TryCreateType(typeof(Random), edmType), Times.Once());
        }

        [Fact]
        public void LoadTypes_calls_CreateRelationships_on_the_given_c_space_collection()
        {
            var edmItemCollection = new EdmItemCollection();
            var mockFactory = CreateMockFactory();

            new CodeFirstOSpaceLoader(mockFactory.Object).LoadTypes(edmItemCollection, new ObjectItemCollection());

            mockFactory.Verify(m => m.CreateRelationships(edmItemCollection), Times.Once());
        }

        [Fact]
        public void LoadTypes_resolves_all_references_in_the_ReferenceResolutions_collection()
        {
            var resolved = false;
            var mockFactory = CreateMockFactory();
            mockFactory.Object.ReferenceResolutions.Add(() => { resolved = true; });

            new CodeFirstOSpaceLoader(mockFactory.Object).LoadTypes(new EdmItemCollection(), new ObjectItemCollection());

            Assert.True(resolved);
        }

        [Fact]
        public void LoadTypes_sets_all_loaded_types_to_read_only()
        {
            var mockFactory = CreateMockFactory();
            var clrEntityType = new ClrEntityType(typeof(Random), "Cheese", "Pickle");
            mockFactory.Object.LoadedTypes.Add("Pickle", clrEntityType);

            new CodeFirstOSpaceLoader(mockFactory.Object).LoadTypes(new EdmItemCollection(), new ObjectItemCollection());

            Assert.True(clrEntityType.IsReadOnly);
        }

        [Fact]
        public void LoadTypes_adds_all_loaded_types_to_the_o_space_collection()
        {
            var mockFactory = CreateMockFactory();
            var mockObjectItemCollection = new Mock<ObjectItemCollection>();
            new CodeFirstOSpaceLoader(mockFactory.Object).LoadTypes(new EdmItemCollection(), mockObjectItemCollection.Object);

            mockObjectItemCollection.Verify(m => m.AddLoadedTypes(mockFactory.Object.LoadedTypes), Times.Once());
        }

        [Fact]
        public void LoadTypes_marks_the_o_space_collection_as_loaded()
        {
            var objectItemCollection = new ObjectItemCollection();
            new CodeFirstOSpaceLoader(CreateMockFactory().Object).LoadTypes(new EdmItemCollection(), objectItemCollection);

            Assert.True(objectItemCollection.OSpaceTypesLoaded);
        }

        private static T SetupEdmType<T>(T edmType, Type clrType) where T : GlobalItem
        {
            edmType.DataSpace = DataSpace.CSpace;
            edmType.Annotations.SetClrType(clrType);
            edmType.SetReadOnly();
            return edmType;
        }

        private static Mock<CodeFirstOSpaceTypeFactory> CreateMockFactory()
        {
            var actions = new List<Action>();
            var edmTypes = new Dictionary<string, EdmType>();
            var ctoo = new Dictionary<EdmType, EdmType>();

            var mockFactory = new Mock<CodeFirstOSpaceTypeFactory>();
            mockFactory.Setup(m => m.ReferenceResolutions).Returns(actions);
            mockFactory.Setup(m => m.LoadedTypes).Returns(edmTypes);
            mockFactory.Setup(m => m.CspaceToOspace).Returns(ctoo);

            return mockFactory;
        }
    }
}
