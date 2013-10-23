// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System.Collections.Generic;
    using Xunit;

    public class FunctionDetailsReaderTests
    {
        private readonly EntityClientMockFactory entityClientMockFactory
            = new EntityClientMockFactory();

        [Fact]
        public void CurrentRow_is_null_for_empty_reader()
        {
            using (var functionDetailsReader =
                new FunctionDetailsReader(
                    entityClientMockFactory.CreateMockEntityCommand(null).Object,
                    EntityFrameworkVersion.Version3))
            {
                Assert.Null(functionDetailsReader.CurrentRow);
                Assert.False(functionDetailsReader.Read());
                Assert.Null(functionDetailsReader.CurrentRow);
            }
        }

        [Fact]
        public void CurrentRow_exposes_underlying_reader_values()
        {
            var expectedValues = new object[12];
            expectedValues[0] = "catalog";

            using (var functionDetailsReader =
                new FunctionDetailsReader(
                    entityClientMockFactory.CreateMockEntityCommand(
                        new List<object[]> { expectedValues }).Object,
                    EntityFrameworkVersion.Version3))
            {
                Assert.Null(functionDetailsReader.CurrentRow);
                Assert.True(functionDetailsReader.Read());
                Assert.NotNull(functionDetailsReader.CurrentRow);
                Assert.Equal("catalog", functionDetailsReader.CurrentRow.Catalog);
                Assert.False(functionDetailsReader.Read());
                Assert.Null(functionDetailsReader.CurrentRow);
            }
        }
    }
}
