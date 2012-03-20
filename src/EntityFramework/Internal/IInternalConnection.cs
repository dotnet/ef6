namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;

    /// <summary>
    ///     IInternalConnection objects manage DbConnections.
    ///     Two concrete implementations of this interface exist--LazyInternalConnection and EagerInternalConnection.
    /// </summary>
    internal interface IInternalConnection : IDisposable
    {
        /// <summary>
        ///     Returns the underlying DbConnection.
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        ///     Returns a key consisting of the connection type and connection string.
        ///     If this is an EntityConnection then the metadata path is included in the key returned.
        /// </summary>
        string ConnectionKey { get; }

        /// <summary>
        ///     Gets a value indicating whether the connection is an EF connection which therefore contains
        ///     metadata specifying the model, or instead is a store connection, in which case it contains no
        ///     model info.
        /// </summary>
        /// <value><c>true</c> if the connection contains model info; otherwise, <c>false</c>.</value>
        bool ConnectionHasModel { get; }

        /// <summary>
        ///     Returns the origin of the underlying connection string.
        /// </summary>
        DbConnectionStringOrigin ConnectionStringOrigin { get; }

        /// <summary>
        ///     Gets or sets an object representing a config file used for looking for DefaultConnectionFactory entries
        ///     and connection strins.
        /// </summary>
        AppConfig AppConfig { get; set; }

        /// <summary>
        ///     Gets or sets the provider to be used when creating the underlying connection.
        /// </summary>
        string ProviderName { get; set; }

        /// <summary>
        ///     Gets the name of the underlying connection string.
        /// </summary>
        string ConnectionStringName { get; }

        /// <summary>
        ///     Gets the original connection string.
        /// </summary>
        string OriginalConnectionString { get; }

        /// <summary>
        ///     Creates an <see cref = "ObjectContext" /> from metadata in the connection.  This method must
        ///     only be called if ConnectionHasModel returns true.
        /// </summary>
        /// <returns>The newly created context.</returns>
        ObjectContext CreateObjectContextFromConnectionModel();
    }
}