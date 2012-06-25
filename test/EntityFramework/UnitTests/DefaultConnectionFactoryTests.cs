namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using Xunit;

    /// <summary>
    /// Tests for Database.DefaultConnectionFactory and related infrastructure classes.
    /// </summary>
    public class DefaultConnectionFactoryTests : TestBase
    {
        #region DefaultConnectionFactory positive tests

        [Fact]
        public void DefaultConnectionFactory_is_SqlServerConnectionFactory()
        {
            Assert.NotNull(Database.DefaultConnectionFactory);
            Assert.IsType<SqlConnectionFactory>(Database.DefaultConnectionFactory);
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
                Database.DefaultConnectionFactory = new FakeConnectionFactory();

                Assert.NotNull(Database.DefaultConnectionFactory);
                Assert.IsType<FakeConnectionFactory>(Database.DefaultConnectionFactory);
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
            Assert.Equal("value", Assert.Throws<ArgumentNullException>(() => Database.DefaultConnectionFactory = null).ParamName);
        }

        #endregion
    }
}
