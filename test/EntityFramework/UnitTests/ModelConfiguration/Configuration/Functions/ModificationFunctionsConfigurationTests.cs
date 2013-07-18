// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class ModificationFunctionsConfigurationTests
    {
        [Fact]
        public void Can_merge_configurations()
        {
            var modificationFunctionsConfigurationA = new ModificationStoredProceduresConfiguration();
            var modificationFunctionConfiguration = new ModificationStoredProcedureConfiguration();

            modificationFunctionsConfigurationA.Insert(modificationFunctionConfiguration);
            modificationFunctionsConfigurationA.Update(modificationFunctionConfiguration);
            modificationFunctionsConfigurationA.Delete(modificationFunctionConfiguration);

            var modificationFunctionsConfigurationB = new ModificationStoredProceduresConfiguration();

            modificationFunctionsConfigurationB.Merge(modificationFunctionsConfigurationA, true);

            Assert.Same(modificationFunctionConfiguration, modificationFunctionsConfigurationB.InsertModificationStoredProcedureConfiguration);
            Assert.Same(modificationFunctionConfiguration, modificationFunctionsConfigurationB.UpdateModificationStoredProcedureConfiguration);
            Assert.Same(modificationFunctionConfiguration, modificationFunctionsConfigurationB.DeleteModificationStoredProcedureConfiguration);
        }

        [Fact]
        public void Can_clone_configuration()
        {
            var modificationFunctionsConfiguration = new ModificationStoredProceduresConfiguration();

            var modificationFunctionConfiguration = new ModificationStoredProcedureConfiguration();

            modificationFunctionsConfiguration.Insert(modificationFunctionConfiguration);
            modificationFunctionsConfiguration.Update(modificationFunctionConfiguration);
            modificationFunctionsConfiguration.Delete(modificationFunctionConfiguration);

            var clone = modificationFunctionsConfiguration.Clone();

            Assert.NotSame(modificationFunctionsConfiguration, clone);
            Assert.NotSame(modificationFunctionConfiguration, clone.InsertModificationStoredProcedureConfiguration);
            Assert.NotSame(modificationFunctionConfiguration, clone.UpdateModificationStoredProcedureConfiguration);
            Assert.NotSame(modificationFunctionConfiguration, clone.DeleteModificationStoredProcedureConfiguration);
        }

        [Fact]
        public void Configure_should_call_configure_function_configurations()
        {
            var modificationFunctionsConfiguration = new ModificationStoredProceduresConfiguration();

            var mockModificationFunctionConfiguration = new Mock<ModificationStoredProcedureConfiguration>();

            modificationFunctionsConfiguration.Insert(mockModificationFunctionConfiguration.Object);
            modificationFunctionsConfiguration.Update(mockModificationFunctionConfiguration.Object);
            modificationFunctionsConfiguration.Delete(mockModificationFunctionConfiguration.Object);

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("C", DataSpace.CSpace));

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    new EntityType("E", "N", DataSpace.CSpace),
                    new EdmFunction("F", "N", DataSpace.SSpace),
                    new StorageModificationFunctionParameterBinding[0],
                    null,
                    null);

            modificationFunctionsConfiguration.Configure(
                new StorageEntityTypeModificationFunctionMapping(
                    new EntityType("E", "N", DataSpace.CSpace),
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping),
                ProviderRegistry.Sql2008_ProviderManifest);

            mockModificationFunctionConfiguration
                .Verify(
                    m => m.Configure(storageModificationFunctionMapping, It.IsAny<DbProviderManifest>()),
                    Times.Exactly(3));
        }

        [Fact]
        public void Configure_association_set_should_call_configure_function_configurations()
        {
            var modificationFunctionsConfiguration = new ModificationStoredProceduresConfiguration();

            var mockModificationFunctionConfiguration = new Mock<ModificationStoredProcedureConfiguration>();

            modificationFunctionsConfiguration.Insert(mockModificationFunctionConfiguration.Object);
            modificationFunctionsConfiguration.Delete(mockModificationFunctionConfiguration.Object);

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer("C", DataSpace.CSpace));

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    new EntityType("E", "N", DataSpace.CSpace),
                    new EdmFunction("F", "N", DataSpace.SSpace),
                    new StorageModificationFunctionParameterBinding[0],
                    null,
                    null);

            modificationFunctionsConfiguration.Configure(
                new StorageAssociationSetModificationFunctionMapping(
                    new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)),
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping),
                ProviderRegistry.Sql2008_ProviderManifest);

            mockModificationFunctionConfiguration
                .Verify(
                    m => m.Configure(storageModificationFunctionMapping, It.IsAny<DbProviderManifest>()),
                    Times.Exactly(2));
        }

        [Fact]
        public void IsCompatible_should_check_compatibility_of_insert_configuration()
        {
            var modificationFunctionsConfiguration1 = new ModificationStoredProceduresConfiguration();
            var modificationFunctionsConfiguration2 = new ModificationStoredProceduresConfiguration();

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            var modificationFunctionConfiguration1 = new ModificationStoredProcedureConfiguration();
            var modificationFunctionConfiguration2 = new ModificationStoredProcedureConfiguration();

            modificationFunctionsConfiguration1.Insert(modificationFunctionConfiguration1);

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            modificationFunctionsConfiguration2.Insert(modificationFunctionConfiguration2);

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            modificationFunctionConfiguration1.HasName("Foo");

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            modificationFunctionConfiguration2.HasName("Bar");

            Assert.False(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));
        }

        [Fact]
        public void IsCompatible_should_check_compatibility_of_delete_configuration()
        {
            var modificationFunctionsConfiguration1 = new ModificationStoredProceduresConfiguration();
            var modificationFunctionsConfiguration2 = new ModificationStoredProceduresConfiguration();

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            var modificationFunctionConfiguration1 = new ModificationStoredProcedureConfiguration();
            var modificationFunctionConfiguration2 = new ModificationStoredProcedureConfiguration();

            modificationFunctionsConfiguration1.Delete(modificationFunctionConfiguration1);

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            modificationFunctionsConfiguration2.Delete(modificationFunctionConfiguration2);

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            modificationFunctionConfiguration1.HasName("Foo");

            Assert.True(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));

            modificationFunctionConfiguration2.HasName("Bar");

            Assert.False(modificationFunctionsConfiguration1.IsCompatibleWith(modificationFunctionsConfiguration2));
        }
    }
}
