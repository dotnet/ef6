namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;

    /// <summary>
    /// The KeySet class encapsulates all information about the keys of a RelOp node in
    /// the query tree.
    /// A KeyVec is logically a set of vars that uniquely identify the row of the current
    /// RelOp. Some RelOps may have no unique keys - such a state is identified by the
    /// "NoKeys" property
    /// </summary>
    internal class KeyVec
    {
        #region private state

        private readonly VarVec m_keys;
        private bool m_noKeys;

        #endregion

        #region constructors

        internal KeyVec(Command itree)
        {
            m_keys = itree.CreateVarVec();
            m_noKeys = true;
        }

        #endregion

        internal void InitFrom(KeyVec keyset)
        {
            m_keys.InitFrom(keyset.m_keys);
            m_noKeys = keyset.m_noKeys;
        }

        internal void InitFrom(IEnumerable<Var> varSet)
        {
            InitFrom(varSet, false);
        }

        internal void InitFrom(IEnumerable<Var> varSet, bool ignoreParameters)
        {
            m_keys.InitFrom(varSet, ignoreParameters);
            // Bug 434541: An empty set of keys is not the same as "no" keys.
            // Caveat Emptor
            m_noKeys = false;
        }

        internal void InitFrom(KeyVec left, KeyVec right)
        {
            if (left.m_noKeys
                || right.m_noKeys)
            {
                m_noKeys = true;
            }
            else
            {
                m_noKeys = false;
                m_keys.InitFrom(left.m_keys);
                m_keys.Or(right.m_keys);
            }
        }

        internal void InitFrom(List<KeyVec> keyVecList)
        {
            m_noKeys = false;
            m_keys.Clear();
            foreach (var keyVec in keyVecList)
            {
                if (keyVec.m_noKeys)
                {
                    m_noKeys = true;
                    return;
                }
                m_keys.Or(keyVec.m_keys);
            }
        }

        internal void Clear()
        {
            m_noKeys = true;
            m_keys.Clear();
        }

        internal VarVec KeyVars
        {
            get { return m_keys; }
        }

        internal bool NoKeys
        {
            get { return m_noKeys; }
            set { m_noKeys = value; }
        }
    }
}