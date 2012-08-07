// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.Utilities
{
    using Xunit;

    public class FuncExtensionsTests
    {
        [Fact]
        public void NullIfNotImplemented_translates_NotImplementedException_to_null_for_reference_type()
        {
            Assert.Null(
                FuncExtensions.NullIfNotImplemented<string>(
                    () => { throw new NotImplementedException(); }));
        }

        [Fact]
        public void NullIfNotImplemented_translates_NotImplementedException_to_null_for_nullable_type()
        {
            Assert.Null(
                FuncExtensions.NullIfNotImplemented<int?>(
                    () => { throw new NotImplementedException(); }));
        }
    }
}
