namespace System.Data.Entity.Migrations.Infrastructure
{
    /// <summary>
    ///     Provides additional metadata about a code-based migration.
    /// </summary>
    public interface IMigrationMetadata
    {
        /// <summary>
        ///     Gets the unique identifier for the migration.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Gets the state of the model before this migration is run.
        /// </summary>
        string Source { get; }

        /// <summary>
        ///     Gets the state of the model after this migration is run.
        /// </summary>
        string Target { get; }
    }
}
