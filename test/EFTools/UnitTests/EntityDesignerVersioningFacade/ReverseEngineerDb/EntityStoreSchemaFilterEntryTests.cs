// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Globalization;
    using Xunit;

    public class EntityStoreSchemaFilterEntryTests
    {
        [Fact]
        public void EntityStoreSchemaFilterEntry_ctor_sets_fields()
        {
            var filterEntry =
                new EntityStoreSchemaFilterEntry(
                    "catalog",
                    "schema",
                    "name",
                    EntityStoreSchemaFilterObjectTypes.Table,
                    EntityStoreSchemaFilterEffect.Exclude);

            Assert.Equal("catalog", filterEntry.Catalog);
            Assert.Equal("schema", filterEntry.Schema);
            Assert.Equal("name", filterEntry.Name);
            Assert.Equal(EntityStoreSchemaFilterObjectTypes.Table, filterEntry.Types);
            Assert.Equal(EntityStoreSchemaFilterEffect.Exclude, filterEntry.Effect);
        }

        [Fact]
        public void EntityStoreSchemaFilterEntry_ctor_sets_types_All_and_effect_Allows_by_default()
        {
            var filterEntry =
                new EntityStoreSchemaFilterEntry("catalog", "schema", "name");

            Assert.Equal("catalog", filterEntry.Catalog);
            Assert.Equal("schema", filterEntry.Schema);
            Assert.Equal("name", filterEntry.Name);
            Assert.Equal(EntityStoreSchemaFilterObjectTypes.All, filterEntry.Types);
            Assert.Equal(EntityStoreSchemaFilterEffect.Allow, filterEntry.Effect);
        }

        [Fact]
        public void EntityStoreSchemaFilterEntry_ctor_throws_for_Types_None()
        {
            Assert.Equal(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources_VersioningFacade.InvalidStringArgument,
                    "types"),
                Assert.Throws<ArgumentException>(
                    () => new EntityStoreSchemaFilterEntry(
                              null,
                              null,
                              null,
                              EntityStoreSchemaFilterObjectTypes.None,
                              EntityStoreSchemaFilterEffect.Exclude)).Message);
        }
    }
}
