// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Configuration
{
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.SqlServer;

#if NET452
    using System.Runtime.Remoting.Messaging;
    using MySql.Data.MySqlClient;
#else
    using System.Threading;
#endif

    public class ProviderAgnosticConfiguration : DbConfiguration
    {
#if NET452
        private static readonly string _providerInvariantName = ConfigurationManager.AppSettings["ProviderInvariantName"];
        private static readonly string _baseConnectionString = ConfigurationManager.AppSettings["BaseConnectionString"];
#else
        private static readonly string _providerInvariantName = "System.Data.SqlClient";
        private static readonly string _baseConnectionString = @"Data Source=(localdb)\MSSQLLocalDB; Integrated Security=True;";
        private static readonly AsyncLocal<bool> _suspendExecutionStrategy = new AsyncLocal<bool>();
#endif

        public ProviderAgnosticConfiguration()
        {
#if NET452
            SetHistoryContext(
                "MySql.Data.MySqlClient",
                (connection, defaultSchema) => new MySqlHistoryContext(connection, defaultSchema));
#endif

            SetExecutionStrategy("System.Data.SqlClient", () => SuspendExecutionStrategy
              ? (IDbExecutionStrategy)new DefaultExecutionStrategy()
              : new SqlAzureExecutionStrategy());

            switch (_providerInvariantName)
            {
                case "System.Data.SqlClient":
                    SetDefaultConnectionFactory(new SqlConnectionFactory(_baseConnectionString));
                    break;

#if NET452
                case "MySql.Data.MySqlClient" :
                    SetDefaultConnectionFactory(new MySqlConnectionFactory());
                    break;
#endif

                default:
                    throw new InvalidOperationException("Unknown ProviderInvariantName specified in App.config: " + _providerInvariantName);
            }

            AddDependencyResolver(MutableResolver.Instance);
        }

        public static bool SuspendExecutionStrategy
        {
#if NET452
            get => (bool?)CallContext.LogicalGetData("SuspendExecutionStrategy") ?? false;
            set => CallContext.LogicalSetData("SuspendExecutionStrategy", value);
#else
            get => _suspendExecutionStrategy.Value;
            set => _suspendExecutionStrategy.Value = value;
#endif
        }
    }
}
