// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;

    internal sealed class CommandTracer : IDisposable, IDbInterceptor
    {
        private readonly List<DbCommand> _commands = new List<DbCommand>();
        private readonly List<DbCommandTree> _commandTrees = new List<DbCommandTree>();

        private readonly DbContext _context;
        private readonly Interception _interception;

        public CommandTracer(DbContext context)
            : this(context, Interception.Instance)
        {
        }

        internal CommandTracer(DbContext context, Interception interception)
        {
            Check.NotNull(context, "context");
            DebugCheck.NotNull(interception);

            _context = context;
            _interception = interception;

            _interception.Add(_context, this);
        }

        public IEnumerable<DbCommand> DbCommands
        {
            get { return _commands; }
        }

        public IEnumerable<DbCommandTree> CommandTrees
        {
            get { return _commandTrees; }
        }

        bool IDbInterceptor.CommandExecuting(DbCommand command)
        {
            _commands.Add(command);

            return false; // cancel execution
        }

        DbCommandTree IDbInterceptor.CommandTreeCreated(DbCommandTree commandTree)
        {
            _commandTrees.Add(commandTree);

            return commandTree;
        }

        bool IDbInterceptor.ConnectionOpening(DbConnection connection)
        {
            return false; // don't open
        }

        void IDisposable.Dispose()
        {
            _interception.Remove(_context, this);
        }
    }
}
