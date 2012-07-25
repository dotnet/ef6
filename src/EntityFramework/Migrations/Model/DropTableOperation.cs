// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents dropping an existing table.
    /// </summary>
    public class DropTableOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly CreateTableOperation _inverse;

        /// <summary>
        ///     Initializes a new instance of the DropTableOperation class.
        /// </summary>
        /// <param name = "name">The name of the table to be dropped.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropTableOperation(string name, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            _name = name;
        }

        /// <summary>
        ///     Initializes a new instance of the DropTableOperation class.
        /// </summary>
        /// <param name = "name">The name of the table to be dropped.</param>
        /// <param name = "inverse">An operation that represents reverting dropping the table.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropTableOperation(string name, CreateTableOperation inverse, object anonymousArguments = null)
            : this(name, anonymousArguments)
        {
            Contract.Requires(inverse != null);

            _inverse = inverse;
        }

        /// <summary>
        ///     Gets the name of the table to be dropped.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///     Gets an operation that represents reverting dropping the table.
        ///     The inverse cannot be automatically calculated, 
        ///     if it was not supplied to the constructor this property will return null.
        /// </summary>
        public override MigrationOperation Inverse
        {
            get { return _inverse; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return true; }
        }
    }
}
