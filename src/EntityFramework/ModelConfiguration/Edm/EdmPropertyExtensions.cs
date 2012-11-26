// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;

    internal static class EdmPropertyExtensions
    {
        public static StoreGeneratedPattern? GetStoreGeneratedPattern(this EdmProperty property)
        {
            Contract.Requires(property != null);

            return
                (StoreGeneratedPattern?)
                property.Annotations.GetAnnotation(XmlConstants.StoreGeneratedPattern);
        }

        public static void SetStoreGeneratedPattern(
            this EdmProperty property, StoreGeneratedPattern storeGeneratedPattern)
        {
            Contract.Requires(property != null);

            property.Annotations.SetAnnotation(XmlConstants.StoreGeneratedPattern, storeGeneratedPattern);
        }

        public static object GetConfiguration(this EdmProperty property)
        {
            Contract.Requires(property != null);

            return property.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this EdmProperty property, object configuration)
        {
            Contract.Requires(property != null);

            property.Annotations.SetConfiguration(configuration);
        }

        public static List<EdmPropertyPath> ToPropertyPathList(this EdmProperty property)
        {
            return ToPropertyPathList(property, new List<EdmProperty>());
        }

        public static List<EdmPropertyPath> ToPropertyPathList(this EdmProperty property, List<EdmProperty> currentPath)
        {
            var propertyPaths = new List<EdmPropertyPath>();
            IncludePropertyPath(propertyPaths, currentPath, property);
            return propertyPaths;
        }

        private static void IncludePropertyPath(
            List<EdmPropertyPath> propertyPaths, List<EdmProperty> currentPath, EdmProperty property)
        {
            currentPath.Add(property);
            if (property.IsUnderlyingPrimitiveType)
            {
                propertyPaths.Add(new EdmPropertyPath(currentPath));
            }
            else if (property.IsComplexType)
            {
                foreach (var p in property.ComplexType.Properties)
                {
                    IncludePropertyPath(propertyPaths, currentPath, p);
                }
            }
            currentPath.Remove(property);
        }
    }
}
