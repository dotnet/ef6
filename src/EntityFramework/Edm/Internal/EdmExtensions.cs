// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal static class EdmExtensions
    {
        internal static IEnumerable<EdmProperty> GetValidKey(this EntityType entityType)
        {
            List<EdmProperty> keyProps = null;
            foreach (var declaringType in entityType.ToHierarchy().Reverse())
            {
                if (declaringType.DeclaredKeyProperties.Count() > 0)
                {
                    if (keyProps != null)
                    {
                        // Redeclaration of key properties means the entity does not contain a valid key
                        keyProps = null;
                        return Enumerable.Empty<EdmProperty>();
                    }
                    else
                    {
                        keyProps = new List<EdmProperty>();
                        var duplicateKeyProps = new HashSet<EdmProperty>();
                        var duplicateKeyPropNames = new HashSet<string>();
                        var entityProps =
                            new HashSet<EdmProperty>(declaringType.DeclaredProperties.Where(p => p != null));
                        foreach (var keyProp in declaringType.DeclaredKeyProperties)
                        {
                            if (keyProp != null
                                &&
                                !duplicateKeyProps.Contains(keyProp)
                                &&
                                entityProps.Contains(keyProp)
                                &&
                                !string.IsNullOrEmpty(keyProp.Name)
                                &&
                                !string.IsNullOrWhiteSpace(keyProp.Name)
                                &&
                                !duplicateKeyPropNames.Contains(keyProp.Name))
                            {
                                keyProps.Add(keyProp);
                                duplicateKeyProps.Add(keyProp);
                                duplicateKeyPropNames.Add(keyProp.Name);
                            }
                            else
                            {
                                return Enumerable.Empty<EdmProperty>();
                            }
                        }
                    }
                }
            }

            return (keyProps ?? Enumerable.Empty<EdmProperty>());
        }

        public static List<EdmProperty> GetKeyProperties(this EntityType entityType)
        {
            var visitedTypes = new HashSet<EntityType>();
            var keyProperties = new List<EdmProperty>();
            GetKeyProperties(visitedTypes, entityType, keyProperties);
            return keyProperties;
        }

        private static void GetKeyProperties(
            HashSet<EntityType> visitedTypes, EntityType visitingType, List<EdmProperty> keyProperties)
        {
            if (visitedTypes.Contains(visitingType))
            {
                return;
            }

            visitedTypes.Add(visitingType);
            if (visitingType.BaseType != null)
            {
                GetKeyProperties(visitedTypes, (EntityType)visitingType.BaseType, keyProperties);
            }
            else
            {
                // only the base type can define key properties
                if (visitingType.DeclaredKeyProperties != null)
                {
                    keyProperties.AddRange(visitingType.DeclaredKeyProperties);
                }
            }
        }

        internal static IEnumerable<EntityType> ToHierarchy(this EntityType edmType)
        {
            return SafeTraverseHierarchy(edmType);
        }

        internal static IEnumerable<ComplexType> ToHierarchy(this ComplexType edmType)
        {
            return SafeTraverseHierarchy(edmType);
        }

        private static IEnumerable<T> SafeTraverseHierarchy<T>(T startFrom)
            where T : EdmType
        {
            var visitedTypes = new HashSet<T>();
            var thisType = startFrom;
            while (thisType != null
                   && !visitedTypes.Contains(thisType))
            {
                visitedTypes.Add(thisType);
                yield return thisType;
                thisType = thisType.BaseType as T;
            }
        }

        internal static AssociationEndMember GetFromEnd(this NavigationProperty navProp)
        {
            DebugCheck.NotNull(navProp.Association);
            return navProp.Association.SourceEnd == navProp.ResultEnd
                       ? navProp.Association.TargetEnd
                       : navProp.Association.SourceEnd;
        }

        internal static AssociationEndMember PrincipalEnd(
            this ReferentialConstraint constraint, AssociationType association)
        {
            DebugCheck.NotNull(constraint);
            DebugCheck.NotNull(association);
            return constraint.DependentEnd == association.SourceEnd ? association.TargetEnd : association.SourceEnd;
        }

        internal static bool IsTypeHierarchyRoot(this EntityType entityType)
        {
            return entityType.BaseType == null;
        }

        internal static bool IsForeignKey(this AssociationType association, double version)
        {
            if (version >= XmlConstants.EdmVersionForV2
                && association.Constraint != null)
            {
                // in V2, referential constraint implies foreign key
                return true;
            }
            return false;
        }
    }
}
