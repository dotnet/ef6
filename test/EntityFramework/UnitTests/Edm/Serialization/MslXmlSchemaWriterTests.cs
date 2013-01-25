// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Xunit;

    public class MslXmlSchemaWriterTests
    {
        [Fact]
        public void WriteEntitySetMappingElement_should_write_modification_function_mappings()
        {
            var fixture = new Fixture();

            var entityType = new EntityType();
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            var entityContainer = new EntityContainer("EC", DataSpace.SSpace);

            entityContainer.AddEntitySetBase(entitySet);

            var storageEntitySetMapping
                = new StorageEntitySetMapping(
                    entitySet,
                    new StorageEntityContainerMapping(entityContainer));

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    entityType,
                    new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload()),
                    Enumerable.Empty<StorageModificationFunctionParameterBinding>(),
                    null,
                    null);

            storageEntitySetMapping.AddModificationFunctionMapping(
                new StorageEntityTypeModificationFunctionMapping(
                    entityType,
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping,
                    storageModificationFunctionMapping));

            fixture.Writer.WriteEntitySetMappingElement(storageEntitySetMapping);

            Assert.Equal(
                @"<EntitySetMapping Name=""ES"">
  <EntityTypeMapping TypeName="".E"">
    <ModificationFunctionMapping>
      <InsertFunction FunctionName=""N.F"" />
      <UpdateFunction FunctionName=""N.F"" />
      <DeleteFunction FunctionName=""N.F"" />
    </ModificationFunctionMapping>
  </EntityTypeMapping>
</EntitySetMapping>",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionMapping_should_write_simple_parameter_and_result_bindings()
        {
            var fixture = new Fixture();

            var entityType = new EntityType();
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            new EntityContainer("EC", DataSpace.SSpace).AddEntitySetBase(entitySet);

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    entityType,
                    new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload()),
                    new[]
                        {
                            new StorageModificationFunctionParameterBinding(
                                new FunctionParameter(
                                "P",
                                TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                ParameterMode.In),
                                new StorageModificationFunctionMemberPath(
                                new[] { new EdmProperty("M") },
                                null),
                                true)
                        },
                    null,
                    new[]
                        {
                            new StorageModificationFunctionResultBinding("C", new EdmProperty("M"))
                        });

            fixture.Writer.WriteFunctionMapping("InsertFunction", storageModificationFunctionMapping);

            Assert.Equal(
                @"<InsertFunction FunctionName=""N.F"">
  <ScalarProperty Name=""M"" ParameterName=""P"" Version=""Current"" />
  <ResultBinding Name=""M"" ColumnName=""C"" />
</InsertFunction>",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionMapping_should_write_complex_parameter_bindings()
        {
            var fixture = new Fixture();

            var entityType = new EntityType();
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            new EntityContainer("EC", DataSpace.SSpace).AddEntitySetBase(entitySet);

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    entityType,
                    new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload()),
                    new[]
                        {
                            new StorageModificationFunctionParameterBinding(
                                new FunctionParameter(
                                "P",
                                TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                ParameterMode.In),
                                new StorageModificationFunctionMemberPath(
                                new[]
                                    {
                                        EdmProperty.Complex("C1", new ComplexType()),
                                        EdmProperty.Complex("C2", new ComplexType()),
                                        new EdmProperty("M")
                                    },
                                null),
                                true)
                        },
                    null,
                    null);

            fixture.Writer.WriteFunctionMapping("InsertFunction", storageModificationFunctionMapping);

            Assert.Equal(
                @"<InsertFunction FunctionName=""N.F"">
  <ComplexProperty Name=""C1"" TypeName=""."">
    <ComplexProperty Name=""C2"" TypeName=""."">
      <ScalarProperty Name=""M"" ParameterName=""P"" Version=""Current"" />
    </ComplexProperty>
  </ComplexProperty>
</InsertFunction>",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionMapping_should_write_association_end_bindings()
        {
            var fixture = new Fixture();

            var entityType = new EntityType();
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            new EntityContainer("EC", DataSpace.SSpace).AddEntitySetBase(entitySet);
            var associationSet = new AssociationSet("AS", new AssociationType());

            var associationEndMember1 = new AssociationEndMember("Source", new EntityType());
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(entitySet, associationSet, associationEndMember1));

            var associationEndMember2 = new AssociationEndMember("Target", new EntityType());
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(entitySet, associationSet, associationEndMember2));

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    entitySet,
                    entityType,
                    new EdmFunction("F", "N", DataSpace.SSpace, new EdmFunctionPayload()),
                    new[]
                        {
                            new StorageModificationFunctionParameterBinding(
                                new FunctionParameter(
                                "P",
                                TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                ParameterMode.In),
                                new StorageModificationFunctionMemberPath(
                                new EdmMember[]
                                    {
                                        new EdmProperty("K"),
                                        associationEndMember1
                                    },
                                associationSet),
                                true)
                        },
                    null,
                    null);

            fixture.Writer.WriteFunctionMapping("InsertFunction", storageModificationFunctionMapping);

            Assert.Equal(
                @"<InsertFunction FunctionName=""N.F"">
  <AssociationEnd AssociationSet=""AS"" From=""Source"" To=""Target"">
    <ScalarProperty Name=""K"" ParameterName=""P"" Version=""Current"" />
  </AssociationEnd>
</InsertFunction>",
                fixture.ToString());
        }

        private class Fixture
        {
            public readonly MslXmlSchemaWriter Writer;

            private readonly StringBuilder _stringBuilder;
            private readonly XmlWriter _xmlWriter;

            public Fixture()
            {
                _stringBuilder = new StringBuilder();

                _xmlWriter = XmlWriter.Create(
                    _stringBuilder,
                    new XmlWriterSettings
                        {
                            OmitXmlDeclaration = true,
                            Indent = true
                        });

                Writer = new MslXmlSchemaWriter(_xmlWriter, 3.0);
            }

            public override string ToString()
            {
                _xmlWriter.Flush();

                return _stringBuilder.ToString();
            }
        }
    }
}
