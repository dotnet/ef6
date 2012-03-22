namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents a provider specific SQL statement to be executed directly against the target database.
    /// </summary>
    public class SqlOperation : MigrationOperation
    {
        private readonly string _sql;

        /// <summary>
        ///     Initializes a new instance of the SqlOperation class.
        /// </summary>
        /// <param name = "sql">The SQL to be executed.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        public SqlOperation(string sql, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(sql));

            _sql = sql;
        }

        /// <summary>
        ///     Gets the SQL to be executed.
        /// </summary>
        public virtual string Sql
        {
            get { return _sql; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this statement should be performed outside of
        ///     the transaction scope that is used to make the migration process transactional.
        ///     If set to true, this operation will not be rolled back if the migration process fails.
        /// </summary>
        public virtual bool SuppressTransaction { get; set; }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return true; }
        }
    }
}
