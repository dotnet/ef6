// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal static class EdmEntityTypeExtensions
    {
        public static EdmEntityType GetRootType(this EdmEntityType entityType)
        {
            Contract.Requires(entityType != null);

            var rootType = entityType;

            while (rootType.BaseType != null)
            {
                rootType = rootType.BaseType;
            }

            return rootType;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static bool IsAncestorOf(this EdmEntityType ancestor, EdmEntityType entityType)
        {
            Contract.Requires(ancestor != null);
            Contract.Requires(entityType != null);

            while (entityType != null)
            {
                if (entityType.BaseType == ancestor)
                {
                    return true;
                }
                entityType = entityType.BaseType;
            }
            return false;
        }

        public static IEnumerable<EdmProperty> KeyProperties(this EdmEntityType entityType)
        {
            Contract.Requires(entityType != null);

            return entityType.GetRootType().DeclaredKeyProperties;
        }

        public static object GetConfiguration(this EdmEntityType entityType)
        {
            Contract.Requires(entityType != null);

            return entityType.Annotations.GetConfiguration();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static void SetConfiguration(this EdmEntityType entityType, object configuration)
        {
            Contract.Requires(entityType != null);

            entityType.Annotations.SetConfiguration(configuration);
        }

        public static Type GetClrType(this EdmEntityType entityType)
        {
            Contract.Requires(entityType != null);

            return entityType.Annotations.GetClrType();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static void SetClrType(this EdmEntityType entityType, Type type)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(type != null);

            entityType.Annotations.SetClrType(type);
        }

        // Depth-first, pre-order visitor.
        // Note that the pre-order traversal is important for correctness of the transformations.
        public static IEnumerable<EdmEntityType> TypeHierarchyIterator(this EdmEntityType entityType, EdmModel model)
        {
            Contract.Requires(entityType != null);

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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmProperty AddPrimitiveProperty(this EdmEntityType entityType, string name)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var property = new EdmProperty().AsPrimitive();

            property.Name = name;

            entityType.DeclaredProperties.Add(property);

            return property;
        }

        public static EdmProperty AddComplexProperty(
            this EdmEntityType entityType, string name, EdmComplexType complexType)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(complexType != null);

            var property = new EdmProperty
                               {
                                   Name = name
                               }.AsComplex(complexType);

            entityType.DeclaredProperties.Add(property);

            return property;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmProperty GetDeclaredPrimitiveProperty(this EdmEntityType entityType, string name)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            return entityType.GetDeclaredPrimitiveProperties().SingleOrDefault(p => p.Name == name);
        }

        public static EdmProperty GetDeclaredPrimitiveProperty(this EdmEntityType entityType, PropertyInfo propertyInfo)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(propertyInfo != null);

            return entityType
                .GetDeclaredPrimitiveProperties()
                .SingleOrDefault(p => p.GetClrPropertyInfo().IsSameAs(propertyInfo));
        }

        public static IEnumerable<EdmProperty> GetDeclaredPrimitiveProperties(this EdmEntityType entityType)
        {
            Contract.Requires(entityType != null);

            return entityType.DeclaredProperties.Where(p => p.PropertyType.IsUnderlyingPrimitiveType);
        }

        public static EdmNavigationProperty AddNavigationProperty(
            this EdmEntityType entityType, string name, EdmAssociationType associationType)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(associationType != null);

            var navigationProperty = new EdmNavigationProperty
                                         {
                                             Name = name,
                                             Association = associationType,
                                             ResultEnd = associationType.TargetEnd
                                         };

            entityType.DeclaredNavigationProperties.Add(navigationProperty);

            return navigationProperty;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmNavigationProperty GetNavigationProperty(this EdmEntityType entityType, string name)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            return entityType.NavigationProperties.SingleOrDefault(np => np.Name == name);
        }

        public static EdmNavigationProperty GetNavigationProperty(
            this EdmEntityType entityType, PropertyInfo propertyInfo)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(propertyInfo != null);

            return entityType.NavigationProperties.SingleOrDefault(np => np.GetClrPropertyInfo().IsSameAs(propertyInfo));
        }

        public static bool IsRootOfSet(this EdmEntityType entityType, IEnumerable<EdmEntityType> set)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(set != null);

            return set.All(
                et => et == entityType // same type
                      || entityType.IsAncestorOf(et) // entityType is parent of et
                      || !et.IsAncestorOf(entityType)); // et is not a parent of entityType (i.e. they can be unrelated)
        }
    }
}
