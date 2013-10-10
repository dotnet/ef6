// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Linq;
    using Xunit;

    public class CommandTracerTests
    {
        [Fact]
        public void Can_trace_commands_and_execution_cancelled()
        {
            using (var context = new TracerContext())
            {
                context.TracerEntities.Add(
                    new TracerEntity
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
                        "INSERT [dbo].[TracerEntities]([Id], [Name])\r\nVALUES (@0, @1)\r\n",
                        interceptedCommand.CommandText);

                    Assert.Equal(2, interceptedCommand.Parameters.Count);
                }

                context.TracerEntities.Add(
                    new TracerEntity
                        {
                            Id = 2,
                            Name = "Orange squash gums"
                        });

                context.SaveChanges();

                Assert.Equal(1, commandTracer.DbCommands.Count());
                Assert.Null(context.TracerEntities.SingleOrDefault(p => p.Id == 1));
                Assert.NotNull(context.TracerEntities.SingleOrDefault(p => p.Id == 2));
            }
        }

        [Fact]
        public void Can_trace_command_trees()
        {
            using (var context = new TracerContext())
            {
                context.TracerEntities.Add(
                    new TracerEntity
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

        private class TracerContext : DbContext
        {
            public TracerContext()
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<TracerContext>());
            }

            public DbSet<TracerEntity> TracerEntities { get; set; }
        }

        private class TracerEntity
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public virtual int Id { get; set; }

            public virtual string Name { get; set; }
        }
    }
}
