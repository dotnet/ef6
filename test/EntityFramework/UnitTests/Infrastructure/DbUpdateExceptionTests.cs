// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Internal;
    using System.Linq;
    using Moq;
    using Xunit;

    internal class DbUpdateExceptionTests
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

        #endregion

        [Fact]
        public void DbUpdateException_is_marked_as_Serializable()
        {
            Assert.True(typeof(DbUpdateException).GetCustomAttributes(typeof(SerializableAttribute), inherit: false).Any());
        }
    }
}
