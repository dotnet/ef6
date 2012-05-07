namespace System.Data.Entity.Core.Common.EntitySql
{
    internal interface IGetAlternativeName
    {
        /// <summary>
        /// If current scope entry reperesents an alternative group key name (see SemanticAnalyzer.ProcessGroupByClause(...) for more info)
        /// then this property returns the alternative name, otherwise null.
        /// </summary>
        string[] AlternativeName { get; }
    }
}