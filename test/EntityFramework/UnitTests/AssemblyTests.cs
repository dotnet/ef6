// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.SqlServerCompact;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using System.Security;
    using Xunit;

    public class AssemblyTests : TestBase
    {
        public static Assembly EntityFrameworkSqlServerCompactAssembly
        {
            get { return typeof(SqlCeProviderServices).Assembly; }
        }

        [Fact]
        public void EntityFramework_assembly_is_CLSCompliant()
        {
            Assert.True(typeof(DbModelBuilder).Assembly.GetCustomAttributes<CLSCompliantAttribute>().Single().IsCompliant);
        }

        [Fact]
        public void MaxLengthAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.MaxLengthAttribute"));
        }

        [Fact]
        public void MinLengthAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.MinLengthAttribute"));
        }

        [Fact]
        public void ColumnAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"));
        }

        [Fact]
        public void ComplexTypeAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.ComplexTypeAttribute"));
        }

        [Fact]
        public void DatabaseGeneratedAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute"));
        }

        [Fact]
        public void DatabaseGeneratedOption_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption"));
        }

        [Fact]
        public void ForeignKeyAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute"));
        }

        [Fact]
        public void InversePropertyAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.InversePropertyAttribute"));
        }

        [Fact]
        public void NotMappedAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(
                EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute"));
        }

        [Fact]
        public void TableAttribute_is_type_forwarded_on_net45_but_not_on_net40()
        {
            AssertTypeIsInExpectedAssembly(EntityFrameworkAssembly.GetType("System.ComponentModel.DataAnnotations.Schema.TableAttribute"));
        }

        private void AssertTypeIsInExpectedAssembly(Type type)
        {
#if NET40
            Assert.Same(EntityFrameworkAssembly, type.Assembly);
#else
            Assert.Same(SystemComponentModelDataAnnotationsAssembly, type.Assembly);
#endif
        }

        [Fact]
        public void EntityFramework_assembly_has_no_security_attributes()
        {
            Assert.False(EntityFrameworkAssembly.GetCustomAttributes<SecurityTransparentAttribute>().Any());
            Assert.False(EntityFrameworkAssembly.GetCustomAttributes<SecurityCriticalAttribute>().Any());
            Assert.False(EntityFrameworkAssembly.GetCustomAttributes<AllowPartiallyTrustedCallersAttribute>().Any());
            Assert.False(EntityFrameworkAssembly.GetCustomAttributes<SecurityRulesAttribute>().Any());
        }

        [Fact]
        public void EntityFramework_SqlServer_assembly_has_no_security_attributes()
        {
            Assert.False(EntityFrameworkSqlServerAssembly.GetCustomAttributes<SecurityTransparentAttribute>().Any());
            Assert.False(EntityFrameworkSqlServerAssembly.GetCustomAttributes<SecurityCriticalAttribute>().Any());
            Assert.False(EntityFrameworkSqlServerAssembly.GetCustomAttributes<AllowPartiallyTrustedCallersAttribute>().Any());
            Assert.False(EntityFrameworkSqlServerAssembly.GetCustomAttributes<SecurityRulesAttribute>().Any());
        }

        [Fact]
        public void EntityFramework_SqlCompact_assembly_has_no_security_attributes()
        {
            Assert.False(EntityFrameworkSqlServerCompactAssembly.GetCustomAttributes<SecurityTransparentAttribute>().Any());
            Assert.False(EntityFrameworkSqlServerCompactAssembly.GetCustomAttributes<SecurityCriticalAttribute>().Any());
            Assert.False(EntityFrameworkSqlServerCompactAssembly.GetCustomAttributes<AllowPartiallyTrustedCallersAttribute>().Any());
            Assert.False(EntityFrameworkSqlServerCompactAssembly.GetCustomAttributes<SecurityRulesAttribute>().Any());
        }
    }
}
