// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Utilities;
    using System.Threading;

    // <summary>
    // Cached dynamic method to get the property value from a CLR instance
    // </summary>
    internal class NavigationPropertyAccessor
    {
        public NavigationPropertyAccessor(string propertyName)
        {
            _propertyName = propertyName;
        }

        private Func<object, object> _memberGetter;
        private Action<object, object> _memberSetter;
        private Action<object, object> _collectionAdd;
        private Func<object, object, bool> _collectionRemove;
        private Func<object> _collectionCreate;
        private readonly string _propertyName;

        public bool HasProperty
        {
            get { return (_propertyName != null); }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        // <summary>
// cached dynamic method to get the property value from a CLR instance
// </summary>
        public Func<object, object> ValueGetter
        {
            get { return _memberGetter; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberGetter, value, null);
            }
        }

        // <summary>
// cached dynamic method to set the property value from a CLR instance
// </summary>
        public Action<object, object> ValueSetter
        {
            get { return _memberSetter; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberSetter, value, null);
            }
        }

        public Action<object, object> CollectionAdd
        {
            get { return _collectionAdd; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _collectionAdd, value, null);
            }
        }

        public Func<object, object, bool> CollectionRemove
        {
            get { return _collectionRemove; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _collectionRemove, value, null);
            }
        }

        public Func<object> CollectionCreate
        {
            get { return _collectionCreate; }
            set
            {
                DebugCheck.NotNull(value);
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _collectionCreate, value, null);
            }
        }

        public static NavigationPropertyAccessor NoNavigationProperty
        {
            get { return new NavigationPropertyAccessor(null); }
        }
    }
}
