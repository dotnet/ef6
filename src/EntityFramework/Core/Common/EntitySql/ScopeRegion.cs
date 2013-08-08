// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Diagnostics;

    internal sealed class ScopeRegion
    {
        private readonly ScopeManager _scopeManager;

        internal ScopeRegion(ScopeManager scopeManager, int firstScopeIndex, int scopeRegionIndex)
        {
            _scopeManager = scopeManager;
            _firstScopeIndex = firstScopeIndex;
            _scopeRegionIndex = scopeRegionIndex;
        }

        /// <summary>
        /// First scope of the region.
        /// </summary>
        internal int FirstScopeIndex
        {
            get { return _firstScopeIndex; }
        }

        private readonly int _firstScopeIndex;

        /// <summary>
        /// Index of the scope region.
        /// Outer scope regions have smaller index value than inner scope regions.
        /// </summary>
        internal int ScopeRegionIndex
        {
            get { return _scopeRegionIndex; }
        }

        private readonly int _scopeRegionIndex;

        /// <summary>
        /// True if given scope is in the current scope region.
        /// </summary>
        internal bool ContainsScope(int scopeIndex)
        {
            return (scopeIndex >= _firstScopeIndex);
        }

        /// <summary>
        /// Marks current scope region as performing group/folding operation.
        /// </summary>
        internal void EnterGroupOperation(DbExpressionBinding groupAggregateBinding)
        {
            Debug.Assert(!IsAggregating, "Scope region group operation is not reentrant.");
            _groupAggregateBinding = groupAggregateBinding;
        }

        /// <summary>
        /// Clears the <see cref="IsAggregating" /> flag on the group scope.
        /// </summary>
        internal void RollbackGroupOperation()
        {
            Debug.Assert(IsAggregating, "Scope region must inside group operation in order to leave it.");
            _groupAggregateBinding = null;
        }

        /// <summary>
        /// True when the scope region performs group/folding operation.
        /// </summary>
        internal bool IsAggregating
        {
            get { return _groupAggregateBinding != null; }
        }

        internal DbExpressionBinding GroupAggregateBinding
        {
            get
            {
                Debug.Assert(IsAggregating, "IsAggregating must be true.");
                return _groupAggregateBinding;
            }
        }

        private DbExpressionBinding _groupAggregateBinding;

        /// <summary>
        /// Returns list of group aggregates evaluated on the scope region.
        /// </summary>
        internal List<GroupAggregateInfo> GroupAggregateInfos
        {
            get { return _groupAggregateInfos; }
        }

        private readonly List<GroupAggregateInfo> _groupAggregateInfos = new List<GroupAggregateInfo>();

        /// <summary>
        /// Adds group aggregate name to the scope region.
        /// </summary>
        internal void RegisterGroupAggregateName(string groupAggregateName)
        {
            Debug.Assert(!_groupAggregateNames.Contains(groupAggregateName), "!_groupAggregateNames.ContainsKey(groupAggregateName)");
            _groupAggregateNames.Add(groupAggregateName);
        }

        internal bool ContainsGroupAggregate(string groupAggregateName)
        {
            return _groupAggregateNames.Contains(groupAggregateName);
        }

        private readonly HashSet<string> _groupAggregateNames = new HashSet<string>();

        /// <summary>
        /// True if a recent expression resolution was correlated.
        /// </summary>
        internal bool WasResolutionCorrelated { get; set; }

        /// <summary>
        /// Applies <paramref name="action" /> to all scope entries in the current scope region.
        /// </summary>
        internal void ApplyToScopeEntries(Action<ScopeEntry> action)
        {
            Debug.Assert(FirstScopeIndex <= _scopeManager.CurrentScopeIndex, "FirstScopeIndex <= CurrentScopeIndex");

            for (var i = FirstScopeIndex; i <= _scopeManager.CurrentScopeIndex; ++i)
            {
                foreach (var scopeEntry in _scopeManager.GetScopeByIndex(i))
                {
                    action(scopeEntry.Value);
                }
            }
        }

        /// <summary>
        /// Applies <paramref name="action" /> to all scope entries in the current scope region.
        /// </summary>
        internal void ApplyToScopeEntries(Func<ScopeEntry, ScopeEntry> action)
        {
            Debug.Assert(FirstScopeIndex <= _scopeManager.CurrentScopeIndex, "FirstScopeIndex <= CurrentScopeIndex");

            for (var i = FirstScopeIndex; i <= _scopeManager.CurrentScopeIndex; ++i)
            {
                var scope = _scopeManager.GetScopeByIndex(i);
                List<KeyValuePair<string, ScopeEntry>> updatedEntries = null;
                foreach (var scopeEntry in scope)
                {
                    var newScopeEntry = action(scopeEntry.Value);
                    Debug.Assert(newScopeEntry != null, "newScopeEntry != null");
                    if (scopeEntry.Value != newScopeEntry)
                    {
                        if (updatedEntries == null)
                        {
                            updatedEntries = new List<KeyValuePair<string, ScopeEntry>>();
                        }
                        updatedEntries.Add(new KeyValuePair<string, ScopeEntry>(scopeEntry.Key, newScopeEntry));
                    }
                }
                if (updatedEntries != null)
                {
                    updatedEntries.ForEach((updatedScopeEntry) => scope.Replace(updatedScopeEntry.Key, updatedScopeEntry.Value));
                }
            }
        }

        internal void RollbackAllScopes()
        {
            _scopeManager.RollbackToScope(FirstScopeIndex - 1);
        }
    }
}
