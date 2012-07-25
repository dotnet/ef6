// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.ConfigFile
{
    using System;
    using System.Data.Entity;
    using Xunit;

    public class DatabaseInitializerElementTests : TestBase
    {
        [Fact]
        public void DatabaseInitializerElement_converts_valid_type()
        {
            var param = new DatabaseInitializerElement { InitializerTypeName = "System.Int32" };
            Assert.Equal(typeof(int), param.GetInitializerType());
        }

        [Fact]
        public void DatabaseInitializerElement_throws_converting_invalid_type()
        {
            var param = new DatabaseInitializerElement { InitializerTypeName = "Not.A.Type" };
            Assert.True(Assert.Throws<TypeLoadException>(() => param.GetInitializerType()).Message.Contains(" 'Not.A.Type' "));
        }
    }
}