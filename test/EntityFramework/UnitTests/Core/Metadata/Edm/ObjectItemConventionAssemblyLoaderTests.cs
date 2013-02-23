// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using Xunit;

    public class ObjectItemConventionAssemblyLoaderTests
    {
        public class ConventionOSpaceTypeFactory : TestBase
        {
            [Fact]
            public void ReferenceResolutions_delegates_to_the_loader()
            {
                var loader = new ObjectItemConventionAssemblyLoader(typeof(object).Assembly, new ObjectItemLoadingSessionData());
                var factory = new ObjectItemConventionAssemblyLoader.ConventionOSpaceTypeFactory(loader);

                Assert.NotNull(factory.ReferenceResolutions);
                Assert.Same(factory.ReferenceResolutions, factory.ReferenceResolutions);
            }

            [Fact]
            public void LogLoadMessage_delegates_to_the_loader_session()
            {
                var mockLogger = new Mock<LoadMessageLogger>(null);

                var mockSession = new Mock<ObjectItemLoadingSessionData>();
                mockSession.Setup(m => m.LoadMessageLogger).Returns(mockLogger.Object);

                var loader = new ObjectItemConventionAssemblyLoader(typeof(object).Assembly, mockSession.Object);

                var factory = new ObjectItemConventionAssemblyLoader.ConventionOSpaceTypeFactory(loader);

                var entityType = new EntityType("E", "N", DataSpace.CSpace);
                factory.LogLoadMessage("Cheese", entityType);

                mockLogger.Verify(m => m.LogLoadMessage("Cheese", entityType));
            }

            [Fact]
            public void LogError_delegates_to_the_loader_session()
            {
                var mockLogger = new Mock<LoadMessageLogger>(null);
                mockLogger
                    .Setup(m => m.CreateErrorMessageWithTypeSpecificLoadLogs(It.IsAny<string>(), It.IsAny<EdmType>()))
                    .Returns("The Message");

                var mockSession = new Mock<ObjectItemLoadingSessionData>();
                mockSession.Setup(m => m.LoadMessageLogger).Returns(mockLogger.Object);
                var edmItemErrors = new List<EdmItemError>();
                mockSession.Setup(m => m.EdmItemErrors).Returns(edmItemErrors);

                var loader = new ObjectItemConventionAssemblyLoader(typeof(object).Assembly, mockSession.Object);

                var factory = new ObjectItemConventionAssemblyLoader.ConventionOSpaceTypeFactory(loader);

                var entityType = new EntityType("E", "N", DataSpace.CSpace);
                factory.LogError("Cheese", entityType);

                mockLogger.Verify(m => m.CreateErrorMessageWithTypeSpecificLoadLogs("Cheese", entityType));

                Assert.Equal("The Message", edmItemErrors.Select(e => e.Message).Single());
            }

            [Fact]
            public void TrackClosure_delegates_to_the_loader()
            {
                var mockLoader = new Mock<ObjectItemConventionAssemblyLoader>(typeof(object).Assembly, new ObjectItemLoadingSessionData());
                var factory = new ObjectItemConventionAssemblyLoader.ConventionOSpaceTypeFactory(mockLoader.Object);

                factory.TrackClosure(typeof(Random));

                mockLoader.Verify(m => m.TrackClosure(typeof(Random)));
            }

            [Fact]
            public void CspaceToOspace_delegates_to_loader_session()
            {
                var mockSession = new Mock<ObjectItemLoadingSessionData>();
                var dictionary = new Dictionary<EdmType, EdmType>();
                mockSession.Setup(m => m.CspaceToOspace).Returns(dictionary);

                var loader = new ObjectItemConventionAssemblyLoader(typeof(object).Assembly, mockSession.Object);

                var factory = new ObjectItemConventionAssemblyLoader.ConventionOSpaceTypeFactory(loader);

                Assert.Same(dictionary, factory.CspaceToOspace);
            }

            [Fact]
            public void LoadedTypes_delegates_to_loader_session()
            {
                var mockSession = new Mock<ObjectItemLoadingSessionData>();
                var dictionary = new Dictionary<string, EdmType>();
                mockSession.Setup(m => m.TypesInLoading).Returns(dictionary);

                var loader = new ObjectItemConventionAssemblyLoader(typeof(object).Assembly, mockSession.Object);

                var factory = new ObjectItemConventionAssemblyLoader.ConventionOSpaceTypeFactory(loader);

                Assert.Same(dictionary, factory.LoadedTypes);
            }

            [Fact]
            public void AddToTypesInAssembly_delegates_to_the_loader_cache_entry()
            {
                var mockLoader = new Mock<ObjectItemConventionAssemblyLoader>(typeof(object).Assembly, new ObjectItemLoadingSessionData());
                var cacheEntry = new MutableAssemblyCacheEntry();
                mockLoader.Setup(m => m.CacheEntry).Returns(cacheEntry);

                var factory = new ObjectItemConventionAssemblyLoader.ConventionOSpaceTypeFactory(mockLoader.Object);

                var entityType = new EntityType("E", "N", DataSpace.CSpace);
                factory.AddToTypesInAssembly(entityType);

                Assert.Same(entityType, cacheEntry.TypesInAssembly.Single());
            }
        }
    }
}
