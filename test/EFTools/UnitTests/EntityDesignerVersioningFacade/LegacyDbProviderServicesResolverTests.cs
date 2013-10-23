// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Data.Entity.Core.Common;
    using Microsoft.Data.Entity.Design.VersioningFacade.LegacyProviderWrapper;
    using Xunit;

    public class LegacyDbProviderServicesResolverTests
    {
        [Fact]
        public void LegacyDbProviderServicesResolver_creates_wrapper_for_legacy_providers()
        {
            Assert.IsType<LegacyDbProviderServicesWrapper>(
                new LegacyDbProviderServicesResolver().GetService(typeof(DbProviderServices), "System.Data.SqlClient"));
        }

        [Fact]
        public void DefaultDbProviderServicesResolver_returns_null_for_unknown_type()
        {
            Assert.Null(
                new LegacyDbProviderServicesResolver().GetService(typeof(Object), "System.Data.SqlClient"));
        }

        [Fact]
        public void DefaultDbProviderServicesResolver_returns_null_for_non_string_key()
        {
            Assert.Null(
                new LegacyDbProviderServicesResolver().GetService(typeof(DbProviderServices), new object()));
        }
    }
}
