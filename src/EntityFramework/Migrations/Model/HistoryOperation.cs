// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Operation representing DML changes to the migrations history table.
    ///     The migrations history table is used to store a log of the migrations that have been applied to the database.
    /// </summary>
    public class HistoryOperation : MigrationOperation
    {
        private readonly IEnumerable<InterceptedCommand> _commands;

        /// <summary>
        ///     Initializes a new instance of the HistoryOperation class.
        /// </summary>
        /// <param name="commands"> A sequence of commands representing the operations being applied to the history table. </param>
        /// <param name="anonymousArguments"> Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public HistoryOperation(IEnumerable<InterceptedCommand> commands, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotNull(commands, "commands");
            if (!commands.Any())
            {
                throw new ArgumentException(Strings.CollectionEmpty("commands", "HistoryOperation"));
            }

            _commands = commands;
        }

        /// <summary>
        ///     A sequence of commands representing the operations being applied to the history table.
        /// </summary>
        public IEnumerable<InterceptedCommand> Commands
        {
            get { return _commands; }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
