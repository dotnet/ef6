// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    using System.Diagnostics;
    using System.Globalization;

    internal class TileBinaryOperator<T_Query> : Tile<T_Query>
        where T_Query : ITileQuery
    {
        private readonly Tile<T_Query> m_arg1;
        private readonly Tile<T_Query> m_arg2;

        public TileBinaryOperator(Tile<T_Query> arg1, Tile<T_Query> arg2, TileOpKind opKind, T_Query query)
            : base(opKind, query)
        {
            Debug.Assert(arg1 != null && arg2 != null);
            m_arg1 = arg1;
            m_arg2 = arg2;
        }

        public override Tile<T_Query> Arg1
        {
            get { return m_arg1; }
        }

        public override Tile<T_Query> Arg2
        {
            get { return m_arg2; }
        }

        public override string Description
        {
            get
            {
                string descriptionFormat = null;
                switch (OpKind)
                {
                    case TileOpKind.Join:
                        descriptionFormat = "({0} & {1})";
                        break;
                    case TileOpKind.AntiSemiJoin:
                        descriptionFormat = "({0} - {1})";
                        break;
                    case TileOpKind.Union:
                        descriptionFormat = "({0} | {1})";
                        break;
                    default:
                        Debug.Fail("Unexpected binary operator");
                        break;
                }
                return String.Format(CultureInfo.InvariantCulture, descriptionFormat, Arg1.Description, Arg2.Description);
            }
        }

        internal override Tile<T_Query> Replace(Tile<T_Query> oldTile, Tile<T_Query> newTile)
        {
            var newArg1 = Arg1.Replace(oldTile, newTile);
            var newArg2 = Arg2.Replace(oldTile, newTile);
            if (newArg1 != Arg1
                || newArg2 != Arg2)
            {
                return new TileBinaryOperator<T_Query>(newArg1, newArg2, OpKind, Query);
            }
            return this;
        }
    }
}
