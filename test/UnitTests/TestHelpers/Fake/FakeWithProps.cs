// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Internal;
    using Moq;
    using MockHelper = System.Data.Entity.Internal.MockHelper;

    internal class FakeWithProps
    {
        public int ValueTypeProp { get; set; }
        public string RefTypeProp { get; set; }
        public FakeWithProps ComplexProp { get; set; }
        public FakeEntity Reference { get; set; }
        public ICollection<FakeEntity> Collection { get; set; }

        internal static readonly PropertyEntryMetadata ValueTypePropertyMetadata = new PropertyEntryMetadata(
            typeof(FakeWithProps), typeof(int), "ValueTypeProp", isMapped: true, isComplex: false);

        internal static readonly PropertyEntryMetadata RefTypePropertyMetadata = new PropertyEntryMetadata(
            typeof(FakeWithProps), typeof(string), "RefTypeProp", isMapped: true, isComplex: false);

        internal static readonly PropertyEntryMetadata ComplexPropertyMetadata = new PropertyEntryMetadata(
            typeof(FakeWithProps), typeof(FakeWithProps), "ComplexProp", isMapped: true, isComplex: true);

        internal static readonly NavigationEntryMetadata ReferenceMetadata = new NavigationEntryMetadata(
            typeof(FakeWithProps), typeof(FakeEntity), "Reference", isCollection: false);

        internal static readonly NavigationEntryMetadata CollectionMetadata = new NavigationEntryMetadata(
            typeof(FakeWithProps), typeof(FakeEntity), "Collection", isCollection: true);

        internal static Mock<InternalEntityEntryForMock<FakeWithProps>> CreateMockInternalEntityEntry(
            InternalPropertyValues currentValues = null,
            InternalPropertyValues originalValues = null)
        {
            currentValues = currentValues ?? CreateSimpleValues(10);
            var entity = (FakeWithProps)currentValues.ToObject();
            var mockInternalEntry = MockHelper.CreateMockInternalEntityEntry(
                entity, new EntityReference<FakeEntity>(), new EntityCollection<FakeEntity>(), isDetached: false);
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", It.IsAny<Type>(), It.IsAny<Type>())).Returns(
                ValueTypePropertyMetadata);
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", It.IsAny<Type>(), It.IsAny<Type>())).Returns(
                RefTypePropertyMetadata);
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("ComplexProp", It.IsAny<Type>(), It.IsAny<Type>())).Returns(
                ComplexPropertyMetadata);
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("Reference", It.IsAny<Type>(), It.IsAny<Type>()));
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("Collection", It.IsAny<Type>(), It.IsAny<Type>()));
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("Missing", It.IsAny<Type>(), It.IsAny<Type>()));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ValueTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("RefTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ComplexProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("Reference")).Returns(ReferenceMetadata);
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("Collection")).Returns(CollectionMetadata);
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ValueTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("RefTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ComplexProp"));
            mockInternalEntry.SetupGet(e => e.CurrentValues).Returns(currentValues);
            mockInternalEntry.SetupGet(e => e.OriginalValues).Returns(originalValues ?? CreateSimpleValues(20));
            mockInternalEntry.CallBase = true;
            return mockInternalEntry;
        }

        internal static TestInternalPropertyValues<FakeWithProps> CreateSimpleValues(int tag)
        {
            var level3Properties = new Dictionary<string, object>
                                       {
                                           { "ValueTypeProp", 3 + tag },
                                           { "RefTypeProp", "3" + tag },
                                       };
            var level3Values = new TestInternalPropertyValues<FakeWithProps>(level3Properties);

            var level2Properties = new Dictionary<string, object>
                                       {
                                           { "ValueTypeProp", 2 + tag },
                                           { "RefTypeProp", "2" + tag },
                                           { "ComplexProp", level3Values },
                                       };
            var level2Values = new TestInternalPropertyValues<FakeWithProps>(level2Properties, new[] { "ComplexProp" });

            var level1Properties = new Dictionary<string, object>
                                       {
                                           { "ValueTypeProp", 1 + tag },
                                           { "RefTypeProp", "1" + tag },
                                           { "ComplexProp", level2Values },
                                       };
            return new TestInternalPropertyValues<FakeWithProps>(level1Properties, new[] { "ComplexProp" });
        }
    }
}
