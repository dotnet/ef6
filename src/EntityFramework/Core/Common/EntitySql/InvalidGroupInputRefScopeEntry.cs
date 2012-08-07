// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Resources;

    /// <summary>
    ///     Represents a group input scope entry that should no longer be referenced.
    /// </summary>
    internal sealed class InvalidGroupInputRefScopeEntry : ScopeEntry
    {
        internal InvalidGroupInputRefScopeEntry()
            : base(ScopeEntryKind.InvalidGroupInputRef)
        {
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            var message = Strings.InvalidGroupIdentifierReference(refName);
            throw EntitySqlException.Create(errCtx, message, null);
        }
    }
}
