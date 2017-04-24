namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents a row being deleted from a table.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class DeleteOperation : MigrationOperation
    {
        private readonly string _table;
        private readonly string[] _columns;
        private readonly object[][] _values;

        /// <summary>
        /// Initializes a new instance of the DeleteOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc. 
        /// </summary>
        /// <param name="table"> The name of the table the row should be deleteded from. </param>
        /// <param name="columns"> The name/s of the column/s that are going to identify the row be deleted. </param>
        /// <param name="values"> The value/s that going to identify the row be deleted. </param>
        public DeleteOperation(string table, string[] columns, object[][] values)
            : base(null)
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(columns, "columns");
            Check.NotNull(values, "values");

            _table = table;
            _columns = columns;
            _values = values;
        }

        /// <summary>
        /// Gets the name of the table the row should be deleteded from.
        /// </summary>
        public string Table
        {
            get { return _table; }
        }

        /// <summary>
        /// Gets the name/s of the column/s that are going to identify the row be deleted. 
        /// </summary>
        public string[] Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets the value/s that going to identify the row be deleted.
        /// </summary>
        public object[][] Values
        {
            get { return _values; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange { get { return false; } }
    }
}
