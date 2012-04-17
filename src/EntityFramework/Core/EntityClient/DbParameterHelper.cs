namespace System.Data.Entity.Core.EntityClient
{
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    public sealed partial class EntityParameter : DbParameter
    {
        private object _value;

        private object _parent;

        private ParameterDirection _direction;
        private int? _size;

        private string _sourceColumn;
        private DataRowVersion _sourceVersion;
        private bool _sourceColumnNullMapping;

        private bool? _isNullable;

        private EntityParameter(EntityParameter source)
            : this()
        {
            Contract.Requires(source != null);

            source.CloneHelper(this);

            var cloneable = (_value as ICloneable);
            if (null != cloneable)
            {
                _value = cloneable.Clone();
            }
        }

        private object CoercedValue { get; set; }

        [
            RefreshProperties(RefreshProperties.All)
        ]
        [EntityResCategory(EntityRes.DataCategory_Data)]
        [EntityResDescription(EntityRes.DbParameter_Direction)]
        public override ParameterDirection Direction
        {
            get
            {
                var direction = _direction;
                return ((0 != direction) ? direction : ParameterDirection.Input);
            }
            set
            {
                if (_direction != value)
                {
                    switch (value)
                    {
                        case ParameterDirection.Input:
                        case ParameterDirection.Output:
                        case ParameterDirection.InputOutput:
                        case ParameterDirection.ReturnValue:
                            PropertyChanging();
                            _direction = value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(typeof(ParameterDirection).Name, Strings.ADP_InvalidEnumerationValue(typeof(ParameterDirection).Name, ((int)value).ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }
        }

        public override bool IsNullable
        {
            get
            {
                var result = _isNullable.HasValue ? _isNullable.Value : true;
                return result;
            }
            set { _isNullable = value; }
        }

        [
            EntityResCategory(EntityRes.DataCategory_Data)
        ]
        [EntityResDescription(EntityRes.DbParameter_Size)]
        public override int Size
        {
            get
            {
                var size = _size.HasValue ? _size.Value : 0;
                if (0 == size)
                {
                    size = ValueSize(Value);
                }
                return size;
            }
            set
            {
                if (!_size.HasValue
                    || _size.Value != value)
                {
                    if (value < -1)
                    {
                        throw new ArgumentException(Strings.ADP_InvalidSizeValue(value.ToString(CultureInfo.InvariantCulture)));
                    }
                    PropertyChanging();
                    if (0 == value)
                    {
                        _size = null;
                    }
                    else
                    {
                        _size = value;
                    }
                }
            }
        }

        private void ResetSize()
        {
            if (_size.HasValue)
            {
                PropertyChanging();
                _size = null;
            }
        }

        private bool ShouldSerializeSize()
        {
            return (_size.HasValue && _size.Value != 0);
        }

        [
            EntityResCategory(EntityRes.DataCategory_Update)
        ]
        [EntityResDescription(EntityRes.DbParameter_SourceColumn)]
        public override string SourceColumn
        {
            get
            {
                var sourceColumn = _sourceColumn;
                return ((null != sourceColumn) ? sourceColumn : string.Empty);
            }
            set { _sourceColumn = value; }
        }

        public override bool SourceColumnNullMapping
        {
            get { return _sourceColumnNullMapping; }
            set { _sourceColumnNullMapping = value; }
        }

        [
            EntityResCategory(EntityRes.DataCategory_Update)
        ]
        [EntityResDescription(EntityRes.DbParameter_SourceVersion)]
        public override DataRowVersion SourceVersion
        {
            get
            {
                var sourceVersion = _sourceVersion;
                return ((0 != sourceVersion) ? sourceVersion : DataRowVersion.Current);
            }
            set
            {
                switch (value)
                {
                    case DataRowVersion.Original:
                    case DataRowVersion.Current:
                    case DataRowVersion.Proposed:
                    case DataRowVersion.Default:
                        _sourceVersion = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(typeof(DataRowVersion).Name, Strings.ADP_InvalidEnumerationValue(typeof(DataRowVersion).Name, ((int)value).ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        private void CloneHelperCore(EntityParameter destination)
        {
            destination._value = _value;

            destination._direction = _direction;
            destination._size = _size;

            destination._sourceColumn = _sourceColumn;
            destination._sourceVersion = _sourceVersion;
            destination._sourceColumnNullMapping = _sourceColumnNullMapping;
            destination._isNullable = _isNullable;
        }

        internal void CopyTo(DbParameter destination)
        {
            Contract.Requires(destination != null);
            CloneHelper((EntityParameter)destination);
        }

        internal object CompareExchangeParent(object value, object comparand)
        {
            var parent = _parent;
            if (comparand == parent)
            {
                _parent = value;
            }
            return parent;
        }

        internal void ResetParent()
        {
            _parent = null;
        }

        public override string ToString()
        {
            return ParameterName;
        }

        private static int ValueSizeCore(object value)
        {
            if (!EntityUtil.IsNull(value))
            {
                var svalue = (value as string);
                if (null != svalue)
                {
                    return svalue.Length;
                }
                var bvalue = (value as byte[]);
                if (null != bvalue)
                {
                    return bvalue.Length;
                }
                var cvalue = (value as char[]);
                if (null != cvalue)
                {
                    return cvalue.Length;
                }
                if ((value is byte)
                    || (value is char))
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}
