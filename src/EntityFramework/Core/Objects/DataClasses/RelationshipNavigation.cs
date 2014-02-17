// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    // <summary>
    // This class describes a relationship navigation from the
    // navigation property on one entity to another entity.  It is
    // used throughout the collections and refs system to describe a
    // relationship and to connect from the navigation property on
    // one end of a relationship to the navigation property on the
    // other end.
    // </summary>
    [Serializable]
    internal class RelationshipNavigation
    {
        // ------------
        // Constructors
        // ------------

        // <summary>
        // Creates a navigation object with the given relationship
        // name, role name for the source and role name for the
        // destination.
        // </summary>
        // <param name="relationshipName"> Canonical-space name of the relationship. </param>
        // <param name="from"> Name of the role which is the source of the navigation. </param>
        // <param name="to"> Name of the role which is the destination of the navigation. </param>
        // <param name="fromAccessor"> The navigation property which is the source of the navigation. </param>
        // <param name="toAccessor"> The navigation property which is the destination of the navigation. </param>
        internal RelationshipNavigation(
            string relationshipName, string from, string to, NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
        {
            Check.NotEmpty(relationshipName, "relationshipName");
            Check.NotEmpty(@from, "from");
            Check.NotEmpty(to, "to");

            _relationshipName = relationshipName;
            _from = from;
            _to = to;

            _fromAccessor = fromAccessor;
            _toAccessor = toAccessor;
        }

        // <summary>
        // Creates a navigation object with the given relationship
        // name, role name for the source and role name for the
        // destination.
        // </summary>
        // <param name="associationType"> The association type representing the relationship. </param>
        // <param name="from"> Name of the role which is the source of the navigation. </param>
        // <param name="to"> Name of the role which is the destination of the navigation. </param>
        // <param name="fromAccessor"> The navigation property which is the source of the navigation. </param>
        // <param name="toAccessor"> The navigation property which is the destination of the navigation. </param>
        internal RelationshipNavigation(AssociationType associationType, string from, string to,
            NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
        {
            DebugCheck.NotNull(associationType);
            DebugCheck.NotEmpty(@from);
            DebugCheck.NotEmpty(to);

            _associationType = associationType;

            _relationshipName = associationType.FullName;
            _from = from;
            _to = to;

            _fromAccessor = fromAccessor;
            _toAccessor = toAccessor;
        }

        // ------
        // Fields
        // ------

        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private readonly string _relationshipName;
        private readonly string _from;
        private readonly string _to;

        [NonSerialized]
        private RelationshipNavigation _reverse;

        [NonSerialized]
        private NavigationPropertyAccessor _fromAccessor;

        [NonSerialized]
        private NavigationPropertyAccessor _toAccessor;

        [NonSerialized]
        private readonly AssociationType _associationType;

        internal AssociationType AssociationType
        {
            get { return _associationType; }
        }

        // ----------
        // Properties
        // ----------

        // <summary>
        // Canonical-space relationship name.
        // </summary>
        internal string RelationshipName
        {
            get { return _relationshipName; }
        }

        // <summary>
        // Role name for the source of this navigation.
        // </summary>
        internal string From
        {
            get { return _from; }
        }

        // <summary>
        // Role name for the destination of this navigation.
        // </summary>
        internal string To
        {
            get { return _to; }
        }

        // <summary>
        // Navigation property name for the destination of this navigation.
        // NOTE: There is not a FromPropertyAccessor property on RelationshipNavigation because it is not currently accessed anywhere
        // It is only used to calculate the "reverse" RelationshipNavigation.
        // </summary>
        internal NavigationPropertyAccessor ToPropertyAccessor
        {
            get { return _toAccessor; }
        }

        internal bool IsInitialized
        {
            get { return _toAccessor != null && _fromAccessor != null; }
        }

        internal void InitializeAccessors(NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
        {
            _fromAccessor = fromAccessor;
            _toAccessor = toAccessor;
        }

        // <summary>
        // The "reverse" version of this navigation.
        // </summary>
        internal RelationshipNavigation Reverse
        {
            get
            {
                if (_reverse == null
                    || !_reverse.IsInitialized)
                {
                    // the reverse relationship is exactly like this
                    // one but from & to are switched
                    _reverse = _associationType != null
                        ? new RelationshipNavigation(_associationType, _to, _from, _toAccessor, _fromAccessor)
                        : new RelationshipNavigation(_relationshipName, _to, _from, _toAccessor, _fromAccessor);
                }

                return _reverse;
            }
        }

        // <summary>
        // Compares this instance to a given Navigation by their values.
        // </summary>
        public override bool Equals(object obj)
        {
            var compareTo = obj as RelationshipNavigation;
            return ((this == compareTo)
                    || ((null != this) && (null != compareTo)
                        && (RelationshipName == compareTo.RelationshipName)
                        && (From == compareTo.From)
                        && (To == compareTo.To)));
        }

        // <summary>
        // Returns a value-based hash code.
        // </summary>
        // <returns> the hash value of this Navigation </returns>
        public override int GetHashCode()
        {
            return RelationshipName.GetHashCode();
        }

        // -------
        // Methods
        // -------

        // <summary>
        // ToString is provided to simplify debugging, etc.
        // </summary>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "RelationshipNavigation: ({0},{1},{2})",
                _relationshipName,
                _from,
                _to);
        }
    }
}
