// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using Xunit;

    public class DbDatabaseMappingBuilderTests
    {
        private static readonly DbProviderManifest ProviderManifest =
            SqlProviderServices.Instance.GetProviderManifest("2008");

        [Fact]
        public void Build_builds_valid_DbDatabaseMapping_for_entity_types()
        {
            var storeEntityType =
                EntityType.Create("foo_S", "bar_S", DataSpace.SSpace, null, null, null);

            var modelEntityType =
                EntityType.Create("foo_C", "bar_C", DataSpace.CSpace, null, null, null);

            var storeEntitySet = EntitySet.Create("ES_S", "Ns_S", null, null, storeEntityType, null);
            var storeContainer = EntityContainer.Create("C_S", DataSpace.SSpace, new[] { storeEntitySet }, null, null);
            var storeModel = EdmModel.CreateStoreModel(storeContainer, null, null);

            var modelEntitySet = EntitySet.Create("ES_C", "Ns_C", null, null, storeEntityType, null);
            var modelContainer = EntityContainer.Create("C_C", DataSpace.CSpace, new[] { modelEntitySet }, null, null);

            var mappingContext = new SimpleMappingContext(storeModel, true);
            mappingContext.AddMapping(storeContainer, modelContainer);
            mappingContext.AddMapping(storeEntitySet, modelEntitySet);
            mappingContext.AddMapping(storeEntityType, modelEntityType);

            var dbMapping = DbDatabaseMappingBuilder.Build(mappingContext).DatabaseMapping;

            Assert.Same(storeModel, dbMapping.Database);
            var entityContainerMapping = dbMapping.EntityContainerMappings.Single();
            Assert.Same(storeContainer, entityContainerMapping.StorageEntityContainer);
            Assert.Same(modelContainer, entityContainerMapping.EdmEntityContainer);
            Assert.Equal(1, entityContainerMapping.EntitySetMappings.Count());

            Assert.NotNull(dbMapping.Model);
            Assert.Same(modelContainer, dbMapping.Model.Containers.Single());
            Assert.Same(modelEntityType, dbMapping.Model.EntityTypes.Single());
        }

        [Fact]
        public void Build_adds_association_types_to_model()
        {
            var storeEntityType =
                EntityType.Create("foo_S", "bar_S", DataSpace.SSpace, null, null, null);
            var storeEntitySet = EntitySet.Create("ES_S", "Ns_S", null, null, storeEntityType, null);
            var storeContainer = EntityContainer.Create("C_S", DataSpace.SSpace, new[] { storeEntitySet }, null, null);
            var storeModel = EdmModel.CreateStoreModel(storeContainer, null, null);

            var conceptualAssociationType =
                AssociationType.Create("AT_C", "ns", false, DataSpace.CSpace, null, null, null, null);
            var associationSet = AssociationSet.Create("AS_C", conceptualAssociationType, null, null, null);
            var modelContainer = EntityContainer.Create("C_C", DataSpace.CSpace, new[] { associationSet }, null, null);

            var mappingContext = new SimpleMappingContext(storeModel, true);
            mappingContext.AddMapping(storeContainer, modelContainer);
            mappingContext.AddMapping(new CollapsibleEntityAssociationSets(storeEntitySet), associationSet);

            var model = DbDatabaseMappingBuilder.Build(mappingContext);

            Assert.Same(conceptualAssociationType, model.GetConceptualModel().AssociationTypes.SingleOrDefault());
        }

        [Fact]
        public void Build_builds_valid_DbDatabaseMapping_for_functions()
        {
            var rowTypeProperty = CreateStoreProperty("p1", "int");

            var complexTypeProperty =
                EdmProperty.Create(
                    "p2",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));

            var functionImportReturnComplexType =
                ComplexType.Create(
                    "CT",
                    "entityModel",
                    DataSpace.CSpace,
                    new[] { complexTypeProperty }, null);

            var storeFunction = EdmFunction.Create(
                "f_s",
                "storeModel",
                DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        IsComposable = true,
                        IsFunctionImport = false,
                        ReturnParameters =
                            new[]
                                {
                                    FunctionParameter.Create(
                                        "ReturnType",
                                        RowType.Create(new[] { rowTypeProperty }, null).GetCollectionType(),
                                        ParameterMode.ReturnValue)
                                }
                    },
                null);

            var functionImport =
                EdmFunction.Create(
                    "f_c",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                        {
                            IsComposable = true,
                            IsFunctionImport = true,
                            ReturnParameters =
                                new[]
                                    {
                                        FunctionParameter.Create(
                                            "ReturnType",
                                            functionImportReturnComplexType.GetCollectionType(),
                                            ParameterMode.ReturnValue)
                                    }
                        },
                    null);

            var modelContainer = EntityContainer.Create("C_C", DataSpace.CSpace, new EntitySet[0], new[] { functionImport }, null);
            var storeContainer = EntityContainer.Create("C_S", DataSpace.SSpace, new EntitySet[0], null, null);

            var storeModel = EdmModel.CreateStoreModel(storeContainer, null, null);
            storeModel.AddItem(storeFunction);

            var mappingContext = new SimpleMappingContext(storeModel, true);
            mappingContext.AddMapping(rowTypeProperty, complexTypeProperty);
            mappingContext.AddMapping(storeFunction, functionImport);
            mappingContext.AddMapping(storeContainer, modelContainer);

            var entityModel = DbDatabaseMappingBuilder.Build(mappingContext).GetConceptualModel();

            Assert.NotNull(entityModel);
            Assert.Equal(new[] { "f_c" }, entityModel.Containers.Single().FunctionImports.Select(f => f.Name));
            Assert.Equal(new[] { "CT" }, entityModel.ComplexTypes.Select(t => t.Name));
        }

        [Fact]
        public void Build_does_not_try_map_not_mapped_functions()
        {
            var rowTypeProperty = CreateStoreProperty("p1", "int");
            var storeFunction = EdmFunction.Create(
                "f_s",
                "storeModel",
                DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        IsComposable = true,
                        IsFunctionImport = false,
                        ReturnParameters =
                            new[]
                                {
                                    FunctionParameter.Create(
                                        "ReturnType",
                                        RowType.Create(new[] { rowTypeProperty }, null).GetCollectionType(),
                                        ParameterMode.ReturnValue)
                                }
                    },
                null);

            var modelContainer = EntityContainer.Create("C_C", DataSpace.CSpace, new EntitySet[0], null, null);
            var storeContainer = EntityContainer.Create("C_S", DataSpace.SSpace, new EntitySet[0], null, null);

            var storeModel = EdmModel.CreateStoreModel(storeContainer, null, null);
            storeModel.AddItem(storeFunction);

            var mappingContext = new SimpleMappingContext(storeModel, true);
            mappingContext.AddMapping(storeContainer, modelContainer);

            var entityModel = DbDatabaseMappingBuilder.Build(mappingContext).GetConceptualModel();

            Assert.NotNull(entityModel);
            Assert.Empty(entityModel.Containers.Single().FunctionImports);
        }

        [Fact]
        public void BuildEntitySetMapping_creates_valid_entity_set_mapping()
        {
            var storeEntityType =
                EntityType.Create("foo_S", "bar_S", DataSpace.SSpace, null, null, null);

            var modelEntityType =
                EntityType.Create("foo_C", "bar_C", DataSpace.CSpace, null, null, null);

            var storeEntitySet = EntitySet.Create("ES_S", "Ns_S", null, null, storeEntityType, null);
            var storeContainer = EntityContainer.Create("C_S", DataSpace.SSpace, new[] { storeEntitySet }, null, null);
            var modelEntitySet = EntitySet.Create("ES_C", "Ns_C", null, null, storeEntityType, null);
            var modelContainer = EntityContainer.Create("C_C", DataSpace.SSpace, new[] { modelEntitySet }, null, null);

            var storageContainerMapping =
                new EntityContainerMapping(modelContainer, storeContainer, null, false, false);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(storeEntitySet, modelEntitySet);
            mappingContext.AddMapping(storeEntityType, modelEntityType);

            var entitySetMapping =
                DbDatabaseMappingBuilder.BuildEntitySetMappings(storageContainerMapping, mappingContext).Single();

            Assert.Same(modelEntitySet, entitySetMapping.EntitySet);
            Assert.Equal(1, entitySetMapping.EntityTypeMappings.Count());
        }

        [Fact]
        public void BuildEntityMapping_creates_valid_entity_mappings()
        {
            var storeEntityType =
                EntityType.Create(
                    "foo_S",
                    "bar_S",
                    DataSpace.SSpace,
                    new[] { "Id" },
                    new[] { CreateStoreProperty("Id", "int") },
                    null);

            var modelEntityType =
                EntityType.Create(
                    "foo_C",
                    "bar_C",
                    DataSpace.CSpace,
                    new[] { "C_Id" },
                    new[] { EdmProperty.CreatePrimitive("C_Id", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) },
                    null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(storeEntityType, modelEntityType);
            mappingContext.AddMapping(storeEntityType.Properties.Single(), modelEntityType.Properties.Single());

            var modelEntitySet = EntitySet.Create("ES", "Ns", null, null, modelEntityType, null);
            var modelEntityContainer = EntityContainer.Create("C", DataSpace.SSpace, new[] { modelEntitySet }, null, null);

            var storeEntitySet = EntitySet.Create("ES", "Ns.Store", null, null, storeEntityType, null);
            var storageEntitySetMapping =
                new EntitySetMapping(
                    modelEntitySet,
                    new EntityContainerMapping(modelEntityContainer, null, null, false, false));

            var typeMapping =
                DbDatabaseMappingBuilder
                    .BuildEntityTypeMapping(storageEntitySetMapping, mappingContext, storeEntitySet);

            Assert.Same(modelEntityType, typeMapping.EntityType);
            Assert.Equal(1, typeMapping.MappingFragments.Count);
            var mappingFragment = typeMapping.MappingFragments.Single();
            Assert.Equal(storeEntityType, mappingFragment.Table);
            Assert.Equal(1, mappingFragment.ColumnMappings.Count());
        }

        [Fact]
        public static void BuildAssociationSetMappings_creates_valid_association_set_mappings()
        {
            var mappingContext = CreateSimpleMappingContext(isForeignKey: false);

            var storeContainer = mappingContext.StoreContainers().Single();
            var modelContainer = mappingContext[storeContainer];
            var entityContainerMapping =
                new EntityContainerMapping(modelContainer, storeContainer, null, false, false);
            var associationSetMappings =
                DbDatabaseMappingBuilder.BuildAssociationSetMappings(entityContainerMapping, mappingContext);

            Assert.Equal(1, associationSetMappings.Count());

            var setMapping = associationSetMappings.First();

            Assert.Equal("modelAS", setMapping.Set.Name);
            Assert.Equal("storeET2", setMapping.Table.Name);
            Assert.Equal(1, setMapping.TypeMappings.Count());
            Assert.Equal(1, setMapping.TypeMappings.ElementAt(0).Types.Count);
            Assert.Equal("modelAT", setMapping.TypeMappings.ElementAt(0).Types[0].Name);
            Assert.Equal("modelEM1", setMapping.SourceEndMapping.AssociationEnd.Name);
            Assert.Equal("modelEM2", setMapping.TargetEndMapping.AssociationEnd.Name);
            Assert.Equal(1, setMapping.SourceEndMapping.Properties.Count);
            Assert.Equal(1, setMapping.TargetEndMapping.Properties.Count);
            Assert.Equal("modelSourceId", setMapping.SourceEndMapping.Properties[0].Property.Name);
            Assert.Equal("modelTargetId", setMapping.TargetEndMapping.Properties[0].Property.Name);
            Assert.Equal(1, setMapping.Conditions.Count());
            Assert.Equal("storeTargetId", setMapping.Conditions.First().Column.Name);
            Assert.Equal(false, setMapping.Conditions.First().IsNull);
        }

        [Fact]
        public void BuildAssociationSetMappings_builds_conceptual_assocation_set_mapping_for_collapsed_store_entity_sets()
        {
            #region Setting up many to many relationship in the SSpace Teacher * -- 1 TeacherStudents 1 -- * Teachers

            var joinStoreEntityType =
                EntityType.Create(
                    "TeacherStudents", "ns.Store", DataSpace.SSpace,
                    new[] { "JoinTeacherId", "JoinStudentId" },
                    new[]
                        {
                            CreateStoreProperty("JoinTeacherId", "int"),
                            CreateStoreProperty("JoinStudentId", "int")
                        }, null);

            var joinStoreEntitySet =
                EntitySet.Create("TeacherStudentsSet", "dbo", "TeacherStudentTable", null, joinStoreEntityType, null);

            var storeTeacherEntityType =
                EntityType.Create(
                    "Teacher", "ns.Store", DataSpace.SSpace, new[] { "TeacherId" },
                    new[] { CreateStoreProperty("TeacherId", "int") }, null);
            var storeTeacherEntitySet =
                EntitySet.Create("TeachersSet", "dbo", "Teachers", null, storeTeacherEntityType, null);

            var storeStudentEntityType =
                EntityType.Create(
                    "Student", "ns.Store", DataSpace.SSpace, new[] { "StudentId" },
                    new[] { CreateStoreProperty("StudentId", "int") }, null);
            var storeStudentEntitySet =
                EntitySet.Create("StudentSet", "dbo", "Students", null, storeStudentEntityType, null);

            var storeTeachersEndMember =
                AssociationEndMember.Create(
                    "Teachers", storeTeacherEntityType.GetReferenceType(), RelationshipMultiplicity.Many,
                    OperationAction.None, null);

            var storeTeacherStudentsfromTeachersEndMember =
                AssociationEndMember.Create(
                    "TeacherStudents_fromTeachers", joinStoreEntityType.GetReferenceType(), RelationshipMultiplicity.One,
                    OperationAction.None, null);

            var storeTeacherAssociationType =
                AssociationType.Create(
                    "Teacher_TeacherStudentsAssociationType", "ns.Store", false, DataSpace.SSpace,
                    storeTeachersEndMember, storeTeacherStudentsfromTeachersEndMember,
                    new ReferentialConstraint(
                        storeTeachersEndMember, storeTeacherStudentsfromTeachersEndMember, storeTeacherEntityType.KeyProperties,
                        joinStoreEntityType.KeyProperties.Where(p => p.Name == "JoinTeacherId")),
                    null);

            var storeTeacherAssociationSet =
                AssociationSet.Create(
                    "Teacher_TeacherStudents", storeTeacherAssociationType, storeTeacherEntitySet, joinStoreEntitySet, null);

            var storeStudentsEndMember =
                AssociationEndMember.Create(
                    "Students", storeStudentEntityType.GetReferenceType(), RelationshipMultiplicity.Many,
                    OperationAction.None, null);

            var storeTeacherStudentsfromStudentsEndMember =
                AssociationEndMember.Create(
                    "TeacherStudents_fromStudents", joinStoreEntityType.GetReferenceType(), RelationshipMultiplicity.One,
                    OperationAction.None, null);

            var storeStudentAssociationType =
                AssociationType.Create(
                    "Student_TeacherStudentsAssociationType", "ns.Store", false, DataSpace.SSpace,
                    storeStudentsEndMember,
                    storeTeacherStudentsfromStudentsEndMember,
                    new ReferentialConstraint(
                        storeStudentsEndMember, storeTeacherStudentsfromStudentsEndMember, storeStudentEntityType.KeyProperties,
                        joinStoreEntityType.KeyProperties.Where(p => p.Name == "JoinStudentId")),
                    null);

            var storeStudentAssociationSet =
                AssociationSet.Create(
                    "Student_TeacherStudents", storeStudentAssociationType, storeStudentEntitySet, joinStoreEntitySet, null);

            var collapsedAssociationSet = new CollapsibleEntityAssociationSets(joinStoreEntitySet);
            collapsedAssociationSet.AssociationSets.Add(storeTeacherAssociationSet);
            collapsedAssociationSet.AssociationSets.Add(storeStudentAssociationSet);

            #endregion

            #region Setting up many to many relationship in the CSpace Teacher * -- * Teachers

            var conceptualContainer = EntityContainer.Create("ConceptualContainer", DataSpace.CSpace, null, null, null);

            var edmIntTypeUsage =
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));

            var conceptualTeacherEntityType =
                EntityType.Create(
                    "Teacher", "ns", DataSpace.CSpace, new[] { "TeacherId" },
                    new[] { EdmProperty.Create("TeacherId", edmIntTypeUsage) }, null);

            var conceptualTeacherEntitySet =
                EntitySet.Create("TeachersSet", null, null, null, conceptualTeacherEntityType, null);

            var conceptualStudentEntityType =
                EntityType.Create(
                    "Student", "ns", DataSpace.CSpace, new[] { "StudentId" },
                    new[] { EdmProperty.Create("StudentId", edmIntTypeUsage) }, null);

            var conceptualStudentEntitySet =
                EntitySet.Create("StudentSet", "dbo", "Students", null, conceptualStudentEntityType, null);

            var conceptualTeachersEndMember =
                AssociationEndMember.Create(
                    "TeachersEnd", conceptualTeacherEntityType.GetReferenceType(), RelationshipMultiplicity.Many,
                    OperationAction.None, null);

            var conceptualStudentsEndMember =
                AssociationEndMember.Create(
                    "StudentsEnd", conceptualStudentEntityType.GetReferenceType(), RelationshipMultiplicity.Many,
                    OperationAction.None, null);

            var conceptualAssociationType =
                AssociationType.Create(
                    "TeacherStudentAssociation",
                    "ns.Model",
                    false,
                    DataSpace.CSpace,
                    conceptualTeachersEndMember,
                    conceptualStudentsEndMember,
                    new ReferentialConstraint(
                        conceptualTeachersEndMember, conceptualStudentsEndMember,
                        conceptualTeacherEntityType.KeyProperties, conceptualStudentEntityType.KeyProperties),
                    null);

            var conceptualAssociationSet =
                AssociationSet.Create(
                    "TeacherStudentSet", conceptualAssociationType, conceptualTeacherEntitySet,
                    conceptualStudentEntitySet, null);

            #endregion

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(collapsedAssociationSet, conceptualAssociationSet);
            mappingContext.AddMapping(storeTeachersEndMember, conceptualTeachersEndMember);
            mappingContext.AddMapping(storeStudentsEndMember, conceptualStudentsEndMember);
            mappingContext.AddMapping(
                storeTeacherAssociationSet.AssociationSetEnds.ElementAt(0),
                conceptualAssociationSet.AssociationSetEnds.ElementAt(0));
            mappingContext.AddMapping(
                storeStudentAssociationSet.AssociationSetEnds.ElementAt(0),
                conceptualAssociationSet.AssociationSetEnds.ElementAt(1));
            mappingContext.AddMapping(
                storeStudentEntityType.KeyProperties.Single(), conceptualStudentEntityType.KeyProperties.Single());
            mappingContext.AddMapping(
                storeTeacherEntityType.KeyProperties.Single(), conceptualTeacherEntityType.KeyProperties.Single());

            var storageEntitySetMapping =
                new EntityContainerMapping(conceptualContainer, null, null, false, false);

            var associationSetMapping =
                DbDatabaseMappingBuilder.BuildAssociationSetMappings(storageEntitySetMapping, mappingContext)
                    .SingleOrDefault();
            Assert.NotNull(associationSetMapping);

            var mappingFragment = associationSetMapping.TypeMappings.SingleOrDefault();
            Assert.NotNull(mappingFragment);

            var propertyMappings = mappingFragment.MappingFragments.Single().Properties;
            Assert.Equal(2, propertyMappings.Count);
            Assert.Same(conceptualTeachersEndMember, ((EndPropertyMapping)propertyMappings[0]).AssociationEnd);
            Assert.Same(conceptualStudentsEndMember, ((EndPropertyMapping)propertyMappings[1]).AssociationEnd);

            var scalarPropertyMapping = ((EndPropertyMapping)propertyMappings[0]).Properties.Single();
            Assert.Same(conceptualTeacherEntityType.KeyMembers.Single(), scalarPropertyMapping.Property);
            Assert.Same(
                joinStoreEntityType.KeyMembers.Single(m => m.Name == "JoinTeacherId"),
                scalarPropertyMapping.Column);

            scalarPropertyMapping = ((EndPropertyMapping)propertyMappings[1]).Properties.Single();
            Assert.Same(conceptualStudentEntityType.KeyMembers.Single(), scalarPropertyMapping.Property);
            Assert.Same(
                joinStoreEntityType.KeyMembers.Single(m => m.Name == "JoinStudentId"),
                scalarPropertyMapping.Column);
        }

        [Fact]
        public static void BuildAssociationSetMappings_does_not_create_mappings_if_association_type_is_foreign_key()
        {
            var mappingContext = CreateSimpleMappingContext(isForeignKey: true);

            var storeContainer = mappingContext.StoreContainers().Single();
            var modelContainer = mappingContext[storeContainer];
            var entityContainerMapping =
                new EntityContainerMapping(modelContainer, storeContainer, null, false, false);
            var associationSetMappings =
                DbDatabaseMappingBuilder.BuildAssociationSetMappings(entityContainerMapping, mappingContext);

            Assert.Equal(0, associationSetMappings.Count());
        }

        private static SimpleMappingContext CreateSimpleMappingContext(bool isForeignKey)
        {
            var int32TypeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            var storeP1 = new[] { CreateStoreProperty("storeSourceId", "int") };
            var storeP2 = new[] { CreateStoreProperty("storeTargetId", "int") };
            var modelP1 = new[] { EdmProperty.Create("modelSourceId", int32TypeUsage) };
            var modelP2 = new[] { EdmProperty.Create("modelTargetId", int32TypeUsage) };
            var storeET1 = EntityType.Create("storeET1", "N", DataSpace.SSpace, new[] { "storeSourceId" }, storeP1, null);
            var storeET2 = EntityType.Create("storeET2", "N", DataSpace.SSpace, new[] { "storeTargetId" }, storeP2, null);
            var modelET1 = EntityType.Create("modelET1", "N", DataSpace.CSpace, new[] { "modelSourceId" }, modelP1, null);
            var modelET2 = EntityType.Create("modelET2", "N", DataSpace.CSpace, new[] { "modelTargetId" }, modelP2, null);
            var storeES1 = EntitySet.Create("storeES1", null, null, null, storeET1, null);
            var storeES2 = EntitySet.Create("storeES2", null, null, null, storeET2, null);
            var modelES1 = EntitySet.Create("modelES1", null, null, null, modelET1, null);
            var modelES2 = EntitySet.Create("modelES2", null, null, null, modelET2, null);
            var storeEM1 = AssociationEndMember.Create(
                "storeEM1", storeET1.GetReferenceType(), RelationshipMultiplicity.One, OperationAction.None, null);
            var storeEM2 = AssociationEndMember.Create(
                "storeEM2", storeET2.GetReferenceType(), RelationshipMultiplicity.One, OperationAction.None, null);
            var modelEM1 = AssociationEndMember.Create(
                "modelEM1", modelET1.GetReferenceType(), RelationshipMultiplicity.Many, OperationAction.None, null);
            var modelEM2 = AssociationEndMember.Create(
                "modelEM2", modelET2.GetReferenceType(), RelationshipMultiplicity.Many, OperationAction.None, null);
            var storeRC = new ReferentialConstraint(storeEM1, storeEM2, storeP1, storeP2);
            var modelRC = new ReferentialConstraint(modelEM1, modelEM2, modelP1, modelP2);
            var storeAT = AssociationType.Create("storeAT", "N", isForeignKey, DataSpace.SSpace, storeEM1, storeEM2, storeRC, null);
            var modelAT = AssociationType.Create("modelAT", "N", isForeignKey, DataSpace.CSpace, modelEM1, modelEM2, modelRC, null);
            var storeAS = AssociationSet.Create("storeAS", storeAT, storeES1, storeES2, null);
            var modelAS = AssociationSet.Create("modelAS", modelAT, modelES1, modelES2, null);
            var storeContainer = EntityContainer.Create(
                "storeContainer", DataSpace.SSpace, new EntitySetBase[] { storeES1, storeES2, storeAS }, null, null);
            var modelContainer = EntityContainer.Create(
                "modelContainer", DataSpace.CSpace, new EntitySetBase[] { modelES1, modelES2, modelAS }, null, null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);

            mappingContext.AddMapping(storeP1[0], modelP1[0]);
            mappingContext.AddMapping(storeP2[0], modelP2[0]);
            mappingContext.AddMapping(storeET1, modelET1);
            mappingContext.AddMapping(storeET2, modelET2);
            mappingContext.AddMapping(storeES1, modelES1);
            mappingContext.AddMapping(storeES2, modelES2);
            mappingContext.AddMapping(storeEM1, modelEM1);
            mappingContext.AddMapping(storeEM2, modelEM2);
            mappingContext.AddMapping(storeAT, modelAT);
            mappingContext.AddMapping(storeAS, modelAS);
            mappingContext.AddMapping(storeAS.AssociationSetEnds[0], modelAS.AssociationSetEnds[0]);
            mappingContext.AddMapping(storeAS.AssociationSetEnds[1], modelAS.AssociationSetEnds[1]);
            mappingContext.AddMapping(storeContainer, modelContainer);

            return mappingContext;
        }

        [Fact]
        public void BuildPropertyMapping_creates_valid_property_mappings()
        {
            var storeEntityType =
                EntityType.Create(
                    "foo",
                    "bar",
                    DataSpace.SSpace,
                    new[] { "Id" },
                    new[]
                        {
                            CreateStoreProperty("Id", "int"),
                            CreateStoreProperty("FirstName", "nvarchar"),
                            CreateStoreProperty("LastName", "char")
                        },
                    null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            foreach (var storeProperty in storeEntityType.Properties)
            {
                mappingContext.AddMapping(
                    storeProperty,
                    EdmProperty.CreatePrimitive(
                        storeProperty.Name + "Model",
                        (PrimitiveType)storeProperty.TypeUsage.EdmType));
            }

            var propertyMappings =
                DbDatabaseMappingBuilder
                    .BuildPropertyMapping(storeEntityType, mappingContext);

            Assert.Equal(
                new[] { "Id", "FirstName", "LastName" },
                propertyMappings.Select(m => m.ColumnProperty.Name));

            Assert.Equal(
                new[] { "IdModel", "FirstNameModel", "LastNameModel" },
                propertyMappings.SelectMany(m => m.PropertyPath, (m, p) => p.Name));
        }

        [Fact]
        public void BuildPropertyMapping_builds_property_mappings_for_foreign_key_properties_if_foreign_keys_enabled()
        {
            var storeEntityType =
                EntityType.Create(
                    "foo",
                    "bar",
                    DataSpace.SSpace,
                    new[] { "Id" },
                    new[]
                        {
                            CreateStoreProperty("Id", "int"),
                            CreateStoreProperty("ForeignKey", "int"),
                        },
                    null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.StoreForeignKeyProperties.Add(storeEntityType.Properties.Single(p => p.Name == "ForeignKey"));

            foreach (var storeProperty in storeEntityType.Properties)
            {
                mappingContext.AddMapping(
                    storeProperty,
                    EdmProperty.CreatePrimitive(
                        storeProperty.Name + "Model",
                        (PrimitiveType)storeProperty.TypeUsage.EdmType));
            }

            var propertyMappings =
                DbDatabaseMappingBuilder
                    .BuildPropertyMapping(storeEntityType, mappingContext);

            Assert.Equal(
                new[] { "Id", "ForeignKey" },
                propertyMappings.Select(m => m.ColumnProperty.Name));

            Assert.Equal(
                new[] { "IdModel", "ForeignKeyModel" },
                propertyMappings.SelectMany(m => m.PropertyPath, (m, p) => p.Name));
        }

        [Fact]
        public void BuildPropertyMapping_does_not_build_property_mappings_for_foreign_key_properties_if_foreign_keys_disabled()
        {
            var storeEntityType =
                EntityType.Create(
                    "foo",
                    "bar",
                    DataSpace.SSpace,
                    new[] { "Id" },
                    new[]
                        {
                            CreateStoreProperty("Id", "int"),
                            CreateStoreProperty("ForeignKey", "int"),
                        },
                    null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), false);
            mappingContext.StoreForeignKeyProperties.Add(storeEntityType.Properties.Single(p => p.Name == "ForeignKey"));

            foreach (var storeProperty in storeEntityType.Properties)
            {
                mappingContext.AddMapping(
                    storeProperty,
                    EdmProperty.CreatePrimitive(
                        storeProperty.Name + "Model",
                        (PrimitiveType)storeProperty.TypeUsage.EdmType));
            }

            var propertyMappings =
                DbDatabaseMappingBuilder
                    .BuildPropertyMapping(storeEntityType, mappingContext);

            Assert.Equal(
                new[] { "Id" },
                propertyMappings.Select(m => m.ColumnProperty.Name));

            Assert.Equal(
                new[] { "IdModel" },
                propertyMappings.SelectMany(m => m.PropertyPath, (m, p) => p.Name));
        }

        [Fact]
        public void
            BuildPropertyMapping_builds_property_mappings_for_foreign_key_properties_which_are_primary_keys_even_if_foreign_keys_disabled()
        {
            var storeEntityType =
                EntityType.Create(
                    "foo",
                    "bar",
                    DataSpace.SSpace,
                    new[] { "IdAndForeignKey" },
                    new[]
                        {
                            CreateStoreProperty("IdAndForeignKey", "int"),
                        },
                    null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), false);
            mappingContext.StoreForeignKeyProperties.Add(storeEntityType.Properties.Single(p => p.Name == "IdAndForeignKey"));

            foreach (var storeProperty in storeEntityType.Properties)
            {
                mappingContext.AddMapping(
                    storeProperty,
                    EdmProperty.CreatePrimitive(
                        storeProperty.Name + "Model",
                        (PrimitiveType)storeProperty.TypeUsage.EdmType));
            }

            var propertyMappings =
                DbDatabaseMappingBuilder
                    .BuildPropertyMapping(storeEntityType, mappingContext);

            Assert.Equal(
                new[] { "IdAndForeignKey" },
                propertyMappings.Select(m => m.ColumnProperty.Name));

            Assert.Equal(
                new[] { "IdAndForeignKeyModel" },
                propertyMappings.SelectMany(m => m.PropertyPath, (m, p) => p.Name));
        }

        [Fact]
        public void BuildComposableFunctionMapping_creates_valid_function_mapping()
        {
            var rowTypeProperty = CreateStoreProperty("p1", "int");

            var complexTypeProperty =
                EdmProperty.Create(
                    "p2",
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));

            var functionImportReturnComplexType =
                ComplexType.Create(
                    "c",
                    "entityModel",
                    DataSpace.CSpace,
                    new[] { complexTypeProperty }, null);

            var storeFunction = EdmFunction.Create(
                "f_s",
                "storeModel",
                DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        IsComposable = true,
                        IsFunctionImport = false,
                        ReturnParameters =
                            new[]
                                {
                                    FunctionParameter.Create(
                                        "ReturnType",
                                        RowType.Create(new[] { rowTypeProperty }, null).GetCollectionType(),
                                        ParameterMode.ReturnValue)
                                }
                    },
                null);

            var functionImport =
                EdmFunction.Create(
                    "f_c",
                    "entityModel",
                    DataSpace.CSpace,
                    new EdmFunctionPayload
                        {
                            IsComposable = true,
                            IsFunctionImport = false,
                            ReturnParameters =
                                new[]
                                    {
                                        FunctionParameter.Create(
                                            "ReturnType",
                                            functionImportReturnComplexType.GetCollectionType(),
                                            ParameterMode.ReturnValue)
                                    }
                        },
                    null);

            var mappingContext = new SimpleMappingContext(new EdmModel(DataSpace.SSpace), true);
            mappingContext.AddMapping(rowTypeProperty, complexTypeProperty);
            mappingContext.AddMapping(storeFunction, functionImport);

            var functionImportMapping =
                DbDatabaseMappingBuilder.BuildComposableFunctionMapping(storeFunction, mappingContext);

            Assert.NotNull(functionImportMapping);
            Assert.Same(storeFunction, functionImportMapping.TargetFunction);
            Assert.Same(functionImport, functionImportMapping.FunctionImport);

            var structuralTypeMappings = functionImportMapping.StructuralTypeMappings;
            Assert.NotNull(structuralTypeMappings);

            Assert.Same(functionImportReturnComplexType, structuralTypeMappings.Single().Item1);
            Assert.Empty(structuralTypeMappings.Single().Item2);
            Assert.Same(complexTypeProperty, structuralTypeMappings.Single().Item3.Single().Property);
            Assert.Same(rowTypeProperty, ((ScalarPropertyMapping)structuralTypeMappings.Single().Item3.Single()).Column);
        }

        private static EdmProperty CreateStoreProperty(string name, string typeName)
        {
            return EdmProperty.CreatePrimitive(
                name,
                ProviderManifest.GetStoreTypes().Single(t => t.Name == typeName));
        }
    }
}
