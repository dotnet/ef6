// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal.MockingProxies;
    using Moq;

    /// <summary>
    /// A derived InternalContext implementation that exposes a parameterless constructor
    /// that creates a mocked underlying DbContext such that the internal context can
    /// also be mocked.
    /// </summary>
    internal abstract class InternalContextForMock<TContext> : InternalContext
        where TContext : DbContext
    {
        private readonly ObjectContext _objectContext;

        protected InternalContextForMock()
            : this(Core.Objects.MockHelper.CreateMockObjectContext<DbDataRecord>())
        {
        }

        protected InternalContextForMock(ObjectContext objectContext)
            : base(new Mock<TContext>
                       {
                           CallBase = true
                       }.Object)
        {
            Mock.Get((TContext)Owner).Setup(c => c.InternalContext).Returns(this);
            _objectContext = objectContext;
        }

        public override ObjectContext ObjectContext
        {
            get { return _objectContext; }
        }

        public override ObjectContext GetObjectContextWithoutDatabaseInitialization()
        {
            return _objectContext;
        }

        public override ClonedObjectContext CreateObjectContextForDdlOps()
        {
            var clonedObjectContextMock = new Mock<ClonedObjectContext>();
            clonedObjectContextMock.Setup(m => m.ObjectContext).Returns(new ObjectContextProxy(_objectContext));
            return clonedObjectContextMock.Object;
        }
    }
}
