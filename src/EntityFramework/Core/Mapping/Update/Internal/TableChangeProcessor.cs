namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
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
    /// Processes changes applying to a table by merging inserts and deletes into updates
    /// where appropriate.
    /// </summary>
    /// <remarks>
    /// This class is essentially responsible for identifying inserts, deletes
    /// and updates in a particular table based on the <see cref="ChangeNode" />
    /// produced by value propagation w.r.t. the update mapping view for that table.
    /// Assumes the change node includes at most a single insert and at most a single delete
    /// for a given key (where we have both, the change is treated as an update).
    /// </remarks>
    internal class TableChangeProcessor
    {
        #region Constructors

        /// <summary>
        /// Constructs processor based on the contents of a change node.
        /// </summary>
        /// <param name="table">Table for which changes are being processed.</param>
        internal TableChangeProcessor(EntitySet table)
        {
            Contract.Requires(table != null);

            m_table = table;

            // cache information about table key
            m_keyOrdinals = InitializeKeyOrdinals(table);
        }

        #endregion

        #region Fields

        private readonly EntitySet m_table;
        private readonly int[] m_keyOrdinals;

        #endregion

        #region Properties

        /// <summary>
        /// Gets metadata for the table being modified.
        /// </summary>
        internal EntitySet Table
        {
            get { return m_table; }
        }

        /// <summary>
        /// Gets a map from column ordinal to property descriptions for columns that are components of the table's
        /// primary key.
        /// </summary>
        internal int[] KeyOrdinals
        {
            get { return m_keyOrdinals; }
        }

        #endregion

        #region Methods

        // Determines whether the given ordinal position in the property list
        // for this table is a key value.
        internal bool IsKeyProperty(int propertyOrdinal)
        {
            foreach (var keyOrdinal in m_keyOrdinals)
            {
                if (propertyOrdinal == keyOrdinal)
                {
                    return true;
                }
            }
            return false;
        }

        // Determines which column ordinals in the table are part of the key.
        private static int[] InitializeKeyOrdinals(EntitySet table)
        {
            var tableType = table.ElementType;
            IList<EdmMember> keyMembers = tableType.KeyMembers;
            var members = TypeHelpers.GetAllStructuralMembers(tableType);
            var keyOrdinals = new int[keyMembers.Count];

            for (var keyMemberIndex = 0; keyMemberIndex < keyMembers.Count; keyMemberIndex++)
            {
                var keyMember = keyMembers[keyMemberIndex];
                keyOrdinals[keyMemberIndex] = members.IndexOf(keyMember);

                Debug.Assert(
                    keyOrdinals[keyMemberIndex] >= 0 && keyOrdinals[keyMemberIndex] < members.Count,
                    "an EntityType key member must also be a member of the entity type");
            }

            return keyOrdinals;
        }

        // Processes all insert and delete requests in the table's <see cref="ChangeNode" />. Inserts
        // and deletes with the same key are merged into updates.
        internal List<UpdateCommand> CompileCommands(ChangeNode changeNode, UpdateCompiler compiler)
        {
            var keys = new Set<CompositeKey>(compiler.m_translator.KeyComparer);

            // Retrieve all delete results (original values) and insert results (current values) while
            // populating a set of all row keys. The set contains a single key per row.
            var deleteResults = ProcessKeys(compiler, changeNode.Deleted, keys);
            var insertResults = ProcessKeys(compiler, changeNode.Inserted, keys);

            var commands = new List<UpdateCommand>(deleteResults.Count + insertResults.Count);

            // Examine each row key to see if the row is being deleted, inserted or updated
            foreach (var key in keys)
            {
                PropagatorResult deleteResult;
                PropagatorResult insertResult;

                var hasDelete = deleteResults.TryGetValue(key, out deleteResult);
                var hasInsert = insertResults.TryGetValue(key, out insertResult);

                Debug.Assert(
                    hasDelete || hasInsert, "(update/TableChangeProcessor) m_keys must not contain a value " +
                                            "if there is no corresponding insert or delete");

                try
                {
                    if (!hasDelete)
                    {
                        // this is an insert
                        commands.Add(compiler.BuildInsertCommand(insertResult, this));
                    }
                    else if (!hasInsert)
                    {
                        // this is a delete
                        commands.Add(compiler.BuildDeleteCommand(deleteResult, this));
                    }
                    else
                    {
                        // this is an update because it has both a delete result and an insert result
                        var updateCommand = compiler.BuildUpdateCommand(deleteResult, insertResult, this);
                        if (null != updateCommand)
                        {
                            // if null is returned, it means it is a no-op update
                            commands.Add(updateCommand);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (UpdateTranslator.RequiresContext(e))
                    {
                        // collect state entries in scope for the current compilation
                        var stateEntries = new List<IEntityStateEntry>();
                        if (null != deleteResult)
                        {
                            stateEntries.AddRange(
                                SourceInterpreter.GetAllStateEntries(
                                    deleteResult, compiler.m_translator, m_table));
                        }
                        if (null != insertResult)
                        {
                            stateEntries.AddRange(
                                SourceInterpreter.GetAllStateEntries(
                                    insertResult, compiler.m_translator, m_table));
                        }

                        throw new UpdateException(Strings.Update_GeneralExecutionException, e, stateEntries.Cast<ObjectStateEntry>().Distinct());
                    }
                    throw;
                }
            }

            return commands;
        }

        // Determines key values for a list of changes. Side effect: populates <see cref="keys" /> which
        // includes an entry for every key involved in a change.
        private Dictionary<CompositeKey, PropagatorResult> ProcessKeys(
            UpdateCompiler compiler, List<PropagatorResult> changes, Set<CompositeKey> keys)
        {
            var map = new Dictionary<CompositeKey, PropagatorResult>(
                compiler.m_translator.KeyComparer);

            foreach (var change in changes)
            {
                // Reassign change to row since we cannot modify iteration variable
                var row = change;

                var key = new CompositeKey(GetKeyConstants(row));

                // Make sure we aren't inserting another row with the same key
                PropagatorResult other;
                if (map.TryGetValue(key, out other))
                {
                    DiagnoseKeyCollision(compiler, change, key, other);
                }

                map.Add(key, row);
                keys.Add(key);
            }

            return map;
        }

        [SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCode",
            Justification = "Based on Bug VSTS Pioneer #433188: IsVisibleOutsideAssembly is wrong on generic instantiations.")]
        private void DiagnoseKeyCollision(UpdateCompiler compiler, PropagatorResult change, CompositeKey key, PropagatorResult other)
        {
            var keyManager = compiler.m_translator.KeyManager;
            var otherKey = new CompositeKey(GetKeyConstants(other));

            // determine if the conflict is due to shared principal key values
            var sharedPrincipal = true;
            for (var i = 0; sharedPrincipal && i < key.KeyComponents.Length; i++)
            {
                var identifier1 = key.KeyComponents[i].Identifier;
                var identifier2 = otherKey.KeyComponents[i].Identifier;

                if (!keyManager.GetPrincipals(identifier1).Intersect(keyManager.GetPrincipals(identifier2)).Any())
                {
                    sharedPrincipal = false;
                }
            }

            if (sharedPrincipal)
            {
                // if the duplication is due to shared principals, there is a duplicate key exception
                var stateEntries = SourceInterpreter.GetAllStateEntries(change, compiler.m_translator, m_table)
                    .Concat(SourceInterpreter.GetAllStateEntries(other, compiler.m_translator, m_table));
                throw new UpdateException(Strings.Update_DuplicateKeys, null, stateEntries.Cast<ObjectStateEntry>().Distinct());
            }
            else
            {
                // if there are no shared principals, it implies that common dependents are the problem
                HashSet<IEntityStateEntry> commonDependents = null;
                foreach (var keyValue in key.KeyComponents.Concat(otherKey.KeyComponents))
                {
                    var dependents = new HashSet<IEntityStateEntry>();
                    foreach (var dependentId in keyManager.GetDependents(keyValue.Identifier))
                    {
                        PropagatorResult dependentResult;
                        if (keyManager.TryGetIdentifierOwner(dependentId, out dependentResult)
                            &&
                            null != dependentResult.StateEntry)
                        {
                            dependents.Add(dependentResult.StateEntry);
                        }
                    }
                    if (null == commonDependents)
                    {
                        commonDependents = new HashSet<IEntityStateEntry>(dependents);
                    }
                    else
                    {
                        commonDependents.IntersectWith(dependents);
                    }
                }

                // to ensure the exception shape is consistent with constraint violations discovered while processing
                // commands (a more conventional scenario in which different tables are contributing principal values)
                // wrap a DataConstraintException in an UpdateException
                throw new UpdateException(Strings.Update_GeneralExecutionException, new ConstraintException(Strings.Update_ReferentialConstraintIntegrityViolation), commonDependents.Cast<ObjectStateEntry>().Distinct());
            }
        }

        // Extracts key constants from the given row.
        private PropagatorResult[] GetKeyConstants(PropagatorResult row)
        {
            var keyConstants = new PropagatorResult[m_keyOrdinals.Length];
            for (var i = 0; i < m_keyOrdinals.Length; i++)
            {
                var constant = row.GetMemberValue(m_keyOrdinals[i]);

                keyConstants[i] = constant;
            }
            return keyConstants;
        }

        #endregion
    }
}
