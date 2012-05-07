namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    /// VertexLiteral pair, used for ConversionContext.GetSuccessors
    /// </summary>
    internal sealed class LiteralVertexPair<T_Identifier>
    {
        internal readonly Vertex Vertex;
        internal readonly Literal<T_Identifier> Literal;

        internal LiteralVertexPair(Vertex vertex, Literal<T_Identifier> literal)
        {
            Vertex = vertex;
            Literal = literal;
        }
    }
}