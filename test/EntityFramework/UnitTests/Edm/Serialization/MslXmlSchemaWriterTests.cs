// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
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

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
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
        public void WriteFunctionMapping_should_write_simple_parameter_and_result_bindings_and_rows_affected_parameter()
        {
            var fixture = new Fixture();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            new EntityContainer("EC", DataSpace.SSpace).AddEntitySetBase(entitySet);

            var property = new EdmProperty("M");

            var typeUsage = TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

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
                                typeUsage,
                                ParameterMode.In),
                                new StorageModificationFunctionMemberPath(
                                new[] { property },
                                null),
                                true),
                            new StorageModificationFunctionParameterBinding(
                                new FunctionParameter(
                                "P_Original",
                                typeUsage,
                                ParameterMode.In),
                                new StorageModificationFunctionMemberPath(
                                new[] { property },
                                null),
                                false)
                        },
                    new FunctionParameter("RowsAffected", typeUsage, ParameterMode.Out),
                    new[]
                        {
                            new StorageModificationFunctionResultBinding("C", property)
                        });

            fixture.Writer.WriteFunctionMapping("InsertFunction", storageModificationFunctionMapping);

            Assert.Equal(
                @"<InsertFunction FunctionName=""N.F"" RowsAffectedParameter=""RowsAffected"">
  <ScalarProperty Name=""M"" ParameterName=""P"" Version=""Current"" />
  <ScalarProperty Name=""M"" ParameterName=""P_Original"" Version=""Original"" />
  <ResultBinding Name=""M"" ColumnName=""C"" />
</InsertFunction>",
                fixture.ToString());
        }

        [Fact]
        public void WriteFunctionMapping_should_write_complex_parameter_bindings()
        {
            var fixture = new Fixture();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
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

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            new EntityContainer("EC", DataSpace.SSpace).AddEntitySetBase(entitySet);
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationEndMember1 = new AssociationEndMember("Source", new EntityType("E", "N", DataSpace.CSpace));
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(entitySet, associationSet, associationEndMember1));

            var associationEndMember2 = new AssociationEndMember("Target", new EntityType("E", "N", DataSpace.CSpace));
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

        [Fact]
        public void WriteAssociationSetMapping_should_write_modification_function_mapping()
        {
            var fixture = new Fixture();

            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var entitySet = new EntitySet("ES", "S", null, null, entityType);
            new EntityContainer("EC", DataSpace.SSpace).AddEntitySetBase(entitySet);
            var associationSet = new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace));

            var associationEndMember1 = new AssociationEndMember("Source", new EntityType("E", "N", DataSpace.CSpace));
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(entitySet, associationSet, associationEndMember1));

            var associationEndMember2 = new AssociationEndMember("Target", new EntityType("E", "N", DataSpace.CSpace));
            associationSet.AddAssociationSetEnd(new AssociationSetEnd(entitySet, associationSet, associationEndMember2));

            var storageModificationFunctionMapping
                = new StorageModificationFunctionMapping(
                    associationSet,
                    associationSet.ElementType,
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
                                true),
                            new StorageModificationFunctionParameterBinding(
                                new FunctionParameter(
                                "P",
                                TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)),
                                ParameterMode.In),
                                new StorageModificationFunctionMemberPath(
                                new EdmMember[]
                                    {
                                        new EdmProperty("K"),
                                        associationEndMember2
                                    },
                                associationSet),
                                false)
                        },
                    null,
                    null);

            var associationSetMapping
                = new StorageAssociationSetMapping(
                    associationSet,
                    entitySet)
                      {
                          SourceEndMapping
                              = new StorageEndPropertyMapping(new EdmProperty("S"))
                                    {
                                        EndMember = associationEndMember1
                                    },
                          TargetEndMapping
                              = new StorageEndPropertyMapping(new EdmProperty("T"))
                                    {
                                        EndMember = associationEndMember2
                                    },
                          ModificationFunctionMapping = new StorageAssociationSetModificationFunctionMapping(
                              associationSet,
                              storageModificationFunctionMapping,
                              storageModificationFunctionMapping)
                      };

            fixture.Writer.WriteAssociationSetMappingElement(associationSetMapping);

            Assert.Equal(
                @"<AssociationSetMapping Name=""AS"" TypeName="".A"" StoreEntitySet=""E"">
  <EndProperty Name=""Source"" />
  <EndProperty Name=""Target"" />
  <ModificationFunctionMapping>
    <InsertFunction FunctionName=""N.F"">
      <EndProperty Name=""Source"">
        <ScalarProperty Name=""K"" ParameterName=""P"" Version=""Current"" />
      </EndProperty>
      <EndProperty Name=""Target"">
        <ScalarProperty Name=""K"" ParameterName=""P"" Version=""Original"" />
      </EndProperty>
    </InsertFunction>
    <DeleteFunction FunctionName=""N.F"">
      <EndProperty Name=""Source"">
        <ScalarProperty Name=""K"" ParameterName=""P"" Version=""Current"" />
      </EndProperty>
      <EndProperty Name=""Target"">
        <ScalarProperty Name=""K"" ParameterName=""P"" Version=""Original"" />
      </EndProperty>
    </DeleteFunction>
  </ModificationFunctionMapping>
</AssociationSetMapping>",
                fixture.ToString());
        }

        [Fact]
        public void WriteEntityContainerMappingElement_should_write_function_import_elements()
        {
            var typeUsage =
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            var complexTypeProperty1 = new EdmProperty("CTProperty1", typeUsage);
            var complexTypeProperty2 = new EdmProperty("CTProperty2", typeUsage);

            var complexType = new ComplexType("CT", "Ns", DataSpace.CSpace);
            complexType.AddMember(complexTypeProperty1);
            complexType.AddMember(complexTypeProperty2);

            var functionImport =
                new EdmFunction(
                    "f_c", "Ns", DataSpace.CSpace,
                    new EdmFunctionPayload
                        {
                            IsComposable = true,
                            IsFunctionImport = true,
                            ReturnParameters =
                                new[]
                                    {
                                        new FunctionParameter(
                                            "ReturnValue",
                                            TypeUsage.CreateDefaultTypeUsage(complexType.GetCollectionType()),
                                            ParameterMode.ReturnValue)
                                    },
                            Parameters =
                                new[]
                                    {
                                        new FunctionParameter("param", typeUsage, ParameterMode.Out)
                                    }
                        });


            var rowTypeProperty1 = new EdmProperty("RTProperty1", typeUsage);
            var rowTypeProperty2 = new EdmProperty("RTProperty2", typeUsage);
            var rowType = new RowType(new[] { rowTypeProperty1, rowTypeProperty2 });

            var storeFunction =
                new EdmFunction(
                    "f_s", "Ns.Store", DataSpace.SSpace,
                    new EdmFunctionPayload
                        {
                            ReturnParameters =
                                new[]
                                    {
                                        new FunctionParameter(
                                            "Return",
                                            TypeUsage.CreateDefaultTypeUsage(rowType),
                                            ParameterMode.ReturnValue),
                                    },
                            Parameters =
                                new[]
                                    {
                                        new FunctionParameter("param", typeUsage, ParameterMode.Out)
                                    }
                        });

            var structuralTypeMapping =
                new Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>(
                    complexType, new List<StorageConditionPropertyMapping>(), new List<StoragePropertyMapping>());

            structuralTypeMapping.Item3.Add(new StorageScalarPropertyMapping(complexTypeProperty1, rowTypeProperty1));
            structuralTypeMapping.Item3.Add(new StorageScalarPropertyMapping(complexTypeProperty2, rowTypeProperty2));

            var functionImportMapping = new FunctionImportMappingComposable(
                functionImport,
                storeFunction,
                new List<Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>>
                    {
                        structuralTypeMapping
                    });

            var fixture = new Fixture();
            fixture.Writer.WriteFunctionImportMappingElement(functionImportMapping);

            Assert.Equal(
                @"<FunctionImportMapping FunctionName=""Ns.Store.f_s"" FunctionImportName=""f_c"">
  <ResultMapping>
    <ComplexTypeMapping TypeName=""Ns.CT"">
      <ScalarProperty Name=""CTProperty1"" ColumnName=""RTProperty1"" />
      <ScalarProperty Name=""CTProperty2"" ColumnName=""RTProperty2"" />
    </ComplexTypeMapping>
  </ResultMapping>
</FunctionImportMapping>",
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
