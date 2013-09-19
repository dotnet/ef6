// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Base class for changes that affect foreign key constraints.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public abstract class ForeignKeyOperation : MigrationOperation
    {
        private string _principalTable;
        private string _dependentTable;

        private readonly List<string> _dependentColumns = new List<string>();

        private string _name;

        /// <summary>
        /// Initializes a new instance of the ForeignKeyOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected ForeignKeyOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        /// Gets or sets the name of the table that the foreign key constraint targets.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public string PrincipalTable
        {
            get { return _principalTable; }
            set
            {
                Check.NotEmpty(value, "value");

                _principalTable = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the table that the foreign key columns exist in.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public string DependentTable
        {
            get { return _dependentTable; }
            set
            {
                Check.NotEmpty(value, "value");

                _dependentTable = value;
            }
        }

        /// <summary>
        /// The names of the foreign key column(s).
        /// 
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        public IList<string> DependentColumns
        {
            get { return _dependentColumns; }
        }

        /// <summary>
        /// Gets a value indicating if a specific name has been supplied for this foreign key constraint.
        /// </summary>
        public bool HasDefaultName
        {
            get { return string.Equals(Name, DefaultName, StringComparison.Ordinal); }
        }

        /// <summary>
        /// Gets or sets the name of this foreign key constraint.
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
