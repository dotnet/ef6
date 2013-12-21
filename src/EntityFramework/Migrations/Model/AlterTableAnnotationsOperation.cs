// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents changes made to custom annotations on a table.
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources
    /// (such as the end user of an application). If input is accepted from such sources it should be validated
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class AlterTableAnnotationsOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly List<ColumnModel> _columns = new List<ColumnModel>();
        private readonly IDictionary<string, AnnotationPair> _annotations;

        /// <summary>
        /// Initializes a new instance of the AlterTableAnnotationsOperation class.
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources
        /// (such as the end user of an application). If input is accepted from such sources it should be validated
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name"> Name of the table on which annotations have changed. </param>
        /// <param name="annotations">The custom annotations on the table that have changed.</param>
        /// <param name="anonymousArguments">
        /// Additional arguments that may be processed by providers. Use anonymous type syntax to
        /// specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public AlterTableAnnotationsOperation(string name, IDictionary<string, AnnotationPair> annotations, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            _annotations = annotations ?? new Dictionary<string, AnnotationPair>();
        }

        /// <summary>
        /// Gets the name of the table on which annotations have changed.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the columns to be included in the table for which annotations have changed.
        /// </summary>
        public virtual IList<ColumnModel> Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// Gets the custom annotations that have changed on the table.
        /// </summary>
        public virtual IDictionary<string, AnnotationPair> Annotations
        {
            get { return _annotations; }
        }

        /// <summary>
        /// Gets an operation that is the inverse of this one such that annotations will be changed back to how
        /// they were before this operation was applied.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get
            {
                var inverse = new AlterTableAnnotationsOperation(
                    Name, Annotations.ToDictionary(a => a.Key, a => new AnnotationPair(a.Value.NewValue, a.Value.OldValue)));

                inverse._columns.AddRange(_columns);

                return inverse;
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
