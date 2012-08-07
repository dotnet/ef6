// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Implements the semantic resolver in the context of a metadata workspace and typespace.
    /// </summary>
    /// <remarks>
    ///     not thread safe
    /// </remarks>
    internal sealed class SemanticResolver
    {
        #region Fields

        private readonly ParserOptions _parserOptions;
        private readonly Dictionary<string, DbParameterReferenceExpression> _parameters;
        private readonly Dictionary<string, DbVariableReferenceExpression> _variables;
        private readonly TypeResolver _typeResolver;
        private readonly ScopeManager _scopeManager;
        private readonly List<ScopeRegion> _scopeRegions = new List<ScopeRegion>();
        private bool _ignoreEntityContainerNameResolution;
        private GroupAggregateInfo _currentGroupAggregateInfo;
        private uint _namegenCounter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Creates new instance of <see cref="SemanticResolver" />.
        /// </summary>
        internal static SemanticResolver Create(
            Perspective perspective,
            ParserOptions parserOptions,
            IEnumerable<DbParameterReferenceExpression> parameters,
            IEnumerable<DbVariableReferenceExpression> variables)
        {
            Contract.Requires(perspective != null);
            Contract.Requires(parserOptions != null);

            return new SemanticResolver(
                parserOptions,
                ProcessParameters(parameters, parserOptions),
                ProcessVariables(variables, parserOptions),
                new TypeResolver(perspective, parserOptions));
        }

        /// <summary>
        ///     Creates a copy of <see cref="SemanticResolver" /> with clean scopes and shared inline function definitions inside of the type resolver.
        /// </summary>
        internal SemanticResolver CloneForInlineFunctionConversion()
        {
            return new SemanticResolver(
                _parserOptions,
                _parameters,
                _variables,
                _typeResolver);
        }

        private SemanticResolver(
            ParserOptions parserOptions,
            Dictionary<string, DbParameterReferenceExpression> parameters,
            Dictionary<string, DbVariableReferenceExpression> variables,
            TypeResolver typeResolver)
        {
            _parserOptions = parserOptions;
            _parameters = parameters;
            _variables = variables;
            _typeResolver = typeResolver;

            //
            // Creates Scope manager
            //
            _scopeManager = new ScopeManager(NameComparer);

            //
            // Push a root scope region
            //
            EnterScopeRegion();

            //
            // Add command free variables to the root scope
            //
            foreach (var variable in _variables.Values)
            {
                CurrentScope.Add(variable.VariableName, new FreeVariableScopeEntry(variable));
            }
        }

        /// <summary>
        ///     Validates that the specified parameters have valid, non-duplicated names
        /// </summary>
        /// <param name="paramDefs"> The set of query parameters </param>
        /// <returns> A valid dictionary that maps parameter names to <see cref="DbParameterReferenceExpression" /> s using the current NameComparer </returns>
        private static Dictionary<string, DbParameterReferenceExpression> ProcessParameters(
            IEnumerable<DbParameterReferenceExpression> paramDefs, ParserOptions parserOptions)
        {
            var retParams = new Dictionary<string, DbParameterReferenceExpression>(parserOptions.NameComparer);

            if (paramDefs != null)
            {
                foreach (var paramDef in paramDefs)
                {
                    if (retParams.ContainsKey(paramDef.ParameterName))
                    {
                        var message = Strings.MultipleDefinitionsOfParameter(paramDef.ParameterName);
                        throw new EntitySqlException(message);
                    }

                    Debug.Assert(paramDef.ResultType.IsReadOnly, "paramDef.ResultType.IsReadOnly must be set");

                    retParams.Add(paramDef.ParameterName, paramDef);
                }
            }

            return retParams;
        }

        /// <summary>
        ///     Validates that the specified variables have valid, non-duplicated names
        /// </summary>
        /// <param name="varDefs"> The set of free variables </param>
        /// <returns> A valid dictionary that maps variable names to <see cref="DbVariableReferenceExpression" /> s using the current NameComparer </returns>
        private static Dictionary<string, DbVariableReferenceExpression> ProcessVariables(
            IEnumerable<DbVariableReferenceExpression> varDefs, ParserOptions parserOptions)
        {
            var retVars = new Dictionary<string, DbVariableReferenceExpression>(parserOptions.NameComparer);

            if (varDefs != null)
            {
                foreach (var varDef in varDefs)
                {
                    if (retVars.ContainsKey(varDef.VariableName))
                    {
                        var message = Strings.MultipleDefinitionsOfVariable(varDef.VariableName);
                        throw new EntitySqlException(message);
                    }

                    Debug.Assert(varDef.ResultType.IsReadOnly, "varDef.ResultType.IsReadOnly must be set");

                    retVars.Add(varDef.VariableName, varDef);
                }
            }

            return retVars;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Returns ordinary command parameters. Empty dictionary in case of no parameters.
        /// </summary>
        internal Dictionary<string, DbParameterReferenceExpression> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        ///     Returns command free variables. Empty dictionary in case of no variables.
        /// </summary>
        internal Dictionary<string, DbVariableReferenceExpression> Variables
        {
            get { return _variables; }
        }

        /// <summary>
        ///     TypeSpace/Metadata/Perspective dependent type resolver.
        /// </summary>
        internal TypeResolver TypeResolver
        {
            get { return _typeResolver; }
        }

        /// <summary>
        ///     Returns current Parser Options.
        /// </summary>
        internal ParserOptions ParserOptions
        {
            get { return _parserOptions; }
        }

        /// <summary>
        ///     Returns the current string comparer.
        /// </summary>
        internal StringComparer NameComparer
        {
            get { return _parserOptions.NameComparer; }
        }

        /// <summary>
        ///     Returns the list of scope regions: outer followed by inner.
        /// </summary>
        internal IEnumerable<ScopeRegion> ScopeRegions
        {
            get { return _scopeRegions; }
        }

        /// <summary>
        ///     Returns the current scope region.
        /// </summary>
        internal ScopeRegion CurrentScopeRegion
        {
            get { return _scopeRegions[_scopeRegions.Count - 1]; }
        }

        /// <summary>
        ///     Returns the current scope.
        /// </summary>
        internal Scope CurrentScope
        {
            get { return _scopeManager.CurrentScope; }
        }

        /// <summary>
        ///     Returns index of the current scope.
        /// </summary>
        internal int CurrentScopeIndex
        {
            get { return _scopeManager.CurrentScopeIndex; }
        }

        /// <summary>
        ///     Returns the current group aggregate info when processing group aggregate argument.
        /// </summary>
        internal GroupAggregateInfo CurrentGroupAggregateInfo
        {
            get { return _currentGroupAggregateInfo; }
        }

        #endregion

        #region GetExpressionFromScopeEntry

        /// <summary>
        ///     Returns the appropriate expression from a given scope entry.
        ///     May return null for scope entries like <see cref="InvalidGroupInputRefScopeEntry" />.
        /// </summary>
        private DbExpression GetExpressionFromScopeEntry(ScopeEntry scopeEntry, int scopeIndex, string varName, ErrorContext errCtx)
        {
            //
            // If
            //      1) we are in the context of a group aggregate or group key, 
            //      2) and the scopeEntry can have multiple interpretations depending on the aggregation context,
            //      3) and the defining scope region of the scopeEntry is outer or equal to the defining scope region of the group aggregate,
            //      4) and the defining scope region of the scopeEntry is not performing conversion of a group key definition,
            // Then the expression that corresponds to the scopeEntry is either the GroupVarBasedExpression or the GroupAggBasedExpression.
            // Otherwise the default expression that corresponds to the scopeEntry is provided by scopeEntry.GetExpression(...) call.
            //
            // Explanation for #2 from the list above:
            // A scope entry may have multiple aggregation-context interpretations:
            //      - An expression in the context of a group key definition, obtained by scopeEntry.GetExpression(...);
            //        Example: select k1 from {0} as a group by a%2 as k1
            //                                                  ^^^
            //      - An expression in the context of a function aggregate, provided by iGroupExpressionExtendedInfo.GroupVarBasedExpression;
            //        Example: select max( a ) from {0} as a group by a%2 as k1
            //                            ^^^
            //      - An expression in the context of a group partition, provided by iGroupExpressionExtendedInfo.GroupAggBasedExpression;
            //        Example: select GroupPartition( a ) from {0} as a group by a%2 as k1
            //                                       ^^^
            // Note that expressions obtained from aggregation-context-dependent scope entries outside of the three contexts mentioned above
            // will default to the value returned by the scopeEntry.GetExpression(...) call. This value is the same as in the group key definition context.
            // These expressions have correct result types which enables partial expression validation. 
            // However the contents of the expressions are invalid outside of the group key definitions, hence they can not appear in the final expression tree.
            // SemanticAnalyzer.ProcessGroupByClause(...) method guarantees that such expressions are only temporarily used during GROUP BY clause processing and
            // dropped afterwards.
            // Example: select a, k1 from {0} as a group by a%2 as k1
            //                 ^^^^^ - these expressions are processed twice: once during GROUP BY and then SELECT clause processing,
            //                         the expressions obtained during GROUP BY clause processing are dropped and only
            //                         the ones obtained during SELECT clause processing are accepted.
            //
            // Explanation for #3 from the list above:
            //      - An outer scope entry referenced inside of an aggregate may lift the aggregate to the outer scope region for evaluation, 
            //        hence such a scope entry must be interpreted in the aggregation context. See explanation for #4 below for more info.
            //        Example: 
            //
            //          select 
            //              (select max(x) from {1} as y) 
            //          from {0} as x
            //
            //      - If a scope entry is defined inside of a group aggregate, then the scope entry is not affected by the aggregate, 
            //        hence such a scope entry is not interpreted in the aggregation context.
            //        Example:
            //
            //          select max(
            //                       anyelement( select b from {1} as b )  
            //                    )
            //          from {0} as a group by a %2 as a1
            //
            //        In this query the aggregate argument contains a nested query expression.
            //        The nested query references b. Because b is defined inside of the aggregate it is not interpreted in the aggregation context and
            //        the expression for b should not be GroupVar/GroupAgg based, even though the reference to b appears inside of an aggregate.
            //
            // Explanation for #4 from the list above:
            // An aggregate evaluating on a particular scope region defines the interpretation of scope entries defined on that scope region.
            // In the case when an inner aggregate references a scope entry belonging to the evaluating region of an outer aggregate, the interpretation
            // of the scope entry is controlled by the outer aggregate, otherwise it is controlled by the inner aggregate.
            // Example:
            //
            //      select a1
            //      from {0} as a group by 
            //                                anyelement(select value max(a + b) from {1} as b)
            //                          as a1
            //
            // In this query the aggregate inside of a1 group key definition, the max(a + b), references scope entry a.
            // Because a is referenced inside of the group key definition (which serves as an outer aggregate) and the key definition belongs to
            // the same scope region as a, a is interpreted in the context of the group key definition, not the function aggregate and
            // the expression for a is obtained by scopeEntry.GetExpression(...) call, not iGroupExpressionExtendedInfo.GroupVarBasedExpression.
            //

            var expr = scopeEntry.GetExpression(varName, errCtx);
            Debug.Assert(expr != null, "scopeEntry.GetExpression(...) returned null");

            if (_currentGroupAggregateInfo != null)
            {
                //
                // Make sure defining scope regions agree as described above.
                // Outer scope region has smaller index value than the inner.
                //
                var definingScopeRegionOfScopeEntry = GetDefiningScopeRegion(scopeIndex);
                if (definingScopeRegionOfScopeEntry.ScopeRegionIndex
                    <= _currentGroupAggregateInfo.DefiningScopeRegion.ScopeRegionIndex)
                {
                    //
                    // Let the group aggregate know the scope of the scope entry it references.
                    // This affects the scope region that will evaluate the group aggregate.
                    //
                    _currentGroupAggregateInfo.UpdateScopeIndex(scopeIndex, this);

                    var iGroupExpressionExtendedInfo = scopeEntry as IGroupExpressionExtendedInfo;
                    if (iGroupExpressionExtendedInfo != null)
                    {
                        //
                        // Find the aggregate that controls interpretation of the current scope entry.
                        // This would be a containing aggregate with the defining scope region matching definingScopeRegionOfScopeEntry.
                        // If there is no such aggregate, then the current containing aggregate controls interpretation.
                        //
                        GroupAggregateInfo expressionInterpretationContext;
                        for (expressionInterpretationContext = _currentGroupAggregateInfo;
                             expressionInterpretationContext != null &&
                             expressionInterpretationContext.DefiningScopeRegion.ScopeRegionIndex
                             >= definingScopeRegionOfScopeEntry.ScopeRegionIndex;
                             expressionInterpretationContext = expressionInterpretationContext.ContainingAggregate)
                        {
                            if (expressionInterpretationContext.DefiningScopeRegion.ScopeRegionIndex
                                == definingScopeRegionOfScopeEntry.ScopeRegionIndex)
                            {
                                break;
                            }
                        }
                        if (expressionInterpretationContext == null
                            ||
                            expressionInterpretationContext.DefiningScopeRegion.ScopeRegionIndex
                            < definingScopeRegionOfScopeEntry.ScopeRegionIndex)
                        {
                            expressionInterpretationContext = _currentGroupAggregateInfo;
                        }

                        switch (expressionInterpretationContext.AggregateKind)
                        {
                            case GroupAggregateKind.Function:
                                if (iGroupExpressionExtendedInfo.GroupVarBasedExpression != null)
                                {
                                    expr = iGroupExpressionExtendedInfo.GroupVarBasedExpression;
                                }
                                break;

                            case GroupAggregateKind.Partition:
                                if (iGroupExpressionExtendedInfo.GroupAggBasedExpression != null)
                                {
                                    expr = iGroupExpressionExtendedInfo.GroupAggBasedExpression;
                                }
                                break;

                            case GroupAggregateKind.GroupKey:
                                //
                                // User the current expression obtained from scopeEntry.GetExpression(...)
                                //
                                break;

                            default:
                                Debug.Fail("Unexpected group aggregate kind.");
                                break;
                        }
                    }
                }
            }

            return expr;
        }

        #endregion

        #region Name resolution

        #region Resolve simple / metadata member name

        internal IDisposable EnterIgnoreEntityContainerNameResolution()
        {
            Debug.Assert(!_ignoreEntityContainerNameResolution, "EnterIgnoreEntityContainerNameResolution() is not reentrant.");
            _ignoreEntityContainerNameResolution = true;
            return new Disposer(
                delegate
                    {
                        Debug.Assert(_ignoreEntityContainerNameResolution, "_ignoreEntityContainerNameResolution must be true.");
                        _ignoreEntityContainerNameResolution = false;
                    });
        }

        internal ExpressionResolution ResolveSimpleName(string name, bool leftHandSideOfMemberAccess, ErrorContext errCtx)
        {
            Debug.Assert(!String.IsNullOrEmpty(name), "name must not be null or empty");

            //
            // Try resolving as a scope entry.
            //
            ScopeEntry scopeEntry;
            int scopeIndex;
            if (TryScopeLookup(name, out scopeEntry, out scopeIndex))
            {
                //
                // Check for invalid join left expression correlation.
                //
                if (scopeEntry.EntryKind == ScopeEntryKind.SourceVar
                    && ((SourceScopeEntry)scopeEntry).IsJoinClauseLeftExpr)
                {
                    var message = Strings.InvalidJoinLeftCorrelation;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // Set correlation flag.
                //
                SetScopeRegionCorrelationFlag(scopeIndex);

                return new ValueExpression(GetExpressionFromScopeEntry(scopeEntry, scopeIndex, name, errCtx));
            }

            // 
            // Try resolving as a member of the default entity container.
            //
            var defaultEntityContainer = TypeResolver.Perspective.GetDefaultContainer();
            ExpressionResolution defaultEntityContainerResolution;
            if (defaultEntityContainer != null
                && TryResolveEntityContainerMemberAccess(defaultEntityContainer, name, out defaultEntityContainerResolution))
            {
                return defaultEntityContainerResolution;
            }

            if (!_ignoreEntityContainerNameResolution)
            {
                // 
                // Try resolving as an entity container.
                //
                EntityContainer entityContainer;
                if (TypeResolver.Perspective.TryGetEntityContainer(
                    name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out entityContainer))
                {
                    return new EntityContainerExpression(entityContainer);
                }
            }

            //
            // Otherwise, resolve as an unqualified name. 
            //
            return TypeResolver.ResolveUnqualifiedName(name, leftHandSideOfMemberAccess /* partOfQualifiedName */, errCtx);
        }

        internal MetadataMember ResolveSimpleFunctionName(string name, ErrorContext errCtx)
        {
            //
            // "Foo()" represents a simple function name. Resolve it as an unqualified name by calling the type resolver directly.
            // Note that calling type resolver directly will avoid resolution of the identifier as a local variable or entity container
            // (these resolutions are performed only by ResolveSimpleName(...)).
            //
            var resolution = TypeResolver.ResolveUnqualifiedName(name, false /* partOfQualifiedName */, errCtx);
            if (resolution.MetadataMemberClass
                == MetadataMemberClass.Namespace)
            {
                // 
                // Try resolving as a function import inside the default entity container.
                //
                var defaultEntityContainer = TypeResolver.Perspective.GetDefaultContainer();
                ExpressionResolution defaultEntityContainerResolution;
                if (defaultEntityContainer != null &&
                    TryResolveEntityContainerMemberAccess(defaultEntityContainer, name, out defaultEntityContainerResolution)
                    &&
                    defaultEntityContainerResolution.ExpressionClass == ExpressionResolutionClass.MetadataMember)
                {
                    resolution = (MetadataMember)defaultEntityContainerResolution;
                }
            }
            return resolution;
        }

        /// <summary>
        ///     Performs scope lookup returning the scope entry and its index.
        /// </summary>
        private bool TryScopeLookup(string key, out ScopeEntry scopeEntry, out int scopeIndex)
        {
            scopeEntry = null;
            scopeIndex = -1;

            for (var i = CurrentScopeIndex; i >= 0; i--)
            {
                if (_scopeManager.GetScopeByIndex(i).TryLookup(key, out scopeEntry))
                {
                    scopeIndex = i;
                    return true;
                }
            }

            return false;
        }

        internal MetadataMember ResolveMetadataMemberName(string[] name, ErrorContext errCtx)
        {
            return TypeResolver.ResolveMetadataMemberName(name, errCtx);
        }

        #endregion

        #region Resolve member name in member access

        #region Resolve property access

        /// <summary>
        ///     Resolve property <paramref name="name" /> off the <paramref name="valueExpr" />.
        /// </summary>
        internal ValueExpression ResolvePropertyAccess(DbExpression valueExpr, string name, ErrorContext errCtx)
        {
            DbExpression propertyExpr;

            if (TryResolveAsPropertyAccess(valueExpr, name, out propertyExpr))
            {
                return new ValueExpression(propertyExpr);
            }

            if (TryResolveAsRefPropertyAccess(valueExpr, name, errCtx, out propertyExpr))
            {
                return new ValueExpression(propertyExpr);
            }

            if (TypeSemantics.IsCollectionType(valueExpr.ResultType))
            {
                var message = Strings.NotAMemberOfCollection(name, valueExpr.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }
            else
            {
                var message = Strings.NotAMemberOfType(name, valueExpr.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        /// <summary>
        ///     Try resolving <paramref name="name" /> as a property of the value returned by the <paramref name="valueExpr" />.
        /// </summary>
        private bool TryResolveAsPropertyAccess(DbExpression valueExpr, string name, out DbExpression propertyExpr)
        {
            Debug.Assert(valueExpr != null, "valueExpr != null");

            propertyExpr = null;

            if (Helper.IsStructuralType(valueExpr.ResultType.EdmType))
            {
                EdmMember member;
                if (TypeResolver.Perspective.TryGetMember(
                    (StructuralType)valueExpr.ResultType.EdmType, name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/,
                    out member))
                {
                    Debug.Assert(member != null, "member != null");
                    Debug.Assert(NameComparer.Equals(name, member.Name), "this.NameComparer.Equals(name, member.Name)");
                    propertyExpr = DbExpressionBuilder.CreatePropertyExpressionFromMember(valueExpr, member);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     If <paramref name="valueExpr" /> returns a reference, then deref and try resolving <paramref name="name" /> as a property of the dereferenced value.
        /// </summary>
        private bool TryResolveAsRefPropertyAccess(DbExpression valueExpr, string name, ErrorContext errCtx, out DbExpression propertyExpr)
        {
            Debug.Assert(valueExpr != null, "valueExpr != null");

            propertyExpr = null;

            if (TypeSemantics.IsReferenceType(valueExpr.ResultType))
            {
                DbExpression derefExpr = valueExpr.Deref();
                var derefExprType = derefExpr.ResultType;

                if (TryResolveAsPropertyAccess(derefExpr, name, out propertyExpr))
                {
                    return true;
                }
                else
                {
                    var message = Strings.InvalidDeRefProperty(name, derefExprType.EdmType.FullName, valueExpr.ResultType.EdmType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }

            return false;
        }

        #endregion

        #region Resolve entity container member access

        /// <summary>
        ///     Resolve entity set or function import <paramref name="name" /> in the <paramref name="entityContainer" />
        /// </summary>
        internal ExpressionResolution ResolveEntityContainerMemberAccess(EntityContainer entityContainer, string name, ErrorContext errCtx)
        {
            ExpressionResolution resolution;
            if (TryResolveEntityContainerMemberAccess(entityContainer, name, out resolution))
            {
                return resolution;
            }
            else
            {
                var message = Strings.MemberDoesNotBelongToEntityContainer(name, entityContainer.Name);
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        private bool TryResolveEntityContainerMemberAccess(
            EntityContainer entityContainer, string name, out ExpressionResolution resolution)
        {
            EntitySetBase entitySetBase;
            EdmFunction functionImport;
            if (TypeResolver.Perspective.TryGetExtent(
                entityContainer, name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out entitySetBase))
            {
                resolution = new ValueExpression(entitySetBase.Scan());
                return true;
            }
            else if (TypeResolver.Perspective.TryGetFunctionImport(
                entityContainer, name, _parserOptions.NameComparisonCaseInsensitive /*ignoreCase*/, out functionImport))
            {
                resolution = new MetadataFunctionGroup(functionImport.FullName, new[] { functionImport });
                return true;
            }
            else
            {
                resolution = null;
                return false;
            }
        }

        #endregion

        #region Resolve metadata member access

        /// <summary>
        ///     Resolve namespace, type or function <paramref name="name" /> in the <paramref name="metadataMember" />
        /// </summary>
        internal MetadataMember ResolveMetadataMemberAccess(MetadataMember metadataMember, string name, ErrorContext errCtx)
        {
            return TypeResolver.ResolveMetadataMemberAccess(metadataMember, name, errCtx);
        }

        #endregion

        #endregion

        #region Resolve internal aggregate name / alternative group key name

        /// <summary>
        ///     Try resolving an internal aggregate name.
        /// </summary>
        internal bool TryResolveInternalAggregateName(string name, ErrorContext errCtx, out DbExpression dbExpression)
        {
            ScopeEntry scopeEntry;
            int scopeIndex;
            if (TryScopeLookup(name, out scopeEntry, out scopeIndex))
            {
                //
                // Set the correlation flag.
                //
                SetScopeRegionCorrelationFlag(scopeIndex);

                dbExpression = scopeEntry.GetExpression(name, errCtx);
                return true;
            }
            else
            {
                dbExpression = null;
                return false;
            }
        }

        /// <summary>
        ///     Try resolving multipart identifier as an alternative name of a group key (see SemanticAnalyzer.ProcessGroupByClause(...) for more info).
        /// </summary>
        internal bool TryResolveDotExprAsGroupKeyAlternativeName(DotExpr dotExpr, out ValueExpression groupKeyResolution)
        {
            groupKeyResolution = null;

            string[] names;
            ScopeEntry scopeEntry;
            int scopeIndex;
            if (IsInAnyGroupScope() &&
                dotExpr.IsMultipartIdentifier(out names)
                &&
                TryScopeLookup(TypeResolver.GetFullName(names), out scopeEntry, out scopeIndex))
            {
                var iGetAlternativeName = scopeEntry as IGetAlternativeName;

                //
                // Accept only if names[] match alternative name part by part.
                //
                if (iGetAlternativeName != null && iGetAlternativeName.AlternativeName != null
                    &&
                    names.SequenceEqual(iGetAlternativeName.AlternativeName, NameComparer))
                {
                    //
                    // Set correlation flag
                    //
                    SetScopeRegionCorrelationFlag(scopeIndex);

                    groupKeyResolution =
                        new ValueExpression(
                            GetExpressionFromScopeEntry(scopeEntry, scopeIndex, TypeResolver.GetFullName(names), dotExpr.ErrCtx));
                    return true;
                }
            }
            return false;
        }

        #endregion

        #endregion

        #region Name generation utils (GenerateInternalName, CreateNewAlias, InferAliasName)

        /// <summary>
        ///     Generates unique internal name.
        /// </summary>
        internal string GenerateInternalName(string hint)
        {
            // string concat is much faster than String.Format
            return "_##" + hint + unchecked(_namegenCounter++).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Creates a new alias name based on the <paramref name="expr" /> information.
        /// </summary>
        private string CreateNewAlias(DbExpression expr)
        {
            var extent = expr as DbScanExpression;
            if (null != extent)
            {
                return extent.Target.Name;
            }

            var property = expr as DbPropertyExpression;
            if (null != property)
            {
                return property.Property.Name;
            }

            var varRef = expr as DbVariableReferenceExpression;
            if (null != varRef)
            {
                return varRef.VariableName;
            }

            return GenerateInternalName(String.Empty);
        }

        /// <summary>
        ///     Returns alias name from <paramref name="aliasedExpr" /> ast node if it contains an alias,
        ///     otherwise creates a new alias name based on the <paramref name="aliasedExpr" />.Expr or <paramref
        ///      name="convertedExpression" /> information.
        /// </summary>
        internal string InferAliasName(AliasedExpr aliasedExpr, DbExpression convertedExpression)
        {
            if (aliasedExpr.Alias != null)
            {
                return aliasedExpr.Alias.Name;
            }

            var id = aliasedExpr.Expr as Identifier;
            if (null != id)
            {
                return id.Name;
            }

            var dotExpr = aliasedExpr.Expr as DotExpr;
            string[] names;
            if (null != dotExpr
                && dotExpr.IsMultipartIdentifier(out names))
            {
                return names[names.Length - 1];
            }

            return CreateNewAlias(convertedExpression);
        }

        #endregion

        #region Scope/ScopeRegion utils

        /// <summary>
        ///     Enters a new scope region.
        /// </summary>
        internal IDisposable EnterScopeRegion()
        {
            //
            // Push new scope (the first scope in the new scope region)
            //
            _scopeManager.EnterScope();

            //
            // Create new scope region and push it
            //
            var scopeRegion = new ScopeRegion(_scopeManager, CurrentScopeIndex, _scopeRegions.Count);
            _scopeRegions.Add(scopeRegion);

            //
            // Return scope region disposer that rolls back the scope.
            //
            return new Disposer(
                delegate
                    {
                        Debug.Assert(CurrentScopeRegion == scopeRegion, "Scope region stack is corrupted.");

                        //
                        // Root scope region is permanent.
                        //
                        Debug.Assert(_scopeRegions.Count > 1, "_scopeRegionFlags.Count > 1");

                        //
                        // Reset aggregate info of AST nodes of aggregates resolved to the CurrentScopeRegion.
                        //
                        CurrentScopeRegion.GroupAggregateInfos.ForEach(groupAggregateInfo => groupAggregateInfo.DetachFromAstNode());

                        //
                        // Rollback scopes of the region.
                        //
                        CurrentScopeRegion.RollbackAllScopes();

                        //
                        // Remove the scope region.
                        //
                        _scopeRegions.Remove(CurrentScopeRegion);
                    });
        }

        /// <summary>
        ///     Rollback all scopes above the <paramref name="scopeIndex" />.
        /// </summary>
        internal void RollbackToScope(int scopeIndex)
        {
            _scopeManager.RollbackToScope(scopeIndex);
        }

        /// <summary>
        ///     Enter a new scope.
        /// </summary>
        internal void EnterScope()
        {
            _scopeManager.EnterScope();
        }

        /// <summary>
        ///     Leave the current scope.
        /// </summary>
        internal void LeaveScope()
        {
            _scopeManager.LeaveScope();
        }

        /// <summary>
        ///     Returns true if any of the ScopeRegions from the closest to the outermost has IsAggregating = true
        /// </summary>
        internal bool IsInAnyGroupScope()
        {
            for (var i = 0; i < _scopeRegions.Count; i++)
            {
                if (_scopeRegions[i].IsAggregating)
                {
                    return true;
                }
            }
            return false;
        }

        internal ScopeRegion GetDefiningScopeRegion(int scopeIndex)
        {
            //
            // Starting from the innermost, find the outermost scope region that contains the scope.
            //
            for (var i = _scopeRegions.Count - 1; i >= 0; --i)
            {
                if (_scopeRegions[i].ContainsScope(scopeIndex))
                {
                    return _scopeRegions[i];
                }
            }
            Debug.Fail("Failed to find the defining scope region for the given scope.");
            return null;
        }

        /// <summary>
        ///     Sets the scope region correlation flag based on the scope index of the referenced scope entry.
        /// </summary>
        private void SetScopeRegionCorrelationFlag(int scopeIndex)
        {
            GetDefiningScopeRegion(scopeIndex).WasResolutionCorrelated = true;
        }

        #endregion

        #region Group aggregate utils

        /// <summary>
        ///     Enters processing of a function group aggregate.
        /// </summary>
        internal IDisposable EnterFunctionAggregate(MethodExpr methodExpr, ErrorContext errCtx, out FunctionAggregateInfo aggregateInfo)
        {
            aggregateInfo = new FunctionAggregateInfo(methodExpr, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
            return EnterGroupAggregate(aggregateInfo);
        }

        /// <summary>
        ///     Enters processing of a group partition aggregate.
        /// </summary>
        internal IDisposable EnterGroupPartition(
            GroupPartitionExpr groupPartitionExpr, ErrorContext errCtx, out GroupPartitionInfo aggregateInfo)
        {
            aggregateInfo = new GroupPartitionInfo(groupPartitionExpr, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
            return EnterGroupAggregate(aggregateInfo);
        }

        /// <summary>
        ///     Enters processing of a group partition aggregate.
        /// </summary>
        internal IDisposable EnterGroupKeyDefinition(
            GroupAggregateKind aggregateKind, ErrorContext errCtx, out GroupKeyAggregateInfo aggregateInfo)
        {
            aggregateInfo = new GroupKeyAggregateInfo(aggregateKind, errCtx, _currentGroupAggregateInfo, CurrentScopeRegion);
            return EnterGroupAggregate(aggregateInfo);
        }

        private IDisposable EnterGroupAggregate(GroupAggregateInfo aggregateInfo)
        {
            _currentGroupAggregateInfo = aggregateInfo;
            return new Disposer(
                delegate
                    {
                        //
                        // First, pop the element from the stack to keep the stack valid...
                        //
                        Debug.Assert(_currentGroupAggregateInfo == aggregateInfo, "Aggregare info stack is corrupted.");
                        _currentGroupAggregateInfo = aggregateInfo.ContainingAggregate;

                        //
                        // ...then validate and seal the aggregate info.
                        // Note that this operation may throw an EntitySqlException.
                        //
                        aggregateInfo.ValidateAndComputeEvaluatingScopeRegion(this);
                    });
        }

        #endregion

        #region Function overload resolution (untyped null aware)

        internal static EdmFunction ResolveFunctionOverloads(
            IList<EdmFunction> functionsMetadata,
            IList<TypeUsage> argTypes,
            bool isGroupAggregateFunction,
            out bool isAmbiguous)
        {
            return FunctionOverloadResolver.ResolveFunctionOverloads(
                functionsMetadata,
                argTypes,
                UntypedNullAwareFlattenArgumentType,
                UntypedNullAwareFlattenParameterType,
                UntypedNullAwareIsPromotableTo,
                UntypedNullAwareIsStructurallyEqual,
                isGroupAggregateFunction,
                out isAmbiguous);
        }

        internal static TFunctionMetadata ResolveFunctionOverloads<TFunctionMetadata, TFunctionParameterMetadata>(
            IList<TFunctionMetadata> functionsMetadata,
            IList<TypeUsage> argTypes,
            Func<TFunctionMetadata, IList<TFunctionParameterMetadata>> getSignatureParams,
            Func<TFunctionParameterMetadata, TypeUsage> getParameterTypeUsage,
            Func<TFunctionParameterMetadata, ParameterMode> getParameterMode,
            bool isGroupAggregateFunction,
            out bool isAmbiguous) where TFunctionMetadata : class
        {
            return FunctionOverloadResolver.ResolveFunctionOverloads(
                functionsMetadata,
                argTypes,
                getSignatureParams,
                getParameterTypeUsage,
                getParameterMode,
                UntypedNullAwareFlattenArgumentType,
                UntypedNullAwareFlattenParameterType,
                UntypedNullAwareIsPromotableTo,
                UntypedNullAwareIsStructurallyEqual,
                isGroupAggregateFunction,
                out isAmbiguous);
        }

        private static IEnumerable<TypeUsage> UntypedNullAwareFlattenArgumentType(TypeUsage argType)
        {
            return argType != null ? TypeSemantics.FlattenType(argType) : new TypeUsage[] { null };
        }

        private static IEnumerable<TypeUsage> UntypedNullAwareFlattenParameterType(TypeUsage paramType, TypeUsage argType)
        {
            return argType != null ? TypeSemantics.FlattenType(paramType) : new[] { paramType };
        }

        private static bool UntypedNullAwareIsPromotableTo(TypeUsage fromType, TypeUsage toType)
        {
            if (fromType == null)
            {
                //
                // We can implicitly promote null to any type except collection.
                //
                return !Helper.IsCollectionType(toType.EdmType);
            }
            else
            {
                return TypeSemantics.IsPromotableTo(fromType, toType);
            }
        }

        private static bool UntypedNullAwareIsStructurallyEqual(TypeUsage fromType, TypeUsage toType)
        {
            if (fromType == null)
            {
                return UntypedNullAwareIsPromotableTo(fromType, toType);
            }
            else
            {
                return TypeSemantics.IsStructurallyEqual(fromType, toType);
            }
        }

        #endregion
    }
}
