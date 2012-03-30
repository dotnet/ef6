namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents deleting a new record from the migrations history table.
    ///     The migrations history table is used to store a log of the migrations that have been applied to the database.
    /// </summary>
    public class DeleteHistoryOperation : HistoryOperation
    {
        /// <summary>
        ///     Initializes a new instance of the DeleteHistoryOperation class.
        /// </summary>
        /// <param name = "table">Name of the migrations history table.</param>
        /// <param name = "migrationId">Id of the migration record to be deleted.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DeleteHistoryOperation(string table, string migrationId, object anonymousArguments = null)
            : base(table, migrationId, anonymousArguments)
        {
        }
    }
}
