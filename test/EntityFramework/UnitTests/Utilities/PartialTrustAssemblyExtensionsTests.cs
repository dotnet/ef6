// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Utilities
{
    using System.Linq;
    using System.Reflection;
    using Xunit;

    [PartialTrustFixture]
    public class PartialTrustAssemblyExtensionsTests : TestBase
    {
        [Fact]
        public void GetAccessibleTypes_returns_all_types_in_an_assembly_that_can_be_loaded_in_partial_trust()
        {
            // Verify that GetTypes causes a loader exception
            Assert.Throws<ReflectionTypeLoadException>(() => typeof(PartialTrustAssemblyExtensionsTests).Assembly.GetTypes());

            // Verify that GetAccessibleTypes handles this
            var types = typeof(PartialTrustAssemblyExtensionsTests).Assembly.GetAccessibleTypes();
            Assert.Contains(typeof(AssemblyExtensionsTests), types);
            Assert.False(types.Any(t => t == null));
        }
    }
}