// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// FieldMetadata class providing the correlation between the column ordinals and MemberMetadata.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct FieldMetadata
    {
        private readonly EdmMember _fieldType;
        private readonly int _ordinal;

        /// <summary>
        /// Initializes a new <see cref="T:System.Data.Entity.Core.Common.FieldMetadata" /> object with the specified ordinal value and field type.
        /// </summary>
        /// <param name="ordinal">An integer specified the location of the metadata.</param>
        /// <param name="fieldType">The field type.</param>
        public FieldMetadata(int ordinal, EdmMember fieldType)
        {
            if (ordinal < 0)
            {
                throw new ArgumentOutOfRangeException("ordinal");
            }
            Check.NotNull(fieldType, "fieldType");

            _fieldType = fieldType;
            _ordinal = ordinal;
        }

        /// <summary>
        /// Gets the type of field for this <see cref="T:System.Data.Entity.Core.Common.FieldMetadata" /> object.
        /// </summary>
        /// <returns>
        /// The type of field for this <see cref="T:System.Data.Entity.Core.Common.FieldMetadata" /> object.
        /// </returns>
        public EdmMember FieldType
        {
            get { return _fieldType; }
        }

        /// <summary>
        /// Gets the ordinal for this <see cref="T:System.Data.Entity.Core.Common.FieldMetadata" /> object.
        /// </summary>
        /// <returns>An integer representing the ordinal value.</returns>
        public int Ordinal
        {
            get { return _ordinal; }
        }
    }
}
