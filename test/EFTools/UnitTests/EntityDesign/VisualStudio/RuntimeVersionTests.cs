// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Xunit;

    public class RuntimeVersionTests
    {
        [Fact]
        public void Latest_returns_version()
        {
            Assert.NotNull(RuntimeVersion.Latest);
        }

        [Fact]
        public void Version5_returns_net40()
        {
            var entityFrameworkVersion =
                RuntimeVersion.Version5(targetNetFrameworkVersion: NetFrameworkVersioningHelper.NetFrameworkVersion4);

            Assert.Equal(RuntimeVersion.Version5Net40, entityFrameworkVersion);
        }

        [Fact]
        public void Version5_returns_net45()
        {
            var entityFrameworkVersion =
                RuntimeVersion.Version5(targetNetFrameworkVersion: NetFrameworkVersioningHelper.NetFrameworkVersion4_5);

            Assert.Equal(RuntimeVersion.Version5Net45, entityFrameworkVersion);
        }

        [Fact]
        public void GetName_returns_formatted_name()
        {
            var entityFrameworkVersion = new Version(1, 2, 3, 4);

            var entityFrameworkVersionName = RuntimeVersion.GetName(entityFrameworkVersion, null);

            Assert.Equal(
                string.Format(
                    Resources.EntityFrameworkVersionName,
                    new Version(entityFrameworkVersion.Major, entityFrameworkVersion.Minor)),
                entityFrameworkVersionName);
        }

        [Fact]
        public void GetName_fixes_up_net40_ef5()
        {
            var entityFrameworkVersion = RuntimeVersion.Version5Net40;

            var entityFrameworkVersionName = RuntimeVersion.GetName(entityFrameworkVersion, null);

            Assert.Equal(
                string.Format(Resources.EntityFrameworkVersionName, new Version(5, 0)),
                entityFrameworkVersionName);
        }

        [Fact]
        public void GetName_fixes_up_SDE_only_ef5()
        {
            var netFrameworkVersions =
                new[]
                    {
                        new Version(4, 5), // .NET Framework 4.5 
                        new Version(4, 5, 1), // .NET Framework 4.5.1
                        new Version(42, 0, 0, 0) // a future version of .NET Framework
                    };

            var entityFrameworkVersion = new Version(4, 0, 0, 0);

            foreach (var targetNetFrameworkVersion in netFrameworkVersions)
            {
                var entityFrameworkVersionName =
                    RuntimeVersion.GetName(entityFrameworkVersion, targetNetFrameworkVersion);

                Assert.Equal(
                    string.Format(Resources.EntityFrameworkVersionName, new Version(5, 0)),
                    entityFrameworkVersionName);
            }
        }

        [Fact]
        public void RequiresLegacyProvider_returns_true_when_under_six()
        {
            var entityFrameworkVersion = new Version(4, 0, 0, 0);

            var legacyProviderRequired = RuntimeVersion.RequiresLegacyProvider(entityFrameworkVersion);

            Assert.True(legacyProviderRequired);
        }

        [Fact]
        public void RequiresLegacyProvider_returns_false_when_six()
        {
            var entityFrameworkVersion = RuntimeVersion.Version6;

            var legacyProviderRequired = RuntimeVersion.RequiresLegacyProvider(entityFrameworkVersion);

            Assert.False(legacyProviderRequired);
        }

        [Fact]
        public void RequiresLegacyProvider_returns_false_when_over_six()
        {
            var entityFrameworkVersion = new Version(7, 0, 0, 0);

            var legacyProviderRequired = RuntimeVersion.RequiresLegacyProvider(entityFrameworkVersion);

            Assert.False(legacyProviderRequired);
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_three_when_null_on_NetFramework_3_5()
        {
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion3_5;
            var targetSchemaVersion =
                RuntimeVersion.GetTargetSchemaVersion(null, targetNetFrameworkVersion);

            Assert.Equal(EntityFrameworkVersion.Version1, targetSchemaVersion);
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_three_when_null_on_NetFramework_4_or_newer()
        {
            var netFrameworkVersions =
                new[]
                    {
                        new Version(4, 0, 0, 0),
                        new Version(4, 5, 0, 0),
                        new Version(4, 5, 1, 0),
                        new Version(42, 0, 0, 0)
                    };

            foreach (var targetNetFrameworkVersion in netFrameworkVersions)
            {
                var targetSchemaVersion =
                    RuntimeVersion.GetTargetSchemaVersion(null, targetNetFrameworkVersion);

                Assert.Equal(EntityFrameworkVersion.Version3, targetSchemaVersion);
            }
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_three_when_null_on_NetFramework_4_5()
        {
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4_5;
            var targetSchemaVersion =
                RuntimeVersion.GetTargetSchemaVersion(null, targetNetFrameworkVersion);

            Assert.Equal(EntityFrameworkVersion.Version3, targetSchemaVersion);
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_three_when_five()
        {
            var entityFrameworkVersion = RuntimeVersion.Version5Net45;
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4_5;
            var targetSchemaVersion =
                RuntimeVersion.GetTargetSchemaVersion(entityFrameworkVersion, targetNetFrameworkVersion);

            Assert.Equal(EntityFrameworkVersion.Version3, targetSchemaVersion);
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_three_when_over_five()
        {
            var entityFrameworkVersion = RuntimeVersion.Version6;
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4;

            var targetSchemaVersion =
                RuntimeVersion.GetTargetSchemaVersion(entityFrameworkVersion, targetNetFrameworkVersion);

            Assert.Equal(EntityFrameworkVersion.Version3, targetSchemaVersion);
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_two_when_four_on_NetFramework_4()
        {
            var entityFrameworkVersion = new Version(4, 0, 0, 0);
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4;

            var targetSchemaVersion =
                RuntimeVersion.GetTargetSchemaVersion(entityFrameworkVersion, targetNetFrameworkVersion);

            Assert.Equal(EntityFrameworkVersion.Version2, targetSchemaVersion);
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_three_when_four_on_NetFramework_4_5()
        {
            var entityFrameworkVersion = new Version(4, 0, 0, 0);
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion4_5;

            var targetSchemaVersion =
                RuntimeVersion.GetTargetSchemaVersion(entityFrameworkVersion, targetNetFrameworkVersion);

            Assert.Equal(EntityFrameworkVersion.Version3, targetSchemaVersion);
        }

        [Fact]
        public void GetTargetSchemaVersion_returns_one_when_less_than_four()
        {
            var entityFrameworkVersion = RuntimeVersion.Version1;
            var targetNetFrameworkVersion = NetFrameworkVersioningHelper.NetFrameworkVersion3_5;

            var targetSchemaVersion =
                RuntimeVersion.GetTargetSchemaVersion(entityFrameworkVersion, targetNetFrameworkVersion);

            Assert.Equal(EntityFrameworkVersion.Version1, targetSchemaVersion);
        }

        [Fact]
        public void SchemaVersionLatestForAssemblyVersion_returns_correct_values_for_v1()
        {
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(3, 5, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion3_5));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(4, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(4, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(4, 1, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(4, 4, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(5, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(6, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version1, new Version(6, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
        }

        [Fact]
        public void SchemaVersionLatestForAssemblyVersion_returns_correct_values_for_v2()
        {
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(3, 5, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion3_5));
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(4, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(4, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(4, 1, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(4, 4, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(5, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(6, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version2, new Version(6, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
        }

        [Fact]
        public void SchemaVersionLatestForAssemblyVersion_returns_correct_values_for_v3()
        {
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(3, 5, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion3_5));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(4, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(4, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(4, 1, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.False(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(4, 4, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(5, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(6, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4));
            Assert.True(
                RuntimeVersion.IsSchemaVersionLatestForAssemblyVersion(
                    EntityFrameworkVersion.Version3, new Version(6, 0, 0, 0), NetFrameworkVersioningHelper.NetFrameworkVersion4_5));
        }

        [Fact]
        public void GetSchemaVersionForNetFrameworkVersion_returns_correct_schema_version_for_NetFramework_version()
        {
            Assert.Equal(
                EntityFrameworkVersion.Version3,
                RuntimeVersion.GetSchemaVersionForNetFrameworkVersion(new Version(4, 5, 1)));

            Assert.Equal(
                EntityFrameworkVersion.Version3,
                RuntimeVersion.GetSchemaVersionForNetFrameworkVersion(new Version(4, 5)));

            Assert.Equal(
                EntityFrameworkVersion.Version2,
                RuntimeVersion.GetSchemaVersionForNetFrameworkVersion(new Version(4, 0)));

            Assert.Equal(
                EntityFrameworkVersion.Version1,
                RuntimeVersion.GetSchemaVersionForNetFrameworkVersion(new Version(3, 5)));
        }
    }
}
