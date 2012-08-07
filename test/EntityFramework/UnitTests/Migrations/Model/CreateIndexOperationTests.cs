// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class CreateIndexOperationTests
    {
        [Fact]
        public void Can_get_and_set_table_and_column_info()
        {
            var createIndexOperation
                = new CreateIndexOperation
                      {
                          Table = "T",
                          IsUnique = true,
                          Name = "Custom"
                      };

            createIndexOperation.Columns.Add("C");

            Assert.Equal("T", createIndexOperation.Table);
            Assert.Equal("C", createIndexOperation.Columns.Single());
            Assert.Equal("Custom", createIndexOperation.Name);
            Assert.True(createIndexOperation.IsUnique);
            Assert.Equal("IX_C", createIndexOperation.DefaultName);
            Assert.False(createIndexOperation.HasDefaultName);
        }

        [Fact]
        public void DefaultName_is_restricted_to_128_chars()
        {
            var createIndexOperation = new CreateIndexOperation
                                           {
                                               Table = "T"
                                           };

            createIndexOperation.Columns.Add(new string('C', 150));

            Assert.Equal(128, createIndexOperation.DefaultName.Length);
        }

        [Fact]
        public void Inverse_should_return_drop_index_operation()
        {
            var createIndexOperation
                = new CreateIndexOperation
                      {
                          Table = "T",
                          IsUnique = true,
                          Name = "Custom"
                      };

            createIndexOperation.Columns.Add("C");

            var inverse = (DropIndexOperation)createIndexOperation.Inverse;

            Assert.Same(createIndexOperation, inverse.Inverse);
            Assert.Equal("T", inverse.Table);
            Assert.Equal("Custom", inverse.Name);
            Assert.Equal("C", inverse.Columns.Single());
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentException(Strings.ArgumentIsNullOrWhitespace("value")).Message,
                Assert.Throws<ArgumentException>(
                    () => new CreateIndexOperation
                              {
                                  Table = null
                              }).Message);
        }
    }
}
