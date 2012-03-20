namespace System.Data.Entity.Infrastructure
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Describes the origin of the database connection string associated with a <see cref = "DbContext" />.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Casing is intentional")]
    public enum DbConnectionStringOrigin
    {
        /// <summary>
        ///     The connection string was created by convention.
        /// </summary>
        Convention,

        /// <summary>
        ///     The connection string was read from external configuration.
        /// </summary>
        Configuration,

        /// <summary>
        ///     The connection string was explicitly specified at runtime.
        /// </summary>
        UserCode,

        /// <summary>
        ///     The connection string was overriden by connection information supplied to DbContextInfo. 
        /// </summary>
        DbContextInfo
    }
}