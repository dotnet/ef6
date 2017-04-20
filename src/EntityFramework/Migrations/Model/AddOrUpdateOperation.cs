using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// AddOrUpdateRowOperation
    /// </summary>
    public class AddOrUpdateOperation : MigrationOperation
    {
        private readonly string _table;
        private readonly List<string> _identifiers;
        private readonly List<string> _columns;
        private readonly List<object> _values;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="identifiers"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
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
        /// 
        /// </summary>
        public string Table
        {
            get { return _table; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<string> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<object> Values
        {
            get { return _values; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<string> Identifiers
        {

            get { return _identifiers; }
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
