// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class ModelGenErrorCacheTests
    {
        [Fact]
        public void Can_add_get_remove_errors()
        {
            var errorCache = new ModelGenErrorCache();
            var errors = new List<EdmSchemaError>(new[] { new EdmSchemaError("test", 42, EdmSchemaErrorSeverity.Error) });

            errorCache.AddErrors("abc", errors);
            Assert.Same(errors, errorCache.GetErrors("abc"));

            errorCache.RemoveErrors("abc");
            Assert.Null(errorCache.GetErrors("abc"));
        }

        [Fact]
        public void GetErrors_returns_null_if_no_errors_for_file_name()
        {
            Assert.Null(new ModelGenErrorCache().GetErrors("abc"));
        }

        [Fact]
        public void Removing_non_existing_errors_does_not_fail()
        {
            new ModelGenErrorCache().RemoveErrors("abc");
        }
    }
}
