// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Ensures that all metadata in a given expression tree is from the specified metadata workspace,
    ///     potentially rebinding and rebuilding the expressions to appropriate replacement metadata where necessary.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rebinder")]
    public class DbExpressionRebinder : DefaultExpressionVisitor
    {
        private readonly MetadataWorkspace _metadata;
        private readonly Perspective _perspective;

        internal DbExpressionRebinder()
        {
        }

        protected DbExpressionRebinder(MetadataWorkspace targetWorkspace)
        {
            Debug.Assert(targetWorkspace != null, "Metadata workspace is null");
            _metadata = targetWorkspace;
            _perspective = new ModelPerspective(targetWorkspace);
        }

        protected override EntitySetBase VisitEntitySet(EntitySetBase entitySet)
        {
            EntityContainer container;
            if (_metadata.TryGetEntityContainer(entitySet.EntityContainer.Name, entitySet.EntityContainer.DataSpace, out container))
            {
                EntitySetBase extent = null;
                if (container.BaseEntitySets.TryGetValue(entitySet.Name, false, out extent) &&
                    extent != null
                    &&
                    entitySet.BuiltInTypeKind == extent.BuiltInTypeKind) // EntitySet -> EntitySet, AssociationSet -> AssociationSet, etc
                {
                    return extent;
                }

                throw new ArgumentException(Strings.Cqt_Copier_EntitySetNotFound(entitySet.EntityContainer.Name, entitySet.Name));
            }

            throw new ArgumentException(Strings.Cqt_Copier_EntityContainerNotFound(entitySet.EntityContainer.Name));
        }

        protected override EdmFunction VisitFunction(EdmFunction functionMetadata)
        {
            var paramTypes = new List<TypeUsage>(functionMetadata.Parameters.Count);
            foreach (var funcParam in functionMetadata.Parameters)
            {
                var mappedParamType = VisitTypeUsage(funcParam.TypeUsage);
                paramTypes.Add(mappedParamType);
            }

            if (DataSpace.SSpace
                == functionMetadata.DataSpace)
            {
                EdmFunction foundFunc = null;
                if (_metadata.TryGetFunction(
                    functionMetadata.Name,
                    functionMetadata.NamespaceName,
                    paramTypes.ToArray(),
                    false /* ignoreCase */,
                    functionMetadata.DataSpace,
                    out foundFunc)
                    &&
                    foundFunc != null)
                {
                    return foundFunc;
                }
            }
            else
            {
                // Find the function or function import.
                IList<EdmFunction> candidateFunctions;
                if (_perspective.TryGetFunctionByName(
                    functionMetadata.NamespaceName, functionMetadata.Name, /*ignoreCase:*/ false, out candidateFunctions))
                {
                    Debug.Assert(
                        null != candidateFunctions && candidateFunctions.Count > 0,
                        "Perspective.TryGetFunctionByName returned true with null/empty function result list");

                    bool isAmbiguous;
                    var retFunc = FunctionOverloadResolver.ResolveFunctionOverloads(
                        candidateFunctions, paramTypes, /*isGroupAggregateFunction:*/ false, out isAmbiguous);
                    if (!isAmbiguous
                        &&
                        retFunc != null)
                    {
                        return retFunc;
                    }
                }
            }

            throw new ArgumentException(
                Strings.Cqt_Copier_FunctionNotFound(TypeHelpers.GetFullName(functionMetadata.NamespaceName, functionMetadata.Name)));
        }

        protected override EdmType VisitType(EdmType type)
        {
            var retType = type;

            if (BuiltInTypeKind.RefType
                == type.BuiltInTypeKind)
            {
                var refType = (RefType)type;
                var mappedEntityType = (EntityType)VisitType(refType.ElementType);
                if (!ReferenceEquals(refType.ElementType, mappedEntityType))
                {
                    retType = new RefType(mappedEntityType);
                }
            }
            else if (BuiltInTypeKind.CollectionType
                     == type.BuiltInTypeKind)
            {
                var collectionType = (CollectionType)type;
                var mappedElementType = VisitTypeUsage(collectionType.TypeUsage);
                if (!ReferenceEquals(collectionType.TypeUsage, mappedElementType))
                {
                    retType = new CollectionType(mappedElementType);
                }
            }
            else if (BuiltInTypeKind.RowType
                     == type.BuiltInTypeKind)
            {
                var rowType = (RowType)type;
                List<KeyValuePair<string, TypeUsage>> mappedPropInfo = null;
                for (var idx = 0; idx < rowType.Properties.Count; idx++)
                {
                    var originalProp = rowType.Properties[idx];
                    var mappedPropType = VisitTypeUsage(originalProp.TypeUsage);
                    if (!ReferenceEquals(originalProp.TypeUsage, mappedPropType))
                    {
                        if (mappedPropInfo == null)
                        {
                            mappedPropInfo = new List<KeyValuePair<string, TypeUsage>>(
                                rowType.Properties.Select(
                                    prop => new KeyValuePair<string, TypeUsage>(prop.Name, prop.TypeUsage)
                                    ));
                        }
                        mappedPropInfo[idx] = new KeyValuePair<string, TypeUsage>(originalProp.Name, mappedPropType);
                    }
                }
                if (mappedPropInfo != null)
                {
                    var mappedProps = mappedPropInfo.Select(propInfo => new EdmProperty(propInfo.Key, propInfo.Value));
                    retType = new RowType(mappedProps, rowType.InitializerMetadata);
                }
            }
            else
            {
                if (!_metadata.TryGetType(type.Name, type.NamespaceName, type.DataSpace, out retType)
                    ||
                    null == retType)
                {
                    throw new ArgumentException(Strings.Cqt_Copier_TypeNotFound(TypeHelpers.GetFullName(type.NamespaceName, type.Name)));
                }
            }

            return retType;
        }

        protected override TypeUsage VisitTypeUsage(TypeUsage type)
        {
            //
            // If the target metatadata workspace contains the same type instances, then the type does not
            // need to be 'mapped' and the same TypeUsage instance may be returned. This can happen if the
            // target workspace and the workspace of the source Command Tree are using the same ItemCollection.
            //
            var retEdmType = VisitType(type.EdmType);
            if (ReferenceEquals(retEdmType, type.EdmType))
            {
                return type;
            }

            //
            // Retrieve the Facets from this type usage so that
            // 1) They can be used to map the type if it is a primitive type
            // 2) They can be applied to the new type usage that references the mapped type
            //
            var facets = new Facet[type.Facets.Count];
            var idx = 0;
            foreach (var f in type.Facets)
            {
                facets[idx] = f;
                idx++;
            }

            return TypeUsage.Create(retEdmType, facets);
        }

        private static bool TryGetMember<TMember>(DbExpression instance, string memberName, out TMember member) where TMember : EdmMember
        {
            member = null;
            var declType = instance.ResultType.EdmType as StructuralType;
            if (declType != null)
            {
                EdmMember foundMember = null;
                if (declType.Members.TryGetValue(memberName, false, out foundMember))
                {
                    member = foundMember as TMember;
                }
            }

            return (member != null);
        }

        public override DbExpression Visit(DbPropertyExpression expression)
        {
            DbExpression result = expression;
            var newInstance = VisitExpression(expression.Instance);
            if (!ReferenceEquals(expression.Instance, newInstance))
            {
                if (Helper.IsRelationshipEndMember(expression.Property))
                {
                    RelationshipEndMember endMember;
                    if (!TryGetMember(newInstance, expression.Property.Name, out endMember))
                    {
                        var type = newInstance.ResultType.EdmType;
                        throw new ArgumentException(
                            Strings.Cqt_Copier_EndNotFound(
                                expression.Property.Name, TypeHelpers.GetFullName(type.NamespaceName, type.Name)));
                    }
                    result = newInstance.Property(endMember);
                }
                else if (Helper.IsNavigationProperty(expression.Property))
                {
                    NavigationProperty navProp;
                    if (!TryGetMember(newInstance, expression.Property.Name, out navProp))
                    {
                        var type = newInstance.ResultType.EdmType;
                        throw new ArgumentException(
                            Strings.Cqt_Copier_NavPropertyNotFound(
                                expression.Property.Name, TypeHelpers.GetFullName(type.NamespaceName, type.Name)));
                    }
                    result = newInstance.Property(navProp);
                }
                else
                {
                    EdmProperty prop;
                    if (!TryGetMember(newInstance, expression.Property.Name, out prop))
                    {
                        var type = newInstance.ResultType.EdmType;
                        throw new ArgumentException(
                            Strings.Cqt_Copier_PropertyNotFound(
                                expression.Property.Name, TypeHelpers.GetFullName(type.NamespaceName, type.Name)));
                    }
                    result = newInstance.Property(prop);
                }
            }
            return result;
        }
    }
}
