namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    /// <summary>
    ///     Base class for changes that affect foreign key constraints.
    /// </summary>
    public abstract class ForeignKeyOperation : MigrationOperation
    {
        private string _principalTable;
        private string _dependentTable;

        private readonly List<string> _dependentColumns = new List<string>();

        private string _name;

        /// <summary>
        ///     Initializes a new instance of the ForeignKeyOperation class.
        /// </summary>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected ForeignKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     Gets or sets the name of the table that the foreign key constraint targets.
        /// </summary>
        public string PrincipalTable
        {
            get { return _principalTable; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _principalTable = value;
            }
        }

        /// <summary>
        ///     Gets or sets the name of the table that the foreign key columns exist in.
        /// </summary>
        public string DependentTable
        {
            get { return _dependentTable; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                _dependentTable = value;
            }
        }

        /// <summary>
        ///     The names of the foreign key column(s).
        /// </summary>
        public IList<string> DependentColumns
        {
            get { return _dependentColumns; }
        }

        /// <summary>
        ///     Gets a value indicating if a specific name has been supplied for this foreign key constraint.
        /// </summary>
        public bool HasDefaultName
        {
            get { return string.Equals(Name, DefaultName, StringComparison.Ordinal); }
        }

        /// <summary>
        ///     Gets or sets the name of this foreign key constraint.
        ///     If no name is supplied, a default name will be calculated.
        /// </summary>
        public string Name
        {
            get { return _name ?? DefaultName; }
            set { _name = value; }
        }

        internal string DefaultName
        {
            get
            {
                return
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "FK_{0}_{1}_{2}",
                        DependentTable,
                        PrincipalTable,
                        DependentColumns.Join(separator: "_"))
                        .RestrictTo(128);
            }
        }
    }
}
