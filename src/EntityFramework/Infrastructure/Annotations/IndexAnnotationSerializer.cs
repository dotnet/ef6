// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Annotations
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This class is used to serialize and deserialize <see cref="IndexAnnotation"/> objects so that they
    /// can be stored in the EDMX form of the Entity Framework model.
    /// </summary>
    /// <remarks>
    /// An example of the serialized format is:
    /// { Name: 'MyIndex', Order: 7, IsClustered: True, IsUnique: False } { } { Name: 'MyOtherIndex' }.
    /// Note that properties that have not been explicitly set in an index attribute will be excluded from
    /// the serialized output. So, in the example above, the first index has all properties specified,
    /// the second has none, and the third has just the name set.
    /// </remarks>
    public class IndexAnnotationSerializer : IMetadataAnnotationSerializer
    {
        internal const string FormatExample
            = "{ Name: 'MyIndex', Order: 7, IsClustered: True, IsUnique: False } { } { Name: 'MyOtherIndex' }";

        private readonly Regex _serializedFormat = new Regex(
            @"\s*\{\s*(Name:\s*'(?<name>[^']+)')?"
            + @"[,\s]*(Order:\s*(?<order>[\d]+))?"
            + @"[,\s]*(IsClustered:\s*(?<clustered>[\w]+))?"
            + @"[,\s]*(IsUnique:\s*(?<unique>[\w]+))?"
            + @"\s*\z", RegexOptions.IgnoreCase);

        /// <summary>
        /// Serializes the given <see cref="IndexAnnotation"/> into a string for storage in the EDMX XML.
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

            var builder = new StringBuilder();
            foreach (var index in annotation.Indexes)
            {
                builder.Append(index.DetailsToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Deserializes the given string back into an <see cref="IndexAnnotation"/> object.
        /// </summary>
        /// <param name="name">The name of the annotation that is being deserialized.</param>
        /// <param name="value">The string to deserialize.</param>
        /// <returns>The deserialized annotation value.</returns>
        /// <exception cref="FormatException">If there is an error reading the serialized value.</exception>
        public virtual object Deserialize(string name, string value)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(value, "value");

            var indexes = new List<IndexAttribute>();
            foreach (var indexString in value.Split('}').Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var match = _serializedFormat.Match(indexString);
                if (!match.Success)
                {
                    throw BuildFormatException(value);
                }

                try
                {
                    var nameGroup = match.Groups["name"];
                    var index = string.IsNullOrEmpty(nameGroup.Value) ? new IndexAttribute() : new IndexAttribute(nameGroup.Value);

                    var orderGroup = match.Groups["order"];
                    if (!string.IsNullOrEmpty(orderGroup.Value))
                    {
                        index.Order = int.Parse(orderGroup.Value, CultureInfo.InvariantCulture);
                    }

                    var clusteredGroup = match.Groups["clustered"];
                    if (!string.IsNullOrEmpty(clusteredGroup.Value))
                    {
                        index.IsClustered = clusteredGroup.Value.StartsWith("T", StringComparison.OrdinalIgnoreCase);
                    }

                    var uniqueGroup = match.Groups["unique"];
                    if (!string.IsNullOrEmpty(uniqueGroup.Value))
                    {
                        index.IsUnique = uniqueGroup.Value.StartsWith("T", StringComparison.OrdinalIgnoreCase);
                    }

                    indexes.Add(index);
                }
                catch (Exception ex)
                {
                    throw BuildFormatException(value, ex);
                }
            }

            return new IndexAnnotation(indexes);
        }

        private static FormatException BuildFormatException(string value, Exception innerException = null)
        {
            return new FormatException(
                Strings.AnnotationSerializeBadFormat(value, typeof(IndexAnnotationSerializer).Name, FormatExample), innerException);
        }
    }
}
