// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Operation representing DML changes to the migrations history table.
    /// The migrations history table is used to store a log of the migrations that have been applied to the database.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public class HistoryOperation : MigrationOperation
    {
        private readonly IList<DbModificationCommandTree> _commandTrees;

        /// <summary>
        /// Initializes a new instance of the HistoryOperation class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="commandTrees"> A sequence of command trees representing the operations being applied to the history table. </param>
        /// <param name="anonymousArguments"> Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public HistoryOperation(IList<DbModificationCommandTree> commandTrees, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotNull(commandTrees, "commandTrees");

            if (!commandTrees.Any())
            {
                throw new ArgumentException(Strings.CollectionEmpty("commandTrees", "HistoryOperation"));
            }

            _commandTrees = commandTrees;
        }

        /// <summary>
        /// A sequence of commands representing the operations being applied to the history table.
        /// </summary>
        public IList<DbModificationCommandTree> CommandTrees
        {
            get { return _commandTrees; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
