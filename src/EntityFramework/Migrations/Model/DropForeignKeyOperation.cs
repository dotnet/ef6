namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents a foreign key constraint being dropped from a table.
    /// </summary>
    public class DropForeignKeyOperation : ForeignKeyOperation
    {
        private readonly AddForeignKeyOperation _inverse;

        /// <summary>
        ///     Initializes a new instance of the DropForeignKeyOperation class.
        ///     The PrincipalTable, DependentTable and DependentColumns properties should also be populated.
        /// </summary>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropForeignKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the DropForeignKeyOperation class.
        /// </summary>
        /// <param name = "inverse">The operation that represents reverting dropping the foreign key constraint.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropForeignKeyOperation(AddForeignKeyOperation inverse, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Contract.Requires(inverse != null);

            _inverse = inverse;
        }

        /// <summary>
        ///     Gets an operation to drop the associated index on the foreign key column(s).
        /// </summary>
        /// <returns>An operation to drop the index.</returns>
        public virtual DropIndexOperation CreateDropIndexOperation()
        {
            var dropIndexOperation
                = new DropIndexOperation(_inverse.CreateCreateIndexOperation())
                      {
                          Table = DependentTable
                      };

            DependentColumns.Each(c => dropIndexOperation.Columns.Add(c));

            return dropIndexOperation;
        }

        /// <summary>
        ///     Gets an operation that represents reverting dropping the foreign key constraint.
        ///     The inverse cannot be automatically calculated, 
        ///     if it was not supplied to the constructor this property will return null.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get { return _inverse; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
