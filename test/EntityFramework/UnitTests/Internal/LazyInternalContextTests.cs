// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using Moq;
    using Moq.Protected;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using Xunit;

    public class LazyInternalContextTests
    {
        [Fact]
        public void CommandTimeout_is_obtained_from_and_set_in_ObjectContext_if_ObjectContext_exists()
        {
            var mockContext = new Mock<LazyInternalContext>(
                new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null, null)
                {
                    CallBase = true
                };
            var objectContext = new ObjectContext
                {
                    CommandTimeout = 77
                };
            mockContext.Setup(m => m.ObjectContextInUse).Returns(objectContext);

            Assert.Equal(77, mockContext.Object.CommandTimeout);

            mockContext.Object.CommandTimeout = 88;

            Assert.Equal(88, objectContext.CommandTimeout);
        }

        [Fact]
        public void CommandTimeout_is_obtained_from_and_set_in_field_if_ObjectContext_does_not_exist()
        {
            var mockContext = new Mock<LazyInternalContext>(
                new Mock<DbContext>().Object, new Mock<IInternalConnection>().Object, null, null, null, null, null)
                {
                    CallBase = true
                };
            mockContext.Setup(m => m.ObjectContextInUse).Returns((ObjectContext)null);

            Assert.Null(mockContext.Object.CommandTimeout);

            mockContext.Object.CommandTimeout = 88;

            Assert.Equal(88, mockContext.Object.CommandTimeout);
        }

        [Fact]
        public void Dispose_calls_EntityConnection_dispose_before_InternalConnection_dispose()
        {
            var results = new List<string>();

            var underlyingConnection = new Mock<DbConnection>();
            
            var lazyInternalConnectionMock = new Mock<LazyInternalConnection>("fake");
            lazyInternalConnectionMock.SetupGet(ic => ic.Connection).Returns(underlyingConnection.Object);
            lazyInternalConnectionMock.Setup(ic => ic.Dispose()).Callback(() => results.Add("LazyInternalConnection Dispose() called"));

            var metadataWorkspaceMock = new Mock<MetadataWorkspace>();
            metadataWorkspaceMock.Setup(mw => mw.GetItemCollection(DataSpace.CSSpace)).Returns(() => null);
            
            var dbContext = new TestDbContext();

            var entityConnectionMock = new Mock<EntityConnection>();
            entityConnectionMock.SetupGet(ec => ec.ConnectionString).Returns("fake");
            entityConnectionMock.Setup(ec => ec.GetMetadataWorkspace()).Returns(metadataWorkspaceMock.Object);
            entityConnectionMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).
                Callback<bool>(b => results.Add("EntityConnection Dispose() called"));

            var objectContextMock = new Mock<ObjectContext>(entityConnectionMock.Object, true)
            {
                CallBase = true
            };
            objectContextMock.SetupGet(oc => oc.MetadataWorkspace).Returns(metadataWorkspaceMock.Object);

            var lazyInternalContext = new LazyInternalContext(
                dbContext, lazyInternalConnectionMock.Object, null, null, null, null, objectContextMock.Object);
            
            lazyInternalContext.DisposeContext();

            Assert.Equal(2, results.Count);
            Assert.Same("EntityConnection Dispose() called", results[0]);
            Assert.Same("LazyInternalConnection Dispose() called", results[1]);
        }

        public class TestDbContext : DbContext
        {
            internal override void InitializeLazyInternalContext(IInternalConnection internalConnection, DbCompiledModel model = null)
            {
                // do nothing - just placeholder object
            }
        }
    }
}
