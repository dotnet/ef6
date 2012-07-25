// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Encapsulates metadata information relevant to update for records extracted from
    /// the entity state manager, such as concurrency flags and key information.
    /// </summary>
    internal class ExtractorMetadata
    {
        internal ExtractorMetadata(EntitySetBase entitySetBase, StructuralType type, UpdateTranslator translator)
        {
            Contract.Requires(entitySetBase != null);
            Contract.Requires(type != null);
            Contract.Requires(translator != null);

            m_type = type;
            m_translator = translator;

            EntityType entityType = null;
            Set<EdmMember> keyMembers;
            Set<EdmMember> foreignKeyMembers;

            switch (type.BuiltInTypeKind)
            {
                case BuiltInTypeKind.RowType:
                    // for row types (which are actually association end key records in disguise), all members
                    // are keys
                    keyMembers = new Set<EdmMember>(((RowType)type).Properties).MakeReadOnly();
                    foreignKeyMembers = Set<EdmMember>.Empty;
                    break;
                case BuiltInTypeKind.EntityType:
                    entityType = (EntityType)type;
                    keyMembers = new Set<EdmMember>(entityType.KeyMembers).MakeReadOnly();
                    foreignKeyMembers = new Set<EdmMember>(
                        ((EntitySet)entitySetBase).ForeignKeyDependents
                            .SelectMany(fk => fk.Item2.ToProperties)).MakeReadOnly();
                    break;
                default:
                    keyMembers = Set<EdmMember>.Empty;
                    foreignKeyMembers = Set<EdmMember>.Empty;
                    break;
            }

            var members = TypeHelpers.GetAllStructuralMembers(type);
            m_memberMap = new MemberInformation[members.Count];
            // for each member, cache expensive to compute metadata information
            for (var ordinal = 0; ordinal < members.Count; ordinal++)
            {
                var member = members[ordinal];
                // figure out flags for this member
                var flags = PropagatorFlags.NoFlags;
                var entityKeyOrdinal = default(int?);

                if (keyMembers.Contains(member))
                {
                    flags |= PropagatorFlags.Key;
                    if (null != entityType)
                    {
                        entityKeyOrdinal = entityType.KeyMembers.IndexOf(member);
                    }
                }
                if (foreignKeyMembers.Contains(member))
                {
                    flags |= PropagatorFlags.ForeignKey;
                }

                if (MetadataHelper.GetConcurrencyMode(member)
                    == ConcurrencyMode.Fixed)
                {
                    flags |= PropagatorFlags.ConcurrencyValue;
                }

                // figure out whether this member is mapped to any server generated
                // columns in the store
                var isServerGenerated = m_translator.ViewLoader.IsServerGen(entitySetBase, m_translator.MetadataWorkspace, member);

                // figure out whether member nullability is used as a condition in mapping
                var isNullConditionMember = m_translator.ViewLoader.IsNullConditionMember(
                    entitySetBase, m_translator.MetadataWorkspace, member);

                // add information about this member
                m_memberMap[ordinal] = new MemberInformation(
                    ordinal, entityKeyOrdinal, flags, member, isServerGenerated, isNullConditionMember);
            }
        }

        private readonly MemberInformation[] m_memberMap;
        private readonly StructuralType m_type;
        private readonly UpdateTranslator m_translator;

        /// <summary>
        /// Requires: record must have correct type for this metadata instance.
        /// Populates a new <see cref="PropagatorResult"/> object representing a member of a record matching the 
        /// type of this extractor. Given a record and a member, this method wraps the value of the member
        /// in a PropagatorResult. This operation can be performed efficiently by this class, which knows
        /// important stuff about the type being extracted.
        /// </summary>
        /// <param name="stateEntry">state manager entry containing value (used for error reporting)</param>
        /// <param name="record">Record containing value (used to find the actual value)</param>
        /// <param name="currentValues">Indicates whether we are reading current or original values.</param>
        /// <param name="key">Entity key for the state entry. Must be set for entity records.</param>
        /// <param name="ordinal">Ordinal of Member for which to retrieve a value.</param>
        /// modified (must be ordinally aligned with the type). Null indicates all members are modified.</param>
        /// <param name="modifiedPropertiesBehavior">Indicates how to determine whether a property is modified.</param>
        /// <returns>Propagator result describing this member value.</returns>
        internal PropagatorResult RetrieveMember(
            IEntityStateEntry stateEntry, IExtendedDataRecord record, bool useCurrentValues,
            EntityKey key, int ordinal, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
        {
            var memberInformation = m_memberMap[ordinal];

            // get identifier value
            int identifier;
            if (memberInformation.IsKeyMember)
            {
                // retrieve identifier for this key member
                Debug.Assert(
                    null != (object)key, "entities must have keys, and only entity members are marked IsKeyMember by " +
                                         "the metadata wrapper");
                var keyOrdinal = memberInformation.EntityKeyOrdinal.Value;
                identifier = m_translator.KeyManager.GetKeyIdentifierForMemberOffset(key, keyOrdinal, ((EntityType)m_type).KeyMembers.Count);
            }
            else if (memberInformation.IsForeignKeyMember)
            {
                identifier = m_translator.KeyManager.GetKeyIdentifierForMember(key, record.GetName(ordinal), useCurrentValues);
            }
            else
            {
                identifier = PropagatorResult.NullIdentifier;
            }

            // determine if the member is modified
            var isModified = modifiedPropertiesBehavior == ModifiedPropertiesBehavior.AllModified ||
                             (modifiedPropertiesBehavior == ModifiedPropertiesBehavior.SomeModified &&
                              stateEntry.ModifiedProperties != null &&
                              stateEntry.ModifiedProperties[memberInformation.Ordinal]);

            // determine member value
            Debug.Assert(record.GetName(ordinal) == memberInformation.Member.Name, "expect record to present properties in metadata order");
            if (memberInformation.CheckIsNotNull
                && record.IsDBNull(ordinal))
            {
                throw EntityUtil.Update(Strings.Update_NullValue(record.GetName(ordinal)), null, stateEntry);
            }
            var value = record.GetValue(ordinal);

            // determine what kind of member this is

            // entityKey (association end)
            var entityKey = value as EntityKey;
            if (null != (object)entityKey)
            {
                return CreateEntityKeyResult(stateEntry, entityKey);
            }

            // record (nested complex type)
            var nestedRecord = value as IExtendedDataRecord;
            if (null != nestedRecord)
            {
                // for structural types, we track whether the entire complex type value is modified or not
                var nestedModifiedPropertiesBehavior = isModified
                                                           ? ModifiedPropertiesBehavior.AllModified
                                                           : ModifiedPropertiesBehavior.NoneModified;
                var translator = m_translator;

                return ExtractResultFromRecord(
                    stateEntry, isModified, nestedRecord, useCurrentValues, translator, nestedModifiedPropertiesBehavior);
            }

            // simple value (column/property value)
            return CreateSimpleResult(stateEntry, record, memberInformation, identifier, isModified, ordinal, value);
        }

        // Note that this is called only for association ends. Entities have key values inline.
        private PropagatorResult CreateEntityKeyResult(IEntityStateEntry stateEntry, EntityKey entityKey)
        {
            // get metadata for key
            var entityType = entityKey.GetEntitySet(m_translator.MetadataWorkspace).ElementType;
            var keyRowType = entityType.GetKeyRowType();

            var keyMetadata = m_translator.GetExtractorMetadata(stateEntry.EntitySet, keyRowType);
            var keyMemberCount = keyRowType.Properties.Count;
            var keyValues = new PropagatorResult[keyMemberCount];

            for (var ordinal = 0; ordinal < keyRowType.Properties.Count; ordinal++)
            {
                EdmMember keyMember = keyRowType.Properties[ordinal];
                // retrieve information about this key value
                var keyMemberInformation = keyMetadata.m_memberMap[ordinal];

                var keyIdentifier = m_translator.KeyManager.GetKeyIdentifierForMemberOffset(entityKey, ordinal, keyRowType.Properties.Count);

                object keyValue = null;
                if (entityKey.IsTemporary)
                {
                    // If the EntityKey is temporary, we need to retrieve the appropriate
                    // key value from the entity itself (or in this case, the IEntityStateEntry).
                    var entityEntry = stateEntry.StateManager.GetEntityStateEntry(entityKey);
                    Debug.Assert(
                        entityEntry.State == EntityState.Added,
                        "The corresponding entry for a temp EntityKey should be in the Added State.");
                    keyValue = entityEntry.CurrentValues[keyMember.Name];
                }
                else
                {
                    // Otherwise, we extract the value from within the EntityKey.
                    keyValue = entityKey.FindValueByName(keyMember.Name);
                }
                Debug.Assert(keyValue != null, "keyValue should've been retrieved.");

                // construct propagator result
                keyValues[ordinal] = PropagatorResult.CreateKeyValue(
                    keyMemberInformation.Flags,
                    keyValue,
                    stateEntry,
                    keyIdentifier);

                // see UpdateTranslator.Identifiers for information on key identifiers and ordinals
            }

            return PropagatorResult.CreateStructuralValue(keyValues, keyMetadata.m_type, false);
        }

        private PropagatorResult CreateSimpleResult(
            IEntityStateEntry stateEntry, IExtendedDataRecord record, MemberInformation memberInformation,
            int identifier, bool isModified, int recordOrdinal, object value)
        {
            var updatableRecord = record as CurrentValueRecord;

            // construct flags for the value, which is needed for complex type and simple members
            var flags = memberInformation.Flags;
            if (!isModified)
            {
                flags |= PropagatorFlags.Preserve;
            }
            if (PropagatorResult.NullIdentifier != identifier)
            {
                // construct a key member
                PropagatorResult result;
                if ((memberInformation.IsServerGenerated || memberInformation.IsForeignKeyMember)
                    && null != updatableRecord)
                {
                    result = PropagatorResult.CreateServerGenKeyValue(flags, value, stateEntry, identifier, recordOrdinal);
                }
                else
                {
                    result = PropagatorResult.CreateKeyValue(flags, value, stateEntry, identifier);
                }

                // we register the entity as the "owner" of an identity so that back-propagation can succeed
                // (keys can only be back-propagated to entities, not association ends). It also allows us
                // to walk to the entity state entry in case of exceptions, since the state entry propagated
                // through the stack may be eliminated in a project above a join.
                m_translator.KeyManager.RegisterIdentifierOwner(result);

                return result;
            }
            else
            {
                if ((memberInformation.IsServerGenerated || memberInformation.IsForeignKeyMember)
                    && null != updatableRecord)
                {
                    // note: we only produce a server gen result when 
                    return PropagatorResult.CreateServerGenSimpleValue(flags, value, updatableRecord, recordOrdinal);
                }
                else
                {
                    return PropagatorResult.CreateSimpleValue(flags, value);
                }
            }
        }

        /// <summary>
        /// Converts a record to a propagator result
        /// </summary>
        /// <param name="stateEntry">state manager entry containing the record</param>
        /// <param name="isModified">Indicates whether the root element is modified (i.e., whether the type has changed)</param>
        /// <param name="record">Record to convert</param>
        /// <param name="useCurrentValues">Indicates whether we are retrieving current or original values.</param>
        /// <param name="translator">Translator for session context; registers new metadata for the record type if none
        /// exists</param>
        /// <param name="modifiedPropertiesBehavior">Indicates how to determine whether a property is modified.</param>
        /// <returns>Result corresponding to the given record</returns>
        internal static PropagatorResult ExtractResultFromRecord(
            IEntityStateEntry stateEntry, bool isModified, IExtendedDataRecord record,
            bool useCurrentValues, UpdateTranslator translator, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
        {
            var structuralType = (StructuralType)record.DataRecordInfo.RecordType.EdmType;
            var metadata = translator.GetExtractorMetadata(stateEntry.EntitySet, structuralType);
            var key = stateEntry.EntityKey;

            var nestedValues = new PropagatorResult[record.FieldCount];
            for (var ordinal = 0; ordinal < nestedValues.Length; ordinal++)
            {
                nestedValues[ordinal] = metadata.RetrieveMember(
                    stateEntry, record, useCurrentValues, key,
                    ordinal, modifiedPropertiesBehavior);
            }

            return PropagatorResult.CreateStructuralValue(nestedValues, structuralType, isModified);
        }

        private class MemberInformation
        {
            /// <summary>
            /// Gets ordinal of the member.
            /// </summary>
            internal readonly int Ordinal;

            /// <summary>
            /// Gets key ordinal for primary key member (null if not a primary key).
            /// </summary>
            internal readonly int? EntityKeyOrdinal;

            /// <summary>
            /// Gets propagator flags for the member, excluding the 'Preserve' flag
            /// which can only be set in context.
            /// </summary>
            internal readonly PropagatorFlags Flags;

            /// <summary>
            /// Indicates whether this is a key member.
            /// </summary>
            internal bool IsKeyMember
            {
                get { return PropagatorFlags.Key == (Flags & PropagatorFlags.Key); }
            }

            /// <summary>
            /// Indicates whether this is a foreign key member.
            /// </summary>
            internal bool IsForeignKeyMember
            {
                get { return PropagatorFlags.ForeignKey == (Flags & PropagatorFlags.ForeignKey); }
            }

            /// <summary>
            /// Indicates whether this value is server generated.
            /// </summary>
            internal readonly bool IsServerGenerated;

            /// <summary>
            /// Indicates whether non-null values are supported for this member.
            /// </summary>
            internal readonly bool CheckIsNotNull;

            /// <summary>
            /// Gets the member described by this wrapper.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            internal readonly EdmMember Member;

            internal MemberInformation(
                int ordinal, int? entityKeyOrdinal, PropagatorFlags flags, EdmMember member, bool isServerGenerated,
                bool isNullConditionMember)
            {
                Debug.Assert(
                    entityKeyOrdinal.HasValue ==
                    (member.DeclaringType.BuiltInTypeKind == BuiltInTypeKind.EntityType
                     && (flags & PropagatorFlags.Key) == PropagatorFlags.Key),
                    "key ordinal should only be provided if this is an entity key property");

                Ordinal = ordinal;
                EntityKeyOrdinal = entityKeyOrdinal;
                Flags = flags;
                Member = member;
                IsServerGenerated = isServerGenerated;
                // in two cases, we must check that a member value is not null:
                // - where the type participates in an isnull condition, nullability constraints must be honored
                // - for complex types, mapping relies on nullability constraint
                // - in other cases, nullability does not impact round trippability so we don't check
                CheckIsNotNull = !TypeSemantics.IsNullable(member) &&
                                 (isNullConditionMember || member.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType);
            }
        }
    }
}
