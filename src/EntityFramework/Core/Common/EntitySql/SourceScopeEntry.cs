// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Diagnostics;

    /// <summary>
    ///     Represents simple source var scope entry.
    /// </summary>
    internal sealed class SourceScopeEntry : ScopeEntry, IGroupExpressionExtendedInfo, IGetAlternativeName
    {
        private readonly string[] _alternativeName;
        private List<string> _propRefs;
        private DbExpression _varBasedExpression;
        private DbExpression _groupVarBasedExpression;
        private DbExpression _groupAggBasedExpression;

        internal SourceScopeEntry(DbVariableReferenceExpression varRef)
            : this(varRef, null)
        {
        }

        internal SourceScopeEntry(DbVariableReferenceExpression varRef, string[] alternativeName)
            : base(ScopeEntryKind.SourceVar)
        {
            _varBasedExpression = varRef;
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

        internal bool IsJoinClauseLeftExpr { get; set; }

        string[] IGetAlternativeName.AlternativeName
        {
            get { return _alternativeName; }
        }

        /// <summary>
        ///     Prepend <paramref name="parentVarRef" /> to the property chain.
        /// </summary>
        internal SourceScopeEntry AddParentVar(DbVariableReferenceExpression parentVarRef)
        {
            //
            // No parent var adjustment is allowed while adjusted to group var (see AdjustToGroupVar(...) for more info).
            //
            Debug.Assert(_groupVarBasedExpression == null, "_groupVarBasedExpression == null");
            Debug.Assert(_groupAggBasedExpression == null, "_groupAggBasedExpression == null");

            if (_propRefs == null)
            {
                Debug.Assert(_varBasedExpression is DbVariableReferenceExpression, "_varBasedExpression is DbVariableReferenceExpression");
                _propRefs = new List<string>(2);
                _propRefs.Add(((DbVariableReferenceExpression)_varBasedExpression).VariableName);
            }

            _varBasedExpression = parentVarRef;
            for (var i = _propRefs.Count - 1; i >= 0; --i)
            {
                _varBasedExpression = _varBasedExpression.Property(_propRefs[i]);
            }
            _propRefs.Add(parentVarRef.VariableName);

            return this;
        }

        /// <summary>
        ///     Replace existing var at the head of the property chain with the new <paramref name="parentVarRef" />.
        /// </summary>
        internal void ReplaceParentVar(DbVariableReferenceExpression parentVarRef)
        {
            //
            // No parent var adjustment is allowed while adjusted to group var (see AdjustToGroupVar(...) for more info).
            //
            Debug.Assert(_groupVarBasedExpression == null, "_groupVarBasedExpression == null");
            Debug.Assert(_groupAggBasedExpression == null, "_groupAggBasedExpression == null");

            if (_propRefs == null)
            {
                Debug.Assert(_varBasedExpression is DbVariableReferenceExpression, "_varBasedExpression is DbVariableReferenceExpression");
                _varBasedExpression = parentVarRef;
            }
            else
            {
                Debug.Assert(_propRefs.Count > 0, "_propRefs.Count > 0");
                _propRefs.RemoveAt(_propRefs.Count - 1);
                AddParentVar(parentVarRef);
            }
        }

        /// <summary>
        ///     Rebuild the current scope entry expression as the property chain off the <paramref name="parentVarRef" /> expression.
        ///     Also build 
        ///     - <see cref="IGroupExpressionExtendedInfo.GroupVarBasedExpression" /> off the <paramref name="parentGroupVarRef" /> expression;
        ///     - <see cref="IGroupExpressionExtendedInfo.GroupAggBasedExpression" /> off the <paramref name="groupAggRef" /> expression.
        ///     This adjustment is reversable by <see cref="RollbackAdjustmentToGroupVar" />(...).
        /// </summary>
        internal void AdjustToGroupVar(
            DbVariableReferenceExpression parentVarRef, DbVariableReferenceExpression parentGroupVarRef,
            DbVariableReferenceExpression groupAggRef)
        {
            // Adjustment is not reentrant.
            Debug.Assert(_groupVarBasedExpression == null, "_groupVarBasedExpression == null");
            Debug.Assert(_groupAggBasedExpression == null, "_groupAggBasedExpression == null");

            //
            // Let's assume this entry represents variable "x" in the following query:
            //      select x, y, z from {1, 2} as x join {2, 3} as y on x = y join {3, 4} as z on y = z
            // In this case _propRefs contains x._##join0._##join1 and the corresponding input expression looks like this:
            //     |_Input : '_##join1'
            //     | |_InnerJoin
            //     |   |_Left : '_##join0'
            //     |   | |_InnerJoin
            //     |   |   |_Left : 'x'
            //     |   |   |_Right : 'y'
            //     |   |_Right : 'z'
            // When we start processing a group by, like in this query:
            //      select k1, k2, k3 from {1, 2} as x join {2, 3} as y on x = y join {3, 4} as z on y = z group by x as k1, y as k2, z as k3
            // we are switching to the following input expression:
            //     |_Input : '_##geb2', '_##group3'
            //     | |_InnerJoin
            //     |   |_Left : '_##join0'
            //     |   | |_InnerJoin
            //     |   |   |_Left : 'x'
            //     |   |   |_Right : 'y'
            //     |   |_Right : 'z'
            // where _##join1 is replaced by _##geb2 for the regular expression and by _##group3 for the group var based expression.
            // So the switch, or the adjustment, is done by 
            //      a. replacing _##join1 with _##geb2 in _propRefs and rebuilding the regular expression accordingly to get
            //         the following property chain: _##geb2._##join1.x
            //      b. building a group var based expression using _##group3 instead of _##geb2 to get
            //         the following property chain: _##group3._##join1.x
            //

            //
            // Rebuild ScopeEntry.Expression using the new parent var.
            //
            ReplaceParentVar(parentVarRef);

            //
            // Build the GroupVarBasedExpression and GroupAggBasedExpression, 
            // take into account that parentVarRef has already been added to the _propRefs in the AdjustToParentVar(...) call, so ignore it.
            //
            _groupVarBasedExpression = parentGroupVarRef;
            _groupAggBasedExpression = groupAggRef;
            if (_propRefs != null)
            {
                for (var i = _propRefs.Count - 2 /*ignore the parentVarRef*/; i >= 0; --i)
                {
                    _groupVarBasedExpression = _groupVarBasedExpression.Property(_propRefs[i]);
                    _groupAggBasedExpression = _groupAggBasedExpression.Property(_propRefs[i]);
                }
            }
        }

        /// <summary>
        ///     Rolls back the <see cref="AdjustToGroupVar" />(...) adjustment, clears the <see
        ///      cref="IGroupExpressionExtendedInfo.GroupVarBasedExpression" />.
        /// </summary>
        internal void RollbackAdjustmentToGroupVar(DbVariableReferenceExpression pregroupParentVarRef)
        {
            Debug.Assert(_groupVarBasedExpression != null, "_groupVarBasedExpression != null");

            _groupVarBasedExpression = null;
            _groupAggBasedExpression = null;
            ReplaceParentVar(pregroupParentVarRef);
        }
    }
}
