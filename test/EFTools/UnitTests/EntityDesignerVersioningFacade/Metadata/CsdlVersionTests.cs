// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Metadata
{
    using System;
    using System.Linq;
    using Xunit;

    public class CsdlVersionTests
    {
        [Fact]
        public void GetAllVersions_returns_all_declared_versions()
        {
            var declaredVersions =
                typeof(CsdlVersion)
                    .GetFields()
                    .Where(f => f.FieldType == typeof(Version))
                    .Select(f => f.GetValue(null))
                    .OrderByDescending(v => v);

            Assert.True(
                declaredVersions.SequenceEqual(CsdlVersion.GetAllVersions().OrderByDescending(v => v)));
        }

        [Fact]
        public void IsValidVersion_returns_true_for_valid_versions()
        {
            Assert.True(CsdlVersion.IsValidVersion(new Version(1, 0, 0, 0)));
            Assert.True(CsdlVersion.IsValidVersion(new Version(1, 1, 0, 0)));
            Assert.True(CsdlVersion.IsValidVersion(new Version(2, 0, 0, 0)));
            Assert.True(CsdlVersion.IsValidVersion(new Version(3, 0, 0, 0)));
        }

        [Fact]
        public void IsValidVersion_returns_false_for_invalid_versions()
        {
            Assert.False(CsdlVersion.IsValidVersion(null));
            Assert.False(CsdlVersion.IsValidVersion(new Version(0, 0, 0, 0)));
            Assert.False(CsdlVersion.IsValidVersion(new Version(4, 0, 0, 0)));
            Assert.False(CsdlVersion.IsValidVersion(new Version(3, 0)));
            Assert.False(CsdlVersion.IsValidVersion(new Version(2, 0, 0)));
        }

        [Fact]
        public void CsdlVersion_was_added_if_EntityFramework_version_was_added()
        {
            // +1 to account for CSDL version 1.1
            Assert.Equal(
                EntityFrameworkVersion.GetAllVersions().Count() + 1,
                CsdlVersion.GetAllVersions().Count());
        }
    }
}
