// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using Xunit;

    public class DeleteModificationFunctionConfigurationTTests : ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void RowsAffectedParameter_should_set_column_name()
        {
            var configuration = new UpdateModificationFunctionConfiguration<Entity>();

            configuration.RowsAffectedParameter("Foo");

            Assert.Equal("Foo", configuration.Configuration.RowsAffectedParameterName);
        }

        protected override ModificationFunctionConfiguration<Entity> CreateConfiguration()
        {
            return new DeleteModificationFunctionConfiguration<Entity>();
        }
    }
}
