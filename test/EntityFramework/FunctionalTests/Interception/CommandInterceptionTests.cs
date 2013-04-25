// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class CommandInterceptionTests : FunctionalTestBase
    {
        [Fact]
        public void Initialization_and_simple_query_and_update_commands_can_be_logged()
        {
            var logger = new CommandLogger();
            Interception.AddInterceptor(logger);

            try
            {
                using (var context = new BlogContextLogAll())
                {
                    BlogContext.DoStuff(context);
                }
            }
            finally
            {
                Interception.RemoveInterceptor(logger);
            }

            var commandsUsed = new bool[Enum.GetValues(typeof(CommandMethod)).Length];

            // Check that every "executed" call is preceded by an "executing" call.
            // (The reverse is not true since "executed" is not called for operations that throw.)
            for (var i = 0; i < logger.Log.Count; i++)
            {
                var method = logger.Log[i].Method;
                commandsUsed[(int)method] = true;

                if (method.ToString().EndsWith("Executed"))
                {
                    Assert.Equal(method - 1, logger.Log[i - 1].Method);
                }
            }

            // Check that every type of command used has log entries
            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuted]);
            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
            Assert.True(commandsUsed[(int)CommandMethod.ScalarExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.ScalarExecuted]);

#if NET40
            Assert.False(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuting]);
            Assert.False(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuted]);
            Assert.False(commandsUsed[(int)CommandMethod.AsyncReaderExecuting]);
            Assert.False(commandsUsed[(int)CommandMethod.AsyncReaderExecuted]);
#else
            Assert.True(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuted]);
            Assert.True(commandsUsed[(int)CommandMethod.AsyncReaderExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.AsyncReaderExecuted]);
#endif

            // EF and SQL provider never send ExecuteScalarAsync
            Assert.False(commandsUsed[(int)CommandMethod.AsyncScalarExecuting]);
            Assert.False(commandsUsed[(int)CommandMethod.AsyncScalarExecuted]);

            // Sanity check on command text
            var commandTexts = logger.Log.Select(l => l.CommandText.ToLowerInvariant());
            Assert.True(commandTexts.Any(c => c.StartsWith("select")));
            Assert.True(commandTexts.Any(c => c.StartsWith("create")));
            Assert.True(commandTexts.Any(c => c.StartsWith("alter")));
            Assert.True(commandTexts.Any(c => c.StartsWith("insert")));
            Assert.True(commandTexts.Any(c => c.StartsWith("update")));

            // Sanity check on results
            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.NonQueryExecuted).All(l => l.Result != null));
            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.ReaderExecuted).All(l => l.Result != null));

#if !NET40
            Assert.True(
                logger.Log.Where(
                    l => l.Method.ToString().StartsWith("Async")
                         && l.Method.ToString().EndsWith("Executed")).All(l => l.Result != null));
#endif
        }

        public class BlogContextLogAll : BlogContext
        {
            static BlogContextLogAll()
            {
                Database.SetInitializer<BlogContextLogAll>(new BlogInitializer());
            }
        }

        [Fact]
        public void Multiple_contexts_running_concurrently_can_use_interception()
        {
            using (var context = new BlogContextNoInit())
            {
                context.Database.Initialize(force: false);
            }

            var loggers = new ConcurrentBag<CommandLogger>();

            const int executionCount = 5;
            ExecuteInParallel(
                () =>
                    {
                        using (var context = new BlogContextNoInit())
                        {
                            var logger = new CommandLogger(context);
                            Interception.AddInterceptor(logger);
                            loggers.Add(logger);

                            try
                            {
                                BlogContext.DoStuff(context);
                            }
                            finally
                            {
                                Interception.RemoveInterceptor(logger);
                            }

                            var commandsUsed = new bool[Enum.GetValues(typeof(CommandMethod)).Length];

                            // Check that every "executed" call is precedded by an "executing" call.
                            // (The reverse is not true since "executed" is not called for operations that throw.)
                            for (var i = 0; i < logger.Log.Count; i++)
                            {
                                var method = logger.Log[i].Method;
                                commandsUsed[(int)method] = true;

                                if (method.ToString().EndsWith("Executed"))
                                {
                                    Assert.Equal(method - 1, logger.Log[i - 1].Method);
                                }
                            }

                            // Check that expected command have log entries
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
#if NET40
                            Assert.False(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuting]);
                            Assert.False(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuted]);
                            Assert.False(commandsUsed[(int)CommandMethod.AsyncReaderExecuting]);
                            Assert.False(commandsUsed[(int)CommandMethod.AsyncReaderExecuted]);
#else
                            Assert.True(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.AsyncNonQueryExecuted]);
                            Assert.True(commandsUsed[(int)CommandMethod.AsyncReaderExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.AsyncReaderExecuted]);
#endif

                            // Sanity check on command text
                            var commandTexts = logger.Log.Select(l => l.CommandText.ToLowerInvariant());
                            Assert.True(commandTexts.Any(c => c.StartsWith("select")));
                            Assert.True(commandTexts.Any(c => c.StartsWith("insert")));
                            Assert.True(commandTexts.Any(c => c.StartsWith("update")));

                            // Sanity check on results
                            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.NonQueryExecuted).All(l => l.Result != null));
                            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.ReaderExecuted).All(l => l.Result != null));
#if !NET40
                            Assert.True(
                                logger.Log.Where(
                                    l => l.Method.ToString().StartsWith("Async")
                                         && l.Method.ToString().EndsWith("Executed")).All(l => l.Result != null));
#endif
                        }
                    }, executionCount);

            // Check that each execution logged exectly the same commands.

            Assert.Equal(executionCount, loggers.Count);

            var firstLog = loggers.First().Log;
            foreach (var log in loggers.Select(l => l.Log).Skip(1))
            {
                Assert.Equal(firstLog.Count, log.Count);

                for (var i = 0; i < log.Count; i++)
                {
                    Assert.Equal(firstLog[i].Method, log[i].Method);
                    Assert.Equal(firstLog[i].CommandText, log[i].CommandText);

                    if (firstLog[i].Result == null)
                    {
                        Assert.Null(log[i].Result);
                    }
                    else
                    {
                        Assert.Same(firstLog[i].Result.GetType(), log[i].Result.GetType());
                    }
                }
            }
        }

        public class BlogContextNoInit : BlogContext
        {
            static BlogContextNoInit()
            {
                Database.SetInitializer<BlogContextNoInit>(new BlogInitializer());
            }
        }

        public enum CommandMethod
        {
            NonQueryExecuting = 0,
            NonQueryExecuted,
            ReaderExecuting,
            ReaderExecuted,
            ScalarExecuting,
            ScalarExecuted,
            AsyncNonQueryExecuting,
            AsyncNonQueryExecuted,
            AsyncReaderExecuting,
            AsyncReaderExecuted,
            AsyncScalarExecuting,
            AsyncScalarExecuted,
        }

        public class CommandLogger : IDbCommandInterceptor
        {
            private readonly DbContext _context;
            private readonly IList<CommandLogItem> _log = new List<CommandLogItem>();

            public CommandLogger(DbContext context = null)
            {
                _context = context;
            }

            public IList<CommandLogItem> Log
            {
                get { return _log; }
            }

            public void NonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.NonQueryExecuting, command));
                }
            }

            public int NonQueryExecuted(DbCommand command, int result, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.NonQueryExecuted, command, result));
                }
                return result;
            }

            public void ReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ReaderExecuting, command));
                }
            }

            public DbDataReader ReaderExecuted(
                DbCommand command, CommandBehavior behavior, DbDataReader result, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ReaderExecuted, command, result));
                }

                return result;
            }

            public void ScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ScalarExecuting, command));
                }
            }

            public object ScalarExecuted(DbCommand command, object result, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ScalarExecuted, command, result));
                }

                return result;
            }

            public void AsyncNonQueryExecuting(DbCommand command, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.AsyncNonQueryExecuting, command));
                }
            }

            public Task<int> AsyncNonQueryExecuted(DbCommand command, Task<int> result, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.AsyncNonQueryExecuted, command, result));
                }

                return result;
            }

            public void AsyncReaderExecuting(DbCommand command, CommandBehavior behavior, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.AsyncReaderExecuting, command));
                }
            }

            public Task<DbDataReader> AsyncReaderExecuted(
                DbCommand command, CommandBehavior behavior, Task<DbDataReader> result, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.AsyncReaderExecuted, command, result));
                }

                return result;
            }

            public void AsyncScalarExecuting(DbCommand command, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.AsyncScalarExecuting, command));
                }
            }

            public Task<object> AsyncScalarExecuted(DbCommand command, Task<object> result, DbInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.AsyncScalarExecuted, command, result));
                }

                return result;
            }

            private bool ShouldLog(DbInterceptionContext interceptionContext)
            {
                return _context == null || interceptionContext.DbContexts.Contains(_context);
            }
        }

        public class CommandLogItem
        {
            public CommandLogItem(CommandMethod method, DbCommand command, object result = null)
            {
                Method = method;
                CommandText = command.CommandText;
                Result = result;
            }

            public CommandMethod Method { get; set; }
            public string CommandText { get; set; }
            public object Result { get; set; }
        }
    }
}
