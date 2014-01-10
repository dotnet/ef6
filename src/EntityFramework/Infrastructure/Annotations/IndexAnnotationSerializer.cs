// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Annotations
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     This class is used to serialize and deserialize <see cref="IndexAnnotation" /> objects so that they
    ///     can be stored in the EDMX form of the Entity Framework model.
    /// </summary>
    /// <remarks>
    ///     An example of the serialized format is:
    ///     { Name: 'MyIndex', Order: 7, IsClustered: True, IsUnique: False } { } { Name: 'MyOtherIndex' }.
    ///     Note that properties that have not been explicitly set in an index attribute will be excluded from
    ///     the serialized output. So, in the example above, the first index has all properties specified,
    ///     the second has none, and the third has just the name set.
    /// </remarks>
    public class IndexAnnotationSerializer : IMetadataAnnotationSerializer
    {
        internal const string FormatExample
            = "{ Name: MyIndex, Order: 7, IsClustered: True, IsUnique: False } { } { Name: MyOtherIndex }";

        private static readonly Regex _indexesSplitter
            = new Regex(@"(?<!\\)}\s*{", RegexOptions.Compiled);

        private static readonly Regex _indexPartsSplitter
            = new Regex(@"(?<!\\),", RegexOptions.Compiled);

        /// <summary>
        ///     Serializes the given <see cref="IndexAnnotation" /> into a string for storage in the EDMX XML.
        /// </summary>
        /// <param name="name">The name of the annotation that is being serialized.</param>
        /// <param name="value">The value to serialize which must be an IndexAnnotation object.</param>
        /// <returns>The serialized value.</returns>
        public virtual string Serialize(string name, object value)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(value, "value");

            var annotation = value as IndexAnnotation;

            if (annotation == null)
            {
                throw new ArgumentException(
                    Strings.AnnotationSerializeWrongType(
                        value.GetType().Name, typeof(IndexAnnotationSerializer).Name, typeof(IndexAnnotation).Name));
            }

            var stringBuilder = new StringBuilder();

            foreach (var index in annotation.Indexes)
            {
                stringBuilder.Append(SerializeIndexAttribute(index));
            }

            return stringBuilder.ToString();
        }

        // For example: "{ Name: 'Foo', Order: 1, IsClustered: True, IsUnique: False }"
        internal static string SerializeIndexAttribute(IndexAttribute indexAttribute)
        {
            DebugCheck.NotNull(indexAttribute);

            var builder = new StringBuilder("{ ");

            if (!string.IsNullOrWhiteSpace(indexAttribute.Name))
            {
                builder
                    .Append("Name: ")
                    .Append(
                        indexAttribute.Name
                            .Replace(",", @"\,")
                            .Replace("{", @"\{"));
            }

            if (indexAttribute.Order != -1)
            {
                if (builder.Length > 2)
                {
                    builder.Append(", ");
                }

                builder.Append("Order: ").Append(indexAttribute.Order);
            }

            if (indexAttribute.IsClusteredConfigured)
            {
                if (builder.Length > 2)
                {
                    builder.Append(", ");
                }

                builder.Append("IsClustered: ").Append(indexAttribute.IsClustered);
            }

            if (indexAttribute.IsUniqueConfigured)
            {
                if (builder.Length > 2)
                {
                    builder.Append(", ");
                }

                builder.Append("IsUnique: ").Append(indexAttribute.IsUnique);
            }

            if (builder.Length > 2)
            {
                builder.Append(" ");
            }

            builder.Append("}");

            return builder.ToString();
        }

        /// <summary>
        ///     Deserializes the given string back into an <see cref="IndexAnnotation" /> object.
        /// </summary>
        /// <param name="name">The name of the annotation that is being deserialized.</param>
        /// <param name="value">The string to deserialize.</param>
        /// <returns>The deserialized annotation value.</returns>
        /// <exception cref="FormatException">If there is an error reading the serialized value.</exception>
        public virtual object Deserialize(string name, string value)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(value, "value");

            value = value.Trim();

            if (!value.StartsWith("{", StringComparison.Ordinal)
                || !value.EndsWith("}", StringComparison.Ordinal))
            {
                throw BuildFormatException(value);
            }

            var indexes = new List<IndexAttribute>();

            var indexStrings = _indexesSplitter.Split(value).Select(s => s.Trim()).ToList();

            indexStrings[0] = indexStrings[0].Substring(1);

            var lastIndex = indexStrings.Count - 1;

            indexStrings[lastIndex] = indexStrings[lastIndex].Substring(0, indexStrings[lastIndex].Length - 1);

            foreach (var indexString in indexStrings)
            {
                var indexAttribute = new IndexAttribute();

                if (!string.IsNullOrWhiteSpace(indexString))
                {
                    foreach (var indexPart in _indexPartsSplitter.Split(indexString).Select(s => s.Trim()))
                    {
                        if (indexPart.StartsWith("Name:", StringComparison.Ordinal))
                        {
                            var indexName = indexPart.Substring(5).Trim();

                            if (string.IsNullOrWhiteSpace(indexName)
                                || !string.IsNullOrWhiteSpace(indexAttribute.Name))
                            {
                                throw BuildFormatException(value);
                            }

                            indexAttribute.Name = indexName.Replace(@"\,", ",").Replace(@"\{", "{");
                        }
                        else if (indexPart.StartsWith("Order:", StringComparison.Ordinal))
                        {
                            int order;

                            if (!int.TryParse(indexPart.Substring(6).Trim(), out order)
                                || indexAttribute.Order != -1)
                            {
                                throw BuildFormatException(value);
                            }

                            indexAttribute.Order = order;
                        }
                        else if (indexPart.StartsWith("IsClustered:", StringComparison.Ordinal))
                        {
                            bool isClustered;

                            if (!bool.TryParse(indexPart.Substring(12).Trim(), out isClustered)
                                || indexAttribute.IsClusteredConfigured)
                            {
                                throw BuildFormatException(value);
                            }

                            indexAttribute.IsClustered = isClustered;
                        }
                        else if (indexPart.StartsWith("IsUnique:", StringComparison.Ordinal))
                        {
                            bool isUnique;

                            if (!bool.TryParse(indexPart.Substring(9).Trim(), out isUnique)
                                || indexAttribute.IsUniqueConfigured)
                            {
                                throw BuildFormatException(value);
                            }

                            indexAttribute.IsUnique = isUnique;
                        }
                        else
                        {
                            throw BuildFormatException(value);
                        }
                    }
                }

                indexes.Add(indexAttribute);
            }

            return new IndexAnnotation(indexes);
        }

        private static FormatException BuildFormatException(string value)
        {
            return new FormatException(
                Strings.AnnotationSerializeBadFormat(value, typeof(IndexAnnotationSerializer).Name, FormatExample));
        }
    }
}
