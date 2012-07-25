// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Represents a projection item definition scope entry.
    /// </summary>
    internal sealed class ProjectionItemDefinitionScopeEntry : ScopeEntry
    {
        private readonly DbExpression _expression;

        internal ProjectionItemDefinitionScopeEntry(DbExpression expression)
            : base(ScopeEntryKind.ProjectionItemDefinition)
        {
            _expression = expression;
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            return _expression;
        }
    }

    /// <summary>
    /// Represents a free variable scope entry. 
    /// Example: parameters of an inline function definition are free variables in the scope of the function definition.
    /// </summary>
    internal sealed class FreeVariableScopeEntry : ScopeEntry
    {
        private readonly DbVariableReferenceExpression _varRef;

        internal FreeVariableScopeEntry(DbVariableReferenceExpression varRef)
            : base(ScopeEntryKind.FreeVar)
        {
            _varRef = varRef;
        }

        internal override DbExpression GetExpression(string refName, ErrorContext errCtx)
        {
            return _varRef;
        }
    }

    /// <summary>
    /// Represents a generic list of scopes.
    /// </summary>
    internal sealed class ScopeManager
    {
        private readonly IEqualityComparer<string> _keyComparer;
        private readonly List<Scope> _scopes = new List<Scope>();

        /// <summary>
        /// Initialize scope manager using given key-string comparer.
        /// </summary>
        internal ScopeManager(IEqualityComparer<string> keyComparer)
        {
            _keyComparer = keyComparer;
        }

        /// <summary>
        /// Enter a new scope.
        /// </summary>
        internal void EnterScope()
        {
            _scopes.Add(new Scope(_keyComparer));
        }

        /// <summary>
        /// Leave the current scope.
        /// </summary>
        internal void LeaveScope()
        {
            Debug.Assert(CurrentScopeIndex >= 0);
            _scopes.RemoveAt(CurrentScopeIndex);
        }

        /// <summary>
        /// Return current scope index.
        /// Outer scopes have smaller index values than inner scopes.
        /// </summary>
        internal int CurrentScopeIndex
        {
            get { return _scopes.Count - 1; }
        }

        /// <summary>
        /// Return current scope.
        /// </summary>
        internal Scope CurrentScope
        {
            get { return _scopes[CurrentScopeIndex]; }
        }

        /// <summary>
        /// Get a scope by the index.
        /// </summary>
        internal Scope GetScopeByIndex(int scopeIndex)
        {
            Debug.Assert(scopeIndex >= 0, "scopeIndex >= 0");
            Debug.Assert(scopeIndex <= CurrentScopeIndex, "scopeIndex <= CurrentScopeIndex");
            if (0 > scopeIndex
                || scopeIndex > CurrentScopeIndex)
            {
                var message = Strings.InvalidScopeIndex;
                throw new EntitySqlException(message);
            }
            return _scopes[scopeIndex];
        }

        /// <summary>
        /// Rollback all scopes to the scope at the index.
        /// </summary>
        internal void RollbackToScope(int scopeIndex)
        {
            //
            // assert preconditions
            //
            Debug.Assert(scopeIndex >= 0, "[PRE] savePoint.ScopeIndex >= 0");
            Debug.Assert(scopeIndex <= CurrentScopeIndex, "[PRE] savePoint.ScopeIndex <= CurrentScopeIndex");
            Debug.Assert(CurrentScopeIndex >= 0, "[PRE] CurrentScopeIndex >= 0");

            if (scopeIndex > CurrentScopeIndex || scopeIndex < 0
                || CurrentScopeIndex < 0)
            {
                var message = Strings.InvalidSavePoint;
                throw new EntitySqlException(message);
            }

            var delta = CurrentScopeIndex - scopeIndex;
            if (delta > 0)
            {
                _scopes.RemoveRange(scopeIndex + 1, CurrentScopeIndex - scopeIndex);
            }

            //
            // make sure invariants are preserved
            //
            Debug.Assert(scopeIndex == CurrentScopeIndex, "[POST] savePoint.ScopeIndex == CurrentScopeIndex");
            Debug.Assert(CurrentScopeIndex >= 0, "[POST] CurrentScopeIndex >= 0");
        }

        /// <summary>
        /// True if key exists in current scope.
        /// </summary>
        internal bool IsInCurrentScope(string key)
        {
            return CurrentScope.Contains(key);
        }
    }
}
