// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using UnitTests.TestHelpers;
    using Xunit;

    public class NetFrameworkVersioningHelperTests
    {
        [Fact]
        public void TargetNetFrameworkVersion_returns_targeted_version_from_valid_net_framework_moniker()
        {
            var mockMonikerHelper = new MockDTE(".NETFramework,Version=v4.0,Profile=Client");

            Assert.Equal(
                NetFrameworkVersioningHelper.NetFrameworkVersion4,
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(
                    mockMonikerHelper.Project, mockMonikerHelper.ServiceProvider));
        }

        [Fact]
        public void TargetNetFrameworkVersion_returns_null_for_null_target_net_framework_moniker()
        {
            var mockMonikerHelper = new MockDTE(null);

            Assert.Null(
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(
                    mockMonikerHelper.Project, mockMonikerHelper.ServiceProvider));
        }

        [Fact]
        public void TargetNetFrameworkVersion_returns_null_for_empty_target_net_framework_moniker()
        {
            var mockMonikerHelper = new MockDTE(string.Empty);

            Assert.Null(
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(
                    mockMonikerHelper.Project, mockMonikerHelper.ServiceProvider));
        }

        [Fact]
        public void TargetNetFrameworkVersion_returns_null_for_non_string_target_net_framework_moniker()
        {
            var mockMonikerHelper = new MockDTE(new object());

            Assert.Null(
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(
                    mockMonikerHelper.Project, mockMonikerHelper.ServiceProvider));
        }

        [Fact]
        public void TargetNetFrameworkVersion_returns_null_for_invalid_target_framework_moniker()
        {
            var mockMonikerHelper = new MockDTE("abc");

            Assert.Null(
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(
                    mockMonikerHelper.Project, mockMonikerHelper.ServiceProvider));
        }

        [Fact]
        public void TargetNetFrameworkVersion_returns_null_for_misc_project()
        {
            const string vsMiscFilesProjectUniqueName = "<MiscFiles>";

            var mockMonikerHelper = new MockDTE("abc", vsMiscFilesProjectUniqueName);

            Assert.Null(
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(
                    mockMonikerHelper.Project, mockMonikerHelper.ServiceProvider));
        }

        [Fact]
        public void TargetNetFrameworkVersion_returns_null_for_non_NetFramework_project()
        {
            var mockMonikerHelper = new MockDTE("Xbox,Version=v4.0");

            Assert.Null(
                NetFrameworkVersioningHelper.TargetNetFrameworkVersion(
                    mockMonikerHelper.Project, mockMonikerHelper.ServiceProvider));
        }
    }
}
