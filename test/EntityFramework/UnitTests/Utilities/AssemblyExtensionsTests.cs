// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using Xunit;

    public class AssemblyExtensionsTests
    {
        [Fact]
        public void GetInformationalVersion_returns_the_informational_version()
        {
            Assert.True(typeof(DbMigrator).Assembly.GetInformationalVersion().StartsWith("6.0.0-alpha2"));
        }
    }
}
