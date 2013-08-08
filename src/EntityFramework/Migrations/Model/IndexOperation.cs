// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Common base class for operations affecting indexes.
    /// </summary>
    public abstract class IndexOperation : MigrationOperation
    {
        private string _table;
        private readonly List<string> _columns = new List<string>();
        private string _name;

        /// <summary>
        /// Initializes a new instance of the IndexOperation class.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected IndexOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        /// Gets or sets the table the index belongs to.
        /// </summary>
        public string Table
        {
            get { return _table; }
            set
            {
                Check.NotEmpty(value, "value");

                _table = value;
            }
        }

        /// <summary>
        /// Gets the columns that are indexed.
        /// </summary>
        public IList<string> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets a value indicating if a specific name has been supplied for this index.
        /// </summary>
        public bool HasDefaultName
        {
            get { return string.Equals(Name, DefaultName, StringComparison.Ordinal); }
        }

        /// <summary>
        /// Gets or sets the name of this index.
        /// If no name is supplied, a default name will be calculated.
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
                    string.Format(CultureInfo.InvariantCulture, "IX_{0}", Columns.Join(separator: "_")).RestrictTo(128);
            }
        }
    }
}
