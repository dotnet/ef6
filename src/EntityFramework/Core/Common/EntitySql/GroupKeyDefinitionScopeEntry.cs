// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Common.CommandTrees;

    // <summary>
    // Represents group key during GROUP BY clause processing phase, used during group aggregate search mode.
    // This entry will be replaced by the <see cref="SourceScopeEntry" /> when GROUP BY processing is complete.
    // </summary>
    internal sealed class GroupKeyDefinitionScopeEntry : ScopeEntry, IGroupExpressionExtendedInfo, IGetAlternativeName
    {
        private readonly DbExpression _varBasedExpression;
        private readonly DbExpression _groupVarBasedExpression;
        private readonly DbExpression _groupAggBasedExpression;
        private readonly string[] _alternativeName;

        internal GroupKeyDefinitionScopeEntry(
            DbExpression varBasedExpression,
            DbExpression groupVarBasedExpression, DbExpression
                groupAggBasedExpression,
            string[] alternativeName)
            : base(ScopeEntryKind.GroupKeyDefinition)
        {
            _varBasedExpression = varBasedExpression;
            _groupVarBasedExpression = groupVarBasedExpression;
            _groupAggBasedExpression = groupAggBasedExpression;
            _alternativeName = alternativeName;
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            return _varBasedExpression;
        }

        DbExpression IGroupExpressionExtendedInfo.GroupVarBasedExpression
        {
            get { return _groupVarBasedExpression; }
        }

        DbExpression IGroupExpressionExtendedInfo.GroupAggBasedExpression
        {
            get { return _groupAggBasedExpression; }
        }

        string[] IGetAlternativeName.AlternativeName
        {
            get { return _alternativeName; }
        }
    }
}
