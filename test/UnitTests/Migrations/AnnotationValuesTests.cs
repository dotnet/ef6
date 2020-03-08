// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Infrastructure.Annotations;
    using Xunit;

    public class AnnotationValuesTests
    {
        [Fact]
        public void Properties_return_expected_values()
        {
            var pair = new AnnotationValues("old", "new");

            Assert.Equal("old", pair.OldValue);
            Assert.Equal("new", pair.NewValue);
        }

        [Fact]
        public void Pairs_with_same_values_are_equal()
        {
            Assert.Equal(new AnnotationValues("old", "new"), new AnnotationValues("old", "new"));
            Assert.Equal(new AnnotationValues("old", null), new AnnotationValues("old", null));
            Assert.Equal(new AnnotationValues(null, "new"), new AnnotationValues(null, "new"));
            Assert.Equal(new AnnotationValues(null, null), new AnnotationValues(null, null));

            Assert.True(new AnnotationValues("old", "new") == new AnnotationValues("old", "new"));
            Assert.True(new AnnotationValues("old", null) == new AnnotationValues("old", null));
            Assert.True(new AnnotationValues(null, "new") == new AnnotationValues(null, "new"));
            Assert.True(new AnnotationValues(null, null) == new AnnotationValues(null, null));
        }

        [Fact]
        public void Pairs_with_same_values_have_same_hashcode()
        {
            Assert.Equal(new AnnotationValues("old", "new").GetHashCode(), new AnnotationValues("old", "new").GetHashCode());
            Assert.Equal(new AnnotationValues("old", null).GetHashCode(), new AnnotationValues("old", null).GetHashCode());
            Assert.Equal(new AnnotationValues(null, "new").GetHashCode(), new AnnotationValues(null, "new").GetHashCode());
            Assert.Equal(new AnnotationValues(null, null).GetHashCode(), new AnnotationValues(null, null).GetHashCode());
        }

        [Fact]
        public void Pairs_with_different_values_are_not_equal()
        {
            Assert.NotEqual(new AnnotationValues("old", "new"), new AnnotationValues("old", "chew"));
            Assert.NotEqual(new AnnotationValues("old", "new"), new AnnotationValues("cold", "new"));
            Assert.NotEqual(new AnnotationValues("old", "new"), null);
            Assert.NotEqual(null, new AnnotationValues("old", "new"));
            Assert.NotEqual(new AnnotationValues("old", "new"), (object)new Random());
            Assert.NotEqual((object)new Random(), new AnnotationValues("old", "new"));

            Assert.True(new AnnotationValues("old", "new") != new AnnotationValues("old", "chew"));
            Assert.True(new AnnotationValues("old", "new") != new AnnotationValues("cold", "new"));
            Assert.True(new AnnotationValues("old", "new") != null);
            Assert.True(null != new AnnotationValues("old", "new"));
        }
    }
}
