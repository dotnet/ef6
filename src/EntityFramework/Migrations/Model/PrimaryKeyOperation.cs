// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Common base class to represent operations affecting primary keys.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public abstract class PrimaryKeyOperation : MigrationOperation
    {
        /// <summary>
        /// Returns the default name for the primary key.
        /// </summary>
        /// <param name="table">The target table name.</param>
        /// <returns>The default primary key name.</returns>
        public static string BuildDefaultName(string table)
        {
            Check.NotEmpty(table, "table");

            return string.Format(CultureInfo.InvariantCulture, "PK_{0}", table).RestrictTo(128);
        }

        private readonly List<string> _columns = new List<string>();

        private string _table;
        private string _name;

        /// <summary>
        /// Initializes a new instance of the PrimaryKeyOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected PrimaryKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
            IsClustered = true;
        }

        /// <summary>
        /// Gets or sets the name of the table that contains the primary key.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
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
        /// Gets the column(s) that make up the primary key.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public IList<string> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets a value indicating if a specific name has been supplied for this primary key.
        /// </summary>
        public bool HasDefaultName
        {
            get { return string.Equals(Name, DefaultName, StringComparison.Ordinal); }
        }

        /// <summary>
        /// Gets or sets the name of this primary key.
        /// If no name is supplied, a default name will be calculated.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public string Name
        {
            get { return _name ?? DefaultName; }
            set { _name = value; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        internal string DefaultName
        {
            get { return BuildDefaultName(Table); }
        }

        /// <summary>
        /// Gets or sets whether this is a clustered primary key.
        /// </summary>
        public bool IsClustered { get; set; }
    }
}
