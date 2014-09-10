// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// EntityRecordInfo class providing a simple way to access both the type information and the column information.
    /// </summary>
    public class EntityRecordInfo : DataRecordInfo
    {
        private readonly EntityKey _entityKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Data.Entity.Core.Common.EntityRecordInfo" /> class of a specific entity type with an enumerable collection of data fields and with specific key and entity set information.
        /// </summary>
        /// <param name="metadata">
        /// The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityType" /> of the entity represented by the
        /// <see
        ///     cref="T:System.Data.Common.DbDataRecord" />
        /// described by this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.EntityRecordInfo" />
        /// object.
        /// </param>
        /// <param name="memberInfo">
        /// An enumerable collection of <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmMember" /> objects that represent column information.
        /// </param>
        /// <param name="entityKey">The key for the entity.</param>
        /// <param name="entitySet">The entity set to which the entity belongs.</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EntityRecordInfo(EntityType metadata, IEnumerable<EdmMember> memberInfo, EntityKey entityKey, EntitySet entitySet)
            : base(TypeUsage.Create(metadata), memberInfo)
        {
            Check.NotNull(entityKey, "entityKey");
            Check.NotNull(entitySet, "entitySet");

            _entityKey = entityKey;
            ValidateEntityType(entitySet);
        }

#if DEBUG
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
#endif
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "entitySet")]
        internal EntityRecordInfo(EntityType metadata, EntityKey entityKey, EntitySet entitySet)
            : base(TypeUsage.Create(metadata))
        {
            DebugCheck.NotNull(entityKey);

            _entityKey = entityKey;
#if DEBUG
            try
            {
                ValidateEntityType(entitySet);
            }
            catch
            {
                Debug.Assert(false, "should always be valid EntityType when internally constructed");
                throw;
            }
#endif
        }

        // <summary>
        // Reusing TypeUsage and FieldMetadata from another EntityRecordInfo which has all the same info
        // but with a different EntityKey instance.
        // </summary>
#if DEBUG
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
#else
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "entitySet")]
#endif
        internal EntityRecordInfo(DataRecordInfo info, EntityKey entityKey, EntitySet entitySet)
            : base(info)
        {
            _entityKey = entityKey;
#if DEBUG
            try
            {
                ValidateEntityType(entitySet);
            }
            catch
            {
                Debug.Assert(false, "should always be valid EntityType when internally constructed");
                throw;
            }
#endif
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Entity.Core.EntityKey" /> for the entity.
        /// </summary>
        /// <returns>The key for the entity.</returns>
        public EntityKey EntityKey
        {
            get { return _entityKey; }
        }

        // using EntitySetBase versus EntitySet prevents the unnecessary cast of ElementType to EntityType
        private void ValidateEntityType(EntitySetBase entitySet)
        {
            if (!ReferenceEquals(RecordType.EdmType, null)
                && !ReferenceEquals(_entityKey, EntityKey.EntityNotValidKey)
                && !ReferenceEquals(_entityKey, EntityKey.NoEntitySetKey)
                && !ReferenceEquals(RecordType.EdmType, entitySet.ElementType)
                && !entitySet.ElementType.IsBaseTypeOf(RecordType.EdmType))
            {
                throw new ArgumentException(Strings.EntityTypesDoNotAgree);
            }
        }
    }
}
