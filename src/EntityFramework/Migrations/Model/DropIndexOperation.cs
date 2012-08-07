// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents dropping an existing index.
    /// </summary>
    public class DropIndexOperation : IndexOperation
    {
        private readonly CreateIndexOperation _inverse;

        /// <summary>
        ///     Initializes a new instance of the DropIndexOperation class.
        /// </summary>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropIndexOperation(object anonymousArguments = null)
            : base(anonymousArguments)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the DropIndexOperation class.
        /// </summary>
        /// <param name="inverse"> The operation that represents reverting dropping the index. </param>
        /// <param name="anonymousArguments"> Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropIndexOperation(CreateIndexOperation inverse, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Contract.Requires(inverse != null);

            _inverse = inverse;
        }

        /// <summary>
        ///     Gets an operation that represents reverting dropping the index.
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
            get { return false; }
        }
    }
}
