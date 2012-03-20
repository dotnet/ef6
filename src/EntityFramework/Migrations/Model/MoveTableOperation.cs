namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Represents moving a table from one schema to another.
    /// </summary>
    public class MoveTableOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _newSchema;

        /// <summary>
        ///     Initializes a new instance of the MoveTableOperation class.
        /// </summary>
        /// <param name = "name">Name of the table to be moved.</param>
        /// <param name = "newSchema">Name of the schema to move the table to.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        public MoveTableOperation(string name, string newSchema, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            _name = name;
            _newSchema = newSchema;
        }

        /// <summary>
        ///     Gets the name of the table to be moved.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Gets the name of the schema to move the table to.
        /// </summary>
        public virtual string NewSchema
        {
            get { return _newSchema; }
        }

        /// <summary>
        ///     Gets an operation that moves the table back to its original schema.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                string oldSchema = null;

                var parts = _name.Split(new[] { '.' }, 2);

                if (parts.Length > 1)
                {
                    oldSchema = parts[0];
                }

                var table = parts.Last();

                return new MoveTableOperation(NewSchema + '.' + table, oldSchema);
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}