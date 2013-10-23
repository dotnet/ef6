// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.InProcTests.Extensions
{
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class EFArtifactExtensions
    {
        #region Extension methods for getting conceptual items

        public static ComplexType GetFreshComplexType(this EFArtifact artifact, string complexTypeName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(complexTypeName), "!string.IsNullOrWhiteSpace(complexTypeName)");

            return artifact.GetConceptualChildByName<ComplexType>(complexTypeName);
        }

        public static ConceptualEntityType GetFreshConceptualEntity(this EFArtifact artifact, string entityTypeName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(entityTypeName), "!string.IsNullOrWhiteSpace(entityTypeName)");

            return artifact.GetConceptualChildByName<ConceptualEntityType>(entityTypeName);
        }

        public static EntitySet GetFreshConceptualEntitySet(this EFArtifact artifact, string entitySetName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(entitySetName), "!string.IsNullOrWhiteSpace(entitySetName)");

            return artifact.ConceptualModel()
                .EntityContainers().Single()
                .EntitySets().SingleOrDefault(e => e.Name.Value == entitySetName);
        }

        public static Association GetFreshAssociation(this EFArtifact artifact, string associationTypeName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(associationTypeName), "!string.IsNullOrWhiteSpace(associationTypeName)");

            return artifact.GetConceptualChildByName<Association>(associationTypeName);
        }

        public static AssociationEnd GetFreshAssociationEnd(this EFArtifact artifact, string associationTypeName, int index)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(associationTypeName), "!string.IsNullOrWhiteSpace(associationTypeName)");

            return GetFreshAssociation(artifact, associationTypeName).AssociationEnds()[index];
        }

        public static FunctionImport GetFreshFunctionImport(this EFArtifact artifact, string functionImportName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(functionImportName), "!string.IsNullOrWhiteSpace(functionImportName)");

            return (FunctionImport)artifact.ConceptualModel().EntityContainers().Single().GetFirstNamedChildByLocalName(functionImportName);
        }

        #endregion

        #region Extension methods for getting mapping items

        public static EntitySetMapping GetFreshEntitySetMapping(this EFArtifact artifact, string entitySetName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(entitySetName), "!string.IsNullOrWhiteSpace(entitySetName)");

            return
                artifact.MappingModel()
                    .EntityContainerMappings().Single()
                    .EntitySetMappings().SingleOrDefault(m => m.Name.RefName == entitySetName);
        }

        public static EntityTypeMapping GetFreshEntityTypeMapping(this EFArtifact artifact, string entitySetName, string entityTypeName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(entityTypeName), "!string.IsNullOrWhiteSpace(entityTypeName)");

            var entitySetMapping = GetFreshEntitySetMapping(artifact, entitySetName);

            return entitySetMapping != null
                       ? entitySetMapping.EntityTypeMappings().SingleOrDefault(m => m.TypeName.RefName == entityTypeName)
                       : null;
        }

        public static ConceptualEntityContainer GetFreshConceptualEntityContainer(this EFArtifact artifact, string entityContainerName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(entityContainerName), "!string.IsNullOrWhiteSpace(entityContainerName)");

            return
                (ConceptualEntityContainer)
                artifact.ConceptualModel().EntityContainers().SingleOrDefault(e => e.Name.Value == entityContainerName);
        }

        public static FunctionImportMapping GetFreshFunctionMapping(this EFArtifact artifact, string functionImportName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(functionImportName), "!string.IsNullOrWhiteSpace(functionImportName)");

            return
                artifact.MappingModel()
                    .EntityContainerMappings().Single()
                    .FunctionImportMappings().SingleOrDefault(m => m.FunctionImportName.RefName == functionImportName);
        }

        public static Condition GetFreshCondition(this EFArtifact artifact, string entitySetName, string columnName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(entitySetName), "!string.IsNullOrWhiteSpace(entitySetName)");
            Debug.Assert(!string.IsNullOrWhiteSpace(columnName), "!string.IsNullOrWhiteSpace(columnName)");

            return artifact.MappingModel()
                .EntityContainerMappings()
                .Single()
                .EntitySetMappings().Single(m => m.Name.RefName == entitySetName)
                .EntityTypeMappings().SelectMany(t => t.MappingFragments())
                .SelectMany(f => f.Conditions()).SingleOrDefault(c => c.ColumnName.RefName == columnName);
        }

        public static AssociationSetMapping GetFreshAssociationSetMapping(this EFArtifact artifact, string associationSetMappingName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(associationSetMappingName), "!string.IsNullOrWhiteSpace(associationSetName)");

            return artifact
                .MappingModel()
                .EntityContainerMappings().Single()
                .AssociationSetMappings().SingleOrDefault(asm => asm.Name.Target.LocalName.Value == associationSetMappingName);
        }

        public static ScalarProperty GetFreshScalarProperty(
            this EFArtifact artifact, string entitySetName, string entityTypeName, string propertyName)
        {
            var entityTypeMapping = GetFreshEntityTypeMapping(artifact, entitySetName, entityTypeName);

            return entityTypeMapping != null
                       ? entityTypeMapping
                             .MappingFragments()
                             .SelectMany(m => m.ScalarProperties())
                             .SingleOrDefault(p => p.Name.Target.LocalName.Value == propertyName)
                       : null;
        }

        #endregion

        #region Extension methods for getting storage items

        public static StorageEntityType GetFreshStorageEntity(this EFArtifact artifact, string entityTypeName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(entityTypeName), "!string.IsNullOrWhiteSpace(entityTypeName)");

            return artifact.GetStorageChildByName<StorageEntityType>(entityTypeName);
        }

        public static Function GetFreshStorageFunction(this EFArtifact artifact, string functionName)
        {
            Debug.Assert(artifact != null, "artifact != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(functionName), "!string.IsNullOrWhiteSpace(functionName)");

            return artifact.GetStorageChildByName<Function>(functionName);
        }

        public static T GetConceptualChildByName<T>(this EFArtifact artifact, string childName)
            where T : EFNameableItem
        {
            Debug.Assert(artifact != null, "artifact != null");

            var conceptualModel = artifact.ConceptualModel();

            return (T)conceptualModel.GetFirstNamedChildByLocalName(childName);
        }

        public static T GetStorageChildByName<T>(this EFArtifact artifact, string childName)
            where T : EFNameableItem
        {
            Debug.Assert(artifact != null, "artifact != null");

            var storageModel = artifact.StorageModel();

            return (T)storageModel.GetFirstNamedChildByLocalName(childName);
        }

        #endregion
    }
}
