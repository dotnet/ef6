namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;

    /// <summary>
    ///     Represents a foreign key constraint being added to a table.
    /// </summary>
    public class AddForeignKeyOperation : ForeignKeyOperation
    {
        private readonly List<string> _principalColumns = new List<string>();

        /// <summary>
        ///     Initializes a new instance of the AddForeignKeyOperation class.
        ///     The PrincipalTable, PrincipalColumns, DependentTable and DependentColumns properties should also be populated.
        /// </summary>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        public AddForeignKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     The names of the column(s) that the foreign key constraint should target.
        /// </summary>
        public IList<string> PrincipalColumns
        {
            get { return _principalColumns; }
        }

        /// <summary>
        ///     Gets or sets a value indicating if cascade delete should be configured on the foreign key constraint.
        /// </summary>
        public bool CascadeDelete { get; set; }

        /// <summary>
        ///     Gets an operation to create an index on the foreign key column(s).
        /// </summary>
        /// <returns>An operation to add the index.</returns>
        public virtual CreateIndexOperation CreateCreateIndexOperation()
        {
            var createIndexOperation
                = new CreateIndexOperation
                      {
                          Table = DependentTable
                      };

            DependentColumns.Each(c => createIndexOperation.Columns.Add(c));

            return createIndexOperation;
        }

        /// <summary>
        ///     Gets an operation to drop the foreign key constraint.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var dropForeignKeyOperation = new DropForeignKeyOperation
                                                  {
                                                      Name = Name,
                                                      PrincipalTable = PrincipalTable,
                                                      DependentTable = DependentTable,
                                                  };

                DependentColumns.Each(c => dropForeignKeyOperation.DependentColumns.Add(c));

                return dropForeignKeyOperation;
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
