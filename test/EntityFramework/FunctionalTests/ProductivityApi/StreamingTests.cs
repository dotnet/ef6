// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using ConcurrencyModel;
    using Xunit;

    /// <summary>
    /// Tests for the AsStreaming extension methods on IQueryable.
    /// </summary>
    public class StreamingTests : FunctionalTestBase
    {
        #region Tests for AsStreaming on DbSet/DbQuery

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Generic_AsStreaming_can_be_used_on_IQueryable()
        {
            using (var context = new F1Context())
            {
                var winners = context.Drivers.Where(d => d.Wins > 20);

                var count = 0;
                foreach (var winner in winners)
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in winners.AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Generic_AsStreaming_can_be_used_directly_on_DbSet()
        {
            using (var context = new F1Context())
            {
                var drivers = context.Drivers;

                var count = 0;
                foreach (var driver in drivers)
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(42, count);

                count = 0;
                foreach (var driver in drivers.AsStreaming())
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(42, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Generic_AsStreaming_can_be_used_directly_on_DbQuery()
        {
            using (var context = new F1Context())
            {
                var winners = (DbQuery<Driver>)context.Drivers.Where(d => d.Wins > 20);

                var count = 0;
                foreach (var winner in winners)
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in winners.AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Non_generic_AsStreaming_can_be_used_on_IQueryable()
        {
            using (var context = new F1Context())
            {
                var expression = context.Drivers.Where(d => d.Wins > 20).Expression;
                var query = ((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery(expression);
                
                var count = 0;
                foreach (var winner in query)
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in query.AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Non_generic_AsStreaming_can_be_used_directly_on_DbSet()
        {
            using (var context = new F1Context())
            {
                var drivers = context.Set(typeof(Driver));

                var count = 0;
                foreach (var driver in drivers)
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(42, count);

                count = 0;
                foreach (var driver in drivers.AsStreaming())
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(42, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Non_generic_AsStreaming_can_be_used_directly_on_DbQuery()
        {
            using (var context = new F1Context())
            {
                var expression = context.Drivers.Where(d => d.Wins > 20).Expression;
                var query = (DbQuery)((IQueryable)context.Set(typeof(Driver))).Provider.CreateQuery(expression);

                var count = 0;
                foreach (var winner in query)
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in query.AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Non_generic_AsStreaming_can_be_used_on_generic_IQueryable()
        {
            using (var context = new F1Context())
            {
                var winners = (IQueryable)context.Drivers.Where(d => d.Wins > 20);

                var count = 0;
                foreach (var winner in winners)
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in winners.AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Non_generic_AsStreaming_can_be_used_directly_on_generic_DbSet()
        {
            using (var context = new F1Context())
            {
                var drivers = (IQueryable)context.Drivers;

                var count = 0;
                foreach (var driver in drivers)
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(42, count);

                count = 0;
                foreach (var driver in drivers.AsStreaming())
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(42, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void AsStreaming_can_be_used_before_the_rest_of_the_query()
        {
            using (var context = new F1Context())
            {
                var drivers = context.Drivers;

                var count = 0;
                foreach (var driver in drivers.Where(d => d.Wins > 20))
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var driver in drivers.AsStreaming().Where(d => d.Wins > 20))
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }
        
        #endregion

        #region Tests for AsStreaming on ObjectSet/ObjectQuery

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Generic_ObjectSet_AsStreaming_can_be_used_on_IQueryable()
        {
            using (var context = new F1Context())
            {
                var winners = CreateObjectSet<Driver>(context).Where(d => d.Wins > 20);

                var count = 0;
                foreach (var winner in winners)
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in winners.AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Generic_ObjectSet_AsStreaming_can_be_used_directly_on_ObjectSet()
        {
            using (var context = new F1Context())
            {
                var drivers = CreateObjectSet<Driver>(context);

                var count = 0;
                foreach (var driver in drivers)
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(42, count);

                count = 0;
                foreach (var driver in drivers.AsStreaming())
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(42, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Non_generic_ObjectSet_AsStreaming_can_be_used_on_generic_IQueryable()
        {
            using (var context = new F1Context())
            {
                var winners = (IQueryable)CreateObjectSet<Driver>(context).Where(d => d.Wins > 20);

                var count = 0;
                foreach (var winner in winners)
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in winners.AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Non_generic_ObjectSet_AsStreaming_can_be_used_directly_on_generic_ObjectSet()
        {
            using (var context = new F1Context())
            {
                var drivers = (IQueryable)CreateObjectSet<Driver>(context);

                var count = 0;
                foreach (var driver in drivers)
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(42, count);

                count = 0;
                foreach (var driver in drivers.AsStreaming())
                {
                    Assert.NotNull(driver);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(42, count);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void ObjectSet_AsStreaming_can_be_used_before_the_rest_of_the_query()
        {
            using (var context = new F1Context())
            {
                var drivers = CreateObjectSet<Driver>(context);

                var count = 0;
                foreach (var winner in drivers.Where(d => d.Wins > 20))
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                    count++;
                }
                Assert.Equal(2, count);

                count = 0;
                foreach (var winner in drivers.Where(d => d.Wins > 20).AsStreaming())
                {
                    Assert.NotNull(winner);
                    Assert.True(context.Database.Connection.State == ConnectionState.Open);
                    count++;
                }
                Assert.Equal(2, count);
            }
        }

        private ObjectSet<TEntity> CreateObjectSet<TEntity>(DbContext context) where TEntity : class
        {
            return GetObjectContext(context).CreateObjectSet<TEntity>();
        }

        #endregion
    }
}
