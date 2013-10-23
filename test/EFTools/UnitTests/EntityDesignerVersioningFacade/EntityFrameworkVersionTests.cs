// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Linq;
    using Xunit;

    public class EntityFrameworkVersionTests
    {
        [Fact]
        public void GetAllVersions_returns_all_declared_versions()
        {
            var declaredVersions =
                typeof(EntityFrameworkVersion)
                    .GetFields()
                    .Where(f => f.FieldType == typeof(Version))
                    .Select(f => f.GetValue(null))
                    .OrderByDescending(v => v);

            Assert.True(declaredVersions.SequenceEqual(EntityFrameworkVersion.GetAllVersions()));
        }

        [Fact]
        public void IsValidVersion_returns_true_for_valid_versions()
        {
            Assert.True(EntityFrameworkVersion.IsValidVersion(new Version(1, 0, 0, 0)));
            Assert.True(EntityFrameworkVersion.IsValidVersion(new Version(2, 0, 0, 0)));
            Assert.True(EntityFrameworkVersion.IsValidVersion(new Version(3, 0, 0, 0)));
        }

        [Fact]
        public void IsValidVersion_returns_false_for_invalid_versions()
        {
            Assert.False(EntityFrameworkVersion.IsValidVersion(null));
            Assert.False(EntityFrameworkVersion.IsValidVersion(new Version(0, 0, 0, 0)));
            Assert.False(EntityFrameworkVersion.IsValidVersion(new Version(4, 0, 0, 0)));
            Assert.False(EntityFrameworkVersion.IsValidVersion(new Version(3, 0)));
            Assert.False(EntityFrameworkVersion.IsValidVersion(new Version(2, 0, 0)));
        }

        [Fact]
        public void DoubleToVersion_returns_valid_version_for_known_double_versions()
        {
            Assert.Equal(EntityFrameworkVersion.Version3, EntityFrameworkVersion.DoubleToVersion(3.0));
            Assert.Equal(EntityFrameworkVersion.Version2, EntityFrameworkVersion.DoubleToVersion(2.0));
            Assert.Equal(EntityFrameworkVersion.Version1, EntityFrameworkVersion.DoubleToVersion(1.0));
        }

        [Fact]
        public void VersionToDouble_returns_valid_version_for_known_double_versions()
        {
            Assert.Equal(3.0, EntityFrameworkVersion.VersionToDouble(EntityFrameworkVersion.Version3));
            Assert.Equal(2.0, EntityFrameworkVersion.VersionToDouble(EntityFrameworkVersion.Version2));
            Assert.Equal(1.0, EntityFrameworkVersion.VersionToDouble(EntityFrameworkVersion.Version1));
        }

        [Fact]
        public void Latest_EF_version_is_really_latest()
        {
            Assert.Equal(
                EntityFrameworkVersion.GetAllVersions().OrderByDescending(v => v).First(),
                EntityFrameworkVersion.Latest);
        }
    }
}
