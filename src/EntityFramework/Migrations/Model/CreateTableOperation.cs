// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents creating a table.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class CreateTableOperation : MigrationOperation, IAnnotationTarget
    {
        private readonly string _name;
        private readonly List<ColumnModel> _columns = new List<ColumnModel>();
        private AddPrimaryKeyOperation _primaryKey;
        private readonly IDictionary<string, object> _annotations;

        /// <summary>
        /// Initializes a new instance of the CreateTableOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name"> Name of the table to be created. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateTableOperation(string name, object anonymousArguments = null)
            : this(name, null, anonymousArguments)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CreateTableOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name"> Name of the table to be created. </param>
        /// <param name="annotations">Custom annotations that exist on the table to be created. May be null or empty.</param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public CreateTableOperation(string name, IDictionary<string, object> annotations, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            _annotations = annotations ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the name of the table to be created.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the columns to be included in the new table.
        /// </summary>
        public virtual IList<ColumnModel> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets or sets the primary key for the new table.
        /// </summary>
        public AddPrimaryKeyOperation PrimaryKey
        {
            get { return _primaryKey; }
            set
            {
                Check.NotNull(value, "value");

                _primaryKey = value;
                _primaryKey.Table = Name;
            }
        }

        /// <summary>
        /// Gets custom annotations that exist on the table to be created.
        /// </summary>
        public virtual IDictionary<string, object> Annotations
        {
            get { return _annotations; }
        }

        /// <summary>
        /// Gets an operation to drop the table.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                return new DropTableOperation(
                    Name,
                    Annotations,
                    Columns
                        .Where(c => c.Annotations.Count > 0)
                        .ToDictionary(
                            c => c.Name, c => (IDictionary<string, object>)c.Annotations.ToDictionary(a => a.Key, a => a.Value.NewValue)));
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }

        bool IAnnotationTarget.HasAnnotations
        {
            get
            {
                return Annotations.Any()
                       || Columns.SelectMany(c => c.Annotations).Any();
            }
        }
    }
}
