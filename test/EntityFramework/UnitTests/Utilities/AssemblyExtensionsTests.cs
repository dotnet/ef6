// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

[assembly: System.Data.Entity.Utilities.AssemblyExtensionsTests.Laundering]

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class AssemblyExtensionsTests
    {
        public class GetInformationalVersion : TestBase
        {
            [Fact]
            public void GetInformationalVersion_returns_the_informational_version()
            {
                Assert.True(typeof(DbMigrator).Assembly().GetInformationalVersion().StartsWith("6.1.0-alpha1"));
            }
        }

        [Fact]
        public void Can_return_attributes_from_assembly()
        {
            Assert.IsType<LaunderingAttribute>(GetType().Assembly().GetCustomAttributes<LaunderingAttribute>().Single());

            Assert.Contains(
                "LaunderingAttribute",
                GetType().Assembly().GetCustomAttributes<Attribute>().Select(a => a.GetType().Name));
        }


        [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
        public class LaunderingAttribute : Attribute
        {
        }
    }
}
