// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Spatial;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
#if !NET40
    using System.Threading;
    using System.Threading.Tasks;
#endif

    internal class ShapedBufferedDataRecord : BufferedDataRecordBase
    {
        private int _rowCapacity = 1;

        // Resizing bool[] is faster than BitArray, but the latter is more efficient for long-term storage.
        private BitArray _bools;
        private bool[] _tempBools;
        private int _boolCount;
        private byte[] _bytes;
        private int _byteCount;
        private char[] _chars;
        private int _charCount;
        private DateTime[] _dateTimes;
        private int _dateTimeCount;
        private decimal[] _decimals;
        private int _decimalCount;
        private double[] _doubles;
        private int _doubleCount;
        private float[] _floats;
        private int _floatCount;
        private Guid[] _guids;
        private int _guidCount;
        private short[] _shorts;
        private int _shortCount;
        private int[] _ints;
        private int _intCount;
        private long[] _longs;
        private int _longCount;
        private object[] _objects;
        private int _objectCount;
        private int[] _ordinalToIndexMap;

        private BitArray _nulls;
        private bool[] _tempNulls;
        private int _nullCount;
        private int[] _nullOrdinalToIndexMap;

        private TypeCase[] _columnTypeCases;

        protected ShapedBufferedDataRecord()
        {
        }

        internal static BufferedDataRecordBase Initialize(
            string providerManifestToken, DbProviderServices providerServices, DbDataReader reader, Type[] columnTypes,
            bool[] nullableColumns)
        {
            var record = new ShapedBufferedDataRecord();
            record.ReadMetadata(providerManifestToken, providerServices, reader);

            DbSpatialDataReader spatialDataReader = null;
            if (columnTypes.Any(t => t == typeof(DbGeography) || t == typeof(DbGeometry)))
            {
                spatialDataReader = providerServices.GetSpatialDataReader(reader, providerManifestToken);
            }

            return record.Initialize(reader, spatialDataReader, columnTypes, nullableColumns);
        }

#if !NET40

        internal static Task<BufferedDataRecordBase> InitializeAsync(
            string providerManifestToken, DbProviderServices providerServices, DbDataReader reader, Type[] columnTypes,
            bool[] nullableColumns, CancellationToken cancellationToken)
        {
            var record = new ShapedBufferedDataRecord();
            record.ReadMetadata(providerManifestToken, providerServices, reader);

            DbSpatialDataReader spatialDataReader = null;
            if (columnTypes.Any(t => t == typeof(DbGeography) || t == typeof(DbGeometry)))
            {
                spatialDataReader = providerServices.GetSpatialDataReader(reader, providerManifestToken);
            }

            return record.InitializeAsync(reader, spatialDataReader, columnTypes, nullableColumns, cancellationToken);
        }

#endif

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private BufferedDataRecordBase Initialize(
            DbDataReader reader, DbSpatialDataReader spatialDataReader, Type[] columnTypes, bool[] nullableColumns)
        {
            InitializeFields(columnTypes, nullableColumns);

            while (reader.Read())
            {
                _currentRowNumber++;

                if (_rowCapacity == _currentRowNumber)
                {
                    DoubleBufferCapacity();
                }

                var columnCount = Math.Max(columnTypes.Length, nullableColumns.Length);

                for (var i = 0; i < columnCount; i++)
                {
                    if (i < _columnTypeCases.Length)
                    {
                        switch (_columnTypeCases[i])
                        {
                            case TypeCase.Bool:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadBool(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadBool(reader, i);
                                }
                                break;
                            case TypeCase.Byte:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadByte(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadByte(reader, i);
                                }
                                break;
                            case TypeCase.Char:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadChar(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadChar(reader, i);
                                }
                                break;
                            case TypeCase.DateTime:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadDateTime(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadDateTime(reader, i);
                                }
                                break;
                            case TypeCase.Decimal:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadDecimal(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadDecimal(reader, i);
                                }
                                break;
                            case TypeCase.Double:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadDouble(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadDouble(reader, i);
                                }
                                break;
                            case TypeCase.Float:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadFloat(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadFloat(reader, i);
                                }
                                break;
                            case TypeCase.Guid:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadGuid(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadGuid(reader, i);
                                }
                                break;
                            case TypeCase.Short:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadShort(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadShort(reader, i);
                                }
                                break;
                            case TypeCase.Int:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadInt(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadInt(reader, i);
                                }
                                break;
                            case TypeCase.Long:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadLong(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadLong(reader, i);
                                }
                                break;
                            case TypeCase.DbGeography:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadGeography(spatialDataReader, i);
                                    }
                                }
                                else
                                {
                                    ReadGeography(spatialDataReader, i);
                                }
                                break;
                            case TypeCase.DbGeometry:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadGeometry(spatialDataReader, i);
                                    }
                                }
                                else
                                {
                                    ReadGeometry(spatialDataReader, i);
                                }
                                break;
                            case TypeCase.Empty:
                                if (nullableColumns[i])
                                {
                                    _tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i);
                                }
                                break;
                            default:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i)))
                                    {
                                        ReadObject(reader, i);
                                    }
                                }
                                else
                                {
                                    ReadObject(reader, i);
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (nullableColumns[i])
                        {
                            _tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = reader.IsDBNull(i);
                        }
                    }
                }
            }

            _bools = new BitArray(_tempBools);
            _tempBools = null;
            _nulls = new BitArray(_tempNulls);
            _tempNulls = null;
            _rowCount = _currentRowNumber + 1;
            _currentRowNumber = -1;

            return this;
        }

#if !NET40


        private async Task<BufferedDataRecordBase> InitializeAsync(
            DbDataReader reader, DbSpatialDataReader spatialDataReader, Type[] columnTypes, bool[] nullableColumns,
            CancellationToken cancellationToken)
        {
            InitializeFields(columnTypes, nullableColumns);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
            {
                _currentRowNumber++;

                if (_rowCapacity == _currentRowNumber)
                {
                    DoubleBufferCapacity();
                }

                var columnCount = columnTypes.Length > nullableColumns.Length
                                      ? columnTypes.Length
                                      : nullableColumns.Length;

                for (var i = 0; i < columnCount; i++)
                {
                    if (i < _columnTypeCases.Length)
                    {
                        switch (_columnTypeCases[i])
                        {
                            case TypeCase.Bool:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadBoolAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadBoolAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Byte:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadByteAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadByteAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Char:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadCharAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadCharAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.DateTime:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadDateTimeAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadDateTimeAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Decimal:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadDecimalAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadDecimalAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Double:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadDoubleAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadDoubleAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Float:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadFloatAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadFloatAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Guid:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadGuidAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadGuidAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Short:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadShortAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadShortAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Int:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadIntAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadIntAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Long:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadLongAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadLongAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.DbGeography:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadGeographyAsync(spatialDataReader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadGeographyAsync(spatialDataReader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.DbGeometry:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadGeometryAsync(spatialDataReader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadGeometryAsync(spatialDataReader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            case TypeCase.Empty:
                                if (nullableColumns[i])
                                {
                                    _tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                            default:
                                if (nullableColumns[i])
                                {
                                    if (!(_tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
                                    {
                                        await ReadObjectAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                    }
                                }
                                else
                                {
                                    await ReadObjectAsync(reader, i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (nullableColumns[i])
                        {
                            _tempNulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[i]] = await reader.IsDBNullAsync(i, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                        }
                    }
                }
            }

            _bools = new BitArray(_tempBools);
            _tempBools = null;
            _nulls = new BitArray(_tempNulls);
            _tempNulls = null;
            _rowCount = _currentRowNumber + 1;
            _currentRowNumber = -1;

            return this;
        }


#endif

        private void InitializeFields(Type[] columnTypes, bool[] nullableColumns)
        {
            _columnTypeCases = Enumerable.Repeat(TypeCase.Empty, columnTypes.Length).ToArray();
            var fieldCount = Math.Max(FieldCount, Math.Max(columnTypes.Length, nullableColumns.Length));

            _ordinalToIndexMap = Enumerable.Repeat(-1, fieldCount).ToArray();

            for (var i = 0; i < columnTypes.Length; i++)
            {
                var type = columnTypes[i];
                if (type == null)
                {
                }
                else if (type == typeof(bool))
                {
                    _columnTypeCases[i] = TypeCase.Bool;
                    _ordinalToIndexMap[i] = _boolCount;
                    _boolCount++;
                }
                else if (type == typeof(byte))
                {
                    _columnTypeCases[i] = TypeCase.Byte;
                    _ordinalToIndexMap[i] = _byteCount;
                    _byteCount++;
                }
                else if (type == typeof(char))
                {
                    _columnTypeCases[i] = TypeCase.Char;
                    _ordinalToIndexMap[i] = _charCount;
                    _charCount++;
                }
                else if (type == typeof(DateTime))
                {
                    _columnTypeCases[i] = TypeCase.DateTime;
                    _ordinalToIndexMap[i] = _dateTimeCount;
                    _dateTimeCount++;
                }
                else if (type == typeof(decimal))
                {
                    _columnTypeCases[i] = TypeCase.Decimal;
                    _ordinalToIndexMap[i] = _decimalCount;
                    _decimalCount++;
                }
                else if (type == typeof(double))
                {
                    _columnTypeCases[i] = TypeCase.Double;
                    _ordinalToIndexMap[i] = _doubleCount;
                    _doubleCount++;
                }
                else if (type == typeof(float))
                {
                    _columnTypeCases[i] = TypeCase.Float;
                    _ordinalToIndexMap[i] = _floatCount;
                    _floatCount++;
                }
                else if (type == typeof(Guid))
                {
                    _columnTypeCases[i] = TypeCase.Guid;
                    _ordinalToIndexMap[i] = _guidCount;
                    _guidCount++;
                }
                else if (type == typeof(short))
                {
                    _columnTypeCases[i] = TypeCase.Short;
                    _ordinalToIndexMap[i] = _shortCount;
                    _shortCount++;
                }
                else if (type == typeof(int))
                {
                    _columnTypeCases[i] = TypeCase.Int;
                    _ordinalToIndexMap[i] = _intCount;
                    _intCount++;
                }
                else if (type == typeof(long))
                {
                    _columnTypeCases[i] = TypeCase.Long;
                    _ordinalToIndexMap[i] = _longCount;
                    _longCount++;
                }
                else
                {
                    if (type == typeof(DbGeography))
                    {
                        _columnTypeCases[i] = TypeCase.DbGeography;
                    }
                    else if (type == typeof(DbGeometry))
                    {
                        _columnTypeCases[i] = TypeCase.DbGeometry;
                    }
                    else
                    {
                        _columnTypeCases[i] = TypeCase.Object;
                    }
                    _ordinalToIndexMap[i] = _objectCount;
                    _objectCount++;
                }
            }

            _tempBools = new bool[_rowCapacity * _boolCount];
            _bytes = new byte[_rowCapacity * _byteCount];
            _chars = new char[_rowCapacity * _charCount];
            _dateTimes = new DateTime[_rowCapacity * _dateTimeCount];
            _decimals = new decimal[_rowCapacity * _decimalCount];
            _doubles = new double[_rowCapacity * _doubleCount];
            _floats = new float[_rowCapacity * _floatCount];
            _guids = new Guid[_rowCapacity * _guidCount];
            _shorts = new short[_rowCapacity * _shortCount];
            _ints = new int[_rowCapacity * _intCount];
            _longs = new long[_rowCapacity * _longCount];
            _objects = new object[_rowCapacity * _objectCount];

            _nullOrdinalToIndexMap = Enumerable.Repeat(-1, fieldCount).ToArray();
            for (var i = 0; i < nullableColumns.Length; i++)
            {
                if (nullableColumns[i])
                {
                    _nullOrdinalToIndexMap[i] = _nullCount;
                    _nullCount++;
                }
            }
            _tempNulls = new bool[_rowCapacity * _nullCount];
        }

        private void DoubleBufferCapacity()
        {
            _rowCapacity <<= 1;

            var newBools = new bool[_tempBools.Length << 1];
            Array.Copy(_tempBools, newBools, _tempBools.Length);
            _tempBools = newBools;

            var newBytes = new byte[_bytes.Length << 1];
            Array.Copy(_bytes, newBytes, _bytes.Length);
            _bytes = newBytes;

            var newChars = new char[_chars.Length << 1];
            Array.Copy(_chars, newChars, _chars.Length);
            _chars = newChars;

            var newDateTimes = new DateTime[_dateTimes.Length << 1];
            Array.Copy(_dateTimes, newDateTimes, _dateTimes.Length);
            _dateTimes = newDateTimes;

            var newDecimals = new decimal[_decimals.Length << 1];
            Array.Copy(_decimals, newDecimals, _decimals.Length);
            _decimals = newDecimals;

            var newDoubles = new double[_doubles.Length << 1];
            Array.Copy(_doubles, newDoubles, _doubles.Length);
            _doubles = newDoubles;

            var newFloats = new float[_floats.Length << 1];
            Array.Copy(_floats, newFloats, _floats.Length);
            _floats = newFloats;

            var newGuids = new Guid[_guids.Length << 1];
            Array.Copy(_guids, newGuids, _guids.Length);
            _guids = newGuids;

            var newShorts = new short[_shorts.Length << 1];
            Array.Copy(_shorts, newShorts, _shorts.Length);
            _shorts = newShorts;

            var newInts = new int[_ints.Length << 1];
            Array.Copy(_ints, newInts, _ints.Length);
            _ints = newInts;

            var newLongs = new long[_longs.Length << 1];
            Array.Copy(_longs, newLongs, _longs.Length);
            _longs = newLongs;

            var newObjects = new object[_objects.Length << 1];
            Array.Copy(_objects, newObjects, _objects.Length);
            _objects = newObjects;

            var newNulls = new bool[_tempNulls.Length << 1];
            Array.Copy(_tempNulls, newNulls, _tempNulls.Length);
            _tempNulls = newNulls;
        }
        
        private void ReadBool(DbDataReader reader, int ordinal)
        {
            _tempBools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]] = reader.GetBoolean(ordinal);
        }

#if !NET40

        private async Task ReadBoolAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _tempBools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<bool>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadByte(DbDataReader reader, int ordinal)
        {
            _bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]] = reader.GetByte(ordinal);
        }

#if !NET40

        private async Task ReadByteAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<byte>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadChar(DbDataReader reader, int ordinal)
        {
            _chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]] = reader.GetChar(ordinal);
        }

#if !NET40

        private async Task ReadCharAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<char>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadDateTime(DbDataReader reader, int ordinal)
        {
            _dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]] = reader.GetDateTime(ordinal);
        }

#if !NET40

        private async Task ReadDateTimeAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<DateTime>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadDecimal(DbDataReader reader, int ordinal)
        {
            _decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]] = reader.GetDecimal(ordinal);
        }

#if !NET40

        private async Task ReadDecimalAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<decimal>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadDouble(DbDataReader reader, int ordinal)
        {
            _doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]] = reader.GetDouble(ordinal);
        }

#if !NET40

        private async Task ReadDoubleAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<double>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadFloat(DbDataReader reader, int ordinal)
        {
            _floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]] = reader.GetFloat(ordinal);
        }

#if !NET40

        private async Task ReadFloatAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<float>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadGuid(DbDataReader reader, int ordinal)
        {
            _guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]] = reader.GetGuid(ordinal);
        }

#if !NET40

        private async Task ReadGuidAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<Guid>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadShort(DbDataReader reader, int ordinal)
        {
            _shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]] = reader.GetInt16(ordinal);
        }

#if !NET40

        private async Task ReadShortAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<short>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadInt(DbDataReader reader, int ordinal)
        {
            _ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]] = reader.GetInt32(ordinal);
        }

#if !NET40

        private async Task ReadIntAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<int>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadLong(DbDataReader reader, int ordinal)
        {
            _longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]] = reader.GetInt64(ordinal);
        }

#if !NET40

        private async Task ReadLongAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<long>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadObject(DbDataReader reader, int ordinal)
        {
            _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] = reader.GetValue(ordinal);
        }

#if !NET40

        private async Task ReadObjectAsync(
            DbDataReader reader, int ordinal, CancellationToken cancellationToken)
        {
            _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] =
                await reader.GetFieldValueAsync<object>(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadGeography(DbSpatialDataReader spatialReader, int ordinal)
        {
            _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] = spatialReader.GetGeography(ordinal);
        }

#if !NET40

        private async Task ReadGeographyAsync(
            DbSpatialDataReader spatialReader, int ordinal, CancellationToken cancellationToken)
        {
            _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] =
                await spatialReader.GetGeographyAsync(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif

        private void ReadGeometry(DbSpatialDataReader spatialReader, int ordinal)
        {
            _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] = spatialReader.GetGeometry(ordinal);
        }

#if !NET40

        private async Task ReadGeometryAsync(
            DbSpatialDataReader spatialReader, int ordinal, CancellationToken cancellationToken)
        {
            _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] =
                await spatialReader.GetGeometryAsync(ordinal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

#endif
        
        public override bool GetBoolean(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Bool)
            {
                return _bools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<bool>(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Byte)
            {
                return _bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<byte>(ordinal);
        }

        public override char GetChar(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Char)
            {
                return _chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<char>(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.DateTime)
            {
                return _dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<DateTime>(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Decimal)
            {
                return _decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<decimal>(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Double)
            {
                return _doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<double>(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Float)
            {
                return _floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<float>(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Guid)
            {
                return _guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<Guid>(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Short)
            {
                return _shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<short>(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Int)
            {
                return _ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<int>(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Long)
            {
                return _longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<long>(ordinal);
        }

        public override string GetString(int ordinal)
        {
            if (_columnTypeCases[ordinal] == TypeCase.Object)
            {
                return (string)_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]];
            }
            return GetFieldValue<string>(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return GetFieldValue<object>(ordinal);
        }

        public override int GetValues(object[] values)
        {
            throw new NotSupportedException();
        }

        public override T GetFieldValue<T>(int ordinal)
        {
            switch (_columnTypeCases[ordinal])
            {
                case TypeCase.Bool:
                    return (T)(object)GetBoolean(ordinal);
                case TypeCase.Byte:
                    return (T)(object)GetByte(ordinal);
                case TypeCase.Char:
                    return (T)(object)GetChar(ordinal);
                case TypeCase.DateTime:
                    return (T)(object)GetDateTime(ordinal);
                case TypeCase.Decimal:
                    return (T)(object)GetDecimal(ordinal);
                case TypeCase.Double:
                    return (T)(object)GetDouble(ordinal);
                case TypeCase.Float:
                    return (T)(object)GetFloat(ordinal);
                case TypeCase.Guid:
                    return (T)(object)GetGuid(ordinal);
                case TypeCase.Short:
                    return (T)(object)GetInt16(ordinal);
                case TypeCase.Int:
                    return (T)(object)GetInt32(ordinal);
                case TypeCase.Long:
                    return (T)(object)GetInt64(ordinal);
                case TypeCase.Empty:
                    return default(T);
                default:
                    return (T)_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]];
            }
        }
        
#if !NET40

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return Task.FromResult(GetFieldValue<T>(ordinal));
        }

#endif

        public override bool IsDBNull(int ordinal)
        {
            return _nulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[ordinal]];
        }
        
#if !NET40

        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return Task.FromResult(IsDBNull(ordinal));
        }

#endif

        public override bool Read()
        {
            return IsDataReady = ++_currentRowNumber < _rowCount;
        }
        
#if !NET40

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Read());
        }

#endif

        private enum TypeCase
        {
            Empty,
            Object,
            Bool,
            Byte,
            Char,
            DateTime,
            Decimal,
            Double,
            Float,
            Guid,
            Short,
            Int,
            Long,
            DbGeography,
            DbGeometry
        }
    }
}
