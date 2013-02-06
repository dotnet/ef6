// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class ModificationFunctionsConfigurationTests
    {
        [Fact]
        public void Can_clone_configuration()
        {
            var modificationFunctionsConfiguration = new ModificationFunctionsConfiguration();

            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            modificationFunctionsConfiguration.InsertFunction(modificationFunctionConfiguration);
            modificationFunctionsConfiguration.UpdateFunction(modificationFunctionConfiguration);
            modificationFunctionsConfiguration.DeleteFunction(modificationFunctionConfiguration);

            var clone = modificationFunctionsConfiguration.Clone();

            Assert.NotSame(modificationFunctionsConfiguration, clone);
            Assert.NotSame(modificationFunctionConfiguration, clone.InsertModificationFunctionConfiguration);
            Assert.NotSame(modificationFunctionConfiguration, clone.UpdateModificationFunctionConfiguration);
            Assert.NotSame(modificationFunctionConfiguration, clone.DeleteModificationFunctionConfiguration);
        }

        [Fact]
        public void Configure_should_call_configure_function_configurations()
        {
            var modificationFunctionsConfiguration = new ModificationFunctionsConfiguration();

            var mockModificationFunctionConfiguration = new Mock<ModificationFunctionConfiguration>();

            modificationFunctionsConfiguration.InsertFunction(mockModificationFunctionConfiguration.Object);
            modificationFunctionsConfiguration.UpdateFunction(mockModificationFunctionConfiguration.Object);
            modificationFunctionsConfiguration.DeleteFunction(mockModificationFunctionConfiguration.Object);

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer());

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    new EntityType(),
                    new EdmFunction(),
                    new StorageModificationFunctionParameterBinding[0],
                    null,
                    null);

            modificationFunctionsConfiguration.Configure(
                new StorageEntityTypeModificationFunctionMapping(
                    new EntityType(),
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping));

            mockModificationFunctionConfiguration
                .Verify(m => m.Configure(storageModificationFunctionMapping), Times.Exactly(3));
        }
    }
}
