// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class TypeMappingTests
    {
        [Fact]
        public void Can_add_remove_mapping_fragment()
        {
            var storageSetMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            var storageTypeMapping
                = new EntityTypeMapping(storageSetMapping);

            Assert.Empty(storageTypeMapping.MappingFragments);

            var mappingFragment = new MappingFragment(new EntitySet(), storageTypeMapping, false);

            storageTypeMapping.AddFragment(mappingFragment);

            Assert.Same(mappingFragment, storageTypeMapping.MappingFragments.Single());

            storageTypeMapping.RemoveFragment(mappingFragment);

            Assert.Empty(storageTypeMapping.MappingFragments);
        }

        [Fact]
        public void Can_get_set_mapping()
        {
            var storageSetMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            var storageTypeMapping
                = new EntityTypeMapping(storageSetMapping);

            Assert.Same(storageSetMapping, storageTypeMapping.SetMapping);
        }

        [Fact]
        public void Can_not_add_null_fragment()
        {
            var storageSetMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));

            Assert.Equal(
                "fragment",
                Assert.Throws<ArgumentNullException>(
                    () => new EntityTypeMapping(storageSetMapping).AddFragment(null)).ParamName);
        }

        [Fact]
        public void Cannot_add_mapping_fragment_when_read_only()
        {
            var setMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));
            var typeMapping
                = new EntityTypeMapping(setMapping);

            typeMapping.SetReadOnly();

            var mappingFragment = new MappingFragment(new EntitySet(), typeMapping, false);

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => typeMapping.AddFragment(mappingFragment)).Message);
        }

        [Fact]
        public void Cannot_remove_mapping_fragment_when_read_only()
        {
            var setMapping
                = new EntitySetMapping(
                    new EntitySet(),
                    new EntityContainerMapping(new EntityContainer("C", DataSpace.CSpace)));
            var typeMapping
                = new EntityTypeMapping(setMapping);
            var mappingFragment = new MappingFragment(new EntitySet(), typeMapping, false);

            typeMapping.AddFragment(mappingFragment);

            typeMapping.SetReadOnly();

            Assert.Equal(
                Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => typeMapping.RemoveFragment(mappingFragment)).Message);
        }
    }
}
