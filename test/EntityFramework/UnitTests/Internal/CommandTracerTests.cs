// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Linq;
    using ProductivityApiTests;
    using Xunit;

    public class CommandTracerTests
    {
        [Fact]
        public void Can_trace_commands_and_execution_cancelled()
        {
            using (var context = new ChangeTrackingProxyTests.YummyContext())
            {
                context.Products.Add(
                    new ChangeTrackingProxyTests.YummyProduct
                        {
                            Id = 1,
                            Name = "Pineapple Lumps"
                        });

                CommandTracer commandTracer;

                using (commandTracer = new CommandTracer(context))
                {
                    context.SaveChanges();

                    var interceptedCommand = commandTracer.DbCommands.Single();

                    Assert.Equal(
                        "insert [dbo].[YummyProducts]([Id], [Name])\r\nvalues (@0, @1)\r\n",
                        interceptedCommand.CommandText);

                    Assert.Equal(2, interceptedCommand.Parameters.Count);
                }

                context.Products.Add(
                    new ChangeTrackingProxyTests.YummyProduct
                        {
                            Id = 2,
                            Name = "Orange squash gums"
                        });

                context.SaveChanges();

                Assert.Equal(1, commandTracer.DbCommands.Count());
                Assert.Null(context.Products.SingleOrDefault(p => p.Id == 1));
                Assert.NotNull(context.Products.SingleOrDefault(p => p.Id == 2));
            }
        }

        [Fact]
        public void Can_trace_command_trees()
        {
            using (var context = new ChangeTrackingProxyTests.YummyContext())
            {
                context.Products.Add(
                    new ChangeTrackingProxyTests.YummyProduct
                    {
                        Id = 1,
                        Name = "Pineapple Lumps"
                    });
                
                using (var commandTracer = new CommandTracer(context))
                {
                    context.SaveChanges();

                    var interceptedCommandTree = commandTracer.CommandTrees.Single();

                    Assert.IsType<DbInsertCommandTree>(interceptedCommandTree);
                }
            }
        }
    }
}
