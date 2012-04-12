namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Common.Utils.Boolean;
    using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Validates each mapping fragment/cell (Qc = Qs)
    /// by unfolding update views in Qs and checking query equivalence
    /// </summary>
    internal class RewritingValidator
    {
        private readonly ViewgenContext _viewgenContext;
        private readonly MemberDomainMap _domainMap;
        private readonly CellTreeNode _basicView;
        private readonly IEnumerable<MemberPath> _keyAttributes;
        private readonly ErrorLog _errorLog;

        internal RewritingValidator(ViewgenContext context, CellTreeNode basicView)
        {
            _viewgenContext = context;
            _basicView = basicView;
            _domainMap = _viewgenContext.MemberMaps.UpdateDomainMap;
            _keyAttributes = MemberPath.GetKeyMembers(_viewgenContext.Extent, _domainMap);
            _errorLog = new ErrorLog();
        }

        #region Main logic

        internal void Validate()
        {
            // turn rewritings into cell trees
            // plain: according to rewritings for case statements
            var plainMemberValueTrees = CreateMemberValueTrees(false);
            // complement: uses complement rewriting for the last WHEN ... THEN
            // This is how the final case statement will be generated in update views
            var complementMemberValueTrees = CreateMemberValueTrees(true);

            var plainWhereClauseVisitor = new WhereClauseVisitor(_basicView, plainMemberValueTrees);
            var complementWhereClauseVisitor = new WhereClauseVisitor(_basicView, complementMemberValueTrees);

            // produce CellTree for each SQuery
            foreach (var wrapper in _viewgenContext.AllWrappersForExtent)
            {
                var cell = wrapper.OnlyInputCell;
                // construct cell tree for CQuery
                CellTreeNode cQueryTree = new LeafCellTreeNode(_viewgenContext, wrapper);
                // sQueryTree: unfolded update view inside S-side of the cell
                CellTreeNode sQueryTree;
                // construct cell tree for SQuery (will be used for domain constraint checking)
                var complementSQueryTreeForCondition = complementWhereClauseVisitor.GetCellTreeNode(cell.SQuery.WhereClause);
                Debug.Assert(complementSQueryTreeForCondition != null, "Rewriting for S-side query is unsatisfiable");
                if (complementSQueryTreeForCondition == null)
                {
                    continue; // situation should never happen
                }
                if (complementSQueryTreeForCondition != _basicView)
                {
                    // intersect with basic expression
                    sQueryTree = new OpCellTreeNode(_viewgenContext, CellTreeOpType.IJ, complementSQueryTreeForCondition, _basicView);
                }
                else
                {
                    sQueryTree = _basicView;
                }

                // Append in-set or in-end condition to both queries to produce more concise errors
                // Otherwise, the errors are of the form "if there exists an entity in extent, then violation". We don't care about empty extents
                var inExtentCondition = BoolExpression.CreateLiteral(wrapper.CreateRoleBoolean(), _viewgenContext.MemberMaps.QueryDomainMap);

                BoolExpression unsatisfiedConstraint;
                if (!CheckEquivalence(
                    cQueryTree.RightFragmentQuery, sQueryTree.RightFragmentQuery, inExtentCondition,
                    out unsatisfiedConstraint))
                {
                    var extentName = StringUtil.FormatInvariant("{0}", _viewgenContext.Extent);

                    // Simplify to produce more readable error messages
                    cQueryTree.RightFragmentQuery.Condition.ExpensiveSimplify();
                    sQueryTree.RightFragmentQuery.Condition.ExpensiveSimplify();

                    var message = Strings.ViewGen_CQ_PartitionConstraint(extentName);

                    ReportConstraintViolation(
                        message, unsatisfiedConstraint, ViewGenErrorCode.PartitionConstraintViolation,
                        cQueryTree.GetLeaves().Concat(sQueryTree.GetLeaves()));
                }

                var plainSQueryTreeForCondition = plainWhereClauseVisitor.GetCellTreeNode(cell.SQuery.WhereClause);
                Debug.Assert(plainSQueryTreeForCondition != null, "Rewriting for S-side query is unsatisfiable");
                if (plainSQueryTreeForCondition != null)
                {
                    // Query is non-empty. Check domain constraints on:
                    // (a) swapped members
                    DomainConstraintVisitor.CheckConstraints(plainSQueryTreeForCondition, wrapper, _viewgenContext, _errorLog);
                    //If you have already found errors, just continue on to the next wrapper instead of                    //collecting more errors for the same 
                    if (_errorLog.Count > 0)
                    {
                        continue;
                    }
                    // (b) projected members
                    CheckConstraintsOnProjectedConditionMembers(plainMemberValueTrees, wrapper, sQueryTree, inExtentCondition);
                    if (_errorLog.Count > 0)
                    {
                        continue;
                    }
                }
                CheckConstraintsOnNonNullableMembers(wrapper);
            }

            if (_errorLog.Count > 0)
            {
                ExceptionHelpers.ThrowMappingException(_errorLog, _viewgenContext.Config);
            }
        }

        // Checks equivalence of two C-side queries
        // inExtentConstraint holds a role variable that effectively denotes that some extent is non-empty
        private bool CheckEquivalence(
            FragmentQuery cQuery, FragmentQuery sQuery, BoolExpression inExtentCondition,
            out BoolExpression unsatisfiedConstraint)
        {
            var cMinusSx = _viewgenContext.RightFragmentQP.Difference(cQuery, sQuery);
            var sMinusCx = _viewgenContext.RightFragmentQP.Difference(sQuery, cQuery);

            // add in-extent condition
            var cMinusS = FragmentQuery.Create(BoolExpression.CreateAnd(cMinusSx.Condition, inExtentCondition));
            var sMinusC = FragmentQuery.Create(BoolExpression.CreateAnd(sMinusCx.Condition, inExtentCondition));

            unsatisfiedConstraint = null;
            var forwardInclusion = true;
            var backwardInclusion = true;

            if (_viewgenContext.RightFragmentQP.IsSatisfiable(cMinusS))
            {
                unsatisfiedConstraint = cMinusS.Condition;
                forwardInclusion = false;
            }
            if (_viewgenContext.RightFragmentQP.IsSatisfiable(sMinusC))
            {
                unsatisfiedConstraint = sMinusC.Condition;
                backwardInclusion = false;
            }
            if (forwardInclusion && backwardInclusion)
            {
                return true;
            }
            else
            {
                unsatisfiedConstraint.ExpensiveSimplify();
                return false;
            }
        }

        private void ReportConstraintViolation(
            string message, BoolExpression extraConstraint, ViewGenErrorCode errorCode, IEnumerable<LeftCellWrapper> relevantWrappers)
        {
            if (ErrorPatternMatcher.FindMappingErrors(_viewgenContext, _domainMap, _errorLog))
            {
                return;
            }

            extraConstraint.ExpensiveSimplify();
            // gather all relevant cell wrappers and sort them in the original input order
            var relevantCellWrappers = new HashSet<LeftCellWrapper>(relevantWrappers);
            var relevantWrapperList = new List<LeftCellWrapper>(relevantCellWrappers);
            relevantWrapperList.Sort(LeftCellWrapper.OriginalCellIdComparer);

            var builder = new StringBuilder();
            builder.AppendLine(message);
            EntityConfigurationToUserString(extraConstraint, builder);
            _errorLog.AddEntry(new ErrorLog.Record(errorCode, builder.ToString(), relevantCellWrappers, ""));
        }

        // according to case statements, where WHEN ... THEN was replaced by ELSE
        private Dictionary<MemberValueBinding, CellTreeNode> CreateMemberValueTrees(bool complementElse)
        {
            var memberValueTrees = new Dictionary<MemberValueBinding, CellTreeNode>();

            foreach (var column in _domainMap.ConditionMembers(_viewgenContext.Extent))
            {
                var domain = new List<Constant>(_domainMap.GetDomain(column));

                // all domain members but the last
                var memberCover = new OpCellTreeNode(_viewgenContext, CellTreeOpType.Union);
                for (var i = 0; i < domain.Count; i++)
                {
                    var domainValue = domain[i];
                    var memberValue = new MemberValueBinding(column, domainValue);
                    var memberConditionQuery = QueryRewriter.CreateMemberConditionQuery(column, domainValue, _keyAttributes, _domainMap);
                    Tile<FragmentQuery> rewriting;
                    if (_viewgenContext.TryGetCachedRewriting(memberConditionQuery, out rewriting))
                    {
                        // turn rewriting into a cell tree
                        var cellTreeNode = QueryRewriter.TileToCellTree(rewriting, _viewgenContext);
                        memberValueTrees[memberValue] = cellTreeNode;
                        // collect a union of all domain constants but the last
                        if (i < domain.Count - 1)
                        {
                            memberCover.Add(cellTreeNode);
                        }
                    }
                    else
                    {
                        Debug.Fail(String.Format(CultureInfo.InvariantCulture, "No cached rewriting for {0}={1}", column, domainValue));
                    }
                }

                if (complementElse && domain.Count > 1)
                {
                    var lastDomainValue = domain[domain.Count - 1];
                    var lastMemberValue = new MemberValueBinding(column, lastDomainValue);
                    memberValueTrees[lastMemberValue] = new OpCellTreeNode(_viewgenContext, CellTreeOpType.LASJ, _basicView, memberCover);
                }
            }

            return memberValueTrees;
        }

        #endregion

        #region Checking constraints on projected condition members

        private void CheckConstraintsOnProjectedConditionMembers(
            Dictionary<MemberValueBinding, CellTreeNode> memberValueTrees, LeftCellWrapper wrapper, CellTreeNode sQueryTree,
            BoolExpression inExtentCondition)
        {
            // for S-side condition members that are projected,
            // add condition <member=value> on both sides of the mapping constraint, and check key equivalence
            // applies to columns that are (1) projected and (2) conditional
            foreach (var column in _domainMap.ConditionMembers(_viewgenContext.Extent))
            {
                // Get the slot on the C side and see if it is projected
                var index = _viewgenContext.MemberMaps.ProjectedSlotMap.IndexOf(column);
                var slot = wrapper.RightCellQuery.ProjectedSlotAt(index) as MemberProjectedSlot;
                if (slot != null)
                {
                    foreach (var domainValue in _domainMap.GetDomain(column))
                    {
                        CellTreeNode sQueryTreeForDomainValue;
                        if (memberValueTrees.TryGetValue(new MemberValueBinding(column, domainValue), out sQueryTreeForDomainValue))
                        {
                            var cWhereClause = PropagateCellConstantsToWhereClause(
                                wrapper, wrapper.RightCellQuery.WhereClause,
                                domainValue, column, _viewgenContext.MemberMaps);
                            var cCombinedQuery = FragmentQuery.Create(cWhereClause);
                            var sCombinedTree = (sQueryTree == _basicView)
                                                    ? sQueryTreeForDomainValue
                                                    : new OpCellTreeNode(
                                                          _viewgenContext, CellTreeOpType.IJ, sQueryTreeForDomainValue, sQueryTree);

                            BoolExpression unsatisfiedConstraint;
                            if (!CheckEquivalence(
                                cCombinedQuery, sCombinedTree.RightFragmentQuery, inExtentCondition,
                                out unsatisfiedConstraint))
                            {
                                var memberLossMessage = Strings.ViewGen_CQ_DomainConstraint(slot.ToUserString());
                                ReportConstraintViolation(
                                    memberLossMessage, unsatisfiedConstraint, ViewGenErrorCode.DomainConstraintViolation,
                                    sCombinedTree.GetLeaves().Concat(new[] { wrapper }));
                            }
                        }
                    }
                }
            }
        }

        // effects: Given a sequence of constants that need to be propagated
        // to the C-side and the current boolean expression, generates a new
        // expression of the form "expression AND C-side Member in constants"
        // expression" and returns it. Each constant is propagated only if member
        // is projected -- if member is not projected, returns "expression"
        internal static BoolExpression PropagateCellConstantsToWhereClause(
            LeftCellWrapper wrapper, BoolExpression expression,
            Constant constant, MemberPath member,
            MemberMaps memberMaps)
        {
            var joinSlot = wrapper.GetCSideMappedSlotForSMember(member);
            if (joinSlot == null)
            {
                return expression;
            }

            var negatedConstant = constant as NegatedConstant;

            // Look at the constants and determine if they correspond to
            // typeConstants or scalarConstants
            // This slot is being projected. We need to add a where clause element
            Debug.Assert(constant is ScalarConstant || constant.IsNull() || negatedConstant != null, "Invalid type of constant");

            // We want the possible values for joinSlot.MemberPath which is a
            // C-side element -- so we use the queryDomainMap
            var possibleValues = memberMaps.QueryDomainMap.GetDomain(joinSlot.MemberPath);
            // Note: the values in constaints can be null or not null as
            // well (i.e., just not scalarConstants)
            var allowedValues = new Set<Constant>(Constant.EqualityComparer);
            if (negatedConstant != null)
            {
                // select all values from the c-side domain that are not in the negated set
                allowedValues.Unite(possibleValues);
                allowedValues.Difference(negatedConstant.Elements);
            }
            else
            {
                allowedValues.Add(constant);
            }
            MemberRestriction restriction = new ScalarRestriction(joinSlot.MemberPath, allowedValues, possibleValues);

            var result = BoolExpression.CreateAnd(expression, BoolExpression.CreateLiteral(restriction, memberMaps.QueryDomainMap));
            return result;
        }

        #endregion

        /// <summary>
        /// Given a LeftCellWrapper for the S-side fragment and a non-nullable colum m, return a CQuery with nullability condition
        /// appended to Cquery of c-side member that column m is mapped to
        /// </summary>
        private static FragmentQuery AddNullConditionOnCSideFragment(LeftCellWrapper wrapper, MemberPath member, MemberMaps memberMaps)
        {
            var projectedSlot = wrapper.GetCSideMappedSlotForSMember(member);
            if (projectedSlot == null
                || !projectedSlot.MemberPath.IsNullable) //don't bother checking further fore non nullable C-side member
            {
                return null;
            }
            var expression = wrapper.RightCellQuery.WhereClause;

            var possibleValues = memberMaps.QueryDomainMap.GetDomain(projectedSlot.MemberPath);
            var allowedValues = new Set<Constant>(Constant.EqualityComparer);
            allowedValues.Add(Constant.Null);

            //Create a condition as conjunction of originalCondition and slot IS NULL
            MemberRestriction restriction = new ScalarRestriction(projectedSlot.MemberPath, allowedValues, possibleValues);
            var resultingExpr = BoolExpression.CreateAnd(expression, BoolExpression.CreateLiteral(restriction, memberMaps.QueryDomainMap));

            return FragmentQuery.Create(resultingExpr);
        }

        /// <summary>
        /// Checks whether non nullable S-side members are mapped to nullable C-query.
        /// It is possible that C-side attribute is nullable but the fragment's C-query is not
        /// </summary>
        private void CheckConstraintsOnNonNullableMembers(LeftCellWrapper wrapper)
        {
            //For each non-condition member that has non-nullability constraint
            foreach (var column in _domainMap.NonConditionMembers(_viewgenContext.Extent))
            {
                var isColumnSimpleType = (column.EdmType as SimpleType) != null;

                if (!column.IsNullable && isColumnSimpleType)
                {
                    var cFragment = AddNullConditionOnCSideFragment(wrapper, column, _viewgenContext.MemberMaps);

                    if (cFragment != null
                        && _viewgenContext.RightFragmentQP.IsSatisfiable(cFragment))
                    {
                        _errorLog.AddEntry(
                            new ErrorLog.Record(
                                ViewGenErrorCode.NullableMappingForNonNullableColumn,
                                Strings.Viewgen_NullableMappingForNonNullableColumn(wrapper.LeftExtent.ToString(), column.ToFullString()),
                                wrapper.Cells, ""));
                    }
                }
            }
        }

        #region Methods for turning a boolean condition into user string

        internal static void EntityConfigurationToUserString(BoolExpression condition, StringBuilder builder)
        {
            //By default write the Round tripping message
            EntityConfigurationToUserString(condition, builder, true);
        }

        internal static void EntityConfigurationToUserString(
            BoolExpression condition, StringBuilder builder, bool writeRoundTrippingMessage)
        {
            condition.AsUserString(builder, "PK", writeRoundTrippingMessage);
        }

        #endregion

        #region WhereClauseVisitor: turns WHERE clause into CellTreeNode

        private class WhereClauseVisitor : Visitor<DomainConstraint<BoolLiteral, Constant>, CellTreeNode>
        {
            private readonly ViewgenContext _viewgenContext;
            private readonly CellTreeNode _topLevelTree;
            private readonly Dictionary<MemberValueBinding, CellTreeNode> _memberValueTrees;

            internal WhereClauseVisitor(CellTreeNode topLevelTree, Dictionary<MemberValueBinding, CellTreeNode> memberValueTrees)
            {
                _topLevelTree = topLevelTree;
                _memberValueTrees = memberValueTrees;
                _viewgenContext = topLevelTree.ViewgenContext;
            }

            // returns _topLevelTree when expression evaluates to True, null if it evaluates to False
            internal CellTreeNode GetCellTreeNode(BoolExpression whereClause)
            {
                return whereClause.Tree.Accept(this);
            }

            internal override CellTreeNode VisitAnd(AndExpr<DomainConstraint<BoolLiteral, Constant>> expression)
            {
                var childrenTrees = AcceptChildren(expression.Children);
                var node = new OpCellTreeNode(_viewgenContext, CellTreeOpType.IJ);
                foreach (var childNode in childrenTrees)
                {
                    if (childNode == null)
                    {
                        return null; // unsatisfiable
                    }
                    if (childNode != _topLevelTree)
                    {
                        node.Add(childNode);
                    }
                }
                return node.Children.Count == 0 ? _topLevelTree : node;
            }

            internal override CellTreeNode VisitTrue(TrueExpr<DomainConstraint<BoolLiteral, Constant>> expression)
            {
                return _topLevelTree;
            }

            internal override CellTreeNode VisitTerm(TermExpr<DomainConstraint<BoolLiteral, Constant>> expression)
            {
                var oneOf = (MemberRestriction)expression.Identifier.Variable.Identifier;
                var range = expression.Identifier.Range;

                // create a disjunction
                var disjunctionNode = new OpCellTreeNode(_viewgenContext, CellTreeOpType.Union);
                CellTreeNode singleNode = null;
                foreach (var value in range)
                {
                    if (TryGetCellTreeNode(oneOf.RestrictedMemberSlot.MemberPath, value, out singleNode))
                    {
                        disjunctionNode.Add(singleNode);
                    }
                    // else, there is no rewriting for this member value, i.e., it is empty
                }
                switch (disjunctionNode.Children.Count)
                {
                    case 0:
                        return null; // empty rewriting
                    case 1:
                        return singleNode;
                    default:
                        return disjunctionNode;
                }
            }

            internal override CellTreeNode VisitFalse(FalseExpr<DomainConstraint<BoolLiteral, Constant>> expression)
            {
                throw new NotImplementedException();
            }

            internal override CellTreeNode VisitNot(NotExpr<DomainConstraint<BoolLiteral, Constant>> expression)
            {
                throw new NotImplementedException();
            }

            internal override CellTreeNode VisitOr(OrExpr<DomainConstraint<BoolLiteral, Constant>> expression)
            {
                throw new NotImplementedException();
            }

            private bool TryGetCellTreeNode(MemberPath memberPath, Constant value, out CellTreeNode singleNode)
            {
                return (_memberValueTrees.TryGetValue(new MemberValueBinding(memberPath, value), out singleNode));
            }

            private IEnumerable<CellTreeNode> AcceptChildren(IEnumerable<BoolExpr<DomainConstraint<BoolLiteral, Constant>>> children)
            {
                foreach (var child in children)
                {
                    yield return child.Accept(this);
                }
            }
        }

        #endregion

        #region DomainConstraintVisitor: checks domain constraints

        internal class DomainConstraintVisitor : CellTreeNode.SimpleCellTreeVisitor<bool, bool>
        {
            private readonly LeftCellWrapper m_wrapper;
            private readonly ViewgenContext m_viewgenContext;
            private readonly ErrorLog m_errorLog;

            private DomainConstraintVisitor(LeftCellWrapper wrapper, ViewgenContext context, ErrorLog errorLog)
            {
                m_wrapper = wrapper;
                m_viewgenContext = context;
                m_errorLog = errorLog;
            }

            internal static void CheckConstraints(
                CellTreeNode node, LeftCellWrapper wrapper,
                ViewgenContext context, ErrorLog errorLog)
            {
                var visitor = new DomainConstraintVisitor(wrapper, context, errorLog);
                node.Accept(visitor, true);
            }

            internal override bool VisitLeaf(LeafCellTreeNode node, bool dummy)
            {
                // make sure all projected attributes in wrapper correspond exactly to those in node
                var thisQuery = m_wrapper.RightCellQuery;
                var thatQuery = node.LeftCellWrapper.RightCellQuery;
                var collidingColumns = new List<MemberPath>();
                if (thisQuery != thatQuery)
                {
                    for (var i = 0; i < thisQuery.NumProjectedSlots; i++)
                    {
                        var thisSlot = thisQuery.ProjectedSlotAt(i) as MemberProjectedSlot;
                        if (thisSlot != null)
                        {
                            var thatSlot = thatQuery.ProjectedSlotAt(i) as MemberProjectedSlot;
                            if (thatSlot != null)
                            {
                                var tableMember = m_viewgenContext.MemberMaps.ProjectedSlotMap[i];
                                if (!tableMember.IsPartOfKey)
                                {
                                    if (!MemberPath.EqualityComparer.Equals(thisSlot.MemberPath, thatSlot.MemberPath))
                                    {
                                        collidingColumns.Add(tableMember);
                                    }
                                }
                            }
                        }
                    }
                }
                if (collidingColumns.Count > 0)
                {
                    var columnsString = MemberPath.PropertiesToUserString(collidingColumns, false);
                    var message = Strings.ViewGen_NonKeyProjectedWithOverlappingPartitions(columnsString);
                    var record = new ErrorLog.Record(
                        ViewGenErrorCode.NonKeyProjectedWithOverlappingPartitions, message,
                        new[] { m_wrapper, node.LeftCellWrapper }, String.Empty);
                    m_errorLog.AddEntry(record);
                }
                return true;
            }

            internal override bool VisitOpNode(OpCellTreeNode node, bool dummy)
            {
                if (node.OpType
                    == CellTreeOpType.LASJ)
                {
                    // add conditions only on the positive node
                    node.Children[0].Accept(this, dummy);
                }
                else
                {
                    foreach (var child in node.Children)
                    {
                        child.Accept(this, dummy);
                    }
                }
                return true;
            }
        }

        #endregion

        #region MemberValueBinding struct: (MemberPath, CellConstant) pair

        private struct MemberValueBinding : IEquatable<MemberValueBinding>
        {
            internal readonly MemberPath Member;
            internal readonly Constant Value;

            public MemberValueBinding(MemberPath member, Constant value)
            {
                Member = member;
                Value = value;
            }

            public override string ToString()
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}={1}", Member, Value);
            }

            #region IEquatable<MemberValue> Members

            public bool Equals(MemberValueBinding other)
            {
                return MemberPath.EqualityComparer.Equals(Member, other.Member) &&
                       Constant.EqualityComparer.Equals(Value, other.Value);
            }

            #endregion
        }

        #endregion
    }
}
