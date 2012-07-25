// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    using System.Collections.Generic;
    using System.Globalization;

    internal abstract class Tile<T_Query>
        where T_Query : ITileQuery
    {
        private readonly T_Query m_query;
        private readonly TileOpKind m_opKind;

        protected Tile(TileOpKind opKind, T_Query query)
        {
            m_opKind = opKind;
            m_query = query;
        }

        public T_Query Query
        {
            get { return m_query; }
        }

        public abstract string Description { get; }

        // multiple occurrences possible
        public IEnumerable<T_Query> GetNamedQueries()
        {
            return GetNamedQueries(this);
        }

        private static IEnumerable<T_Query> GetNamedQueries(Tile<T_Query> rewriting)
        {
            if (rewriting != null)
            {
                if (rewriting.OpKind
                    == TileOpKind.Named)
                {
                    yield return ((TileNamed<T_Query>)rewriting).NamedQuery;
                }
                else
                {
                    foreach (var query in GetNamedQueries(rewriting.Arg1))
                    {
                        yield return query;
                    }
                    foreach (var query in GetNamedQueries(rewriting.Arg2))
                    {
                        yield return query;
                    }
                }
            }
        }

        public override string ToString()
        {
            var formattedQuery = Description;
            if (formattedQuery != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}: [{1}]", Description, Query);
            }
            else
            {
                return String.Format(CultureInfo.InvariantCulture, "[{0}]", Query);
            }
        }

        public abstract Tile<T_Query> Arg1 { get; }

        public abstract Tile<T_Query> Arg2 { get; }

        public TileOpKind OpKind
        {
            get { return m_opKind; }
        }

        internal abstract Tile<T_Query> Replace(Tile<T_Query> oldTile, Tile<T_Query> newTile);
    }
}
