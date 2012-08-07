// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbConnectionInfoTests : TestBase
    {
        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("connectionName"),
                Assert.Throws<ArgumentException>(() => new DbConnectionInfo(null)).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("connectionName"),
                Assert.Throws<ArgumentException>(() => new DbConnectionInfo("")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("connectionString"),
                Assert.Throws<ArgumentException>(() => new DbConnectionInfo(null, "invariant")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("connectionString"),
                Assert.Throws<ArgumentException>(() => new DbConnectionInfo("", "invariant")).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new DbConnectionInfo("connection", null)).Message);
            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("providerInvariantName"),
                Assert.Throws<ArgumentException>(() => new DbConnectionInfo("connection", "")).Message);
        }

        [Fact]
        public void Can_find_named_connection_in_config()
        {
            var config = new AppConfig(CreateEmptyConfig().AddConnectionString("FindMe", "connection_string", "provider_invariant_name"));
            var info = new DbConnectionInfo("FindMe");
            var connection = info.GetConnectionString(config);

            Assert.Equal("FindMe", connection.Name);
            Assert.Equal("connection_string", connection.ConnectionString);
            Assert.Equal("provider_invariant_name", connection.ProviderName);
        }

        [Fact]
        public void GetConnectionString_throws_when_cant_find_named_connection_in_config()
        {
            var config = new AppConfig(CreateEmptyConfig());
            var info = new DbConnectionInfo("FindMe");
            Assert.Equal(
                Strings.DbConnectionInfo_ConnectionStringNotFound("FindMe"),
                Assert.Throws<InvalidOperationException>(() => info.GetConnectionString(config)).Message);
        }

        [Fact]
        public void Returns_valid_connection_from_string_and_provider()
        {
            var config = new AppConfig(CreateEmptyConfig());
            var info = new DbConnectionInfo("connection_string", "provider_invariant_name");
            var connection = info.GetConnectionString(config);

            Assert.Null(connection.Name);
            Assert.Equal("connection_string", connection.ConnectionString);
            Assert.Equal("provider_invariant_name", connection.ProviderName);
        }
    }
}
