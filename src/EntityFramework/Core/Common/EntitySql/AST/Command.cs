// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents eSQL command as node.
    /// </summary>
    internal sealed class Command : Node
    {
        private readonly NodeList<NamespaceImport> _namespaceImportList;
        private readonly Statement _statement;

        /// <summary>
        /// Initializes eSQL command.
        /// </summary>
        /// <param name="nsImportList"> optional namespace imports </param>
        /// <param name="statement"> command statement </param>
        internal Command(NodeList<NamespaceImport> nsImportList, Statement statement)
        {
            _namespaceImportList = nsImportList;
            _statement = statement;
        }

        /// <summary>
        /// Returns optional namespace imports. May be null.
        /// </summary>
        internal NodeList<NamespaceImport> NamespaceImportList
        {
            get { return _namespaceImportList; }
        }

        /// <summary>
        /// Returns command statement.
        /// </summary>
        internal Statement Statement
        {
            get { return _statement; }
        }
    }
}
