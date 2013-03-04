// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using Xunit;

    public class ExpressionConverterTests
    {
        [Fact]
        public void DescribeClrType_returns_full_name_for_normal_types()
        {
            Assert.Equal("System.Data.Entity.Core.Objects.ELinq.ExpressionConverterTests", ExpressionConverter.DescribeClrType(GetType()));

            Assert.Equal(
                "System.Data.Entity.Core.Objects.ELinq.ExpressionConverterTests+ANest", 
                ExpressionConverter.DescribeClrType(typeof(ANest)));
        }

        private class ANest
        {
        }
    }
}
