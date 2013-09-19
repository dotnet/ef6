// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public sealed partial class EntityParameterCollection : DbParameterCollection
    {
        private List<EntityParameter> _items;

        /// <summary>
        /// Gets an Integer that contains the number of elements in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <returns>
        /// The number of elements in the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> as an Integer.
        /// </returns>
        public override int Count
        {
            get { return ((null != _items) ? _items.Count : 0); }
        }

        private List<EntityParameter> InnerList
        {
            get
            {
                var items = _items;

                if (null == items)
                {
                    items = new List<EntityParameter>();
                    _items = items;
                }
                return items;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// has a fixed size.
        /// </summary>
        /// <returns>
        /// Returns true if the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> has a fixed size; otherwise false.
        /// </returns>
        public override bool IsFixedSize
        {
            get { return ((IList)InnerList).IsFixedSize; }
        }

        /// <summary>
        /// Gets a value that indicates whether the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// is read-only.
        /// </summary>
        /// <returns>
        /// Returns true if the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> is read only; otherwise false.
        /// </returns>
        public override bool IsReadOnly
        {
            get { return ((IList)InnerList).IsReadOnly; }
        }

        /// <summary>
        /// Gets a value that indicates whether the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// is synchronized.
        /// </summary>
        /// <returns>
        /// Returns true if the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> is synchronized; otherwise false.
        /// </returns>
        public override bool IsSynchronized
        {
            get { return ((ICollection)InnerList).IsSynchronized; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </returns>
        public override object SyncRoot
        {
            get { return ((ICollection)InnerList).SyncRoot; }
        }

        /// <summary>
        /// Adds the specified object to the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />.
        /// </summary>
        /// <returns>
        /// The index of the new <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object.
        /// </returns>
        /// <param name="value">
        /// An <see cref="T:System.Object" />.
        /// </param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int Add(object value)
        {
            OnChange();

            Check.NotNull(value, "value");

            ValidateType(value);
            Validate(-1, value);
            InnerList.Add((EntityParameter)value);
            return Count - 1;
        }

        /// <summary>
        /// Adds an array of values to the end of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <param name="values">
        /// The <see cref="T:System.Array" /> values to add.
        /// </param>
        public override void AddRange(Array values)
        {
            OnChange();

            Check.NotNull(values, "values");

            foreach (var value in values)
            {
                ValidateType(value);
            }
            foreach (EntityParameter value in values)
            {
                Validate(-1, value);
                InnerList.Add(value);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private int CheckName(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
            {
                throw new IndexOutOfRangeException(Strings.EntityParameterCollectionInvalidParameterName(parameterName));
            }
            return index;
        }

        /// <summary>
        /// Removes all the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> objects from the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        public override void Clear()
        {
            OnChange();
            var items = InnerList;

            if (null != items)
            {
                foreach (var item in items)
                {
                    item.ResetParent();
                }
                items.Clear();
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object" /> is in this
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> contains the value; otherwise false.
        /// </returns>
        /// <param name="value">
        /// The <see cref="T:System.Object" /> value.
        /// </param>
        public override bool Contains(object value)
        {
            return (-1 != IndexOf(value));
        }

        /// <summary>
        /// Copies all the elements of the current <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" /> to the specified one-dimensional
        /// <see
        ///     cref="T:System.Array" />
        /// starting at the specified destination <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from the current
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </param>
        /// <param name="index">
        /// A 32-bit integer that represents the index in the <see cref="T:System.Array" /> at which copying starts.
        /// </param>
        public override void CopyTo(Array array, int index)
        {
            ((ICollection)InnerList).CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> for the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return ((ICollection)InnerList).GetEnumerator();
        }

        /// <inhertidoc />
        protected override DbParameter GetParameter(int index)
        {
            RangeCheck(index);
            return InnerList[index];
        }

        /// <inhertidoc />
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        protected override DbParameter GetParameter(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index < 0)
            {
                throw new IndexOutOfRangeException(Strings.EntityParameterCollectionInvalidParameterName(parameterName));
            }
            return InnerList[index];
        }

        private static int IndexOf(IEnumerable items, string parameterName)
        {
            if (null != items)
            {
                var i = 0;

                foreach (EntityParameter parameter in items)
                {
                    if (0 == EntityUtil.SrcCompare(parameterName, parameter.ParameterName))
                    {
                        return i;
                    }
                    ++i;
                }
                i = 0;

                foreach (EntityParameter parameter in items)
                {
                    if (0 == EntityUtil.DstCompare(parameterName, parameter.ParameterName))
                    {
                        return i;
                    }
                    ++i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the location of the specified <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> with the specified name.
        /// </summary>
        /// <returns>
        /// The zero-based location of the specified <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> with the specified case-sensitive name. Returns -1 when the object does not exist in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </returns>
        /// <param name="parameterName">
        /// The case-sensitive name of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> to find.
        /// </param>
        public override int IndexOf(string parameterName)
        {
            return IndexOf(InnerList, parameterName);
        }

        /// <summary>
        /// Gets the location of the specified <see cref="T:System.Object" /> in the collection.
        /// </summary>
        /// <returns>
        /// The zero-based location of the specified <see cref="T:System.Object" /> that is a
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" />
        /// in the collection. Returns -1 when the object does not exist in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </returns>
        /// <param name="value">
        /// The <see cref="T:System.Object" /> to find.
        /// </param>
        public override int IndexOf(object value)
        {
            if (null != value)
            {
                ValidateType(value);

                var items = InnerList;

                if (null != items)
                {
                    var count = items.Count;

                    for (var i = 0; i < count; i++)
                    {
                        if (value == items[i])
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Inserts an <see cref="T:System.Object" /> into the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">
        /// An <see cref="T:System.Object" /> to be inserted in the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// .
        /// </param>
        public override void Insert(int index, object value)
        {
            OnChange();

            Check.NotNull(value, "value");

            ValidateType(value);
            Validate(-1, value);
            InnerList.Insert(index, (EntityParameter)value);
        }

        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        private void RangeCheck(int index)
        {
            if ((index < 0)
                || (Count <= index))
            {
                throw new IndexOutOfRangeException(
                    Strings.EntityParameterCollectionInvalidIndex(
                        index.ToString(CultureInfo.InvariantCulture), Count.ToString(CultureInfo.InvariantCulture)));
            }
        }

        /// <summary>Removes the specified parameter from the collection.</summary>
        /// <param name="value">
        /// A <see cref="T:System.Object" /> object to remove from the collection.
        /// </param>
        public override void Remove(object value)
        {
            OnChange();

            Check.NotNull(value, "value");

            ValidateType(value);
            var index = IndexOf(value);
            if (-1 != index)
            {
                RemoveIndex(index);
            }
            else if (this != ((EntityParameter)value).CompareExchangeParent(null, this))
            {
                throw new ArgumentException(Strings.EntityParameterCollectionRemoveInvalidObject);
            }
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> from the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> object to remove.
        /// </param>
        public override void RemoveAt(int index)
        {
            OnChange();
            RangeCheck(index);
            RemoveIndex(index);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> from the
        /// <see
        ///     cref="T:System.Data.Entity.Core.EntityClient.EntityParameterCollection" />
        /// at the specified parameter name.
        /// </summary>
        /// <param name="parameterName">
        /// The name of the <see cref="T:System.Data.Entity.Core.EntityClient.EntityParameter" /> to remove.
        /// </param>
        public override void RemoveAt(string parameterName)
        {
            OnChange();
            var index = CheckName(parameterName);
            RemoveIndex(index);
        }

        private void RemoveIndex(int index)
        {
            var items = InnerList;
            Debug.Assert((null != items) && (0 <= index) && (index < Count), "RemoveIndex, invalid");
            var item = items[index];
            items.RemoveAt(index);
            item.ResetParent();
        }

        private void Replace(int index, object newValue)
        {
            var items = InnerList;
            Debug.Assert((null != items) && (0 <= index) && (index < Count), "Replace Index invalid");
            ValidateType(newValue);
            Validate(index, newValue);
            var item = items[index];
            items[index] = (EntityParameter)newValue;
            item.ResetParent();
        }

        /// <inhertidoc />
        protected override void SetParameter(int index, DbParameter value)
        {
            OnChange();
            RangeCheck(index);
            Replace(index, value);
        }

        /// <inhertidoc />
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            OnChange();
            var index = IndexOf(parameterName);
            if (index < 0)
            {
                throw new IndexOutOfRangeException(Strings.EntityParameterCollectionInvalidParameterName(parameterName));
            }
            Replace(index, value);
        }

        private void Validate(int index, object value)
        {
            Check.NotNull(value, "value");

            var entityParameter = (EntityParameter)value;
            var parent = entityParameter.CompareExchangeParent(this, null);
            if (null != parent)
            {
                if (this != parent)
                {
                    throw new ArgumentException(Strings.EntityParameterContainedByAnotherCollection);
                }
                if (index != IndexOf(value))
                {
                    throw new ArgumentException(Strings.EntityParameterContainedByAnotherCollection);
                }
            }

            var name = entityParameter.ParameterName;
            if (0 == name.Length)
            {
                index = 1;
                do
                {
                    name = EntityUtil.Parameter + index.ToString(CultureInfo.CurrentCulture);
                    index++;
                }
                while (-1
                       != IndexOf(name));
                entityParameter.ParameterName = name;
            }
        }

        private static void ValidateType(object value)
        {
            Check.NotNull(value, "value");

            if (!_itemType.IsInstanceOfType(value))
            {
                throw new InvalidCastException(Strings.InvalidEntityParameterType(value.GetType().Name));
            }
        }
    };
}
