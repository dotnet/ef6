// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.SqlClient;

    public abstract class EndToEndFunctionsTest : TestBase, IDisposable
    {
        private readonly DbConnection _connection;

        protected EndToEndFunctionsTest()
        {
            var databaseName = GetType().FullName;
            var connectionString = SimpleConnectionString(databaseName);

            _connection = new SqlConnection(connectionString);

            using (var context = CreateContext())
            {
                context.Database.CreateIfNotExists();
            }
        }

        public virtual void Dispose()
        {
            _connection.Dispose();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entities().Configure(e => e.MapToStoredProcedures());
        }

        protected DbContext CreateContext()
        {
            var modelBuilder = new DbModelBuilder();
            OnModelCreating(modelBuilder);

            var model = modelBuilder.Build(_connection);
            var compiledModel = model.Compile();

            return new DbContext(_connection, compiledModel, false);
        }

        protected void Execute(string commandText)
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = commandText;

                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                command.ExecuteNonQuery();
            }
        }
    }
}
