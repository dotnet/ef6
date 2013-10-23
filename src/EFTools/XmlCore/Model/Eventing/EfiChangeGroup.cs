// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    using System.Collections.Generic;

    /// <summary>
    ///     This represents the argument that will be sent to subscribers for the
    ///     SchemaChanged event handler in our service below.  Generally, it
    ///     corresponds to a set of atomic changes from a transaction, either
    ///     initiated by SchemaLite, by another client of the XML Editor, or a sync
    ///     from disk.
    /// </summary>
    internal class EfiChangeGroup
    {
        private readonly EfiTransaction _tx;
        private readonly List<EfiChange> _creates = new List<EfiChange>();
        private readonly List<EfiChange> _deletes = new List<EfiChange>();
        private readonly Dictionary<EFObject, EfiChange> _updates = new Dictionary<EFObject, EfiChange>();

        internal EfiChangeGroup(EfiTransaction tx)
        {
            _tx = tx;
        }

        internal void RecordModelChange(
            EfiChange.EfiChangeType changeType, EFObject changed,
            string property, object oldValue, object newValue)
        {
            switch (changeType)
            {
                case EfiChange.EfiChangeType.Create:
                    _creates.Add(new EfiChange(changeType, changed));
                    break;
                case EfiChange.EfiChangeType.Delete:
                    _deletes.Add(new EfiChange(changeType, changed));
                    break;
                case EfiChange.EfiChangeType.Update:
                    EfiChange change = null;
                    if (_updates.ContainsKey(changed))
                    {
                        change = _updates[changed];
                    }
                    else
                    {
                        change = new EfiChange(changeType, changed);
                        _updates[changed] = change;
                    }
                    change.RecordModelChange(property, oldValue, newValue);
                    break;
            }
        }

        /// <summary>
        ///     Sorts the change group using stable sort algorithm (i.e. one that preservs order of equal values).
        /// </summary>
        internal ICollection<EfiChange> SortChangesForProcessing(EfiChangeComparer changeComparer)
        {
            var changes = new List<EfiChangeStableSortItem>(Count);
            var i = 0;
            foreach (var change in Changes)
            {
                changes.Add(new EfiChangeStableSortItem(change, i));
                i++;
            }
            changes.Sort(changeComparer);

            var result = new List<EfiChange>(changes.Count);
            foreach (var entry in changes)
            {
                result.Add(entry.EfiChange);
            }

            return result;
        }

        /// <summary>
        ///     Returns the total number of changes in this change group.
        /// </summary>
        internal int Count
        {
            get { return _creates.Count + _updates.Count + _deletes.Count; }
        }

        /// <summary>
        ///     Returns a list of the changes encapsulated by this change group.
        /// </summary>
        internal IEnumerable<EfiChange> Changes
        {
            get
            {
                foreach (var createChange in _creates)
                {
                    yield return createChange;
                }
                foreach (var updateChange in _updates.Values)
                {
                    yield return updateChange;
                }
                foreach (var deleteChange in _deletes)
                {
                    yield return deleteChange;
                }
            }
        }

        /// <summary>
        ///     The transaction causing this change in the case of a commit or
        ///     rollback change.
        /// </summary>
        internal EfiTransaction Transaction
        {
            get { return _tx; }
        }
    }
}
