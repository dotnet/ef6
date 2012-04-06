namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Entity SQL Parser result information.
    /// </summary>
    public sealed class ParseResult
    {
        private readonly DbCommandTree _commandTree;
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<FunctionDefinition> _functionDefs;

        internal ParseResult(DbCommandTree commandTree, List<FunctionDefinition> functionDefs)
        {
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");
            EntityUtil.CheckArgumentNull(functionDefs, "functionDefs");

            this._commandTree = commandTree;
            this._functionDefs = functionDefs.AsReadOnly();
        }

        /// <summary>
        /// A command tree produced during parsing.
        /// </summary>
        public DbCommandTree CommandTree { get { return _commandTree; } }

        /// <summary>
        /// List of <see cref="FunctionDefinition"/> objects describing query inline function definitions.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<FunctionDefinition> FunctionDefinitions { get { return this._functionDefs; } }
    }

    /// <summary>
    /// Entity SQL query inline function definition, returned as a part of <see cref="ParseResult"/>.
    /// </summary>
    public sealed class FunctionDefinition
    {
        private readonly string _name;
        private readonly DbLambda _lambda;
        private readonly int _startPosition;
        private readonly int _endPosition;

        internal FunctionDefinition(string name, DbLambda lambda, int startPosition, int endPosition)
        {
            Debug.Assert(name != null, "name can not be null");
            Debug.Assert(lambda != null, "lambda cannot be null");

            this._name = name;
            this._lambda = lambda;
            this._startPosition = startPosition;
            this._endPosition = endPosition;
        }

        /// <summary>
        /// Function name.
        /// </summary>
        public string Name { get { return this._name; } }

        /// <summary>
        /// Function body and parameters.
        /// </summary>
        public DbLambda Lambda { get { return this._lambda; } }

        /// <summary>
        /// Start position of the function definition in the eSQL query text.
        /// </summary>
        public int StartPosition { get { return this._startPosition; } }

        /// <summary>
        /// End position of the function definition in the eSQL query text.
        /// </summary>
        public int EndPosition { get { return this._endPosition; } }
    }
}