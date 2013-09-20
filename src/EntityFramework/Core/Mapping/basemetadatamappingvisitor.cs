// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    internal abstract class BaseMetadataMappingVisitor
    {
        private readonly bool _sortSequence;

        protected BaseMetadataMappingVisitor(bool sortSequence)
        {
            _sortSequence = sortSequence;
        }

        protected virtual void Visit(EntityContainerMapping entityContainerMapping)
        {
            Visit(entityContainerMapping.EdmEntityContainer);
            Visit(entityContainerMapping.StorageEntityContainer);

            foreach (var mapping in GetSequence(entityContainerMapping.EntitySetMaps, it => IdentityHelper.GetIdentity(it)))
            {
                Visit(mapping);
            }
        }

        protected virtual void Visit(EntitySetBase entitySetBase)
        {
            // this is a switching node, so no object header and footer will be add for this node,
            // also this Visit won't add the object to the seen list

            switch (entitySetBase.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntitySet:
                    Visit((EntitySet)entitySetBase);
                    break;
                case BuiltInTypeKind.AssociationSet:
                    Visit((AssociationSet)entitySetBase);
                    break;
                default:
                    Debug.Fail(
                        string.Format(
                            CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", entitySetBase.BuiltInTypeKind));
                    break;
            }
        }

        protected virtual void Visit(EntitySetBaseMapping setMapping)
        {
            foreach (var typeMapping in GetSequence(setMapping.TypeMappings, it => IdentityHelper.GetIdentity(it)))
            {
                Visit(typeMapping);
            }
            Visit(setMapping.EntityContainerMapping);
        }

        protected virtual void Visit(EntityContainer entityContainer)
        {
            foreach (var set in GetSequence(entityContainer.BaseEntitySets, it => it.Identity))
            {
                Visit(set);
            }
        }

        protected virtual void Visit(EntitySet entitySet)
        {
            Visit(entitySet.ElementType);
            Visit(entitySet.EntityContainer);
        }

        protected virtual void Visit(AssociationSet associationSet)
        {
            Visit(associationSet.ElementType);
            Visit(associationSet.EntityContainer);
            foreach (var end in GetSequence(associationSet.AssociationSetEnds, it => it.Identity))
            {
                Visit(end);
            }
        }

        protected virtual void Visit(EntityType entityType)
        {
            foreach (var kmember in GetSequence(entityType.KeyMembers, it => it.Identity))
            {
                Visit(kmember);
            }

            foreach (var member in GetSequence(entityType.GetDeclaredOnlyMembers<EdmMember>(), it => it.Identity))
            {
                Visit(member);
            }

            foreach (var nproperty in GetSequence(entityType.NavigationProperties, it => it.Identity))
            {
                Visit(nproperty);
            }

            foreach (var property in GetSequence(entityType.Properties, it => it.Identity))
            {
                Visit(property);
            }
        }

        protected virtual void Visit(AssociationType associationType)
        {
            foreach (var endMember in GetSequence(associationType.AssociationEndMembers, it => it.Identity))
            {
                Visit(endMember);
            }
            Visit(associationType.BaseType);
            foreach (var keyMember in GetSequence(associationType.KeyMembers, it => it.Identity))
            {
                Visit(keyMember);
            }
            foreach (var member in GetSequence(associationType.GetDeclaredOnlyMembers<EdmMember>(), it => it.Identity))
            {
                Visit(member);
            }
            foreach (var item in GetSequence(associationType.ReferentialConstraints, it => it.Identity))
            {
                Visit(item);
            }
            foreach (var item in GetSequence(associationType.RelationshipEndMembers, it => it.Identity))
            {
                Visit(item);
            }
        }

        protected virtual void Visit(AssociationSetEnd associationSetEnd)
        {
            Visit(associationSetEnd.CorrespondingAssociationEndMember);
            Visit(associationSetEnd.EntitySet);
            Visit(associationSetEnd.ParentAssociationSet);
        }

        protected virtual void Visit(EdmProperty edmProperty)
        {
            Visit(edmProperty.TypeUsage);
        }

        protected virtual void Visit(NavigationProperty navigationProperty)
        {
            Visit(navigationProperty.FromEndMember);
            Visit(navigationProperty.RelationshipType);
            Visit(navigationProperty.ToEndMember);
            Visit(navigationProperty.TypeUsage);
        }

        protected virtual void Visit(EdmMember edmMember)
        {
            Visit(edmMember.TypeUsage);
        }

        protected virtual void Visit(AssociationEndMember associationEndMember)
        {
            Visit(associationEndMember.TypeUsage);
        }

        protected virtual void Visit(ReferentialConstraint referentialConstraint)
        {
            foreach (var property in GetSequence(referentialConstraint.FromProperties, it => it.Identity))
            {
                Visit(property);
            }
            Visit(referentialConstraint.FromRole);

            foreach (var property in GetSequence(referentialConstraint.ToProperties, it => it.Identity))
            {
                Visit(property);
            }
            Visit(referentialConstraint.ToRole);
        }

        protected virtual void Visit(RelationshipEndMember relationshipEndMember)
        {
            Visit(relationshipEndMember.TypeUsage);
        }

        protected virtual void Visit(TypeUsage typeUsage)
        {
            Visit(typeUsage.EdmType);
            foreach (var facet in GetSequence(typeUsage.Facets, it => it.Identity))
            {
                Visit(facet);
            }
        }

        protected virtual void Visit(RelationshipType relationshipType)
        {
            // switching node, will not be add to the seen list
            if (relationshipType == null)
            {
                return;
            }

            #region Inner data visit

            switch (relationshipType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.AssociationType:
                    Visit((AssociationType)relationshipType);
                    break;
                default:
                    Debug.Fail(
                        String.Format(
                            CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", relationshipType.BuiltInTypeKind));
                    break;
            }

            #endregion
        }

        protected virtual void Visit(EdmType edmType)
        {
            // switching node, will not be add to the seen list
            if (edmType == null)
            {
                return;
            }

            #region Inner data visit

            switch (edmType.BuiltInTypeKind)
            {
                case BuiltInTypeKind.EntityType:
                    Visit((EntityType)edmType);
                    break;
                case BuiltInTypeKind.AssociationType:
                    Visit((AssociationType)edmType);
                    break;
                case BuiltInTypeKind.EdmFunction:
                    Visit((EdmFunction)edmType);
                    break;
                case BuiltInTypeKind.ComplexType:
                    Visit((ComplexType)edmType);
                    break;
                case BuiltInTypeKind.PrimitiveType:
                    Visit((PrimitiveType)edmType);
                    break;
                case BuiltInTypeKind.RefType:
                    Visit((RefType)edmType);
                    break;
                case BuiltInTypeKind.CollectionType:
                    Visit((CollectionType)edmType);
                    break;
                case BuiltInTypeKind.EnumType:
                    Visit((EnumType)edmType);
                    break;
                default:
                    Debug.Fail(
                        String.Format(CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", edmType.BuiltInTypeKind));
                    break;
            }

            #endregion
        }

        protected virtual void Visit(Facet facet)
        {
            Visit(facet.FacetType);
        }

        protected virtual void Visit(EdmFunction edmFunction)
        {
            Visit(edmFunction.BaseType);
            foreach (var entitySet in GetSequence(edmFunction.EntitySets, it => it.Identity))
            {
                if (entitySet != null)
                {
                    Visit(entitySet);
                }
            }
            foreach (var functionParameter in GetSequence(edmFunction.Parameters, it => it.Identity))
            {
                Visit(functionParameter);
            }
            foreach (var returnParameter in GetSequence(edmFunction.ReturnParameters, it => it.Identity))
            {
                Visit(returnParameter);
            }
        }

        protected virtual void Visit(PrimitiveType primitiveType)
        {
        }

        protected virtual void Visit(ComplexType complexType)
        {
            Visit(complexType.BaseType);
            foreach (var member in GetSequence(complexType.Members, it => it.Identity))
            {
                Visit(member);
            }
            foreach (var property in GetSequence(complexType.Properties, it => it.Identity))
            {
                Visit(property);
            }
        }

        protected virtual void Visit(RefType refType)
        {
            Visit(refType.BaseType);
            Visit(refType.ElementType);
        }

        protected virtual void Visit(EnumType enumType)
        {
            foreach (var member in GetSequence(enumType.Members, it => it.Identity))
            {
                Visit(member);
            }
        }

        protected virtual void Visit(EnumMember enumMember)
        {
        }

        protected virtual void Visit(CollectionType collectionType)
        {
            Visit(collectionType.BaseType);
            Visit(collectionType.TypeUsage);
        }

        protected virtual void Visit(EntityTypeBase entityTypeBase)
        {
            // switching node
            if (entityTypeBase == null)
            {
                return;
            }
            switch (entityTypeBase.BuiltInTypeKind)
            {
                case BuiltInTypeKind.AssociationType:
                    Visit((AssociationType)entityTypeBase);
                    break;
                case BuiltInTypeKind.EntityType:
                    Visit((EntityType)entityTypeBase);
                    break;
                default:
                    Debug.Fail(
                        String.Format(
                            CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", entityTypeBase.BuiltInTypeKind));
                    break;
            }
        }

        protected virtual void Visit(FunctionParameter functionParameter)
        {
            Visit(functionParameter.DeclaringFunction);
            Visit(functionParameter.TypeUsage);
        }

        protected virtual void Visit(DbProviderManifest providerManifest)
        {
        }

        protected virtual void Visit(TypeMapping typeMapping)
        {
            foreach (var type in GetSequence(typeMapping.IsOfTypes, it => it.Identity))
            {
                Visit(type);
            }

            foreach (var fragment in GetSequence(typeMapping.MappingFragments, it => IdentityHelper.GetIdentity(it)))
            {
                Visit(fragment);
            }

            Visit(typeMapping.SetMapping);

            foreach (var type in GetSequence(typeMapping.Types, it => it.Identity))
            {
                Visit(type);
            }
        }

        protected virtual void Visit(MappingFragment mappingFragment)
        {
            foreach (var property in GetSequence(mappingFragment.AllProperties, it => IdentityHelper.GetIdentity(it)))
            {
                Visit(property);
            }

            Visit((EntitySetBase)mappingFragment.TableSet);
        }

        protected virtual void Visit(PropertyMapping propertyMapping)
        {
            // this is a switching node, so no object header and footer will be add for this node,
            // also this Visit won't add the object to the seen list

            if (propertyMapping.GetType()
                == typeof(ComplexPropertyMapping))
            {
                Visit((ComplexPropertyMapping)propertyMapping);
            }
            else if (propertyMapping.GetType()
                     == typeof(ConditionPropertyMapping))
            {
                Visit((ConditionPropertyMapping)propertyMapping);
            }
            else if (propertyMapping.GetType()
                     == typeof(ScalarPropertyMapping))
            {
                Visit((ScalarPropertyMapping)propertyMapping);
            }
            else
            {
                Debug.Fail(
                    String.Format(
                        CultureInfo.InvariantCulture, "Found type '{0}', did we add a new type?", propertyMapping.GetType()));
            }
        }

        protected virtual void Visit(ComplexPropertyMapping complexPropertyMapping)
        {
            Visit(complexPropertyMapping.Property);
            foreach (var mapping in GetSequence(complexPropertyMapping.TypeMappings, it => IdentityHelper.GetIdentity(it)))
            {
                Visit(mapping);
            }
        }

        protected virtual void Visit(ConditionPropertyMapping conditionPropertyMapping)
        {
            Visit(conditionPropertyMapping.Column);
            Visit(conditionPropertyMapping.Property);
        }

        protected virtual void Visit(ScalarPropertyMapping scalarPropertyMapping)
        {
            Visit(scalarPropertyMapping.Column);
            Visit(scalarPropertyMapping.Property);
        }

        protected virtual void Visit(ComplexTypeMapping complexTypeMapping)
        {
            foreach (var property in GetSequence(complexTypeMapping.AllProperties, it => IdentityHelper.GetIdentity(it)))
            {
                Visit(property);
            }

            foreach (var type in GetSequence(complexTypeMapping.IsOfTypes, it => it.Identity))
            {
                Visit(type);
            }

            foreach (var type in GetSequence(complexTypeMapping.Types, it => it.Identity))
            {
                Visit(type);
            }
        }

        protected IEnumerable<T> GetSequence<T>(IEnumerable<T> sequence, Func<T, string> keySelector)
        {
            return _sortSequence ? sequence.OrderBy(keySelector, StringComparer.Ordinal) : sequence;
        }

        // Internal for testing
        internal static class IdentityHelper
        {
            public static string GetIdentity(EntitySetBaseMapping mapping)
            {
                return mapping.Set.Identity;
            }

            public static string GetIdentity(TypeMapping mapping)
            {
                var entityTypeMapping = mapping as EntityTypeMapping;
                if (entityTypeMapping != null)
                {
                    return GetIdentity(entityTypeMapping);
                }

                var associationTypeMapping = (AssociationTypeMapping)mapping;
                return GetIdentity(associationTypeMapping);
            }

            public static string GetIdentity(EntityTypeMapping mapping)
            {
                var types = mapping.Types.Select(it => it.Identity)
                    .OrderBy(it => it, StringComparer.Ordinal);
                var isOfTypes = mapping.IsOfTypes.Select(it => it.Identity)
                    .OrderBy(it => it, StringComparer.Ordinal);
                return string.Join(",", types.Concat(isOfTypes));
            }

            public static string GetIdentity(AssociationTypeMapping mapping)
            {
                return mapping.AssociationType.Identity;
            }

            public static string GetIdentity(ComplexTypeMapping mapping)
            {
                var properties = mapping.AllProperties.Select(it => GetIdentity(it))
                    .OrderBy(it => it, StringComparer.Ordinal);
                var types = mapping.Types.Select(it => it.Identity)
                    .OrderBy(it => it, StringComparer.Ordinal);
                var isOfTypes = mapping.IsOfTypes.Select(it => it.Identity)
                    .OrderBy(it => it, StringComparer.Ordinal);
                return string.Join(",", properties.Concat(types).Concat(isOfTypes));
            }

            public static string GetIdentity(MappingFragment mapping)
            {
                return mapping.TableSet.Identity;
            }

            public static string GetIdentity(PropertyMapping mapping)
            {
                var scalarPropertyMapping = mapping as ScalarPropertyMapping;
                if (scalarPropertyMapping != null)
                {
                    return GetIdentity(scalarPropertyMapping);
                }

                var complexPropertyMapping = mapping as ComplexPropertyMapping;
                if (complexPropertyMapping != null)
                {
                    return GetIdentity(complexPropertyMapping);
                }

                var endPropertyMapping = mapping as EndPropertyMapping;
                if (endPropertyMapping != null)
                {
                    return GetIdentity(endPropertyMapping);
                }

                var conditionPropertyMapping = (ConditionPropertyMapping)mapping;
                return GetIdentity(conditionPropertyMapping);
            }

            public static string GetIdentity(ScalarPropertyMapping mapping)
            {
                return "ScalarProperty(Identity=" + mapping.Property.Identity
                    + ",ColumnIdentity=" + mapping.Column.Identity + ")";
            }

            public static string GetIdentity(ComplexPropertyMapping mapping)
            {
                return "ComplexProperty(Identity=" + mapping.Property.Identity + ")";
            }

            public static string GetIdentity(ConditionPropertyMapping mapping)
            {
                return mapping.Property != null
                    ? "ConditionProperty(Identity=" + mapping.Property.Identity + ")"
                    : "ConditionProperty(ColumnIdentity=" + mapping.Column.Identity + ")";
            }

            public static string GetIdentity(EndPropertyMapping mapping)
            {
                return "EndProperty(Identity=" + mapping.AssociationEnd.Identity + ")";
            }
        }
    }
}
