// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for <see cref="MetadataProperty"/>.
    /// </summary>
    internal static class MetadataPropertyExtensions
    {
        private const string ClrTypeAnnotation = "ClrType";
        private const string ClrPropertyInfoAnnotation = "ClrPropertyInfo";
        private const string ClrAttributesAnnotation = "ClrAttributes";
        private const string ConfiguationAnnotation = "Configuration";

        /// <summary>
        /// Gets the CLR attributes defined on a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to get attributes from.</param>
        /// <returns>The attributes.</returns>
        public static IList<Attribute> GetClrAttributes(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return (IList<Attribute>)metadataProperties.GetAnnotation(ClrAttributesAnnotation);
        }

        /// <summary>
        /// Sets the CLR attributes on a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to set attributes on.</param>
        /// <param name="attributes">The attributes to be set.</param>
        public static void SetClrAttributes(
            this ICollection<MetadataProperty> metadataProperties, IList<Attribute> attributes)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotNull(attributes);

            metadataProperties.SetAnnotation(ClrAttributesAnnotation, attributes);
        }

        /// <summary>
        /// Gets the CLR property info for a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to get CLR property info for.</param>
        /// <returns>The CLR property info</returns>
        public static PropertyInfo GetClrPropertyInfo(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return (PropertyInfo)metadataProperties.GetAnnotation(ClrPropertyInfoAnnotation);
        }

        /// <summary>
        /// Sets the CLR property info for a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to set CLR property info for.</param>
        /// <param name="propertyInfo">The property info.</param>
        public static void SetClrPropertyInfo(
            this ICollection<MetadataProperty> metadataProperties, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotNull(propertyInfo);

            metadataProperties.SetAnnotation(ClrPropertyInfoAnnotation, propertyInfo);
        }

        /// <summary>
        /// Gets the CLR type for a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to get the CLR type for.</param>
        /// <returns>The CLR type.</returns>
        public static Type GetClrType(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return (Type)metadataProperties.GetAnnotation(ClrTypeAnnotation);
        }

        /// <summary>
        /// Sets the CLR type for a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to set the CLR type for.</param>
        /// <param name="type">The CLR type.</param>
        public static void SetClrType(this ICollection<MetadataProperty> metadataProperties, Type type)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotNull(type);

            metadataProperties.SetAnnotation(ClrTypeAnnotation, type);
        }

        /// <summary>
        /// Gets the configuration for a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to get the configuration for.</param>
        /// <returns>The configuration.</returns>
        public static object GetConfiguration(this IEnumerable<MetadataProperty> metadataProperties)
        {
            DebugCheck.NotNull(metadataProperties);

            return metadataProperties.GetAnnotation(ConfiguationAnnotation);
        }

        /// <summary>
        /// Sets the configuration for a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties to set the configuration for.</param>
        /// <param name="configuration">The configuration.</param>
        public static void SetConfiguration(
            this ICollection<MetadataProperty> metadataProperties, object configuration)
        {
            DebugCheck.NotNull(metadataProperties);

            metadataProperties.SetAnnotation(ConfiguationAnnotation, configuration);
        }

        /// <summary>
        /// Gets the annotation from a set of properties. 
        /// </summary>
        /// <param name="metadataProperties">The properties.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <returns>The annotation.</returns>
        public static object GetAnnotation(this IEnumerable<MetadataProperty> metadataProperties, string name)
        {
            DebugCheck.NotNull(metadataProperties);
            DebugCheck.NotEmpty(name);

            var annotation = metadataProperties.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            return annotation != null ? annotation.Value : null;
        }

        /// <summary>
        /// Sets an annotation on a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="value">The value of the annotation.</param>
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

        /// <summary>
        /// Removes an annotation from a set of properties.
        /// </summary>
        /// <param name="metadataProperties">The properties.</param>
        /// <param name="name">The name of the annotation.</param>
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

        /// <summary>
        /// Copies annotations from one set of properties to another.
        /// </summary>
        /// <param name="sourceAnnotations">The source properties.</param>
        /// <param name="targetAnnotations">The target properties.</param>
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
