// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    public class DropIndexOperationTests
    {
        [Fact]
        public void Can_get_and_set_properties()
        {
            var dropIndexOperation
                = new DropIndexOperation
                    {
                        Table = "T",
                        Name = "Custom"
                    };

            dropIndexOperation.Columns.Add("foo");
            dropIndexOperation.Columns.Add("bar");

            Assert.Equal("T", dropIndexOperation.Table);
            Assert.Equal("foo", dropIndexOperation.Columns.First());
            Assert.Equal("bar", dropIndexOperation.Columns.Last());
            Assert.Equal("Custom", dropIndexOperation.Name);
            Assert.Equal("IX_foo_bar", dropIndexOperation.DefaultName);
            Assert.False(dropIndexOperation.HasDefaultName);
        }
    }
}