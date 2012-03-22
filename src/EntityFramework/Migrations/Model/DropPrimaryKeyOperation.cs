namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Migrations.Extensions;

    /// <summary>
    ///     Represents dropping a primary key from a table.
    /// </summary>
    public class DropPrimaryKeyOperation : PrimaryKeyOperation
    {
        /// <summary>
        ///     Initializes a new instance of the DropPrimaryKeyOperation class.
        ///     The Table and Columns properties should also be populated.
        /// </summary>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        public DropPrimaryKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     Gets an operation to add the primary key.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var addPrimaryKeyOperation = new AddPrimaryKeyOperation
                                                 {
                                                     Name = Name,
                                                     Table = Table
                                                 };

                Columns.Each(c => addPrimaryKeyOperation.Columns.Add(c));

                return addPrimaryKeyOperation;
            }
        }
    }
}
