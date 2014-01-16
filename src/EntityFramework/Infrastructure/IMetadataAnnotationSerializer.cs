// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Implement this interface to allow custom annotations represented by <see cref="MetadataProperty" /> instances to be
    /// serialized to and from the EDMX XML. Usually a serializer instance is set using the
    /// <see cref="DbConfiguration.SetMetadataAnnotationSerializer" /> method.
    /// </summary>
    public interface IMetadataAnnotationSerializer
    {
        /// <summary>
        /// Serializes the given annotation value into a string for storage in the EDMX XML.
        /// </summary>
        /// <param name="name">The name of the annotation that is being serialized.</param>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized value.</returns>
        string Serialize(string name, object value);

        /// <summary>
        /// Deserializes the given string back into the expected annotation value.
        /// </summary>
        /// <param name="name">The name of the annotation that is being deserialized.</param>
        /// <param name="value">The string to deserialize.</param>
        /// <returns>The deserialized annotation value.</returns>
        object Deserialize(string name, string value);
    }
}
