// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SampleEntityFrameworkProvider
{
    public partial class SampleFactory : DbProviderFactory
    {
        public static readonly SampleFactory Instance = new SampleFactory();

        public override bool CanCreateDataSourceEnumerator
        {
            get
            {
                return true;
            }
        }

        public override DbCommand CreateCommand()
        {
            return new SampleCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new SqlCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new SampleConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new SqlConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return System.Data.Sql.SqlDataSourceEnumerator.Instance;
        }

        public override DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        public override System.Security.CodeAccessPermission CreatePermission(System.Security.Permissions.PermissionState state)
        {
            return new SqlClientPermission(state);
        }

    }
}
