// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace UnitTests
{
    using System.Data.Entity;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
    using System.Linq;
    using System.Security;
    using Xunit;

    /// <summary>
    /// The EF assemblies are designed to be bin-deployed. If the assembly is GAC'ed, then
    /// it is only callable from other full trust assemblies.
    /// </summary>
    public class TargetAssemblyTests
    {
        [Fact]
        public void EntityFramework_assembly_has_no_security_attributes()
        {
            Assert.False(typeof(DbContext).Assembly.GetCustomAttributes(true).OfType<SecurityTransparentAttribute>().Any());
            Assert.False(typeof(DbContext).Assembly.GetCustomAttributes(true).OfType<SecurityCriticalAttribute>().Any());
            Assert.False(typeof(DbContext).Assembly.GetCustomAttributes(true).OfType<AllowPartiallyTrustedCallersAttribute>().Any());
            Assert.False(typeof(DbContext).Assembly.GetCustomAttributes(true).OfType<SecurityRulesAttribute>().Any());
        }

        [Fact]
        public void EntityFramework_SqlServer_assembly_has_no_security_attributes()
        {
            Assert.False(typeof(SqlProviderServices).Assembly.GetCustomAttributes(true).OfType<SecurityTransparentAttribute>().Any());
            Assert.False(typeof(SqlProviderServices).Assembly.GetCustomAttributes(true).OfType<SecurityCriticalAttribute>().Any());
            Assert.False(typeof(SqlProviderServices).Assembly.GetCustomAttributes(true).OfType<AllowPartiallyTrustedCallersAttribute>().Any());
            Assert.False(typeof(SqlProviderServices).Assembly.GetCustomAttributes(true).OfType<SecurityRulesAttribute>().Any());
        }

        [Fact]
        public void EntityFramework_SqlCompact_assembly_has_no_security_attributes()
        {
            Assert.False(typeof(SqlCeProviderServices).Assembly.GetCustomAttributes(true).OfType<SecurityTransparentAttribute>().Any());
            Assert.False(typeof(SqlCeProviderServices).Assembly.GetCustomAttributes(true).OfType<SecurityCriticalAttribute>().Any());
            Assert.False(typeof(SqlCeProviderServices).Assembly.GetCustomAttributes(true).OfType<AllowPartiallyTrustedCallersAttribute>().Any());
            Assert.False(typeof(SqlCeProviderServices).Assembly.GetCustomAttributes(true).OfType<SecurityRulesAttribute>().Any());
        }
    }
}
