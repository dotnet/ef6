// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using ConcurrencyModel;
    using Xunit;

    /// <summary>
    ///     Tests for the AsNoTracking extension methods on IQueryable.
    /// </summary>
    public class NoTrackingTests : FunctionalTestBase
    {
        #region Tests for AsNoTracking on DbSet/DbQuery

        [Fact]
        public void Generic_AsNoTracking_can_be_used_on_IQueryable()
        {
            using (var context = new F1Context())
            {
                var webber = context.Drivers.Where(d => d.Name == "Mark Webber").AsNoTracking().Single();

                Assert.Equal("Mark Webber", webber.Name);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Generic_AsNoTracking_can_be_used_directly_on_DbSet()
        {
            using (var context = new F1Context())
            {
                var drivers = context.Drivers.AsNoTracking().ToList();

                Assert.True(drivers.Count > 0);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Generic_AsNoTracking_can_be_used_directly_on_DbQuery()
        {
            using (var context = new F1Context())
            {
                var webber =
                    ((DbQuery<Driver>)context.Drivers.Where(d => d.Name == "Mark Webber")).AsNoTracking().Single();

                Assert.Equal("Mark Webber", webber.Name);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Non_generic_AsNoTracking_can_be_used_on_IQueryable()
        {
            using (var context = new F1Context())
            {
                var expression = context.Drivers.Where(d => d.Name == "Mark Webber").Expression;
                var query = ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery(expression);

                var webber = query.AsNoTracking().ToList<Driver>().Single();

                Assert.Equal("Mark Webber", webber.Name);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Non_generic_AsNoTracking_can_be_used_directly_on_DbSet()
        {
            using (var context = new F1Context())
            {
                var drivers = context.Set(typeof(Driver)).AsNoTracking().ToList<Driver>();

                Assert.True(drivers.Count > 0);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Non_generic_AsNoTracking_can_be_used_directly_on_DbQuery()
        {
            using (var context = new F1Context())
            {
                var expression = context.Drivers.Where(d => d.Name == "Mark Webber").Expression;
                var query = ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery(expression);

                var webber = ((DbQuery)query).AsNoTracking().ToList<Driver>().Single();

                Assert.Equal("Mark Webber", webber.Name);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Non_generic_AsNoTracking_can_be_used_on_generic_IQueryable()
        {
            using (var context = new F1Context())
            {
                var webber =
                    ((IQueryable)context.Drivers.Where(d => d.Name == "Mark Webber")).AsNoTracking().Cast<Driver>().
                        Single();

                Assert.Equal("Mark Webber", webber.Name);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Non_generic_AsNoTracking_can_be_used_directly_on_generic_DbSet()
        {
            using (var context = new F1Context())
            {
                var drivers = ((IQueryable)context.Drivers).AsNoTracking().ToList<Driver>();

                Assert.True(drivers.Count > 0);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void AsNoTracking_can_be_used_before_the_rest_of_the_query()
        {
            using (var context = new F1Context())
            {
                var drivers = context.Drivers.AsNoTracking().Where(d => d.Wins > 0).ToList();

                Assert.Equal(13, drivers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void AsNoTracking_can_be_used_after_the_rest_of_the_query()
        {
            using (var context = new F1Context())
            {
                var drivers = context.Drivers.Where(d => d.Wins > 0).AsNoTracking().ToList();

                Assert.Equal(13, drivers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void AsNoTracking_does_not_change_the_state_of_the_underlying_set()
        {
            using (var context = new F1Context())
            {
                var set = context.Drivers;
                var drivers = set.AsNoTracking().ToList();

                Assert.True(drivers.Count > 0);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                drivers = set.ToList();
                Assert.True(drivers.Count > 0);
                Assert.Equal(drivers.Count, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void AsNoTracking_does_not_change_the_state_of_the_original_query()
        {
            using (var context = new F1Context())
            {
                var query = context.Drivers.Where(d => d.Wins > 0);
                var drivers = query.AsNoTracking().ToList();

                Assert.Equal(13, drivers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                drivers = query.ToList();
                Assert.True(drivers.Count > 0);
                Assert.Equal(drivers.Count, context.ChangeTracker.Entries().Count());
            }
        }

        #endregion

        #region Tests for AsNoTracking on ObjectSet/ObjectQuery

        [Fact]
        public void Generic_ObjectSet_AsNoTracking_can_be_used_on_IQueryable()
        {
            using (var context = new F1Context())
            {
                var webber =
                    CreateObjectSet<Driver>(context).Where(d => d.Name == "Mark Webber").AsNoTracking().Single();

                Assert.Equal("Mark Webber", webber.Name);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Generic_ObjectSet_AsNoTracking_can_be_used_directly_on_ObjectSet()
        {
            using (var context = new F1Context())
            {
                var drivers = CreateObjectSet<Driver>(context).AsNoTracking().ToList();

                Assert.True(drivers.Count > 0);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Non_generic_ObjectSet_AsNoTracking_can_be_used_on_generic_IQueryable()
        {
            using (var context = new F1Context())
            {
                var webber =
                    ((IQueryable)CreateObjectSet<Driver>(context).Where(d => d.Name == "Mark Webber")).AsNoTracking().
                        Cast<Driver>().Single();

                Assert.Equal("Mark Webber", webber.Name);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void Non_generic_ObjectSet_AsNoTracking_can_be_used_directly_on_generic_ObjectSet()
        {
            using (var context = new F1Context())
            {
                var drivers = ((IQueryable)CreateObjectSet<Driver>(context)).AsNoTracking().ToList<Driver>();

                Assert.True(drivers.Count > 0);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void ObjectSet_AsNoTracking_can_be_used_before_the_rest_of_the_query()
        {
            using (var context = new F1Context())
            {
                var drivers = CreateObjectSet<Driver>(context).AsNoTracking().Where(d => d.Wins > 0).ToList();

                Assert.Equal(13, drivers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void ObjectSet_AsNoTracking_can_be_used_after_the_rest_of_the_query()
        {
            using (var context = new F1Context())
            {
                var drivers = CreateObjectSet<Driver>(context).Where(d => d.Wins > 0).AsNoTracking().ToList();

                Assert.Equal(13, drivers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void ObjectSet_AsNoTracking_does_not_change_the_state_of_the_underlying_set()
        {
            using (var context = new F1Context())
            {
                var set = CreateObjectSet<Driver>(context);
                var drivers = set.AsNoTracking().ToList();

                Assert.True(drivers.Count > 0);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                drivers = set.ToList();
                Assert.True(drivers.Count > 0);
                Assert.Equal(drivers.Count, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public void ObjectSet_AsNoTracking_does_not_change_the_state_of_the_original_query()
        {
            using (var context = new F1Context())
            {
                var query = CreateObjectSet<Driver>(context).Where(d => d.Wins > 0);
                var drivers = query.AsNoTracking().ToList();

                Assert.Equal(13, drivers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());

                drivers = query.ToList();
                Assert.True(drivers.Count > 0);
                Assert.Equal(drivers.Count, context.ChangeTracker.Entries().Count());
            }
        }

        private ObjectSet<TEntity> CreateObjectSet<TEntity>(DbContext context) where TEntity : class
        {
            return GetObjectContext(context).CreateObjectSet<TEntity>();
        }

        #endregion
    }
}
