namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Migrations.Extensions;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Represents adding a primary key to a table.
    /// </summary>
    public class AddPrimaryKeyOperation : PrimaryKeyOperation
    {
        /// <summary>
        ///     Initializes a new instance of the AddPrimaryKeyOperation class.
        ///     The Table and Columns properties should also be populated.
        /// </summary>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public AddPrimaryKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     Gets an operation to drop the primary key.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var dropPrimaryKeyOperation = new DropPrimaryKeyOperation
                                                  {
                                                      Name = Name,
                                                      Table = Table
                                                  };

                Columns.Each(c => dropPrimaryKeyOperation.Columns.Add(c));

                return dropPrimaryKeyOperation;
            }
        }
    }
}
