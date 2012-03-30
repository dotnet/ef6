namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents renaming an existing table.
    /// </summary>
    public class RenameTableOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _newName;

        /// <summary>
        ///     Initializes a new instance of the RenameTableOperation class.
        /// </summary>
        /// <param name = "name">Name of the table to be renamed.</param>
        /// <param name = "newName">New name for the table.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public RenameTableOperation(string name, string newName, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(newName));

            _name = name;
            _newName = newName;
        }

        /// <summary>
        ///     Gets the name of the table to be renamed.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Gets the new name for the table.
        /// </summary>
        public virtual string NewName
        {
            get { return _newName; }
        }

        /// <summary>
        ///     Gets an operation that reverts the rename.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get { return new RenameTableOperation(NewName, Name); }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
