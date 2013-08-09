// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Moq;
    using Xunit;

    public class FieldNameLookupTests
    {
        [Fact]
        public void Lookup_can_be_built_from_collection()
        {
            var lookup = new FieldNameLookup(new ReadOnlyCollection<string>(new List<string> { "A", "B", "C" }));

            Assert.Equal(0, lookup.GetOrdinal("A"));
            Assert.Equal(1, lookup.GetOrdinal("B"));
            Assert.Equal(2, lookup.GetOrdinal("C"));
        }

        [Fact]
        public void Lookup_can_be_built_from_reader()
        {
            var mockDataRecord = new Mock<IDataRecord>();
            mockDataRecord.Setup(m => m.FieldCount).Returns(3);
            mockDataRecord.Setup(m => m.GetName(0)).Returns("A");
            mockDataRecord.Setup(m => m.GetName(1)).Returns("B");
            mockDataRecord.Setup(m => m.GetName(2)).Returns("C");

            var lookup = new FieldNameLookup(mockDataRecord.Object);

            Assert.Equal(0, lookup.GetOrdinal("A"));
            Assert.Equal(1, lookup.GetOrdinal("B"));
            Assert.Equal(2, lookup.GetOrdinal("C"));
        }

        [Fact]
        public void GetOrdinal_uses_case_sensitive_then_insensitive_then_ignores_kana()
        {
            var lookup = new FieldNameLookup(new ReadOnlyCollection<string>(new List<string> { "A", "a", "b", "ゕ" }));

            Assert.Equal(0, lookup.GetOrdinal("A"));
            Assert.Equal(1, lookup.GetOrdinal("a"));
            Assert.Equal(2, lookup.GetOrdinal("B"));
            Assert.Equal(3, lookup.GetOrdinal("ヵ"));
        }

        [Fact]
        public void GetOrdinal_returns_lowest_ordinal_if_same_name_appears_more_than_once()
        {
            var lookup = new FieldNameLookup(new ReadOnlyCollection<string>(new List<string> { "A", "B", "B", "A" }));

            Assert.Equal(0, lookup.GetOrdinal("A"));
            Assert.Equal(1, lookup.GetOrdinal("B"));
        }

        [Fact]
        public void GetOrdinal_throws_if_name_is_not_found()
        {
            var lookup = new FieldNameLookup(new ReadOnlyCollection<string>(new List<string> { "A", "a", "b", "ゕ" }));

            Assert.Equal(
                "ヶ",
                Assert.Throws<IndexOutOfRangeException>(() => lookup.GetOrdinal("ヶ")).Message);
        }
    }
}
