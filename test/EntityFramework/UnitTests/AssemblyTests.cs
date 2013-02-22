// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServerCompact;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using Xunit;

    public class AssemblyTests : TestBase
    {
        private static readonly Assembly _entityFrameworkDll = typeof(DbContext).Assembly;
        private static readonly Assembly _dataAnnotationsDll = typeof(RequiredAttribute).Assembly;

        [Fact]
        public void EntityFramework_assembly_is_CLSCompliant()
        {
            var attr = typeof(DbModelBuilder).Assembly.GetCustomAttributes(true).OfType<CLSCompliantAttribute>().Single();
            Assert.True(attr.IsCompliant);
        }

        [Fact]
        public void MaxLengthAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(_entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.MaxLengthAttribute"));
        }

        [Fact]
        public void MinLengthAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(_entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.MinLengthAttribute"));
        }

        [Fact]
        public void ColumnAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(_entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"));
        }

        [Fact]
        public void ComplexTypeAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(_entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.ComplexTypeAttribute"));
        }

        [Fact]
        public void DatabaseGeneratedAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                _entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute"));
        }

        [Fact]
        public void DatabaseGeneratedOption_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                _entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption"));
        }

        [Fact]
        public void ForeignKeyAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(_entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute"));
        }

        [Fact]
        public void InversePropertyAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                _entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.InversePropertyAttribute"));
        }

        [Fact]
        public void NotMappedAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(_entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute"));
        }

        [Fact]
        public void TableAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(_entityFrameworkDll.GetType("System.ComponentModel.DataAnnotations.Schema.TableAttribute"));
        }

        private void AssertTypeIsInExpectedAssembly(Type type)
        {
#if NET40
            Assert.Same(_entityFrameworkDll, type.Assembly);
#else
            Assert.Same(_dataAnnotationsDll, type.Assembly);
#endif
        }

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
