// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    internal abstract class EdmModelVisitor
    {
        protected static void VisitCollection<T>(IEnumerable<T> collection, Action<T> visitMethod)
        {
            if (collection != null)
            {
                foreach (var element in collection)
                {
                    visitMethod(element);
                }
            }
        }

        protected internal virtual void VisitEdmModel(EdmModel item)
        {
            if (item != null)
            {
                VisitComplexTypes(item.ComplexTypes);
                VisitEntityTypes(item.EntityTypes);
                VisitEnumTypes(item.EnumTypes);
                VisitAssociationTypes(item.AssociationTypes);
                VisitFunctions(item.Functions);
                VisitEntityContainers(item.Containers);
            }
        }

        protected virtual void VisitAnnotations(MetadataItem item, IEnumerable<MetadataProperty> annotations)
        {
            VisitCollection(annotations, VisitAnnotation);
        }

        protected virtual void VisitAnnotation(MetadataProperty item)
        {
        }

        protected internal virtual void VisitMetadataItem(MetadataItem item)
        {
            if (item != null)
            {
                if (item.Annotations.Any())
                {
                    VisitAnnotations(item, item.Annotations);
                }
            }
        }

        protected virtual void VisitEntityContainers(IEnumerable<EntityContainer> entityContainers)
        {
            VisitCollection(entityContainers, VisitEdmEntityContainer);
        }

        protected virtual void VisitEdmEntityContainer(EntityContainer item)
        {
            VisitMetadataItem(item);
            if (item != null)
            {
                if (item.EntitySets.Count > 0)
                {
                    VisitEntitySets(item, item.EntitySets);
                }

                if (item.AssociationSets.Count > 0)
                {
                    VisitAssociationSets(item, item.AssociationSets);
                }

                if (item.FunctionImports.Count > 0)
                {
                    VisitFunctionImports(item, item.FunctionImports);
                }
            }
        }

        protected internal virtual void VisitEdmFunction(EdmFunction function)
        {
            VisitMetadataItem(function);

            if (function != null)
            {
                if (function.Parameters != null)
                {
                    VisitFunctionParameters(function.Parameters);
                }

                if (function.ReturnParameters != null)
                {
                    VisitFunctionReturnParameters(function.ReturnParameters);
                }
            }
        }

        protected virtual void VisitEntitySets(EntityContainer container, IEnumerable<EntitySet> entitySets)
        {
            VisitCollection(entitySets, VisitEdmEntitySet);
        }

        protected internal virtual void VisitEdmEntitySet(EntitySet item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitAssociationSets(
            EntityContainer container, IEnumerable<AssociationSet> associationSets)
        {
            VisitCollection(associationSets, VisitEdmAssociationSet);
        }

        protected virtual void VisitEdmAssociationSet(AssociationSet item)
        {
            VisitMetadataItem(item);
            if (item.SourceSet != null)
            {
                VisitEdmAssociationSetEnd(item.SourceSet);
            }
            if (item.TargetSet != null)
            {
                VisitEdmAssociationSetEnd(item.TargetSet);
            }
        }

        protected virtual void VisitEdmAssociationSetEnd(EntitySet item)
        {
            VisitMetadataItem(item);
        }

        protected internal virtual void VisitFunctionImports(EntityContainer container, IEnumerable<EdmFunction> functionImports)
        {
            VisitCollection(functionImports, VisitFunctionImport);
        }

        protected internal virtual void VisitFunctionImport(EdmFunction functionImport)
        {
            VisitMetadataItem(functionImport);

            if (functionImport.Parameters != null)
            {
                VisitFunctionImportParameters(functionImport.Parameters);
            }

            if (functionImport.ReturnParameters != null)
            {
                VisitFunctionImportReturnParameters(functionImport.ReturnParameters);
            }
        }

        protected internal virtual void VisitFunctionImportParameters(IEnumerable<FunctionParameter> parameters)
        {
            VisitCollection(parameters, VisitFunctionImportParameter);
        }

        protected internal virtual void VisitFunctionImportParameter(FunctionParameter parameter)
        {
            VisitMetadataItem(parameter);
        }

        protected internal virtual void VisitFunctionImportReturnParameters(IEnumerable<FunctionParameter> parameters)
        {
            VisitCollection(parameters, VisitFunctionImportReturnParameter);
        }

        protected internal virtual void VisitFunctionImportReturnParameter(FunctionParameter parameter)
        {
            VisitMetadataItem(parameter);
        }

        protected virtual void VisitComplexTypes(IEnumerable<ComplexType> complexTypes)
        {
            VisitCollection(complexTypes, VisitComplexType);
        }

        protected virtual void VisitComplexType(ComplexType item)
        {
            VisitMetadataItem(item);
            if (item.Properties.Count > 0)
            {
                VisitCollection(item.Properties, VisitEdmProperty);
            }
        }

        protected virtual void VisitDeclaredProperties(ComplexType complexType, IEnumerable<EdmProperty> properties)
        {
            VisitCollection(properties, VisitEdmProperty);
        }

        protected virtual void VisitEntityTypes(IEnumerable<EntityType> entityTypes)
        {
            VisitCollection(entityTypes, VisitEdmEntityType);
        }

        protected virtual void VisitEnumTypes(IEnumerable<EnumType> enumTypes)
        {
            VisitCollection(enumTypes, VisitEdmEnumType);
        }

        protected internal virtual void VisitFunctions(IEnumerable<EdmFunction> functions)
        {
            VisitCollection(functions, VisitEdmFunction);
        }

        protected virtual void VisitFunctionParameters(IEnumerable<FunctionParameter> parameters)
        {
            VisitCollection(parameters, VisitFunctionParameter);
        }

        protected internal virtual void VisitFunctionParameter(FunctionParameter functionParameter)
        {
            VisitMetadataItem(functionParameter);
        }

        protected internal virtual void VisitFunctionReturnParameters(IEnumerable<FunctionParameter> returnParameters)
        {
            VisitCollection(returnParameters, VisitFunctionReturnParameter);
        }

        protected internal virtual void VisitFunctionReturnParameter(FunctionParameter returnParameter)
        {
            VisitMetadataItem(returnParameter);

            VisitEdmType(returnParameter.TypeUsage.EdmType);
        }

        protected internal virtual void VisitEdmType(EdmType edmType)
        {
            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.PrimitiveType:
                    VisitPrimitiveType((PrimitiveType)edmType);
                    break;
                case BuiltInTypeKind.CollectionType:
                    VisitCollectionType((CollectionType)edmType);
                    break;
                case BuiltInTypeKind.RowType:
                    VisitRowType((RowType)edmType);
                    break;
                default:
                    Debug.Fail("Unsupported EDM Type.");
                    break;
            }
        }

        protected internal virtual void VisitCollectionType(CollectionType collectionType)
        {
            VisitMetadataItem(collectionType);

            VisitEdmType(collectionType.TypeUsage.EdmType);
        }

        protected internal virtual void VisitRowType(RowType rowType)
        {
            VisitMetadataItem(rowType);

            if (rowType.DeclaredProperties.Count > 0)
            {
                VisitCollection(rowType.DeclaredProperties, VisitEdmProperty);
            }
        }

        protected internal virtual void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            VisitMetadataItem(primitiveType);
        }

        protected virtual void VisitEdmEnumType(EnumType item)
        {
            VisitMetadataItem(item);
            if (item != null)
            {
                if (item.Members.Count > 0)
                {
                    VisitEnumMembers(item, item.Members);
                }
            }
        }

        protected virtual void VisitEnumMembers(EnumType enumType, IEnumerable<EnumMember> members)
        {
            VisitCollection(members, VisitEdmEnumTypeMember);
        }

        protected internal virtual void VisitEdmEntityType(EntityType item)
        {
            VisitMetadataItem(item);
            if (item != null)
            {
                if (item.BaseType == null
                    && item.KeyProperties.Count > 0)
                {
                    VisitKeyProperties(item, item.KeyProperties);
                }

                if (item.DeclaredProperties.Count > 0)
                {
                    VisitDeclaredProperties(item, item.DeclaredProperties);
                }

                if (item.DeclaredNavigationProperties.Count > 0)
                {
                    VisitDeclaredNavigationProperties(item, item.DeclaredNavigationProperties);
                }
            }
        }

        protected virtual void VisitKeyProperties(EntityType entityType, IList<EdmProperty> properties)
        {
            VisitCollection(properties, VisitEdmProperty);
        }

        protected virtual void VisitDeclaredProperties(EntityType entityType, IList<EdmProperty> properties)
        {
            VisitCollection(properties, VisitEdmProperty);
        }

        protected virtual void VisitDeclaredNavigationProperties(
            EntityType entityType, IEnumerable<NavigationProperty> navigationProperties)
        {
            VisitCollection(navigationProperties, VisitEdmNavigationProperty);
        }

        protected virtual void VisitAssociationTypes(IEnumerable<AssociationType> associationTypes)
        {
            VisitCollection(associationTypes, VisitEdmAssociationType);
        }

        protected internal virtual void VisitEdmAssociationType(AssociationType item)
        {
            VisitMetadataItem(item);

            if (item != null)
            {
                if (item.SourceEnd != null)
                {
                    VisitEdmAssociationEnd(item.SourceEnd);
                }
                if (item.TargetEnd != null)
                {
                    VisitEdmAssociationEnd(item.TargetEnd);
                }
            }
            if (item.Constraint != null)
            {
                VisitEdmAssociationConstraint(item.Constraint);
            }
        }

        protected internal virtual void VisitEdmProperty(EdmProperty item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitEdmEnumTypeMember(EnumMember item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitEdmAssociationEnd(RelationshipEndMember item)
        {
            VisitMetadataItem(item);
        }

        protected virtual void VisitEdmAssociationConstraint(ReferentialConstraint item)
        {
            if (item != null)
            {
                VisitMetadataItem(item);
                if (item.ToRole != null)
                {
                    VisitEdmAssociationEnd(item.ToRole);
                }
                VisitCollection(item.ToProperties, VisitEdmProperty);
            }
        }

        protected virtual void VisitEdmNavigationProperty(NavigationProperty item)
        {
            VisitMetadataItem(item);
        }
    }
}
