// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Moq;
    using Xunit;

    /// <summary>
    ///     Unit tests for the AsNoTracking extension methods on IQueryable.
    /// </summary>
    public class AsNoTrackingTests : TestBase
    {
        #region AsNoTracking negative contract tests

        [Fact]
        public void AsNoTracking_with_null_source_called_on_extension_method_throws()
        {
            Assert.Equal(
                "source", Assert.Throws<ArgumentNullException>(() => IQueryableExtensions.AsNoTracking<FakeEntity>(null)).ParamName);
        }

        [Fact]
        public void Non_generic_AsNoTracking_with_null_source_called_on_extension_method_throws()
        {
            Assert.Equal("source", Assert.Throws<ArgumentNullException>(() => IQueryableExtensions.AsNoTracking(null)).ParamName);
        }

        #endregion

        #region Positive custom IQueryable AsNoTracking tests

        [Fact]
        public void AsNoTracking_on_IEnumerable_does_nothing()
        {
            var enumerable = new List<FakeEntity>
                                 {
                                     new FakeEntity(),
                                     new FakeEntity(),
                                     new FakeEntity()
                                 }.AsQueryable();
            var afterNoTracking = enumerable.AsNoTracking();

            Assert.Same(enumerable, afterNoTracking);
            Assert.Equal(3, afterNoTracking.Count());
        }

        [Fact]
        public void AsNoTracking_on_IQueryable_with_no_AsNoTracking_method_does_nothing()
        {
            var mockQueryable = new Mock<IQueryable<FakeEntity>>().Object;
            var afterAsNoTracking = mockQueryable.AsNoTracking();

            Assert.Same(mockQueryable, afterAsNoTracking);
        }

        public interface INoTrackingable<T> : IQueryable<T>
        {
            INoTrackingable<T> AsNoTracking();
        }

        [Fact]
        public void AsNoTracking_on_IQueryable_with_AsNoTracking_method_calls_that_method()
        {
            var mockQueryable = new Mock<INoTrackingable<FakeEntity>>(MockBehavior.Strict);
            IQueryable<FakeEntity> source = mockQueryable.Object;
            var result = new Mock<INoTrackingable<FakeEntity>>().Object;
            mockQueryable.Setup(i => i.AsNoTracking()).Returns(result);

            var afterAsNoTracking = source.AsNoTracking();

            Assert.Same(result, afterAsNoTracking);
        }

        public interface INoTrackingableWithFunnyAsNoTracking<T> : IQueryable<T>
        {
            INoTrackingableWithFunnyAsNoTracking<T> AsNoTracking(string buffy, string summers);
        }

        [Fact]
        public void AsNoTracking_on_IQueryable_with_non_matching_AsNoTracking_is_ignored()
        {
            IQueryable<FakeEntity> source = new Mock<INoTrackingableWithFunnyAsNoTracking<FakeEntity>>(MockBehavior.Strict).Object;

            var afterAsNoTracking = source.AsNoTracking();

            Assert.Same(source, afterAsNoTracking);
        }

        public interface INoTrackingableReturningVoid<T> : IQueryable<T>
        {
            void AsNoTracking();
        }

        [Fact]
        public void AsNoTracking_on_IQueryable_with_void_AsNoTracking_method_is_ignored()
        {
            var mockQueryable = new Mock<INoTrackingableReturningVoid<FakeEntity>>(MockBehavior.Strict);

            var afterAsNoTracking = ((IQueryable<FakeEntity>)mockQueryable.Object).AsNoTracking();

            Assert.Same(mockQueryable.Object, afterAsNoTracking);
        }

        public interface INoTrackingableReturningString<T> : IQueryable<T>
        {
            string AsNoTracking();
        }

        [Fact]
        public void AsNoTracking_on_IQueryable_with_AsNoTracking_returning_string_is_ignored()
        {
            IQueryable<FakeEntity> source = new Mock<INoTrackingableReturningString<FakeEntity>>(MockBehavior.Strict).Object;

            var afterAsNoTracking = source.AsNoTracking();

            Assert.Same(source, afterAsNoTracking);
        }

        [Fact]
        public void Non_generic_AsNoTracking_on_IEnumerable_does_nothing()
        {
            var enumerable = (IQueryable)new List<FakeEntity>
                                             {
                                                 new FakeEntity(),
                                                 new FakeEntity(),
                                                 new FakeEntity()
                                             }.AsQueryable();
            var afterAsNoTracking = enumerable.AsNoTracking();

            Assert.Same(enumerable, afterAsNoTracking);
            Assert.Equal(3, afterAsNoTracking.ToList<FakeEntity>().Count());
        }

        [Fact]
        public void Non_generic_AsNoTracking_on_IQueryable_with_no_AsNoTracking_method_does_nothing()
        {
            var mockQueryable = new Mock<IQueryable>().Object;
            var afterAsNoTracking = mockQueryable.AsNoTracking();

            Assert.Same(mockQueryable, afterAsNoTracking);
        }

        public interface INoTrackingable : IQueryable
        {
            INoTrackingable AsNoTracking();
        }

        [Fact]
        public void Non_generic_AsNoTracking_on_IQueryable_with_AsNoTracking_method_calls_that_method()
        {
            var mockQueryable = new Mock<INoTrackingable>(MockBehavior.Strict);
            IQueryable source = mockQueryable.Object;
            var result = new Mock<INoTrackingable>().Object;
            mockQueryable.Setup(i => i.AsNoTracking()).Returns(result);

            var afterAsNoTracking = source.AsNoTracking();

            Assert.Same(result, afterAsNoTracking);
        }

        public interface INoTrackingableWithFunnyAsNoTracking : IQueryable
        {
            INoTrackingableWithFunnyAsNoTracking AsNoTracking(string buffy, string summers);
        }

        [Fact]
        public void Non_generic_AsNoTracking_on_IQueryable_with_non_matching_AsNoTracking_is_ignored()
        {
            IQueryable source = new Mock<INoTrackingableWithFunnyAsNoTracking>(MockBehavior.Strict).Object;

            var afterAsNoTracking = source.AsNoTracking();

            Assert.Same(source, afterAsNoTracking);
        }

        public interface INoTrackingableReturningVoid : IQueryable
        {
            void AsNoTracking();
        }

        [Fact]
        public void Non_generic_AsNoTracking_on_IQueryable_with_void_AsNoTracking_method_is_ignored()
        {
            var mockQueryable = new Mock<INoTrackingableReturningVoid>(MockBehavior.Strict);

            var afterAsNoTracking = ((IQueryable)mockQueryable.Object).AsNoTracking();

            Assert.Same(mockQueryable.Object, afterAsNoTracking);
        }

        public interface INoTrackingableReturningString : IQueryable
        {
            string AsNoTracking();
        }

        [Fact]
        public void Non_generic_AsNoTracking_on_IQueryable_with_AsNoTracking_returning_string_is_ignored()
        {
            IQueryable source = new Mock<INoTrackingableReturningString>(MockBehavior.Strict).Object;

            var afterAsNoTracking = source.AsNoTracking();

            Assert.Same(source, afterAsNoTracking);
        }

        #endregion
    }
}
