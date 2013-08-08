// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// DataRecordInfo class providing a simple way to access both the type information and the column information.
    /// </summary>
    public class DataRecordInfo
    {
        private readonly ReadOnlyCollection<FieldMetadata> _fieldMetadata;
        private readonly TypeUsage _metadata;

        internal DataRecordInfo()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="T:System.Data.Common.DbDataRecord" /> object for a specific type with an enumerable collection of data fields.
        /// </summary>
        /// <param name="metadata">
        /// The metadata for the type represented by this object, supplied by
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" />
        /// .
        /// </param>
        /// <param name="memberInfo">
        /// An enumerable collection of <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmMember" /> objects that represent column information.
        /// </param>
        public DataRecordInfo(TypeUsage metadata, IEnumerable<EdmMember> memberInfo)
        {
            Check.NotNull(metadata, "metadata");
            var members = TypeHelpers.GetAllStructuralMembers(metadata.EdmType);

            var fieldList = new List<FieldMetadata>(members.Count);

            if (null != memberInfo)
            {
                foreach (var member in memberInfo)
                {
                    if ((null != member)
                        && (0 <= members.IndexOf(member))
                        && ((BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind)
                            || // for ComplexType, EntityType; BuiltTypeKind.NaviationProperty not allowed
                            (BuiltInTypeKind.AssociationEndMember == member.BuiltInTypeKind))) // for AssociationType
                    {
                        // each memberInfo must be non-null and be part of Properties or AssociationEndMembers
                        //validate that EdmMembers are from the same type or base type of the passed in metadata.
                        if ((member.DeclaringType != metadata.EdmType)
                            && !member.DeclaringType.IsBaseTypeOf(metadata.EdmType))
                        {
                            throw new ArgumentException(Strings.EdmMembersDefiningTypeDoNotAgreeWithMetadataType);
                        }
                        fieldList.Add(new FieldMetadata(fieldList.Count, member));
                    }
                    else
                    {
                        // expecting empty memberInfo for non-structural && non-null member part of members if structural
                        throw Error.InvalidEdmMemberInstance();
                    }
                }
            }

            // expecting structural types to have something at least 1 property
            // (((null == structural) && (0 == fieldList.Count)) || ((null != structural) && (0 < fieldList.Count)))
            if (Helper.IsStructuralType(metadata.EdmType) == (0 < fieldList.Count))
            {
                _fieldMetadata = new ReadOnlyCollection<FieldMetadata>(fieldList);
                _metadata = metadata;
            }
            else
            {
                throw Error.InvalidEdmMemberInstance();
            }
        }

        /// <summary>
        /// Construct FieldMetadata for structuralType.Members from TypeUsage
        /// </summary>
        internal DataRecordInfo(TypeUsage metadata)
        {
            DebugCheck.NotNull(metadata);

            var structuralMembers = TypeHelpers.GetAllStructuralMembers(metadata);
            var fieldList = new FieldMetadata[structuralMembers.Count];
            for (var i = 0; i < fieldList.Length; ++i)
            {
                var member = structuralMembers[i];
                Debug.Assert(
                    (BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind) ||
                    (BuiltInTypeKind.AssociationEndMember == member.BuiltInTypeKind),
                    "unexpected BuiltInTypeKind for member");
                fieldList[i] = new FieldMetadata(i, member);
            }
            _fieldMetadata = new ReadOnlyCollection<FieldMetadata>(fieldList);
            _metadata = metadata;
        }

        /// <summary>
        /// Reusing TypeUsage and FieldMetadata from another EntityRecordInfo which has all the same info
        /// but with a different EntityKey instance.
        /// </summary>
        internal DataRecordInfo(DataRecordInfo recordInfo)
        {
            _fieldMetadata = recordInfo._fieldMetadata;
            _metadata = recordInfo._metadata;
        }

        /// <summary>
        /// Gets <see cref="T:System.Data.Entity.Core.Common.FieldMetadata" /> for this
        /// <see
        ///     cref="P:System.Data.Entity.Core.IExtendedDataRecord.DataRecordInfo" />
        /// object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Common.FieldMetadata" /> object.
        /// </returns>
        public ReadOnlyCollection<FieldMetadata> FieldMetadata
        {
            get { return _fieldMetadata; }
        }

        /// <summary>
        /// Gets type info for this object as a <see cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" /> object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.TypeUsage" /> value.
        /// </returns>
        public virtual TypeUsage RecordType
        {
            get { return _metadata; }
        }
    }
}
