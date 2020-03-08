// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    // <summary>
    // A VarVec is a compressed representation of a set of variables - with no duplicates
    // and no ordering
    // A VarVec should be used in many places where we expect a number of vars to be
    // passed around; and we don't care particularly about the ordering of the vars
    // This is obviously not suitable for representing sort keys, but is still
    // reasonable for representing group by keys, and a variety of others.
    // </summary>
    internal class VarVec : IEnumerable<Var>
    {
        #region Nested Classes

        // <summary>
        // A VarVec enumerator is a specialized enumerator for a VarVec.
        // </summary>
        internal class VarVecEnumerator : IEnumerator<Var>, IDisposable
        {
            #region private state

            private int m_position;
            private Command m_command;
            private BitVec m_bitArray;

            #endregion

            #region Constructors

            // <summary>
            // Constructs a new enumerator for the specified Vec
            // </summary>
            internal VarVecEnumerator(VarVec vec)
            {
                Init(vec);
            }

            #endregion

            #region public surface

            // <summary>
            // Initialize the enumerator to enumerate over the supplied Vec
            // </summary>
            internal void Init(VarVec vec)
            {
                m_position = -1;
                m_command = vec.m_command;
                m_bitArray = vec.m_bitVector;
            }

            #endregion

            #region IEnumerator<Var> Members

            // <summary>
            // Get the Var at the current position
            // </summary>
            public Var Current
            {
                get { return (m_position >= 0 && m_position < m_bitArray.Length) ? m_command.GetVar(m_position) : null; }
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return Current; }
            }

            static readonly int[] MultiplyDeBruijnBitPosition =
            {
                0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
                31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
            };

            // <summary>
            // Move to the next position
            // </summary>
            public bool MoveNext()
            {
                int[] values = m_bitArray.m_array;
                m_position++;
                int length = m_bitArray.Length;
                int valuesLen = BitVec.GetArrayLength(length, 32);
                int i = m_position / 32;
                int v = 0, mask = 0;

                if (i < valuesLen)
                {

                    v = values[i];
                    // zero lowest bits that are skipped
                    mask = (~0 << (m_position % 32));

                    v &= mask;

                    if (v != 0)
                    {
                        m_position = (i * 32) + MultiplyDeBruijnBitPosition[((uint)((v & -v) * 0x077CB531U)) >> 27];
                        return true;
                    }

                    i++;
                    for (; i < valuesLen; i++)
                    {
                        v = values[i];

                        if (v == 0)
                        {
                            continue;
                        }

                        m_position = (i * 32) + MultiplyDeBruijnBitPosition[((uint)((v & -v) * 0x077CB531U)) >> 27];
                        return true;
                    }
                }
                m_position = length;
                return false;
            }

            // <summary>
            // Reset enumerator to start off again
            // </summary>
            public void Reset()
            {
                m_position = -1;
            }

            #endregion

            #region IDisposable Members

            // <summary>
            // Dispose of the current enumerator - return it to the Command
            // </summary>
            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                m_bitArray = null;
                m_command.ReleaseVarVecEnumerator(this);
            }

            #endregion
        }

        #endregion

        #region public methods

        internal void Clear()
        {
            m_bitVector.Length = 0;
        }

        internal void And(VarVec other)
        {
            Align(other);
            m_bitVector.And(other.m_bitVector);
        }

        internal void Or(VarVec other)
        {
            Align(other);
            m_bitVector.Or(other.m_bitVector);
        }

        // <summary>
        // Computes (this Minus other) by performing (this And (Not(other)))
        // A temp VarVec is used and released at the end of the operation
        // </summary>
        internal void Minus(VarVec other)
        {
            var tmp = m_command.CreateVarVec(other);
            tmp.m_bitVector.Length = m_bitVector.Length;
            tmp.m_bitVector.Not();
            And(tmp);
            m_command.ReleaseVarVec(tmp);
        }

        // <summary>
        // Does this have a non-zero overlap with the other vec
        // </summary>
        internal bool Overlaps(VarVec other)
        {
            var otherCopy = m_command.CreateVarVec(other);
            otherCopy.And(this);
            var overlaps = !otherCopy.IsEmpty;
            m_command.ReleaseVarVec(otherCopy);
            return overlaps;
        }

        // <summary>
        // Does this Vec include every var in the other vec?
        // Written this way deliberately under the assumption that "other"
        // is a relatively small vec
        // </summary>
        internal bool Subsumes(VarVec other)
        {
            int[] values = m_bitVector.m_array;
            int[] otherValues = other.m_bitVector.m_array;

            // if the other is longer, and it has a bit set past the current vector's length return false
            if (otherValues.Length > values.Length)
            {
                for (var i = values.Length; i < otherValues.Length; i++)
                {
                    if (otherValues[i] != 0)
                    {
                        return false;
                    }
                }
            }

            int length = Math.Min(otherValues.Length, values.Length);

            for (var i = 0; i < length; i++)
            {
                if (!((values[i] & otherValues[i]) == otherValues[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal void InitFrom(VarVec other)
        {
            Clear();
            m_bitVector.Length = other.m_bitVector.Length;
            m_bitVector.Or(other.m_bitVector);
        }

        internal void InitFrom(IEnumerable<Var> other)
        {
            InitFrom(other, false);
        }

        internal void InitFrom(IEnumerable<Var> other, bool ignoreParameters)
        {
            Clear();
            foreach (var v in other)
            {
                if (!ignoreParameters
                    || (v.VarType != VarType.Parameter))
                {
                    Set(v);
                }
            }
        }

        // <summary>
        // The enumerator pattern
        // </summary>
        public IEnumerator<Var> GetEnumerator()
        {
            return m_command.GetVarVecEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // <summary>
        // Number of vars in this set
        // </summary>
        [Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "v", Justification = "Allows count of objects from within this object.")]
        internal int Count
        {
            get
            {
                var count = 0;
                foreach (var v in this)
                {
                    count++;
                }
                return count;
            }
        }

        internal bool IsSet(Var v)
        {
            Align(v.Id);
            return m_bitVector.Get(v.Id);
        }

        internal void Set(Var v)
        {
            Align(v.Id);
            m_bitVector.Set(v.Id, true);
        }

        internal void Clear(Var v)
        {
            Align(v.Id);
            m_bitVector.Set(v.Id, false);
        }

        // <summary>
        // Is this Vec empty?
        // </summary>
        internal bool IsEmpty
        {
            get { return First == null; }
        }

        // <summary>
        // Get me the first var that is set
        // </summary>
        internal Var First
        {
            get
            {
                foreach (var v in this)
                {
                    return v;
                }
                return null;
            }
        }

        // <summary>
        // Walk through the input varVec, replace any vars that have been "renamed" based
        // on the input varMap, and return the new VarVec
        // </summary>
        // <param name="varMap"> dictionary of renamed vars </param>
        // <returns> a new VarVec </returns>
        internal VarVec Remap(IDictionary<Var, Var> varMap)
        {
            var newVec = m_command.CreateVarVec();
            foreach (var v in this)
            {
                Var newVar;
                if (!varMap.TryGetValue(v, out newVar))
                {
                    newVar = v;
                }
                newVec.Set(newVar);
            }
            return newVec;
        }

        #endregion

        #region constructors

        internal VarVec(Command command)
        {
            m_bitVector = new BitVec(64);
            m_command = command;
        }

        #endregion

        #region private methods

        private void Align(VarVec other)
        {
            if (other.m_bitVector.Length == m_bitVector.Length)
            {
                return;
            }
            if (other.m_bitVector.Length > m_bitVector.Length)
            {
                m_bitVector.Length = other.m_bitVector.Length;
            }
            else
            {
                other.m_bitVector.Length = m_bitVector.Length;
            }
        }

        private void Align(int idx)
        {
            if (idx >= m_bitVector.Length)
            {
                m_bitVector.Length = idx + 1;
            }
        }

        // <summary>
        // Debugging support
        // provide a string representation for debugging.
        // </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var separator = String.Empty;

            foreach (var v in this)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, v.Id);
                separator = ",";
            }
            return sb.ToString();
        }

        #endregion

        #region private state

        private readonly BitVec m_bitVector;
        private readonly Command m_command;

        #endregion

        #region Clone

        // <summary>
        // Create a clone of this vec
        // </summary>
        public VarVec Clone()
        {
            var newVec = m_command.CreateVarVec();
            newVec.InitFrom(this);
            return newVec;
        }

        #endregion
    }

    internal class BitVec
    {
        private BitVec()
        {
        }

        /*=========================================================================
        ** Allocates space to hold length bit values. All of the values in the bit
        ** array are set to false.
        **
        ** Exceptions: ArgumentException if length < 0.
        =========================================================================*/
        public BitVec(int length)
            : this(length, false)
        {
        }

        /*=========================================================================
        ** Allocates space to hold length bit values. All of the values in the bit
        ** array are set to defaultValue.
        **
        ** Exceptions: ArgumentOutOfRangeException if length < 0.
        =========================================================================*/
        public BitVec(int length, bool defaultValue)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "ArgumentOutOfRange_NeedNonNegNum");
            }

            m_array = ArrayPool.Instance.GetArray(GetArrayLength(length, BitsPerInt32));
            m_length = length;

            int fillValue = defaultValue ? unchecked(((int)0xffffffff)) : 0;
            for (int i = 0; i < m_array.Length; i++)
            {
                m_array[i] = fillValue;
            }

            _version = 0;
        }

        /*=========================================================================
        ** Allocates space to hold the bit values in bytes. bytes[0] represents
        ** bits 0 - 7, bytes[1] represents bits 8 - 15, etc. The LSB of each byte
        ** represents the lowest index value; bytes[0] & 1 represents bit 0,
        ** bytes[0] & 2 represents bit 1, bytes[0] & 4 represents bit 2, etc.
        **
        ** Exceptions: ArgumentException if bytes == null.
        =========================================================================*/
        public BitVec(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            // this value is chosen to prevent overflow when computing m_length.
            // m_length is of type int32 and is exposed as a property, so 
            // type of m_length can't be changed to accommodate.
            if (bytes.Length > Int32.MaxValue / BitsPerByte)
            {
                throw new ArgumentException("Argument_ArrayTooLarge", "bytes");
            }

            m_array = ArrayPool.Instance.GetArray(GetArrayLength(bytes.Length, BytesPerInt32));
            m_length = bytes.Length * BitsPerByte;

            int i = 0;
            int j = 0;
            while (bytes.Length - j >= 4)
            {
                m_array[i++] = (bytes[j] & 0xff) |
                              ((bytes[j + 1] & 0xff) << 8) |
                              ((bytes[j + 2] & 0xff) << 16) |
                              ((bytes[j + 3] & 0xff) << 24);
                j += 4;
            }

            switch (bytes.Length - j)
            {
                case 3:
                    m_array[i] = ((bytes[j + 2] & 0xff) << 16);
                    goto case 2;
                // fall through
                case 2:
                    m_array[i] |= ((bytes[j + 1] & 0xff) << 8);
                    goto case 1;
                // fall through
                case 1:
                    m_array[i] |= (bytes[j] & 0xff);
                    break;
            }

            _version = 0;
        }

        public BitVec(bool[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            m_array = ArrayPool.Instance.GetArray(GetArrayLength(values.Length, BitsPerInt32));
            m_length = values.Length;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                    m_array[i / 32] |= (1 << (i % 32));
            }

            _version = 0;

        }

        /*=========================================================================
        ** Allocates space to hold the bit values in values. values[0] represents
        ** bits 0 - 31, values[1] represents bits 32 - 63, etc. The LSB of each
        ** integer represents the lowest index value; values[0] & 1 represents bit
        ** 0, values[0] & 2 represents bit 1, values[0] & 4 represents bit 2, etc.
        **
        ** Exceptions: ArgumentException if values == null.
        =========================================================================*/
        public BitVec(int[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            // this value is chosen to prevent overflow when computing m_length
            if (values.Length > Int32.MaxValue / BitsPerInt32)
            {
                //throw new ArgumentException(Environment.GetResourceString("Argument_ArrayTooLarge", BitsPerInt32), "values");
            }

            m_array = ArrayPool.Instance.GetArray(values.Length);
            m_length = values.Length * BitsPerInt32;

            Array.Copy(values, m_array, values.Length);

            _version = 0;
        }

        /*=========================================================================
        ** Allocates a new BitVec with the same length and bit values as bits.
        **
        ** Exceptions: ArgumentException if bits == null.
        =========================================================================*/
        public BitVec(BitVec bits)
        {
            if (bits == null)
            {
                throw new ArgumentNullException("bits");
            }

            int arrayLength = GetArrayLength(bits.m_length, BitsPerInt32);
            m_array = ArrayPool.Instance.GetArray(arrayLength);
            m_length = bits.m_length;

            Array.Copy(bits.m_array, m_array, arrayLength);

            _version = bits._version;
        }

        public bool this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Set(index, value);
            }
        }

        /*=========================================================================
        ** Returns the bit value at position index.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public bool Get(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            }

            return (m_array[index / 32] & (1 << (index % 32))) != 0;
        }

        /*=========================================================================
        ** Sets the bit value at position index to value.
        **
        ** Exceptions: ArgumentOutOfRangeException if index < 0 or
        **             index >= GetLength().
        =========================================================================*/
        public void Set(int index, bool value)
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException("index", "ArgumentOutOfRange_Index");
            }

            if (value)
            {
                m_array[index / 32] |= (1 << (index % 32));
            }
            else
            {
                m_array[index / 32] &= ~(1 << (index % 32));
            }

            _version++;
        }

        /*=========================================================================
        ** Sets all the bit values to value.
        =========================================================================*/
        public void SetAll(bool value)
        {
            int fillValue = value ? unchecked(((int)0xffffffff)) : 0;
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] = fillValue;
            }

            _version++;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ANDed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitVec And(BitVec value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");

            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] &= value.m_array[i];
            }

            _version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance ORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitVec Or(BitVec value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");

            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] |= value.m_array[i];
            }

            _version++;
            return this;
        }

        /*=========================================================================
        ** Returns a reference to the current instance XORed with value.
        **
        ** Exceptions: ArgumentException if value == null or
        **             value.Length != this.Length.
        =========================================================================*/
        public BitVec Xor(BitVec value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (Length != value.Length)
                throw new ArgumentException("Arg_ArrayLengthsDiffer");

            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] ^= value.m_array[i];
            }

            _version++;
            return this;
        }

        /*=========================================================================
        ** Inverts all the bit values. On/true bit values are converted to
        ** off/false. Off/false bit values are turned on/true. The current instance
        ** is updated and returned.
        =========================================================================*/
        public BitVec Not()
        {
            int ints = GetArrayLength(m_length, BitsPerInt32);
            for (int i = 0; i < ints; i++)
            {
                m_array[i] = ~m_array[i];
            }

            _version++;
            return this;
        }

        public int Length
        {
            get
            {
                return m_length;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NeedNonNegNum");
                }

                int newints = GetArraySize(value, BitsPerInt32);
                if (newints > m_array.Length || newints + _ShrinkThreshold < m_array.Length)
                {
                    // grow or shrink (if wasting more than _ShrinkThreshold ints)
                    int[] newarray = ArrayPool.Instance.GetArray(newints); //new int[newints];
                    Array.Copy(m_array, newarray, newints > m_array.Length ? m_array.Length : newints);
                    ArrayPool.Instance.PutArray(m_array);
                    m_array = newarray;
                }

                if (value > m_length)
                {
                    // clear high bit values in the last int
                    int last = GetArrayLength(m_length, BitsPerInt32) - 1;
                    int bits = m_length % 32;
                    if (bits > 0)
                    {
                        m_array[last] &= (1 << bits) - 1;
                    }

                    // clear remaining int values
                    Array.Clear(m_array, last + 1, newints - last - 1);
                }

                m_length = value;
                _version++;
            }
        }

        // XPerY=n means that n Xs can be stored in 1 Y. 
        private const int BitsPerInt32 = 32;
        private const int BytesPerInt32 = 4;
        private const int BitsPerByte = 8;

        /// <summary>
        /// Used for conversion between different representations of bit array. 
        /// Returns (n+(div-1))/div, rearranged to avoid arithmetic overflow. 
        /// For example, in the bit to int case, the straightforward calc would 
        /// be (n+31)/32, but that would cause overflow. So instead it's 
        /// rearranged to ((n-1)/32) + 1, with special casing for 0.
        /// 
        /// Usage:
        /// GetArrayLength(77, BitsPerInt32): returns how many ints must be 
        /// allocated to store 77 bits.
        /// </summary>
        /// <param name="n">length of array</param>
        /// <param name="div">use a conversion constant, e.g. BytesPerInt32 to get
        /// how many ints are required to store n bytes</param>
        /// <returns>length of the array</returns>
        public static int GetArrayLength(int n, int div)
        {
            return n > 0 ? (((n - 1) / div) + 1) : 0;
        }

        private static int GetArraySize(int n, int div)
        {
            // compute the next highest power of 2 of 32-bit v
            uint v = Convert.ToUInt32(GetArrayLength(n, div));
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return Convert.ToInt32(v);
        }

        public int[] m_array;
        private int m_length;
        private int _version;
        private const int _ShrinkThreshold = 1024; //256;

        private class ArrayPool
        {
            private ConcurrentDictionary<int, ConcurrentBag<int[]>> dictionary;

            private ArrayPool()
            {
                dictionary = new ConcurrentDictionary<int, ConcurrentBag<int[]>>();
            }

            private static readonly ArrayPool instance = new ArrayPool();

            public static ArrayPool Instance
            {
                get
                {
                    return instance;
                }
            }

            public int[] GetArray(int length)
            {
                ConcurrentBag<int[]> arrays = GetBag(length);

                int[] arr;
                if (arrays.TryTake(out arr)) return arr;

                return new int[length];
            }

            private ConcurrentBag<int[]> GetBag(int length)
            {
				return dictionary.GetOrAdd(length, l => new ConcurrentBag<int[]>());
            }

            public void PutArray(int[] arr)
            {
                ConcurrentBag<int[]> arrays = GetBag(arr.Length);
                Array.Clear(arr, 0, arr.Length);
                arrays.Add(arr);
            }
        }
    }
}
