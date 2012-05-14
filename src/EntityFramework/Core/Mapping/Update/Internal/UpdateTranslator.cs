namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// This class performs to following tasks to persist C-Space changes to the store:
    /// <list>
    /// <item>Extract changes from the entity state manager</item>
    /// <item>Group changes by C-Space extent</item>
    /// <item>For each affected S-Space table, perform propagation (get changes in S-Space terms)</item>
    /// <item>Merge S-Space inserts and deletes into updates where appropriate</item>
    /// <item>Produce S-Space commands implementating the modifications (insert, delete and update SQL statements)</item>
    /// </list>
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class UpdateTranslator
    {
        #region Constructors
        /// <summary>
        /// Constructs a new instance of <see cref="UpdateTranslator"/> based on the contents of the given entity state manager.
        /// </summary>
        /// <param name="stateManager">Entity state manager containing changes to be processed.</param>
        /// <param name="adapter">Map adapter requesting the changes.</param>
        internal UpdateTranslator(IEntityStateManager stateManager, EntityAdapter adapter)
        {
            Contract.Requires(stateManager != null);
            Contract.Requires(adapter != null);

            _stateManager = stateManager;
            _adapter = adapter;

            // propagation state
            _changes = new Dictionary<EntitySetBase, ChangeNode>();
            _functionChanges = new Dictionary<EntitySetBase, List<ExtractedStateEntry>>();
            _stateEntries = new List<IEntityStateEntry>();
            _knownEntityKeys = new Set<EntityKey>();
            _requiredEntities = new Dictionary<EntityKey, AssociationSet>();
            _optionalEntities = new Set<EntityKey>();
            _includedValueEntities = new Set<EntityKey>();

            // connection state
            _providerServices = DbProviderServices.GetProviderServices(adapter.Connection.StoreProviderFactory);

            // ancillary propagation services
            _recordConverter = new RecordConverter(this);
            _constraintValidator = new RelationshipConstraintValidator();

            // metadata cache
            _extractorMetadata = new Dictionary<Tuple<EntitySetBase, StructuralType>, ExtractorMetadata>();

            // key management
            KeyManager = new KeyManager();
            KeyComparer = CompositeKey.CreateComparer(KeyManager);
        }

        /// <summary>
        /// For testing purposes only
        /// </summary>
        protected UpdateTranslator()
        { }

        #endregion

        #region Fields

        private readonly EntityAdapter _adapter;

        // propagation state
        private readonly Dictionary<EntitySetBase, ChangeNode> _changes;
        private readonly Dictionary<EntitySetBase, List<ExtractedStateEntry>> _functionChanges;
        private readonly List<IEntityStateEntry> _stateEntries;
        private readonly Set<EntityKey> _knownEntityKeys;
        private readonly Dictionary<EntityKey, AssociationSet> _requiredEntities;
        private readonly Set<EntityKey> _optionalEntities;
        private readonly Set<EntityKey> _includedValueEntities;

        // workspace state
        private readonly IEntityStateManager _stateManager;

        // ancillary propagation services
        private readonly RecordConverter _recordConverter;
        private readonly RelationshipConstraintValidator _constraintValidator;

        // provider information
        private readonly DbProviderServices _providerServices;
        private Dictionary<StorageModificationFunctionMapping, DbCommandDefinition> _modificationFunctionCommandDefinitions;

        // metadata cache
        private readonly Dictionary<Tuple<EntitySetBase, StructuralType>, ExtractorMetadata> _extractorMetadata;

        #endregion

        #region Properties

        /// <summary>
        /// Gets workspace used in this session.
        /// </summary>
        internal MetadataWorkspace MetadataWorkspace
        {
            get { return Connection.GetMetadataWorkspace(); }
        }

        /// <summary>
        /// Gets key manager that handles interpretation of keys (including resolution of 
        /// referential-integrity/foreign key constraints)
        /// </summary>
        internal readonly KeyManager KeyManager;

        /// <summary>
        /// Gets the view loader metadata wrapper for the current workspace.
        /// </summary>
        internal ViewLoader ViewLoader
        {
            get { return MetadataWorkspace.GetUpdateViewLoader(); }
        }

        /// <summary>
        /// Gets record converter which translates state entry records into propagator results.
        /// </summary>
        internal RecordConverter RecordConverter
        {
            get { return _recordConverter; }
        }

        /// <summary>
        /// Get the connection used for update commands.
        /// </summary>
        internal virtual EntityConnection Connection
        {
            get { return _adapter.Connection; }
        }

        /// <summary>
        /// Gets command timeout for update commands. If null, use default.
        /// </summary>
        internal virtual int? CommandTimeout
        {
            get { return _adapter.CommandTimeout; }
        }

        internal readonly IEqualityComparer<CompositeKey> KeyComparer;

        #endregion

        #region Methods

        /// <summary>
        /// Registers any referential constraints contained in the state entry (so that
        /// constrained members have the same identifier values). Only processes relationships
        /// with referential constraints defined.
        /// </summary>
        /// <param name="stateEntry">State entry</param>
        internal void RegisterReferentialConstraints(IEntityStateEntry stateEntry)
        {
            if (stateEntry.IsRelationship)
            {
                var associationSet = (AssociationSet)stateEntry.EntitySet;
                if (0 < associationSet.ElementType.ReferentialConstraints.Count)
                {
                    var record = stateEntry.State == EntityState.Added
                                     ? stateEntry.CurrentValues
                                     : stateEntry.OriginalValues;
                    foreach (var constraint in associationSet.ElementType.ReferentialConstraints)
                    {
                        // retrieve keys at the ends
                        var principalKey = (EntityKey)record[constraint.FromRole.Name];
                        var dependentKey = (EntityKey)record[constraint.ToRole.Name];

                        // associate keys, where the from side 'owns' the to side
                        using (var principalPropertyEnum = constraint.FromProperties.GetEnumerator())
                        {
                            using (var dependentPropertyEnum = constraint.ToProperties.GetEnumerator())
                            {
                                while (principalPropertyEnum.MoveNext() && dependentPropertyEnum.MoveNext())
                                {
                                    int principalKeyMemberCount;
                                    int dependentKeyMemberCount;

                                    // get offsets for from and to key properties
                                    var principalOffset = GetKeyMemberOffset(
                                        constraint.FromRole, principalPropertyEnum.Current,
                                        out principalKeyMemberCount);
                                    var dependentOffset = GetKeyMemberOffset(
                                        constraint.ToRole, dependentPropertyEnum.Current,
                                        out dependentKeyMemberCount);

                                    var principalIdentifier = KeyManager.GetKeyIdentifierForMemberOffset(
                                        principalKey, principalOffset, principalKeyMemberCount);
                                    var dependentIdentifier = KeyManager.GetKeyIdentifierForMemberOffset(
                                        dependentKey, dependentOffset, dependentKeyMemberCount);

                                    // register equivalence of identifiers
                                    KeyManager.AddReferentialConstraint(stateEntry, dependentIdentifier, principalIdentifier);
                                }
                            }
                        }
                    }
                }
            }
            else if (!stateEntry.IsKeyEntry)
            {
                if (stateEntry.State == EntityState.Added || stateEntry.State == EntityState.Modified)
                {
                    RegisterEntityReferentialConstraints(stateEntry, true);
                }
                if (stateEntry.State == EntityState.Deleted
                    || stateEntry.State == EntityState.Modified)
                {
                    RegisterEntityReferentialConstraints(stateEntry, false);
                }
            }
        }

        private void RegisterEntityReferentialConstraints(IEntityStateEntry stateEntry, bool currentValues)
        {
            var record = currentValues
                             ? stateEntry.CurrentValues
                             : (IExtendedDataRecord)stateEntry.OriginalValues;
            var entitySet = (EntitySet)stateEntry.EntitySet;
            var dependentKey = stateEntry.EntityKey;

            foreach (var foreignKey in entitySet.ForeignKeyDependents)
            {
                var associationSet = foreignKey.Item1;
                var constraint = foreignKey.Item2;
                var dependentType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)constraint.ToRole);
                if (dependentType.IsAssignableFrom(record.DataRecordInfo.RecordType.EdmType))
                {
                    EntityKey principalKey = null;

                    // First, check for an explicit reference
                    if (!currentValues
                        || !_stateManager.TryGetReferenceKey(dependentKey, (AssociationEndMember)constraint.FromRole, out principalKey))
                    {
                        // build a key based on the foreign key values
                        var principalType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)constraint.FromRole);
                        var hasNullValue = false;
                        var keyValues = new object[principalType.KeyMembers.Count];
                        for (int i = 0, n = keyValues.Length; i < n; i++)
                        {
                            var keyMember = (EdmProperty)principalType.KeyMembers[i];

                            // Find corresponding foreign key value
                            var constraintOrdinal = constraint.FromProperties.IndexOf(keyMember);
                            var recordOrdinal = record.GetOrdinal(constraint.ToProperties[constraintOrdinal].Name);
                            if (record.IsDBNull(recordOrdinal))
                            {
                                hasNullValue = true;
                                break;
                            }
                            keyValues[i] = record.GetValue(recordOrdinal);
                        }

                        if (!hasNullValue)
                        {
                            var principalSet = associationSet.AssociationSetEnds[constraint.FromRole.Name].EntitySet;
                            if (1 == keyValues.Length)
                            {
                                principalKey = new EntityKey(principalSet, keyValues[0]);
                            }
                            else
                            {
                                principalKey = new EntityKey(principalSet, keyValues);
                            }
                        }
                    }

                    if (null != principalKey)
                    {
                        // find the right principal key... (first, existing entities; then, added entities; finally, just the key)
                        IEntityStateEntry existingPrincipal;
                        EntityKey tempKey;
                        if (_stateManager.TryGetEntityStateEntry(principalKey, out existingPrincipal))
                        {
                            // nothing to do. the principal key will resolve to the existing entity
                        }
                        else if (currentValues && KeyManager.TryGetTempKey(principalKey, out tempKey))
                        {
                            // if we aren't dealing with current values, we cannot resolve to a temp key (original values
                            // cannot indicate a relationship to an 'added' entity).
                            if (null == tempKey)
                            {
                                throw EntityUtil.Update(
                                    Strings.Update_AmbiguousForeignKey(constraint.ToRole.DeclaringType.FullName), null, stateEntry);
                            }
                            else
                            {
                                principalKey = tempKey;
                            }
                        }

                        // pull the principal end into the update pipeline (supports value propagation)
                        AddValidAncillaryKey(principalKey, _optionalEntities);

                        // associate keys, where the from side 'owns' the to side
                        for (int i = 0, n = constraint.FromProperties.Count; i < n; i++)
                        {
                            var principalProperty = constraint.FromProperties[i];
                            var dependentProperty = constraint.ToProperties[i];

                            int principalKeyMemberCount;

                            // get offsets for from and to key properties
                            var principalOffset = GetKeyMemberOffset(constraint.FromRole, principalProperty, out principalKeyMemberCount);
                            var principalIdentifier = KeyManager.GetKeyIdentifierForMemberOffset(
                                principalKey, principalOffset, principalKeyMemberCount);
                            int dependentIdentifier;

                            if (entitySet.ElementType.KeyMembers.Contains(dependentProperty))
                            {
                                int dependentKeyMemberCount;
                                var dependentOffset = GetKeyMemberOffset(
                                    constraint.ToRole, dependentProperty,
                                    out dependentKeyMemberCount);
                                dependentIdentifier = KeyManager.GetKeyIdentifierForMemberOffset(
                                    dependentKey, dependentOffset, dependentKeyMemberCount);
                            }
                            else
                            {
                                dependentIdentifier = KeyManager.GetKeyIdentifierForMember(
                                    dependentKey, dependentProperty.Name, currentValues);
                            }

                            // don't allow the user to insert or update an entity that refers to a deleted principal
                            if (currentValues && null != existingPrincipal &&
                                existingPrincipal.State == EntityState.Deleted &&
                                (stateEntry.State == EntityState.Added || stateEntry.State == EntityState.Modified))
                            {
                                throw EntityUtil.Update(
                                    Strings.Update_InsertingOrUpdatingReferenceToDeletedEntity(associationSet.ElementType.FullName),
                                    null,
                                    stateEntry,
                                    existingPrincipal);
                            }

                            // register equivalence of identifiers
                            KeyManager.AddReferentialConstraint(stateEntry, dependentIdentifier, principalIdentifier);
                        }
                    }
                }
            }
        }

        // requires: role must not be null and property must be a key member for the role end
        private static int GetKeyMemberOffset(RelationshipEndMember role, EdmProperty property, out int keyMemberCount)
        {
            Contract.Requires(null != role);
            Contract.Requires(null != property);

            Contract.Assert(BuiltInTypeKind.RefType == role.TypeUsage.EdmType.BuiltInTypeKind,
                "relationship ends must be of RefType");
            var endType = (RefType)role.TypeUsage.EdmType;
            Contract.Assert(BuiltInTypeKind.EntityType == endType.ElementType.BuiltInTypeKind,
                "relationship ends must reference EntityType");
            var entityType = (EntityType)endType.ElementType;
            keyMemberCount = entityType.KeyMembers.Count;
            return entityType.KeyMembers.IndexOf(property);
        }

        /// <summary>
        /// Yields all relationship state entries with the given key as an end.
        /// </summary>
        /// <param name="entityKey"></param>
        /// <returns></returns>
        internal IEnumerable<IEntityStateEntry> GetRelationships(EntityKey entityKey)
        {
            return _stateManager.FindRelationshipsByKey(entityKey);
        }

        /// <summary>
        /// Persists stateManager changes to the store.
        /// </summary>
        /// <param name="stateManager">StateManager containing changes to persist.</param>
        /// <param name="adapter">Map adapter requesting the changes.</param>
        /// <returns>Total number of state entries affected</returns>
        internal int Update()
        {
            // tracks values for identifiers in this session
            var identifierValues = new Dictionary<int, object>();

            // tracks values for generated values in this session
            var generatedValues = new List<KeyValuePair<PropagatorResult, object>>();

            var orderedCommands = ProduceCommands();

            // used to track the source of commands being processed in case an exception is thrown
            UpdateCommand source = null;
            try
            {
                foreach (var command in orderedCommands)
                {
                    // Remember the data sources so that we can throw meaningful exception
                    source = command;
                    var rowsAffected = command.Execute(identifierValues, generatedValues);
                    ValidateRowsAffected(rowsAffected, source);
                }
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (RequiresContext(e))
                {
                    throw new UpdateException(Strings.Update_GeneralExecutionException, e, DetermineStateEntriesFromSource(source).Cast<ObjectStateEntry>().Distinct());
                }
                throw;
            }

            BackPropagateServerGen(generatedValues);

            var totalStateEntries = AcceptChanges();

            return totalStateEntries;
        }

        private IEnumerable<UpdateCommand> ProduceCommands()
        {
            // load all modified state entries
            PullModifiedEntriesFromStateManager();
            PullUnchangedEntriesFromStateManager();

            // check constraints
            _constraintValidator.ValidateConstraints();
            KeyManager.ValidateReferentialIntegrityGraphAcyclic();

            // gather all commands (aggregate in a dependency orderer to determine operation order
            var dynamicCommands = ProduceDynamicCommands();
            var functionCommands = ProduceFunctionCommands();
            var orderer = new UpdateCommandOrderer(dynamicCommands.Concat(functionCommands), this);
            IEnumerable<UpdateCommand> orderedCommands;
            IEnumerable<UpdateCommand> remainder;
            if (!orderer.TryTopologicalSort(out orderedCommands, out remainder))
            {
                // throw an exception if it is not possible to perform dependency ordering
                throw DependencyOrderingError(remainder);
            }

            return orderedCommands;
        }

        // effects: given rows affected, throws if the count suggests a concurrency failure.
        // Throws a concurrency exception based on the current command sources (which allow
        // us to populated the EntityStateEntries on UpdateException)
        private void ValidateRowsAffected(long rowsAffected, UpdateCommand source)
        {
            // 0 rows affected indicates a concurrency failure; negative values suggest rowcount is off; 
            // positive values suggest at least one row was affected (we generally expect exactly one, 
            // but triggers/view logic/logging may change this value)
            if (0 == rowsAffected)
            {
                var stateEntries = DetermineStateEntriesFromSource(source);
                var message = Strings.Update_ConcurrencyError(rowsAffected);
                throw new OptimisticConcurrencyException(message, null, stateEntries.Cast<ObjectStateEntry>().Distinct());
            }
        }

        private IEnumerable<IEntityStateEntry> DetermineStateEntriesFromSource(UpdateCommand source)
        {
            if (null == source)
            {
                return Enumerable.Empty<IEntityStateEntry>();
            }
            return source.GetStateEntries(this);
        }

        // effects: Given a list of pairs describing the contexts for server generated values and their actual
        // values, backpropagates to the relevant state entries
        private void BackPropagateServerGen(List<KeyValuePair<PropagatorResult, object>> generatedValues)
        {
            foreach (var generatedValue in generatedValues)
            {
                PropagatorResult context;

                // check if a redirect to "owner" result is possible
                if (PropagatorResult.NullIdentifier == generatedValue.Key.Identifier ||
                    !KeyManager.TryGetIdentifierOwner(generatedValue.Key.Identifier, out context))
                {
                    // otherwise, just use the straightforward context
                    context = generatedValue.Key;
                }

                var value = generatedValue.Value;
                if (context.Identifier
                    == PropagatorResult.NullIdentifier)
                {
                    SetServerGenValue(context, value);
                }
                else
                {
                    // check if we need to back propagate this value to any other positions (e.g. for foreign keys)
                    foreach (var dependent in KeyManager.GetDependents(context.Identifier))
                    {
                        if (KeyManager.TryGetIdentifierOwner(dependent, out context))
                        {
                            SetServerGenValue(context, value);
                        }
                    }
                }
            }
        }

        private static void SetServerGenValue(PropagatorResult context, object value)
        {
            if (context.RecordOrdinal
                != PropagatorResult.NullOrdinal)
            {
                var targetRecord = context.Record;

                // determine if type compensation is required
                IExtendedDataRecord recordWithMetadata = targetRecord;
                var member = recordWithMetadata.DataRecordInfo.FieldMetadata[context.RecordOrdinal].FieldType;

                value = value ?? DBNull.Value; // records expect DBNull rather than null
                value = AlignReturnValue(value, member, context);
                targetRecord.SetValue(context.RecordOrdinal, value);
            }
        }

        /// <summary>
        /// Aligns a value returned from the store with the expected type for the member.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="member">Metadata for the member being set.</param>
        /// <param name="context">The context generating the return value.</param>
        /// <returns>Converted return value</returns>
        private static object AlignReturnValue(object value, EdmMember member, PropagatorResult context)
        {
            if (DBNull.Value.Equals(value))
            {
                // check if there is a nullability constraint on the value
                if (BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind
                    &&
                    !((EdmProperty)member).Nullable)
                {
                    throw EntityUtil.Update(
                        Strings.Update_NullReturnValueForNonNullableMember(
                            member.Name,
                            member.DeclaringType.FullName), null);
                }
            }
            else if (!Helper.IsSpatialType(member.TypeUsage))
            {
                Type clrType;
                Type clrEnumType = null;
                if (Helper.IsEnumType(member.TypeUsage.EdmType))
                {
                    var underlyingType = Helper.AsPrimitive(member.TypeUsage.EdmType);
                    clrEnumType = context.Record.GetFieldType(context.RecordOrdinal);
                    clrType = underlyingType.ClrEquivalentType;
                    Debug.Assert(clrEnumType.IsEnum);
                }
                else
                {
                    // convert the value to the appropriate CLR type
                    Debug.Assert(
                        BuiltInTypeKind.PrimitiveType == member.TypeUsage.EdmType.BuiltInTypeKind,
                        "we only allow return values that are instances of EDM primitive or enum types");
                    var primitiveType = (PrimitiveType)member.TypeUsage.EdmType;
                    clrType = primitiveType.ClrEquivalentType;
                }

                try
                {
                    value = Convert.ChangeType(value, clrType, CultureInfo.InvariantCulture);
                    if (clrEnumType != null)
                    {
                        value = Enum.ToObject(clrEnumType, value);
                    }
                }
                catch (Exception e)
                {
                    // we should not be wrapping all exceptions
                    if (RequiresContext(e))
                    {
                        var userClrType = clrEnumType ?? clrType;
                        throw EntityUtil.Update(
                            Strings.Update_ReturnValueHasUnexpectedType(
                                value.GetType().FullName,
                                userClrType.FullName,
                                member.Name,
                                member.DeclaringType.FullName), e);
                    }
                    throw;
                }
            }

            // return the adjusted value
            return value;
        }

        /// <summary>
        /// Accept changes to entities and relationships processed by this translator instance.
        /// </summary>
        /// <returns>Number of state entries affected.</returns>
        private int AcceptChanges()
        {
            var affectedCount = 0;
            foreach (var stateEntry in _stateEntries)
            {
                // only count and accept changes for state entries that are being explicitly modified
                if (EntityState.Unchanged
                    != stateEntry.State)
                {
                    if (_adapter.AcceptChangesDuringUpdate)
                    {
                        stateEntry.AcceptChanges();
                    }
                    affectedCount++;
                }
            }
            return affectedCount;
        }

        /// <summary>
        /// Gets extents for which this translator has identified changes to be handled
        /// by the standard update pipeline.
        /// </summary>
        /// <returns>Enumeration of modified C-Space extents.</returns>
        private IEnumerable<EntitySetBase> GetDynamicModifiedExtents()
        {
            return _changes.Keys;
        }

        /// <summary>
        /// Gets extents for which this translator has identified changes to be handled
        /// by function mappings.
        /// </summary>
        /// <returns>Enumreation of modified C-Space extents.</returns>
        private IEnumerable<EntitySetBase> GetFunctionModifiedExtents()
        {
            return _functionChanges.Keys;
        }

        /// <summary>
        /// Produce dynamic store commands for this translator's changes.
        /// </summary>
        /// <returns>Database commands in a safe order</returns>
        private IEnumerable<UpdateCommand> ProduceDynamicCommands()
        {
            // Initialize DBCommand update compiler
            var updateCompiler = new UpdateCompiler(this);

            // Determine affected
            var tables = new Set<EntitySet>();

            foreach (var extent in GetDynamicModifiedExtents())
            {
                var affectedTables = ViewLoader.GetAffectedTables(extent, MetadataWorkspace);
                //Since these extents don't have Functions defined for update operations,
                //the affected tables should be provided via MSL.
                //If we dont find any throw an exception
                if (affectedTables.Count == 0)
                {
                    throw EntityUtil.Update(Strings.Update_MappingNotFound(extent.Name), null /*stateEntries*/);
                }

                foreach (var table in affectedTables)
                {
                    tables.Add(table);
                }
            }

            // Determine changes to apply to each table
            foreach (var table in tables)
            {
                var umView = Connection.GetMetadataWorkspace().GetCqtView(table);

                // Propagate changes to root of tree (at which point they are S-Space changes)
                var changeNode = Propagator.Propagate(this, table, umView);

                // Process changes for the table
                var change = new TableChangeProcessor(table);
                foreach (var command in change.CompileCommands(changeNode, updateCompiler))
                {
                    yield return command;
                }
            }
        }

        // Generates and caches a command definition for the given function
        internal DbCommandDefinition GenerateCommandDefinition(StorageModificationFunctionMapping functionMapping)
        {
            if (null == _modificationFunctionCommandDefinitions)
            {
                _modificationFunctionCommandDefinitions = new Dictionary<StorageModificationFunctionMapping, DbCommandDefinition>();
            }
            DbCommandDefinition commandDefinition;
            if (!_modificationFunctionCommandDefinitions.TryGetValue(functionMapping, out commandDefinition))
            {
                // synthesize a RowType for this mapping
                TypeUsage resultType = null;
                if (null != functionMapping.ResultBindings
                    && 0 < functionMapping.ResultBindings.Count)
                {
                    var properties = new List<EdmProperty>(functionMapping.ResultBindings.Count);
                    foreach (var resultBinding in functionMapping.ResultBindings)
                    {
                        properties.Add(new EdmProperty(resultBinding.ColumnName, resultBinding.Property.TypeUsage));
                    }
                    var rowType = new RowType(properties);
                    var collectionType = new CollectionType(rowType);
                    resultType = TypeUsage.Create(collectionType);
                }

                // add function parameters
                var functionParams =
                    functionMapping.Function.Parameters.Select(
                        paramInfo => new KeyValuePair<string, TypeUsage>(paramInfo.Name, paramInfo.TypeUsage));

                // construct DbFunctionCommandTree including implict return type
                var tree = new DbFunctionCommandTree(
                    MetadataWorkspace, DataSpace.SSpace,
                    functionMapping.Function, resultType, functionParams);

                commandDefinition = _providerServices.CreateCommandDefinition(tree);
            }
            return commandDefinition;
        }

        // Produces all function commands in a safe order
        private IEnumerable<UpdateCommand> ProduceFunctionCommands()
        {
            foreach (var extent in GetFunctionModifiedExtents())
            {
                // Get a handle on the appropriate translator
                var translator = ViewLoader.GetFunctionMappingTranslator(extent, MetadataWorkspace);

                if (null != translator)
                {
                    // Compile commands
                    foreach (var stateEntry in GetExtentFunctionModifications(extent))
                    {
                        var command = translator.Translate(this, stateEntry);
                        if (null != command)
                        {
                            yield return command;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a metadata wrapper for the given type. The wrapper makes
        /// certain tasks in the update pipeline more efficient.
        /// </summary>
        /// <param name="type">Structural type</param>
        /// <returns>Metadata wrapper</returns>
        internal ExtractorMetadata GetExtractorMetadata(EntitySetBase entitySetBase, StructuralType type)
        {
            ExtractorMetadata metadata;
            var key = Tuple.Create(entitySetBase, type);
            if (!_extractorMetadata.TryGetValue(key, out metadata))
            {
                metadata = new ExtractorMetadata(entitySetBase, type, this);
                _extractorMetadata.Add(key, metadata);
            }
            return metadata;
        }

        /// <summary>
        /// Returns error when it is not possible to order update commands. Argument is the 'remainder', or commands
        /// that could not be ordered due to a cycle.
        /// </summary>
        private UpdateException DependencyOrderingError(IEnumerable<UpdateCommand> remainder)
        {
            Debug.Assert(null != remainder && remainder.Count() > 0, "must provide non-empty remainder");

            var stateEntries = new HashSet<IEntityStateEntry>();

            foreach (var command in remainder)
            {
                stateEntries.UnionWith(command.GetStateEntries(this));
            }

            // throw exception containing all related state entries
            throw new UpdateException(Strings.Update_ConstraintCycle, null, stateEntries.Cast<ObjectStateEntry>().Distinct());
        }

        /// <summary>
        /// Creates a command in the current context.
        /// </summary>
        /// <param name="commandTree">DbCommand tree</param>
        /// <returns>DbCommand produced by the current provider.</returns>
        internal DbCommand CreateCommand(DbModificationCommandTree commandTree)
        {
            DbCommand command;
            Debug.Assert(
                null != _providerServices, "constructor ensures either the command definition " +
                                            "builder or provider service is available");
            Debug.Assert(null != Connection.StoreConnection, "EntityAdapter.Update ensures the store connection is set");
            try
            {
                command = _providerServices.CreateCommand(commandTree);
            }
            catch (Exception e)
            {
                // we should not be wrapping all exceptions
                if (RequiresContext(e))
                {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw new EntityCommandCompilationException(Strings.EntityClient_CommandDefinitionPreparationFailed, e);
                }
                throw;
            }
            return command;
        }

        /// <summary>
        /// Helper method to allow the setting of parameter values to update stored procedures.
        /// Allows the DbProvider an opportunity to rewrite the parameter to suit provider specific needs.
        /// </summary>
        /// <param name="parameter">Parameter to set.</param>
        /// <param name="typeUsage">The type of the parameter.</param>
        /// <param name="value">The value to which to set the parameter.</param>
        internal void SetParameterValue(DbParameter parameter, TypeUsage typeUsage, object value)
        {
            _providerServices.SetParameterValue(parameter, typeUsage, value);
        }

        /// <summary>
        /// Determines whether the given exception requires additional context from the update pipeline (in other
        /// words, whether the exception should be wrapped in an UpdateException).
        /// </summary>
        /// <param name="e">Exception to test.</param>
        /// <returns>true if exception should be wrapped; false otherwise</returns>
        internal static bool RequiresContext(Exception e)
        {
            // if the exception isn't catchable, never wrap
            if (!EntityUtil.IsCatchableExceptionType(e))
            {
                return false;
            }

            // update and incompatible provider exceptions already contain the necessary context
            return !(e is UpdateException) && !(e is ProviderIncompatibleException);
        }

        #region Private initialization methods

        /// <summary>
        /// Retrieve all modified entries from the state manager.
        /// </summary>
        private void PullModifiedEntriesFromStateManager()
        {
            // do a first pass over added entries to register 'by value' entity key targets that may be resolved as 
            // via a foreign key
            foreach (var addedEntry in _stateManager.GetEntityStateEntries(EntityState.Added))
            {
                if (!addedEntry.IsRelationship
                    && !addedEntry.IsKeyEntry)
                {
                    KeyManager.RegisterKeyValueForAddedEntity(addedEntry);
                }
            }

            // do a second pass over entries to register referential integrity constraints
            // for server-generation
            foreach (
                var modifiedEntry in _stateManager.GetEntityStateEntries(EntityState.Modified | EntityState.Added | EntityState.Deleted))
            {
                RegisterReferentialConstraints(modifiedEntry);
            }

            foreach (
                var modifiedEntry in _stateManager.GetEntityStateEntries(EntityState.Modified | EntityState.Added | EntityState.Deleted))
            {
                LoadStateEntry(modifiedEntry);
            }
        }

        /// <summary>
        /// Retrieve all required/optional/value entries into the state manager. These are entries that --
        /// although unmodified -- affect or are affected by updates.
        /// </summary>
        private void PullUnchangedEntriesFromStateManager()
        {
            foreach (var required in _requiredEntities)
            {
                var key = required.Key;

                if (!_knownEntityKeys.Contains(key))
                {
                    // pull the value into the translator if we don't already it
                    IEntityStateEntry requiredEntry;

                    if (_stateManager.TryGetEntityStateEntry(key, out requiredEntry)
                        && !requiredEntry.IsKeyEntry)
                    {
                        // load the object as a no-op update
                        LoadStateEntry(requiredEntry);
                    }
                    else
                    {
                        // throw an exception
                        throw EntityUtil.Update(Strings.Update_MissingEntity(required.Value.Name, TypeHelpers.GetFullName(key.EntityContainerName, key.EntitySetName)), null);
                    }
                }
            }

            foreach (var key in _optionalEntities)
            {
                if (!_knownEntityKeys.Contains(key))
                {
                    IEntityStateEntry optionalEntry;

                    if (_stateManager.TryGetEntityStateEntry(key, out optionalEntry)
                        && !optionalEntry.IsKeyEntry)
                    {
                        // load the object as a no-op update
                        LoadStateEntry(optionalEntry);
                    }
                }
            }

            foreach (var key in _includedValueEntities)
            {
                if (!_knownEntityKeys.Contains(key))
                {
                    IEntityStateEntry valueEntry;

                    if (_stateManager.TryGetEntityStateEntry(key, out valueEntry))
                    {
                        // Convert state entry so that its values are known to the update pipeline.
                        var result = _recordConverter.ConvertCurrentValuesToPropagatorResult(
                            valueEntry, ModifiedPropertiesBehavior.NoneModified);
                    }
                }
            }
        }

        /// <summary>
        /// Validates and tracks a state entry being processed by this translator.
        /// </summary>
        /// <param name="stateEntry"></param>
        private void ValidateAndRegisterStateEntry(IEntityStateEntry stateEntry)
        {
            Contract.Requires(stateEntry != null);

            var extent = stateEntry.EntitySet;
            if (null == extent)
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidStateEntry, 1, null);
            }

            // Determine the key. May be null if the state entry does not represent an entity.
            var entityKey = stateEntry.EntityKey;
            IExtendedDataRecord record = null;

            // verify the structure of the entry values
            if (0 != ((EntityState.Added | EntityState.Modified | EntityState.Unchanged) & stateEntry.State))
            {
                // added, modified and unchanged entries have current values
                record = stateEntry.CurrentValues;
                ValidateRecord(extent, record);
            }
            if (0 != ((EntityState.Modified | EntityState.Deleted | EntityState.Unchanged) & stateEntry.State))
            {
                // deleted, modified and unchanged entries have original values
                record = (IExtendedDataRecord)stateEntry.OriginalValues;
                ValidateRecord(extent, record);
            }
            Debug.Assert(null != record, "every state entry must contain a record");

            // check for required ends of relationships
            var associationSet = extent as AssociationSet;
            if (null != associationSet)
            {
                var associationSetMetadata = ViewLoader.GetAssociationSetMetadata(associationSet, MetadataWorkspace);

                if (associationSetMetadata.HasEnds)
                {
                    foreach (var field in record.DataRecordInfo.FieldMetadata)
                    {
                        // ends of relationship record must be EntityKeys
                        var end = (EntityKey)record.GetValue(field.Ordinal);

                        // ends of relationships must have AssociationEndMember metadata
                        var endMetadata = (AssociationEndMember)field.FieldType;

                        if (associationSetMetadata.RequiredEnds.Contains(endMetadata))
                        {
                            if (!_requiredEntities.ContainsKey(end))
                            {
                                _requiredEntities.Add(end, associationSet);
                            }
                        }

                        else if (associationSetMetadata.OptionalEnds.Contains(endMetadata))
                        {
                            AddValidAncillaryKey(end, _optionalEntities);
                        }

                        else if (associationSetMetadata.IncludedValueEnds.Contains(endMetadata))
                        {
                            AddValidAncillaryKey(end, _includedValueEntities);
                        }
                    }
                }

                // register relationship with validator
                _constraintValidator.RegisterAssociation(associationSet, record, stateEntry);
            }
            else
            {
                // register entity with validator
                _constraintValidator.RegisterEntity(stateEntry);
            }

            // add to the list of entries being tracked
            _stateEntries.Add(stateEntry);
            if (null != (object)entityKey)
            {
                _knownEntityKeys.Add(entityKey);
            }
        }

        /// <summary>
        /// effects: given an entity key and a set, adds key to the set iff. the corresponding entity
        /// is:
        /// 
        ///     not a stub (or 'key') entry, and;
        ///     not a core element in the update pipeline (it's not being directly modified)
        /// </summary>
        private void AddValidAncillaryKey(EntityKey key, Set<EntityKey> keySet)
        {
            // Note: an entity is ancillary iff. it is unchanged (otherwise it is tracked as a "standard" changed entity)
            IEntityStateEntry endEntry;
            if (_stateManager.TryGetEntityStateEntry(key, out endEntry) && // make sure the entity is tracked
                !endEntry.IsKeyEntry
                && // make sure the entity is not a stub
                endEntry.State == EntityState.Unchanged) // if the entity is being modified, it's already included anyways
            {
                keySet.Add(key);
            }
        }

        private void ValidateRecord(EntitySetBase extent, IExtendedDataRecord record)
        {
            Debug.Assert(null != extent, "must be verified by caller");

            DataRecordInfo recordInfo;
            if ((null == record) ||
                (null == (recordInfo = record.DataRecordInfo))
                ||
                (null == recordInfo.RecordType))
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidStateEntry, 2, null);
            }

            VerifyExtent(MetadataWorkspace, extent);

            // additional validation happens lazily as values are loaded from the record
        }

        // Verifies the given extent is present in the given workspace.
        private static void VerifyExtent(MetadataWorkspace workspace, EntitySetBase extent)
        {
            // get the container to which the given extent belongs
            var actualContainer = extent.EntityContainer;

            // try to retrieve the container in the given workspace
            EntityContainer referenceContainer = null;
            if (null != actualContainer)
            {
                workspace.TryGetEntityContainer(
                    actualContainer.Name, actualContainer.DataSpace, out referenceContainer);
            }

            // determine if the given extent lives in a container from the given workspace
            // (the item collections for each container are reference equivalent when they are declared in the
            // same item collection)
            if (null == actualContainer || null == referenceContainer
                ||
                !ReferenceEquals(actualContainer, referenceContainer))
            {
                // FUTURE(CMeek):: We use reference equality to determine if two containers have compatible
                // Metadata. This is overly strict in some scenarios. At present, Metadata does not expose
                // any services to determine compatibility, so for now this is the best we can do. In most
                // scenarios, Metadata caching ensures the same container is returned anyways.
                throw EntityUtil.Update(Strings.Update_WorkspaceMismatch, null);
            }
        }

        private void LoadStateEntry(IEntityStateEntry stateEntry)
        {
            Debug.Assert(null != stateEntry, "state entry must exist");

            // make sure the state entry doesn't contain invalid data and register it with the
            // update pipeline
            ValidateAndRegisterStateEntry(stateEntry);

            // use data structure internal to the update pipeline instead of the raw state entry
            var extractedStateEntry = new ExtractedStateEntry(this, stateEntry);

            // figure out if this state entry is being handled by a function (stored procedure) or
            // through dynamic SQL
            var extent = stateEntry.EntitySet;
            if (null == ViewLoader.GetFunctionMappingTranslator(extent, MetadataWorkspace))
            {
                // if there is no function mapping, register a ChangeNode (used for update
                // propagation and dynamic SQL generation)
                var changeNode = GetExtentModifications(extent);
                if (null != extractedStateEntry.Original)
                {
                    changeNode.Deleted.Add(extractedStateEntry.Original);
                }
                if (null != extractedStateEntry.Current)
                {
                    changeNode.Inserted.Add(extractedStateEntry.Current);
                }
            }
            else
            {
                // for function updates, store off the extracted state entry in its entirety
                // (used when producing FunctionUpdateCommands)
                var functionEntries = GetExtentFunctionModifications(extent);
                functionEntries.Add(extractedStateEntry);
            }
        }

        /// <summary>
        /// Retrieve a change node for an extent. If none exists, creates and registers a new one.
        /// </summary>
        /// <param name="extent">Extent for which to return a change node.</param>
        /// <returns>Change node for requested extent.</returns>
        internal ChangeNode GetExtentModifications(EntitySetBase extent)
        {
            Contract.Requires(extent != null);
            Debug.Assert(null != _changes, "(UpdateTranslator/GetChangeNodeForExtent) method called before translator initialized");

            ChangeNode changeNode;

            if (!_changes.TryGetValue(extent, out changeNode))
            {
                changeNode = new ChangeNode(TypeUsage.Create(extent.ElementType));
                _changes.Add(extent, changeNode);
            }

            return changeNode;
        }

        /// <summary>
        /// Retrieve a list of state entries being processed by custom user functions.
        /// </summary>
        /// <param name="extent">Extent for which to return entries.</param>
        /// <returns>List storing the entries.</returns>
        internal List<ExtractedStateEntry> GetExtentFunctionModifications(EntitySetBase extent)
        {
            Contract.Requires(extent != null);
            Debug.Assert(null != _functionChanges, "method called before translator initialized");

            List<ExtractedStateEntry> entries;

            if (!_functionChanges.TryGetValue(extent, out entries))
            {
                entries = new List<ExtractedStateEntry>();
                _functionChanges.Add(extent, entries);
            }

            return entries;
        }

        #endregion

        #endregion

        /// <summary>
        /// Class validating relationship cardinality constraints. Only reasons about constraints that can be inferred
        /// by examining change requests from the store.
        /// (no attempt is made to ensure consistency of the store subsequently, since this would require pulling in all
        /// values from the store).
        /// </summary>
        private class RelationshipConstraintValidator
        {
            #region Constructor

            internal RelationshipConstraintValidator()
            {
                m_existingRelationships =
                    new Dictionary<DirectionalRelationship, DirectionalRelationship>(EqualityComparer<DirectionalRelationship>.Default);
                m_impliedRelationships =
                    new Dictionary<DirectionalRelationship, IEntityStateEntry>(EqualityComparer<DirectionalRelationship>.Default);
                m_referencingRelationshipSets = new Dictionary<EntitySet, List<AssociationSet>>(EqualityComparer<EntitySet>.Default);
            }

            #endregion

            #region Fields

            /// <summary>
            /// Relationships registered in the validator.
            /// </summary>
            private readonly Dictionary<DirectionalRelationship, DirectionalRelationship> m_existingRelationships;

            /// <summary>
            /// Relationships the validator determines are required based on registered entities.
            /// </summary>
            private readonly Dictionary<DirectionalRelationship, IEntityStateEntry> m_impliedRelationships;

            /// <summary>
            /// Cache used to store relationship sets with ends bound to entity sets.
            /// </summary>
            private readonly Dictionary<EntitySet, List<AssociationSet>> m_referencingRelationshipSets;

            #endregion

            #region Methods

            /// <summary>
            /// Add an entity to be tracked by the validator. Requires that the input describes an entity.
            /// </summary>
            /// <param name="stateEntry">State entry for the entity being tracked.</param>
            internal void RegisterEntity(IEntityStateEntry stateEntry)
            {
                Contract.Requires(stateEntry != null);

                if (EntityState.Added == stateEntry.State
                    || EntityState.Deleted == stateEntry.State)
                {
                    // We only track added and deleted entities because modifications to entities do not affect
                    // cardinality constraints. Relationships are based on end keys, and it is not
                    // possible to modify key values.
                    Debug.Assert(null != (object)stateEntry.EntityKey, "entity state entry must have an entity key");
                    var entityKey = stateEntry.EntityKey;
                    var entitySet = (EntitySet)stateEntry.EntitySet;
                    var entityType = EntityState.Added == stateEntry.State
                                         ? GetEntityType(stateEntry.CurrentValues)
                                         : GetEntityType(stateEntry.OriginalValues);

                    // figure out relationship set ends that are associated with this entity set
                    foreach (var associationSet in GetReferencingAssocationSets(entitySet))
                    {
                        // describe unidirectional relationships in which the added entity is the "destination"
                        var ends = associationSet.AssociationSetEnds;
                        foreach (var fromEnd in ends)
                        {
                            foreach (var toEnd in ends)
                            {
                                // end to itself does not describe an interesting relationship subpart
                                if (ReferenceEquals(
                                    toEnd.CorrespondingAssociationEndMember,
                                    fromEnd.CorrespondingAssociationEndMember))
                                {
                                    continue;
                                }

                                // skip ends that don't target the current entity set
                                if (!toEnd.EntitySet.EdmEquals(entitySet))
                                {
                                    continue;
                                }

                                // skip ends that aren't required
                                if (0 == MetadataHelper.GetLowerBoundOfMultiplicity(
                                    fromEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity))
                                {
                                    continue;
                                }

                                // skip ends that don't target the current entity type
                                if (!MetadataHelper.GetEntityTypeForEnd(toEnd.CorrespondingAssociationEndMember)
                                         .IsAssignableFrom(entityType))
                                {
                                    continue;
                                }

                                // register the relationship so that we know it's required
                                var relationship = new DirectionalRelationship(
                                    entityKey, fromEnd.CorrespondingAssociationEndMember,
                                    toEnd.CorrespondingAssociationEndMember, associationSet, stateEntry);
                                m_impliedRelationships.Add(relationship, stateEntry);
                            }
                        }
                    }
                }
            }

            // requires: input is an IExtendedDataRecord representing an entity
            // returns: entity type for the given record
            private static EntityType GetEntityType(DbDataRecord dbDataRecord)
            {
                var extendedRecord = dbDataRecord as IExtendedDataRecord;
                Debug.Assert(extendedRecord != null);

                Debug.Assert(BuiltInTypeKind.EntityType == extendedRecord.DataRecordInfo.RecordType.EdmType.BuiltInTypeKind);
                return (EntityType)extendedRecord.DataRecordInfo.RecordType.EdmType;
            }

            /// <summary>
            /// Add a relationship to be tracked by the validator.
            /// </summary>
            /// <param name="associationSet">Relationship set to which the given record belongs.</param>
            /// <param name="record">Relationship record. Must conform to the type of the relationship set.</param>
            /// <param name="stateEntry">State entry for the relationship being tracked</param>
            internal void RegisterAssociation(AssociationSet associationSet, IExtendedDataRecord record, IEntityStateEntry stateEntry)
            {
                Contract.Requires(associationSet != null);
                Contract.Requires(record != null);
                Contract.Requires(stateEntry != null);

                Debug.Assert(associationSet.ElementType.Equals(record.DataRecordInfo.RecordType.EdmType));

                // retrieve the ends of the relationship
                var endNameToKeyMap = new Dictionary<string, EntityKey>(
                    StringComparer.Ordinal);
                foreach (var field in record.DataRecordInfo.FieldMetadata)
                {
                    var endName = field.FieldType.Name;
                    var entityKey = (EntityKey)record.GetValue(field.Ordinal);
                    endNameToKeyMap.Add(endName, entityKey);
                }

                // register each unidirectional relationship subpart in the relationship instance
                var ends = associationSet.AssociationSetEnds;
                foreach (var fromEnd in ends)
                {
                    foreach (var toEnd in ends)
                    {
                        // end to itself does not describe an interesting relationship subpart
                        if (ReferenceEquals(toEnd.CorrespondingAssociationEndMember, fromEnd.CorrespondingAssociationEndMember))
                        {
                            continue;
                        }

                        var toEntityKey = endNameToKeyMap[toEnd.CorrespondingAssociationEndMember.Name];
                        var relationship = new DirectionalRelationship(
                            toEntityKey, fromEnd.CorrespondingAssociationEndMember,
                            toEnd.CorrespondingAssociationEndMember, associationSet, stateEntry);
                        AddExistingRelationship(relationship);
                    }
                }
            }

            /// <summary>
            /// Validates cardinality constraints for all added entities/relationships.
            /// </summary>
            internal void ValidateConstraints()
            {
                // ensure all expected relationships exist
                foreach (var expected in m_impliedRelationships)
                {
                    var expectedRelationship = expected.Key;
                    var stateEntry = expected.Value;

                    // determine actual end cardinality
                    var count = GetDirectionalRelationshipCountDelta(expectedRelationship);

                    if (EntityState.Deleted
                        == stateEntry.State)
                    {
                        // our cardinality expectations are reversed for delete (cardinality of 1 indicates
                        // we want -1 operation total)
                        count = -count;
                    }

                    // determine expected cardinality
                    var minimumCount = MetadataHelper.GetLowerBoundOfMultiplicity(expectedRelationship.FromEnd.RelationshipMultiplicity);
                    var maximumCountDeclared =
                        MetadataHelper.GetUpperBoundOfMultiplicity(expectedRelationship.FromEnd.RelationshipMultiplicity);
                    var maximumCount = maximumCountDeclared.HasValue ? maximumCountDeclared.Value : count; // negative value
                    // indicates unlimited cardinality

                    if (count < minimumCount
                        || count > maximumCount)
                    {
                        // We could in theory "fix" the cardinality constraint violation by introducing surrogates,
                        // but we risk doing work on behalf of the user they don't want performed (e.g., deleting an
                        // entity or relationship the user has intentionally left untouched).
                        throw EntityUtil.UpdateRelationshipCardinalityConstraintViolation(
                            expectedRelationship.AssociationSet.Name, minimumCount, maximumCountDeclared,
                            TypeHelpers.GetFullName(
                                expectedRelationship.ToEntityKey.EntityContainerName, expectedRelationship.ToEntityKey.EntitySetName),
                            count, expectedRelationship.FromEnd.Name,
                            stateEntry);
                    }
                }

                // ensure actual relationships have required ends
                foreach (var actualRelationship in m_existingRelationships.Keys)
                {
                    int addedCount;
                    int deletedCount;
                    actualRelationship.GetCountsInEquivalenceSet(out addedCount, out deletedCount);
                    var absoluteCount = Math.Abs(addedCount - deletedCount);
                    var minimumCount = MetadataHelper.GetLowerBoundOfMultiplicity(actualRelationship.FromEnd.RelationshipMultiplicity);
                    var maximumCount = MetadataHelper.GetUpperBoundOfMultiplicity(actualRelationship.FromEnd.RelationshipMultiplicity);

                    // Check that we haven't inserted or deleted too many relationships
                    if (maximumCount.HasValue)
                    {
                        var violationType = default(EntityState?);
                        var violationCount = default(int?);
                        if (addedCount > maximumCount.Value)
                        {
                            violationType = EntityState.Added;
                            violationCount = addedCount;
                        }
                        else if (deletedCount > maximumCount.Value)
                        {
                            violationType = EntityState.Deleted;
                            violationCount = deletedCount;
                        }
                        if (violationType.HasValue)
                        {
                            throw new UpdateException(Strings.Update_RelationshipCardinalityViolation(
                                maximumCount.Value,
                                violationType.Value, actualRelationship.AssociationSet.ElementType.FullName,
                                actualRelationship.FromEnd.Name, actualRelationship.ToEnd.Name, violationCount.Value), null, actualRelationship.GetEquivalenceSet().Select(reln => reln.StateEntry).Cast<ObjectStateEntry>().Distinct());
                        }
                    }

                    // We care about the case where there is a relationship but no entity when
                    // the relationship and entity map to the same table. If there is a relationship
                    // with 1..1 cardinality to the entity and the relationship is being added or deleted,
                    // it is required that the entity is also added or deleted.
                    if (1 == absoluteCount && 1 == minimumCount
                        && 1 == maximumCount) // 1..1 relationship being added/deleted
                    {
                        var isAdd = addedCount > deletedCount;

                        // Ensure the entity is also being added or deleted
                        IEntityStateEntry entityEntry;

                        // Identify the following error conditions:
                        // - the entity is not being modified at all
                        // - the entity is being modified, but not in the way we expect (it's not being added or deleted)
                        if (!m_impliedRelationships.TryGetValue(actualRelationship, out entityEntry) ||
                            (isAdd && EntityState.Added != entityEntry.State)
                            ||
                            (!isAdd && EntityState.Deleted != entityEntry.State))
                        {
                            var message = Strings.Update_MissingRequiredEntity(actualRelationship.AssociationSet.Name, actualRelationship.StateEntry.State, actualRelationship.ToEnd.Name);
                            throw EntityUtil.Update(message, null, actualRelationship.StateEntry);
                        }
                    }
                }
            }

            /// <summary>
            /// Determines the net change in relationship count.
            /// For instance, if the directional relationship is added 2 times and deleted 3, the return value is -1.
            /// </summary>
            private int GetDirectionalRelationshipCountDelta(DirectionalRelationship expectedRelationship)
            {
                // lookup up existing relationship from expected relationship
                DirectionalRelationship existingRelationship;
                if (m_existingRelationships.TryGetValue(expectedRelationship, out existingRelationship))
                {
                    int addedCount;
                    int deletedCount;
                    existingRelationship.GetCountsInEquivalenceSet(out addedCount, out deletedCount);
                    return addedCount - deletedCount;
                }
                else
                {
                    // no modifications to the relationship... return 0 (no net change)
                    return 0;
                }
            }

            private void AddExistingRelationship(DirectionalRelationship relationship)
            {
                DirectionalRelationship existingRelationship;
                if (m_existingRelationships.TryGetValue(relationship, out existingRelationship))
                {
                    existingRelationship.AddToEquivalenceSet(relationship);
                }
                else
                {
                    m_existingRelationships.Add(relationship, relationship);
                }
            }

            /// <summary>
            /// Determine which relationship sets reference the given entity set.
            /// </summary>
            /// <param name="entitySet">Entity set for which to identify relationships</param>
            /// <returns>Relationship sets referencing the given entity set</returns>
            private IEnumerable<AssociationSet> GetReferencingAssocationSets(EntitySet entitySet)
            {
                List<AssociationSet> relationshipSets;

                // check if this information is cached
                if (!m_referencingRelationshipSets.TryGetValue(entitySet, out relationshipSets))
                {
                    relationshipSets = new List<AssociationSet>();

                    // relationship sets must live in the same container as the entity sets they reference
                    var container = entitySet.EntityContainer;
                    foreach (var extent in container.BaseEntitySets)
                    {
                        var associationSet = extent as AssociationSet;

                        if (null != associationSet
                            && !associationSet.ElementType.IsForeignKey)
                        {
                            foreach (var end in associationSet.AssociationSetEnds)
                            {
                                if (end.EntitySet.Equals(entitySet))
                                {
                                    relationshipSets.Add(associationSet);
                                    break;
                                }
                            }
                        }
                    }

                    // add referencing relationship information to the cache
                    m_referencingRelationshipSets.Add(entitySet, relationshipSets);
                }

                return relationshipSets;
            }

            #endregion

            #region Nested types

            /// <summary>
            /// An instance of an actual or expected relationship. This class describes one direction
            /// of the relationship. 
            /// </summary>
            private class DirectionalRelationship : IEquatable<DirectionalRelationship>
            {
                /// <summary>
                /// Entity key for the entity being referenced by the relationship.
                /// </summary>
                internal readonly EntityKey ToEntityKey;

                /// <summary>
                /// Name of the end referencing the entity key.
                /// </summary>
                internal readonly AssociationEndMember FromEnd;

                /// <summary>
                /// Name of the end the entity key references.
                /// </summary>
                internal readonly AssociationEndMember ToEnd;

                /// <summary>
                /// State entry containing this relationship.
                /// </summary>
                internal readonly IEntityStateEntry StateEntry;

                /// <summary>
                /// Reference to the relationship set.
                /// </summary>
                internal readonly AssociationSet AssociationSet;

                /// <summary>
                /// Reference to next 'equivalent' relationship in circular linked list.
                /// </summary>
                private DirectionalRelationship _equivalenceSetLinkedListNext;

                private readonly int _hashCode;

                internal DirectionalRelationship(
                    EntityKey toEntityKey, AssociationEndMember fromEnd, AssociationEndMember toEnd, AssociationSet associationSet,
                    IEntityStateEntry stateEntry)
                {
                    Contract.Requires(toEntityKey != null);
                    Contract.Requires(fromEnd != null);
                    Contract.Requires(toEnd != null);
                    Contract.Requires(associationSet != null);
                    Contract.Requires(stateEntry != null);

                    ToEntityKey = toEntityKey;
                    FromEnd = fromEnd;
                    ToEnd = toEnd;
                    AssociationSet = associationSet;
                    StateEntry = stateEntry;
                    _equivalenceSetLinkedListNext = this;

                    _hashCode = toEntityKey.GetHashCode() ^
                                fromEnd.GetHashCode() ^
                                toEnd.GetHashCode() ^
                                associationSet.GetHashCode();
                }

                /// <summary>
                /// Requires: 'other' must refer to the same relationship metadata and the same target entity and
                /// must not already be a part of an equivalent set.
                /// Adds the given relationship to linked list containing all equivalent relationship instances
                /// for this relationship (e.g. all orders associated with a specific customer)
                /// </summary>
                internal void AddToEquivalenceSet(DirectionalRelationship other)
                {
                    Debug.Assert(null != other, "other must not be null");
                    Debug.Assert(Equals(other), "other must be another instance of the same relationship target");
                    Debug.Assert(
                        ReferenceEquals(other._equivalenceSetLinkedListNext, other), "other must not be part of an equivalence set yet");
                    var currentSuccessor = _equivalenceSetLinkedListNext;
                    _equivalenceSetLinkedListNext = other;
                    other._equivalenceSetLinkedListNext = currentSuccessor;
                }

                /// <summary>
                /// Returns all relationships in equivalence set.
                /// </summary>
                internal IEnumerable<DirectionalRelationship> GetEquivalenceSet()
                {
                    // yield everything in circular linked list
                    var current = this;
                    do
                    {
                        yield return current;
                        current = current._equivalenceSetLinkedListNext;
                    }
                    while (!ReferenceEquals(current, this));
                }

                /// <summary>
                /// Determines the number of add and delete operations contained in this equivalence set.
                /// </summary>
                internal void GetCountsInEquivalenceSet(out int addedCount, out int deletedCount)
                {
                    addedCount = 0;
                    deletedCount = 0;
                    // yield everything in circular linked list
                    var current = this;
                    do
                    {
                        if (current.StateEntry.State
                            == EntityState.Added)
                        {
                            addedCount++;
                        }
                        else if (current.StateEntry.State
                                 == EntityState.Deleted)
                        {
                            deletedCount++;
                        }
                        current = current._equivalenceSetLinkedListNext;
                    }
                    while (!ReferenceEquals(current, this));
                }

                public override int GetHashCode()
                {
                    return _hashCode;
                }

                public bool Equals(DirectionalRelationship other)
                {
                    if (ReferenceEquals(this, other))
                    {
                        return true;
                    }
                    if (null == other)
                    {
                        return false;
                    }
                    if (ToEntityKey != other.ToEntityKey)
                    {
                        return false;
                    }
                    if (AssociationSet != other.AssociationSet)
                    {
                        return false;
                    }
                    if (ToEnd != other.ToEnd)
                    {
                        return false;
                    }
                    if (FromEnd != other.FromEnd)
                    {
                        return false;
                    }
                    return true;
                }

                public override bool Equals(object obj)
                {
                    Debug.Fail("use only typed Equals method");
                    return Equals(obj as DirectionalRelationship);
                }

                public override string ToString()
                {
                    return String.Format(
                        CultureInfo.InvariantCulture, "{0}.{1}-->{2}: {3}",
                        AssociationSet.Name, FromEnd.Name, ToEnd.Name,
                        StringUtil.BuildDelimitedList(ToEntityKey.EntityKeyValues, null, null));
                }
            }

            #endregion
        }
    }
}
