// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ProductivityApi
{
    using Xunit;

    public class HasChangesTests
    {
        public HasChangesTests()
        {
            //reduce the overhead for this tests
            Database.SetInitializer<SomeContext>(new NullDatabaseInitializer<SomeContext>());
        }

        [Fact]
        public void HasChanges_return_false_if_context_dont_have_changes()
        {
            using (var context = new SomeContext())
            {
                context.Foos.Attach(new Foo() { Bar = "bar" });

                Assert.False(context.ChangeTracker.HasChanges());
            }
        }


        [Fact]
        public void HasChanges_return_true_if_context_have_entities_in_added_state()
        {
            using (var context = new SomeContext())
            {
                var foo1 = new Foo() { Bar = "the foo" };

                var foo2 = new Foo() { Bar = "the foo" };

                context.Foos.AddRange(new[] { foo1, foo2 });
                Assert.True(context.ChangeTracker.HasChanges());
            }
        }

        [Fact]
        public void HasChanges_return_true_if_context_have_entities_in_deleted_state()
        {
            using (var context = new SomeContext())
            {
                var foo = new Foo() { Bar = "the foo" };

                context.Foos.Attach(foo);
                context.Entry(foo).State = EntityState.Deleted;

                Assert.True(context.ChangeTracker.HasChanges());
            }
        }

        [Fact]
        public void HasChanges_return_true_if_context_have_entities_in_modified_state()
        {
            using (var context = new SomeContext())
            {
                var foo = new Foo() { Bar = "the foo" };

                context.Entry(foo).State = EntityState.Modified;

                Assert.True(context.ChangeTracker.HasChanges());
            }
        }

        [Fact]
        public void HasChanges_detect_changes_for_non_proxy_types()
        {
            using (var context = new SomeContext())
            {
                var foo = new Foo() { Bar = "the foo" };

                context.Foos.Attach(foo);

                foo.Bar = "ops!";

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
