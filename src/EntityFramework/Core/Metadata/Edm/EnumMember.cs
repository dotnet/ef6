// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents an enumeration member.
    /// </summary>
    public sealed class EnumMember : MetadataItem
    {
        // <summary>
        // The name of this enumeration member.
        // </summary>
        private readonly string _name;

        // <summary>
        // The value of this enumeration member.
        // </summary>
        private readonly object _value;

        // <summary>
        // Initializes a new instance of the <see cref="EnumMember" /> type by using the specified name and value.
        // </summary>
        // <param name="name"> The name of this enumeration member. Must not be null or the empty string. </param>
        // <param name="value"> The value of this enumeration member. </param>
        // <exception cref="System.ArgumentNullException">Thrown if name argument is null</exception>
        // <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal EnumMember(string name, object value)
            : base(MetadataFlags.Readonly)
        {
            Check.NotEmpty(name, "name");
            DebugCheck.NotNull(value);
            Debug.Assert(
                value is SByte || value is Byte || value is Int16 || value is Int32 || value is Int64,
                "Unsupported type of enum member value.");

            _name = name;
            _value = value;
        }

        /// <summary> Gets the kind of this type. </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.EnumMember; }
        }

        /// <summary> Gets the name of this enumeration member. </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public string Name
        {
            get { return _name; }
        }

        /// <summary> Gets the value of this enumeration member. </summary>
        [MetadataProperty(BuiltInTypeKind.PrimitiveType, false)]
        public object Value
        {
            get { return _value; }
        }

        // <summary>
        // Gets the identity for this item as a string
        // </summary>
        internal override string Identity
        {
            get { return Name; }
        }

        /// <summary> Overriding System.Object.ToString to provide better String representation for this type. </summary>
        /// <returns>The name of this enumeration member.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Creates a read-only EnumMember instance.
        /// </summary>
        /// <param name="name">The name of the enumeration member.</param>
        /// <param name="value">The value of the enumeration member.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the enumeration member.</param>
        /// <returns>The newly created EnumMember instance.</returns>
        /// <exception cref="System.ArgumentException">name is null or empty.</exception>
        [CLSCompliant(false)]
        public static EnumMember Create(string name, sbyte value,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");

            return CreateInternal(name, value, metadataProperties);
        }

        /// <summary>
        /// Creates a read-only EnumMember instance.
        /// </summary>
        /// <param name="name">The name of the enumeration member.</param>
        /// <param name="value">The value of the enumeration member.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the enumeration member.</param>
        /// <returns>The newly created EnumMember instance.</returns>
        /// <exception cref="System.ArgumentException">name is null or empty.</exception>
        public static EnumMember Create(string name, byte value,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");

            return CreateInternal(name, value, metadataProperties);
        }

        /// <summary>
        /// Creates a read-only EnumMember instance.
        /// </summary>
        /// <param name="name">The name of the enumeration member.</param>
        /// <param name="value">The value of the enumeration member.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the enumeration member.</param>
        /// <returns>The newly created EnumMember instance.</returns>
        /// <exception cref="System.ArgumentException">name is null or empty.</exception>
        public static EnumMember Create(string name, short value,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");

            return CreateInternal(name, value, metadataProperties);
        }

        /// <summary>
        /// Creates a read-only EnumMember instance.
        /// </summary>
        /// <param name="name">The name of the enumeration member.</param>
        /// <param name="value">The value of the enumeration member.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the enumeration member.</param>
        /// <returns>The newly created EnumMember instance.</returns>
        /// <exception cref="System.ArgumentException">name is null or empty.</exception>
        public static EnumMember Create(string name, int value,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");

            return CreateInternal(name, value, metadataProperties);
        }

        /// <summary>
        /// Creates a read-only EnumMember instance.
        /// </summary>
        /// <param name="name">The name of the enumeration member.</param>
        /// <param name="value">The value of the enumeration member.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the enumeration member.</param>
        /// <returns>The newly created EnumMember instance.</returns>
        /// <exception cref="System.ArgumentException">name is null or empty.</exception>
        public static EnumMember Create(string name, long value,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");

            return CreateInternal(name, value, metadataProperties);
        }

        private static EnumMember CreateInternal(
            string name,
            object value,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            var instance = new EnumMember(name, value);

            if (metadataProperties != null)
            {
                instance.AddMetadataProperties(metadataProperties);
            }

            instance.SetReadOnly();

            return instance;
        }
    }
}
