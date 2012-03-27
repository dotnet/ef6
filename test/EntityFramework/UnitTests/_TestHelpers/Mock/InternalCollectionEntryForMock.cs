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
        private static readonly NavigationEntryMetadata FakeCollectionMetadata = new NavigationEntryMetadata(typeof(PropertyApiTests.FakeWithProps), typeof(FakeEntity), "Collection", isCollection: true);
        
        protected InternalCollectionEntryForMock()
            : base(new Mock<PropertyApiTests.InternalEntityEntryForMock<FakeEntity>>().Object, FakeCollectionMetadata)
        {
        }
    }
}