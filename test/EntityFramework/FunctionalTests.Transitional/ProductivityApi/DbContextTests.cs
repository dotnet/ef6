// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using Xunit;

    /// <summary>
    ///     Tests for the primary methods on DbContext.
    /// </summary>
    public class DbContextReplaceConnectionTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public DbContextReplaceConnectionTests()
        {
            CreateMetadataFilesForSimpleModel();
        }

        #endregion

        #region Replace connection tests

        [Fact]
        public void Can_replace_connection()
        {
            using (var context = new ReplaceConnectionContext())
            {
                using (var newConnection = new LazyInternalConnection(
                    context,
                    new DbConnectionInfo(
                        SimpleConnectionString("NewReplaceConnectionContextDatabase"),
                        "System.Data.SqlClient")))
                {
                    Can_replace_connection_implementation(context, newConnection);
                }
            }
        }

        [Fact]
        public void Can_replace_connection_with_different_provider()
        {
            using (var context = new ReplaceConnectionContext())
            {
                using (var newConnection = new LazyInternalConnection(
                    context,
                    new DbConnectionInfo(
                        "Data Source=NewReplaceConnectionContextDatabase.sdf",
                        "System.Data.SqlServerCe.4.0")))
                {
                    Can_replace_connection_implementation(context, newConnection);
                }
            }
        }

        private void Can_replace_connection_implementation(
            ReplaceConnectionContext context,
            LazyInternalConnection newConnection)
        {
            Database.Delete(newConnection.Connection);
            Database.Delete(typeof(ReplaceConnectionContext).DatabaseName());

            context.InternalContext.OverrideConnection(newConnection);

            context.Entities.Add(
                new PersistEntity
                {
                    Name = "Testing"
                });
            context.SaveChanges();

            Assert.Same(newConnection.Connection, context.Database.Connection);
            Assert.True(Database.Exists(newConnection.Connection));
            Assert.False(Database.Exists(typeof(ReplaceConnectionContext).DatabaseName()));

            // By pass EF just to make sure everything targetted the correct database
            var cmd = newConnection.Connection.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM PersistEntities";
            cmd.Connection.Open();
            Assert.Equal(1, cmd.ExecuteScalar());
            cmd.Connection.Close();
        }

        public class ReplaceConnectionContext : DbContext
        {
            public DbSet<PersistEntity> Entities { get; set; }
        }

        public class PersistEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}
