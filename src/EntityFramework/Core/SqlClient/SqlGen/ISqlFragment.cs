namespace System.Data.Entity.Core.SqlClient.SqlGen
{
    using System.Data.Entity.Core.Common.CommandTrees;

    /// <summary>
    /// Represents the sql fragment for any node in the query tree.
    /// </summary>
    /// <remarks>
    /// The nodes in a query tree produce various kinds of sql
    /// <list type="bullet">
    /// <item>A select statement.</item>
    /// <item>A reference to an extent. (symbol)</item>
    /// <item>A raw string.</item>
    /// </list>
    /// We have this interface to allow for a common return type for the methods
    /// in the expression visitor <see cref="DbExpressionVisitor{TResultType}"/>
    /// 
    /// Add the endd of translation, the sql fragments are converted into real strings.
    /// </remarks>
    internal interface ISqlFragment
    {
        /// <summary>
        /// Write the string represented by this fragment into the stream.
        /// </summary>
        /// <param name="writer">The stream that collects the strings.</param>
        /// <param name="sqlGenerator">Context information used for renaming.
        /// The global lists are used to generated new names without collisions.</param>
        void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator);
    }
}
