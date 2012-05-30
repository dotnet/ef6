namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Resources;
    using Xunit;

    /// <summary>
    /// Unit tests for Database.
    /// </summary>
    public class DatabaseTests : TestBase
    {
        #region Negative constructor tests

        [Fact]
        public void Calling_Database_DatabaseExists_with_null_nameOrConnectionString_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => Database.Exists((string)null)).Message);
        }

        [Fact]
        public void Calling_Database_DatabaseExists_with_empty_nameOrConnectionString_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => Database.Exists("")).Message);
        }

        [Fact]
        public void Calling_Database_DatabaseExists_with_whitespace_nameOrConnectionString_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => Database.Exists(" ")).Message);
        }

        [Fact]
        public void Calling_Database_DeleteDatabaseIfExists_with_null_nameOrConnectionString_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => Database.Delete((string)null)).Message);
        }

        [Fact]
        public void Calling_Database_DeleteDatabaseIfExists_with_empty_nameOrConnectionString_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => Database.Delete("")).Message);
        }

        [Fact]
        public void Calling_Database_DeleteDatabaseIfExists_with_whitespace_nameOrConnectionString_throws()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("nameOrConnectionString"), Assert.Throws<ArgumentException>(() => Database.Delete(" ")).Message);
        }

        [Fact]
        public void Calling_Database_DatabaseExists_with_null_existingConnection_throws()
        {
            Assert.Equal("existingConnection", Assert.Throws<ArgumentNullException>(() => Database.Exists((DbConnection)null)).ParamName);
        }

        [Fact]
        public void Calling_Database_DeleteDatabaseIfExists_with_null_existingConnection_throws()
        {
            Assert.Equal("existingConnection", Assert.Throws<ArgumentNullException>(() => Database.Delete((DbConnection)null)).ParamName);
        }

        #endregion
    }
}
