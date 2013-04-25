// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ProductivityApi
{
    using Xunit;

    public class HasChangesTests : FunctionalTestBase
    {
        public HasChangesTests()
        {
            //reduce the overhead for this tests
            Database.SetInitializer<SomeContext>(null);
        }

        [Fact]
        public void HasChanges_return_false_if_context_dont_have_changes()
        {
            using (var context = new SomeContext())
            {
                context.Foos.Attach(new Foo { Bar = "bar" });

                Assert.False(context.ChangeTracker.HasChanges());
            }
        }

        [Fact]
        public void HasChanges_return_true_if_context_have_entities_in_added_state()
        {
            using (var context = new SomeContext())
            {
                context.Foos.AddRange(new[] { new Foo { Bar = "the foo" }, new Foo { Bar = "the foo" } });
                
                Assert.True(context.ChangeTracker.HasChanges());
            }
        }

        [Fact]
        public void HasChanges_return_true_if_context_have_entities_in_deleted_state()
        {
            using (var context = new SomeContext())
            {
                context.Entry(new Foo { Bar = "the foo" }).State = EntityState.Deleted;

                Assert.True(context.ChangeTracker.HasChanges());
            }
        }

        [Fact]
        public void HasChanges_return_true_if_context_have_entities_in_modified_state()
        {
            using (var context = new SomeContext())
            {
                context.Entry(new Foo { Bar = "the foo" }).State = EntityState.Modified;

                Assert.True(context.ChangeTracker.HasChanges());
            }
        }

        [Fact]
        public void HasChanges_does_not_call_DetectChanges_if_it_has_been_disabled()
        {
            using (var context = new SomeContext())
            {
                var foo = context.Foos.Attach(new Foo { Bar = "the foo" });

                context.Configuration.AutoDetectChangesEnabled = false;

                foo.Bar = "the bar";

                Assert.False(context.ChangeTracker.HasChanges());

                context.ChangeTracker.DetectChanges();

                Assert.True(context.ChangeTracker.HasChanges());
            }
        }
    }

    internal class SomeContext
        : DbContext
    {
        public DbSet<Foo> Foos { get; set; }
    }

    internal class Foo
    {
        public int Id { get; set; }
        public string Bar { get; set; }
    }
}
