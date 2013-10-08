// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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

        [Fact] // CodePlex 1142
        public void Lazy_loading_works_for_tracking_proxies_with_two_nav_props_with_same_FK_when_one_FK_changes()
        {
            using (var context = new TwoIntoOneContext())
            {
                var booking = context.Bookings.Find(4);

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(1, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                booking.ModifiedById = 3;

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(3, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                // Trigger lazy loading
                Assert.Equal(1, booking.CreatedBy.Id);
                Assert.Equal(3, booking.ModifiedBy.Id);

                Assert.True(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.True(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);
            }
        }

        [Fact] // CodePlex 1142
        public void Lazy_loading_works_for_tracking_proxies_with_two_nav_props_with_different_FKs_when_one_FK_changes()
        {
            using (var context = new TwoIntoOneContext())
            {
                var booking = context.Bookings.Find(5);

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(2, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                booking.ModifiedById = 3;

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(3, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                // Trigger lazy loading
                Assert.Equal(1, booking.CreatedBy.Id);
                Assert.Equal(3, booking.ModifiedBy.Id);

                Assert.True(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.True(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);
            }
        }

        [Fact] // CodePlex 1142
        public void Lazy_loading_works_for_non_tracking_proxies_with_two_nav_props_with_same_FK_when_one_FK_changes()
        {
            using (var context = new TwoIntoOneContext())
            {
                var booking = context.BookingsNp.Find(4);

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(1, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                booking.ModifiedById = 3;

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(3, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                // Trigger lazy loading
                Assert.Equal(1, booking.CreatedBy.Id);
                Assert.Equal(3, booking.ModifiedBy.Id);

                Assert.True(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.True(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);
            }
        }

        [Fact] // CodePlex 1142
        public void Lazy_loading_works_for_non_tracking_proxies_with_two_nav_props_with_different_FKs_when_one_FK_changes()
        {
            using (var context = new TwoIntoOneContext())
            {
                var booking = context.BookingsNp.Find(5);

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(2, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                booking.ModifiedById = 3;

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(3, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                // Trigger lazy loading
                Assert.Equal(1, booking.CreatedBy.Id);
                Assert.Equal(3, booking.ModifiedBy.Id);

                Assert.True(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.True(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);
            }
        }

        [Fact] // CodePlex 1142
        public void Explicit_loading_works_for_non_tracking_proxies_with_two_nav_props_with_same_FK_when_one_FK_changes()
        {
            using (var context = new TwoIntoOneContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var booking = context.BookingsNp.Find(4);

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(1, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                booking.ModifiedById = 3;

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(3, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                context.Entry(booking).Reference(b => b.CreatedBy).Load();
                Assert.Equal(1, context.UsersNp.Local.Count(u => u.Id == 1));
                Assert.Equal(1, booking.CreatedBy.Id);

                context.Entry(booking).Reference(b => b.ModifiedBy).Load();
                Assert.Equal(1, context.UsersNp.Local.Count(u => u.Id == 3));
                Assert.Equal(3, booking.ModifiedBy.Id);

                Assert.True(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.True(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);
            }
        }

        [Fact] // CodePlex 1142
        public void Explicit_loading_works_for_non_tracking_proxies_with_two_nav_props_with_different_FKs_when_one_FK_changes()
        {
            using (var context = new TwoIntoOneContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var booking = context.BookingsNp.Find(5);

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(2, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                booking.ModifiedById = 3;

                Assert.Equal(1, booking.CreatedById);
                Assert.Equal(3, booking.ModifiedById);
                Assert.False(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.False(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);

                context.Entry(booking).Reference(b => b.CreatedBy).Load();
                Assert.Equal(1, context.UsersNp.Local.Count(u => u.Id == 1));
                Assert.Equal(1, booking.CreatedBy.Id);

                context.Entry(booking).Reference(b => b.ModifiedBy).Load();
                Assert.Equal(1, context.UsersNp.Local.Count(u => u.Id == 3));
                Assert.Equal(3, booking.ModifiedBy.Id);

                Assert.True(context.Entry(booking).Reference(b => b.CreatedBy).IsLoaded);
                Assert.True(context.Entry(booking).Reference(b => b.ModifiedBy).IsLoaded);
            }
        }

        public class TwoIntoOneContext : DbContext
        {
            static TwoIntoOneContext()
            {
                Database.SetInitializer(new TwoIntoOneInitializer());
            }

            public DbSet<Booking> Bookings { get; set; }
            public DbSet<User> Users { get; set; }
            public DbSet<BookingNp> BookingsNp { get; set; }
            public DbSet<UserNp> UsersNp { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Booking>()
                    .HasRequired(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .WillCascadeOnDelete(false);

                modelBuilder
                    .Entity<Booking>()
                    .HasRequired(e => e.ModifiedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ModifiedById)
                    .WillCascadeOnDelete(false);

                modelBuilder
                    .Entity<BookingNp>()
                    .HasRequired(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .WillCascadeOnDelete(false);

                modelBuilder
                    .Entity<BookingNp>()
                    .HasRequired(e => e.ModifiedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ModifiedById)
                    .WillCascadeOnDelete(false);
            }
        }

        public class TwoIntoOneInitializer : DropCreateDatabaseIfModelChanges<TwoIntoOneContext>
        {
            protected override void Seed(TwoIntoOneContext context)
            {
                context.Users.AddRange(new[] { new User { Id = 1 }, new User { Id = 2 }, new User { Id = 3 } });
                context.Bookings.AddRange(
                    new[]
                        {
                            new Booking { Id = 4, CreatedById = 1, ModifiedById = 1 },
                            new Booking { Id = 5, CreatedById = 1, ModifiedById = 2 }
                        });

                context.UsersNp.AddRange(new[] { new UserNp { Id = 1 }, new UserNp { Id = 2 }, new UserNp { Id = 3 } });
                context.BookingsNp.AddRange(
                    new[]
                        {
                            new BookingNp { Id = 4, CreatedById = 1, ModifiedById = 1 },
                            new BookingNp { Id = 5, CreatedById = 1, ModifiedById = 2 }
                        });
            }
        }

        public class Booking
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public virtual long Id { get; set; }

            public virtual long CreatedById { get; set; }
            public virtual User CreatedBy { get; set; }

            public virtual long ModifiedById { get; set; }
            public virtual User ModifiedBy { get; set; }
        }

        public class User
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public virtual long Id { get; set; }
        }

        public class BookingNp
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public long Id { get; set; }

            public long CreatedById { get; set; }
            public virtual UserNp CreatedBy { get; set; }

            public long ModifiedById { get; set; }
            public virtual UserNp ModifiedBy { get; set; }
        }

        public class UserNp
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public long Id { get; set; }
        }
    }
}
