namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    ///     Represents inserting a new record into the migrations history table.
    ///     The migrations history table is used to store a log of the migrations that have been applied to the database.
    /// </summary>
    public class InsertHistoryOperation : HistoryOperation
    {
        private static readonly string _productVersion =
            Assembly.GetExecutingAssembly().GetInformationalVersion();

        private readonly byte[] _model;

        /// <summary>
        ///     Initializes a new instance of the InsertHistoryOperation class.
        /// </summary>
        /// <param name = "table">Name of the migrations history table.</param>
        /// <param name = "migrationId">Id of the migration record to be inserted.</param>
        /// <param name = "model">Value to be stored in the model column.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        public InsertHistoryOperation(
            string table,
            string migrationId,
            byte[] model,
            object anonymousArguments = null)
            : base(table, migrationId, anonymousArguments)
        {
            Contract.Requires(model != null);

            _model = model;
        }

        /// <summary>
        ///     Gets the value to store in the history table representing the target model of the migration.
        /// </summary>
        public byte[] Model
        {
            get { return _model; }
        }

        /// <summary>
        ///     Gets the value to store in the history table indicating the version of Entity Framework used to produce this migration.
        /// </summary>
        public string ProductVersion
        {
            get { return _productVersion; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
