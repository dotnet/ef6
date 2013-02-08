// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;

    public sealed class PropertyPathTests
    {
        [Fact]
        public void Equals_should_compare_sequences()
        {
            var propertyInfo = new MockPropertyInfo();

            Assert.Equal(new PropertyPath(propertyInfo.Object), new PropertyPath(propertyInfo.Object));
        }

        [Fact]
        public void Equals_operator_should_compare_sequences()
        {
            var propertyInfo = new MockPropertyInfo();

            Assert.True(new PropertyPath(propertyInfo.Object) == new PropertyPath(propertyInfo.Object));
        }

        [Fact]
        public void GetHashCode_should_use_sequence_hash_code()
        {
            var propertyInfo = new MockPropertyInfo();

            Assert.Equal(new PropertyPath(propertyInfo.Object).GetHashCode(), new PropertyPath(propertyInfo.Object).GetHashCode());
        }

        [Fact]
        public void PropertyPath_ToString_return_correct_value()
        {
            var propertyPath = new PropertyPath(
                new List<PropertyInfo>
                    {
                        new MockPropertyInfo(typeof(int), "P1"),
                        new MockPropertyInfo(typeof(int), "P2"),
                        new MockPropertyInfo(typeof(int), "P3")
                    });

            Assert.Equal("P1.P2.P3", propertyPath.ToString());
        }
    }
}
