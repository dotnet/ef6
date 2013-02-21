// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class CodeFirstOSpaceTypeFactoryTests : TestBase
    {
        [Fact]
        public void ReferenceResolutions_provides_a_simple_list()
        {
            var factory = new CodeFirstOSpaceTypeFactory();
            Assert.NotNull(factory.ReferenceResolutions);
            Assert.Same(factory.ReferenceResolutions, factory.ReferenceResolutions);
        }

        [Fact]
        public void LogLoadMessage_does_nothing()
        {
            new CodeFirstOSpaceTypeFactory().LogLoadMessage("", null);
        }

        [Fact]
        public void LogError_throws_expected_exception()
        {
            Assert.Equal(
                Strings.InvalidSchemaEncountered("Cheese"),
                Assert.Throws<MetadataException>(() => new CodeFirstOSpaceTypeFactory().LogError("Cheese", null)).Message);
        }

        [Fact]
        public void TrackClosure_does_nothing()
        {
            new CodeFirstOSpaceTypeFactory().TrackClosure(null);
        }

        [Fact]
        public void CspaceToOspace_provides_a_simple_dictionary()
        {
            var factory = new CodeFirstOSpaceTypeFactory();
            Assert.NotNull(factory.CspaceToOspace);
            Assert.Same(factory.CspaceToOspace, factory.CspaceToOspace);
        }

        [Fact]
        public void LoadedTypes_provides_a_simple_dictionary()
        {
            var factory = new CodeFirstOSpaceTypeFactory();
            Assert.NotNull(factory.LoadedTypes);
            Assert.Same(factory.LoadedTypes, factory.LoadedTypes);
        }

        [Fact]
        public void AddToTypesInAssembly_does_nothing()
        {
            new CodeFirstOSpaceTypeFactory().AddToTypesInAssembly(null);
        }
    }
}
