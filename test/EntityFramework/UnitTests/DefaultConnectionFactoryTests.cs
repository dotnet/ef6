// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;
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
        public void DefaultConnectionFactory_can_be_changed()
        {
            try
            {
#pragma warning disable 612,618
                Database.DefaultConnectionFactory = new FakeConnectionFactory();

                Assert.IsType<FakeConnectionFactory>(Database.DefaultConnectionFactory);
#pragma warning restore 612,618
            }
            finally
            {
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
