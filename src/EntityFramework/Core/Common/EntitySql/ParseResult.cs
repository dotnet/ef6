namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Entity SQL Parser result information.
    /// </summary>
    public sealed class ParseResult
    {
        private readonly DbCommandTree _commandTree;
        private readonly ReadOnlyCollection<FunctionDefinition> _functionDefs;

        internal ParseResult(DbCommandTree commandTree, List<FunctionDefinition> functionDefs)
        {
            Contract.Requires(commandTree != null);
            Contract.Requires(functionDefs != null);

            _commandTree = commandTree;
            _functionDefs = functionDefs.AsReadOnly();
        }

        /// <summary>
        /// A command tree produced during parsing.
        /// </summary>
        public DbCommandTree CommandTree
        {
            get { return _commandTree; }
        }

        /// <summary>
        /// List of <see cref="FunctionDefinition"/> objects describing query inline function definitions.
        /// </summary>
        public ReadOnlyCollection<FunctionDefinition> FunctionDefinitions
        {
            get { return _functionDefs; }
        }
    }
}
