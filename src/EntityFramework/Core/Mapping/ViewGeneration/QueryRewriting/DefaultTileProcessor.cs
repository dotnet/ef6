namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting
{
    internal class DefaultTileProcessor<T_Query> : TileProcessor<Tile<T_Query>>
        where T_Query : ITileQuery
    {
        private readonly TileQueryProcessor<T_Query> _tileQueryProcessor;

        internal DefaultTileProcessor(TileQueryProcessor<T_Query> tileQueryProcessor)
        {
            _tileQueryProcessor = tileQueryProcessor;
        }

        internal TileQueryProcessor<T_Query> QueryProcessor
        {
            get { return _tileQueryProcessor; }
        }

        internal override bool IsEmpty(Tile<T_Query> tile)
        {
            return false == _tileQueryProcessor.IsSatisfiable(tile.Query);
        }

        internal override Tile<T_Query> Union(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return new TileBinaryOperator<T_Query>(arg1, arg2, TileOpKind.Union, _tileQueryProcessor.Union(arg1.Query, arg2.Query));
        }

        internal override Tile<T_Query> Join(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return new TileBinaryOperator<T_Query>(arg1, arg2, TileOpKind.Join, _tileQueryProcessor.Intersect(arg1.Query, arg2.Query));
        }

        internal override Tile<T_Query> AntiSemiJoin(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return new TileBinaryOperator<T_Query>(
                arg1, arg2, TileOpKind.AntiSemiJoin, _tileQueryProcessor.Difference(arg1.Query, arg2.Query));
        }

        internal override Tile<T_Query> GetArg1(Tile<T_Query> tile)
        {
            return tile.Arg1;
        }

        internal override Tile<T_Query> GetArg2(Tile<T_Query> tile)
        {
            return tile.Arg2;
        }

        internal override TileOpKind GetOpKind(Tile<T_Query> tile)
        {
            return tile.OpKind;
        }

        internal bool IsContainedIn(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return IsEmpty(AntiSemiJoin(arg1, arg2));
        }

        internal bool IsEquivalentTo(Tile<T_Query> arg1, Tile<T_Query> arg2)
        {
            return IsContainedIn(arg1, arg2) && IsContainedIn(arg2, arg1);
        }
    }
}
