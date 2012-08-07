// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;

    /// <summary>
    ///     Represents information about one collection being managed by the NestOps. 
    ///     The CollectionVar is a Var that represents the entire collection.
    /// </summary>
    internal class CollectionInfo
    {
        #region public methods

        /// <summary>
        ///     The collection-var
        /// </summary>
        internal Var CollectionVar
        {
            get { return m_collectionVar; }
        }

        /// <summary>
        ///     the column map for the collection element
        /// </summary>
        internal ColumnMap ColumnMap
        {
            get { return m_columnMap; }
        }

        /// <summary>
        ///     list of vars describing the collection element; flattened to remove 
        ///     nested collections
        /// </summary>
        internal VarList FlattenedElementVars
        {
            get { return m_flattenedElementVars; }
        }

        /// <summary>
        ///     list of keys specific to this collection
        /// </summary>
        internal VarVec Keys
        {
            get { return m_keys; }
        }

        /// <summary>
        ///     list of sort keys specific to this collection
        /// </summary>
        internal List<SortKey> SortKeys
        {
            get { return m_sortKeys; }
        }

        /// <summary>
        ///     Discriminator Value for this collection (for a given NestOp).
        ///     Should we break this out into a subtype of CollectionInfo
        /// </summary>
        internal object DiscriminatorValue
        {
            get { return m_discriminatorValue; }
        }

        #endregion

        #region constructors

        internal CollectionInfo(
            Var collectionVar, ColumnMap columnMap, VarList flattenedElementVars, VarVec keys, List<SortKey> sortKeys,
            object discriminatorValue)
        {
            m_collectionVar = collectionVar;
            m_columnMap = columnMap;
            m_flattenedElementVars = flattenedElementVars;
            m_keys = keys;
            m_sortKeys = sortKeys;
            m_discriminatorValue = discriminatorValue;
        }

        #endregion

        #region private state

        private readonly Var m_collectionVar; // the collection Var
        private readonly ColumnMap m_columnMap; // column map for the collection element
        private readonly VarList m_flattenedElementVars; // elementVars, removing collections;
        private readonly VarVec m_keys; //list of keys specific to this collection
        private readonly List<SortKey> m_sortKeys; //list of sort keys specific to this collection
        private readonly object m_discriminatorValue;

        #endregion
    }
}
