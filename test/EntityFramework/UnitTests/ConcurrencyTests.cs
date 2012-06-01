namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Moq;
    using Xunit;

    /// <summary>
    /// General unit tests for concurrency exceptions.  Note that most of
    /// the actual functionality is contained in core EF and is tested through
    /// functional tests.
    /// </summary>
    public class ConcurrencyTests : TestBase
    {
        #region Tests for access to entries in DbUpdateException

        [Fact]
        public void GetEntry_returns_null_if_the_DbUpdateException_contains_null_entries()
        {
            var mockInternalContext = new Mock<InternalContextForMock>().Object;
            var ex = new DbUpdateException(mockInternalContext, new UpdateException(), involvesIndependentAssociations: false);

            Assert.Null(ex.Entries.SingleOrDefault());
        }

        [Fact]
        public void GetEntry_throws_if_the_DbUpdateException_contains_null_context()
        {
            var ex = new DbUpdateException("", new UpdateException());

            Assert.Null(ex.Entries.SingleOrDefault());
        }

        #endregion

        #region Tests for FxCop-required constructors

        [Fact]
        public void DbUpdateException_exposes_public_empty_constructor()
        {
            new DbUpdateException();
        }

        [Fact]
        public void DbUpdateException_exposes_public_string_constructor()
        {
            var ex = new DbUpdateException("Foo");

            Assert.Equal("Foo", ex.Message);
        }

        [Fact]
        public void DbUpdateException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new DbUpdateException("Foo", inner);

            Assert.Equal("Foo", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void DbUpdateConcurrencyException_exposes_public_empty_constructor()
        {
            new DbUpdateConcurrencyException();
        }

        [Fact]
        public void DbUpdateConcurrencyException_exposes_public_string_constructor()
        {
            var ex = new DbUpdateConcurrencyException("Foo");

            Assert.Equal("Foo", ex.Message);
        }

        [Fact]
        public void DbUpdateConcurrencyException_exposes_public_string_and_inner_exception_constructor()
        {
            var inner = new Exception();

            var ex = new DbUpdateConcurrencyException("Foo", inner);

            Assert.Equal("Foo", ex.Message);
            Assert.Same(inner, ex.InnerException);
        }

        #endregion

        #region Serialization tests

        [Fact]
        public void DbUpdateException_is_marked_as_Serializable()
        {
            Assert.True(typeof(DbUpdateException).GetCustomAttributes(typeof(SerializableAttribute), inherit: false).Any());
        }

        [Fact]
        public void DbUpdateConcurrencyException_is_marked_as_Serializable()
        {
            Assert.True(typeof(DbUpdateConcurrencyException).GetCustomAttributes(typeof(SerializableAttribute), inherit: false).Any());
        }

        #endregion
    }
}
