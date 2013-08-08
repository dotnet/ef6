// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Helps map one variable to the next.
    /// </summary>
    internal class VarMap : Dictionary<Var, Var>
    {
        #region public surfaces

        internal VarMap GetReverseMap()
        {
            var reverseMap = new VarMap();
            foreach (var kv in this)
            {
                Var x;
                // On the odd chance that a var is in the varMap more than once, the first one
                // is going to be the one we want to use, because it might be the discriminator
                // var;
                if (!reverseMap.TryGetValue(kv.Value, out x))
                {
                    reverseMap[kv.Value] = kv.Key;
                }
            }
            return reverseMap;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var separator = string.Empty;

            foreach (var v in Keys)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", separator, v.Id, this[v].Id);
                separator = ",";
            }
            return sb.ToString();
        }

        #endregion

        #region constructors

        #endregion
    }
}
