namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    /// Represents a row being added or updated to a table.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class AddOrUpdateOperation : MigrationOperation
    {
        private readonly string _table;
        private readonly List<string> _identifiers;
        private readonly List<string> _columns;
        private readonly List<object> _values;

        /// <summary>
        /// Initializes a new instance of the AddorUpdateOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="table"> The name of the table the row should be added or updated to. </param>
        /// <param name="columns"> The name/s of the column/s that going to have data added or updated. </param>
        /// <param name="values"> The value/s that going to be or updated. </param>
        public AddOrUpdateOperation(string table, string[] columns, object[] values) : this(table,new []{string.Empty}, columns, values)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AddorUpdateOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc. 
        /// </summary>
        /// <param name="table"> The name of the table the row should be added or updated to. </param>
        /// <param name="identifiers"> The name/s of the column/s that going to identiy if the column/s with the respective value exists. </param>
        /// <param name="columns"> The name/s of the column/s that going to have data added or updated. </param>
        /// <param name="values"> The value/s that going to be or updated. </param>
        public AddOrUpdateOperation(string table, string[] identifiers, string[] columns, object[] values) : base(null)
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(columns, "columns");
            Check.NotNull(values, "values");
            Check.NotNull(identifiers, "identifiers");

            _table = table;
            _columns = columns.ToList();
            _values = values.ToList();
            _identifiers = identifiers.ToList();
        }
        /// <summary>
        /// Gets the name of the table the row should be added or updated to.
        /// </summary>
        public string Table
        {
            get { return _table; }
        }

        /// <summary>
        /// Gets the name/s of the column/s that going to have data added or updated. 
        /// </summary>
        public IList<string> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets the value/s that going to be or updated.
        /// </summary>
        public IList<object> Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Gets the name/s of the column/s that going to identiy if the column/s with the respective value exists.
        /// </summary>
        public IList<string> Identifiers
        {
            get { return _identifiers; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
