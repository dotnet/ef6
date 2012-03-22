namespace System.Data.Entity.Edm.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class EdmExtensions
    {
        internal static IEnumerable<EdmProperty> GetValidKey(this EdmEntityType entityType)
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
                            if (keyProp != null &&
                                !duplicateKeyProps.Contains(keyProp) &&
                                entityProps.Contains(keyProp) &&
                                !string.IsNullOrEmpty(keyProp.Name) &&
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

        public static List<EdmProperty> GetKeyProperties(this EdmEntityType entityType)
        {
            var visitedTypes = new HashSet<EdmEntityType>();
            var keyProperties = new List<EdmProperty>();
            GetKeyProperties(visitedTypes, entityType, keyProperties);
            return keyProperties;
        }

        private static void GetKeyProperties(
            HashSet<EdmEntityType> visitedTypes, EdmEntityType visitingType, List<EdmProperty> keyProperties)
        {
            if (visitedTypes.Contains(visitingType))
            {
                return;
            }

            visitedTypes.Add(visitingType);
            if (visitingType.BaseType != null)
            {
                GetKeyProperties(visitedTypes, visitingType.BaseType, keyProperties);
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

        internal static IEnumerable<EdmEntityType> ToHierarchy(this EdmEntityType edmType)
        {
            return SafeTraverseHierarchy(edmType);
        }

        internal static IEnumerable<EdmComplexType> ToHierarchy(this EdmComplexType edmType)
        {
            return SafeTraverseHierarchy(edmType);
        }

        private static IEnumerable<T> SafeTraverseHierarchy<T>(T startFrom)
            where T : EdmDataModelType
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

        internal static EdmAssociationEnd GetFromEnd(this EdmNavigationProperty navProp)
        {
            Contract.Assert(
                navProp.Association != null,
                "Association on EdmNavigationProperty should not be null, consider adding a null check");
            return navProp.Association.SourceEnd == navProp.ResultEnd
                       ? navProp.Association.TargetEnd
                       : navProp.Association.SourceEnd;
        }

        internal static EdmAssociationEnd PrincipalEnd(
            this EdmAssociationConstraint constraint, EdmAssociationType association)
        {
            Contract.Assert(constraint != null, "Constraint cannot be null");
            Contract.Assert(association != null, "EdmAssociationType cannot be null");
            return constraint.DependentEnd == association.SourceEnd ? association.TargetEnd : association.SourceEnd;
        }

        internal static bool IsTypeHierarchyRoot(this EdmEntityType entityType)
        {
            return entityType.BaseType == null;
        }

        internal static string GetQualifiedName(this EdmNamedMetadataItem item, string qualifiedPrefix)
        {
            return qualifiedPrefix + "." + item.Name;
        }

        internal static bool IsForeignKey(this EdmAssociationType association, double version)
        {
            if (version >= DataModelVersions.Version2
                &&
                association.Constraint != null)
            {
                // in V2, referential constraint implies foreign key
                return true;
            }
            return false;
        }
    }
}
