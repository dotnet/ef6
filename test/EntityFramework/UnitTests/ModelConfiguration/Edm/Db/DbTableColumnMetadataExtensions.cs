// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Db.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public sealed class DbTableColumnMetadataExtensions
    {
        [Fact]
        public void Can_get_and_set_can_override_annotation()
        {
            var tableColumn = new EdmProperty("C");

            tableColumn.SetAllowOverride(true);

            Assert.True(tableColumn.GetAllowOverride());

            tableColumn.SetAllowOverride(false);

            Assert.False(tableColumn.GetAllowOverride());
        }
    }
}
