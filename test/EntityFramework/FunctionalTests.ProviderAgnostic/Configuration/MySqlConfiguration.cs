// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Configuration
{
    using MySql.Data.MySqlClient;
    using System.Data.Entity;

    public class MySqlConfiguration : DbConfiguration
    {
        public MySqlConfiguration()
        {
            SetHistoryContext(
                "MySql.Data.MySqlClient",
                (connection, defaultSchema) => new MySqlHistoryContext(connection, defaultSchema));

            SetDefaultConnectionFactory(new MySqlConnectionFactory());
        }
    }
}
