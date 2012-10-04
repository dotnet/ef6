// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Common
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal static class DataModelAnnotationExtensions
    {
        private const string ClrTypeAnnotation = "ClrType";
        private const string ClrPropertyInfoAnnotation = "ClrPropertyInfo";
        private const string ClrAttributesAnnotation = "ClrAttributes";
        private const string ConfiguationAnnotation = "Configuration";

        public static IList<Attribute> GetClrAttributes(this IEnumerable<DataModelAnnotation> dataModelAnnotations)
        {
            Contract.Requires(dataModelAnnotations != null);

            return (IList<Attribute>)dataModelAnnotations.GetAnnotation(ClrAttributesAnnotation);
        }

        public static void SetClrAttributes(
            this ICollection<DataModelAnnotation> dataModelAnnotations, IList<Attribute> attributes)
        {
            Contract.Requires(dataModelAnnotations != null);
            Contract.Requires(attributes != null);

            dataModelAnnotations.SetAnnotation(ClrAttributesAnnotation, attributes);
        }

        public static PropertyInfo GetClrPropertyInfo(this IEnumerable<DataModelAnnotation> dataModelAnnotations)
        {
            Contract.Requires(dataModelAnnotations != null);

            return (PropertyInfo)dataModelAnnotations.GetAnnotation(ClrPropertyInfoAnnotation);
        }

        public static void SetClrPropertyInfo(
            this ICollection<DataModelAnnotation> dataModelAnnotations, PropertyInfo propertyInfo)
        {
            Contract.Requires(dataModelAnnotations != null);
            Contract.Requires(propertyInfo != null);

            dataModelAnnotations.SetAnnotation(ClrPropertyInfoAnnotation, propertyInfo);
        }

        public static Type GetClrType(this IEnumerable<DataModelAnnotation> dataModelAnnotations)
        {
            Contract.Requires(dataModelAnnotations != null);

            return (Type)dataModelAnnotations.GetAnnotation(ClrTypeAnnotation);
        }

        public static void SetClrType(this ICollection<DataModelAnnotation> dataModelAnnotations, Type type)
        {
            Contract.Requires(dataModelAnnotations != null);
            Contract.Requires(type != null);

            dataModelAnnotations.SetAnnotation(ClrTypeAnnotation, type);
        }

        public static object GetConfiguration(this IEnumerable<DataModelAnnotation> dataModelAnnotations)
        {
            Contract.Requires(dataModelAnnotations != null);

            return dataModelAnnotations.GetAnnotation(ConfiguationAnnotation);
        }

        public static void SetConfiguration(
            this ICollection<DataModelAnnotation> dataModelAnnotations, object configuration)
        {
            Contract.Requires(dataModelAnnotations != null);

            dataModelAnnotations.SetAnnotation(ConfiguationAnnotation, configuration);
        }

        public static object GetAnnotation(this IEnumerable<DataModelAnnotation> dataModelAnnotations, string name)
        {
            Contract.Requires(dataModelAnnotations != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var annotation = dataModelAnnotations.SingleOrDefault(a => a.Name.Equals(name, StringComparison.Ordinal));

            return annotation != null ? annotation.Value : null;
        }

        public static void SetAnnotation(
            this ICollection<DataModelAnnotation> dataModelAnnotations, string name, object value)
        {
            Contract.Requires(dataModelAnnotations != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(value != null);

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
            Contract.Requires(dataModelAnnotations != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

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
            Contract.Requires(sourceAnnotations != null);
            Contract.Requires(targetAnnotations != null);

            foreach (var annotation in sourceAnnotations)
            {
                targetAnnotations.SetAnnotation(annotation.Name, annotation.Value);
            }
        }
    }
}
