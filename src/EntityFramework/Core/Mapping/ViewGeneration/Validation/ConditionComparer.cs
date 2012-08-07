// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Text;

    internal class ConditionComparer : IEqualityComparer<Dictionary<MemberPath, Set<Constant>>>
    {
        public bool Equals(Dictionary<MemberPath, Set<Constant>> one, Dictionary<MemberPath, Set<Constant>> two)
        {
            var keysOfOne = new Set<MemberPath>(one.Keys, MemberPath.EqualityComparer);
            var keysOfTwo = new Set<MemberPath>(two.Keys, MemberPath.EqualityComparer);

            if (!keysOfOne.SetEquals(keysOfTwo))
            {
                return false;
            }

            foreach (var member in keysOfOne)
            {
                var constantsOfOne = one[member];
                var constantsOfTwo = two[member];

                if (!constantsOfOne.SetEquals(constantsOfTwo))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(Dictionary<MemberPath, Set<Constant>> obj)
        {
            var builder = new StringBuilder();
            foreach (var key in obj.Keys)
            {
                builder.Append(key);
            }

            return builder.ToString().GetHashCode();
        }
    }
}
