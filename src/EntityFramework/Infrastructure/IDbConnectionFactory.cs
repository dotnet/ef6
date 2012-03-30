namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Implementations of this interface are used to create DbConnection objects for
    ///     a type of database server based on a given database name.  
    ///     An Instance is set on the <see cref = "Database" /> class to
    ///     cause all DbContexts created with no connection information or just a database
    ///     name or connection string to use a certain type of database server by default.
    ///     Two implementations of this interface are provided: <see cref = "SqlConnectionFactory" />
    ///     is used to create connections to Microsoft SQL Server, including EXPRESS editions.
    ///     <see cref = "SqlCeConnectionFactory" /> is used to create connections to Microsoft SQL
    ///     Server Compact Editions.
    ///     Other implementations for other database servers can be added as needed.
    ///     Note that implementations should be thread safe or immutable since they may
    ///     be accessed by multiple threads at the same time.
    /// </summary>
    [ContractClass(typeof(IDbConnectionFactoryContracts))]
    public interface IDbConnectionFactory
    {
        /// <summary>
        ///     Creates a connection based on the given database name or connection string.
        /// </summary>
        /// <param name = "nameOrConnectionString">The database name or connection string.</param>
        /// <returns>An initialized DbConnection.</returns>
        DbConnection CreateConnection(string nameOrConnectionString);
    }

    [ContractClassFor(typeof(IDbConnectionFactory))]
    internal abstract class IDbConnectionFactoryContracts : IDbConnectionFactory
    {
        DbConnection IDbConnectionFactory.CreateConnection(string nameOrConnectionString)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(nameOrConnectionString));

            throw new NotImplementedException();
        }
    }
}
