// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class DataModelValidationHelper
    {
        /// <summary>
        ///     Returns true if the given two ends are similar - the relationship type that this ends belongs to is the same and the entity set refered by the ends are same and they are from the same role
        /// </summary>
        /// <param name="left"> </param>
        /// <param name="right"> </param>
        /// <returns> </returns>
        internal static bool AreRelationshipEndsEqual(
            KeyValuePair<EdmAssociationSet, EdmEntitySet> left, KeyValuePair<EdmAssociationSet, EdmEntitySet> right)
        {
            if (ReferenceEquals(left.Value, right.Value)
                && ReferenceEquals(left.Key.ElementType, right.Key.ElementType))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Return true if the Referential Constraint on the association is ready for further validation, otherwise return false.
        /// </summary>
        /// <param name="association"> </param>
        /// <returns> </returns>
        internal static bool IsReferentialConstraintReadyForValidation(EdmAssociationType association)
        {
            var constraint = association.Constraint;
            if (constraint == null)
            {
                return false;
            }

            if (constraint.PrincipalEnd(association) == null
                || constraint.DependentEnd == null)
            {
                return false;
            }

            if (constraint.PrincipalEnd(association).EntityType == null
                || constraint.DependentEnd.EntityType == null)
            {
                return false;
            }

            if (constraint.DependentProperties.Count() > 0)
            {
                foreach (var propRef in constraint.DependentProperties)
                {
                    if (propRef == null)
                    {
                        return false;
                    }

                    if (propRef.PropertyType == null
                        || propRef.PropertyType.EdmType == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            var keyList = constraint.PrincipalEnd(association).EntityType.GetValidKey();

            if (keyList.Count() > 0)
            {
                foreach (var propRef in keyList)
                {
                    if (propRef == null ||
                        propRef.PropertyType == null
                        ||
                        propRef.PropertyType.EdmType == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Resolves the given property names to the property in the item Also checks whether the properties form the key for the given type and whether all the properties are nullable or not
        /// </summary>
        /// <param name="roleProperties"> </param>
        /// <param name="roleElement"> </param>
        /// <param name="isKeyProperty"> </param>
        /// <param name="areAllPropertiesNullable"> </param>
        /// <param name="isAnyPropertyNullable"> </param>
        /// <param name="isSubsetOfKeyProperties"> </param>
        internal static void IsKeyProperty(
            List<EdmProperty> roleProperties,
            EdmAssociationEnd roleElement,
            out bool isKeyProperty,
            out bool areAllPropertiesNullable,
            out bool isAnyPropertyNullable,
            out bool isSubsetOfKeyProperties)
        {
            isKeyProperty = true;
            areAllPropertiesNullable = true;
            isAnyPropertyNullable = false;
            isSubsetOfKeyProperties = true;

            if (roleElement.EntityType.GetValidKey().Count()
                != roleProperties.Count())
            {
                isKeyProperty = false;
            }

            // Checking that ToProperties must be the key properties in the entity type referred by the ToRole
            for (var i = 0; i < roleProperties.Count(); i++)
            {
                // Once we find that the properties in the constraint are not a subset of the
                // Key, one need not search for it every time
                if (isSubsetOfKeyProperties)
                {
                    var foundKeyProperty = false;
                    var keyProperties = roleElement.EntityType.GetValidKey().ToList();

                    // All properties that are defined in ToProperties must be the key property on the entity type
                    for (var j = 0; j < keyProperties.Count; j++)
                    {
                        if (keyProperties[j].Equals(roleProperties[i]))
                        {
                            foundKeyProperty = true;
                            break;
                        }
                    }

                    if (!foundKeyProperty)
                    {
                        isKeyProperty = false;
                        isSubsetOfKeyProperties = false;
                    }
                }

                // by default if IsNullable doesn't have a value, the IsNullable is true
                var isNullable = true;
                if (roleProperties[i].PropertyType.IsNullable.HasValue)
                {
                    isNullable = roleProperties[i].PropertyType.IsNullable.Value;
                }

                areAllPropertiesNullable &= isNullable;
                isAnyPropertyNullable |= isNullable;
            }
        }

        /// <summary>
        ///     Return true if the namespaceName is a Edm System Namespace
        /// </summary>
        /// <param name="namespaceName"> </param>
        /// <returns> </returns>
        internal static bool IsEdmSystemNamespace(string namespaceName)
        {
            return (namespaceName == EdmConstants.TransientNamespace ||
                    namespaceName == EdmConstants.EdmNamespace ||
                    namespaceName == EdmConstants.ClrPrimitiveTypeNamespace);
        }

        /// <summary>
        ///     Return true if the entityType is a subtype of any entity type in the dictionary keys, and return the corresponding entry EntitySet value. Otherwise return false.
        /// </summary>
        /// <param name="entityType"> </param>
        /// <param name="baseEntitySetTypes"> </param>
        /// <param name="set"> </param>
        /// <returns> </returns>
        internal static bool TypeIsSubTypeOf(
            EdmEntityType entityType, Dictionary<EdmEntityType, EdmEntitySet> baseEntitySetTypes, out EdmEntitySet set)
        {
            if (entityType.IsTypeHierarchyRoot())
            {
                // can't be a sub type if we are a base type
                set = null;
                return false;
            }

            // walk up the hierarchy looking for a base that is the base type of an entityset
            foreach (var baseType in entityType.ToHierarchy())
            {
                if (baseEntitySetTypes.ContainsKey(baseType))
                {
                    set = baseEntitySetTypes[baseType];
                    return true;
                }
            }

            set = null;
            return false;
        }

        /// <summary>
        ///     Return true if any of the properties in the EdmEntityType defines ConcurrencyMode. Otherwise return false.
        /// </summary>
        /// <param name="entityType"> </param>
        /// <returns> </returns>
        internal static bool IsTypeDefinesNewConcurrencyProperties(EdmEntityType entityType)
        {
            foreach (var property in entityType.DeclaredProperties)
            {
                if (property.PropertyType != null)
                {
                    if (property.PropertyType.PrimitiveType != null
                        && property.ConcurrencyMode != EdmConcurrencyMode.None)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Add member name to the Hash set, raise an error if the name exists already.
        /// </summary>
        /// <param name="item"> </param>
        /// <param name="memberNameList"> </param>
        /// <param name="context"> </param>
        /// <param name="getErrorString"> </param>
        internal static void AddMemberNameToHashSet(
            EdmNamedMetadataItem item,
            HashSet<string> memberNameList,
            DataModelValidationContext context,
            Func<string, string> getErrorString)
        {
            if (item.Name.HasContent())
            {
                if (!memberNameList.Add(item.Name))
                {
                    context.AddError(
                        item,
                        CsdlConstants.Attribute_Name,
                        getErrorString(item.Name),
                        XmlErrorCode.AlreadyDefined);
                }
            }
        }

        /// <summary>
        ///     If the string is null, empty, or only whitespace, return false, otherwise return true
        /// </summary>
        /// <param name="stringToCheck"> </param>
        /// <returns> </returns>
        internal static bool HasContent(this string stringToCheck)
        {
            return !string.IsNullOrWhiteSpace(stringToCheck) && !string.IsNullOrEmpty(stringToCheck);
        }

        /// <summary>
        ///     Determine if a cycle exists in the type hierarchy: use two pointers to walk the chain, if one catches up with the other, we have a cycle.
        /// </summary>
        /// <returns> true if a cycle exists in the type hierarchy, false otherwise </returns>
        internal static bool CheckForInheritanceCycle<T>(T type, Func<T, T> getBaseType)
            where T : class
        {
            var baseType = getBaseType(type);
            if (baseType != null)
            {
                var ref1 = baseType;
                var ref2 = baseType;

                do
                {
                    ref2 = getBaseType(ref2);

                    if (ReferenceEquals(ref1, ref2))
                    {
                        return true;
                    }

                    if (ref1 == null)
                    {
                        return false;
                    }

                    ref1 = getBaseType(ref1);

                    if (ref2 != null)
                    {
                        ref2 = getBaseType(ref2);
                    }
                }
                while (ref2 != null);
            }
            return false;
        }

        internal static bool IsPrimitiveTypesEqual(EdmTypeReference primitiveType1, EdmTypeReference primitiveType2)
        {
            Contract.Assert(primitiveType1.IsPrimitiveType, "primitiveType1 must be a PrimitiveType");
            Contract.Assert(primitiveType2.IsPrimitiveType, "primitiveType2 must be a PrimitiveType");
            if (primitiveType1.PrimitiveType.PrimitiveTypeKind
                == primitiveType2.PrimitiveType.PrimitiveTypeKind)
            {
                return true;
            }
            return false;
        }

        internal static bool IsEdmTypeReferenceValid(EdmTypeReference typeReference)
        {
            var visitedValidTypeReferences = new HashSet<EdmTypeReference>();
            return IsEdmTypeReferenceValid(typeReference, visitedValidTypeReferences);
        }

        private static bool IsEdmTypeReferenceValid(
            EdmTypeReference typeReference, HashSet<EdmTypeReference> visitedValidTypeReferences)
        {
            if (visitedValidTypeReferences.Contains(typeReference))
            {
                return false;
            }

            if (typeReference.EdmType == null)
            {
                return false;
            }
            else
            {
                visitedValidTypeReferences.Add(typeReference);
            }

            return true;
        }
    }
}
