// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    // <summary>
    // Represents group aggregate information during aggregate construction/resolution.
    // </summary>
    internal abstract class GroupAggregateInfo
    {
        protected GroupAggregateInfo(
            GroupAggregateKind aggregateKind,
            GroupAggregateExpr astNode,
            ErrorContext errCtx,
            GroupAggregateInfo containingAggregate,
            ScopeRegion definingScopeRegion)
        {
            Debug.Assert(aggregateKind != GroupAggregateKind.None, "aggregateKind != GroupAggregateKind.None");
            DebugCheck.NotNull(errCtx);
            DebugCheck.NotNull(definingScopeRegion);

            AggregateKind = aggregateKind;
            AstNode = astNode;
            ErrCtx = errCtx;
            DefiningScopeRegion = definingScopeRegion;
            SetContainingAggregate(containingAggregate);
        }

        protected void AttachToAstNode(string aggregateName, TypeUsage resultType)
        {
            DebugCheck.NotNull(aggregateName);
            DebugCheck.NotNull(resultType);
            Debug.Assert(AstNode != null, "AstNode must be set.");
            Debug.Assert(AggregateName == null && AggregateStubExpression == null, "Cannot reattach.");

            AggregateName = aggregateName;
            AggregateStubExpression = resultType.Null();

            // Attach group aggregate info to the ast node.
            AstNode.AggregateInfo = this;
        }

        internal void DetachFromAstNode()
        {
            Debug.Assert(AstNode != null, "AstNode must be set.");
            AstNode.AggregateInfo = null;
        }

        // <summary>
        // Updates referenced scope index of the aggregate.
        // Function call is not allowed after <see cref="ValidateAndComputeEvaluatingScopeRegion" /> has been called.
        // </summary>
        internal void UpdateScopeIndex(int referencedScopeIndex, SemanticResolver sr)
        {
            Debug.Assert(
                _evaluatingScopeRegion == null, "Can not update referenced scope index after _evaluatingScopeRegion have been computed.");

            var referencedScopeRegion = sr.GetDefiningScopeRegion(referencedScopeIndex);

            if (_innermostReferencedScopeRegion == null
                ||
                _innermostReferencedScopeRegion.ScopeRegionIndex < referencedScopeRegion.ScopeRegionIndex)
            {
                _innermostReferencedScopeRegion = referencedScopeRegion;
            }
        }

        // <summary>
        // Gets/sets the innermost referenced scope region of the current aggregate.
        // This property is used to save/restore the scope region value during a potentially throw-away attempt to
        // convert an <see cref="AST.MethodExpr" /> as a collection function in the
        // <see
        //     cref="SemanticAnalyzer.ConvertAggregateFunctionInGroupScope" />
        // method.
        // Setting the value is not allowed after <see cref="ValidateAndComputeEvaluatingScopeRegion" /> has been called.
        // </summary>
        internal ScopeRegion InnermostReferencedScopeRegion
        {
            get { return _innermostReferencedScopeRegion; }
            set
            {
                Debug.Assert(
                    _evaluatingScopeRegion == null,
                    "Can't change _innermostReferencedScopeRegion after _evaluatingScopeRegion has been initialized.");
                _innermostReferencedScopeRegion = value;
            }
        }

        private ScopeRegion _innermostReferencedScopeRegion;

        // <summary>
        // Validates the aggregate info and computes <see cref="EvaluatingScopeRegion" /> property.
        // Seals the aggregate info object (no more AddContainedAggregate(...), RemoveContainedAggregate(...) and UpdateScopeIndex(...) calls allowed).
        // </summary>
        internal void ValidateAndComputeEvaluatingScopeRegion(SemanticResolver sr)
        {
            Debug.Assert(_evaluatingScopeRegion == null, "_evaluatingScopeRegion has already been initialized");
            //
            // If _innermostReferencedScopeRegion is null, it means the aggregate is not correlated (a constant value),
            // so resolve it to the DefiningScopeRegion.
            //
            _evaluatingScopeRegion = _innermostReferencedScopeRegion ?? DefiningScopeRegion;

            if (!_evaluatingScopeRegion.IsAggregating)
            {
                //
                // In some cases the found scope region does not aggregate (has no grouping). So adding the aggregate to that scope won't work.
                // In this situation we need to backtrack from the found region to the first inner region that performs aggregation.
                // Example:
                // select yy.cx, yy.cy, yy.cz
                // from {1, 2} as x cross apply (select zz.cx, zz.cy, zz.cz
                //                               from {3, 4} as y cross apply (select Count(x) as cx, Count(y) as cy, Count(z) as cz
                //                                                             from {5, 6} as z) as zz
                //                              ) as yy
                // Note that Count aggregates cx and cy refer to scope regions that do aggregate. All three aggregates needs to be added to the only
                // aggregating region - the innermost.
                //
                var scopeRegionIndex = _evaluatingScopeRegion.ScopeRegionIndex;
                _evaluatingScopeRegion = null;
                foreach (var innerSR in sr.ScopeRegions.Skip(scopeRegionIndex))
                {
                    if (innerSR.IsAggregating)
                    {
                        _evaluatingScopeRegion = innerSR;
                        break;
                    }
                }
                if (_evaluatingScopeRegion == null)
                {
                    var message = Strings.GroupVarNotFoundInScope;
                    throw new EntitySqlException(message);
                }
            }

            //
            // Validate all the contained aggregates for violation of the containment rule:
            // None of the nested (contained) aggregates must be evaluating on a scope region that is 
            //      a. equal or inner to the evaluating scope of the current aggregate and
            //      b. equal or outer to the defining scope of the current aggregate.
            //
            // Example of a disallowed query:
            //
            //      select 
            //              (select max(x + max(y))
            //               from {1} as y)
            //      from {0} as x
            //
            // Example of an allowed query where the ESR of the nested aggregate is outer to the ESR of the outer aggregate:
            //
            //      select 
            //              (select max(y + max(x))
            //               from {1} as y)
            //      from {0} as x
            //
            // Example of an allowed query where the ESR of the nested aggregate is inner to the DSR of the outer aggregate:
            //
            //      select max(x + anyelement(select value max(y) from {1} as y))
            //      from {0} as x
            //
            Debug.Assert(_evaluatingScopeRegion.IsAggregating, "_evaluatingScopeRegion.IsAggregating must be true");
            Debug.Assert(
                _evaluatingScopeRegion.ScopeRegionIndex <= DefiningScopeRegion.ScopeRegionIndex,
                "_evaluatingScopeRegion must outer to the DefiningScopeRegion");
            ValidateContainedAggregates(_evaluatingScopeRegion.ScopeRegionIndex, DefiningScopeRegion.ScopeRegionIndex);
        }

        // <summary>
        // Recursively validates that <see cref="GroupAggregateInfo.EvaluatingScopeRegion" /> of all contained aggregates
        // is outside of the range of scope regions defined by <paramref name="outerBoundaryScopeRegionIndex" /> and
        // <paramref
        //     name="innerBoundaryScopeRegionIndex" />
        // .
        // Throws in the case of violation.
        // </summary>
        private void ValidateContainedAggregates(int outerBoundaryScopeRegionIndex, int innerBoundaryScopeRegionIndex)
        {
            if (_containedAggregates != null)
            {
                foreach (var containedAggregate in _containedAggregates)
                {
                    if (containedAggregate.EvaluatingScopeRegion.ScopeRegionIndex >= outerBoundaryScopeRegionIndex
                        &&
                        containedAggregate.EvaluatingScopeRegion.ScopeRegionIndex <= innerBoundaryScopeRegionIndex)
                    {
                        int line, column;
                        var currentAggregateInfo = EntitySqlException.FormatErrorContext(
                            ErrCtx.CommandText,
                            ErrCtx.InputPosition,
                            ErrCtx.ErrorContextInfo,
                            ErrCtx.UseContextInfoAsResourceIdentifier,
                            out line, out column);

                        var nestedAggregateInfo = EntitySqlException.FormatErrorContext(
                            containedAggregate.ErrCtx.CommandText,
                            containedAggregate.ErrCtx.InputPosition,
                            containedAggregate.ErrCtx.ErrorContextInfo,
                            containedAggregate.ErrCtx.UseContextInfoAsResourceIdentifier,
                            out line, out column);

                        var message = Strings.NestedAggregateCannotBeUsedInAggregate(nestedAggregateInfo, currentAggregateInfo);
                        throw new EntitySqlException(message);
                    }

                    //
                    // We need to check the full subtree in order to catch this case:
                    //      select max(x +
                    //                     anyelement(select max(y + 
                    //                                               anyelement(select value max(x)
                    //                                               from {2} as z))
                    //                                from {1} as y))
                    //      from {0} as x
                    //
                    containedAggregate.ValidateContainedAggregates(outerBoundaryScopeRegionIndex, innerBoundaryScopeRegionIndex);
                }
            }
        }

        internal void SetContainingAggregate(GroupAggregateInfo containingAggregate)
        {
            if (_containingAggregate != null)
            {
                //
                // Aggregates in this query
                //
                //      select value max(anyelement(select value max(b + max(a + anyelement(select value c1 
                //                                                                          from {2} as c group by c as c1))) 
                //                                  from {1} as b group by b as b1)) 
                //
                //      from {0} as a group by a as a1
                //
                // are processed in the following steps:
                // 1.  the outermost aggregate (max1) begins processing as a collection function;
                // 2.  the middle aggregate (max2) begins processing as a collection function;
                // 3.  the innermost aggregate (max3) is processed as a collection function;
                // 4.  max3 is reprocessed as an aggregate; it does not see any containing aggregates at this point, so it's not wired up;
                //     max3 is validated and sealed;
                //     evaluating scope region for max3 is the outermost scope region, to which it gets assigned;
                //     max3 aggregate info object is attached to the corresponding AST node;
                // 5.  max2 completes processing as a collection function and begins processing as an aggregate;
                // 6.  max3 is reprocessed as an aggregate in the SemanticAnalyzer.TryConvertAsResolvedGroupAggregate(...) method, and 
                //     wired up to max2 as contained/containing;
                // 7.  max2 completes processing as an aggregate;
                //     max2 is validated and sealed;
                //     note that max2 does not see any containing aggregates at this point, so it's wired up only to max3;
                //     evaluating scope region for max2 is the middle scope region to which it gets assigned;
                // 6.  middle scope region completes processing, yields a DbExpression and cleans up all aggregate info objects assigned to it (max2);
                //     max2 is detached from the corresponding AST node;
                //     at this point max3 is still assigned to the outermost scope region and still wired to the dropped max2 as containing/contained;
                // 7.  max1 completes processing as a collection function and begins processing as an aggregate;
                // 8.  max2 is revisited and begins processing as a collection function (note that because the old aggregate info object for max2 was dropped 
                //     and detached from the AST node in step 6, SemanticAnalyzer.TryConvertAsResolvedGroupAggregate(...) does not recognize max2 as an aggregate);
                // 9.  max3 is recognized as an aggregate in the SemanticAnalyzer.TryConvertAsResolvedGroupAggregate(...) method;
                //     max3 is rewired from the dropped max2 (step 6) to max1 as contained/containing, now max1 and max3 are wired as containing/contained;
                // 10. max2 completes processing as a collection function and begins processing as an aggregate;
                //     max2 sees max1 as a containing aggregate and wires to it;
                // 11. max3 is reprocessed as resolved aggregate inside of TryConvertAsResolvedGroupAggregate(...) method;
                //     max3 is rewired from max1 to max2 as containing/contained aggregate;
                // 12. at this point max1 is wired to max2 and max2 is wired to max3, the tree is correct;
                //
                // ... both max1 and max3 are assigned to the same scope for evaluation, this is detected and an error is reported;
                //

                //
                // Remove this aggregate from the old containing aggregate before rewiring to the new parent.
                //
                _containingAggregate.RemoveContainedAggregate(this);
            }

            //
            // Accept the new parent and wire to it as a contained aggregate.
            //
            _containingAggregate = containingAggregate;
            if (_containingAggregate != null)
            {
                _containingAggregate.AddContainedAggregate(this);
            }
        }

        // <summary>
        // Function call is not allowed after <see cref="ValidateAndComputeEvaluatingScopeRegion" /> has been called.
        // Adding new contained aggregate may invalidate the current aggregate.
        // </summary>
        private void AddContainedAggregate(GroupAggregateInfo containedAggregate)
        {
            Debug.Assert(_evaluatingScopeRegion == null, "Can not add contained aggregate after _evaluatingScopeRegion have been computed.");

            if (_containedAggregates == null)
            {
                _containedAggregates = new List<GroupAggregateInfo>();
            }
            Debug.Assert(_containedAggregates.Contains(containedAggregate) == false, "containedAggregate is already registered");
            _containedAggregates.Add(containedAggregate);
        }

        private List<GroupAggregateInfo> _containedAggregates;

        // <summary>
        // Function call is _allowed_ after <see cref="ValidateAndComputeEvaluatingScopeRegion" /> has been called.
        // Removing contained aggregates cannot invalidate the current aggregate.
        // Consider the following query:
        // select value max(a + anyelement(select value max(b + max(a + anyelement(select value c1
        // from {2} as c group by c as c1)))
        // from {1} as b group by b as b1))
        // from {0} as a group by a as a1
        // Outer aggregate - max1, middle aggregate - max2, inner aggregate - max3.
        // In this query after max1 have been processed as a collection function, max2 and max3 are wired as containing/contained.
        // There is a point later when max1 is processed as an aggregate, max2 is processed as a collection function and max3 is processed as
        // an aggregate. Note that at this point the "aggregate" version of max2 is dropped and detached from the AST node when the middle scope region
        // completes processing; also note that because evaluating scope region of max3 is the outer scope region, max3 aggregate info is still attached to
        // the AST node and it is still wired to the dropped aggregate info object of max2. At this point max3 does not see new max2 as a containing aggregate,
        // and it rewires to max1, during this rewiring it needs to remove itself from the old max2 and add itself to max1.
        // The old max2 at this point is sealed, so the removal is performed on the sealed object.
        // </summary>
        private void RemoveContainedAggregate(GroupAggregateInfo containedAggregate)
        {
            Debug.Assert(
                _containedAggregates != null && _containedAggregates.Contains(containedAggregate),
                "_containedAggregates.Contains(containedAggregate)");

            _containedAggregates.Remove(containedAggregate);
        }

        internal readonly GroupAggregateKind AggregateKind;

        // <summary>
        // Null when <see cref="GroupAggregateInfo" /> is created for a group key processing.
        // </summary>
        internal readonly GroupAggregateExpr AstNode;

        internal readonly ErrorContext ErrCtx;

        // <summary>
        // Scope region that contains the aggregate expression.
        // </summary>
        internal readonly ScopeRegion DefiningScopeRegion;

        // <summary>
        // Scope region that evaluates the aggregate expression.
        // </summary>
        internal ScopeRegion EvaluatingScopeRegion
        {
            get
            {
                //
                // _evaluatingScopeRegion is initialized in the ValidateAndComputeEvaluatingScopeRegion(...) method.
                //
                Debug.Assert(_evaluatingScopeRegion != null, "_evaluatingScopeRegion is not initialized");
                return _evaluatingScopeRegion;
            }
        }

        private ScopeRegion _evaluatingScopeRegion;

        // <summary>
        // Parent aggregate expression that contains the current aggregate expression.
        // May be null.
        // </summary>
        internal GroupAggregateInfo ContainingAggregate
        {
            get { return _containingAggregate; }
        }

        private GroupAggregateInfo _containingAggregate;

        internal string AggregateName;
        internal DbNullExpression AggregateStubExpression;
    }
}
