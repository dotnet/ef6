// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;

    internal sealed class CommandTracer : ICancelableDbCommandInterceptor, IDbCommandTreeInterceptor, IEntityConnectionInterceptor, IDisposable
    {
        private readonly List<DbCommand> _commands = new List<DbCommand>();
        private readonly List<DbCommandTree> _commandTrees = new List<DbCommandTree>();

        private readonly DbContext _context;
        private readonly Dispatchers _dispatchers;

        public CommandTracer(DbContext context)
            : this(context, Interception.Dispatch)
        {
        }

        internal CommandTracer(DbContext context, Dispatchers dispatchers)
        {
            DebugCheck.NotNull(context);
            DebugCheck.NotNull(dispatchers);

            _context = context;
            _dispatchers = dispatchers;

            _dispatchers.AddInterceptor(this);
        }

        public IEnumerable<DbCommand> DbCommands
        {
            get { return _commands; }
        }

        public IEnumerable<DbCommandTree> CommandTrees
        {
            get { return _commandTrees; }
        }

        public bool CommandExecuting(DbCommand command, DbInterceptionContext interceptionContext)
        {
            if (interceptionContext.DbContexts.Contains(_context, ReferenceEquals))
            {
                _commands.Add(command);

                return false; // cancel execution
            }

            return true;
        }

        public void TreeCreated(DbCommandTree commandTree, DbCommandTreeInterceptionContext interceptionContext)
        {
            if (interceptionContext.DbContexts.Contains(_context, ReferenceEquals))
            {
                _commandTrees.Add(commandTree);
            }
        }

        public bool ConnectionOpening(EntityConnection connection, DbInterceptionContext interceptionContext)
        {
            return !interceptionContext.DbContexts.Contains(_context, ReferenceEquals);
        }

        void IDisposable.Dispose()
        {
            _dispatchers.RemoveInterceptor(this);
        }
    }
}
