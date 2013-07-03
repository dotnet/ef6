// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal static class DataModelAnnotationExtensions
    {
        private const string ClrTypeAnnotation = "ClrType";
        private const string ClrPropertyInfoAnnotation = "ClrPropertyInfo";
        private const string ClrAttributesAnnotation = "ClrAttributes";
        private const string ConfiguationAnnotation = "Configuration";

        public static Type GetClrType(this IEnumerable<DataModelAnnotation> dataModelAnnotations)
        {
            DebugCheck.NotNull(dataModelAnnotations);

            return (Type)dataModelAnnotations.GetAnnotation(ClrTypeAnnotation);
        }

        public static void SetClrType(this ICollection<DataModelAnnotation> dataModelAnnotations, Type type)
        {
            DebugCheck.NotNull(dataModelAnnotations);
            DebugCheck.NotNull(type);

            dataModelAnnotations.SetAnnotation(ClrTypeAnnotation, type);
        }

        public static object GetConfiguration(this IEnumerable<DataModelAnnotation> dataModelAnnotations)
        {
            DebugCheck.NotNull(dataModelAnnotations);

            return dataModelAnnotations.GetAnnotation(ConfiguationAnnotation);
        }

        public static void SetConfiguration(
            this ICollection<DataModelAnnotation> dataModelAnnotations, object configuration)
        {
            DebugCheck.NotNull(dataModelAnnotations);

            dataModelAnnotations.SetAnnotation(ConfiguationAnnotation, configuration);
        }

        public static object GetAnnotation(this IEnumerable<DataModelAnnotation> dataModelAnnotations, string name)
        {
            DebugCheck.NotNull(dataModelAnnotations);
            DebugCheck.NotEmpty(name);

            var annotation = dataModelAnnotations.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            return annotation != null ? annotation.Value : null;
        }

        public static void SetAnnotation(
            this ICollection<DataModelAnnotation> dataModelAnnotations, string name, object value)
        {
            DebugCheck.NotNull(dataModelAnnotations);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(value);

            var annotation = dataModelAnnotations.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            if (annotation == null)
            {
                dataModelAnnotations.Add(
                    annotation = new DataModelAnnotation
                        {
                            Name = name
                        });
            }

            annotation.Value = value;
        }

        public static void RemoveAnnotation(this ICollection<DataModelAnnotation> dataModelAnnotations, string name)
        {
            DebugCheck.NotNull(dataModelAnnotations);
            DebugCheck.NotEmpty(name);

            var annotationToRemove =
                dataModelAnnotations.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            if (annotationToRemove != null)
            {
                dataModelAnnotations.Remove(annotationToRemove);
            }
        }

        public static void Copy(
            this ICollection<DataModelAnnotation> sourceAnnotations, ICollection<DataModelAnnotation> targetAnnotations)
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
