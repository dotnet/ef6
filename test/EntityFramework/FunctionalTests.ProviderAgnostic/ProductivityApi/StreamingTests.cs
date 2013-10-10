// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using ConcurrencyModel;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Configuration;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.TestModels.ProviderAgnosticModel;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Tests for the AsStreaming extension methods on IQueryable.
    /// </summary>
    public class StreamingTests
    {
        #region Tests for AsStreaming on DbSet/DbQuery

        [Fact]
        public void Generic_AsStreaming_can_be_used_on_IQueryable()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var nonPrivates = context.Gears.Where(g => g.Rank > MilitaryRank.Private);
                    var count = 0;
                    foreach (var nonPrivate in nonPrivates)
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var nonPrivate in nonPrivates.AsStreaming())
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Generic_AsStreaming_can_be_used_directly_on_DbSet()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var gears = context.Gears;

                    var count = 0;
                    foreach (var gear in gears)
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(5, count);

                    count = 0;
                    foreach (var gear in gears.AsStreaming())
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(5, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Generic_AsStreaming_can_be_used_directly_on_DbQuery()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var nonPrivates = (DbQuery<Gear>)context.Gears.Where(g => g.Rank > MilitaryRank.Private);

                    var count = 0;
                    foreach (var nonPrivate in nonPrivates)
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var nonPrivate in nonPrivates.AsStreaming())
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Non_generic_AsStreaming_can_be_used_on_IQueryable()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var expression = context.Gears.Where(g => g.Rank > MilitaryRank.Private).Expression;
                    var query = ((IQueryable)context.Set(typeof(Gear))).Provider.CreateQuery(expression);

                    var count = 0;
                    foreach (var result in query)
                    {
                        Assert.NotNull(result);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var result in query.AsStreaming())
                    {
                        Assert.NotNull(result);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Non_generic_AsStreaming_can_be_used_directly_on_DbSet()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var gears = context.Set(typeof(Gear));

                    var count = 0;
                    foreach (var gear in gears)
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(5, count);

                    count = 0;
                    foreach (var gear in gears.AsStreaming())
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(5, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Non_generic_AsStreaming_can_be_used_directly_on_DbQuery()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var expression = context.Gears.Where(g => g.Rank > MilitaryRank.Private).Expression;
                    var query = (DbQuery)((IQueryable)context.Set(typeof(Gear))).Provider.CreateQuery(expression);
                    var count = 0;
                    foreach (var result in query)
                    {
                        Assert.NotNull(result);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var gear in query.AsStreaming())
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Non_generic_AsStreaming_can_be_used_on_generic_IQueryable()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var nonPrivates = (IQueryable)context.Gears.Where(g => g.Rank > MilitaryRank.Private);
                    var count = 0;
                    foreach (var nonPrivate in nonPrivates)
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var nonPrivate in nonPrivates.AsStreaming())
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Non_generic_AsStreaming_can_be_used_directly_on_generic_DbSet()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var gears = (IQueryable)context.Gears;

                    var count = 0;
                    foreach (var gear in gears)
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(5, count);

                    count = 0;
                    foreach (var gear in gears.AsStreaming())
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(5, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void AsStreaming_can_be_used_before_the_rest_of_the_query()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var gears = context.Gears;

                    var count = 0;
                    foreach (var nonPrivate in gears.Where(g => g.Rank > MilitaryRank.Private))
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var nonPrivate in gears.AsStreaming().Where(g => g.Rank > MilitaryRank.Private))
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        #endregion

        #region Tests for AsStreaming on ObjectSet/ObjectQuery

        [Fact]
        public void Generic_ObjectSet_AsStreaming_can_be_used_on_IQueryable()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var nonPrivates = CreateObjectSet<Gear>(context).Where(g => g.Rank > MilitaryRank.Private);

                    var count = 0;
                    foreach (var nonPrivate in nonPrivates)
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var nonPrivate in nonPrivates.AsStreaming())
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Generic_ObjectSet_AsStreaming_can_be_used_directly_on_ObjectSet()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var gears = CreateObjectSet<Gear>(context);

                    var count = 0;
                    foreach (var gear in gears)
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(5, count);

                    count = 0;
                    foreach (var gear in gears.AsStreaming())
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(5, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Non_generic_ObjectSet_AsStreaming_can_be_used_on_generic_IQueryable()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var nonPrivates = (IQueryable)CreateObjectSet<Gear>(context).Where(g => g.Rank > MilitaryRank.Private);

                    var count = 0;
                    foreach (var nonPrivate in nonPrivates)
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }
                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var nonPrivate in nonPrivates.AsStreaming())
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }
                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void Non_generic_ObjectSet_AsStreaming_can_be_used_directly_on_generic_ObjectSet()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var gears = (IQueryable)CreateObjectSet<Gear>(context);

                    var count = 0;
                    foreach (var gear in gears)
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(5, count);

                    count = 0;
                    foreach (var gear in gears.AsStreaming())
                    {
                        Assert.NotNull(gear);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(5, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void ObjectSet_AsStreaming_can_be_used_before_the_rest_of_the_query()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new ProviderAgnosticContext())
                {
                    var gears = CreateObjectSet<Gear>(context);

                    var count = 0;
                    foreach (var nonPrivate in gears.Where(g => g.Rank > MilitaryRank.Private))
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Closed);
                        count++;
                    }

                    Assert.Equal(3, count);

                    count = 0;
                    foreach (var nonPrivate in gears.Where(g => g.Rank > MilitaryRank.Private).AsStreaming())
                    {
                        Assert.NotNull(nonPrivate);
                        Assert.True(context.Database.Connection.State == ConnectionState.Open);
                        count++;
                    }

                    Assert.Equal(3, count);
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        private ObjectSet<TEntity> CreateObjectSet<TEntity>(DbContext context) where TEntity : class
        {
            return (((IObjectContextAdapter)context).ObjectContext).CreateObjectSet<TEntity>();
        }

        #endregion
    }
}
