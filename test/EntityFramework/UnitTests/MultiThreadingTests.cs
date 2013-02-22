// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Internal.Linq;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    /// <summary>
    ///     Unit tests that execute the various places where we have thread-safe code with multiple threads
    ///     such that we have at least some chance of finding issues in this code. As with any test of this
    ///     type just because these tests pass does not mean that the code is correct. On the other hand,
    ///     if any test ever fails (EVEN ONCE) then we know there is a problem to be investigated.
    /// </summary>
    public class MultiThreadingTests : TestBase
    {
        #region Access to cached property type/getter/setter delegates

        [Fact]
        public void DbHelpers_GetPropertyTypes_can_be_accessed_from_multiple_threads_concurrently()
        {
            ExecuteInParallel(
                () =>
                    {
                        var types = DbHelpers.GetPropertyTypes(typeof(TypeWithALotOfProperties));

                        Assert.Equal(50, types.Count);
                        Assert.Equal(typeof(string), types["Property3"]);
                        Assert.Equal(typeof(int), types["Property26"]);
                    });
        }

        [Fact]
        public void DbHelpers_GetPropertySetters_can_be_accessed_from_multiple_threads_concurrently()
        {
            ExecuteInParallel(
                () =>
                    {
                        var setters = DbHelpers.GetPropertySetters(typeof(TypeWithALotOfProperties));

                        Assert.Equal(40, setters.Count);

                        var testType = new TypeWithALotOfProperties();

                        setters["Property10"](testType, "UnicornsOnTheRun");
                        Assert.Equal("UnicornsOnTheRun", testType.Property10);

                        setters["Property47"](testType, "UnicornNeXTcube");
                        Assert.Equal("UnicornNeXTcube", testType.Property47);
                    });
        }

        [Fact]
        public void DbHelpers_GetPropertyGetters_can_be_accessed_from_multiple_threads_concurrently()
        {
            ExecuteInParallel(
                () =>
                    {
                        var getters = DbHelpers.GetPropertyGetters(typeof(TypeWithALotOfProperties));

                        var testType = new TypeWithALotOfProperties();

                        Assert.Equal(47, getters.Count);
                        Assert.Equal("Hello", getters["Property3"](testType));
                        Assert.Equal(77, getters["Property26"](testType));
                    });
        }

        #endregion

        #region Access to cached ObjectContext constructor delegates

        public class DummyObjectContext : ObjectContext
        {
            public DummyObjectContext(EntityConnection connection)
                : base(connection)
            {
            }
        }

        [Fact]
        public void DbCompiledModel_GetConstructorDelegate_can_be_accessed_from_multiple_threads_concurrently()
        {
            DbCompiledModel_GetConstructorDelegate_can_be_accessed_from_multiple_threads_concurrently_implementation<DummyObjectContext>();
        }

        [Fact]
        public void DbCompiledModel_GetConstructorDelegate_for_non_derived_ObjectContext_can_be_accessed_from_multiple_threads_concurrently(
            
            )
        {
            DbCompiledModel_GetConstructorDelegate_can_be_accessed_from_multiple_threads_concurrently_implementation<ObjectContext>();
        }

        private void DbCompiledModel_GetConstructorDelegate_can_be_accessed_from_multiple_threads_concurrently_implementation<TContext>()
            where TContext : ObjectContext
        {
            ExecuteInParallel(
                () =>
                    {
                        var constructor = DbCompiledModel.GetConstructorDelegate<TContext>();

                        try
                        {
                            // We can't make an ObjectContext inexpensively so we don't want to make a real one
                            // in a unit test, therefore we just pass null and check for the exception.
                            constructor(null);
                            Assert.True(false);
                        }
                        catch (ArgumentNullException ex)
                        {
                            Assert.Equal("connection", ex.ParamName);
                        }
                    });
        }

        #endregion

        #region Setting and calling database initializers for a context type

        // This is used instead of a Moq initializer in cases where the initializer is called multiple times
        // from different threads because the Moq initializer is not thread-safe.
        public class ThreadSafeCountingInitializer<TContext> : IDatabaseInitializer<TContext>
            where TContext : DbContext
        {
            private readonly object _lock = new object();
            private readonly bool _throwFoFirstFive;

            public ThreadSafeCountingInitializer(bool throwFoFirstFive = false)
            {
                _throwFoFirstFive = throwFoFirstFive;
            }

            public int Count { get; set; }

            public void InitializeDatabase(TContext context)
            {
                lock (_lock)
                {
                    Count++;

                    if (_throwFoFirstFive && Count <= 5)
                    {
                        throw new Exception("Fail!");
                    }
                }
            }
        }

        public class ContextForSettingInitializer : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Database_initializer_can_be_set_by_multiple_threads()
        {
            var countingInitializer = new ThreadSafeCountingInitializer<ContextForSettingInitializer>();

            ExecuteInParallel(() => Database.SetInitializer(countingInitializer));

            GetDatabaseForInitialization<ContextForSettingInitializer>().Initialize(force: false);

            Assert.Equal(1, countingInitializer.Count);
        }

        private Database GetDatabaseForInitialization<TContext>() where TContext : DbContextUsingMockInternalContext, new()
        {
            var mock = new Mock<InternalContextForMockWithRealContext<TContext>>
                           {
                               CallBase = true
                           };
            mock.Setup(c => c.UseTempObjectContext()).Callback(() => { });
            return new Database(mock.Object);
        }

        public class ContextForCallingInitializer : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Database_Initialize_without_force_can_be_called_by_multiple_threads_and_initialization_only_happens_once()
        {
            var countingInitializer = new ThreadSafeCountingInitializer<ContextForCallingInitializer>();
            Database.SetInitializer(countingInitializer);

            ExecuteInParallel(() => GetDatabaseForInitialization<ContextForCallingInitializer>().Initialize(force: false));

            Assert.Equal(1, countingInitializer.Count);
        }

        public class ContextForCallingInitializerWithForce : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void Database_Initialize_with_force_can_be_called_by_multiple_threads_and_initialization_happens_every_time()
        {
            var countingInitializer = new ThreadSafeCountingInitializer<ContextForCallingInitializerWithForce>();
            Database.SetInitializer(countingInitializer);

            ExecuteInParallel(() => GetDatabaseForInitialization<ContextForCallingInitializerWithForce>().Initialize(force: true));

            Assert.Equal(20, countingInitializer.Count);
        }

        public class ContextForCallingInitializerWithFailures : DbContextUsingMockInternalContext
        {
        }

        [Fact]
        public void
            Database_Initialize_without_force_can_be_called_by_multiple_threads_and_initialization_is_attempted_until_one_thread_succeeds()
        {
            var countingInitializer = new ThreadSafeCountingInitializer<ContextForCallingInitializerWithFailures>(throwFoFirstFive: true);
            Database.SetInitializer(countingInitializer);

            try
            {
                ExecuteInParallel(() => GetDatabaseForInitialization<ContextForCallingInitializerWithFailures>().Initialize(force: false));
            }
            catch (AggregateException ex)
            {
                Assert.Equal(5, ex.InnerExceptions.Count);
                foreach (var innerException in ex.InnerExceptions)
                {
                    Assert.Equal("Fail!", innerException.Message);
                }
            }

            Assert.Equal(6, countingInitializer.Count);
        }

        #endregion

        #region Using DbSet discovery from multiple threads

        public class ContextForSetDiscovery : DbContext
        {
            // Having a lot of sets makes it more likely that the discovery
            // service will be running concurrently in two threads.
            public DbSet<FakeEntity> Set1 { get; set; }
            public DbSet<FakeEntity> Set2 { get; set; }
            public DbSet<FakeEntity> Set3 { get; set; }
            public DbSet<FakeEntity> Set4 { get; set; }
            public DbSet<FakeEntity> Set5 { get; set; }
            public DbSet<FakeEntity> Set6 { get; set; }
            public DbSet<FakeEntity> Set7 { get; set; }
            public DbSet<FakeEntity> Set8 { get; set; }
            public DbSet<FakeEntity> Set9 { get; set; }
            public DbSet<FakeEntity> Set10 { get; set; }
            public DbSet<FakeEntity> Set11 { get; set; }
            public DbSet<FakeEntity> Set12 { get; set; }
            public DbSet<FakeEntity> Set13 { get; set; }
            public DbSet<FakeEntity> Set14 { get; set; }
            public DbSet<FakeEntity> Set15 { get; set; }
            public DbSet<FakeEntity> Set16 { get; set; }
            public DbSet<FakeEntity> Set17 { get; set; }
            public DbSet<FakeEntity> Set18 { get; set; }
            public DbSet<FakeEntity> Set19 { get; set; }
            public DbSet<FakeEntity> Set20 { get; set; }
            public DbSet<FakeEntity> Set21 { get; set; }
            public DbSet<FakeEntity> Set22 { get; set; }
            public DbSet<FakeEntity> Set23 { get; set; }
            public DbSet<FakeEntity> Set24 { get; set; }
            public DbSet<FakeEntity> Set25 { get; set; }
            public DbSet<FakeEntity> Set26 { get; set; }
            public DbSet<FakeEntity> Set27 { get; set; }
            public DbSet<FakeEntity> Set28 { get; set; }
            public DbSet<FakeEntity> Set29 { get; set; }
            public DbSet<FakeEntity> Set30 { get; set; }
        }

        [Fact]
        public void Set_discovery_can_be_called_from_multiple_threads_at_the_same_time()
        {
            ExecuteInParallel(
                () =>
                    {
                        using (var context = new ContextForSetDiscovery())
                        {
                            new DbSetDiscoveryService(context).InitializeSets();
                            Assert.NotNull(context.Set1);
                            Assert.NotNull(context.Set30);
                        }
                    });
        }

        #endregion

        #region Various factory methods called from multiple threads

        [Fact]
        public void DbMemberEntry_Create_can_be_called_from_multiple_threads()
        {
            ExecuteInParallel(
                () =>
                    {
                        var collectionMetadata = new NavigationEntryMetadata(
                            typeof(FakeWithProps), typeof(FakeEntity), "Collection", isCollection: true);
                        var internalEntry =
                            new InternalCollectionEntry(
                                new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, collectionMetadata);

                        var entry = internalEntry.CreateDbMemberEntry<FakeWithProps, ICollection<FakeEntity>>();

                        Assert.IsAssignableFrom<DbMemberEntry<FakeWithProps, ICollection<FakeEntity>>>(entry);
                    });
        }

        [Fact]
        public void InternalPropertyValues_ToObject_for_entity_type_can_be_called_from_multiple_threads()
        {
            ExecuteInParallel(
                () =>
                    {
                        var values = new TestInternalPropertyValues<DbPropertyValuesTests.FakeTypeWithProps>(null, isEntityValues: true);
                        values.MockInternalContext.Setup(c => c.CreateObject(typeof(DbPropertyValuesTests.FakeTypeWithProps))).Returns(
                            new DbPropertyValuesTests.FakeDerivedTypeWithProps());

                        var clone = values.ToObject();

                        Assert.IsType<DbPropertyValuesTests.FakeDerivedTypeWithProps>(clone);
                    });
        }

        [Fact]
        public void InternalPropertyValues_ToObject_for_non_entity_type_can_be_called_from_multiple_threads()
        {
            ExecuteInParallel(
                () =>
                    {
                        var values = new TestInternalPropertyValues<DbPropertyValuesTests.FakeTypeWithProps>(null, isEntityValues: true);
                        values.MockInternalContext.Setup(c => c.CreateObject(typeof(DbPropertyValuesTests.FakeTypeWithProps))).Returns(
                            new DbPropertyValuesTests.FakeDerivedTypeWithProps());

                        var clone = values.ToObject();

                        Assert.IsType<DbPropertyValuesTests.FakeDerivedTypeWithProps>(clone);
                    });
        }

        [Fact]
        public void Non_generic_DbSet_creation_can_be_called_from_multiple_threads()
        {
            ExecuteInParallel(
                () =>
                    {
                        var internalContext = new Mock<InternalContextForMock>
                                                  {
                                                      CallBase = true
                                                  }.Object;
                        var set = internalContext.Set(typeof(FakeEntity));
                        Assert.IsType<InternalDbSet<FakeEntity>>(set);
                    });
        }

        [Fact]
        public void ObjectContextTypeCache_GetObjectType_can_be_called_from_multiple_threads()
        {
            ExecuteInParallel(
                () =>
                    {
                        var type = ObjectContextTypeCache.GetObjectType(typeof(FakeEntity));

                        Assert.Same(typeof(FakeEntity), type);
                    });
        }

        #endregion

        #region RetryLazy tests

        [Fact]
        public void RetryLazy_only_runs_the_lazy_initializer_once_even_when_called_from_multiple_threads()
        {
            var count = 0;
            var lockObject = new object();
            var initializer = new RetryLazy<string, string>(
                i =>
                    {
                        // Locking here to ensure that count is incremented correctly even if RetryLazy isn't working correctly.
                        lock (lockObject)
                        {
                            count++;
                            return "";
                        }
                    });
            ExecuteInParallel(() => initializer.GetValue(""));
            Assert.Equal(1, count);
        }

        [Fact]
        public void RetryLazy_keeps_trying_to_initialize_until_an_attempt_succeeds()
        {
            var count = 0;
            var lockObject = new object();
            var initializer = new RetryLazy<string, string>(
                i =>
                    {
                        // Locking here to ensure that count is incremented correctly even if RetryLazy isn't working correctly.
                        lock (lockObject)
                        {
                            count++;
                            if (count <= 5)
                            {
                                throw new Exception("Fail!");
                            }
                            return "";
                        }
                    });

            try
            {
                ExecuteInParallel(() => initializer.GetValue(""));
            }
            catch (AggregateException ex)
            {
                Assert.Equal(5, ex.InnerExceptions.Count);
                foreach (var innerException in ex.InnerExceptions)
                {
                    Assert.Equal("Fail!", innerException.Message);
                }
            }
            Assert.Equal(6, count);
        }

        public class InitializerOutput
        {
            public int Input { get; set; }
            public int Count { get; set; }
        }

        [Fact]
        public void RetryLazy_uses_the_given_input_for_each_attempt_at_initialization_until_an_attempt_succeeds()
        {
            var count = 0;
            var lockObject = new object();
            InitializerOutput result = null;
            var inputs = new List<int>();

            var initializer = new RetryLazy<int, InitializerOutput>(
                i =>
                    {
                        // Locking here to ensure that count is incremented correctly even if RetryLazy isn't working correctly.
                        lock (lockObject)
                        {
                            inputs.Add(i);
                            count++;
                            if (count <= 5)
                            {
                                throw new Exception("Fail!");
                            }
                            return new InitializerOutput
                                       {
                                           Input = i,
                                           Count = count
                                       };
                        }
                    });

            var tests = new Action[20];
            for (var i = 0; i < 20; i++)
            {
                var outside = i; // Make sure i is used from outside the closure
                tests[i] = () => result = initializer.GetValue(outside);
            }

            try
            {
                Parallel.Invoke(tests);
            }
            catch (AggregateException ex)
            {
                Assert.Equal(5, ex.InnerExceptions.Count);
            }

            Assert.Equal(6, count);
            Assert.Equal(6, result.Count);
            Assert.Equal(6, inputs.Count);
            Assert.Equal(inputs[5], result.Input);

            for (var i = 0; i < inputs.Count; i++)
            {
                for (var j = 0; j < inputs.Count; j++)
                {
                    if (i != j)
                    {
                        Assert.NotEqual(inputs[i], inputs[j]);
                    }
                }
            }
        }

        #endregion

        #region RetryAction tests

        [Fact]
        public void RetryAction_only_runs_the_action_once_even_when_called_from_multiple_threads()
        {
            var count = 0;
            var lockObject = new object();
            var initializer = new RetryAction<string>(
                i =>
                    {
                        // Locking here to ensure that count is incremented correctly even if RetryAction isn't working correctly.
                        lock (lockObject)
                        {
                            count++;
                        }
                    });
            ExecuteInParallel(() => initializer.PerformAction(""));
            Assert.Equal(1, count);
        }

        [Fact]
        public void RetryAction_keeps_trying_to_run_the_action_until_an_attempt_succeeds()
        {
            var count = 0;
            var lockObject = new object();
            var initializer = new RetryAction<string>(
                i =>
                    {
                        // Locking here to ensure that count is incremented correctly even if RetryAction isn't working correctly.
                        lock (lockObject)
                        {
                            count++;
                            if (count <= 5)
                            {
                                throw new Exception("Fail!");
                            }
                        }
                    });

            try
            {
                ExecuteInParallel(() => initializer.PerformAction(""));
            }
            catch (AggregateException ex)
            {
                Assert.Equal(5, ex.InnerExceptions.Count);
                foreach (var innerException in ex.InnerExceptions)
                {
                    Assert.Equal("Fail!", innerException.Message);
                }
            }
            Assert.Equal(6, count);
        }

        public void RetryAction_uses_the_given_input_for_each_attempt_until_an_attempt_succeeds()
        {
            var count = 0;
            var lockObject = new object();
            var inputs = new List<int>();

            var initializer = new RetryAction<int>(
                i =>
                    {
                        // Locking here to ensure that count is incremented correctly even if RetryAction isn't working correctly.
                        lock (lockObject)
                        {
                            inputs.Add(i);
                            count++;
                            if (count <= 5)
                            {
                                throw new Exception("Fail!");
                            }
                        }
                    });

            var tests = new Action[20];
            for (var i = 0; i < 20; i++)
            {
                var outside = i; // Make sure i is used from outside the closure
                tests[i] = () => initializer.PerformAction(outside);
            }

            try
            {
                Parallel.Invoke(tests);
            }
            catch (AggregateException ex)
            {
                Assert.Equal(5, ex.InnerExceptions.Count);
            }

            Assert.Equal(6, count);
            Assert.Equal(6, inputs.Count);

            for (var i = 0; i < inputs.Count; i++)
            {
                for (var j = 0; j < inputs.Count; j++)
                {
                    if (i != j)
                    {
                        Assert.NotEqual(inputs[i], inputs[j]);
                    }
                }
            }
        }

        #endregion
    }

    #region

    public class TypeWithALotOfProperties
    {
        public int Property0 { get; set; }
        private byte Property1 { get; set; }

        protected int Property2
        {
            get { return 0; }
        }

        public string Property3
        {
            get { return "Hello"; }
        }

        public int Property4 { get; set; }
        public object Property5 { get; set; }
        internal int Property6 { get; set; }
        public int Property7 { get; set; }
        public int Property8 { get; set; }
        public int Property9 { get; set; }
        public string Property10 { get; set; }
        protected int Property11 { get; set; }
        private int Property12 { get; set; }
        public int Property13 { get; set; }
        public byte Property14 { get; set; }

        internal int Property15
        {
            set { }
        }

        private int Property16 { get; set; }
        public object Property17 { get; set; }
        public int Property18 { get; set; }
        public int Property19 { get; set; }

        public string Property20
        {
            get { return ""; }
        }

        public int Property21 { get; set; }

        public int Property22
        {
            get { return 0; }
        }

        private int Property23 { get; set; }
        private string Property24 { get; set; }
        public int Property25 { get; set; }

        public int Property26
        {
            get { return 77; }
        }

        protected long Property27 { get; set; }
        public int Property28 { get; set; }
        private int Property29 { get; set; }

        protected string Property30
        {
            set { }
        }

        protected int Property31 { get; set; }
        private int Property32 { get; set; }
        public object Property33 { get; set; }

        internal int Property34
        {
            get { return 0; }
        }

        public string Property35
        {
            get { return ""; }
        }

        public int Property36 { get; set; }
        protected int Property37 { get; set; }

        public int Property38
        {
            get { return 0; }
        }

        public byte Property39 { get; set; }

        protected int Property40
        {
            get { return 0; }
        }

        public object Property41
        {
            get { return null; }
        }

        public int Property42 { get; set; }
        protected string Property43 { get; set; }
        public int Property44 { get; set; }

        public byte Property45
        {
            set { }
        }

        public byte Property46 { get; set; }
        public string Property47 { get; set; }
        public object Property48 { get; set; }
        public int Property49 { get; set; }
    }

    #endregion
}
