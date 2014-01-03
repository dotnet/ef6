// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class ConsolidatedIndexTests
    {
        [Fact]
        public void Add_adds_merges_all_indexes()
        {
            var index = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 0));
            index.Add("tut3", new IndexAttribute("pong", 2) { IsClustered = true });
            index.Add("tut2", new IndexAttribute("pong", 1));

            Assert.Equal("pong", index.Index.Name);
            Assert.Equal(new[] { "tut1", "tut2", "tut3" }, index.ColumnNames);
            Assert.Equal(true, index.Index.ClusteredConfiguration);
            Assert.Null(index.Index.UniqueConfiguration);
        }

        [Fact]
        public void Add_throws_on_column_order_clashes()
        {
            var index = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 77));

            Assert.Equal(
                Strings.OrderConflictWhenConsolidating("pong", "raS", 77,  "tut1", "tut2"),
                Assert.Throws<InvalidOperationException>(() => index.Add("tut2", new IndexAttribute("pong", 77))).Message);
        }

        [Fact]
        public void Add_throws_when_index_of_given_name_conflicts_with_existing_index_for_that_name()
        {
            var index = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 0) { IsClustered = false });

            Assert.Equal(
                Strings.ConflictWhenConsolidating("pong", "raS", Strings.ConflictingIndexAttributeProperty("IsClustered", "False", "True")),
                Assert.Throws<InvalidOperationException>(
                    () => index.Add("tut3", new IndexAttribute("pong", 2) { IsClustered = true })).Message);
        }

        [Fact]
        public void BuildIndexes_creates_consolidated_index_for_each_named_and_unamed_index()
        {
            var columns = new[]
            {
                CreateColumn("tut1", new IndexAttribute("pong1", 0), new IndexAttribute("pong2", 4)),
                CreateColumn("tut2"),
                CreateColumn("tut3", new IndexAttribute("pong1", 1), new IndexAttribute("pong3"), new IndexAttribute()),
                CreateColumn("tut4", new IndexAttribute("pong1", 2), new IndexAttribute("pong2", 1)),
                CreateColumn("tut5", new IndexAttribute())
            };

            var indexes = ConsolidatedIndex.BuildIndexes("raS", columns).ToArray();

            Assert.Equal(5, indexes.Length);

            var index = indexes.Single(i => i.Index.Name == "pong1");
            Assert.Equal(new[] { "tut1", "tut3", "tut4" }, index.ColumnNames);

            index = indexes.Single(i => i.Index.Name == "pong2");
            Assert.Equal(new[] { "tut4", "tut1" }, index.ColumnNames);

            index = indexes.Single(i => i.Index.Name == "pong3");
            Assert.Equal(new[] { "tut3" }, index.ColumnNames);

            var unnamedIndexes = indexes.Where(i => i.Index.Name == null).ToArray();
            Assert.Equal(2, unnamedIndexes.Length);
            Assert.Equal(new[] { "tut3", "tut5" }, unnamedIndexes.Select(i => i.ColumnNames.Single()).OrderBy(n => n));
        }

        private static Tuple<string, EdmProperty> CreateColumn(string columnName, params IndexAttribute[] indexes)
        {
            var property = EdmProperty.CreatePrimitive("old" + columnName, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            property.AddAnnotation(XmlConstants.IndexAnnotationWithPrefix, new IndexAnnotation(indexes));

            return Tuple.Create(columnName, property);
        }

        [Fact]
        public void CreateCreateIndexOperation_creates_operation_and_its_inverse()
        {
            var index = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 0) { IsClustered = true });
            index.Add("tut2", new IndexAttribute("pong", 1));

            var operation = index.CreateCreateIndexOperation();

            Assert.Equal("raS", operation.Table);
            Assert.Equal("pong", operation.Name);
            Assert.Equal(new List<string> { "tut1", "tut2" }, operation.Columns);
            Assert.False(operation.IsUnique);
            Assert.True(operation.IsClustered);

            var inverse = (DropIndexOperation)operation.Inverse;

            Assert.Equal("raS", inverse.Table);
            Assert.Equal("pong", inverse.Name);
            Assert.Equal(new List<string> { "tut1", "tut2" }, inverse.Columns);
        }

        [Fact]
        public void CreateCreateIndexOperation_creates_operation_and_its_inverse_for_unnamed_index()
        {
            var index = new ConsolidatedIndex("raS", "tut", new IndexAttribute { IsUnique = true });

            var operation = index.CreateCreateIndexOperation();

            Assert.Equal("raS", operation.Table);
            Assert.Equal("tutIndex", operation.Name);
            Assert.Equal(new List<string> { "tut" }, operation.Columns);
            Assert.True(operation.IsUnique);
            Assert.False(operation.IsClustered);

            var inverse = (DropIndexOperation)operation.Inverse;

            Assert.Equal("raS", inverse.Table);
            Assert.Equal("tutIndex", inverse.Name);
            Assert.Equal(new List<string> { "tut" }, inverse.Columns);
        }

        [Fact]
        public void CreateDropIndexOperation_creates_operation_and_its_inverse()
        {
            var index = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 0) { IsClustered = true, IsUnique = true });
            index.Add("tut2", new IndexAttribute("pong", 1));

            var operation = index.CreateDropIndexOperation();

            Assert.Equal("raS", operation.Table);
            Assert.Equal("pong", operation.Name);
            Assert.Equal(new List<string> { "tut1", "tut2" }, operation.Columns);

            var inverse = (CreateIndexOperation)operation.Inverse;

            Assert.Equal("raS", inverse.Table);
            Assert.Equal("pong", inverse.Name);
            Assert.Equal(new List<string> { "tut1", "tut2" }, inverse.Columns);
            Assert.True(inverse.IsUnique);
            Assert.True(inverse.IsClustered);
        }

        [Fact]
        public void CreateDropIndexOperation_creates_operation_and_its_inverse_for_unnamed_index()
        {
            var index = new ConsolidatedIndex("raS", "tut", new IndexAttribute());

            var operation = index.CreateDropIndexOperation();

            Assert.Equal("raS", operation.Table);
            Assert.Equal("tutIndex", operation.Name);
            Assert.Equal(new List<string> { "tut" }, operation.Columns);

            var inverse = (CreateIndexOperation)operation.Inverse;

            Assert.Equal("raS", inverse.Table);
            Assert.Equal("tutIndex", inverse.Name);
            Assert.Equal(new List<string> { "tut" }, inverse.Columns);
            Assert.False(inverse.IsUnique);
            Assert.False(inverse.IsClustered);
        }

        [Fact]
        public void Equals_returns_true_for_equal_indexes()
        {
            var index1 = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 0) { IsClustered = true });
            index1.Add("tut2", new IndexAttribute("pong", 1) { IsUnique = true });

            var index2 = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 3) { IsUnique = true });
            index2.Add("tut2", new IndexAttribute("pong", 4) { IsClustered = true });

            Assert.True(index1.Equals(index1));
            Assert.True(index1.Equals(index2));
            Assert.True(index2.Equals(index1));
        }

        [Fact]
        public void Equals_returns_false_for_different_indexes_with_different_tables()
        {
            var index1 = new ConsolidatedIndex("raS1", "tut", new IndexAttribute());
            var index2 = new ConsolidatedIndex("raS2", "tut", new IndexAttribute());

            Assert.False(index1.Equals(index2));
        }

        [Fact]
        public void Equals_returns_false_for_different_indexes_with_different_names()
        {
            var index1 = new ConsolidatedIndex("raS", "tut", new IndexAttribute("pong1"));
            var index2 = new ConsolidatedIndex("raS", "tut", new IndexAttribute("pong2"));

            Assert.False(index1.Equals(index2));
        }

        [Fact]
        public void Equals_returns_false_for_different_indexes_with_different_configuration()
        {
            var index1 = new ConsolidatedIndex("raS", "tut", new IndexAttribute { IsClustered = true });
            var index2 = new ConsolidatedIndex("raS", "tut", new IndexAttribute { IsClustered = false });

            Assert.False(index1.Equals(index2));
        }

        [Fact]
        public void Equals_returns_false_for_null_or_wrong_object()
        {
            var index = new ConsolidatedIndex("raS", "tut", new IndexAttribute());

            Assert.False(index.Equals(null));
            Assert.False(index.Equals(new Random()));
        }

        [Fact]
        public void Equals_returns_false_for_different_indexes_with_different_column_orders()
        {
            var index1 = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 0));
            index1.Add("tut2", new IndexAttribute("pong", 1));

            var index2 = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 1));
            index2.Add("tut2", new IndexAttribute("pong", 0));

            Assert.False(index1.Equals(index2));
        }

        [Fact]
        public void GetHashCode_returns_the_same_value_for_equal_indexes()
        {
            var index1 = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 0) { IsClustered = true });
            index1.Add("tut2", new IndexAttribute("pong", 1) { IsUnique = true });

            var index2 = new ConsolidatedIndex("raS", "tut1", new IndexAttribute("pong", 3) { IsUnique = true });
            index2.Add("tut2", new IndexAttribute("pong", 4) { IsClustered = true });

            Assert.Equal(index1.GetHashCode(), index2.GetHashCode());
        }
    }
}
