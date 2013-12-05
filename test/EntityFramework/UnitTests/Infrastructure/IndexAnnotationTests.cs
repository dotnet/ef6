// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class IndexAnnotationTests
    {
        [Fact]
        public void Constructors_check_arguments()
        {
            Assert.Equal(
                "index",
                Assert.Throws<ArgumentNullException>(() => new IndexAnnotation((IndexAttribute)null)).ParamName);

            Assert.Equal(
                "indexes",
                Assert.Throws<ArgumentNullException>(() => new IndexAnnotation((IEnumerable<IndexAttribute>)null)).ParamName);

            Assert.Equal(
                "index",
                Assert.Throws<ArgumentNullException>(() => new IndexAnnotation(new IndexAttribute[] { null })).ParamName);
        }

        [Fact]
        public void Multiple_attributes_with_a_given_name_are_merged_when_IndexAnnotation_is_constructed()
        {
            var annotation = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute(),
                    new IndexAttribute("EekyBear"),
                    new IndexAttribute { Order = 1 },
                    new IndexAttribute("EekyBear") { Order = 0 },
                    new IndexAttribute { IsClustered = true, IsUnique = false },
                    new IndexAttribute("EekyBear") { Order = 0, IsClustered = false, IsUnique = true }
                });

            var attributes = annotation.Indexes.ToArray();
            Assert.Equal(2, attributes.Length);

            Assert.Null(attributes[0].Name);
            Assert.Equal(1, attributes[0].Order);
            Assert.Equal(true, attributes[0].ClusteredConfiguration);
            Assert.Equal(false, attributes[0].UniqueConfiguration);

            Assert.Equal("EekyBear", attributes[1].Name);
            Assert.Equal(0, attributes[1].Order);
            Assert.Equal(false, attributes[1].ClusteredConfiguration);
            Assert.Equal(true, attributes[1].UniqueConfiguration);
        }

        [Fact]
        public void Public_constructor_throws_if_given_attributes_are_not_compatible()
        {
            Assert.Equal(
                Strings.ConflictingIndexAttribute(
                    "EekyBear", Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("Order", "1", "0")),
                Assert.Throws<InvalidOperationException>(
                    () => new IndexAnnotation(
                        new[]
                        {
                            new IndexAttribute("EekyBear"),
                            new IndexAttribute("EekyBear") { Order = 0 },
                            new IndexAttribute("EekyBear") { Order = 1, IsClustered = false, IsUnique = true }
                        })).Message);
        }

        [Fact]
        public void Internal_constructor_throws_with_property_info_if_given_attributes_are_not_compatible()
        {
            Assert.Equal(
                Strings.ConflictingIndexAttributesOnProperty(
                    "SomeProperty", "IndexAnnotationTests", "EekyBear",
                    Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("Order", "1", "0")),
                Assert.Throws<InvalidOperationException>(
                    () => new IndexAnnotation(
                        GetType().GetProperty("SomeProperty"),
                        new[]
                        {
                            new IndexAttribute("EekyBear"),
                            new IndexAttribute("EekyBear") { Order = 0 },
                            new IndexAttribute("EekyBear") { Order = 1, IsClustered = false, IsUnique = true }
                        })).Message);
        }

        public int SomeProperty { get; set; }

        [Fact]
        public void Indexes_returns_list_of_contained_attributes()
        {
            Assert.Equal(
                new[] { "EekyBear", "MrsPandy" },
                new IndexAnnotation(
                    new[]
                    {
                        new IndexAttribute("EekyBear"),
                        new IndexAttribute("MrsPandy")
                    }).Indexes.Select(i => i.Name));

            Assert.Equal("Tarquin", new IndexAnnotation(new IndexAttribute("Tarquin")).Indexes.Single().Name);
        }

        [Fact]
        public void IsCompatibleWith_returns_true_if_other_is_same_or_null()
        {
            var annotation = new IndexAnnotation(new IndexAttribute());
            Assert.True(annotation.IsCompatibleWith(annotation));
            Assert.True(annotation.IsCompatibleWith(null));
        }

        [Fact]
        public void IsCompatibleWith_returns_false_if_other_is_not_an_IndexAnnotation()
        {
            var annotation = new IndexAnnotation(new IndexAttribute());
            var result = annotation.IsCompatibleWith(new Random());
            Assert.False(result);
            Assert.Equal(Strings.IncompatibleTypes("Random", "IndexAnnotation"), result.ErrorMessage);
        }

        [Fact]
        public void IsCompatibleWith_returns_true_if_contained_index_lists_are_compatible()
        {
            var annotation1 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute(),
                    new IndexAttribute("EekyBear"),
                    new IndexAttribute("EekyBear") { Order = 0, IsClustered = false, IsUnique = true }
                });

            var annotation2 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute { Order = 1 },
                    new IndexAttribute("EekyBear") { Order = 0 },
                    new IndexAttribute { IsClustered = true, IsUnique = false },
                });

            Assert.True(annotation1.IsCompatibleWith(annotation2));
            Assert.True(annotation2.IsCompatibleWith(annotation1));
        }

        [Fact]
        public void IsCompatibleWith_returns_false_if_any_contained_indexes_are_not_compatible()
        {
            var annotation1 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute(),
                    new IndexAttribute("EekyBear"),
                    new IndexAttribute("EekyBear") { Order = 0, IsClustered = false, IsUnique = true }
                });

            var annotation2 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute { Order = 1 },
                    new IndexAttribute("EekyBear") { Order = 1 },
                    new IndexAttribute { IsClustered = true, IsUnique = false },
                });

            var result = annotation1.IsCompatibleWith(annotation2);
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("Order", "0", "1"), result.ErrorMessage);

            result = annotation2.IsCompatibleWith(annotation1);
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("Order", "1", "0"), result.ErrorMessage);
        }

        [Fact]
        public void MergeWith_returns_current_instance_if_other_is_same_or_null()
        {
            var annotation = new IndexAnnotation(new IndexAttribute());
            Assert.Same(annotation, annotation.MergeWith(annotation));
            Assert.Same(annotation, annotation.MergeWith(null));
        }

        [Fact]
        public void MergeWith_throws_if_other_is_not_an_IndexAnnotation()
        {
            var annotation = new IndexAnnotation(new IndexAttribute());
            Assert.Equal(
                Strings.IncompatibleTypes("Random", "IndexAnnotation"),
                Assert.Throws<ArgumentException>(() => annotation.MergeWith(new Random())).Message);
        }

        [Fact]
        public void MergeWith_merges_index_lists_from_both_annotations()
        {
            var annotation1 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute(),
                    new IndexAttribute("EekyBear"),
                    new IndexAttribute("EekyBear") { Order = 0, IsClustered = false, IsUnique = true }
                });

            var annotation2 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute { Order = 1 },
                    new IndexAttribute("EekyBear") { Order = 0 },
                    new IndexAttribute { IsClustered = true, IsUnique = false },
                });

            var attributes = ((IndexAnnotation)annotation1.MergeWith(annotation2)).Indexes.ToArray();
            Assert.Equal(2, attributes.Length);

            Assert.Equal("EekyBear", attributes[0].Name);
            Assert.Equal(0, attributes[0].Order);
            Assert.Equal(false, attributes[0].ClusteredConfiguration);
            Assert.Equal(true, attributes[0].UniqueConfiguration);

            Assert.Null(attributes[1].Name);
            Assert.Equal(1, attributes[1].Order);
            Assert.Equal(true, attributes[1].ClusteredConfiguration);
            Assert.Equal(false, attributes[1].UniqueConfiguration);

            attributes = ((IndexAnnotation)annotation2.MergeWith(annotation1)).Indexes.ToArray();
            Assert.Equal(2, attributes.Length);

            Assert.Null(attributes[0].Name);
            Assert.Equal(1, attributes[0].Order);
            Assert.Equal(true, attributes[0].ClusteredConfiguration);
            Assert.Equal(false, attributes[0].UniqueConfiguration);

            Assert.Equal("EekyBear", attributes[1].Name);
            Assert.Equal(0, attributes[1].Order);
            Assert.Equal(false, attributes[1].ClusteredConfiguration);
            Assert.Equal(true, attributes[1].UniqueConfiguration);
        }

        [Fact]
        public void MergeLists_throws_if_lists_are_not_compatible()
        {
            var annotation1 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute(),
                    new IndexAttribute("EekyBear"),
                    new IndexAttribute("EekyBear") { Order = 0, IsClustered = false, IsUnique = true }
                });

            var annotation2 = new IndexAnnotation(
                new[]
                {
                    new IndexAttribute { Order = 1 },
                    new IndexAttribute("EekyBear") { Order = 1 },
                    new IndexAttribute { IsClustered = true, IsUnique = false },
                });

            Assert.Equal(
                Strings.ConflictingIndexAttribute(
                    "EekyBear", Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("Order", "1", "0")),
                Assert.Throws<InvalidOperationException>(
                    () => annotation1.MergeWith(annotation2)).Message);

            Assert.Equal(
                Strings.ConflictingIndexAttribute(
                    "EekyBear", Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("Order", "0", "1")),
                Assert.Throws<InvalidOperationException>(
                    () => annotation2.MergeWith(annotation1)).Message);
        }
    }
}
