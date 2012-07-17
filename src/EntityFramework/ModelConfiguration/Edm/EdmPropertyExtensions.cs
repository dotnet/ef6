namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;

    internal static class EdmPropertyExtensions
    {
        public static DbStoreGeneratedPattern? GetStoreGeneratedPattern(this EdmProperty property)
        {
            Contract.Requires(property != null);

            return
                (DbStoreGeneratedPattern?)
                property.Annotations.GetAnnotation(SsdlConstants.Attribute_StoreGeneratedPattern);
        }

        public static void SetStoreGeneratedPattern(
            this EdmProperty property, DbStoreGeneratedPattern storeGeneratedPattern)
        {
            Contract.Requires(property != null);

            property.Annotations.SetAnnotation(SsdlConstants.Attribute_StoreGeneratedPattern, storeGeneratedPattern);
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

        public static EdmProperty AsPrimitive(this EdmProperty property)
        {
            Contract.Requires(property != null);

            property.PropertyType = new EdmTypeReference
                {
                    PrimitiveTypeFacets = new EdmPrimitiveTypeFacets()
                };

            return property;
        }

        public static EdmProperty AsComplex(this EdmProperty property, EdmComplexType complexType)
        {
            Contract.Requires(property != null);
            Contract.Requires(complexType != null);

            property.PropertyType = new EdmTypeReference
                {
                    EdmType = complexType,
                    IsNullable = false
                };

            return property;
        }

        public static EdmProperty AsEnum(this EdmProperty property, EdmEnumType enumType)
        {
            Contract.Requires(property != null);
            Contract.Requires(enumType != null);

            property.PropertyType = new EdmTypeReference
                {
                    EdmType = enumType,
                    PrimitiveTypeFacets = new EdmPrimitiveTypeFacets()
                };

            return property;
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
            if (property.PropertyType.IsUnderlyingPrimitiveType)
            {
                propertyPaths.Add(new EdmPropertyPath(currentPath));
            }
            else if (property.PropertyType.IsComplexType)
            {
                foreach (var p in property.PropertyType.ComplexType.Properties)
                {
                    IncludePropertyPath(propertyPaths, currentPath, p);
                }
            }
            currentPath.Remove(property);
        }
    }
}
