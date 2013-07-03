// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    public static class MetadataPropertyExtensions
    {
        private const string ClrTypeAnnotation = "ClrType";
        private const string ClrPropertyInfoAnnotation = "ClrPropertyInfo";
        private const string ClrAttributesAnnotation = "ClrAttributes";
        private const string ConfiguationAnnotation = "Configuration";

        public static IList<Attribute> GetClrAttributes(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return (IList<Attribute>)metadataProperties.GetAnnotation(ClrAttributesAnnotation);
        }

        public static void SetClrAttributes(
            this ICollection<MetadataProperty> metadataProperties, IList<Attribute> attributes)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotNull(attributes);

            metadataProperties.SetAnnotation(ClrAttributesAnnotation, attributes);
        }

        public static PropertyInfo GetClrPropertyInfo(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return (PropertyInfo)metadataProperties.GetAnnotation(ClrPropertyInfoAnnotation);
        }

        public static void SetClrPropertyInfo(
            this ICollection<MetadataProperty> metadataProperties, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotNull(propertyInfo);

            metadataProperties.SetAnnotation(ClrPropertyInfoAnnotation, propertyInfo);
        }

        public static Type GetClrType(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return (Type)metadataProperties.GetAnnotation(ClrTypeAnnotation);
        }

        public static void SetClrType(this ICollection<MetadataProperty> metadataProperties, Type type)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotNull(type);

            metadataProperties.SetAnnotation(ClrTypeAnnotation, type);
        }

        public static object GetConfiguration(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return metadataProperties.GetAnnotation(ConfiguationAnnotation);
        }

        public static void SetConfiguration(
            this ICollection<MetadataProperty> metadataProperties, object configuration)
        {
            DebugCheck.NotNull(metadataProperties);

            metadataProperties.SetAnnotation(ConfiguationAnnotation, configuration);
        }

        public static object GetAnnotation(this IEnumerable<MetadataProperty> metadataProperties, string name)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotEmpty(name);

            var annotation = metadataProperties.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            return annotation != null ? annotation.Value : null;
        }

        public static void SetAnnotation(
            this ICollection<MetadataProperty> metadataProperties, string name, object value)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(value);

            var annotation = metadataProperties.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            if (annotation == null)
            {
                annotation = MetadataProperty.CreateAnnotation(name, value);
                metadataProperties.Add(annotation);
            }
            else
            {
                annotation.Value = value;
            }
        }

        public static void RemoveAnnotation(this ICollection<MetadataProperty> metadataProperties, string name)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotEmpty(name);

            var annotationToRemove =
                metadataProperties.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            if (annotationToRemove != null)
            {
                metadataProperties.Remove(annotationToRemove);
            }
        }

        public static void Copy(
            this ICollection<MetadataProperty> sourceAnnotations, ICollection<MetadataProperty> targetAnnotations)
        {
            DebugCheck.NotNull(sourceAnnotations);
            DebugCheck.NotNull(targetAnnotations);

            foreach (var annotation in sourceAnnotations)
            {
                targetAnnotations.SetAnnotation(annotation.Name, annotation.Value);
            }
        }
    }
}
