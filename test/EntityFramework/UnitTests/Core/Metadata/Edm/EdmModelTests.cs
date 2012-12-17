// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.ModelConfiguration;
    using System.Linq;
    using Xunit;

    public class EdmModelTests
    {
        [Fact]
        public void GlobalItems_should_return_namespace_items_and_containers()
        {
            var model = new EdmModel().InitializeStore();

            model.AddItem(new EntityType());

            Assert.Equal(2, model.GlobalItems.Count());
        }

        [Fact]
        public void Validate_should_throw_on_error()
        {
            var model = new EdmModel().InitializeConceptual();

            model.AddItem(new EntityType());

            Assert.Throws<ModelValidationException>(() => model.Validate());
        }
    }
}
