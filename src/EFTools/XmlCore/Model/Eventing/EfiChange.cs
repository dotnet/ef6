// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    using System.Collections.Generic;

    /// <summary>
    ///     Represents a change to a field on an EFElement object (if type is update)
    ///     or else it represents the lifecycle event of an object creation or
    ///     deletion.
    /// </summary>
    internal class EfiChange
    {
        internal enum EfiChangeType
        {
            Create,
            Update,
            Delete
        }

        private readonly EfiChangeType _changeType;
        private readonly EFObject _changed;
        private readonly Dictionary<string, OldNewPair> _properties;

        internal EfiChange(EfiChangeType changeType, EFObject changed)
        {
            _changeType = changeType;
            _changed = changed;
            _properties = new Dictionary<string, OldNewPair>();
        }

        internal void RecordModelChange(string property, object oldValue, object newValue)
        {
            if (property == null)
            {
                return;
            }

            OldNewPair onp;
            if (_properties.TryGetValue(property, out onp))
            {
                onp.NewValue = newValue;
            }
            else
            {
                onp = new OldNewPair(oldValue, newValue);
                _properties[property] = onp;
            }
        }

        /// <summary>
        ///     Property indicating whether this is a create, update or delete event
        /// </summary>
        internal EfiChangeType Type
        {
            get { return _changeType; }
        }

        /// <summary>
        ///     Property indicating the created, updated or deleted EFElement.
        /// </summary>
        internal EFObject Changed
        {
            get { return _changed; }
        }

        /// <summary>
        ///     The updated properties of the info object in case of an update.
        ///     This returns a dictionary where each key is the name of an updated
        ///     property and the value is an OldNewPair allowing access to the old
        ///     and new value.
        /// </summary>
        internal Dictionary<string, OldNewPair> Properties
        {
            get { return _properties; }
        }
    }
}
