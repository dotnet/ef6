// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Spatial
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    ///     A data contract serializable representation of a <see cref="DbGeography" /> value.
    /// </summary>
    [DataContract]
    public sealed class DbGeographyWellKnownValue
    {
        /// <summary>
        ///     Gets or sets the coordinate system identifier (SRID) of this value.
        /// </summary>
        [DataMember(Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public int CoordinateSystemId { get; set; }

        /// <summary>
        ///     Gets or sets the well known text representation of this value.
        /// </summary>
        [DataMember(Order = 2, IsRequired = false, EmitDefaultValue = false)]
        public string WellKnownText { get; set; }

        /// <summary>
        ///     Gets or sets the well known binary representation of this value.
        /// </summary>
        [DataMember(Order = 3, IsRequired = false, EmitDefaultValue = false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Required for this feature")]
        public byte[] WellKnownBinary { get; set; }
    }
}
