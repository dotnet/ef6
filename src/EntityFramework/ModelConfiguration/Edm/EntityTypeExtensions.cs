// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal static class EntityTypeExtensions
    {
        private const string TableNameAnnotation = "TableName";
        private const string KeyNamesTypeAnnotation = "KeyNamesType";

        public static void AddColumn(this EntityType table, EdmProperty column)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(column);

            column.SetPreferredName(column.Name);
            column.Name = table.Properties.UniquifyName(column.Name);

            table.AddMember(column);
        }

        public static void SetConfiguration(this EntityType table, object configuration)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(configuration);

            table.GetMetadataProperties().SetConfiguration(configuration);
        }

        public static DatabaseName GetTableName(this EntityType table)
        {
            DebugCheck.NotNull(table);

            return (DatabaseName)table.Annotations.GetAnnotation(TableNameAnnotation);
        }

        public static void SetTableName(this EntityType table, DatabaseName tableName)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(tableName);

            table.GetMetadataProperties().SetAnnotation(TableNameAnnotation, tableName);
        }

        public static EntityType GetKeyNamesType(this EntityType table)
        {
            DebugCheck.NotNull(table);

            return (EntityType)table.Annotations.GetAnnotation(KeyNamesTypeAnnotation);
        }

        public static void SetKeyNamesType(this EntityType table, EntityType entityType)
        {
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(entityType);

            table.GetMetadataProperties().SetAnnotation(KeyNamesTypeAnnotation, entityType);
        }

        internal static IEnumerable<EntityType> ToHierarchy(this EntityType edmType)
        {
            return EdmType.SafeTraverseHierarchy(edmType);
        }

        public static IEnumerable<EdmProperty> GetValidKey(this EntityType entityType)
        {
            List<EdmProperty> keyProps = null;

            foreach (var declaringType in entityType.ToHierarchy().Reverse())
            {
                if (declaringType.BaseType == null
                    && declaringType.KeyProperties.Count > 0)
                {
                    if (keyProps != null)
                    {
                        // Redeclaration of key properties means the entity does not contain a valid key
                        return Enumerable.Empty<EdmProperty>();
                    }

                    keyProps = new List<EdmProperty>();
                    var duplicateKeyProps = new HashSet<EdmProperty>();
                    var duplicateKeyPropNames = new HashSet<string>();
                    var entityProps =
                        new HashSet<EdmProperty>(declaringType.DeclaredProperties.Where(p => p != null));

                    foreach (var keyProp in declaringType.KeyProperties)
                    {
                        if (keyProp != null
                            && !duplicateKeyProps.Contains(keyProp)
                            && entityProps.Contains(keyProp)
                            && !string.IsNullOrEmpty(keyProp.Name)
                            && !string.IsNullOrWhiteSpace(keyProp.Name)
                            && !duplicateKeyPropNames.Contains(keyProp.Name))
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
                var visitingTypeKeyProperties = visitingType.KeyProperties;
                if (visitingTypeKeyProperties.Count > 0)
                {
                    keyProperties.AddRange(visitingTypeKeyProperties);
                }
            }
        }

        public static EntityType GetRootType(this EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            EdmType rootType = entityType;

            while (rootType.BaseType != null)
            {
                rootType = rootType.BaseType;
            }

            return (EntityType)rootType;
        }

        public static bool IsAncestorOf(this EntityType ancestor, EntityType entityType)
        {
            DebugCheck.NotNull(ancestor);
            DebugCheck.NotNull(entityType);

            while (entityType != null)
            {
                if (entityType.BaseType == ancestor)
                {
                    return true;
                }
                entityType = (EntityType)entityType.BaseType;
            }
            return false;
        }

        public static IEnumerable<EdmProperty> KeyProperties(this EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            return entityType.GetRootType().KeyProperties;
        }

        public static object GetConfiguration(this EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            return entityType.Annotations.GetConfiguration();
        }

        public static Type GetClrType(this EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            return entityType.Annotations.GetClrType();
        }

        // Depth-first, pre-order visitor.
        // Note that the pre-order traversal is important for correctness of the transformations.
        public static IEnumerable<EntityType> TypeHierarchyIterator(this EntityType entityType, EdmModel model)
        {
            DebugCheck.NotNull(entityType);

            yield return entityType;

            var derivedEntityTypes = model.GetDerivedTypes(entityType);

            if (derivedEntityTypes != null)
            {
                foreach (var derivedEntityType in derivedEntityTypes)
                {
                    foreach (var derivedEntityType2 in derivedEntityType.TypeHierarchyIterator(model))
                    {
                        yield return derivedEntityType2;
                    }
                }
            }
        }

        public static EdmProperty AddComplexProperty(
            this EntityType entityType, string name, ComplexType complexType)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(complexType);

            var property = EdmProperty.CreateComplex(name, complexType);

            entityType.AddMember(property);

            return property;
        }

        public static EdmProperty GetDeclaredPrimitiveProperty(this EntityType entityType, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(propertyInfo);

            return entityType
                .GetDeclaredPrimitiveProperties()
                .SingleOrDefault(p => p.GetClrPropertyInfo().IsSameAs(propertyInfo));
        }

        public static IEnumerable<EdmProperty> GetDeclaredPrimitiveProperties(this EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            return entityType.DeclaredProperties.Where(p => p.IsUnderlyingPrimitiveType);
        }

        public static NavigationProperty AddNavigationProperty(
            this EntityType entityType, string name, AssociationType associationType)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(associationType);

            var targetEntityType
                = associationType.TargetEnd.GetEntityType();

            var typeUsage
                = associationType.TargetEnd.RelationshipMultiplicity.IsMany()
                      ? (EdmType)targetEntityType.GetCollectionType()
                      : targetEntityType;

            var navigationProperty
                = new NavigationProperty(name, TypeUsage.Create(typeUsage))
                    {
                        RelationshipType = associationType,
                        FromEndMember = associationType.SourceEnd,
                        ToEndMember = associationType.TargetEnd
                    };

            entityType.AddMember(navigationProperty);

            return navigationProperty;
        }

        public static NavigationProperty GetNavigationProperty(
            this EntityType entityType, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(propertyInfo);

            return entityType.NavigationProperties.SingleOrDefault(np => np.GetClrPropertyInfo().IsSameAs(propertyInfo));
        }

        public static bool IsRootOfSet(this EntityType entityType, IEnumerable<EntityType> set)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(set);

            return set.All(
                et => et == entityType // same type
                      || entityType.IsAncestorOf(et) // entityType is parent of et
                      || et.GetRootType() != entityType.GetRootType()); // unrelated
        }
    }
}
