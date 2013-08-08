// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Threading;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class ShaperTests : TestBase
    {
        public class GetEnumerator
        {
            [Fact]
            public void Returns_SimpleEnumerator_for_simple_CoordinatorFactory_sync()
            {
                Returns_SimpleEnumerator_for_simple_CoordinatorFactory(e => e.ToList());
            }

#if !NET40

            [Fact]
            public void Returns_SimpleEnumerator_for_simple_CoordinatorFactory_async()
            {
                Returns_SimpleEnumerator_for_simple_CoordinatorFactory(e => e.ToListAsync().Result);
            }

#endif

            private void Returns_SimpleEnumerator_for_simple_CoordinatorFactory(
                Func<IDbEnumerator<object>, List<object>> toList)
            {
                var sourceEnumerable = new[] { new object[] { 1 }, new object[] { 2 } };

                var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory(s => s.Reader.GetValue(0));

                var shaper = new Shaper<object>(
                    MockHelper.CreateDbDataReader(sourceEnumerable),
                    /*context*/ null,
                    /*workspace*/ null,
                    MergeOption.AppendOnly,
                    /*stateCount*/ 1,
                    coordinatorFactory,
                    /*readerOwned*/ false,
                    /*useSpatialReader*/ false);

                var actualEnumerator = shaper.GetEnumerator();

                Assert.Equal(sourceEnumerable.SelectMany(e => e).ToList(), toList(actualEnumerator));
            }

            [Fact]
            public void Returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories_sync()
            {
                Returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories(e => e.ToList());
            }

#if !NET40

            [Fact]
            public void Returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories_async()
            {
                Returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories(e => e.ToListAsync().Result);
            }

#endif

            private void Returns_ObjectQueryNestedEnumerator_for_nested_coordinatorFactories(
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

                var shaper = new Shaper<object>(
                    MockHelper.CreateDbDataReader(sourceEnumerable),
                    /*context*/ null,
                    /*workspace*/ null,
                    MergeOption.AppendOnly,
                    /*stateCount*/ 3,
                    rootCoordinatorFactory,
                    /*readerOwned*/ false,
                    /*useSpatialReader*/ false);

                var actualEnumerator = shaper.GetEnumerator();

                Assert.Equal(new object[] { 1, 2, 3, 4 }.ToList(), toList(actualEnumerator));
                Assert.Equal(new object[] { 1, 2, 3, 4 }.ToList(), actualValuesFromRootCoordinator);
                Assert.Equal(new[] { "A", "B", "C", "D" }.ToList(), actualValuesFromNestedCoordinatorOne);
                Assert.Equal(new[] { "X" }.ToList(), actualValuesFromNestedCoordinatorTwo);
            }

            [Fact]
            public void Returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState_sync()
            {
                Returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState(
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
            public void Returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState_async()
            {
                Returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState(
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

            private void Returns_RecordStateEnumerator_for_nested_coordinatorFactories_of_RecordState(
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

                var shaper = new Shaper<RecordState>(
                    MockHelper.CreateDbDataReader(sourceEnumerable),
                    /*context*/ null,
                    /*workspace*/ null,
                    MergeOption.AppendOnly,
                    /*stateCount*/ 6,
                    rootCoordinatorFactory,
                    /*readerOwned*/ false,
                    /*useSpatialReader*/ false);

                Assert.Equal(new object[] { 1, "A", 2, "X", 3, "B", 4, "C", "D" }.ToList(), toList(shaper.RootEnumerator));
            }
        }

        public class GetSpatialColumnValueWithErrorHandling : TestBase
        {
            [Fact]
            public void Returns_DbGeography_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: true, throwException: false);
            }

            [Fact]
            public void Returns_DbGeography_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: false, throwException: false);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeography_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: true, throwException: true);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeography_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: false, throwException: true);
            }

            [Fact]
            public void Returns_DbGeometry_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: true, throwException: false);
            }

            [Fact]
            public void Returns_DbGeometry_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: false, throwException: false);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeometry_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: true, throwException: true);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeometry_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: false, throwException: true);
            }

            private void Returns_spatial_column_value(PrimitiveTypeKind spatialType, bool useSpatialReader, bool throwException)
            {
                object[][] sourceEnumerable;

                if (spatialType == PrimitiveTypeKind.Geography)
                {
                    sourceEnumerable = new[] { new object[] { DbGeography.FromText("POINT (90 50)") } };
                }
                else
                {
                    sourceEnumerable = new[] { new object[] { DbGeometry.FromText("POINT (90 50)") } };
                }
                var reader = MockHelper.CreateDbDataReader(sourceEnumerable);

                var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory(s => s.Reader.GetValue(0));

                var shaperMock = new Mock<Shaper<object>>(
                    reader,
                    /*context*/ null,
                    /*workspace*/ null,
                    MergeOption.AppendOnly,
                    /*stateCount*/ 1,
                    coordinatorFactory,
                    /*readerOwned*/ false,
                    /*streaming*/ useSpatialReader)
                                     {
                                         CallBase = true
                                     };

                var spatialDataReaderMock = new Mock<DbSpatialDataReader>(MockBehavior.Strict);
                if (useSpatialReader)
                {
                    if (spatialType == PrimitiveTypeKind.Geography)
                    {
                        spatialDataReaderMock.Setup(m => m.GetGeography(0)).Returns((DbGeography)sourceEnumerable.First()[0]);
                    }
                    else
                    {
                        spatialDataReaderMock.Setup(m => m.GetGeometry(0)).Returns((DbGeometry)sourceEnumerable.First()[0]);
                    }
                }
                shaperMock.Protected().Setup<DbSpatialDataReader>("CreateSpatialDataReader").Returns(spatialDataReaderMock.Object);

                reader.Read();

                object result = null;
                if (spatialType == PrimitiveTypeKind.Geography)
                {
                    if (throwException)
                    {
                        Assert.Equal(
                            Strings.Materializer_InvalidCastReference(typeof(DbGeography), typeof(DbGeometry)),
                            Assert.Throws<InvalidOperationException>(
                                () => shaperMock.Object.GetSpatialColumnValueWithErrorHandling<DbGeometry>(0, spatialType)).Message);
                    }
                    else
                    {
                        result = shaperMock.Object.GetSpatialColumnValueWithErrorHandling<DbGeography>(0, spatialType);
                    }
                }
                else
                {
                    if (throwException)
                    {
                        Assert.Equal(
                            Strings.Materializer_InvalidCastReference(typeof(DbGeometry), typeof(DbGeography)),
                            Assert.Throws<InvalidOperationException>(
                                () => shaperMock.Object.GetSpatialColumnValueWithErrorHandling<DbGeography>(0, spatialType)).Message);
                    }
                    else
                    {
                        result = shaperMock.Object.GetSpatialColumnValueWithErrorHandling<DbGeometry>(0, spatialType);
                    }
                }

                if (!throwException)
                {
                    Assert.Equal(sourceEnumerable.First()[0], result);
                }
                if (useSpatialReader)
                {
                    if (spatialType == PrimitiveTypeKind.Geography)
                    {
                        spatialDataReaderMock.Verify(m => m.GetGeography(0), throwException ? Times.Exactly(2) : Times.Once());
                    }
                    else
                    {
                        spatialDataReaderMock.Verify(m => m.GetGeometry(0), throwException ? Times.Exactly(2) : Times.Once());
                    }
                }
            }
        }

        public class GetSpatialPropertyValueWithErrorHandling : TestBase
        {
            [Fact]
            public void Returns_DbGeography_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: true, throwException: false);
            }

            [Fact]
            public void Returns_DbGeography_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: false, throwException: false);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeography_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: true, throwException: true);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeography_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geography, useSpatialReader: false, throwException: true);
            }

            [Fact]
            public void Returns_DbGeometry_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: true, throwException: false);
            }

            [Fact]
            public void Returns_DbGeometry_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: false, throwException: false);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeometry_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: true, throwException: true);
            }

            [Fact]
            public void Throws_exception_for_mistyped_DbGeometry_not_using_SpatialReader()
            {
                Returns_spatial_column_value(PrimitiveTypeKind.Geometry, useSpatialReader: false, throwException: true);
            }

            private void Returns_spatial_column_value(PrimitiveTypeKind spatialType, bool useSpatialReader, bool throwException)
            {
                object[][] sourceEnumerable;

                if (spatialType == PrimitiveTypeKind.Geography)
                {
                    sourceEnumerable = new[] { new object[] { DbGeography.FromText("POINT (90 50)") } };
                }
                else
                {
                    sourceEnumerable = new[] { new object[] { DbGeometry.FromText("POINT (90 50)") } };
                }
                var reader = MockHelper.CreateDbDataReader(sourceEnumerable);

                var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory(s => s.Reader.GetValue(0));

                var shaperMock = new Mock<Shaper<object>>(
                    reader,
                    /*context*/ null,
                    /*workspace*/ null,
                    MergeOption.AppendOnly,
                    /*stateCount*/ 1,
                    coordinatorFactory,
                    /*readerOwned*/ false,
                    /*streaming*/ useSpatialReader)
                                     {
                                         CallBase = true
                                     };

                var spatialDataReaderMock = new Mock<DbSpatialDataReader>(MockBehavior.Strict);
                if (useSpatialReader)
                {
                    if (spatialType == PrimitiveTypeKind.Geography)
                    {
                        spatialDataReaderMock.Setup(m => m.GetGeography(0)).Returns((DbGeography)sourceEnumerable.First()[0]);
                    }
                    else
                    {
                        spatialDataReaderMock.Setup(m => m.GetGeometry(0)).Returns((DbGeometry)sourceEnumerable.First()[0]);
                    }
                }
                shaperMock.Protected().Setup<DbSpatialDataReader>("CreateSpatialDataReader").Returns(spatialDataReaderMock.Object);

                reader.Read();

                object result = null;
                if (spatialType == PrimitiveTypeKind.Geography)
                {
                    if (throwException)
                    {
                        Assert.Equal(
                            Strings.Materializer_SetInvalidValue(typeof(DbGeometry), "type", "property", typeof(DbGeography)),
                            Assert.Throws<InvalidOperationException>(
                                () =>
                                shaperMock.Object.GetSpatialPropertyValueWithErrorHandling<DbGeometry>(0, "property", "type", spatialType))
                                  .Message);
                    }
                    else
                    {
                        result = shaperMock.Object.GetSpatialPropertyValueWithErrorHandling<DbGeography>(0, "property", "type", spatialType);
                    }
                }
                else
                {
                    if (throwException)
                    {
                        Assert.Equal(
                            Strings.Materializer_SetInvalidValue(typeof(DbGeography), "type", "property", typeof(DbGeometry)),
                            Assert.Throws<InvalidOperationException>(
                                () =>
                                shaperMock.Object.GetSpatialPropertyValueWithErrorHandling<DbGeography>(0, "property", "type", spatialType))
                                  .Message);
                    }
                    else
                    {
                        result = shaperMock.Object.GetSpatialPropertyValueWithErrorHandling<DbGeometry>(0, "property", "type", spatialType);
                    }
                }

                if (!throwException)
                {
                    Assert.Equal(sourceEnumerable.First()[0], result);
                }
                if (useSpatialReader)
                {
                    if (spatialType == PrimitiveTypeKind.Geography)
                    {
                        spatialDataReaderMock.Verify(m => m.GetGeography(0), throwException ? Times.Exactly(2) : Times.Once());
                    }
                    else
                    {
                        spatialDataReaderMock.Verify(m => m.GetGeometry(0), throwException ? Times.Exactly(2) : Times.Once());
                    }
                }
            }
        }

        [Fact]
        public void GetGeographyColumnValue_returns_a_DbGeography_using_SpatialReader()
        {
            GetGeographyColumnValue_returns_a_DbGeography(useSpatialReader: true);
        }

        [Fact]
        public void GetGeographyColumnValue_returns_a_DbGeography_without_using_SpatialReader()
        {
            GetGeographyColumnValue_returns_a_DbGeography(useSpatialReader: false);
        }

        private void GetGeographyColumnValue_returns_a_DbGeography(bool useSpatialReader)
        {
            var sourceEnumerable = new[] { new object[] { DbGeography.FromText("POINT (90 50)") } };
            var reader = MockHelper.CreateDbDataReader(sourceEnumerable);

            var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory(s => s.Reader.GetValue(0));

            var shaperMock = new Mock<Shaper<object>>(
                reader,
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 1,
                coordinatorFactory,
                /*readerOwned*/ false,
                /*streaming*/ useSpatialReader)
                                 {
                                     CallBase = true
                                 };

            var spatialDataReaderMock = new Mock<DbSpatialDataReader>(MockBehavior.Strict);
            if (useSpatialReader)
            {
                spatialDataReaderMock.Setup(m => m.GetGeography(0)).Returns((DbGeography)sourceEnumerable.First()[0]);
            }
            shaperMock.Protected().Setup<DbSpatialDataReader>("CreateSpatialDataReader").Returns(spatialDataReaderMock.Object);

            reader.Read();

            Assert.Equal(sourceEnumerable.First()[0], shaperMock.Object.GetGeographyColumnValue(0));
            if (useSpatialReader)
            {
                spatialDataReaderMock.Verify(m => m.GetGeography(0), Times.Once());
            }
        }

        [Fact]
        public void GetGeometryColumnValue_returns_a_DbGeography_using_SpatialReader()
        {
            GetGeometryColumnValue_returns_a_DbGeography(useSpatialReader: true);
        }

        [Fact]
        public void GetGeometryColumnValue_returns_a_DbGeography_without_using_SpatialReader()
        {
            GetGeometryColumnValue_returns_a_DbGeography(useSpatialReader: false);
        }

        private void GetGeometryColumnValue_returns_a_DbGeography(bool useSpatialReader)
        {
            var sourceEnumerable = new[] { new object[] { DbGeometry.FromText("POINT (90 50)") } };
            var reader = MockHelper.CreateDbDataReader(sourceEnumerable);

            var coordinatorFactory = Objects.MockHelper.CreateCoordinatorFactory(s => s.Reader.GetValue(0));

            var shaperMock = new Mock<Shaper<object>>(
                reader,
                /*context*/ null,
                /*workspace*/ null,
                MergeOption.AppendOnly,
                /*stateCount*/ 1,
                coordinatorFactory,
                /*readerOwned*/ false,
                /*streaming*/ useSpatialReader)
                                 {
                                     CallBase = true
                                 };

            var spatialDataReaderMock = new Mock<DbSpatialDataReader>(MockBehavior.Strict);
            if (useSpatialReader)
            {
                spatialDataReaderMock.Setup(m => m.GetGeometry(0)).Returns((DbGeometry)sourceEnumerable.First()[0]);
            }
            shaperMock.Protected().Setup<DbSpatialDataReader>("CreateSpatialDataReader").Returns(spatialDataReaderMock.Object);

            reader.Read();

            Assert.Equal(sourceEnumerable.First()[0], shaperMock.Object.GetGeometryColumnValue(0));
            if (useSpatialReader)
            {
                spatialDataReaderMock.Verify(m => m.GetGeometry(0), Times.Once());
            }
        }
    }
}
