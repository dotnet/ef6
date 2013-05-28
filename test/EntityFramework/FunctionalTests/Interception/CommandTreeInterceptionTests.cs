// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class CommandTreeInterceptionTests : FunctionalTestBase
    {
        [Fact]
        public void Command_trees_for_initialization_and_simple_query_and_update_commands_can_be_logged()
        {
            // Make sure that HistoryContext has been used to ensure test runs consistently regardless of
            // whether or not other tests have run before it.
            using (var initContext = new BlogContextNoInit())
            {
                initContext.Database.Initialize(force: false);
            }

            var logger = new CommandTreeLogger();
            Interception.AddInterceptor(logger);

            BlogContextLogAll context;
            try
            {
                using (context = new BlogContextLogAll())
                {
                    BlogContext.DoStuff(context);
                }
            }
            finally
            {
                Interception.RemoveInterceptor(logger);
            }

            // Sanity check that we got tree creations logged
            Assert.True(logger.Log.OfType<DbQueryCommandTree>().Any(t => t.DataSpace == DataSpace.CSpace));
            Assert.True(logger.Log.OfType<DbQueryCommandTree>().Any(t => t.DataSpace == DataSpace.SSpace));
            Assert.True(logger.Log.OfType<DbInsertCommandTree>().Any(t => t.DataSpace == DataSpace.SSpace));
#if !NET40
            Assert.True(logger.Log.OfType<DbUpdateCommandTree>().Any(t => t.DataSpace == DataSpace.SSpace));

            Assert.True(
                logger.Log.OfType<DbUpdateCommandTree>()
                      .Any(
                          t => t.SetClauses.OfType<DbSetClause>()
                                .Any(
                                    c => ((DbPropertyExpression)c.Property).Property.Name == "Title"
                                         && (string)((DbConstantExpression)c.Value).Value == "I'm a logger and I'm okay...")));
#endif
            
            foreach (var interceptionContext in logger.LogWithContext.Select(t => t.Item2))
            {
                Assert.Contains(context, interceptionContext.DbContexts);
            }
        }

        public class BlogContextLogAll : BlogContext
        {
            static BlogContextLogAll()
            {
                Database.SetInitializer<BlogContextLogAll>(new BlogInitializer());
            }
        }

        [Fact]
        public void Multiple_contexts_running_concurrently_can_log_command_trees_except_trees_for_cached_queries()
        {
            // Make sure no logs get initialization trees
            using (var context = new BlogContextAllTrees())
            {
                context.Database.Initialize(force: false);
            }

            // Run the test code once to log both update and query trees
            using (var context = new BlogContextAllTrees())
            {
                var logger = new CommandTreeLogger(context);
                Interception.AddInterceptor(logger);

                try
                {
                    BlogContext.DoStuff(context);
                }
                finally
                {
                    Interception.RemoveInterceptor(logger);
                }

#if NET40
                Assert.Equal(7, logger.Log.Count);
#else
                Assert.Equal(8, logger.Log.Count);
#endif

                Assert.True(logger.Log.OfType<DbQueryCommandTree>().Any(t => t.DataSpace == DataSpace.CSpace));
                Assert.True(logger.Log.OfType<DbInsertCommandTree>().Any(t => t.DataSpace == DataSpace.SSpace));
#if !NET40
                Assert.True(logger.Log.OfType<DbUpdateCommandTree>().Any(t => t.DataSpace == DataSpace.SSpace));
#endif
            }

            // Now run again multiple times concurrently--only update trees logged
            var loggers = new ConcurrentBag<CommandTreeLogger>();

            const int executionCount = 5;
            ExecuteInParallel(
                () =>
                    {
                        using (var context = new BlogContextAllTrees())
                        {
                            var logger = new CommandTreeLogger(context);
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
                        }
                    }, executionCount);

            Assert.Equal(executionCount, loggers.Count);

            foreach (var logger in loggers)
            {
#if NET40
                Assert.Equal(1, logger.Log.Count);
#else
                Assert.Equal(2, logger.Log.Count);
#endif

                Assert.False(logger.Log.OfType<DbQueryCommandTree>().Any(t => t.DataSpace == DataSpace.CSpace));
                Assert.True(logger.Log.OfType<DbInsertCommandTree>().Any(t => t.DataSpace == DataSpace.SSpace));
#if !NET40
                Assert.True(logger.Log.OfType<DbUpdateCommandTree>().Any(t => t.DataSpace == DataSpace.SSpace));
#endif
            }
        }

        public class BlogContextAllTrees : BlogContext
        {
            static BlogContextAllTrees()
            {
                Database.SetInitializer<BlogContextAllTrees>(new BlogInitializer());
            }
        }

        public class CommandTreeLogger : IDbCommandTreeInterceptor
        {
            private readonly DbContext _context;
            private readonly IList<Tuple<DbCommandTree, DbCommandTreeInterceptionContext>> _log 
                = new List<Tuple<DbCommandTree, DbCommandTreeInterceptionContext>>();

            public CommandTreeLogger(DbContext context = null)
            {
                _context = context;
            }

            public IList<DbCommandTree> Log
            {
                get { return _log.Select(l => l.Item1).ToList(); }
            }

            public IList<Tuple<DbCommandTree, DbCommandTreeInterceptionContext>> LogWithContext
            {
                get { return _log; }
            }

            public void TreeCreated(DbCommandTree commandTree, DbCommandTreeInterceptionContext interceptionContext)
            {
                Assert.NotEmpty(interceptionContext.DbContexts);
                
                if (_context == null
                    || interceptionContext.DbContexts.Contains(_context))
                {
                    _log.Add(Tuple.Create(commandTree, interceptionContext));
                }
            }
        }
    }
}
