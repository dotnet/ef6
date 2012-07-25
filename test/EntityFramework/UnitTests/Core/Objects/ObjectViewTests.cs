// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Objects
{
    using System;
    using System.Collections;
    using Xunit;

    public class ObjectViewTests
    {
        [Fact]
        public void Add_throws_for_null_argument()
        {
            var mockEntityCollection = MockHelper.CreateMockEntityCollection<object>(null).Object;
            var objectView = new ObjectView<object>(new ObjectViewEntityCollectionData<object, object>(mockEntityCollection), mockEntityCollection);
            Assert.Equal("value",
                Assert.Throws<ArgumentNullException>(
                    () => ((IList)objectView).Add(null)).ParamName);
        }

        [Fact]
        public void Remove_throws_for_null_argument()
        {
            var mockEntityCollection = MockHelper.CreateMockEntityCollection<object>(null).Object;
            var objectView = new ObjectView<object>(new ObjectViewEntityCollectionData<object, object>(mockEntityCollection), mockEntityCollection);
            Assert.Equal("value",
                Assert.Throws<ArgumentNullException>(
                    () => ((IList)objectView).Remove(null)).ParamName);
        }
    }
}
