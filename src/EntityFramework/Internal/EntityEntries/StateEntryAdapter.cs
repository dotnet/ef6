// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     This is a temporary adapter class that wraps an <see cref="ObjectStateEntry" /> and
    ///     presents it as an <see cref="IEntityStateEntry" />.  This class will be removed once
    ///     we roll into the System.Data.Entity assembly.  See <see cref="IEntityStateEntry" />
    ///     for more details.
    /// </summary>
    internal class StateEntryAdapter : IEntityStateEntry
    {
        #region Constructors and fields

        private readonly ObjectStateEntry _stateEntry;

        public StateEntryAdapter(ObjectStateEntry stateEntry)
        {
            Contract.Requires(stateEntry != null);

            _stateEntry = stateEntry;
        }

        #endregion

        #region IEntityStateEntry implementation

        public object Entity
        {
            get { return _stateEntry.Entity; }
        }

        public EntityState State
        {
            get { return _stateEntry.State; }
        }

        public void ChangeState(EntityState state)
        {
            _stateEntry.ChangeState(state);
        }

        public DbUpdatableDataRecord CurrentValues
        {
            get { return _stateEntry.CurrentValues; }
        }

        public DbUpdatableDataRecord GetUpdatableOriginalValues()
        {
            return _stateEntry.GetUpdatableOriginalValues();
        }

        public EntitySetBase EntitySet
        {
            get { return _stateEntry.EntitySet; }
        }

        public EntityKey EntityKey
        {
            get { return _stateEntry.EntityKey; }
        }

        public IEnumerable<string> GetModifiedProperties()
        {
            return _stateEntry.GetModifiedProperties();
        }

        public void SetModifiedProperty(string propertyName)
        {
            _stateEntry.SetModifiedProperty(propertyName);
        }

        public void RejectPropertyChanges(string propertyName)
        {
            _stateEntry.RejectPropertyChanges(propertyName);
        }

        public bool IsPropertyChanged(string propertyName)
        {
            return _stateEntry.IsPropertyChanged(propertyName);
        }

        #endregion
    }
}
