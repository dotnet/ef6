// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal static class EdmEntityTypeExtensions
    {
        public static EntityType GetRootType(this EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            var rootType = entityType;

            while (rootType.BaseType != null)
            {
                rootType = (EntityType)rootType.BaseType;
            }

            return rootType;
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

            return entityType.GetRootType().DeclaredKeyProperties;
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

            var property = EdmProperty.Complex(name, complexType);

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

            var navigationProperty
                = new NavigationProperty(name, TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                      {
                          RelationshipType = associationType,
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
                      || !et.IsAncestorOf(entityType)); // et is not a parent of entityType (i.e. they can be unrelated)
        }
    }
}
