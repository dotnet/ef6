// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using Xunit;

    public class MemberInfoExtensionsTests
    {
        [Fact]
        public void GetValue_returns_value_of_static_property()
        {
            Assert.Equal(
                "People Are People",
                typeof(FakeForGetValue).GetProperty("Property").GetValue());
        }

        [Fact]
        public void GetValue_returns_value_of_static_field()
        {
            Assert.Equal(
                "People Are People",
                typeof(FakeForGetValue).GetField("Field").GetValue());
        }

        public class FakeForGetValue
        {
            public static readonly string Field = "People Are People";

            public static string Property
            {
                get { return Field; }
            }
        }
    }
}
