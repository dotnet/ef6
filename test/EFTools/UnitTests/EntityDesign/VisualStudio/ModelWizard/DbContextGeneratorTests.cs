// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using UnitTests.TestHelpers;
    using Xunit;

    public class DbContextGeneratorTests
    {
        [Fact]
        public void TemplateSupported_returns_true_when_targeting_NetFramework4_or_newer()
        {
            var targets =
                new[]
                    {
                        ".NETFramework,Version=v4.0",
                        ".NETFramework,Version=v4.5",
                        ".NETFramework,Version=v4.5.1"
                    };

            foreach (var target in targets)
            {
                var mockMonikerHelper = new MockDTE(target);

                Assert.True(
                    DbContextCodeGenerator.TemplateSupported(
                        mockMonikerHelper.Project,
                        mockMonikerHelper.ServiceProvider));
            }
        }

        [Fact]
        public void TemplateSupported_returns_false_for_NetFramework3_5()
        {
            var mockMonikerHelper =
                new MockDTE(".NETFramework,Version=v3.5");

            Assert.False(
                DbContextCodeGenerator.TemplateSupported(
                    mockMonikerHelper.Project,
                    mockMonikerHelper.ServiceProvider));
        }

        [Fact]
        public void TemplateSupported_returns_false_when_for_non_NetFramework_projects()
        {
            var targets =
                new[]
                    {
                        ".NETFramework,Version=v1.1",
                        "XBox,Version=v4.0",
                        "abc",
                        string.Empty,
                        null
                    };

            foreach (var target in targets)
            {
                var mockMonikerHelper = new MockDTE(target);

                Assert.False(
                    DbContextCodeGenerator.TemplateSupported(
                        mockMonikerHelper.Project,
                        mockMonikerHelper.ServiceProvider));
            }
        }

        [Fact]
        public void TemplateSupported_returns_false_for_Misc_project()
        {
            const string vsMiscFilesProjectUniqueName = "<MiscFiles>";

            var mockMonikerHelper =
                new MockDTE(".NETFramework,Version=v4.0", vsMiscFilesProjectUniqueName);

            Assert.False(
                DbContextCodeGenerator.TemplateSupported(
                    mockMonikerHelper.Project,
                    mockMonikerHelper.ServiceProvider));
        }
    }
}
