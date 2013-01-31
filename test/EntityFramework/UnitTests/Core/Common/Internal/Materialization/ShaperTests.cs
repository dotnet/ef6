// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using System.Linq;
    using System.Threading;
    using Moq;
    using Xunit;

    public class ShaperTests
    {
        [Fact]
        public void GetEnumerator_returns_SimpleEnumerator_for_simple_CoordinatorFactory_sync()
        {
            GetEnumerator_returns_SimpleEnumerator_for_simple_CoordinatorFactory(e => e.ToList());
        }

#if !NET40

        [Fact]
        public void GetEnumerator_returns_SimpleEnumerator_for_simple_CoordinatorFactory_async()
        {
            GetEnumerator_returns_SimpleEnumerator_for_simple_CoordinatorFactory(e => e.ToListAsync().Result);
        }

#endif

        private void GetEnumerator_returns_SimpleEnumerator_for_simple_CoordinatorFactory(
            Func<IDbEnumerator<object>, List<object>> toList)
        {
            var sourceEnumerable = new[] { new object[] { 1 }, new object[] { 2 } };

            var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory(shaper => shaper.Reader.GetValue(0));

            var shaperMock = new Mock<Shaper<object>>(
                MockHelper.CreateDbDataReader(sourceEnumerable),
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 1,
                coordinatorFactory,
                /*readerOwned*/ false)
                                 {
                                     CallBase = true
                                 };

            var actualEnumerator = shaperMock.Object.GetEnumerator();

            Assert.Equal(sourceEnumerable.SelectMany(e => e).ToList(), toList(actualEnumerator));
        }

        [Fact]
        public void GetEnumerator_returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories_sync()
        {
            GetEnumerator_returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories(e => e.ToList());
        }

#if !NET40

        [Fact]
        public void GetEnumerator_returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories_async()
        {
            GetEnumerator_returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories(e => e.ToListAsync().Result);
        }

#endif

        private void GetEnumerator_returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories(
            Func<IDbEnumerator<object>, List<object>> toList)
        {
            var sourceEnumerable = new[]
                                       {
                                           new object[] { 1, "A", null },
                                           new object[] { 2, null, "X" },
                                           new object[] { 3, "B", "Z" },
                                           // Should stop reading at "B", since the coordinators are at the same depth
                                           new object[] { 4, "C", null },
                                           new object[] { 4, "D", null } // 4 shouldn't be added as it's repeated
                                       };

            var actualValuesFromNestedCoordinatorOne = new List<string>();
            var nestedCoordinatorFactoryOne = Objects.MockHelper.CreateCoordinatorFactory<string, string>(
                depth: 1,
                stateSlot: 1,
                ordinal: 1,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: actualValuesFromNestedCoordinatorOne);

            var actualValuesFromNestedCoordinatorTwo = new List<string>();
            var nestedCoordinatorFactoryTwo = Objects.MockHelper.CreateCoordinatorFactory<string, string>(
                depth: 1,
                stateSlot: 2,
                ordinal: 2,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: actualValuesFromNestedCoordinatorTwo);

            var actualValuesFromRootCoordinator = new List<object>();
            var rootCoordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory<int, object>(
                depth: 0,
                stateSlot: 0,
                ordinal: 0,
                nestedCoordinators: new[] { nestedCoordinatorFactoryOne, nestedCoordinatorFactoryTwo },
                producedValues: actualValuesFromRootCoordinator);

            var shaperMock = new Mock<Shaper<object>>(
                MockHelper.CreateDbDataReader(sourceEnumerable),
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 3,
                rootCoordinatorFactory,
                /*readerOwned*/ false)
                                 {
                                     CallBase = true
                                 };

            var actualEnumerator = shaperMock.Object.GetEnumerator();

            Assert.Equal(new object[] { 1, 2, 3, 4 }.ToList(), toList(actualEnumerator));
            Assert.Equal(new object[] { 1, 2, 3, 4 }.ToList(), actualValuesFromRootCoordinator);
            Assert.Equal(new[] { "A", "B", "C", "D" }.ToList(), actualValuesFromNestedCoordinatorOne);
            Assert.Equal(new[] { "X" }.ToList(), actualValuesFromNestedCoordinatorTwo);
        }

        [Fact]
        public void GetEnumerator_returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState_sync()
        {
            GetEnumerator_returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState(
                e =>
                    {
                        var actualValues = new List<object>();
                        while (e.MoveNext())
                        {
                            actualValues.Add(e.Current.PendingColumnValues[0]);
                        }
                        return actualValues;
                    });
        }

#if !NET40

        [Fact]
        public void GetEnumerator_returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState_async()
        {
            GetEnumerator_returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState(
                e =>
                    {
                        var actualValues = new List<object>();
                        while (e.MoveNextAsync(CancellationToken.None).Result)
                        {
                            actualValues.Add(e.Current.PendingColumnValues[0]);
                        }
                        return actualValues;
                    });
        }

#endif

        private void GetEnumerator_returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState(
            Func<IDbEnumerator<RecordState>, List<object>> toList)
        {
            var sourceEnumerable = new[]
                                       {
                                           new object[] { 1, "A", null },
                                           new object[] { 2, null, "X" },
                                           new object[] { 3, "B", "Z" },
                                           // Should stop reading at "B", since the coordinators are at the same depth
                                           new object[] { 4, "C", null },
                                           new object[] { 4, "D", null } // 4 shouldn't be added as it's repeated
                                       };

            var nestedCoordinatorFactoryOne = Objects.MockHelper.CreateCoordinatorFactory<string, RecordState>(
                depth: 1,
                stateSlot: 2,
                ordinal: 1,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: null);

            var nestedCoordinatorFactoryTwo = Objects.MockHelper.CreateCoordinatorFactory<string, RecordState>(
                depth: 1,
                stateSlot: 4,
                ordinal: 2,
                nestedCoordinators: new CoordinatorFactory[0],
                producedValues: null);

            var rootCoordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory<int, RecordState>(
                depth: 0,
                stateSlot: 0,
                ordinal: 0,
                nestedCoordinators: new[] { nestedCoordinatorFactoryOne, nestedCoordinatorFactoryTwo },
                producedValues: null);

            var shaperMock = new Mock<Shaper<RecordState>>(
                MockHelper.CreateDbDataReader(sourceEnumerable),
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 6,
                rootCoordinatorFactory,
                /*readerOwned*/ false)
                                 {
                                     CallBase = true
                                 };

            Assert.Equal(new object[] { 1, "A", 2, "X", 3, "B", 4, "C", "D" }.ToList(), toList(shaperMock.Object.RootEnumerator));
        }
    }
}
