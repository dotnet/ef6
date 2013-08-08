// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Common base class for operations affecting indexes.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public abstract class IndexOperation : MigrationOperation
    {
        private string _table;
        private readonly List<string> _columns = new List<string>();
        private string _name;

        /// <summary>
        /// Initializes a new instance of the IndexOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected IndexOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        /// Gets or sets the table the index belongs to.
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
        /// Gets the columns that are indexed.
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
        /// Gets a value indicating if a specific name has been supplied for this index.
        /// </summary>
        public bool HasDefaultName
        {
            get { return string.Equals(Name, DefaultName, StringComparison.Ordinal); }
        }

        /// <summary>
        /// Gets or sets the name of this index.
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
