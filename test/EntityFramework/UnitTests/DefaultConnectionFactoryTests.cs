// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;
    using System.Reflection;
    using Xunit;

    /// <summary>
    ///     Tests for Database.DefaultConnectionFactory and related infrastructure classes.
    /// </summary>
    public class DefaultConnectionFactoryTests : TestBase
    {
        #region DefaultConnectionFactory positive tests

        [Fact]
        public void DefaultConnectionFactory_is_SqlServerConnectionFactory()
        {
#pragma warning disable 612,618
            Assert.IsType<SqlConnectionFactory>(Database.DefaultConnectionFactory);
#pragma warning restore 612,618
            Assert.IsType<SqlConnectionFactory>(DbConfiguration.GetService<IDbConnectionFactory>());
        }

        private class FakeConnectionFactory : IDbConnectionFactory
        {
            public DbConnection CreateConnection(string nameOrConnectionString)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Setting_DefaultConnectionFactory_after_configuration_override_is_in_place_has_no_effect()
        {
            try
            {
#pragma warning disable 612,618
                // This call will have no effect because the functional tests are setup with a DbConfiguration
                // that explicitly overrides this using an OnLockingConfiguration handler.
                Database.DefaultConnectionFactory = new FakeConnectionFactory();

                Assert.IsType<SqlConnectionFactory>(Database.DefaultConnectionFactory);
#pragma warning restore 612,618
            }
            finally
            {
                typeof(Database).GetMethod("ResetDefaultConnectionFactory", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                Database.ResetDefaultConnectionFactory();
            }
        }

        #endregion

        #region DefaultConnectionFactory negative tests

        [Fact]
        public void DefaultConnectionFactory_throws_when_set_to_null()
        {
#pragma warning disable 612,618
            Assert.Equal("value", Assert.Throws<ArgumentNullException>(() => Database.DefaultConnectionFactory = null).ParamName);
#pragma warning restore 612,618
        }

        #endregion
    }
}
