namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents creating a database index.
    /// </summary>
    public class CreateIndexOperation : IndexOperation
    {
        /// <summary>
        ///     Initializes a new instance of the CreateIndexOperation class.
        ///     The Table and Columns properties should also be populated.
        /// </summary>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateIndexOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     Gets or sets a value indicating if this is a unique index.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        ///     Gets an operation to drop this index.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var dropIndexOperation = new DropIndexOperation(this)
                    {
                        Name = Name,
                        Table = Table
                    };

                Columns.Each(c => dropIndexOperation.Columns.Add(c));

                return dropIndexOperation;
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
