// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using Moq;
    using ProductivityApiUnitTests;

    /// <summary>
    /// A derived InternalCollectionEntry implementation that exposes a parameterless constructor for mocking.
    /// </summary>
    internal abstract class InternalCollectionEntryForMock : InternalCollectionEntry
    {
        private static readonly NavigationEntryMetadata _fakeCollectionMetadata = new NavigationEntryMetadata(typeof(PropertyApiTests.FakeWithProps), typeof(FakeEntity), "Collection", isCollection: true);
        
        protected InternalCollectionEntryForMock()
            : base(new Mock<PropertyApiTests.InternalEntityEntryForMock<FakeEntity>>().Object, _fakeCollectionMetadata)
        {
        }
    }
}