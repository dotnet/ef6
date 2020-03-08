// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using Xunit;

    public class TransactionContextInitializerTests
    {
        [Fact] // CodePlex 2029
        public void InitializeDatabase_does_not_create_new_connections()
        {
            using (var context = new SomeContext())
            {
                context.Database.Initialize(force: false);

                using (var transactionContext = new SomeTransactionContext(context.Database.Connection))
                {
                    var initializer = new TransactionContextInitializer<TransactionContext>();

                    var recorder = new ConnectionRecorder();
                    DbInterception.Add(recorder);
                    try
                    {
                        using (transactionContext.Database.BeginTransaction())
                        {
                            initializer.InitializeDatabase(transactionContext);
                        }

                        Assert.Equal(1, recorder.Connections.Count);
                    }
                    finally
                    {
                        DbInterception.Remove(recorder);
                    }
                }
            }
        }

        public class SomeContext : DbContext
        {
            static SomeContext()
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<SomeContext>());
            }
        }

        public class SomeTransactionContext : TransactionContext
        {
            static SomeTransactionContext()
            {
                Database.SetInitializer<SomeTransactionContext>(null);
            }

            public SomeTransactionContext(DbConnection existingConnection)
                : base(existingConnection)
            {
            }
        }

        public class ConnectionRecorder : IDbConnectionInterceptor
        {
            private readonly HashSet<DbConnection> _connections
                = new HashSet<DbConnection>(ObjectReferenceEqualityComparer.Default);

            public HashSet<DbConnection> Connections
            {
                get { return _connections; }
            }

            public void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
            {
            }

            public void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
            {
            }

            public void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringSetting(
                DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext)
            {
            }

            public void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
            {
            }

            public void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext)
            {
            }

            public void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
            {
            }

            public void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext)
            {
            }

            public void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
                if (connection.Database != "master")
                {
                    Connections.Add(connection);
                }
            }

            public void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext)
            {
            }

            public void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
            {
            }

            public void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext)
            {
            }
        }
    }
}
