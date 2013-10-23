// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Metadata
{
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class EdmTypeExtensionsTests
    {
        [Fact]
        public void EdmType_GetDataSpace_returns_correct_space_for_EdmType()
        {
            Assert.Equal(DataSpace.CSpace, PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32).GetDataSpace());
        }
    }
}
