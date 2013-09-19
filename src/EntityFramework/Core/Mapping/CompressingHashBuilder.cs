// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography;

    /// <summary>
    /// This class keeps recomputing the hash and adding it to the front of the
    /// builder when the length of the string gets too long
    /// </summary>
    internal class CompressingHashBuilder : StringHashBuilder
    {
        // this max comes from the value that Md5Hasher uses for a buffer size when it is reading
        // from a stream
        private const int HashCharacterCompressionThreshold = 0x1000 / 2; // num bytes / 2 to convert to typical unicode char size
        private const int SpacesPerIndent = 4;

        private int _indent;

        private static readonly Dictionary<Type, string> _legacyTypeNames
            = InitializeLegacyTypeNames();

        // we are starting the buffer at 1.5 times the number of bytes
        // for the threshold
        internal CompressingHashBuilder(HashAlgorithm hashAlgorithm)
            : base(hashAlgorithm, (HashCharacterCompressionThreshold + (HashCharacterCompressionThreshold / 2)) * 2)
        {
        }

        internal override void Append(string content)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.Append(content);
            CompressHash();
        }

        internal override void AppendLine(string content)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));
            base.AppendLine(content);
            CompressHash();
        }

        /// <summary>
        /// Several classes were renamed while creating the public mapping API. The old names were used 
        /// in the process of computing a mapping hash value (see AppendObjectStartDump). To avoid hash 
        /// value changes that invalidate pre-generated views, this method builds a dictionary that maps 
        /// types to their original names, and it is used to lookup the old names when the hash value is
        /// computed.
        /// </summary>
        private static Dictionary<Type, string> InitializeLegacyTypeNames()
        {
            var typeNames = new Dictionary<Type, string>();

            typeNames.Add(typeof(AssociationSetMapping), "System.Data.Entity.Core.Mapping.StorageAssociationSetMapping");
            typeNames.Add(typeof(AssociationSetModificationFunctionMapping), "System.Data.Entity.Core.Mapping.StorageAssociationSetModificationFunctionMapping");
            typeNames.Add(typeof(AssociationTypeMapping), "System.Data.Entity.Core.Mapping.StorageAssociationTypeMapping");
            typeNames.Add(typeof(ComplexPropertyMapping), "System.Data.Entity.Core.Mapping.StorageComplexPropertyMapping");
            typeNames.Add(typeof(ComplexTypeMapping), "System.Data.Entity.Core.Mapping.StorageComplexTypeMapping");
            typeNames.Add(typeof(ConditionPropertyMapping), "System.Data.Entity.Core.Mapping.StorageConditionPropertyMapping");
            typeNames.Add(typeof(EndPropertyMapping), "System.Data.Entity.Core.Mapping.StorageEndPropertyMapping");
            typeNames.Add(typeof(EntityContainerMapping), "System.Data.Entity.Core.Mapping.StorageEntityContainerMapping");
            typeNames.Add(typeof(EntitySetMapping), "System.Data.Entity.Core.Mapping.StorageEntitySetMapping");
            typeNames.Add(typeof(EntityTypeMapping), "System.Data.Entity.Core.Mapping.StorageEntityTypeMapping");
            typeNames.Add(typeof(EntityTypeModificationFunctionMapping), "System.Data.Entity.Core.Mapping.StorageEntityTypeModificationFunctionMapping");
            typeNames.Add(typeof(MappingFragment), "System.Data.Entity.Core.Mapping.StorageMappingFragment");
            typeNames.Add(typeof(ModificationFunctionMapping), "System.Data.Entity.Core.Mapping.StorageModificationFunctionMapping");
            typeNames.Add(typeof(ModificationFunctionMemberPath), "System.Data.Entity.Core.Mapping.StorageModificationFunctionMemberPath");
            typeNames.Add(typeof(ModificationFunctionParameterBinding), "System.Data.Entity.Core.Mapping.StorageModificationFunctionParameterBinding");
            typeNames.Add(typeof(ModificationFunctionResultBinding), "System.Data.Entity.Core.Mapping.StorageModificationFunctionResultBinding");
            typeNames.Add(typeof(PropertyMapping), "System.Data.Entity.Core.Mapping.StoragePropertyMapping");
            typeNames.Add(typeof(ScalarPropertyMapping), "System.Data.Entity.Core.Mapping.StorageScalarPropertyMapping");
            typeNames.Add(typeof(EntitySetBaseMapping), "System.Data.Entity.Core.Mapping.StorageSetMapping");
            typeNames.Add(typeof(TypeMapping), "System.Data.Entity.Core.Mapping.StorageTypeMapping");

            return typeNames;
        }

        /// <summary>
        /// add string like "typename Instance#1"
        /// </summary>
        internal void AppendObjectStartDump(object o, int objectIndex)
        {
            base.Append(string.Empty.PadLeft(SpacesPerIndent * _indent, ' '));

            string typeName;
            if (!_legacyTypeNames.TryGetValue(o.GetType(), out typeName))
            {
                typeName = o.GetType().ToString();
            }

            base.Append(typeName);
            base.Append(" Instance#");
            base.AppendLine(objectIndex.ToString(CultureInfo.InvariantCulture));
            CompressHash();

            _indent++;
        }

        internal void AppendObjectEndDump()
        {
            Debug.Assert(_indent > 0, "Indent and unindent should be paired");
            _indent--;
        }

        private void CompressHash()
        {
            if (base.CharCount >= HashCharacterCompressionThreshold)
            {
                var hash = ComputeHash();
                Clear();
                base.Append(hash);
            }
        }
    }
}
