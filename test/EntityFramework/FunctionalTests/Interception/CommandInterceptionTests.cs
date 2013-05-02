// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class CommandInterceptionTests : FunctionalTestBase
    {
        public CommandInterceptionTests()
        {
            using (var context = new BlogContextNoInit())
            {
                context.Database.Initialize(force: false);
            }
        }

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

            for (var i = 0; i < logger.Log.Count; i++)
            {
                var method = logger.Log[i].Method;
                commandsUsed[(int)method] = true;

                if (method.ToString().EndsWith("Executing"))
                {
                    Assert.Equal(method + 1, logger.Log[i + 1].Method);
                    Assert.Same(logger.Log[i].Command, logger.Log[i + 1].Command);
                }
            }

            // Check that every type of command used has log entries
            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuted]);
            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
            Assert.True(commandsUsed[(int)CommandMethod.ScalarExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.ScalarExecuted]);

            // Sanity check on command text
            var commandTexts = logger.Log.Select(l => l.CommandText.ToLowerInvariant());
            Assert.True(commandTexts.Any(c => c.StartsWith("select")));
            Assert.True(commandTexts.Any(c => c.StartsWith("create")));
            Assert.True(commandTexts.Any(c => c.StartsWith("alter")));
            Assert.True(commandTexts.Any(c => c.StartsWith("insert")));
        }

        public class BlogContextLogAll : BlogContext
        {
            static BlogContextLogAll()
            {
                Database.SetInitializer<BlogContextLogAll>(new BlogInitializer());
            }
        }

        [Fact]
        public void Commands_that_result_in_exceptions_are_still_intercepted()
        {
            var logger = new CommandLogger();
            Interception.AddInterceptor(logger);

            Exception exception;
            try
            {
                using (var context = new BlogContextNoInit())
                {
                    exception = Assert.Throws<SqlException>(() => context.Blogs.SqlQuery("select * from No.Chance").ToList());
                }
            }
            finally
            {
                Interception.RemoveInterceptor(logger);
            }

            Assert.Equal(2, logger.Log.Count);

            var executingLog = logger.Log[0];
            Assert.Equal(CommandMethod.ReaderExecuting, executingLog.Method);
            Assert.False(executingLog.InterceptionContext.IsAsync);
            Assert.Null(executingLog.Result);
            Assert.Null(executingLog.InterceptionContext.Exception);

            var executedLog = logger.Log[1];
            Assert.Equal(CommandMethod.ReaderExecuted, executedLog.Method);
            Assert.False(executedLog.InterceptionContext.IsAsync);
            Assert.Null(executedLog.Result);
            Assert.Same(exception, executedLog.InterceptionContext.Exception);
        }

#if !NET40
        [Fact]
        public void Async_commands_that_result_in_exceptions_are_still_intercepted()
        {
            var logger = new CommandLogger();
            Interception.AddInterceptor(logger);

            try
            {
                using (var context = new BlogContextNoInit())
                {
                    var query = context.Blogs.SqlQuery("select * from No.Chance").ToListAsync();
                    
                    Assert.Throws<AggregateException>(() => query.Wait());

                    Assert.True(query.IsFaulted);
                }
            }
            finally
            {
                Interception.RemoveInterceptor(logger);
            }

            Assert.Equal(2, logger.Log.Count);

            var executingLog = logger.Log[0];
            Assert.Equal(CommandMethod.ReaderExecuting, executingLog.Method);
            Assert.True(executingLog.InterceptionContext.IsAsync);
            Assert.Null(executingLog.Result);
            Assert.Null(executingLog.InterceptionContext.Exception);

            var executedLog = logger.Log[1];
            Assert.Equal(CommandMethod.ReaderExecuted, executedLog.Method);
            Assert.True(executedLog.InterceptionContext.IsAsync);
            Assert.Null(executedLog.Result);
            Assert.IsType<SqlException>(executedLog.InterceptionContext.Exception);
            Assert.True(executedLog.InterceptionContext.TaskStatus.HasFlag(TaskStatus.Faulted));
        }

        [Fact]
        public void Async_commands_that_are_canceled_are_still_intercepted()
        {
            var logger = new CommandLogger();
            Interception.AddInterceptor(logger);

            var cancellation = new CancellationTokenSource();
            var cancellationToken = cancellation.Token;

            try
            {
                using (var context = new BlogContextNoInit())
                {
                    context.Database.Connection.Open();

                    cancellation.Cancel();
                    var command = context.Database.ExecuteSqlCommandAsync("update Blogs set Title = 'No' where Id = -1", cancellationToken);

                    Assert.Throws<AggregateException>(() => command.Wait());
                    Assert.True(command.IsCanceled);

                    context.Database.Connection.Close();
                }
            }
            finally
            {
                Interception.RemoveInterceptor(logger);
            }

            Assert.Equal(2, logger.Log.Count);

            var executingLog = logger.Log[0];
            Assert.Equal(CommandMethod.NonQueryExecuting, executingLog.Method);
            Assert.True(executingLog.InterceptionContext.IsAsync);
            Assert.Null(executingLog.Result);
            Assert.Null(executingLog.InterceptionContext.Exception);

            var executedLog = logger.Log[1];
            Assert.Equal(CommandMethod.NonQueryExecuted, executedLog.Method);
            Assert.True(executedLog.InterceptionContext.IsAsync);
            Assert.Equal(0, executedLog.Result);
            Assert.Null(executingLog.InterceptionContext.Exception);
            Assert.True(executedLog.InterceptionContext.TaskStatus.HasFlag(TaskStatus.Canceled));
        }
#endif

        [Fact]
        public void Multiple_contexts_running_concurrently_can_use_interception()
        {
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

                            for (var i = 0; i < logger.Log.Count; i++)
                            {
                                var method = logger.Log[i].Method;
                                commandsUsed[(int)method] = true;

                                if (method.ToString().EndsWith("Executing"))
                                {
                                    Assert.Equal(method + 1, logger.Log[i + 1].Method);
                                    Assert.Same(logger.Log[i].Command, logger.Log[i + 1].Command);
                                }
                            }

                            // Check that expected command have log entries
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
#if !NET40
                            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuted]);
#endif

                            // Sanity check on command text
                            var commandTexts = logger.Log.Select(l => l.CommandText.ToLowerInvariant());
                            Assert.True(commandTexts.Any(c => c.StartsWith("select")));
                            Assert.True(commandTexts.Any(c => c.StartsWith("insert")));
#if !NET40
                            Assert.True(commandTexts.Any(c => c.StartsWith("update")));
#endif

                            // Sanity check on results
                            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.NonQueryExecuted).All(l => l.Result != null));
                            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.ReaderExecuted).All(l => l.Result != null));
                        }
                    }, executionCount);

            // Check that each execution logged exactly the same commands.

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

        public enum CommandMethod
        {
            NonQueryExecuting = 0,
            NonQueryExecuted,
            ReaderExecuting,
            ReaderExecuted,
            ScalarExecuting,
            ScalarExecuted
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

            public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.NonQueryExecuting, command, interceptionContext));
                }
            }

            public int NonQueryExecuted(DbCommand command, int result, DbCommandInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.NonQueryExecuted, command, interceptionContext, result));
                }
                return result;
            }

            public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ReaderExecuting, command, interceptionContext));
                }
            }

            public DbDataReader ReaderExecuted(
                DbCommand command, DbDataReader result, DbCommandInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ReaderExecuted, command, interceptionContext, result));
                }

                return result;
            }

            public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ScalarExecuting, command, interceptionContext));
                }
            }

            public object ScalarExecuted(DbCommand command, object result, DbCommandInterceptionContext interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(new CommandLogItem(CommandMethod.ScalarExecuted, command, interceptionContext, result));
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
            public CommandLogItem(
                CommandMethod method, 
                DbCommand command, 
                DbCommandInterceptionContext interceptionContext, 
                object result = null)
            {
                Method = method;
                CommandText = command.CommandText;
                Command = command;
                InterceptionContext = interceptionContext;
                Result = result;
            }

            public CommandMethod Method { get; set; }
            public string CommandText { get; set; }
            public DbCommand Command { get; set; }
            public DbCommandInterceptionContext InterceptionContext { get; set; }
            public object Result { get; set; }
        }
    }
}
