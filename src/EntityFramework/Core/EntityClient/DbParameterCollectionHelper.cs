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

        public override bool IsFixedSize
        {
            get { return ((IList)InnerList).IsFixedSize; }
        }

        public override bool IsReadOnly
        {
            get { return ((IList)InnerList).IsReadOnly; }
        }

        public override bool IsSynchronized
        {
            get { return ((ICollection)InnerList).IsSynchronized; }
        }

        public override object SyncRoot
        {
            get { return ((ICollection)InnerList).SyncRoot; }
        }

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

        public override bool Contains(object value)
        {
            return (-1 != IndexOf(value));
        }

        public override void CopyTo(Array array, int index)
        {
            ((ICollection)InnerList).CopyTo(array, index);
        }

        public override IEnumerator GetEnumerator()
        {
            return ((ICollection)InnerList).GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            RangeCheck(index);
            return InnerList[index];
        }

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

        public override int IndexOf(string parameterName)
        {
            return IndexOf(InnerList, parameterName);
        }

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
            else if (this
                     != ((EntityParameter)value).CompareExchangeParent(null, this))
            {
                throw new ArgumentException(Strings.EntityParameterCollectionRemoveInvalidObject);
            }
        }

        public override void RemoveAt(int index)
        {
            OnChange();
            RangeCheck(index);
            RemoveIndex(index);
        }

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

        protected override void SetParameter(int index, DbParameter value)
        {
            OnChange();
            RangeCheck(index);
            Replace(index, value);
        }

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
            if (null == value)
            {
                throw new ArgumentNullException("value", Strings.EntityParameterNull);
            }

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
            if (null == value)
            {
                throw new ArgumentNullException("value", Strings.EntityParameterNull);
            }
            else if (!_itemType.IsInstanceOfType(value))
            {
                throw new InvalidCastException(Strings.InvalidEntityParameterType(value.GetType().Name));
            }
        }
    };
}
