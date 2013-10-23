// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Base.Util
{
    internal abstract class Pair<P, Q>
    {
        private readonly P first;
        private readonly Q second;

        internal Pair(P first, Q second)
        {
            this.first = first;
            this.second = second;
        }

        internal virtual P First
        {
            get { return first; }
        }

        internal virtual Q Second
        {
            get { return second; }
        }

        // Compare pairs based on their contents rather than object equality
        public override bool Equals(object obj)
        {
            if (obj == null
                || GetType() != obj.GetType())
            {
                return false;
            }

            var pair = (Pair<P, Q>)obj;

            if (pair == this)
            {
                return true;
            }

            return (pair.First.Equals(first) && pair.Second.Equals(second));
        }

        public override int GetHashCode()
        {
            return ((first != null) ? first.GetHashCode() : 0) ^ ((second != null) ? second.GetHashCode() : 0);
        }
    }
}
