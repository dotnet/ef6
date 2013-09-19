// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils
{
    using System.Collections.Generic;
    using System.Text;

    internal class Pair<TFirst, TSecond> : InternalBase
    {
        #region Fields

        private readonly TFirst first;
        private readonly TSecond second;

        #endregion

        #region Constructor

        internal Pair(TFirst first, TSecond second)
        {
            this.first = first;
            this.second = second;
        }

        #endregion

        #region Properties

        internal TFirst First
        {
            get { return first; }
        }

        internal TSecond Second
        {
            get { return second; }
        }

        #endregion

        #region Methods

        public override int GetHashCode()
        {
            return (first.GetHashCode() << 5) ^ second.GetHashCode();
        }

        public bool Equals(Pair<TFirst, TSecond> other)
        {
            return first.Equals(other.first) && second.Equals(other.second);
        }

        public override bool Equals(object other)
        {
            var otherPair = other as Pair<TFirst, TSecond>;

            return (otherPair != null && Equals(otherPair));
        }

        #endregion

        #region InternalBase

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append("<");
            builder.Append(first);
            builder.Append(", " + second);
            builder.Append(">");
        }

        #endregion

        internal class PairComparer : IEqualityComparer<Pair<TFirst, TSecond>>
        {
            private PairComparer()
            {
            }

            internal static readonly PairComparer Instance = new PairComparer();
            private static readonly EqualityComparer<TFirst> _firstComparer = EqualityComparer<TFirst>.Default;
            private static readonly EqualityComparer<TSecond> _secondComparer = EqualityComparer<TSecond>.Default;

            public bool Equals(Pair<TFirst, TSecond> x, Pair<TFirst, TSecond> y)
            {
                return _firstComparer.Equals(x.First, y.First) && _secondComparer.Equals(x.Second, y.Second);
            }

            public int GetHashCode(Pair<TFirst, TSecond> source)
            {
                return source.GetHashCode();
            }
        }
    }
}
