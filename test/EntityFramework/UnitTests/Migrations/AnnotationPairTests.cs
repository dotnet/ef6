// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using Xunit;

    public class AnnotationPairTests
    {
        [Fact]
        public void Properties_return_expected_values()
        {
            var pair = new AnnotationPair("old", "new");

            Assert.Equal("old", pair.OldValue);
            Assert.Equal("new", pair.NewValue);
        }

        [Fact]
        public void Pairs_with_same_values_are_equal()
        {
            Assert.Equal(new AnnotationPair("old", "new"), new AnnotationPair("old", "new"));
            Assert.Equal(new AnnotationPair("old", null), new AnnotationPair("old", null));
            Assert.Equal(new AnnotationPair(null, "new"), new AnnotationPair(null, "new"));
            Assert.Equal(new AnnotationPair(null, null), new AnnotationPair(null, null));

            Assert.True(new AnnotationPair("old", "new") == new AnnotationPair("old", "new"));
            Assert.True(new AnnotationPair("old", null) == new AnnotationPair("old", null));
            Assert.True(new AnnotationPair(null, "new") == new AnnotationPair(null, "new"));
            Assert.True(new AnnotationPair(null, null) == new AnnotationPair(null, null));
        }

        [Fact]
        public void Pairs_with_same_values_have_same_hashcode()
        {
            Assert.Equal(new AnnotationPair("old", "new").GetHashCode(), new AnnotationPair("old", "new").GetHashCode());
            Assert.Equal(new AnnotationPair("old", null).GetHashCode(), new AnnotationPair("old", null).GetHashCode());
            Assert.Equal(new AnnotationPair(null, "new").GetHashCode(), new AnnotationPair(null, "new").GetHashCode());
            Assert.Equal(new AnnotationPair(null, null).GetHashCode(), new AnnotationPair(null, null).GetHashCode());
        }

        [Fact]
        public void Pairs_with_different_values_are_not_equal()
        {
            Assert.NotEqual(new AnnotationPair("old", "new"), new AnnotationPair("old", "chew"));
            Assert.NotEqual(new AnnotationPair("old", "new"), new AnnotationPair("cold", "new"));
            Assert.NotEqual(new AnnotationPair("old", "new"), null);
            Assert.NotEqual(null, new AnnotationPair("old", "new"));
            Assert.NotEqual(new AnnotationPair("old", "new"), (object)new Random());
            Assert.NotEqual((object)new Random(), new AnnotationPair("old", "new"));

            Assert.True(new AnnotationPair("old", "new") != new AnnotationPair("old", "chew"));
            Assert.True(new AnnotationPair("old", "new") != new AnnotationPair("cold", "new"));
            Assert.True(new AnnotationPair("old", "new") != null);
            Assert.True(null != new AnnotationPair("old", "new"));
        }
    }
}
