// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Xml.Linq;

    /// <summary>
    /// An extended BindingList implementation that implements sorting.
    /// This class was adapted from the LINQ to SQL class of the same name.
    /// </summary>
    /// <typeparam name="T"> The element type. </typeparam>
    internal class SortableBindingList<T> : BindingList<T>
    {
        #region Fields and constructors

        private bool _isSorted;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor _sortProperty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortableBindingList{T}" /> class with the
        /// the given underlying list.  Note that sorting is dependent on having an actual <see cref="List{T}" />
        /// rather than some other ICollection implementation.
        /// </summary>
        /// <param name="list"> The list. </param>
        public SortableBindingList(List<T> list)
            : base(list)
        {
            DebugCheck.NotNull(list);
        }

        #endregion

        #region BindingList overrides

        /// <summary>
        /// Applies sorting to the list.
        /// </summary>
        /// <param name="prop"> The property to sort by. </param>
        /// <param name="direction"> The sort direction. </param>
        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (PropertyComparer.CanSort(prop.PropertyType))
            {
                ((List<T>)Items).Sort(new PropertyComparer(prop, direction));
                _sortDirection = direction;
                _sortProperty = prop;
                _isSorted = true;
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
        }

        /// <summary>
        /// Stops sorting.
        /// </summary>
        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _sortProperty = null;
        }

        /// <summary>
        /// Gets a value indicating whether this list is sorted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is sorted; otherwise, <c>false</c> .
        /// </value>
        protected override bool IsSortedCore
        {
            get { return _isSorted; }
        }

        /// <summary>
        /// Gets the sort direction.
        /// </summary>
        /// <value> The sort direction. </value>
        protected override ListSortDirection SortDirectionCore
        {
            get { return _sortDirection; }
        }

        /// <summary>
        /// Gets the sort property being used to sort.
        /// </summary>
        /// <value> The sort property. </value>
        protected override PropertyDescriptor SortPropertyCore
        {
            get { return _sortProperty; }
        }

        /// <summary>
        /// Returns <c>true</c> indicating that this list supports sorting.
        /// </summary>
        /// <value>
        /// <c>true</c> .
        /// </value>
        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        #endregion

        #region Comparer implementation

        /// <summary>
        /// Implements comparing for the <see cref="SortableBindingList{T}" /> implementation.
        /// </summary>
        internal class PropertyComparer : Comparer<T>
        {
            private readonly IComparer _comparer;
            private readonly ListSortDirection _direction;
            private readonly PropertyDescriptor _prop;
            private readonly bool _useToString;

            /// <summary>
            /// Initializes a new instance of the <see cref="SortableBindingList{T}.PropertyComparer" /> class
            /// for sorting the list.
            /// </summary>
            /// <param name="prop"> The property to sort by. </param>
            /// <param name="direction"> The sort direction. </param>
            public PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
            {
                if (!prop.ComponentType.IsAssignableFrom(typeof(T)))
                {
                    throw new MissingMemberException(typeof(T).Name, prop.Name);
                }

                Debug.Assert(CanSort(prop.PropertyType), "Cannot use PropertyComparer unless it can be compared by IComparable or ToString");

                _prop = prop;
                _direction = direction;

                if (CanSortWithIComparable(prop.PropertyType))
                {
                    var property = typeof(Comparer<>).MakeGenericType(new[] { prop.PropertyType }).GetDeclaredProperty("Default");
                    _comparer = (IComparer)property.GetValue(null, null);
                    _useToString = false;
                }
                else
                {
                    Debug.Assert(
                        CanSortWithToString(prop.PropertyType),
                        "Cannot use PropertyComparer unless it can be compared by IComparable or ToString");

                    _comparer = StringComparer.CurrentCultureIgnoreCase;
                    _useToString = true;
                }
            }

            /// <summary>
            /// Compares two instances of items in the list.
            /// </summary>
            /// <param name="left"> The left item to compare. </param>
            /// <param name="right"> The right item to compare. </param>
            public override int Compare(T left, T right)
            {
                var leftValue = _prop.GetValue(left);
                var rightValue = _prop.GetValue(right);

                if (_useToString)
                {
                    leftValue = leftValue != null ? leftValue.ToString() : null;
                    rightValue = rightValue != null ? rightValue.ToString() : null;
                }

                return _direction == ListSortDirection.Ascending
                           ? _comparer.Compare(leftValue, rightValue)
                           : _comparer.Compare(rightValue, leftValue);
            }

            /// <summary>
            /// Determines whether this instance can sort for the specified type.
            /// </summary>
            /// <param name="type"> The type. </param>
            /// <returns>
            /// <c>true</c> if this instance can sort for the specified type; otherwise, <c>false</c> .
            /// </returns>
            public static bool CanSort(Type type)
            {
                return CanSortWithToString(type) || CanSortWithIComparable(type);
            }

            /// <summary>
            /// Determines whether this instance can sort for the specified type using IComparable.
            /// </summary>
            /// <param name="type"> The type. </param>
            /// <returns>
            /// <c>true</c> if this instance can sort for the specified type; otherwise, <c>false</c> .
            /// </returns>
            private static bool CanSortWithIComparable(Type type)
            {
                return type.GetInterface("IComparable") != null ||
                       (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
            }

            /// <summary>
            /// Determines whether this instance can sort for the specified type using ToString.
            /// </summary>
            /// <param name="type"> The type. </param>
            /// <returns>
            /// <c>true</c> if this instance can sort for the specified type; otherwise, <c>false</c> .
            /// </returns>
            private static bool CanSortWithToString(Type type)
            {
                return type.Equals(typeof(XNode)) || type.IsSubclassOf(typeof(XNode));
            }
        }

        #endregion
    }
}
