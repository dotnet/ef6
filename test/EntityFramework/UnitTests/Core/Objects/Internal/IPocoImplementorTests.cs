// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using Xunit;

    public class IPocoImplementorTests : TestBase
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(IPocoImplementor.EntityMemberChangingMethod);
            Assert.NotNull(IPocoImplementor.EntityMemberChangedMethod);
            Assert.NotNull(IPocoImplementor.CreateRelationshipManagerMethod);
            Assert.NotNull(IPocoImplementor.GetEntityMethod);
            Assert.NotNull(IPocoImplementor.GetRelationshipManagerMethod);
            Assert.NotNull(IPocoImplementor.GetRelatedReferenceMethod);
            Assert.NotNull(IPocoImplementor.GetRelatedCollectionMethod);
            Assert.NotNull(IPocoImplementor.GetRelatedEndMethod);
            Assert.NotNull(IPocoImplementor.ObjectEqualsMethod);
            Assert.NotNull(IPocoImplementor.InvokeMethod);
            Assert.NotNull(IPocoImplementor.FuncInvokeMethod);
            Assert.NotNull(IPocoImplementor.SetChangeTrackerMethod);
        }
    }
}
