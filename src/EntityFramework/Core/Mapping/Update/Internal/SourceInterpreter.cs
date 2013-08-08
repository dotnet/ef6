// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// This class determines the state entries contributing to an expression
    /// propagated through an update mapping view (values in propagated expressions
    /// remember where they come from)
    /// </summary>
    internal class SourceInterpreter
    {
        private SourceInterpreter(UpdateTranslator translator, EntitySet sourceTable)
        {
            m_stateEntries = new List<IEntityStateEntry>();
            m_translator = translator;
            m_sourceTable = sourceTable;
        }

        private readonly List<IEntityStateEntry> m_stateEntries;
        private readonly UpdateTranslator m_translator;
        private readonly EntitySet m_sourceTable;

        /// <summary>
        /// Finds all markup associated with the given source.
        /// </summary>
        /// <param name="source"> Source expression. Must not be null. </param>
        /// <param name="translator"> Translator containing session information. </param>
        /// <param name="sourceTable"> Table from which the exception was thrown (must not be null). </param>
        /// <returns> Markup. </returns>
        internal static ReadOnlyCollection<IEntityStateEntry> GetAllStateEntries(
            PropagatorResult source, UpdateTranslator translator,
            EntitySet sourceTable)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(translator);
            DebugCheck.NotNull(sourceTable);

            var interpreter = new SourceInterpreter(translator, sourceTable);
            interpreter.RetrieveResultMarkup(source);

            return new ReadOnlyCollection<IEntityStateEntry>(interpreter.m_stateEntries);
        }

        private void RetrieveResultMarkup(PropagatorResult source)
        {
            DebugCheck.NotNull(source);

            if (source.Identifier
                != PropagatorResult.NullIdentifier)
            {
                // state entries travel with identifiers. several state entries may be merged
                // into a single identifier result via joins in the update mapping view
                do
                {
                    if (null != source.StateEntry)
                    {
                        m_stateEntries.Add(source.StateEntry);
                        if (source.Identifier
                            != PropagatorResult.NullIdentifier)
                        {
                            // if this is an identifier, it may also be registered with an "owner".
                            // Return the owner as well if the owner is also mapped to this table.
                            PropagatorResult owner;
                            if (m_translator.KeyManager.TryGetIdentifierOwner(source.Identifier, out owner)
                                && null != owner.StateEntry
                                && ExtentInScope(owner.StateEntry.EntitySet))
                            {
                                m_stateEntries.Add(owner.StateEntry);
                            }

                            // Check if are any referential constraints. If so, the entity key
                            // implies that the dependent relationship instance is also being
                            // handled in this result.
                            foreach (var stateEntry in m_translator.KeyManager.GetDependentStateEntries(source.Identifier))
                            {
                                m_stateEntries.Add(stateEntry);
                            }
                        }
                    }
                    source = source.Next;
                }
                while (null != source);
            }
            else if (!source.IsSimple
                     && !source.IsNull)
            {
                // walk children
                foreach (var child in source.GetMemberValues())
                {
                    RetrieveResultMarkup(child);
                }
            }
        }

        // Determines whether the given table is in scope for the current source: if the source
        // table does not map to the source table for this interpreter, it is not in scope
        // for exceptions thrown from this table.
        private bool ExtentInScope(EntitySetBase extent)
        {
            if (null == extent)
            {
                return false;
            }
            // determine if the extent is mapped to this table
            return m_translator.ViewLoader.GetAffectedTables(extent, m_translator.MetadataWorkspace).Contains(m_sourceTable);
        }
    }
}
