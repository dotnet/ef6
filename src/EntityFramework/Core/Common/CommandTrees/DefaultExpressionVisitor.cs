// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using CqtBuilder = System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.DbExpressionBuilder;

    /// <summary> Visits each element of an expression tree from a given root expression. If any element changes, the tree is rebuilt back to the root and the new root expression is returned; otherwise the original root expression is returned. </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class DefaultExpressionVisitor : DbExpressionVisitor<DbExpression>
    {
        private readonly Dictionary<DbVariableReferenceExpression, DbVariableReferenceExpression> varMappings =
            new Dictionary<DbVariableReferenceExpression, DbVariableReferenceExpression>();

        /// <summary>
        /// Initializes a new instance of the
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DefaultExpressionVisitor" />
        /// class.
        /// </summary>
        protected DefaultExpressionVisitor()
        {
        }

        /// <summary>Replaces an old expression with a new one for the expression visitor.</summary>
        /// <param name="oldExpression">The old expression.</param>
        /// <param name="newExpression">The new expression.</param>
        protected virtual void OnExpressionReplaced(DbExpression oldExpression, DbExpression newExpression)
        {
        }

        /// <summary>Represents an event when the variable is rebound for the expression visitor.</summary>
        /// <param name="fromVarRef">The location of the variable.</param>
        /// <param name="toVarRef">The reference of the variable where it is rebounded.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "toVar")]
        protected virtual void OnVariableRebound(DbVariableReferenceExpression fromVarRef, DbVariableReferenceExpression toVarRef)
        {
        }

        /// <summary>Represents an event when entering the scope for the expression visitor with specified scope variables.</summary>
        /// <param name="scopeVariables">The collection of scope variables.</param>
        protected virtual void OnEnterScope(IEnumerable<DbVariableReferenceExpression> scopeVariables)
        {
        }

        /// <summary>Exits the scope for the expression visitor.</summary>
        protected virtual void OnExitScope()
        {
        }

        /// <summary>Implements the visitor pattern for the expression.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="expression">The expression.</param>
        protected virtual DbExpression VisitExpression(DbExpression expression)
        {
            DbExpression newValue = null;
            if (expression != null)
            {
                newValue = expression.Accept(this);
            }

            return newValue;
        }

        /// <summary>Implements the visitor pattern for the expression list.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="list">The expression list.</param>
        protected virtual IList<DbExpression> VisitExpressionList(IList<DbExpression> list)
        {
            return VisitList(list, VisitExpression);
        }

        /// <summary>Implements the visitor pattern for expression binding.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="binding">The expression binding.</param>
        protected virtual DbExpressionBinding VisitExpressionBinding(DbExpressionBinding binding)
        {
            var result = binding;
            if (binding != null)
            {
                var newInput = VisitExpression(binding.Expression);
                if (!ReferenceEquals(binding.Expression, newInput))
                {
                    result = CqtBuilder.BindAs(newInput, binding.VariableName);
                    RebindVariable(binding.Variable, result.Variable);
                }
            }
            return result;
        }

        /// <summary>Implements the visitor pattern for the expression binding list.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="list">The expression binding list.</param>
        protected virtual IList<DbExpressionBinding> VisitExpressionBindingList(IList<DbExpressionBinding> list)
        {
            return VisitList(list, VisitExpressionBinding);
        }

        /// <summary>Implements the visitor pattern for the group expression binding.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="binding">The binding.</param>
        protected virtual DbGroupExpressionBinding VisitGroupExpressionBinding(DbGroupExpressionBinding binding)
        {
            var result = binding;
            if (binding != null)
            {
                var newInput = VisitExpression(binding.Expression);
                if (!ReferenceEquals(binding.Expression, newInput))
                {
                    result = CqtBuilder.GroupBindAs(newInput, binding.VariableName, binding.GroupVariableName);
                    RebindVariable(binding.Variable, result.Variable);
                    RebindVariable(binding.GroupVariable, result.GroupVariable);
                }
            }
            return result;
        }

        /// <summary>Implements the visitor pattern for the sort clause.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="clause">The sort clause.</param>
        protected virtual DbSortClause VisitSortClause(DbSortClause clause)
        {
            var result = clause;
            if (clause != null)
            {
                var newExpression = VisitExpression(clause.Expression);
                if (!ReferenceEquals(clause.Expression, newExpression))
                {
                    if (!string.IsNullOrEmpty(clause.Collation))
                    {
                        result = (clause.Ascending
                                      ? CqtBuilder.ToSortClause(newExpression, clause.Collation)
                                      : CqtBuilder.ToSortClauseDescending(newExpression, clause.Collation));
                    }
                    else
                    {
                        result = (clause.Ascending
                                      ? CqtBuilder.ToSortClause(newExpression)
                                      : CqtBuilder.ToSortClauseDescending(newExpression));
                    }
                }
            }
            return result;
        }

        /// <summary>Implements the visitor pattern for the sort order.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="sortOrder">The sort order.</param>
        protected virtual IList<DbSortClause> VisitSortOrder(IList<DbSortClause> sortOrder)
        {
            return VisitList(sortOrder, VisitSortClause);
        }

        /// <summary>Implements the visitor pattern for the aggregate.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="aggregate">The aggregate.</param>
        protected virtual DbAggregate VisitAggregate(DbAggregate aggregate)
        {
            // Currently only function or group aggregate are possible
            var functionAggregate = aggregate as DbFunctionAggregate;
            if (functionAggregate != null)
            {
                return VisitFunctionAggregate(functionAggregate);
            }

            var groupAggregate = (DbGroupAggregate)aggregate;
            return VisitGroupAggregate(groupAggregate);
        }

        /// <summary>Implements the visitor pattern for the function aggregate.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="aggregate">The aggregate.</param>
        protected virtual DbFunctionAggregate VisitFunctionAggregate(DbFunctionAggregate aggregate)
        {
            var result = aggregate;
            if (aggregate != null)
            {
                var newFunction = VisitFunction(aggregate.Function);
                var newArguments = VisitExpressionList(aggregate.Arguments);

                Debug.Assert(newArguments.Count == 1, "Function aggregate had more than one argument?");

                if (!ReferenceEquals(aggregate.Function, newFunction)
                    ||
                    !ReferenceEquals(aggregate.Arguments, newArguments))
                {
                    if (aggregate.Distinct)
                    {
                        result = CqtBuilder.AggregateDistinct(newFunction, newArguments[0]);
                    }
                    else
                    {
                        result = CqtBuilder.Aggregate(newFunction, newArguments[0]);
                    }
                }
            }
            return result;
        }

        /// <summary>Implements the visitor pattern for the group aggregate.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="aggregate">The aggregate.</param>
        protected virtual DbGroupAggregate VisitGroupAggregate(DbGroupAggregate aggregate)
        {
            var result = aggregate;
            if (aggregate != null)
            {
                var newArguments = VisitExpressionList(aggregate.Arguments);
                Debug.Assert(newArguments.Count == 1, "Group aggregate had more than one argument?");

                if (!ReferenceEquals(aggregate.Arguments, newArguments))
                {
                    result = CqtBuilder.GroupAggregate(newArguments[0]);
                }
            }
            return result;
        }

        /// <summary>Implements the visitor pattern for the Lambda function.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="lambda">The lambda function.</param>
        protected virtual DbLambda VisitLambda(DbLambda lambda)
        {
            Check.NotNull(lambda, "lambda");

            var result = lambda;
            var newFormals = VisitList(
                lambda.Variables, varRef =>
                    {
                        var newVarType = VisitTypeUsage(varRef.ResultType);
                        if (!ReferenceEquals(varRef.ResultType, newVarType))
                        {
                            return CqtBuilder.Variable(newVarType, varRef.VariableName);
                        }
                        else
                        {
                            return varRef;
                        }
                    }
                );
            EnterScope(newFormals.ToArray()); // ToArray: Don't pass the List instance directly to OnEnterScope
            var newBody = VisitExpression(lambda.Body);
            ExitScope();

            if (!ReferenceEquals(lambda.Variables, newFormals)
                ||
                !ReferenceEquals(lambda.Body, newBody))
            {
                result = CqtBuilder.Lambda(newBody, newFormals);
            }
            return result;
        }

        // Metadata 'Visitor' methods
        /// <summary>Implements the visitor pattern for the type.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="type">The type.</param>
        protected virtual EdmType VisitType(EdmType type)
        {
            return type;
        }

        /// <summary>Implements the visitor pattern for the type usage.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="type">The type.</param>
        protected virtual TypeUsage VisitTypeUsage(TypeUsage type)
        {
            return type;
        }

        /// <summary>Implements the visitor pattern for the entity set.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="entitySet">The entity set.</param>
        protected virtual EntitySetBase VisitEntitySet(EntitySetBase entitySet)
        {
            return entitySet;
        }

        /// <summary>Implements the visitor pattern for the function.</summary>
        /// <returns>The implemented visitor pattern.</returns>
        /// <param name="functionMetadata">The function metadata.</param>
        protected virtual EdmFunction VisitFunction(EdmFunction functionMetadata)
        {
            return functionMetadata;
        }

        #region Private Implementation

        private void NotifyIfChanged(DbExpression originalExpression, DbExpression newExpression)
        {
            if (!ReferenceEquals(originalExpression, newExpression))
            {
                OnExpressionReplaced(originalExpression, newExpression);
            }
        }

        private static IList<TElement> VisitList<TElement>(IList<TElement> list, Func<TElement, TElement> map)
        {
            var result = list;
            if (list != null)
            {
                List<TElement> newList = null;
                for (var idx = 0; idx < list.Count; idx++)
                {
                    var newElement = map(list[idx]);
                    if (newList == null
                        &&
                        !ReferenceEquals(list[idx], newElement))
                    {
                        newList = new List<TElement>(list);
                        result = newList;
                    }

                    if (newList != null)
                    {
                        newList[idx] = newElement;
                    }
                }
            }
            return result;
        }

        private DbExpression VisitUnary(DbUnaryExpression expression, Func<DbExpression, DbExpression> callback)
        {
            DbExpression result = expression;
            var newArgument = VisitExpression(expression.Argument);
            if (!ReferenceEquals(expression.Argument, newArgument))
            {
                result = callback(newArgument);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private DbExpression VisitTypeUnary(
            DbUnaryExpression expression, TypeUsage type, Func<DbExpression, TypeUsage, DbExpression> callback)
        {
            DbExpression result = expression;

            var newArgument = VisitExpression(expression.Argument);
            var newType = VisitTypeUsage(type);

            if (!ReferenceEquals(expression.Argument, newArgument)
                ||
                !ReferenceEquals(type, newType))
            {
                result = callback(newArgument, newType);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private DbExpression VisitBinary(DbBinaryExpression expression, Func<DbExpression, DbExpression, DbExpression> callback)
        {
            DbExpression result = expression;

            var newLeft = VisitExpression(expression.Left);
            var newRight = VisitExpression(expression.Right);
            if (!ReferenceEquals(expression.Left, newLeft)
                ||
                !ReferenceEquals(expression.Right, newRight))
            {
                result = callback(newLeft, newRight);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private DbRelatedEntityRef VisitRelatedEntityRef(DbRelatedEntityRef entityRef)
        {
            RelationshipEndMember newSource;
            RelationshipEndMember newTarget;
            VisitRelationshipEnds(entityRef.SourceEnd, entityRef.TargetEnd, out newSource, out newTarget);
            var newTargetRef = VisitExpression(entityRef.TargetEntityReference);

            if (!ReferenceEquals(entityRef.SourceEnd, newSource)
                ||
                !ReferenceEquals(entityRef.TargetEnd, newTarget)
                ||
                !ReferenceEquals(entityRef.TargetEntityReference, newTargetRef))
            {
                return CqtBuilder.CreateRelatedEntityRef(newSource, newTarget, newTargetRef);
            }
            else
            {
                return entityRef;
            }
        }

        private void VisitRelationshipEnds(
            RelationshipEndMember source, RelationshipEndMember target, out RelationshipEndMember newSource,
            out RelationshipEndMember newTarget)
        {
            Debug.Assert(source.DeclaringType.EdmEquals(target.DeclaringType), "Relationship ends not declared by same relationship type?");
            var mappedType = (RelationshipType)VisitType(target.DeclaringType);

            newSource = mappedType.RelationshipEndMembers[source.Name];
            newTarget = mappedType.RelationshipEndMembers[target.Name];
        }

        private DbExpression VisitTerminal(DbExpression expression, Func<TypeUsage, DbExpression> reconstructor)
        {
            var result = expression;
            var newType = VisitTypeUsage(expression.ResultType);
            if (!ReferenceEquals(expression.ResultType, newType))
            {
                result = reconstructor(newType);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        private void RebindVariable(DbVariableReferenceExpression from, DbVariableReferenceExpression to)
        {
            //
            // The variable is only considered rebound if the name and/or type is different.
            // Otherwise, the original variable reference and the new variable reference are
            // equivalent, and no rebinding of references to the old variable is necessary.
            //
            // When considering the new/old result types,  the TypeUsage instance may be equal
            // or equivalent, but the EdmType must be the same instance, so that expressions
            // such as a DbPropertyExpression with the DbVariableReferenceExpression as the Instance
            // continue to be valid.
            //
            if (!from.VariableName.Equals(to.VariableName, StringComparison.Ordinal)
                ||
                !ReferenceEquals(from.ResultType.EdmType, to.ResultType.EdmType)
                ||
                !from.ResultType.EdmEquals(to.ResultType))
            {
                varMappings[from] = to;
                OnVariableRebound(from, to);
            }
        }

        private DbExpressionBinding VisitExpressionBindingEnterScope(DbExpressionBinding binding)
        {
            var result = VisitExpressionBinding(binding);
            OnEnterScope(new[] { result.Variable });
            return result;
        }

        private void EnterScope(params DbVariableReferenceExpression[] scopeVars)
        {
            OnEnterScope(scopeVars);
        }

        private void ExitScope()
        {
            OnExitScope();
        }

        #endregion

        #region DbExpressionVisitor<DbExpression> Members

        /// <summary>Implements the visitor pattern for the basic functionality required by expression types.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbExpression expression)
        {
            Check.NotNull(expression, "expression");

            throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(expression.GetType().FullName));
        }

        /// <summary>Implements the visitor pattern for the different kinds of constants.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The constant expression.</param>
        public override DbExpression Visit(DbConstantExpression expression)
        {
            Check.NotNull(expression, "expression");

            // Note that it is only safe to call DbConstantExpression.GetValue because the call to
            // DbExpressionBuilder.Constant must clone immutable values (byte[]).
            return VisitTerminal(expression, newType => CqtBuilder.Constant(newType, expression.GetValue()));
        }

        /// <summary>Implements the visitor pattern for a reference to a typed null literal.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitTerminal(expression, CqtBuilder.Null);
        }

        /// <summary>Implements the visitor pattern for a reference to a variable that is currently in scope.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbVariableReferenceExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;
            DbVariableReferenceExpression newRef;
            if (varMappings.TryGetValue(expression, out newRef))
            {
                result = newRef;
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for a reference to a parameter declared on the command tree that contains this expression.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbParameterReferenceExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitTerminal(expression, newType => CqtBuilder.Parameter(newType, expression.ParameterName));
        }

        /// <summary>Implements the visitor pattern for an invocation of a function.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The function expression.</param>
        public override DbExpression Visit(DbFunctionExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;
            var newArguments = VisitExpressionList(expression.Arguments);
            var newFunction = VisitFunction(expression.Function);
            if (!ReferenceEquals(expression.Arguments, newArguments)
                ||
                !ReferenceEquals(expression.Function, newFunction))
            {
                result = CqtBuilder.Invoke(newFunction, newArguments);
            }

            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the application of a lambda function to arguments represented by DbExpression objects.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbLambdaExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;
            var newArguments = VisitExpressionList(expression.Arguments);
            var newLambda = VisitLambda(expression.Lambda);

            if (!ReferenceEquals(expression.Arguments, newArguments)
                ||
                !ReferenceEquals(expression.Lambda, newLambda))
            {
                result = CqtBuilder.Invoke(newLambda, newArguments);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for retrieving an instance property.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbPropertyExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;
            var newInstance = VisitExpression(expression.Instance);
            if (!ReferenceEquals(expression.Instance, newInstance))
            {
                result = CqtBuilder.Property(newInstance, expression.Property.Name);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the comparison operation applied to two arguments.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The cast expression.</param>
        public override DbExpression Visit(DbComparisonExpression expression)
        {
            Check.NotNull(expression, "expression");

            switch (expression.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                    return VisitBinary(expression, CqtBuilder.Equal);

                case DbExpressionKind.NotEquals:
                    return VisitBinary(expression, CqtBuilder.NotEqual);

                case DbExpressionKind.GreaterThan:
                    return VisitBinary(expression, CqtBuilder.GreaterThan);

                case DbExpressionKind.GreaterThanOrEquals:
                    return VisitBinary(expression, CqtBuilder.GreaterThanOrEqual);

                case DbExpressionKind.LessThan:
                    return VisitBinary(expression, CqtBuilder.LessThan);

                case DbExpressionKind.LessThanOrEquals:
                    return VisitBinary(expression, CqtBuilder.LessThanOrEqual);

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>Implements the visitor pattern for a string comparison against the specified pattern with an optional escape string.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbLikeExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newArgument = VisitExpression(expression.Argument);
            var newPattern = VisitExpression(expression.Pattern);
            var newEscape = VisitExpression(expression.Escape);

            if (!ReferenceEquals(expression.Argument, newArgument)
                ||
                !ReferenceEquals(expression.Pattern, newPattern)
                ||
                !ReferenceEquals(expression.Escape, newEscape))
            {
                result = CqtBuilder.Like(newArgument, newPattern, newEscape);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the restriction of the number of elements in the argument collection to the specified limit value.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbLimitExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newArgument = VisitExpression(expression.Argument);
            var newLimit = VisitExpression(expression.Limit);

            if (!ReferenceEquals(expression.Argument, newArgument)
                ||
                !ReferenceEquals(expression.Limit, newLimit))
            {
                Debug.Assert(!expression.WithTies, "Limit.WithTies == true?");
                result = CqtBuilder.Limit(newArgument, newLimit);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the null determination applied to a single argument.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbIsNullExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.IsNull);
        }

        /// <summary>Implements the visitor pattern for the arithmetic operation applied to numeric arguments.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The arithmetic expression.</param>
        public override DbExpression Visit(DbArithmeticExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;
            var newArguments = VisitExpressionList(expression.Arguments);
            if (!ReferenceEquals(expression.Arguments, newArguments))
            {
                switch (expression.ExpressionKind)
                {
                    case DbExpressionKind.Divide:
                        result = CqtBuilder.Divide(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Minus:
                        result = CqtBuilder.Minus(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Modulo:
                        result = CqtBuilder.Modulo(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Multiply:
                        result = CqtBuilder.Multiply(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.Plus:
                        result = CqtBuilder.Plus(newArguments[0], newArguments[1]);
                        break;

                    case DbExpressionKind.UnaryMinus:
                        result = CqtBuilder.UnaryMinus(newArguments[0]);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the logical AND expression.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The logical AND expression.</param>
        public override DbExpression Visit(DbAndExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.And);
        }

        /// <summary>Implements the visitor pattern for the logical OR of two Boolean arguments.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbOrExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.Or);
        }

        /// <summary>Implements the visitor pattern for the DbInExpression.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The DbInExpression that is being visited.</param>
        public override DbExpression Visit(DbInExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;
            var newItem = VisitExpression(expression.Item);
            var newList = VisitExpressionList(expression.List);

            if (!ReferenceEquals(expression.Item, newItem)
                ||
                !ReferenceEquals(expression.List, newList))
            {
                result = CqtBuilder.CreateInExpression(newItem, newList);
            }

            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the logical NOT of a single Boolean argument.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbNotExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.Not);
        }

        /// <summary>Implements the visitor pattern for the removed duplicate elements from the specified set argument.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The distinct expression.</param>
        public override DbExpression Visit(DbDistinctExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.Distinct);
        }

        /// <summary>Implements the visitor pattern for the conversion of the specified set argument to a singleton the conversion of the specified set argument to a singleton.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The element expression.</param>
        public override DbExpression Visit(DbElementExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(
                expression, expression.IsSinglePropertyUnwrapped
                                ? (Func<DbExpression, DbExpression>)CqtBuilder.CreateElementExpressionUnwrapSingleProperty
                                : CqtBuilder.Element);
        }

        /// <summary>Implements the visitor pattern for an empty set determination applied to a single set argument.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbIsEmptyExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.IsEmpty);
        }

        /// <summary>Implements the visitor pattern for the set union operation between the left and right operands.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbUnionAllExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.UnionAll);
        }

        /// <summary>Implements the visitor pattern for the set intersection operation between the left and right operands.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbIntersectExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.Intersect);
        }

        /// <summary>Implements the visitor pattern for the set subtraction operation between the left and right operands.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbExceptExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitBinary(expression, CqtBuilder.Except);
        }

        /// <summary>Implements the visitor pattern for a type conversion operation applied to a polymorphic argument.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbTreatExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitTypeUnary(expression, expression.ResultType, CqtBuilder.TreatAs);
        }

        /// <summary>Implements the visitor pattern for the type comparison of a single argument against the specified type.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbIsOfExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.ExpressionKind
                == DbExpressionKind.IsOfOnly)
            {
                return VisitTypeUnary(expression, expression.OfType, CqtBuilder.IsOfOnly);
            }
            else
            {
                return VisitTypeUnary(expression, expression.OfType, CqtBuilder.IsOf);
            }
        }

        /// <summary>Implements the visitor pattern for the type conversion of a single argument to the specified type.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The cast expression.</param>
        public override DbExpression Visit(DbCastExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitTypeUnary(expression, expression.ResultType, CqtBuilder.CastTo);
        }

        /// <summary>Implements the visitor pattern for the When, Then, and Else clauses.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The case expression.</param>
        public override DbExpression Visit(DbCaseExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newWhens = VisitExpressionList(expression.When);
            var newThens = VisitExpressionList(expression.Then);
            var newElse = VisitExpression(expression.Else);

            if (!ReferenceEquals(expression.When, newWhens)
                ||
                !ReferenceEquals(expression.Then, newThens)
                ||
                !ReferenceEquals(expression.Else, newElse))
            {
                result = CqtBuilder.Case(newWhens, newThens, newElse);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the retrieval of elements of the specified type from the given set argument.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbOfTypeExpression expression)
        {
            Check.NotNull(expression, "expression");

            if (expression.ExpressionKind
                == DbExpressionKind.OfTypeOnly)
            {
                return VisitTypeUnary(expression, expression.OfType, CqtBuilder.OfTypeOnly);
            }
            else
            {
                return VisitTypeUnary(expression, expression.OfType, CqtBuilder.OfType);
            }
        }

        /// <summary>Implements the visitor pattern for the construction of a new instance of a given type, including set and record types.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbNewInstanceExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;
            var newType = VisitTypeUsage(expression.ResultType);
            var newArguments = VisitExpressionList(expression.Arguments);
            var unchanged = (ReferenceEquals(expression.ResultType, newType) && ReferenceEquals(expression.Arguments, newArguments));
            if (expression.HasRelatedEntityReferences)
            {
                var newRefs = VisitList(expression.RelatedEntityReferences, VisitRelatedEntityRef);
                if (!unchanged
                    ||
                    !ReferenceEquals(expression.RelatedEntityReferences, newRefs))
                {
                    result = CqtBuilder.CreateNewEntityWithRelationshipsExpression((EntityType)newType.EdmType, newArguments, newRefs);
                }
            }
            else
            {
                if (!unchanged)
                {
                    result = CqtBuilder.New(newType, newArguments.ToArray());
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for a strongly typed reference to a specific instance within an entity set.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbRefExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var targetType = (EntityType)TypeHelpers.GetEdmType<RefType>(expression.ResultType).ElementType;

            var newArgument = VisitExpression(expression.Argument);
            var newType = (EntityType)VisitType(targetType);
            var newSet = (EntitySet)VisitEntitySet(expression.EntitySet);
            if (!ReferenceEquals(expression.Argument, newArgument)
                ||
                !ReferenceEquals(targetType, newType)
                ||
                !ReferenceEquals(expression.EntitySet, newSet))
            {
                result = CqtBuilder.RefFromKey(newSet, newArgument, newType);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the navigation of a relationship.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbRelationshipNavigationExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            RelationshipEndMember newFrom;
            RelationshipEndMember newTo;
            VisitRelationshipEnds(expression.NavigateFrom, expression.NavigateTo, out newFrom, out newTo);
            var newNavSource = VisitExpression(expression.NavigationSource);

            if (!ReferenceEquals(expression.NavigateFrom, newFrom)
                ||
                !ReferenceEquals(expression.NavigateTo, newTo)
                ||
                !ReferenceEquals(expression.NavigationSource, newNavSource))
            {
                result = CqtBuilder.Navigate(newNavSource, newFrom, newTo);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the expression that retrieves an entity based on the specified reference.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The DEREF expression.</param>
        public override DbExpression Visit(DbDerefExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.Deref);
        }

        /// <summary>Implements the visitor pattern for the retrieval of the key value from the underlying reference value.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbRefKeyExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.GetRefKey);
        }

        /// <summary>Implements the visitor pattern for the expression that extracts a reference from the underlying entity instance.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The entity reference expression.</param>
        public override DbExpression Visit(DbEntityRefExpression expression)
        {
            Check.NotNull(expression, "expression");

            return VisitUnary(expression, CqtBuilder.GetEntityRef);
        }

        /// <summary>Implements the visitor pattern for a scan over an entity set or relationship set, as indicated by the Target property.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbScanExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newSet = VisitEntitySet(expression.Target);
            if (!ReferenceEquals(expression.Target, newSet))
            {
                result = CqtBuilder.Scan(newSet);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for a predicate applied to filter an input set.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The filter expression.</param>
        public override DbExpression Visit(DbFilterExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var input = VisitExpressionBindingEnterScope(expression.Input);
            var predicate = VisitExpression(expression.Predicate);
            ExitScope();
            if (!ReferenceEquals(expression.Input, input)
                ||
                !ReferenceEquals(expression.Predicate, predicate))
            {
                result = CqtBuilder.Filter(input, predicate);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the projection of a given input set over the specified expression.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbProjectExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var input = VisitExpressionBindingEnterScope(expression.Input);
            var projection = VisitExpression(expression.Projection);
            ExitScope();
            if (!ReferenceEquals(expression.Input, input)
                ||
                !ReferenceEquals(expression.Projection, projection))
            {
                result = CqtBuilder.Project(input, projection);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the unconditional join operation between the given collection arguments.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The join expression.</param>
        public override DbExpression Visit(DbCrossJoinExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newInputs = VisitExpressionBindingList(expression.Inputs);
            if (!ReferenceEquals(expression.Inputs, newInputs))
            {
                result = CqtBuilder.CrossJoin(newInputs);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for an inner, left outer, or full outer join operation between the given collection arguments on the specified join condition.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbJoinExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newLeft = VisitExpressionBinding(expression.Left);
            var newRight = VisitExpressionBinding(expression.Right);

            EnterScope(newLeft.Variable, newRight.Variable);
            var newCondition = VisitExpression(expression.JoinCondition);
            ExitScope();

            if (!ReferenceEquals(expression.Left, newLeft)
                ||
                !ReferenceEquals(expression.Right, newRight)
                ||
                !ReferenceEquals(expression.JoinCondition, newCondition))
            {
                if (DbExpressionKind.InnerJoin
                    == expression.ExpressionKind)
                {
                    result = CqtBuilder.InnerJoin(newLeft, newRight, newCondition);
                }
                else if (DbExpressionKind.LeftOuterJoin
                         == expression.ExpressionKind)
                {
                    result = CqtBuilder.LeftOuterJoin(newLeft, newRight, newCondition);
                }
                else
                {
                    Debug.Assert(
                        expression.ExpressionKind == DbExpressionKind.FullOuterJoin,
                        "DbJoinExpression had ExpressionKind other than InnerJoin, LeftOuterJoin or FullOuterJoin?");
                    result = CqtBuilder.FullOuterJoin(newLeft, newRight, newCondition);
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the invocation of the specified function for each element in the specified input set.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The APPLY expression.</param>
        public override DbExpression Visit(DbApplyExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newInput = VisitExpressionBindingEnterScope(expression.Input);
            var newApply = VisitExpressionBinding(expression.Apply);
            ExitScope();

            if (!ReferenceEquals(expression.Input, newInput)
                ||
                !ReferenceEquals(expression.Apply, newApply))
            {
                if (DbExpressionKind.CrossApply
                    == expression.ExpressionKind)
                {
                    result = CqtBuilder.CrossApply(newInput, newApply);
                }
                else
                {
                    Debug.Assert(
                        expression.ExpressionKind == DbExpressionKind.OuterApply,
                        "DbApplyExpression had ExpressionKind other than CrossApply or OuterApply?");
                    result = CqtBuilder.OuterApply(newInput, newApply);
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for a group by operation.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbGroupByExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newInput = VisitGroupExpressionBinding(expression.Input);
            EnterScope(newInput.Variable);
            var newKeys = VisitExpressionList(expression.Keys);
            ExitScope();
            EnterScope(newInput.GroupVariable);
            var newAggs = VisitList(expression.Aggregates, VisitAggregate);
            ExitScope();

            if (!ReferenceEquals(expression.Input, newInput)
                ||
                !ReferenceEquals(expression.Keys, newKeys)
                ||
                !ReferenceEquals(expression.Aggregates, newAggs))
            {
                var groupOutput =
                    TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(expression.ResultType).TypeUsage);

                var boundKeys = groupOutput.Properties.Take(newKeys.Count).Select(p => p.Name).Zip(newKeys).ToList();
                var boundAggs = groupOutput.Properties.Skip(newKeys.Count).Select(p => p.Name).Zip(newAggs).ToList();

                result = CqtBuilder.GroupBy(newInput, boundKeys, boundAggs);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for the skip expression.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbSkipExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newInput = VisitExpressionBindingEnterScope(expression.Input);
            var newSortOrder = VisitSortOrder(expression.SortOrder);
            ExitScope();
            var newCount = VisitExpression(expression.Count);

            if (!ReferenceEquals(expression.Input, newInput)
                ||
                !ReferenceEquals(expression.SortOrder, newSortOrder)
                ||
                !ReferenceEquals(expression.Count, newCount))
            {
                result = CqtBuilder.Skip(newInput, newSortOrder, newCount);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for a sort key that can be used as part of the sort order.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbSortExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var newInput = VisitExpressionBindingEnterScope(expression.Input);
            var newSortOrder = VisitSortOrder(expression.SortOrder);
            ExitScope();

            if (!ReferenceEquals(expression.Input, newInput)
                ||
                !ReferenceEquals(expression.SortOrder, newSortOrder))
            {
                result = CqtBuilder.Sort(newInput, newSortOrder);
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        /// <summary>Implements the visitor pattern for a quantifier operation of the specified kind over the elements of the specified input set.</summary>
        /// <returns>The implemented visitor.</returns>
        /// <param name="expression">The expression.</param>
        public override DbExpression Visit(DbQuantifierExpression expression)
        {
            Check.NotNull(expression, "expression");

            DbExpression result = expression;

            var input = VisitExpressionBindingEnterScope(expression.Input);
            var predicate = VisitExpression(expression.Predicate);
            ExitScope();

            if (!ReferenceEquals(expression.Input, input)
                ||
                !ReferenceEquals(expression.Predicate, predicate))
            {
                if (DbExpressionKind.All
                    == expression.ExpressionKind)
                {
                    result = CqtBuilder.All(input, predicate);
                }
                else
                {
                    Debug.Assert(
                        expression.ExpressionKind == DbExpressionKind.Any,
                        "DbQuantifierExpression had ExpressionKind other than All or Any?");
                    result = CqtBuilder.Any(input, predicate);
                }
            }
            NotifyIfChanged(expression, result);
            return result;
        }

        #endregion
    }
}
