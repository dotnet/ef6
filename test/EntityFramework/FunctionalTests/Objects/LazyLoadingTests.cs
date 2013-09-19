// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using ConcurrencyModel;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class LazyLoadingTests : FunctionalTestBase
    {
        [Fact]
        public void Lazy_loading_of_entity_reference_does_not_work_on_detached_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.Detach(team);

                Assert.Null(team.Engine);
            }
        }

        [Fact]
        public void Lazy_loading_entity_collection_does_not_work_on_detached_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.Detach(team);

                Assert.Equal(0, team.Drivers.Count);
            }
        }

        [Fact]
        public void Lazy_loading_of_entity_reference_does_not_work_on_deleted_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.DeleteObject(team);

                objectContext.DetectChanges();

                Assert.Null(team.Engine);
            }
        }

        [Fact]
        public void Lazy_loading_entity_collection_does_not_work_on_deleted_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.DeleteObject(team);

                Assert.Equal(0, team.Drivers.Count);
            }
        }

        [Fact]
        public void Lazy_loading_of_entity_reference_works_on_modified_entity()
        {
            using (var context = new F1Context())
            {
                var teamId = context.Teams.OrderBy(t => t.Id).AsNoTracking().FirstOrDefault().Id;
                var engineId = context.Teams.Where(t => t.Id == teamId).Select(t => t.Engine).AsNoTracking().FirstOrDefault().Id;

                var team = context.Teams.Where(t => t.Id == teamId).AsNoTracking().Single();
                team.Constructor = "Fooblearius Fooblebar";

                Assert.NotNull(team.Engine);
            }
        }

        [Fact]
        public void Lazy_loading_entity_collection_works_on_modified_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                team.Constructor = "Fooblearius Fooblebar";

                Assert.True(team.Drivers.Count > 0);
            }
        }

        [Fact]
        public void Lazy_loading_does_not_occur_in_the_middle_of_materialization()
        {
            using (var context = new F1Context())
            {
                var teams = context.Teams.OrderBy(t => t.Id).Take(10);
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.ObjectStateManager.ObjectStateManagerChanged += ObjectStateManager_ObjectStateManagerChanged;

                foreach (var team in teams)
                {
                    Assert.True(context.Configuration.LazyLoadingEnabled == true);
                }
            }
        }

        void ObjectStateManager_ObjectStateManagerChanged(object sender, ComponentModel.CollectionChangeEventArgs e)
        {
            Assert.True(((Team)e.Element).Drivers.Count == 0);
        }

        [Fact] // CodePlex 735
        public void Changing_an_FK_does_not_cause_failure_to_load_an_unrelated_navigation_property()
        {
            using (var context = new Context735())
            {
                var child = context.Children.First();

                var parentReference = context.Entry(child).Reference(e => e.Parent);
                var otherReference = context.Entry(child).Reference(e => e.Other);

                Assert.Null(child.ParentId);
                Assert.Equal(1, child.OtherId);
                Assert.True(parentReference.IsLoaded); // FK in database is null => nothing to load
                Assert.False(otherReference.IsLoaded);

                context.Configuration.LazyLoadingEnabled = false;
                Assert.Null(child.Parent);
                Assert.Null(child.Other);
                context.Configuration.LazyLoadingEnabled = true;

                child.ParentId = 1;

                // Lazy load other
                Assert.Equal(1, child.Other.Id);

                Assert.True(parentReference.IsLoaded); // DetectChanges has not yet been called
                Assert.True(otherReference.IsLoaded);

                context.Configuration.LazyLoadingEnabled = false;
                Assert.Null(child.Parent);
                context.Configuration.LazyLoadingEnabled = true;

                context.ChangeTracker.DetectChanges();

                Assert.False(parentReference.IsLoaded); // FK has changed, so IsLoaded reset
                Assert.True(otherReference.IsLoaded);

                // Lazy load parent should now work
                Assert.Equal(1, child.Parent.Id);

                Assert.True(parentReference.IsLoaded);
                Assert.True(otherReference.IsLoaded);
            }
        }

        public class Context735 : DbContext
        {
            static Context735()
            {
                Database.SetInitializer(new Context735Initializer());
            }

            public DbSet<Parent> Parents { get; set; }
            public DbSet<Child> Children { get; set; }
            public DbSet<Other> Others { get; set; }
        }

        public class Context735Initializer : DropCreateDatabaseIfModelChanges<Context735>
        {
            protected override void Seed(Context735 context)
            {
                context.Others.Add(new Other { Id = 1, Name = "Other 1" });
                context.Parents.Add(new Parent { Id = 1, Name = "Parent 1" });
                context.Children.Add(new Child { Id = 1, OtherId = 1, Name = "Child 1" });
            }
        }

        public class Parent
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Name { get; set; }

            public virtual ICollection<Child> Children { get; set; }
        }

        public class Child
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Name { get; set; }

            public int? ParentId { get; set; }
            public virtual Parent Parent { get; set; }

            public int OtherId { get; set; }
            public virtual Other Other { get; set; }
        }

        public class Other
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Name { get; set; }

            public virtual ICollection<Child> Childs { get; set; }
        }
    }
}
