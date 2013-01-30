// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using Xunit;

    public class ProviderRowFinderTests : TestBase
    {
        [Fact]
        public void FindRow_filters_by_hint_type_when_provided()
        {
            HintTest(typeof(DbContext));
        }

        [Fact]
        public void FindRow_considers_all_assemblies_when_hint_type_is_null()
        {
            HintTest(null);
        }

        private static void HintTest(Type hintType)
        {
            var foundRows = new List<DataRow>();

            new ProviderRowFinder(CreateTestRows())
                .FindRow(
                    hintType,
                    r =>
                        {
                            foundRows.Add(r);
                            return false;
                        });

            Assert.Equal(hintType == null ? 3 : 1, foundRows.Count);
            Assert.Equal(1, foundRows.Count(r => (string)r["AssemblyQualifiedName"] == typeof(DbContext).AssemblyQualifiedName));
        }

        [Fact]
        public void FindRow_returns_null_when_no_row_matches()
        {
            Assert.Null(new ProviderRowFinder(CreateTestRows()).FindRow(null, r => false));
        }

        private static IEnumerable<DataRow> CreateTestRows()
        {
            return new[]
                       {
                           CreateProviderRow("Row1", "Row.1", typeof(DbConnection).AssemblyQualifiedName),
                           CreateProviderRow("Row2", "Row.2", typeof(DbContext).AssemblyQualifiedName),
                           CreateProviderRow("Row3", "Row.3", typeof(TestBase).AssemblyQualifiedName),
                       };
        }
    }
}
