//---------------------------------------------------------------------
// <copyright file="Factory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//---------------------------------------------------------------------

/*/////////////////////////////////////////////////////////////////////////////
 * Sample ADO.NET Entity Framework Provider
 * 
 * This factory class creates and returns ADO.NET 2.0 SqlClient components
 * Leveraging the pre-existing components decreases the amount of code
 * for the sample and makes it easier to demonstrate how to enhance an
 * existing ADO.NET 2.0 provider to support the Entity Framework
 * Since the Entity Framework does require extending the Connection class, the
 * sample uses its own Connection class that internally relies on SqlConnection
 * The sample also uses its own Command class because of the interaction
 * between the Command and Connection classes
 */
////////////////////////////////////////////////////////////////////////////


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
