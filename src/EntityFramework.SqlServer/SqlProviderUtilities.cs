namespace System.Data.Entity.SqlServer
{
    using System.Data.Common;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.SqlClient;

    internal class SqlProviderUtilities
    {
        /// <summary>
        /// Requires that the given connection is of type  T. 
        /// Returns the connection or throws.
        /// </summary>
        internal static SqlConnection GetRequiredSqlConnection(DbConnection connection)
        {
            var result = connection as SqlConnection;
            if (null == result)
            {
                throw new ArgumentException(Strings.Mapping_Provider_WrongConnectionType(typeof(SqlConnection)));
            }
            return result;
        }
    }
}
