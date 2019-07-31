// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Threading;
    using Moq;
    using Xunit;
    public class SpatialHelpersTests
    {

#if !NET40

        [Fact]
        public void GetSpatialValueAsync_throws_OperationCanceledException_if_task_is_cancelled()
        {
            Assert.Throws<OperationCanceledException>(
                () => SpatialHelpers.GetSpatialValueAsync(
                    new MetadataWorkspace(), 
                    new Mock<DbDataReader>().Object, 
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Geography)), 
                    /* columnOrdinal */ 0,
                    new CancellationToken(canceled: true))
                        .GetAwaiter().GetResult());
        }

#endif

    }
}
