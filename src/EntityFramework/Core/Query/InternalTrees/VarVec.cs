// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    ///     A VarVec is a compressed representation of a set of variables - with no duplicates
    ///     and no ordering
    ///     A VarVec should be used in many places where we expect a number of vars to be
    ///     passed around; and we don't care particularly about the ordering of the vars
    ///     This is obviously not suitable for representing sort keys, but is still
    ///     reasonable for representing group by keys, and a variety of others.
    /// </summary>
    internal class VarVec : IEnumerable<Var>
    {
        #region Nested Classes

        /// <summary>
        ///     A VarVec enumerator is a specialized enumerator for a VarVec.
        /// </summary>
        internal class VarVecEnumerator : IEnumerator<Var>, IDisposable
        {
            #region private state

            private int m_position;
            private Command m_command;
            private BitArray m_bitArray;

            #endregion

            #region Constructors

            /// <summary>
            ///     Constructs a new enumerator for the specified Vec
            /// </summary>
            internal VarVecEnumerator(VarVec vec)
            {
                Init(vec);
            }

            #endregion

            #region public surface

            /// <summary>
            ///     Initialize the enumerator to enumerate over the supplied Vec
            /// </summary>
            internal void Init(VarVec vec)
            {
                m_position = -1;
                m_command = vec.m_command;
                m_bitArray = vec.m_bitVector;
            }

            #endregion

            #region IEnumerator<Var> Members

            /// <summary>
            ///     Get the Var at the current position
            /// </summary>
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

            /// <summary>
            ///     Move to the next position
            /// </summary>
            public bool MoveNext()
            {
                m_position++;
                for (; m_position < m_bitArray.Length; m_position++)
                {
                    if (m_bitArray[m_position])
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            ///     Reset enumerator to start off again
            /// </summary>
            public void Reset()
            {
                m_position = -1;
            }

            #endregion

            #region IDisposable Members

            /// <summary>
            ///     Dispose of the current enumerator - return it to the Command
            /// </summary>
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

        /// <summary>
        ///     Computes (this Minus other) by performing (this And (Not(other)))
        ///     A temp VarVec is used and released at the end of the operation
        /// </summary>
        internal void Minus(VarVec other)
        {
            var tmp = m_command.CreateVarVec(other);
            tmp.m_bitVector.Length = m_bitVector.Length;
            tmp.m_bitVector.Not();
            And(tmp);
            m_command.ReleaseVarVec(tmp);
        }

        /// <summary>
        ///     Does this have a non-zero overlap with the other vec
        /// </summary>
        internal bool Overlaps(VarVec other)
        {
            var otherCopy = m_command.CreateVarVec(other);
            otherCopy.And(this);
            var overlaps = !otherCopy.IsEmpty;
            m_command.ReleaseVarVec(otherCopy);
            return overlaps;
        }

        /// <summary>
        ///     Does this Vec include every var in the other vec?
        ///     Written this way deliberately under the assumption that "other"
        ///     is a relatively small vec
        /// </summary>
        internal bool Subsumes(VarVec other)
        {
            for (var i = 0; i < other.m_bitVector.Length; i++)
            {
                if (other.m_bitVector[i]
                    && ((i >= m_bitVector.Length) || !m_bitVector[i]))
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

        /// <summary>
        ///     The enumerator pattern
        /// </summary>
        public IEnumerator<Var> GetEnumerator()
        {
            return m_command.GetVarVecEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Number of vars in this set
        /// </summary>
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

        /// <summary>
        ///     Is this Vec empty?
        /// </summary>
        internal bool IsEmpty
        {
            get { return First == null; }
        }

        /// <summary>
        ///     Get me the first var that is set
        /// </summary>
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

        /// <summary>
        ///     Walk through the input varVec, replace any vars that have been "renamed" based
        ///     on the input varMap, and return the new VarVec
        /// </summary>
        /// <param name="varMap"> dictionary of renamed vars </param>
        /// <returns> a new VarVec </returns>
        internal VarVec Remap(Dictionary<Var, Var> varMap)
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
            m_bitVector = new BitArray(64);
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

        /// <summary>
        ///     Debugging support
        ///     provide a string representation for debugging.
        /// </summary>
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

        private readonly BitArray m_bitVector;
        private readonly Command m_command;

        #endregion

        #region Clone

        /// <summary>
        ///     Create a clone of this vec
        /// </summary>
        public VarVec Clone()
        {
            var newVec = m_command.CreateVarVec();
            newVec.InitFrom(this);
            return newVec;
        }

        #endregion
    }
}
