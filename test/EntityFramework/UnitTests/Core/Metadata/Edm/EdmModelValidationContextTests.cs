// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EdmModelValidationContextTests
    {
        [Fact]
        public void IsCSpace_returns_true_when_cspace()
        {
            var validationContext = new EdmModelValidationContext(new EdmModel(DataSpace.CSpace), true);

            Assert.True(validationContext.IsCSpace);

            validationContext = new EdmModelValidationContext(new EdmModel(DataSpace.SSpace), true);

            Assert.False(validationContext.IsCSpace);
        }
    }
}
