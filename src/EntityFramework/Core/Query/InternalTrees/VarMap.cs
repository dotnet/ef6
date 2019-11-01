// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    // <summary>
    // Helps map one variable to the next.
    // </summary>
    internal class VarMap : IDictionary<Var, Var>
    {
        #region public surfaces

        private Dictionary<Var, Var> map;
        private Dictionary<Var, Var> reverseMap;

        internal VarMap GetReverseMap()
        {
            return new VarMap(reverseMap, map);
        }

        public bool ContainsValue(Var value)
        {
            return reverseMap.ContainsKey(value);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var separator = string.Empty;

            foreach (var v in map.Keys)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", separator, v.Id, this[v].Id);
                separator = ",";
            }
            return sb.ToString();
        }

        #endregion

        #region IDictionary

        public Var this[Var key]
        {
            get
            {
                return map[key];
            }
            set
            {
                map[key] = value;
            }
        }

        public ICollection<Var> Keys
        {
            get
            {
                return map.Keys;
            }
        }

        public ICollection<Var> Values
        {
            get
            {
                return map.Values;
            }
        }

        public int Count
        {
            get
            {
                return map.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(Var key, Var value)
        {
            if (!reverseMap.ContainsKey(value))
            {
                reverseMap.Add(value, key);
            }
            map.Add(key, value);
        }

        public void Add(KeyValuePair<Var, Var> item)
        {
            if (!reverseMap.ContainsKey(item.Value))
            {
                ((IDictionary<Var, Var>)reverseMap).Add(new KeyValuePair<Var, Var>(item.Value, item.Key));
            }
            ((IDictionary<Var, Var>)map).Add(item);
        }

        public void Clear()
        {
            map.Clear();
            reverseMap.Clear();
        }

        public bool Contains(KeyValuePair<Var, Var> item)
        {
            return ((IDictionary<Var, Var>)map).Contains(item);
        }

        public bool ContainsKey(Var key)
        {
            return map.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<Var, Var>[] array, int arrayIndex)
        {
            ((IDictionary<Var, Var>)map).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<Var, Var>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        public bool Remove(Var key)
        {
            reverseMap.Remove(map[key]);
            return map.Remove(key);
        }

        public bool Remove(KeyValuePair<Var, Var> item)
        {
            reverseMap.Remove(map[item.Value]);
            return ((IDictionary<Var, Var>)map).Remove(item);
        }

        public bool TryGetValue(Var key, out Var value)
        {
            return ((IDictionary<Var, Var>)map).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return map.GetEnumerator();
        }

        #endregion

        #region constructors

        public VarMap()
        {
            map = new Dictionary<Var, Var>();
            reverseMap = new Dictionary<Var, Var>();
        }

        private VarMap(Dictionary<Var, Var> map, Dictionary<Var, Var> reverseMap)
        {
            this.map = map;
            this.reverseMap = reverseMap;
        }

        #endregion
    }
}
