// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Entity SQL Parser result information.
    /// </summary>
    public sealed class ParseResult
    {
        private readonly DbCommandTree _commandTree;
        private readonly ReadOnlyCollection<FunctionDefinition> _functionDefs;

        internal ParseResult(DbCommandTree commandTree, List<FunctionDefinition> functionDefs)
        {
            DebugCheck.NotNull(commandTree);
            DebugCheck.NotNull(functionDefs);

            _commandTree = commandTree;
            _functionDefs = new ReadOnlyCollection<FunctionDefinition>(functionDefs);
        }

        /// <summary> A command tree produced during parsing. </summary>
        public DbCommandTree CommandTree
        {
            get { return _commandTree; }
        }

        /// <summary>
        /// List of <see cref="T:System.Data.Entity.Core.Common.EntitySql.FunctionDefinition" /> objects describing query inline function definitions.
        /// </summary>
        public ReadOnlyCollection<FunctionDefinition> FunctionDefinitions
        {
            get { return _functionDefs; }
        }
    }
}
