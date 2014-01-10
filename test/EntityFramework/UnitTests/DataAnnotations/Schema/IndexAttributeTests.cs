// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Resources;
    using Xunit;

    public class IndexAttributeTests
    {
        [Fact]
        public void Constructors_check_arguments()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new IndexAttribute(null)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new IndexAttribute(" ")).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new IndexAttribute(null, 0)).Message);

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("name"),
                Assert.Throws<ArgumentException>(() => new IndexAttribute(" ", 0)).Message);

            Assert.Equal(
                "order",
                Assert.Throws<ArgumentOutOfRangeException>(() => new IndexAttribute("I1", -1)).ParamName);
        }

        [Fact]
        public void Name_property_returns_set_name_or_null()
        {
            Assert.Null(new IndexAttribute().Name);
            Assert.Equal("EekyBear", new IndexAttribute("EekyBear").Name);
            Assert.Equal("EekyBear", new IndexAttribute("EekyBear", 0).Name);
        }

        [Fact]
        public void Order_property_returns_set_order_or_minus_one()
        {
            Assert.Equal(-1, new IndexAttribute().Order);
            Assert.Equal(-1, new IndexAttribute("EekyBear").Order);
            Assert.Equal(1, new IndexAttribute("EekyBear", 1).Order);
            Assert.Equal(1, new IndexAttribute { Order = 1 }.Order);
        }

        [Fact]
        public void Order_checks_arguments()
        {
            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => new IndexAttribute { Order = -1 }).ParamName);
        }

        [Fact]
        public void Can_get_and_set_clustered_configuration()
        {
            Assert.False(new IndexAttribute().IsClusteredConfigured);
            Assert.True(new IndexAttribute { IsClustered = true }.IsClusteredConfigured);
            Assert.True(new IndexAttribute { IsClustered = true }.IsClustered);
            Assert.True(new IndexAttribute { IsClustered = false }.IsClusteredConfigured);
            Assert.False(new IndexAttribute { IsClustered = false }.IsClustered);
        }

        [Fact]
        public void Can_get_and_set_unique_configuration()
        {
            Assert.False(new IndexAttribute().IsUniqueConfigured);
            Assert.True(new IndexAttribute { IsUnique = true }.IsUniqueConfigured);
            Assert.True(new IndexAttribute { IsUnique = true }.IsUnique);
            Assert.True(new IndexAttribute { IsUnique = false }.IsUniqueConfigured);
            Assert.False(new IndexAttribute { IsUnique = false }.IsUnique);
        }

        [Fact]
        public void TypeId_returns_different_values_for_different_instances()
        {
            Assert.NotEqual(new IndexAttribute().TypeId, new IndexAttribute().TypeId);

            var index = new IndexAttribute();
            Assert.Equal(index.TypeId, index.TypeId);
        }

        [Fact]
        public void IsCompatibleWith_returns_true_if_other_is_same_or_null()
        {
            var attribute = new IndexAttribute();
            Assert.True(attribute.IsCompatibleWith(attribute));
            Assert.True(attribute.IsCompatibleWith(null));
        }

        [Fact]
        public void IsCompatibleWith_returns_true_when_no_conflicts()
        {
            Assert.True(new IndexAttribute().IsCompatibleWith(new IndexAttribute()));

            Assert.True(new IndexAttribute("MrsPandy").IsCompatibleWith(new IndexAttribute("MrsPandy")));

            Assert.True(new IndexAttribute { Order = 7 }.IsCompatibleWith(new IndexAttribute()));
            Assert.True(new IndexAttribute().IsCompatibleWith(new IndexAttribute { Order = 7 }));
            Assert.True(new IndexAttribute { Order = 7 }.IsCompatibleWith(new IndexAttribute { Order = 7 }));

            Assert.True(new IndexAttribute { IsClustered = true }.IsCompatibleWith(new IndexAttribute()));
            Assert.True(new IndexAttribute().IsCompatibleWith(new IndexAttribute { IsClustered = true }));
            Assert.True(new IndexAttribute { IsClustered = true }.IsCompatibleWith(new IndexAttribute { IsClustered = true }));

            Assert.True(new IndexAttribute { IsClustered = false }.IsCompatibleWith(new IndexAttribute()));
            Assert.True(new IndexAttribute().IsCompatibleWith(new IndexAttribute { IsClustered = false }));
            Assert.True(new IndexAttribute { IsClustered = false }.IsCompatibleWith(new IndexAttribute { IsClustered = false }));

            Assert.True(new IndexAttribute { IsUnique = true }.IsCompatibleWith(new IndexAttribute()));
            Assert.True(new IndexAttribute().IsCompatibleWith(new IndexAttribute { IsUnique = true }));
            Assert.True(new IndexAttribute { IsUnique = true }.IsCompatibleWith(new IndexAttribute { IsUnique = true }));

            Assert.True(new IndexAttribute { IsUnique = false }.IsCompatibleWith(new IndexAttribute()));
            Assert.True(new IndexAttribute().IsCompatibleWith(new IndexAttribute { IsUnique = false }));
            Assert.True(new IndexAttribute { IsUnique = false }.IsCompatibleWith(new IndexAttribute { IsUnique = false }));

            Assert.True(
                new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }
                    .IsCompatibleWith(new IndexAttribute("MrsPandy")));

            Assert.True(
                new IndexAttribute("MrsPandy")
                    .IsCompatibleWith(new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }));

            Assert.True(
                new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }
                    .IsCompatibleWith(new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }));
        }

        [Fact]
        public void IsCompatibleWith_returns_false_for_non_matching_properties()
        {
            var result = new IndexAttribute("MrsPandy").IsCompatibleWith(new IndexAttribute("EekyBear"));
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("Name", "MrsPandy", "EekyBear"), result.ErrorMessage);

            result = new IndexAttribute().IsCompatibleWith(new IndexAttribute("EekyBear"));
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("Name", "", "EekyBear"), result.ErrorMessage);

            result = new IndexAttribute("MrsPandy").IsCompatibleWith(new IndexAttribute());
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("Name", "MrsPandy", ""), result.ErrorMessage);

            result = new IndexAttribute { Order = 7 }.IsCompatibleWith(new IndexAttribute { Order = 8 });
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("Order", "7", "8"), result.ErrorMessage);

            result = new IndexAttribute { IsClustered = false }.IsCompatibleWith(new IndexAttribute { IsClustered = true });
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("IsClustered", "False", "True"), result.ErrorMessage);

            result = new IndexAttribute { IsUnique = true }.IsCompatibleWith(new IndexAttribute { IsUnique = false });
            Assert.False(result);
            Assert.Equal(Strings.ConflictingIndexAttributeProperty("IsUnique", "True", "False"), result.ErrorMessage);


            result = new IndexAttribute("MrsPandy", 8) { IsClustered = true, IsUnique = false }
                .IsCompatibleWith(new IndexAttribute("EekyBear", 7) { IsClustered = false, IsUnique = true });
            Assert.False(result);

            Assert.Equal(
                Strings.ConflictingIndexAttributeProperty("Name", "MrsPandy", "EekyBear")
                + Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("Order", "8", "7")
                + Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("IsClustered", "True", "False")
                + Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("IsUnique", "False", "True"),
                result.ErrorMessage);
        }

        [Fact]
        public void IsCompatibleWith_can_ignore_order()
        {
            Assert.True(new IndexAttribute { Order = 7 }.IsCompatibleWith(new IndexAttribute { Order = 5 }, ignoreOrder: true));
        }

        [Fact]
        public void MergeWith_returns_same_instance_if_other_is_same_or_null()
        {
            var attribute = new IndexAttribute();
            Assert.Same(attribute, attribute.MergeWith(attribute));
            Assert.Same(attribute, attribute.MergeWith(null));
        }

        [Fact]
        public void MergeWith_throws_if_attributes_are_not_compatible()
        {
            Assert.Equal(
                Strings.ConflictingIndexAttribute(
                    "EekyBear", Environment.NewLine + "\t" + Strings.ConflictingIndexAttributeProperty("Order", "7", "8")),
                Assert.Throws<InvalidOperationException>(
                    () => new IndexAttribute("EekyBear", 7).MergeWith(new IndexAttribute("EekyBear", 8))).Message);
        }

        [Fact]
        public void MergeWith_merges_properties_of_compatible_attributes()
        {
            Assert.Null(new IndexAttribute().MergeWith(new IndexAttribute()).Name);

            Assert.Equal("MrsPandy", new IndexAttribute("MrsPandy").MergeWith(new IndexAttribute("MrsPandy")).Name);

            Assert.Equal(7, new IndexAttribute { Order = 7 }.MergeWith(new IndexAttribute()).Order);
            Assert.Equal(7, new IndexAttribute().MergeWith(new IndexAttribute { Order = 7 }).Order);
            Assert.Equal(7, new IndexAttribute { Order = 7 }.MergeWith(new IndexAttribute { Order = 7 }).Order);

            Assert.True(new IndexAttribute { IsClustered = true }.MergeWith(new IndexAttribute()).IsClusteredConfigured);
            Assert.True(new IndexAttribute().MergeWith(new IndexAttribute { IsClustered = true }).IsClusteredConfigured);
            Assert.True(new IndexAttribute { IsClustered = true }.MergeWith(new IndexAttribute { IsClustered = true }).IsClusteredConfigured);

            Assert.True(new IndexAttribute { IsClustered = true }.MergeWith(new IndexAttribute()).IsClustered);
            Assert.True(new IndexAttribute().MergeWith(new IndexAttribute { IsClustered = true }).IsClustered);
            Assert.True(new IndexAttribute { IsClustered = true }.MergeWith(new IndexAttribute { IsClustered = true }).IsClustered);

            Assert.True(new IndexAttribute { IsClustered = false }.MergeWith(new IndexAttribute()).IsClusteredConfigured);
            Assert.True(new IndexAttribute().MergeWith(new IndexAttribute { IsClustered = false }).IsClusteredConfigured);
            Assert.True(new IndexAttribute { IsClustered = false }.MergeWith(new IndexAttribute { IsClustered = false }).IsClusteredConfigured);

            Assert.False(new IndexAttribute { IsClustered = false }.MergeWith(new IndexAttribute()).IsClustered);
            Assert.False(new IndexAttribute().MergeWith(new IndexAttribute { IsClustered = false }).IsClustered);
            Assert.False(new IndexAttribute { IsClustered = false }.MergeWith(new IndexAttribute { IsClustered = false }).IsClustered);

            Assert.True(new IndexAttribute { IsUnique = true }.MergeWith(new IndexAttribute()).IsUniqueConfigured);
            Assert.True(new IndexAttribute().MergeWith(new IndexAttribute { IsUnique = true }).IsUniqueConfigured);
            Assert.True(new IndexAttribute { IsUnique = true }.MergeWith(new IndexAttribute { IsUnique = true }).IsUniqueConfigured);

            Assert.True(new IndexAttribute { IsUnique = true }.MergeWith(new IndexAttribute()).IsUnique);
            Assert.True(new IndexAttribute().MergeWith(new IndexAttribute { IsUnique = true }).IsUnique);
            Assert.True(new IndexAttribute { IsUnique = true }.MergeWith(new IndexAttribute { IsUnique = true }).IsUnique);

            Assert.True(new IndexAttribute { IsUnique = false }.MergeWith(new IndexAttribute()).IsUniqueConfigured);
            Assert.True(new IndexAttribute().MergeWith(new IndexAttribute { IsUnique = false }).IsUniqueConfigured);
            Assert.True(new IndexAttribute { IsUnique = false }.MergeWith(new IndexAttribute { IsUnique = false }).IsUniqueConfigured);

            Assert.False(new IndexAttribute { IsUnique = false }.MergeWith(new IndexAttribute()).IsUnique);
            Assert.False(new IndexAttribute().MergeWith(new IndexAttribute { IsUnique = false }).IsUnique);
            Assert.False(new IndexAttribute { IsUnique = false }.MergeWith(new IndexAttribute { IsUnique = false }).IsUnique);

            var merged = new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }
                .MergeWith(new IndexAttribute("MrsPandy"));
            Assert.Equal("MrsPandy", merged.Name);
            Assert.Equal(7, merged.Order);
            Assert.True(merged.IsClusteredConfigured);
            Assert.False(merged.IsClustered);
            Assert.True(merged.IsUniqueConfigured);
            Assert.True(merged.IsUnique);

            merged = new IndexAttribute("MrsPandy")
                .MergeWith(new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false });
            Assert.Equal("MrsPandy", merged.Name);
            Assert.Equal(7, merged.Order);
            Assert.True(merged.IsClusteredConfigured);
            Assert.False(merged.IsClustered);
            Assert.True(merged.IsUniqueConfigured);
            Assert.True(merged.IsUnique);

            merged = new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }
                .MergeWith(new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false });
            Assert.Equal("MrsPandy", merged.Name);
            Assert.Equal(7, merged.Order);
            Assert.True(merged.IsClusteredConfigured);
            Assert.False(merged.IsClustered);
            Assert.True(merged.IsUniqueConfigured);
            Assert.True(merged.IsUnique);
        }

        [Fact]
        public void MergeWith_can_ignore_order()
        {
            Assert.Equal(-1, new IndexAttribute { Order = 5 }.MergeWith(new IndexAttribute { Order = 7 }, ignoreOrder: true).Order);
        }

        [Fact]
        public void Equals_returns_true_when_attributes_match()
        {
            Assert.True(new IndexAttribute().Equals(new IndexAttribute()));
            Assert.True(new IndexAttribute("MrsPandy").Equals(new IndexAttribute("MrsPandy")));
            Assert.True(new IndexAttribute { Order = 7 }.Equals(new IndexAttribute { Order = 7 }));
            Assert.True(new IndexAttribute { IsClustered = true }.Equals(new IndexAttribute { IsClustered = true }));
            Assert.True(new IndexAttribute { IsClustered = false }.Equals(new IndexAttribute { IsClustered = false }));
            Assert.True(new IndexAttribute { IsUnique = true }.Equals(new IndexAttribute { IsUnique = true }));
            Assert.True(new IndexAttribute { IsUnique = false }.Equals(new IndexAttribute { IsUnique = false }));

            Assert.True(
                new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }
                    .Equals(new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }));

            var attribute = new IndexAttribute();
            Assert.True(attribute.Equals(attribute));
        }

        [Fact]
        public void Equals_returns_false_for_different_attributes()
        {
            Assert.False(new IndexAttribute().Equals(null));
            Assert.False(new IndexAttribute().Equals(new object()));
            Assert.False(new IndexAttribute("MrsPandy").Equals(new IndexAttribute("EekyBear")));
            Assert.False(new IndexAttribute().Equals(new IndexAttribute("EekyBear")));
            Assert.False(new IndexAttribute("MrsPandy").Equals(new IndexAttribute()));
            Assert.False(new IndexAttribute { Order = 7 }.Equals(new IndexAttribute { Order = 8 }));
            Assert.False(new IndexAttribute { IsClustered = false }.Equals(new IndexAttribute { IsClustered = true }));
            Assert.False(new IndexAttribute { IsUnique = true }.Equals(new IndexAttribute { IsUnique = false }));

            Assert.False(
                new IndexAttribute("MrsPandy", 8) { IsClustered = true, IsUnique = false }
                    .Equals(new IndexAttribute("EekyBear", 7) { IsClustered = false, IsUnique = true }));
        }

        [Fact]
        public void GetHashCode_returns_same_value_when_attributes_match()
        {
            Assert.Equal(new IndexAttribute().GetHashCode(), new IndexAttribute().GetHashCode());
            Assert.Equal(new IndexAttribute("MrsPandy").GetHashCode(), new IndexAttribute("MrsPandy").GetHashCode());
            Assert.Equal(new IndexAttribute { Order = 7 }.GetHashCode(), new IndexAttribute { Order = 7 }.GetHashCode());
            Assert.Equal(new IndexAttribute { IsClustered = true }.GetHashCode(), new IndexAttribute { IsClustered = true }.GetHashCode());
            Assert.Equal(new IndexAttribute { IsClustered = false }.GetHashCode(), new IndexAttribute { IsClustered = false }.GetHashCode());
            Assert.Equal(new IndexAttribute { IsUnique = true }.GetHashCode(), new IndexAttribute { IsUnique = true }.GetHashCode());
            Assert.Equal(new IndexAttribute { IsUnique = false }.GetHashCode(), new IndexAttribute { IsUnique = false }.GetHashCode());

            Assert.Equal(
                new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }.GetHashCode(),
                new IndexAttribute("MrsPandy", 7) { IsUnique = true, IsClustered = false }.GetHashCode());
        }
    }
}
