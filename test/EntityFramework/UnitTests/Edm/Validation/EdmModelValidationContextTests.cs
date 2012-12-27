// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Validation;
    using Xunit;

    public class EdmModelValidationContextTests
    {
        [Fact]
        public void IsCSpace_returns_true_when_cspace()
        {
            var validationContext = new EdmModelValidationContext(new EdmModel().InitializeConceptual(), true);

            Assert.True(validationContext.IsCSpace);

            validationContext = new EdmModelValidationContext(new EdmModel().InitializeStore(), true);

            Assert.False(validationContext.IsCSpace);
        }
    }
}
