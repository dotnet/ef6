// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class EntityTypeExtensionsTests
    {
        [Fact]
        public void AddColumn_should_set_properties_and_add_to_columns()
        {
            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);

            var tableColumn = new EdmProperty(
                "Foo",
                ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))));
            table.AddColumn(tableColumn);

            Assert.NotNull(tableColumn);
            Assert.Equal("Foo", tableColumn.Name);
            Assert.True(table.Properties.Contains(tableColumn));
        }
    }
}
