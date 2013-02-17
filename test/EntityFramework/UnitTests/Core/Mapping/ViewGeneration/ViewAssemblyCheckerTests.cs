// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.ViewGeneration;
    using Xunit;

    public class ViewAssemblyCheckerTests : TestBase
    {
        [Fact]
        public void IsViewAssembly_returns_true_for_assembly_with_view_generation_attribute()
        {
            Assert.True(new ViewAssemblyChecker().IsViewAssembly(typeof(PregenContextEdmxViews).Assembly));
        }

        [Fact]
        public void IsViewAssembly_returns_false_for_assembly_without_view_generation_attribute()
        {
            Assert.False(new ViewAssemblyChecker().IsViewAssembly(typeof(RequiredAttribute).Assembly));
        }
    }
}
