// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Internal;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     Provides an API to construct <see cref="DbExpression" />s and allows that API to be accessed as extension methods on the expression type itself.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public static class DbExpressionBuilder
    {
        #region Private Implementation

        private static readonly AliasGenerator _bindingAliases = new AliasGenerator("Var_", 0);

        private static readonly DbNullExpression _binaryNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Binary));

        private static readonly DbNullExpression _boolNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Boolean));

        private static readonly DbNullExpression _byteNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Byte));

        private static readonly DbNullExpression _dateTimeNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.DateTime));

        private static readonly DbNullExpression _dateTimeOffsetNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.DateTimeOffset));

        private static readonly DbNullExpression _decimalNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Decimal));

        private static readonly DbNullExpression _doubleNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Double));

        private static readonly DbNullExpression _geographyNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Geography));

        private static readonly DbNullExpression _geometryNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Geometry));

        private static readonly DbNullExpression _guidNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Guid));

        private static readonly DbNullExpression _int16Null =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int16));

        private static readonly DbNullExpression _int32Null =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int32));

        private static readonly DbNullExpression _int64Null =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Int64));

        private static readonly DbNullExpression _sbyteNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.SByte));

        private static readonly DbNullExpression _singleNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Single));

        private static readonly DbNullExpression _stringNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.String));

        private static readonly DbNullExpression _timeNull =
            Null(EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(PrimitiveTypeKind.Time));

        private static readonly DbConstantExpression _boolTrue = Constant(true);
        private static readonly DbConstantExpression _boolFalse = Constant(false);

        #endregion

        #region Helpers (not strictly Command Tree API)

        public static KeyValuePair<string, DbExpression> As(this DbExpression value, string alias)
        {
            return new KeyValuePair<string, DbExpression>(alias, value);
        }

        public static KeyValuePair<string, DbAggregate> As(this DbAggregate value, string alias)
        {
            return new KeyValuePair<string, DbAggregate>(alias, value);
        }

        #endregion

        #region Bindings - Expression and Group

        /// <summary>
        ///     Creates a new <see cref="DbExpressionBinding" /> that uses a generated variable name to bind the given expression
        /// </summary>
        /// <param name="input"> The expression to bind </param>
        /// <returns> A new expression binding with the specified expression and a generated variable name </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="input" />
        ///     does not have a collection result type
        /// </exception>
        public static DbExpressionBinding Bind(this DbExpression input)
        {
            Check.NotNull(input, "input");

            return input.BindAs(_bindingAliases.Next());
        }

        /// <summary>
        ///     Creates a new <see cref="DbExpressionBinding" /> that uses the specified variable name to bind the given expression
        /// </summary>
        /// <param name="input"> The expression to bind </param>
        /// <param name="varName"> The variable name that should be used for the binding </param>
        /// <returns> A new expression binding with the specified expression and variable name </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="varName" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="input" />
        ///     does not have a collection result type
        /// </exception>
        public static DbExpressionBinding BindAs(this DbExpression input, string varName)
        {
            Check.NotNull(input, "input");
            Check.NotNull(varName, "varName");

            var elementType = ArgumentValidation.ValidateBindAs(input, varName);
            var inputRef = new DbVariableReferenceExpression(elementType, varName);
            return new DbExpressionBinding(input, inputRef);
        }

        /// <summary>
        ///     Creates a new group expression binding that uses generated variable and group variable names to bind the given expression
        /// </summary>
        /// <param name="input"> The expression to bind </param>
        /// <returns> A new group expression binding with the specified expression and a generated variable name and group variable name </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="input" />
        ///     does not have a collection result type
        /// </exception>
        public static DbGroupExpressionBinding GroupBind(this DbExpression input)
        {
            Check.NotNull(input, "input");

            var alias = _bindingAliases.Next();
            return input.GroupBindAs(alias, string.Format(CultureInfo.InvariantCulture, "Group{0}", alias));
        }

        /// <summary>
        ///     Creates a new <see cref="DbGroupExpressionBinding" /> that uses the specified variable name and group variable names to bind the given expression
        /// </summary>
        /// <param name="input"> The expression to bind </param>
        /// <param name="varName"> The variable name that should be used for the binding </param>
        /// <param name="groupVarName"> The variable name that should be used to refer to the group when the new group expression binding is used in a group-by expression </param>
        /// <returns> A new group expression binding with the specified expression, variable name and group variable name </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     ,
        ///     <paramref name="varName" />
        ///     or
        ///     <paramref name="groupVarName" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="input" />
        ///     does not have a collection result type
        /// </exception>
        public static DbGroupExpressionBinding GroupBindAs(this DbExpression input, string varName, string groupVarName)
        {
            Check.NotNull(input, "input");
            Check.NotNull(varName, "varName");
            Check.NotNull(groupVarName, "groupVarName");

            var elementType = ArgumentValidation.ValidateGroupBindAs(input, varName, groupVarName);
            var inputRef = new DbVariableReferenceExpression(elementType, varName);
            var groupRef = new DbVariableReferenceExpression(elementType, groupVarName);
            return new DbGroupExpressionBinding(input, inputRef, groupRef);
        }

        #endregion

        #region Aggregates and SortClauses are required only for Binding-based method support - replaced by OrderBy[Descending]/ThenBy[Descending] and Aggregate[Distinct] methods in new API

        /// <summary>
        ///     Creates a new <see cref="DbFunctionAggregate" />.
        /// </summary>
        /// <param name="function"> The function that defines the aggregate operation. </param>
        /// <param name="argument"> The argument over which the aggregate function should be calculated. </param>
        /// <returns> A new function aggregate with a reference to the given function and argument. The function aggregate's Distinct property will have the value false </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="function" />
        ///     or
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="function" />
        ///     is not an aggregate function or has more than one argument, or
        ///     the result type of
        ///     <paramref name="argument" />
        ///     is not equal or promotable to
        ///     the parameter type of
        ///     <paramref name="function" />
        /// </exception>
        public static DbFunctionAggregate Aggregate(this EdmFunction function, DbExpression argument)
        {
            Check.NotNull(function, "function");
            Check.NotNull(argument, "argument");

            return CreateFunctionAggregate(function, argument, false);
        }

        /// <summary>
        ///     Creates a new <see cref="DbFunctionAggregate" /> that is applied in a distinct fashion.
        /// </summary>
        /// <param name="function"> The function that defines the aggregate operation. </param>
        /// <param name="argument"> The argument over which the aggregate function should be calculated. </param>
        /// <returns> A new function aggregate with a reference to the given function and argument. The function aggregate's Distinct property will have the value true </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="function" />
        ///     or
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="function" />
        ///     is not an aggregate function or has more than one argument, or
        ///     the result type of
        ///     <paramref name="argument" />
        ///     is not equal or promotable to
        ///     the parameter type of
        ///     <paramref name="function" />
        /// </exception>
        public static DbFunctionAggregate AggregateDistinct(this EdmFunction function, DbExpression argument)
        {
            Check.NotNull(function, "function");
            Check.NotNull(argument, "argument");

            return CreateFunctionAggregate(function, argument, true);
        }

        private static DbFunctionAggregate CreateFunctionAggregate(EdmFunction function, DbExpression argument, bool isDistinct)
        {
            var funcArgs = ArgumentValidation.ValidateFunctionAggregate(function, new[] { argument });
            var resultType = function.ReturnParameter.TypeUsage;
            return new DbFunctionAggregate(resultType, funcArgs, function, isDistinct);
        }

        /// <summary>
        ///     Creates a new <see cref="DbGroupAggregate" /> over the specified argument
        /// </summary>
        /// <param name="argument"> The argument over which to perform the nest operation </param>
        /// <returns> A new group aggregate representing the elements of the group referenced by the given argument. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /*ENABLE_ELEMENT_SELECTOR(*/
        internal /*)*/ static DbGroupAggregate GroupAggregate(DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var arguments = new DbExpressionList(new[] { argument }); ;
            var resultType = TypeHelpers.CreateCollectionTypeUsage(argument.ResultType);
            return new DbGroupAggregate(resultType, arguments);
        }

        /// <summary>
        ///     Creates a <see cref="DbLambda" /> with the specified inline Lambda function implementation and formal parameters.
        /// </summary>
        /// <param name="body"> An expression that defines the logic of the Lambda function </param>
        /// <param name="variables">
        ///     A <see cref="DbVariableReferenceExpression" /> collection that represents the formal parameters to the Lambda function. These variables are valid for use in the
        ///     <paramref
        ///         name="body" />
        ///     expression.
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="variables" />
        ///     is null or contains null, or
        ///     <paramref name="body" />
        ///     is null
        /// </exception>
        /// .
        /// <exception cref="ArgumentException">
        ///     <paramref name="variables" />
        ///     contains more than one element with the same variable name.
        /// </exception>
        public static DbLambda Lambda(DbExpression body, IEnumerable<DbVariableReferenceExpression> variables)
        {
            Check.NotNull(body, "body");

            return CreateLambda(body, variables);
        }

        /// <summary>
        ///     Creates a <see cref="DbLambda" /> with the specified inline Lambda function implementation and formal parameters.
        /// </summary>
        /// <param name="body"> An expression that defines the logic of the Lambda function </param>
        /// <param name="variables">
        ///     A <see cref="DbVariableReferenceExpression" /> collection that represents the formal parameters to the Lambda function. These variables are valid for use in the
        ///     <paramref
        ///         name="body" />
        ///     expression.
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="variables" />
        ///     is null or contains null, or
        ///     <paramref name="body" />
        ///     is null
        /// </exception>
        /// .
        /// <exception cref="ArgumentException">
        ///     <paramref name="variables" />
        ///     contains more than one element with the same variable name.
        /// </exception>
        public static DbLambda Lambda(DbExpression body, params DbVariableReferenceExpression[] variables)
        {
            Check.NotNull(body, "body");

            return CreateLambda(body, variables);
        }

        private static DbLambda CreateLambda(DbExpression body, IEnumerable<DbVariableReferenceExpression> variables)
        {
            var validVars = ArgumentValidation.ValidateLambda(variables);
            return new DbLambda(validVars, body);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortClause" /> with an ascending sort order and default collation
        /// </summary>
        /// <param name="key"> The expression that defines the sort key </param>
        /// <returns> A new sort clause with the given sort key and ascending sort order </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="key" />
        ///     does not have an order-comparable result type
        /// </exception>
        public static DbSortClause ToSortClause(this DbExpression key)
        {
            Check.NotNull(key, "key");

            ArgumentValidation.ValidateSortClause(key);
            return new DbSortClause(key, true, String.Empty);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortClause" /> with a descending sort order and default collation
        /// </summary>
        /// <param name="key"> The expression that defines the sort key </param>
        /// <returns> A new sort clause with the given sort key and descending sort order </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="key" />
        ///     does not have an order-comparable result type
        /// </exception>
        public static DbSortClause ToSortClauseDescending(this DbExpression key)
        {
            Check.NotNull(key, "key");

            ArgumentValidation.ValidateSortClause(key);
            return new DbSortClause(key, false, String.Empty);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortClause" /> with an ascending sort order and the specified collation
        /// </summary>
        /// <param name="key"> The expression that defines the sort key </param>
        /// <param name="collation"> The collation to sort under </param>
        /// <returns> A new sort clause with the given sort key and collation, and ascending sort order </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="collation" />
        ///     is empty or contains only space characters
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="key" />
        ///     does not have an order-comparable result type
        /// </exception>
        public static DbSortClause ToSortClause(this DbExpression key, string collation)
        {
            Check.NotNull(key, "key");
            Check.NotNull(collation, "collation");

            ArgumentValidation.ValidateSortClause(key, collation);
            return new DbSortClause(key, true, collation);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortClause" /> with a descending sort order and the specified collation
        /// </summary>
        /// <param name="key"> The expression that defines the sort key </param>
        /// <param name="collation"> The collation to sort under </param>
        /// <returns> A new sort clause with the given sort key and collation, and descending sort order </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="key" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="collation" />
        ///     is empty or contains only space characters
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="key" />
        ///     does not have an order-comparable result type
        /// </exception>
        public static DbSortClause ToSortClauseDescending(this DbExpression key, string collation)
        {
            Check.NotNull(key, "key");
            Check.NotNull(collation, "collation");

            ArgumentValidation.ValidateSortClause(key, collation);
            return new DbSortClause(key, false, collation);
        }

        #endregion

        #region Binding-based methods: All, Any, Cross|OuterApply, Cross|FullOuter|Inner|LeftOuterJoin, Filter, GroupBy, Project, Skip, Sort

        /// <summary>
        ///     Creates a new <see cref="DbQuantifierExpression" /> that determines whether the given predicate holds for all elements of the input set.
        /// </summary>
        /// <param name="input"> An expression binding that specifies the input set. </param>
        /// <param name="predicate"> An expression representing a predicate to evaluate for each member of the input set. </param>
        /// <returns> A new DbQuantifierExpression that represents the All operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="predicate" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbQuantifierExpression All(this DbExpressionBinding input, DbExpression predicate)
        {
            Check.NotNull(predicate, "predicate");
            Check.NotNull(input, "input");

            var booleanResultType = ArgumentValidation.ValidateQuantifier(predicate);
            return new DbQuantifierExpression(DbExpressionKind.All, booleanResultType, input, predicate);
        }

        /// <summary>
        ///     Creates a new <see cref="DbQuantifierExpression" /> that determines whether the given predicate holds for any element of the input set.
        /// </summary>
        /// <param name="input"> An expression binding that specifies the input set. </param>
        /// <param name="predicate"> An expression representing a predicate to evaluate for each member of the input set. </param>
        /// <returns> A new DbQuantifierExpression that represents the Any operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="predicate" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbQuantifierExpression Any(this DbExpressionBinding input, DbExpression predicate)
        {
            Check.NotNull(predicate, "predicate");
            Check.NotNull(input, "input");

            var booleanResultType = ArgumentValidation.ValidateQuantifier(predicate);
            return new DbQuantifierExpression(DbExpressionKind.Any, booleanResultType, input, predicate);
        }

        /// <summary>
        ///     Creates a new <see cref="DbApplyExpression" /> that evaluates the given <paramref name="apply" /> expression once for each element of a given input set,
        ///     producing a collection of rows with corresponding input and apply columns. Rows for which <paramref name="apply" /> evaluates to an empty set are not included.
        /// </summary>
        /// <param name="input">
        ///     An <see cref="DbExpressionBinding" /> that specifies the input set.
        /// </param>
        /// <param name="apply">
        ///     An <see cref="DbExpressionBinding" /> that specifies logic to evaluate once for each member of the input set.
        /// </param>
        /// <returns>
        ///     An new DbApplyExpression with the specified input and apply bindings and an <see cref="DbExpressionKind" /> of CrossApply.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="apply" />
        ///     is null
        /// </exception>
        public static DbApplyExpression CrossApply(this DbExpressionBinding input, DbExpressionBinding apply)
        {
            Check.NotNull(input, "input");
            Check.NotNull(apply, "apply");

            var resultType = ArgumentValidation.ValidateApply(input, apply);
            return new DbApplyExpression(DbExpressionKind.CrossApply, resultType, input, apply);
        }

        /// <summary>
        ///     Creates a new <see cref="DbApplyExpression" /> that evaluates the given <paramref name="apply" /> expression once for each element of a given input set,
        ///     producing a collection of rows with corresponding input and apply columns. Rows for which <paramref name="apply" /> evaluates to an empty set have an apply column value of <code>null</code>.
        /// </summary>
        /// <param name="input">
        ///     An <see cref="DbExpressionBinding" /> that specifies the input set.
        /// </param>
        /// <param name="apply">
        ///     An <see cref="DbExpressionBinding" /> that specifies logic to evaluate once for each member of the input set.
        /// </param>
        /// <returns>
        ///     An new DbApplyExpression with the specified input and apply bindings and an <see cref="DbExpressionKind" /> of OuterApply.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="apply" />
        ///     is null
        /// </exception>
        public static DbApplyExpression OuterApply(this DbExpressionBinding input, DbExpressionBinding apply)
        {
            Check.NotNull(input, "input");
            Check.NotNull(apply, "apply");

            var resultType = ArgumentValidation.ValidateApply(input, apply);
            return new DbApplyExpression(DbExpressionKind.OuterApply, resultType, input, apply);
        }

        /// <summary>
        ///     Creates a new <see cref="DbCrossJoinExpression" /> that unconditionally joins the sets specified by the list of input expression bindings.
        /// </summary>
        /// <param name="inputs"> A list of expression bindings that specifies the input sets. </param>
        /// <returns>
        ///     A new DbCrossJoinExpression, with an <see cref="DbExpressionKind" /> of CrossJoin, that represents the unconditional join of the input sets.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="inputs" />
        ///     is null or contains null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="inputs" />
        ///     contains fewer than 2 expression bindings.
        /// </exception>
        public static DbCrossJoinExpression CrossJoin(IEnumerable<DbExpressionBinding> inputs)
        {
            Check.NotNull(inputs, "inputs");

            TypeUsage resultType;
            var validInputs = ArgumentValidation.ValidateCrossJoin(inputs, out resultType);
            return new DbCrossJoinExpression(resultType, validInputs);
        }

        /// <summary>
        ///     Creates a new <see cref="DbJoinExpression" /> that joins the sets specified by the left and right
        ///     expression bindings, on the specified join condition, using InnerJoin as the <see cref="DbExpressionKind" />.
        /// </summary>
        /// <param name="left">
        ///     An <see cref="DbExpressionBinding" /> that specifies the left set argument.
        /// </param>
        /// <param name="right">
        ///     An <see cref="DbExpressionBinding" /> that specifies the right set argument.
        /// </param>
        /// <param name="joinCondition"> An expression that specifies the condition on which to join. </param>
        /// <returns>
        ///     A new DbJoinExpression, with an <see cref="DbExpressionKind" /> of InnerJoin, that represents the inner join operation applied to the left and right input sets under the given join condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     ,
        ///     <paramref name="right" />
        ///     or
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="joinCondition" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbJoinExpression InnerJoin(this DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(joinCondition, "joinCondition");

            var resultType = ArgumentValidation.ValidateJoin(left, right, joinCondition);
            return new DbJoinExpression(DbExpressionKind.InnerJoin, resultType, left, right, joinCondition);
        }

        /// <summary>
        ///     Creates a new <see cref="DbJoinExpression" /> that joins the sets specified by the left and right
        ///     expression bindings, on the specified join condition, using LeftOuterJoin as the <see cref="DbExpressionKind" />.
        /// </summary>
        /// <param name="left">
        ///     An <see cref="DbExpressionBinding" /> that specifies the left set argument.
        /// </param>
        /// <param name="right">
        ///     An <see cref="DbExpressionBinding" /> that specifies the right set argument.
        /// </param>
        /// <param name="joinCondition"> An expression that specifies the condition on which to join. </param>
        /// <returns>
        ///     A new DbJoinExpression, with an <see cref="DbExpressionKind" /> of LeftOuterJoin, that represents the left outer join operation applied to the left and right input sets under the given join condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     ,
        ///     <paramref name="right" />
        ///     or
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="joinCondition" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbJoinExpression LeftOuterJoin(this DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(joinCondition, "joinCondition");

            var resultType = ArgumentValidation.ValidateJoin(left, right, joinCondition);
            return new DbJoinExpression(DbExpressionKind.LeftOuterJoin, resultType, left, right, joinCondition);
        }

        /// <summary>
        ///     Creates a new <see cref="DbJoinExpression" /> that joins the sets specified by the left and right
        ///     expression bindings, on the specified join condition, using FullOuterJoin as the <see cref="DbExpressionKind" />.
        /// </summary>
        /// <param name="left">
        ///     An <see cref="DbExpressionBinding" /> that specifies the left set argument.
        /// </param>
        /// <param name="right">
        ///     An <see cref="DbExpressionBinding" /> that specifies the right set argument.
        /// </param>
        /// <param name="joinCondition"> An expression that specifies the condition on which to join. </param>
        /// <returns>
        ///     A new DbJoinExpression, with an <see cref="DbExpressionKind" /> of FullOuterJoin, that represents the full outer join operation applied to the left and right input sets under the given join condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     ,
        ///     <paramref name="right" />
        ///     or
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="joinCondition" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbJoinExpression FullOuterJoin(this DbExpressionBinding left, DbExpressionBinding right, DbExpression joinCondition)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(joinCondition, "joinCondition");

            var resultType = ArgumentValidation.ValidateJoin(left, right, joinCondition);
            return new DbJoinExpression(DbExpressionKind.FullOuterJoin, resultType, left, right, joinCondition);
        }

        /// <summary>
        ///     Creates a new <see cref="DbFilterExpression" /> that filters the elements in the given input set using the specified predicate.
        /// </summary>
        /// <param name="input"> An expression binding that specifies the input set. </param>
        /// <param name="predicate"> An expression representing a predicate to evaluate for each member of the input set. </param>
        /// <returns> A new DbFilterExpression that produces the filtered set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="predicate" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbFilterExpression Filter(this DbExpressionBinding input, DbExpression predicate)
        {
            Check.NotNull(input, "input");
            Check.NotNull(predicate, "predicate");

            var resultType = ArgumentValidation.ValidateFilter(input, predicate);
            return new DbFilterExpression(resultType, input, predicate);
        }

        /// <summary>
        ///     Creates a new <see cref="DbGroupByExpression" /> that groups the elements of the input set according to the specified group keys and applies the given aggregates.
        /// </summary>
        /// <param name="input">
        ///     A <see cref="DbGroupExpressionBinding" /> that specifies the input set.
        /// </param>
        /// <param name="keys"> A list of string-expression pairs that define the grouping columns. </param>
        /// <param name="aggregates"> A list of expressions that specify aggregates to apply. </param>
        /// <returns> A new DbGroupByExpression with the specified input set, grouping keys and aggregates. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     ,
        ///     <paramref name="keys" />
        ///     or
        ///     <paramref name="aggregates" />
        ///     is null,
        ///     <paramref name="keys" />
        ///     contains a null key column name or expression, or
        ///     <paramref name="aggregates" />
        ///     contains a null aggregate column name or aggregate.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Both
        ///     <paramref name="keys" />
        ///     and
        ///     <paramref name="aggregates" />
        ///     are empty,
        ///     or an invalid or duplicate column name was specified.
        /// </exception>
        /// <remarks>
        ///     DbGroupByExpression allows either the list of keys or the list of aggregates to be empty, but not both.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DbGroupByExpression GroupBy(
            this DbGroupExpressionBinding input, IEnumerable<KeyValuePair<string, DbExpression>> keys,
            IEnumerable<KeyValuePair<string, DbAggregate>> aggregates)
        {
            Check.NotNull(input, "input");

            DbExpressionList validKeys;
            ReadOnlyCollection<DbAggregate> validAggregates;
            var resultType = ArgumentValidation.ValidateGroupBy(keys, aggregates, out validKeys, out validAggregates);
            return new DbGroupByExpression(resultType, input, validKeys, validAggregates);
        }

        /// <summary>
        ///     Creates a new <see cref="DbProjectExpression" /> that projects the specified expression over the given input set.
        /// </summary>
        /// <param name="input"> An expression binding that specifies the input set. </param>
        /// <param name="projection"> An expression to project over the set. </param>
        /// <returns> A new DbProjectExpression that represents the projection operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="projection" />
        ///     is null
        /// </exception>
        public static DbProjectExpression Project(this DbExpressionBinding input, DbExpression projection)
        {
            Check.NotNull(projection, "projection");
            Check.NotNull(input, "input");

            var resultType = ArgumentValidation.ValidateProject(projection);
            return new DbProjectExpression(resultType, input, projection);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSkipExpression" /> that sorts the given input set by the given sort specifications before skipping the specified number of elements.
        /// </summary>
        /// <param name="input"> An expression binding that specifies the input set. </param>
        /// <param name="sortOrder"> A list of sort specifications that determine how the elements of the input set should be sorted. </param>
        /// <param name="count"> An expression the specifies how many elements of the ordered set to skip. </param>
        /// <returns> A new DbSkipExpression that represents the skip operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     ,
        ///     <paramref name="sortOrder" />
        ///     or
        ///     <paramref name="count" />
        ///     is null,
        ///     or
        ///     <paramref name="sortOrder" />
        ///     contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="sortOrder" />
        ///     is empty,
        ///     or
        ///     <paramref name="count" />
        ///     is not
        ///     <see cref="DbConstantExpression" />
        ///     or
        ///     <see cref="DbParameterReferenceExpression" />
        ///     or has a
        ///     result type that is not equal or promotable to a 64-bit integer type.
        /// </exception>
        public static DbSkipExpression Skip(this DbExpressionBinding input, IEnumerable<DbSortClause> sortOrder, DbExpression count)
        {
            Check.NotNull(input, "input");
            Check.NotNull(count, "count");

            var validSortOrder = ArgumentValidation.ValidateSkip(sortOrder, count);
            return new DbSkipExpression(input.Expression.ResultType, input, validSortOrder, count);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that sorts the given input set by the specified sort specifications.
        /// </summary>
        /// <param name="input"> An expression binding that specifies the input set. </param>
        /// <param name="sortOrder"> A list of sort specifications that determine how the elements of the input set should be sorted. </param>
        /// <returns> A new DbSortExpression that represents the sort operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     or
        ///     <paramref name="sortOrder" />
        ///     is null,
        ///     or
        ///     <paramref name="sortOrder" />
        ///     contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="sortOrder" />
        ///     is empty.
        /// </exception>
        public static DbSortExpression Sort(this DbExpressionBinding input, IEnumerable<DbSortClause> sortOrder)
        {
            Check.NotNull(input, "input");

            var validSortOrder = ArgumentValidation.ValidateSort(sortOrder);
            return new DbSortExpression(input.Expression.ResultType, input, validSortOrder);
        }

        #endregion

        #region Leaf Expressions - Null, Constant, Parameter, Scan

#if DBEXPRESSIONBUILDER_NULLCONSTANTS
    // Binary 
        public static DbNullExpression NullBinary { get { return _binaryNull; } }
        // Boolean 
        public static DbNullExpression NullBoolean { get { return _boolNull; } }
        // Byte 
        public static DbNullExpression NullByte { get { return _byteNull; } }
        // DateTime 
        public static DbNullExpression NullDateTime { get { return _dateTimeNull; } }
        // DateTimeOffset 
        public static DbNullExpression NullDateTimeOffset { get { return _dateTimeOffsetNull; } }
        // Decimal 
        public static DbNullExpression NullDecimal { get { return _decimalNull; } }
        // Double 
        public static DbNullExpression NullDouble { get { return _doubleNull; } }
        // Guid 
        public static DbNullExpression NullGuid { get { return _guidNull; } }
        // Int16 
        public static DbNullExpression NullInt16 { get { return _int16Null; } }
        // Int32 
        public static DbNullExpression NullInt32 { get { return _int32Null; } }
        // Int64 
        public static DbNullExpression NullInt64 { get { return _int64Null; } }
        // SByte 
        public static DbNullExpression NullSByte { get { return _sbyteNull; } }
        // Single 
        public static DbNullExpression NullSingle { get { return _singleNull; } }
        // String 
        public static DbNullExpression NullString { get { return _stringNull; } }
        // Time
        public static DbNullExpression NullTime { get { return _timeNull; } }
#endif

        /// <summary>
        ///     Creates a new <see cref="DbNullExpression" />, which represents a typed null value.
        /// </summary>
        /// <param name="nullType"> The type of the null value. </param>
        /// <returns> An instance of DbNullExpression </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="nullType" />
        ///     is null
        /// </exception>
        public static DbNullExpression Null(this TypeUsage nullType)
        {
            Check.NotNull(nullType, "nullType");

            ArgumentValidation.ValidateNull(nullType);
            return new DbNullExpression(nullType);
        }

        /// <summary>
        ///     Creates a <see cref="DbConstantExpression" /> with the Boolean value <code>true</code>.
        /// </summary>
        /// <returns> A DbConstantExpression with the Boolean value true. </returns>
        public static DbConstantExpression True
        {
            get { return _boolTrue; }
        }

        /// <summary>
        ///     Creates a <see cref="DbConstantExpression" /> with the Boolean value <code>false</code>.
        /// </summary>
        /// <returns> A DbConstantExpression with the Boolean value false. </returns>
        public static DbConstantExpression False
        {
            get { return _boolFalse; }
        }

        /// <summary>
        ///     Creates a new <see cref="DbConstantExpression" /> with the given constant value.
        /// </summary>
        /// <param name="value"> The constant value to represent. </param>
        /// <returns> A new DbConstantExpression with the given value. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="value" />
        ///     is not an instance of a valid constant type
        /// </exception>
        public static DbConstantExpression Constant(object value)
        {
            Check.NotNull(value, "value");

            var constantType = ArgumentValidation.ValidateConstant(value);
            return new DbConstantExpression(constantType, value);
        }

        /// <summary>
        ///     Creates a new <see cref="DbConstantExpression" /> of the specified primitive type with the given constant value.
        /// </summary>
        /// <param name="constantType"> The type of the constant value. </param>
        /// <param name="value"> The constant value to represent. </param>
        /// <returns>
        ///     A new DbConstantExpression with the given value and a result type of <paramref name="constantType" /> .
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" />
        ///     or
        ///     <paramref name="constantType" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="value" />
        ///     is not an instance of a valid constant type,
        ///     <paramref name="constantType" />
        ///     does not represent a primitive type, or
        ///     <paramref name="value" />
        ///     is of a different primitive type than that represented by
        ///     <paramref name="constantType" />
        /// </exception>
        public static DbConstantExpression Constant(this TypeUsage constantType, object value)
        {
            Check.NotNull(constantType, "constantType");
            Check.NotNull(value, "value");
            ArgumentValidation.ValidateConstant(constantType, value);

            return new DbConstantExpression(constantType, value);
        }

        /// <summary>
        ///     Creates a new <see cref="DbParameterReferenceExpression" /> that references a parameter with the specified name and type.
        /// </summary>
        /// <param name="type"> The type of the referenced parameter </param>
        /// <param name="name"> The name of the referenced parameter </param>
        /// <returns>
        ///     A DbParameterReferenceExpression that represents a reference to a parameter with the specified name and type; the result type of the expression will be the same as
        ///     <paramref
        ///         name="type" />
        ///     .
        /// </returns>
        public static DbParameterReferenceExpression Parameter(this TypeUsage type, string name)
        {
            Check.NotNull(type, "type");
            Check.NotNull(name, "name");

            ArgumentValidation.ValidateParameter(type, name);
            return new DbParameterReferenceExpression(type, name);
        }

        /// <summary>
        ///     Creates a new <see cref="DbVariableReferenceExpression" /> that references a variable with the specified name and type.
        /// </summary>
        /// <param name="type"> The type of the referenced variable </param>
        /// <param name="name"> The name of the referenced variable </param>
        /// <returns>
        ///     A DbVariableReferenceExpression that represents a reference to a variable with the specified name and type; the result type of the expression will be the same as
        ///     <paramref
        ///         name="type" />
        ///     .
        /// </returns>
        public static DbVariableReferenceExpression Variable(this TypeUsage type, string name)
        {
            Check.NotNull(type, "type");
            Check.NotNull(name, "name");

            ArgumentValidation.ValidateVariable(type, name);
            return new DbVariableReferenceExpression(type, name);
        }

        /// <summary>
        ///     Creates a new <see cref="DbScanExpression" /> that references the specified entity or relationship set.
        /// </summary>
        /// <param name="targetSet"> Metadata for the entity or relationship set to reference. </param>
        /// <returns> A new DbScanExpression based on the specified entity or relationship set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="targetSet" />
        ///     is null
        /// </exception>
        public static DbScanExpression Scan(this EntitySetBase targetSet)
        {
            Check.NotNull(targetSet, "targetSet");

            var resultType = ArgumentValidation.ValidateScan(targetSet);
            return new DbScanExpression(resultType, targetSet);
        }

        #endregion

        #region Boolean Operators - And, Or, Not

        /// <summary>
        ///     Creates an <see cref="DbAndExpression" /> that performs the logical And of the left and right arguments.
        /// </summary>
        /// <param name="left"> A Boolean expression that specifies the left argument. </param>
        /// <param name="right"> A Boolean expression that specifies the right argument. </param>
        /// <returns> A new DbAndExpression with the specified arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbAndExpression And(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            var resultType = ArgumentValidation.ValidateAnd(left, right);
            return new DbAndExpression(resultType, left, right);
        }

        /// <summary>
        ///     Creates an <see cref="DbOrExpression" /> that performs the logical Or of the left and right arguments.
        /// </summary>
        /// <param name="left"> A Boolean expression that specifies the left argument. </param>
        /// <param name="right"> A Boolean expression that specifies the right argument. </param>
        /// <returns> A new DbOrExpression with the specified arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbOrExpression Or(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            var resultType = ArgumentValidation.ValidateOr(left, right);
            return new DbOrExpression(resultType, left, right);
        }

        /// <summary>
        ///     Creates a <see cref="DbNotExpression" /> that performs the logical negation of the given argument.
        /// </summary>
        /// <param name="argument"> A Boolean expression that specifies the argument. </param>
        /// <returns> A new DbNotExpression with the specified argument. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbNotExpression Not(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var resultType = ArgumentValidation.ValidateNot(argument);
            return new DbNotExpression(resultType, argument);
        }

        #endregion

        #region Arithmetic Operators - Divide, Minus, Modulo, Multiply, Plus, UnaryMinus

        private static DbArithmeticExpression CreateArithmetic(DbExpressionKind kind, DbExpression left, DbExpression right)
        {
            TypeUsage numericResultType;
            var arguments = ArgumentValidation.ValidateArithmetic(left, right, out numericResultType);
            return new DbArithmeticExpression(kind, numericResultType, arguments);
        }

        /// <summary>
        ///     Creates a new <see cref="DbArithmeticExpression" /> that divides the left argument by the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbArithmeticExpression representing the division operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common numeric result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbArithmeticExpression Divide(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateArithmetic(DbExpressionKind.Divide, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbArithmeticExpression" /> that subtracts the right argument from the left argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbArithmeticExpression representing the subtraction operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common numeric result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbArithmeticExpression Minus(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateArithmetic(DbExpressionKind.Minus, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbArithmeticExpression" /> that computes the remainder of the left argument divided by the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbArithmeticExpression representing the modulo operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common numeric result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbArithmeticExpression Modulo(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateArithmetic(DbExpressionKind.Modulo, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbArithmeticExpression" /> that multiplies the left argument by the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbArithmeticExpression representing the multiplication operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common numeric result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbArithmeticExpression Multiply(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateArithmetic(DbExpressionKind.Multiply, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbArithmeticExpression" /> that adds the left argument to the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbArithmeticExpression representing the addition operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common numeric result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbArithmeticExpression Plus(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateArithmetic(DbExpressionKind.Plus, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbArithmeticExpression" /> that negates the value of the argument.
        /// </summary>
        /// <param name="argument"> An expression that specifies the argument. </param>
        /// <returns> A new DbArithmeticExpression representing the negation operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No numeric result type exists for
        ///     <paramref name="argument" />
        ///     .
        /// </exception>
        public static DbArithmeticExpression UnaryMinus(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            TypeUsage resultType;
            var args = ArgumentValidation.ValidateUnaryMinus(argument, out resultType);
            return new DbArithmeticExpression(DbExpressionKind.UnaryMinus, resultType, args);
        }

        /// <summary>
        ///     Creates a new <see cref="DbArithmeticExpression" /> that negates the value of the argument.
        /// </summary>
        /// <param name="argument"> An expression that specifies the argument. </param>
        /// <returns> A new DbArithmeticExpression representing the negation operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No numeric result type exists for
        ///     <paramref name="argument" />
        ///     .
        /// </exception>
        public static DbArithmeticExpression Negate(this DbExpression argument)
        {
            return argument.UnaryMinus();
        }

        #endregion

        #region Comparison Operators - Equal, NotEqual, GreaterThan, LessThan, GreaterThanEqual, LessThanEqual, IsNull, Like

        private static DbComparisonExpression CreateComparison(DbExpressionKind kind, DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            var resultType = ArgumentValidation.ValidateComparison(kind, left, right);
            return new DbComparisonExpression(kind, resultType, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbComparisonExpression" /> that compares the left and right arguments for equality.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbComparisonExpression representing the equality comparison. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common equality-comparable result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbComparisonExpression Equal(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateComparison(DbExpressionKind.Equals, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbComparisonExpression" /> that compares the left and right arguments for inequality.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbComparisonExpression representing the inequality comparison. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common equality-comparable result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbComparisonExpression NotEqual(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateComparison(DbExpressionKind.NotEquals, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbComparisonExpression" /> that determines whether the left argument is greater than the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbComparisonExpression representing the greater-than comparison. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common order-comparable result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbComparisonExpression GreaterThan(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateComparison(DbExpressionKind.GreaterThan, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbComparisonExpression" /> that determines whether the left argument is less than the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbComparisonExpression representing the less-than comparison. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common order-comparable result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbComparisonExpression LessThan(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateComparison(DbExpressionKind.LessThan, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbComparisonExpression" /> that determines whether the left argument is greater than or equal to the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbComparisonExpression representing the greater-than-or-equal-to comparison. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common result type that is both equality- and order-comparable exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbComparisonExpression GreaterThanOrEqual(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateComparison(DbExpressionKind.GreaterThanOrEquals, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbComparisonExpression" /> that determines whether the left argument is less than or equal to the right argument.
        /// </summary>
        /// <param name="left"> An expression that specifies the left argument. </param>
        /// <param name="right"> An expression that specifies the right argument. </param>
        /// <returns> A new DbComparisonExpression representing the less-than-or-equal-to comparison. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common result type that is both equality- and order-comparable exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbComparisonExpression LessThanOrEqual(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            return CreateComparison(DbExpressionKind.LessThanOrEquals, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbIsNullExpression" /> that determines whether the specified argument is null.
        /// </summary>
        /// <param name="argument"> An expression that specifies the argument. </param>
        /// <returns> A new DbIsNullExpression with the specified argument. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     has a collection result type.
        /// </exception>
        public static DbIsNullExpression IsNull(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var resultType = ArgumentValidation.ValidateIsNull(argument);
            return new DbIsNullExpression(resultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLikeExpression" /> that compares the specified input string to the given pattern.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input string. </param>
        /// <param name="pattern"> An expression that specifies the pattern string. </param>
        /// <returns> A new DbLikeExpression with the specified input, pattern and a null escape. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="pattern" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="pattern" />
        ///     does not have a string result type.
        /// </exception>
        public static DbLikeExpression Like(this DbExpression argument, DbExpression pattern)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(pattern, "pattern");

            var resultType = ArgumentValidation.ValidateLike(argument, pattern);
            DbExpression escape = pattern.ResultType.Null();
            return new DbLikeExpression(resultType, argument, pattern, escape);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLikeExpression" /> that compares the specified input string to the given pattern using the optional escape.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input string. </param>
        /// <param name="pattern"> An expression that specifies the pattern string. </param>
        /// <param name="escape"> An optional expression that specifies the escape string. </param>
        /// <returns> A new DbLikeExpression with the specified input, pattern and escape. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     ,
        ///     <paramref name="pattern" />
        ///     or
        ///     <paramref name="escape" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     ,
        ///     <paramref name="pattern" />
        ///     or
        ///     <paramref name="escape" />
        ///     does not have a string result type.
        /// </exception>
        public static DbLikeExpression Like(this DbExpression argument, DbExpression pattern, DbExpression escape)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(pattern, "pattern");
            Check.NotNull(escape, "escape");

            var resultType = ArgumentValidation.ValidateLike(argument, pattern, escape);
            return new DbLikeExpression(resultType, argument, pattern, escape);
        }

        #endregion

        #region Type Operators - Cast, Treat, OfType, OfTypeOnly, IsOf, IsOfOnly

        /// <summary>
        ///     Creates a new <see cref="DbCastExpression" /> that applies a cast operation to a polymorphic argument.
        /// </summary>
        /// <param name="argument"> The argument to which the cast should be applied. </param>
        /// <param name="toType"> Type metadata that specifies the type to cast to. </param>
        /// <returns> A new DbCastExpression with the specified argument and target type. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="toType" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">The specified cast is not valid.</exception>
        public static DbCastExpression CastTo(this DbExpression argument, TypeUsage toType)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(toType, "toType");

            ArgumentValidation.ValidateCastTo(argument, toType);
            return new DbCastExpression(toType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbTreatExpression" />.
        /// </summary>
        /// <param name="argument"> An expression that specifies the instance. </param>
        /// <param name="treatType"> Type metadata for the treat-as type. </param>
        /// <returns> A new DbTreatExpression with the specified argument and type. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="treatType" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="treatType" />
        ///     is not in the same type hierarchy as the result type of
        ///     <paramref name="argument" />
        ///     .
        /// </exception>
        /// <remarks>
        ///     DbTreatExpression requires that <paramref name="argument" /> has a polymorphic result type,
        ///     and that <paramref name="treatType" /> is a type from the same type hierarchy as that result type.
        /// </remarks>
        public static DbTreatExpression TreatAs(this DbExpression argument, TypeUsage treatType)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(treatType, "treatType");

            ArgumentValidation.ValidateTreatAs(argument, treatType);
            return new DbTreatExpression(treatType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbOfTypeExpression" /> that produces a set consisting of the elements of the given input set that are of the specified type.
        /// </summary>
        /// <param name="argument">
        ///     A <see cref="DbExpression" /> that specifies the input set.
        /// </param>
        /// <param name="type"> Type metadata for the type that elements of the input set must have to be included in the resulting set. </param>
        /// <returns>
        ///     A new DbOfTypeExpression with the specified set argument and type, and an ExpressionKind of
        ///     <see
        ///         cref="DbExpressionKind.OfType" />
        ///     .
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="type" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type, or
        ///     <paramref name="type" />
        ///     is not a type in the same type hierarchy as the element type of the
        ///     collection result type of
        ///     <paramref name="argument" />
        ///     .
        /// </exception>
        /// <remarks>
        ///     DbOfTypeExpression requires that <paramref name="argument" /> has a collection result type with
        ///     a polymorphic element type, and that <paramref name="type" /> is a type from the same type hierarchy as that element type.
        /// </remarks>
        public static DbOfTypeExpression OfType(this DbExpression argument, TypeUsage type)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(type, "type");

            var collectionOfTypeResultType = ArgumentValidation.ValidateOfType(argument, type);
            return new DbOfTypeExpression(DbExpressionKind.OfType, collectionOfTypeResultType, argument, type);
        }

        /// <summary>
        ///     Creates a new <see cref="DbOfTypeExpression" /> that produces a set consisting of the elements of the given input set that are of exactly the specified type.
        /// </summary>
        /// <param name="argument">
        ///     An <see cref="DbExpression" /> that specifies the input set.
        /// </param>
        /// <param name="type"> Type metadata for the type that elements of the input set must match exactly to be included in the resulting set. </param>
        /// <returns>
        ///     A new DbOfTypeExpression with the specified set argument and type, and an ExpressionKind of
        ///     <see
        ///         cref="DbExpressionKind.OfTypeOnly" />
        ///     .
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="type" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type, or
        ///     <paramref name="type" />
        ///     is not a type in the same type hierarchy as the element type of the
        ///     collection result type of
        ///     <paramref name="argument" />
        ///     .
        /// </exception>
        /// <remarks>
        ///     DbOfTypeExpression requires that <paramref name="argument" /> has a collection result type with
        ///     a polymorphic element type, and that <paramref name="type" /> is a type from the same type hierarchy as that element type.
        /// </remarks>
        public static DbOfTypeExpression OfTypeOnly(this DbExpression argument, TypeUsage type)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(type, "type");

            var collectionOfTypeResultType = ArgumentValidation.ValidateOfType(argument, type);
            return new DbOfTypeExpression(DbExpressionKind.OfTypeOnly, collectionOfTypeResultType, argument, type);
        }

        /// <summary>
        ///     Creates a new <see cref="DbIsOfExpression" /> that determines whether the given argument is of the specified type or a subtype.
        /// </summary>
        /// <param name="argument"> An expression that specifies the instance. </param>
        /// <param name="type"> Type metadata that specifies the type that the instance's result type should be compared to. </param>
        /// <returns> A new DbIsOfExpression with the specified instance and type and DbExpressionKind IsOf. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="type" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="type" />
        ///     is not in the same type hierarchy as the result type of
        ///     <paramref name="argument" />
        ///     .
        /// </exception>
        /// <remarks>
        ///     DbIsOfExpression requires that <paramref name="argument" /> has a polymorphic result type,
        ///     and that <paramref name="type" /> is a type from the same type hierarchy as that result type.
        /// </remarks>
        public static DbIsOfExpression IsOf(this DbExpression argument, TypeUsage type)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(type, "type");

            var booleanResultType = ArgumentValidation.ValidateIsOf(argument, type);
            return new DbIsOfExpression(DbExpressionKind.IsOf, booleanResultType, argument, type);
        }

        /// <summary>
        ///     Creates a new <see cref="DbIsOfExpression" /> expression that determines whether the given argument is of the specified type, and only that type (not a subtype).
        /// </summary>
        /// <param name="argument"> An expression that specifies the instance. </param>
        /// <param name="type"> Type metadata that specifies the type that the instance's result type should be compared to. </param>
        /// <returns> A new DbIsOfExpression with the specified instance and type and DbExpressionKind IsOfOnly. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="type" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="type" />
        ///     is not in the same type hierarchy as the result type of
        ///     <paramref name="argument" />
        ///     .
        /// </exception>
        /// <remarks>
        ///     DbIsOfExpression requires that <paramref name="argument" /> has a polymorphic result type,
        ///     and that <paramref name="type" /> is a type from the same type hierarchy as that result type.
        /// </remarks>
        public static DbIsOfExpression IsOfOnly(this DbExpression argument, TypeUsage type)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(type, "type");

            var booleanResultType = ArgumentValidation.ValidateIsOf(argument, type);
            return new DbIsOfExpression(DbExpressionKind.IsOfOnly, booleanResultType, argument, type);
        }

        #endregion

        #region Ref Operators - Deref, EntityRef, Ref, RefKey, RelationshipNavigation

        /// <summary>
        ///     Creates a new <see cref="DbDerefExpression" /> that retrieves a specific Entity given a reference expression
        /// </summary>
        /// <param name="argument">
        ///     An <see cref="DbExpression" /> that provides the reference. This expression must have a reference Type
        /// </param>
        /// <returns> A new DbDerefExpression that retrieves the specified Entity </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a reference result type.
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deref")]
        public static DbDerefExpression Deref(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var entityResultType = ArgumentValidation.ValidateDeref(argument);
            return new DbDerefExpression(entityResultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbEntityRefExpression" /> that retrieves the ref of the specifed entity in structural form.
        /// </summary>
        /// <param name="argument"> The expression that provides the entity. This expression must have an entity result type. </param>
        /// <returns> A new DbEntityRefExpression that retrieves a reference to the specified entity. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have an entity result type.
        /// </exception>
        public static DbEntityRefExpression GetEntityRef(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var refResultType = ArgumentValidation.ValidateGetEntityRef(argument);
            return new DbEntityRefExpression(refResultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRefExpression" /> that encodes a reference to a specific entity based on key values.
        /// </summary>
        /// <param name="entitySet"> The entity set in which the referenced element resides. </param>
        /// <param name="keyValues">
        ///     A collection of <see cref="DbExpression" /> s that provide the key values. These expressions must match (in number, type, and order) the key properties of the referenced entity type.
        /// </param>
        /// <returns> A new DbRefExpression that references the element with the specified key values in the given entity set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entitySet" />
        ///     is null, or
        ///     <paramref name="keyValues" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="keyValues" />
        ///     does not match the count of key members declared by the
        ///     <paramref name="entitySet" />
        ///     's element type,
        ///     or
        ///     <paramref name="keyValues" />
        ///     contains an expression with a result type that is not compatible with the type of the corresponding key member.
        /// </exception>
        public static DbRefExpression CreateRef(this EntitySet entitySet, IEnumerable<DbExpression> keyValues)
        {
            Check.NotNull(entitySet, "entitySet");

            return CreateRefExpression(entitySet, keyValues);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRefExpression" /> that encodes a reference to a specific entity based on key values.
        /// </summary>
        /// <param name="entitySet"> The entity set in which the referenced element resides. </param>
        /// <param name="keyValues">
        ///     A collection of <see cref="DbExpression" /> s that provide the key values. These expressions must match (in number, type, and order) the key properties of the referenced entity type.
        /// </param>
        /// <returns> A new DbRefExpression that references the element with the specified key values in the given entity set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entitySet" />
        ///     is null, or
        ///     <paramref name="keyValues" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="keyValues" />
        ///     does not match the count of key members declared by the
        ///     <paramref name="entitySet" />
        ///     's element type,
        ///     or
        ///     <paramref name="keyValues" />
        ///     contains an expression with a result type that is not compatible with the type of the corresponding key member.
        /// </exception>
        public static DbRefExpression CreateRef(this EntitySet entitySet, params DbExpression[] keyValues)
        {
            Check.NotNull(entitySet, "entitySet");

            return CreateRefExpression(entitySet, keyValues);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRefExpression" /> that encodes a reference to a specific entity of a given type based on key values.
        /// </summary>
        /// <param name="entitySet"> The entity set in which the referenced element resides. </param>
        /// <param name="entityType"> The specific type of the referenced entity. This must be an entity type from the same hierarchy as the entity set's element type. </param>
        /// <param name="keyValues">
        ///     A collection of <see cref="DbExpression" /> s that provide the key values. These expressions must match (in number, type, and order) the key properties of the referenced entity type.
        /// </param>
        /// <returns> A new DbRefExpression that references the element with the specified key values in the given entity set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entitySet" />
        ///     or
        ///     <paramref name="entityType" />
        ///     is null, or
        ///     <paramref name="keyValues" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="entityType" />
        ///     is not from the same type hierarchy (a subtype, supertype, or the same type) as
        ///     <paramref name="entitySet" />
        ///     's element type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="keyValues" />
        ///     does not match the count of key members declared by the
        ///     <paramref name="entitySet" />
        ///     's element type,
        ///     or
        ///     <paramref name="keyValues" />
        ///     contains an expression with a result type that is not compatible with the type of the corresponding key member.
        /// </exception>
        public static DbRefExpression CreateRef(this EntitySet entitySet, EntityType entityType, IEnumerable<DbExpression> keyValues)
        {
            Check.NotNull(entitySet, "entitySet");
            Check.NotNull(entityType, "entityType");

            return CreateRefExpression(entitySet, entityType, keyValues);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRefExpression" /> that encodes a reference to a specific entity of a given type based on key values.
        /// </summary>
        /// <param name="entitySet"> The entity set in which the referenced element resides. </param>
        /// <param name="entityType"> The specific type of the referenced entity. This must be an entity type from the same hierarchy as the entity set's element type. </param>
        /// <param name="keyValues">
        ///     A collection of <see cref="DbExpression" /> s that provide the key values. These expressions must match (in number, type, and order) the key properties of the referenced entity type.
        /// </param>
        /// <returns> A new DbRefExpression that references the element with the specified key values in the given entity set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entitySet" />
        ///     or
        ///     <paramref name="entityType" />
        ///     is null, or
        ///     <paramref name="keyValues" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="entityType" />
        ///     is not from the same type hierarchy (a subtype, supertype, or the same type) as
        ///     <paramref name="entitySet" />
        ///     's element type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="keyValues" />
        ///     does not match the count of key members declared by the
        ///     <paramref name="entitySet" />
        ///     's element type,
        ///     or
        ///     <paramref name="keyValues" />
        ///     contains an expression with a result type that is not compatible with the type of the corresponding key member.
        /// </exception>
        public static DbRefExpression CreateRef(this EntitySet entitySet, EntityType entityType, params DbExpression[] keyValues)
        {
            Check.NotNull(entitySet, "entitySet");
            Check.NotNull(entityType, "entityType");

            return CreateRefExpression(entitySet, entityType, keyValues);
        }

        private static DbRefExpression CreateRefExpression(EntitySet entitySet, IEnumerable<DbExpression> keyValues)
        {
            DbExpression keyConstructor;
            var refResultType = ArgumentValidation.ValidateCreateRef(entitySet, entitySet.ElementType, keyValues, out keyConstructor);
            return new DbRefExpression(refResultType, entitySet, keyConstructor);
        }

        private static DbRefExpression CreateRefExpression(EntitySet entitySet, EntityType entityType, IEnumerable<DbExpression> keyValues)
        {
            Check.NotNull(entitySet, "entitySet");
            Check.NotNull(entityType, "entityType");

            DbExpression keyConstructor;
            var refResultType = ArgumentValidation.ValidateCreateRef(entitySet, entityType, keyValues, out keyConstructor);
            return new DbRefExpression(refResultType, entitySet, keyConstructor);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRefExpression" /> that encodes a reference to a specific Entity based on key values.
        /// </summary>
        /// <param name="entitySet"> The Entity set in which the referenced element resides. </param>
        /// <param name="keyRow">
        ///     A <see cref="DbExpression" /> that constructs a record with columns that match (in number, type, and order) the Key properties of the referenced Entity type.
        /// </param>
        /// <returns> A new DbRefExpression that references the element with the specified key values in the given Entity set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entitySet" />
        ///     or
        ///     <paramref name="keyRow" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="keyRow" />
        ///     does not have a record result type that matches the key properties of the referenced entity set's entity type.
        /// </exception>
        /// <remarks>
        ///     <paramref name="keyRow" /> should be an expression that specifies the key values that identify the referenced entity within the given entity set.
        ///     The result type of <paramref name="keyRow" /> should contain a corresponding column for each key property defined by
        ///     <paramref
        ///         name="entitySet" />
        ///     's entity type.
        /// </remarks>
        public static DbRefExpression RefFromKey(this EntitySet entitySet, DbExpression keyRow)
        {
            Check.NotNull(entitySet, "entitySet");
            Check.NotNull(keyRow, "keyRow");

            var refResultType = ArgumentValidation.ValidateRefFromKey(entitySet, keyRow, entitySet.ElementType);
            return new DbRefExpression(refResultType, entitySet, keyRow);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRefExpression" /> that encodes a reference to a specific Entity based on key values.
        /// </summary>
        /// <param name="entitySet"> The Entity set in which the referenced element resides. </param>
        /// <param name="keyRow">
        ///     A <see cref="DbExpression" /> that constructs a record with columns that match (in number, type, and order) the Key properties of the referenced Entity type.
        /// </param>
        /// <param name="entityType"> The type of the Entity that the reference should refer to. </param>
        /// <returns> A new DbRefExpression that references the element with the specified key values in the given Entity set. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="entitySet" />
        ///     ,
        ///     <paramref name="keyRow" />
        ///     or
        ///     <paramref name="entityType" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="entityType" />
        ///     is not in the same type hierarchy as the entity set's entity type, or
        ///     <paramref name="keyRow" />
        ///     does not have a
        ///     record result type that matches the key properties of the referenced entity set's entity type.
        /// </exception>
        /// <remarks>
        ///     <paramref name="keyRow" /> should be an expression that specifies the key values that identify the referenced entity within the given entity set.
        ///     The result type of <paramref name="keyRow" /> should contain a corresponding column for each key property defined by
        ///     <paramref
        ///         name="entitySet" />
        ///     's entity type.
        /// </remarks>
        public static DbRefExpression RefFromKey(this EntitySet entitySet, DbExpression keyRow, EntityType entityType)
        {
            Check.NotNull(entitySet, "entitySet");
            Check.NotNull(keyRow, "keyRow");
            Check.NotNull(entityType, "entityType");

            var refResultType = ArgumentValidation.ValidateRefFromKey(entitySet, keyRow, entityType);
            return new DbRefExpression(refResultType, entitySet, keyRow);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRefKeyExpression" /> that retrieves the key values of the specifed reference in structural form.
        /// </summary>
        /// <param name="argument"> The expression that provides the reference. This expression must have a reference Type with an Entity element type. </param>
        /// <returns> A new DbRefKeyExpression that retrieves the key values of the specified reference. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a reference result type.
        /// </exception>
        public static DbRefKeyExpression GetRefKey(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var rowResultType = ArgumentValidation.ValidateGetRefKey(argument);
            return new DbRefKeyExpression(rowResultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRelationshipNavigationExpression" /> representing the navigation of a composition or association relationship.
        /// </summary>
        /// <param name="navigateFrom"> An expression the specifies the instance from which navigation should occur </param>
        /// <param name="fromEnd"> Metadata for the property that represents the end of the relationship from which navigation should occur </param>
        /// <param name="toEnd"> Metadata for the property that represents the end of the relationship to which navigation should occur </param>
        /// <returns> A new DbRelationshipNavigationExpression representing the navigation of the specified from and to relation ends of the specified relation type from the specified navigation source instance </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="fromEnd" />
        ///     ,
        ///     <paramref name="toEnd" />
        ///     or
        ///     <paramref name="navigateFrom" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="fromEnd" />
        ///     and
        ///     <paramref name="toEnd" />
        ///     are not declared by the same relationship type, or
        ///     <paramref name="navigateFrom" />
        ///     has a result type that is not compatible with the property type of
        ///     <paramref name="fromEnd" />
        ///     .
        /// </exception>
        /// <remarks>
        ///     <see cref="DbRelationshipNavigationExpression" /> requires that navigation always occur from a reference, and so
        ///     <paramref
        ///         name="navigateFrom" />
        ///     must always have a reference result type.
        /// </remarks>
        public static DbRelationshipNavigationExpression Navigate(
            this DbExpression navigateFrom, RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
        {
            Check.NotNull(navigateFrom, "navigateFrom");
            Check.NotNull(fromEnd, "fromEnd");
            Check.NotNull(toEnd, "toEnd");

            RelationshipType relType;
            var resultType = ArgumentValidation.ValidateNavigate(
                navigateFrom, fromEnd, toEnd, out relType, allowAllRelationshipsInSameTypeHierarchy: false);
            return new DbRelationshipNavigationExpression(resultType, relType, fromEnd, toEnd, navigateFrom);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRelationshipNavigationExpression" /> representing the navigation of a composition or association relationship.
        /// </summary>
        /// <param name="type"> Metadata for the relation type that represents the relationship </param>
        /// <param name="fromEndName"> The name of the property of the relation type that represents the end of the relationship from which navigation should occur </param>
        /// <param name="toEndName"> The name of the property of the relation type that represents the end of the relationship to which navigation should occur </param>
        /// <param name="navigateFrom"> An expression the specifies the instance from which naviagtion should occur </param>
        /// <returns> A new DbRelationshipNavigationExpression representing the navigation of the specified from and to relation ends of the specified relation type from the specified navigation source instance </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="type" />
        ///     ,
        ///     <paramref name="fromEndName" />
        ///     ,
        ///     <paramref name="toEndName" />
        ///     or
        ///     <paramref name="navigateFrom" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="type" />
        ///     is not associated with this command tree's metadata workspace or
        ///     <paramref name="navigateFrom" />
        ///     is associated with a different command tree,
        ///     or
        ///     <paramref name="type" />
        ///     does not declare a relation end property with name
        ///     <paramref name="toEndName" />
        ///     or
        ///     <paramref name="fromEndName" />
        ///     ,
        ///     or
        ///     <paramref name="navigateFrom" />
        ///     has a result type that is not compatible with the property type of the relation end property with name
        ///     <paramref name="fromEndName" />
        ///     .
        /// </exception>
        /// <remarks>
        ///     <see cref="DbRelationshipNavigationExpression" /> requires that navigation always occur from a reference, and so
        ///     <paramref
        ///         name="navigateFrom" />
        ///     must always have a reference result type.
        /// </remarks>
        public static DbRelationshipNavigationExpression Navigate(
            this RelationshipType type, string fromEndName, string toEndName, DbExpression navigateFrom)
        {
            Check.NotNull(type, "type");
            Check.NotNull(fromEndName, "fromEndName");
            Check.NotNull(toEndName, "toEndName");
            Check.NotNull(navigateFrom, "navigateFrom");

            RelationshipEndMember fromEnd;
            RelationshipEndMember toEnd;
            var resultType = ArgumentValidation.ValidateNavigate(navigateFrom, type, fromEndName, toEndName, out fromEnd, out toEnd);
            return new DbRelationshipNavigationExpression(resultType, type, fromEnd, toEnd, navigateFrom);
        }

        #endregion

        #region Unary and Binary Set Operators - Distinct, Element, IsEmpty, Except, Intersect, UnionAll, Limit

        /// <summary>
        ///     Creates a new <see cref="DbDistinctExpression" /> that removes duplicates from the given set argument.
        /// </summary>
        /// <param name="argument"> An expression that defines the set over which to perfom the distinct operation. </param>
        /// <returns> A new DbDistinctExpression that represents the distinct operation applied to the specified set argument. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type.
        /// </exception>
        public static DbDistinctExpression Distinct(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var resultType = ArgumentValidation.ValidateDistinct(argument);
            return new DbDistinctExpression(resultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbElementExpression" /> that converts a set into a singleton.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input set. </param>
        /// <returns> A DbElementExpression that represents the conversion of the set argument to a singleton. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type.
        /// </exception>
        public static DbElementExpression Element(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var resultType = ArgumentValidation.ValidateElement(argument);
            return new DbElementExpression(resultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbIsEmptyExpression" /> that determines whether the specified set argument is an empty set.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input set </param>
        /// <returns> A new DbIsEmptyExpression with the specified argument. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type.
        /// </exception>
        public static DbIsEmptyExpression IsEmpty(this DbExpression argument)
        {
            Check.NotNull(argument, "argument");

            var booleanResultType = ArgumentValidation.ValidateIsEmpty(argument);
            return new DbIsEmptyExpression(booleanResultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbExceptExpression" /> that computes the subtraction of the right set argument from the left set argument.
        /// </summary>
        /// <param name="left"> An expression that defines the left set argument. </param>
        /// <param name="right"> An expression that defines the right set argument. </param>
        /// <returns> A new DbExceptExpression that represents the difference of the left argument from the right argument. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common collection result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbExceptExpression Except(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            var resultType = ArgumentValidation.ValidateExcept(left, right);
            return new DbExceptExpression(resultType, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbIntersectExpression" /> that computes the intersection of the left and right set arguments.
        /// </summary>
        /// <param name="left"> An expression that defines the left set argument. </param>
        /// <param name="right"> An expression that defines the right set argument. </param>
        /// <returns> A new DbIntersectExpression that represents the intersection of the left and right arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common collection result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbIntersectExpression Intersect(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            var resultType = ArgumentValidation.ValidateIntersect(left, right);
            return new DbIntersectExpression(resultType, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbUnionAllExpression" /> that computes the union of the left and right set arguments and does not remove duplicates.
        /// </summary>
        /// <param name="left"> An expression that defines the left set argument. </param>
        /// <param name="right"> An expression that defines the right set argument. </param>
        /// <returns> A new DbUnionAllExpression that union, including duplicates, of the the left and right arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common collection result type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbUnionAllExpression UnionAll(this DbExpression left, DbExpression right)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");

            var resultType = ArgumentValidation.ValidateUnionAll(left, right);
            return new DbUnionAllExpression(resultType, left, right);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLimitExpression" /> that restricts the number of elements in the Argument collection to the specified count Limit value.
        ///     Tied results are not included in the output.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input collection. </param>
        /// <param name="count"> An expression that specifies the limit value. </param>
        /// <returns> A new DbLimitExpression with the specified argument and count limit values that does not include tied results. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="count" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type,
        ///     or
        ///     <paramref name="count" />
        ///     does not have a result type that is equal or promotable to a 64-bit integer type.
        /// </exception>
        public static DbLimitExpression Limit(this DbExpression argument, DbExpression count)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(count, "count");

            var resultType = ArgumentValidation.ValidateLimit(argument, count);
            return new DbLimitExpression(resultType, argument, count, false);
        }

        #endregion

        #region General Operators - Case, Function, NewInstance, Property

        /// <summary>
        ///     Creates a new <see cref="DbCaseExpression" />.
        /// </summary>
        /// <param name="whenExpressions"> A list of expressions that provide the conditional for of each case. </param>
        /// <param name="thenExpressions"> A list of expressions that provide the result of each case. </param>
        /// <param name="elseExpression"> An expression that defines the result when no case is matched. </param>
        /// <returns> A new DbCaseExpression with the specified cases and default result. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="whenExpressions" />
        ///     or
        ///     <paramref name="thenExpressions" />
        ///     is null or contains null,
        ///     or
        ///     <paramref name="elseExpression" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="whenExpressions" />
        ///     or
        ///     <paramref name="thenExpressions" />
        ///     is empty or
        ///     <paramref name="whenExpressions" />
        ///     contains an expression with a non-Boolean result type, or
        ///     No common result type exists for all expressions in
        ///     <paramref name="thenExpressions" />
        ///     and
        ///     <paramref name="elseExpression" />
        ///     .
        /// </exception>
        public static DbCaseExpression Case(
            IEnumerable<DbExpression> whenExpressions, IEnumerable<DbExpression> thenExpressions, DbExpression elseExpression)
        {
            Check.NotNull(whenExpressions, "whenExpressions");
            Check.NotNull(thenExpressions, "thenExpressions");
            Check.NotNull(elseExpression, "elseExpression");

            DbExpressionList validWhens;
            DbExpressionList validThens;
            var resultType = ArgumentValidation.ValidateCase(
                whenExpressions, thenExpressions, elseExpression, out validWhens, out validThens);
            return new DbCaseExpression(resultType, validWhens, validThens, elseExpression);
        }

        /// <summary>
        ///     Creates a new <see cref="DbFunctionExpression" /> representing the invocation of the specified function with the given arguments.
        /// </summary>
        /// <param name="function"> Metadata for the function to invoke. </param>
        /// <param name="arguments"> A list of expressions that provide the arguments to the function. </param>
        /// <returns> A new DbFunctionExpression representing the function invocation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="function" />
        ///     is null, or
        ///     <paramref name="arguments" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="arguments" />
        ///     does not equal the number of parameters declared by
        ///     <paramref name="function" />
        ///     ,
        ///     or
        ///     <paramref name="arguments" />
        ///     contains an expression that has a result type that is not equal or promotable
        ///     to the corresponding function parameter type.
        /// </exception>
        public static DbFunctionExpression Invoke(this EdmFunction function, IEnumerable<DbExpression> arguments)
        {
            Check.NotNull(function, "function");

            return InvokeFunction(function, arguments);
        }

        /// <summary>
        ///     Creates a new <see cref="DbFunctionExpression" /> representing the invocation of the specified function with the given arguments.
        /// </summary>
        /// <param name="function"> Metadata for the function to invoke. </param>
        /// <param name="arguments"> Expressions that provide the arguments to the function. </param>
        /// <returns> A new DbFunctionExpression representing the function invocation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="function" />
        ///     is null, or
        ///     <paramref name="arguments" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="arguments" />
        ///     does not equal the number of parameters declared by
        ///     <paramref name="function" />
        ///     ,
        ///     or
        ///     <paramref name="arguments" />
        ///     contains an expression that has a result type that is not equal or promotable
        ///     to the corresponding function parameter type.
        /// </exception>
        public static DbFunctionExpression Invoke(this EdmFunction function, params DbExpression[] arguments)
        {
            Check.NotNull(function, "function");

            return InvokeFunction(function, arguments);
        }

        private static DbFunctionExpression InvokeFunction(EdmFunction function, IEnumerable<DbExpression> arguments)
        {
            DbExpressionList validArguments;
            var resultType = ArgumentValidation.ValidateFunction(function, arguments, out validArguments);
            return new DbFunctionExpression(resultType, function, validArguments);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambdaExpression" /> representing the application of the specified Lambda function to the given arguments.
        /// </summary>
        /// <param name="lambda">
        ///     A <see cref="DbLambda" /> instance representing the Lambda function to apply.
        /// </param>
        /// <param name="arguments"> A list of expressions that provide the arguments. </param>
        /// <returns> A new DbLambdaExpression representing the Lambda function application. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="lambda" />
        ///     is null, or
        ///     <paramref name="arguments" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="arguments" />
        ///     does not equal the number of variables declared by
        ///     <paramref name="lambda" />
        ///     ,
        ///     or
        ///     <paramref name="arguments" />
        ///     contains an expression that has a result type that is not equal or promotable
        ///     to the corresponding variable type.
        /// </exception>
        public static DbLambdaExpression Invoke(this DbLambda lambda, IEnumerable<DbExpression> arguments)
        {
            Check.NotNull(lambda, "lambda");
            Check.NotNull(arguments, "arguments");

            return InvokeLambda(lambda, arguments);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambdaExpression" /> representing the application of the specified Lambda function to the given arguments.
        /// </summary>
        /// <param name="lambda">
        ///     A <see cref="DbLambda" /> instance representing the Lambda function to apply.
        /// </param>
        /// <param name="arguments"> Expressions that provide the arguments. </param>
        /// <returns> A new DbLambdaExpression representing the Lambda function application. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="lambda" />
        ///     is null, or
        ///     <paramref name="arguments" />
        ///     is null or contains null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The count of
        ///     <paramref name="arguments" />
        ///     does not equal the number of variables declared by
        ///     <paramref name="lambda" />
        ///     ,
        ///     or
        ///     <paramref name="arguments" />
        ///     contains an expression that has a result type that is not equal or promotable
        ///     to the corresponding variable type.
        /// </exception>
        public static DbLambdaExpression Invoke(this DbLambda lambda, params DbExpression[] arguments)
        {
            Check.NotNull(lambda, "lambda");
            Check.NotNull(arguments, "arguments");

            return InvokeLambda(lambda, arguments);
        }

        private static DbLambdaExpression InvokeLambda(DbLambda lambda, IEnumerable<DbExpression> arguments)
        {
            DbExpressionList validArguments;
            var resultType = ArgumentValidation.ValidateInvoke(lambda, arguments, out validArguments);
            return new DbLambdaExpression(resultType, lambda, validArguments);
        }

        /// <summary>
        ///     Creates a new <see cref="DbNewInstanceExpression" />. If the type argument is a collection type, the arguments specify the elements of the collection. Otherwise the arguments are used as property or column values in the new instance.
        /// </summary>
        /// <param name="instanceType"> The type of the new instance. </param>
        /// <param name="arguments"> Expressions that specify values of the new instances, interpreted according to the instance's type. </param>
        /// <returns> A new DbNewInstanceExpression with the specified type and arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="instanceType" />
        ///     or
        ///     <paramref name="arguments" />
        ///     is null, or
        ///     <paramref name="arguments" />
        ///     contains null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="arguments" />
        ///     is empty or the result types of the contained expressions do not match the requirements of
        ///     <paramref name="instanceType" />
        ///     (as explained in the remarks section).
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         if <paramref name="instanceType" /> is a a collection type then every expression in <paramref name="arguments" /> must have a result type that is promotable to the element type of the
        ///         <paramref
        ///             name="instanceType" />
        ///         .
        ///     </para>
        ///     <para>
        ///         if <paramref name="instanceType" /> is a row type, <paramref name="arguments" /> must contain as many expressions as there are columns in the row type, and the result type of each expression must be equal or promotable to the type of the corresponding column. A row type that does not declare any columns is invalid.
        ///     </para>
        ///     <para>
        ///         if <paramref name="instanceType" /> is an entity type, <paramref name="arguments" /> must contain as many expressions as there are properties defined by the type, and the result type of each expression must be equal or promotable to the type of the corresponding property.
        ///     </para>
        /// </remarks>
        public static DbNewInstanceExpression New(this TypeUsage instanceType, IEnumerable<DbExpression> arguments)
        {
            Check.NotNull(instanceType, "instanceType");

            return NewInstance(instanceType, arguments);
        }

        /// <summary>
        ///     Creates a new <see cref="DbNewInstanceExpression" />. If the type argument is a collection type, the arguments specify the elements of the collection. Otherwise the arguments are used as property or column values in the new instance.
        /// </summary>
        /// <param name="instanceType"> The type of the new instance. </param>
        /// <param name="arguments"> Expressions that specify values of the new instances, interpreted according to the instance's type. </param>
        /// <returns> A new DbNewInstanceExpression with the specified type and arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="instanceType" />
        ///     or
        ///     <paramref name="arguments" />
        ///     is null, or
        ///     <paramref name="arguments" />
        ///     contains null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="arguments" />
        ///     is empty or the result types of the contained expressions do not match the requirements of
        ///     <paramref name="instanceType" />
        ///     (as explained in the remarks section).
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         if <paramref name="instanceType" /> is a a collection type then every expression in <paramref name="arguments" /> must have a result type that is promotable to the element type of the
        ///         <paramref
        ///             name="instanceType" />
        ///         .
        ///     </para>
        ///     <para>
        ///         if <paramref name="instanceType" /> is a row type, <paramref name="arguments" /> must contain as many expressions as there are columns in the row type, and the result type of each expression must be equal or promotable to the type of the corresponding column. A row type that does not declare any columns is invalid.
        ///     </para>
        ///     <para>
        ///         if <paramref name="instanceType" /> is an entity type, <paramref name="arguments" /> must contain as many expressions as there are properties defined by the type, and the result type of each expression must be equal or promotable to the type of the corresponding property.
        ///     </para>
        /// </remarks>
        public static DbNewInstanceExpression New(this TypeUsage instanceType, params DbExpression[] arguments)
        {
            Check.NotNull(instanceType, "instanceType");

            return NewInstance(instanceType, arguments);
        }

        private static DbNewInstanceExpression NewInstance(TypeUsage instanceType, IEnumerable<DbExpression> arguments)
        {
            DbExpressionList validArguments;
            var resultType = ArgumentValidation.ValidateNew(instanceType, arguments, out validArguments);
            return new DbNewInstanceExpression(resultType, validArguments);
        }

        /// <summary>
        ///     Creates a new <see cref="DbNewInstanceExpression" /> that constructs a collection containing the specified elements. The type of the collection is based on the common type of the elements. If no common element type exists an exception is thrown.
        /// </summary>
        /// <param name="elements"> A list of expressions that provide the elements of the collection </param>
        /// <returns> A new DbNewInstanceExpression with the specified collection type and arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="elements" />
        ///     is null, or contains null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="elements" />
        ///     is empty or contains expressions for which no common result type exists.
        /// </exception>
        public static DbNewInstanceExpression NewCollection(IEnumerable<DbExpression> elements)
        {
            return CreateNewCollection(elements);
        }

        /// <summary>
        ///     Creates a new <see cref="DbNewInstanceExpression" /> that constructs a collection containing the specified elements. The type of the collection is based on the common type of the elements. If no common element type exists an exception is thrown.
        /// </summary>
        /// <param name="elements"> A list of expressions that provide the elements of the collection </param>
        /// <returns> A new DbNewInstanceExpression with the specified collection type and arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="elements" />
        ///     is null, or contains null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="elements" />
        ///     is empty or contains expressions for which no common result type exists.
        /// </exception>
        public static DbNewInstanceExpression NewCollection(params DbExpression[] elements)
        {
            return CreateNewCollection(elements);
        }

        private static DbNewInstanceExpression CreateNewCollection(IEnumerable<DbExpression> elements)
        {
            DbExpressionList validElements;
            var collectionResultType = ArgumentValidation.ValidateNewCollection(elements, out validElements);
            return new DbNewInstanceExpression(collectionResultType, validElements);
        }

        /// <summary>
        ///     Creates a new <see cref="DbNewInstanceExpression" /> that constructs an empty collection of the specified collection type.
        /// </summary>
        /// <param name="collectionType"> The type metadata for the collection to create </param>
        /// <returns>
        ///     A new DbNewInstanceExpression with the specified collection type and an empty <code>Arguments</code> list.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="collectionType" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="collectionType" />
        ///     is not a collection type
        /// </exception>
        public static DbNewInstanceExpression NewEmptyCollection(this TypeUsage collectionType)
        {
            Check.NotNull(collectionType, "collectionType");

            DbExpressionList validElements;
            var validResultType = ArgumentValidation.ValidateNewEmptyCollection(collectionType, out validElements);
            return new DbNewInstanceExpression(validResultType, validElements);
        }

        /// <summary>
        ///     Creates a new <see cref="DbNewInstanceExpression" /> that produces a row with the specified named columns and the given values, specified as expressions.
        /// </summary>
        /// <param name="columnValues"> A list of string-DbExpression key-value pairs that defines the structure and values of the row. </param>
        /// <returns> A new DbNewInstanceExpression that represents the construction of the row. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="columnValues" />
        ///     is null or contains an element with a null column name or expression
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="columnValues" />
        ///     is empty, or contains a duplicate or invalid column name
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DbNewInstanceExpression NewRow(IEnumerable<KeyValuePair<string, DbExpression>> columnValues)
        {
            DbExpressionList validElements;
            var resultType = ArgumentValidation.ValidateNewRow(columnValues, out validElements);
            return new DbNewInstanceExpression(resultType, validElements);
        }

        /// <summary>
        ///     Creates a new <see cref="DbPropertyExpression" /> representing the retrieval of the specified property.
        /// </summary>
        /// <param name="instance"> The instance from which to retrieve the property. May be null if the property is static. </param>
        /// <param name="propertyMetadata"> Metadata for the property to retrieve. </param>
        /// <returns> A new DbPropertyExpression representing the property retrieval. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="propertyMetadata" />
        ///     is null or
        ///     <paramref name="instance" />
        ///     is null and the property is not static.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static DbPropertyExpression Property(this DbExpression instance, EdmProperty propertyMetadata)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(propertyMetadata, "propertyMetadata");

            return PropertyFromMember(instance, propertyMetadata, "propertyMetadata");
        }

        /// <summary>
        ///     Creates a new <see cref="DbPropertyExpression" /> representing the retrieval of the specified navigation property.
        /// </summary>
        /// <param name="instance"> The instance from which to retrieve the navigation property. </param>
        /// <param name="navigationProperty"> Metadata for the navigation property to retrieve. </param>
        /// <returns> A new DbPropertyExpression representing the navigation property retrieval. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="navigationProperty" />
        ///     is null or
        ///     <paramref name="instance" />
        ///     is null.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static DbPropertyExpression Property(this DbExpression instance, NavigationProperty navigationProperty)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(navigationProperty, "navigationProperty");

            return PropertyFromMember(instance, navigationProperty, "navigationProperty");
        }

        /// <summary>
        ///     Creates a new <see cref="DbPropertyExpression" /> representing the retrieval of the specified relationship end member.
        /// </summary>
        /// <param name="instance"> The instance from which to retrieve the relationship end member. </param>
        /// <param name="relationshipEnd"> Metadata for the relationship end member to retrieve. </param>
        /// <returns> A new DbPropertyExpression representing the relationship end member retrieval. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="relationshipEnd" />
        ///     is null or
        ///     <paramref name="instance" />
        ///     is null and the property is not static.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "required for this feature")]
        public static DbPropertyExpression Property(this DbExpression instance, RelationshipEndMember relationshipEnd)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(relationshipEnd, "relationshipEnd");

            return PropertyFromMember(instance, relationshipEnd, "relationshipEnd");
        }

        /// <summary>
        ///     Creates a new <see cref="DbPropertyExpression" /> representing the retrieval of the instance property with the specified name from the given instance.
        /// </summary>
        /// <param name="propertyName"> The name of the property to retrieve. </param>
        /// <param name="instance"> The instance from which to retrieve the property. </param>
        /// <returns> A new DbPropertyExpression that represents the property retrieval </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="propertyName" />
        ///     is null or
        ///     <paramref name="instance" />
        ///     is null and the property is not static.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     No property with the specified name is declared by the type of
        ///     <paramref name="instance" />
        ///     .
        /// </exception>
        public static DbPropertyExpression Property(this DbExpression instance, string propertyName)
        {
            return PropertyByName(instance, propertyName, false);
        }

        private static DbPropertyExpression PropertyFromMember(DbExpression instance, EdmMember property, string propertyArgumentName)
        {
            var resultType = ArgumentValidation.ValidateProperty(instance, property, propertyArgumentName);
            return new DbPropertyExpression(resultType, property, instance);
        }

        private static DbPropertyExpression PropertyByName(DbExpression instance, string propertyName, bool ignoreCase)
        {
            Check.NotNull(instance, "instance");
            Check.NotNull(propertyName, "propertyName");

            EdmMember property;
            var resultType = ArgumentValidation.ValidateProperty(instance, propertyName, ignoreCase, out property);
            return new DbPropertyExpression(resultType, property, instance);
        }

        #endregion

        #region Lambda-based methods: All, Any, Cross|OuterApply, Cross|FullOuter|Inner|LeftOuterJoin, Filter, GroupBy, Project, Skip, Sort

        private static string ExtractAlias(MethodInfo method)
        {
            DebugCheck.NotNull(method);
            var aliases = ExtractAliases(method);
            Debug.Assert(aliases.Length > 0, "Incompatible method: at least one parameter is required");
            return aliases[0];
        }

        internal static string[] ExtractAliases(MethodInfo method)
        {
            DebugCheck.NotNull(method);
            var methodParams = method.GetParameters();
            int start;
            int paramCount;
            if (method.IsStatic
                && typeof(Closure) == methodParams[0].ParameterType)
            {
                // Static lambda method has additional first closure parameter
                start = 1;
                paramCount = methodParams.Length - 1;
            }
            else
            {
                // Otherwise, method parameters align directly with arguments
                start = 0;
                paramCount = methodParams.Length;
            }

            var paramNames = new string[paramCount];
            var generateNames = methodParams.Skip(start).Any(p => p.Name == null);
            for (var idx = start; idx < methodParams.Length; idx++)
            {
                paramNames[idx - start] = (generateNames ? _bindingAliases.Next() : methodParams[idx].Name);
            }
            return paramNames;
        }

        private static DbExpressionBinding ConvertToBinding<TResult>(
            DbExpression source, Func<DbExpression, TResult> argument, out TResult argumentResult)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(argument);

            var alias = ExtractAlias(argument.Method);
            var binding = source.BindAs(alias);
            argumentResult = argument(binding.Variable);
            return binding;
        }

        private static DbExpressionBinding[] ConvertToBinding(
            DbExpression left, DbExpression right,
            Func<DbExpression, DbExpression, DbExpression> argument, out DbExpression argumentExp)
        {
            var aliases = ExtractAliases(argument.Method);
            var leftBinding = left.BindAs(aliases[0]);
            var rightBinding = right.BindAs(aliases[1]);
            argumentExp = argument(leftBinding.Variable, rightBinding.Variable);
            return new[] { leftBinding, rightBinding };
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool TryGetAnonymousTypeValues<TInstance, TRequired>(
            object instance, out List<KeyValuePair<string, TRequired>> values)
        {
            DebugCheck.NotNull(instance);

            // The following heuristic is used to approximate whether or not TInstance is an anonymous type:
            // - Derived directly from System.Object
            // - Declares only public instance properties
            // - All public instance properties are readable and of an appropriate type

            values = null;
            if (typeof(TInstance).BaseType.Equals(typeof(object))
                &&
                typeof(TInstance).GetProperties(BindingFlags.Static).Length == 0
                &&
                typeof(TInstance).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Length == 0)
            {
                List<KeyValuePair<string, TRequired>> foundValues = null;
                foreach (var pi in typeof(TInstance).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (pi.CanRead
                        && typeof(TRequired).IsAssignableFrom(pi.PropertyType))
                    {
                        if (foundValues == null)
                        {
                            foundValues = new List<KeyValuePair<string, TRequired>>();
                        }
                        foundValues.Add(new KeyValuePair<string, TRequired>(pi.Name, (TRequired)pi.GetValue(instance, null)));
                    }
                    else
                    {
                        foundValues = null;
                        break;
                    }
                }
                values = foundValues;
            }

            return (values != null);
        }

        private static bool TryResolveToConstant(Type type, object value, out DbExpression constantOrNullExpression)
        {
            constantOrNullExpression = null;

            var valueType = type;
            if (type.IsGenericType
                && typeof(Nullable<>).Equals(type.GetGenericTypeDefinition()))
            {
                valueType = type.GetGenericArguments()[0];
            }

            PrimitiveTypeKind primitiveTypeKind;
            if (ClrProviderManifest.TryGetPrimitiveTypeKind(valueType, out primitiveTypeKind))
            {
                var resultType = TypeHelpers.GetLiteralTypeUsage(primitiveTypeKind);
                if (null == value)
                {
                    constantOrNullExpression = resultType.Null();
                }
                else
                {
                    constantOrNullExpression = resultType.Constant(value);
                }
            }

            return (constantOrNullExpression != null);
        }

        private static DbExpression ResolveToExpression<TArgument>(TArgument argument)
        {
            object untypedArgument = argument;

            DbExpression constantResult;
            if (TryResolveToConstant(typeof(TArgument), untypedArgument, out constantResult))
            {
                return constantResult;
            }

            if (null == untypedArgument)
            {
                return null;
            }

            // Direct DbExpression result
            if (typeof(DbExpression).IsAssignableFrom(typeof(TArgument)))
            {
                return (DbExpression)untypedArgument;
            }

            // Row
            if (typeof(Row).Equals(typeof(TArgument)))
            {
                return ((Row)untypedArgument).ToExpression();
            }

            // Conversion from anonymous type instance to DbNewInstanceExpression of a corresponding row type
            List<KeyValuePair<string, DbExpression>> columnValues;
            if (TryGetAnonymousTypeValues<TArgument, DbExpression>(untypedArgument, out columnValues))
            {
                return NewRow(columnValues);
            }

            // The specified instance cannot be resolved to a DbExpression
            throw new NotSupportedException(Strings.Cqt_Factory_MethodResultTypeNotSupported(typeof(TArgument).FullName));
        }

        private static DbApplyExpression CreateApply(
            DbExpression source, Func<DbExpression, KeyValuePair<string, DbExpression>> apply,
            Func<DbExpressionBinding, DbExpressionBinding, DbApplyExpression> resultBuilder)
        {
            KeyValuePair<string, DbExpression> applyTemplate;
            var sourceBinding = ConvertToBinding(source, apply, out applyTemplate);
            var applyBinding = applyTemplate.Value.BindAs(applyTemplate.Key);
            return resultBuilder(sourceBinding, applyBinding);
        }

        /// <summary>
        ///     Creates a new <see cref="DbQuantifierExpression" /> that determines whether the given predicate holds for all elements of the input set.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="predicate"> A method representing a predicate to evaluate for each member of the input set. This method must produce an expression with a Boolean result type that provides the predicate logic. </param>
        /// <returns> A new DbQuantifierExpression that represents the All operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="predicate" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbQuantifierExpression All(this DbExpression source, Func<DbExpression, DbExpression> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            DbExpression predicateExp;
            var input = ConvertToBinding(source, predicate, out predicateExp);
            return input.All(predicateExp);
        }

        /// <summary>
        ///     Creates a new <see cref="DbExpression" /> that determines whether the specified set argument is non-empty.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set </param>
        /// <returns>
        ///     A new <see cref="DbNotExpression" /> applied to a new <see cref="DbIsEmptyExpression" /> with the specified argument.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        public static DbExpression Any(this DbExpression source)
        {
            return source.Exists();
        }

        /// <summary>
        ///     Creates a new <see cref="DbExpression" /> that determines whether the specified set argument is non-empty.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input set </param>
        /// <returns>
        ///     A new <see cref="DbNotExpression" /> applied to a new <see cref="DbIsEmptyExpression" /> with the specified argument.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type.
        /// </exception>
        public static DbExpression Exists(this DbExpression argument)
        {
            return argument.IsEmpty().Not();
        }

        /// <summary>
        ///     Creates a new <see cref="DbQuantifierExpression" /> that determines whether the given predicate holds for any element of the input set.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="predicate"> A method representing the predicate to evaluate for each member of the input set. This method must produce an expression with a Boolean result type that provides the predicate logic. </param>
        /// <returns> A new DbQuantifierExpression that represents the Any operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="predicate" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbQuantifierExpression Any(this DbExpression source, Func<DbExpression, DbExpression> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            DbExpression predicateExp;
            var input = ConvertToBinding(source, predicate, out predicateExp);
            return input.Any(predicateExp);
        }

        /// <summary>
        ///     Creates a new <see cref="DbApplyExpression" /> that evaluates the given <paramref name="apply" /> expression once for each element of a given input set,
        ///     producing a collection of rows with corresponding input and apply columns. Rows for which <paramref name="apply" /> evaluates to an empty set are not included.
        /// </summary>
        /// <param name="source">
        ///     A <see cref="DbExpression" /> that specifies the input set.
        /// </param>
        /// <param name="apply"> A method that specifies the logic to evaluate once for each member of the input set. </param>
        /// <returns>
        ///     An new DbApplyExpression with the specified input and apply bindings and an <see cref="DbExpressionKind" /> of CrossApply.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="apply" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The result of
        ///     <paramref name="apply" />
        ///     contains a name or expression that is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The result of
        ///     <paramref name="apply" />
        ///     contains a name or expression that is not valid in an expression binding.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DbApplyExpression CrossApply(this DbExpression source, Func<DbExpression, KeyValuePair<string, DbExpression>> apply)
        {
            Check.NotNull(source, "source");
            Check.NotNull(apply, "apply");

            return CreateApply(source, apply, DbExpressionBuilder.CrossApply);
        }

        /// <summary>
        ///     Creates a new <see cref="DbApplyExpression" /> that evaluates the given <paramref name="apply" /> expression once for each element of a given input set,
        ///     producing a collection of rows with corresponding input and apply columns. Rows for which <paramref name="apply" /> evaluates to an empty set have an apply column value of <code>null</code>.
        /// </summary>
        /// <param name="source">
        ///     A <see cref="DbExpression" /> that specifies the input set.
        /// </param>
        /// <param name="apply"> A method that specifies the logic to evaluate once for each member of the input set. </param>
        /// <returns>
        ///     An new DbApplyExpression with the specified input and apply bindings and an <see cref="DbExpressionKind" /> of OuterApply.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="apply" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The result of
        ///     <paramref name="apply" />
        ///     contains a name or expression that is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The result of
        ///     <paramref name="apply" />
        ///     contains a name or expression that is not valid in an expression binding.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static DbApplyExpression OuterApply(this DbExpression source, Func<DbExpression, KeyValuePair<string, DbExpression>> apply)
        {
            Check.NotNull(source, "source");
            Check.NotNull(apply, "apply");

            return CreateApply(source, apply, DbExpressionBuilder.OuterApply);
        }

        /// <summary>
        ///     Creates a new <see cref="DbJoinExpression" /> that joins the sets specified by the left and right expressions,
        ///     on the specified join condition, using FullOuterJoin as the <see cref="DbExpressionKind" />.
        /// </summary>
        /// <param name="left">
        ///     A <see cref="DbExpression" /> that specifies the left set argument.
        /// </param>
        /// <param name="right">
        ///     A <see cref="DbExpression" /> that specifies the right set argument.
        /// </param>
        /// <param name="joinCondition"> A method representing the condition on which to join. This method must produce an expression with a Boolean result type that provides the logic of the join condition. </param>
        /// <returns>
        ///     A new DbJoinExpression, with an <see cref="DbExpressionKind" /> of FullOuterJoin, that represents the full outer join operation applied to the left and right input sets under the given join condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     ,
        ///     <paramref name="right" />
        ///     or
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="joinCondition" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbJoinExpression FullOuterJoin(
            this DbExpression left, DbExpression right, Func<DbExpression, DbExpression, DbExpression> joinCondition)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(joinCondition, "joinCondition");

            DbExpression condExp;
            var inputs = ConvertToBinding(left, right, joinCondition, out condExp);
            return inputs[0].FullOuterJoin(inputs[1], condExp);
        }

        /// <summary>
        ///     Creates a new <see cref="DbJoinExpression" /> that joins the sets specified by the left and right expressions,
        ///     on the specified join condition, using InnerJoin as the <see cref="DbExpressionKind" />.
        /// </summary>
        /// <param name="left">
        ///     A <see cref="DbExpression" /> that specifies the left set argument.
        /// </param>
        /// <param name="right">
        ///     A <see cref="DbExpression" /> that specifies the right set argument.
        /// </param>
        /// <param name="joinCondition"> A method representing the condition on which to join. This method must produce an expression with a Boolean result type that provides the logic of the join condition. </param>
        /// <returns>
        ///     A new DbJoinExpression, with an <see cref="DbExpressionKind" /> of InnerJoin, that represents the inner join operation applied to the left and right input sets under the given join condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     ,
        ///     <paramref name="right" />
        ///     or
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="joinCondition" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbJoinExpression InnerJoin(
            this DbExpression left, DbExpression right, Func<DbExpression, DbExpression, DbExpression> joinCondition)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(joinCondition, "joinCondition");

            DbExpression condExp;
            var inputs = ConvertToBinding(left, right, joinCondition, out condExp);
            return inputs[0].InnerJoin(inputs[1], condExp);
        }

        /// <summary>
        ///     Creates a new <see cref="DbJoinExpression" /> that joins the sets specified by the left and right expressions,
        ///     on the specified join condition, using LeftOuterJoin as the <see cref="DbExpressionKind" />.
        /// </summary>
        /// <param name="left">
        ///     A <see cref="DbExpression" /> that specifies the left set argument.
        /// </param>
        /// <param name="right">
        ///     A <see cref="DbExpression" /> that specifies the right set argument.
        /// </param>
        /// <param name="joinCondition"> A method representing the condition on which to join. This method must produce an expression with a Boolean result type that provides the logic of the join condition. </param>
        /// <returns>
        ///     A new DbJoinExpression, with an <see cref="DbExpressionKind" /> of LeftOuterJoin, that represents the left outer join operation applied to the left and right input sets under the given join condition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     ,
        ///     <paramref name="right" />
        ///     or
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="joinCondition" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="joinCondition" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbJoinExpression LeftOuterJoin(
            this DbExpression left, DbExpression right, Func<DbExpression, DbExpression, DbExpression> joinCondition)
        {
            Check.NotNull(left, "left");
            Check.NotNull(right, "right");
            Check.NotNull(joinCondition, "joinCondition");

            DbExpression condExp;
            var inputs = ConvertToBinding(left, right, joinCondition, out condExp);
            return inputs[0].LeftOuterJoin(inputs[1], condExp);
        }

        /// <summary>
        ///     Creates a new <see cref="DbJoinExpression" /> that joins the sets specified by the outer and inner expressions,
        ///     on an equality condition between the specified outer and inner keys, using InnerJoin as the
        ///     <see
        ///         cref="DbExpressionKind" />
        ///     .
        /// </summary>
        /// <param name="outer">
        ///     A <see cref="DbExpression" /> that specifies the outer set argument.
        /// </param>
        /// <param name="inner">
        ///     A <see cref="DbExpression" /> that specifies the inner set argument.
        /// </param>
        /// <param name="outerKey"> A method that specifies how the outer key value should be derived from an element of the outer set. </param>
        /// <param name="innerKey"> A method that specifies how the inner key value should be derived from an element of the inner set. </param>
        /// <returns>
        ///     A new DbJoinExpression, with an <see cref="DbExpressionKind" /> of InnerJoin, that represents the inner join operation applied to the left and right input sets under a join condition that compares the outer and inner key values for equality.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="outer" />
        ///     ,
        ///     <paramref name="inner" />
        ///     ,
        ///     <paramref name="outerKey" />
        ///     or
        ///     <paramref name="innerKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="outer" />
        ///     or
        ///     <paramref name="inner" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="outerKey" />
        ///     or
        ///     <paramref name="innerKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expressions produced by
        ///     <paramref name="outerKey" />
        ///     and
        ///     <paramref name="innerKey" />
        ///     are not comparable for equality.
        /// </exception>
        public static DbJoinExpression Join(
            this DbExpression outer, DbExpression inner, Func<DbExpression, DbExpression> outerKey,
            Func<DbExpression, DbExpression> innerKey)
        {
            Check.NotNull(outer, "outer");
            Check.NotNull(inner, "inner");
            Check.NotNull(outerKey, "outerKey");
            Check.NotNull(innerKey, "innerKey");

            DbExpression leftOperand;
            var leftBinding = ConvertToBinding(outer, outerKey, out leftOperand);

            DbExpression rightOperand;
            var rightBinding = ConvertToBinding(inner, innerKey, out rightOperand);

            DbExpression joinCondition = leftOperand.Equal(rightOperand);

            return leftBinding.InnerJoin(rightBinding, joinCondition);
        }

        /// <summary>
        ///     Creates a new <see cref="DbProjectExpression" /> that projects the specified selector over the sets specified by the outer and inner
        ///     expressions, joined on an equality condition between the specified outer and inner keys, using InnerJoin as the
        ///     <see
        ///         cref="DbExpressionKind" />
        ///     .
        /// </summary>
        /// <param name="outer">
        ///     A <see cref="DbExpression" /> that specifies the outer set argument.
        /// </param>
        /// <param name="inner">
        ///     A <see cref="DbExpression" /> that specifies the inner set argument.
        /// </param>
        /// <param name="outerKey"> A method that specifies how the outer key value should be derived from an element of the outer set. </param>
        /// <param name="innerKey"> A method that specifies how the inner key value should be derived from an element of the inner set. </param>
        /// <param name="selector">
        ///     A method that specifies how an element of the result set should be derived from elements of the inner and outer sets. This method must produce an instance of a type that is compatible with Join and can be resolved into a
        ///     <see
        ///         cref="DbExpression" />
        ///     . Compatibility requirements for <typeparamref name="TSelector" /> are described in remarks.
        /// </param>
        /// <returns>
        ///     A new DbProjectExpression with the specified selector as its projection, and a new DbJoinExpression as its input. The input DbJoinExpression is created with an
        ///     <see
        ///         cref="DbExpressionKind" />
        ///     of InnerJoin, that represents the inner join operation applied to the left and right input sets under a join condition that compares the outer and inner key values for equality.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="outer" />
        ///     ,
        ///     <paramref name="inner" />
        ///     ,
        ///     <paramref name="outerKey" />
        ///     ,
        ///     <paramref name="innerKey" />
        ///     or
        ///     <paramref name="selector" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="outer" />
        ///     or
        ///     <paramref name="inner" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="outerKey" />
        ///     or
        ///     <paramref name="innerKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The result of
        ///     <paramref name="selector" />
        ///     is null after conversion to DbExpression.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expressions produced by
        ///     <paramref name="outerKey" />
        ///     and
        ///     <paramref name="innerKey" />
        ///     are not comparable for equality.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The result of
        ///     <paramref name="selector" />
        ///     is not compatible with SelectMany.
        /// </exception>
        /// <remarks>
        ///     To be compatible with Join, <typeparamref name="TSelector" /> must be derived from <see cref="DbExpression" />,
        ///     or must be an anonymous type with DbExpression-derived properties.
        ///     <para>
        ///         The following are examples of supported types for <typeparamref name="TSelector" /> : <code>outer.Join(inner, o => o.Property("ID"), i => i.Property("ID"), (o, i) => o.Property("Name"))</code> (
        ///         <typeparamref
        ///             name="TSelector" />
        ///         is <see cref="DbPropertyExpression" /> ). <code>outer.Join(inner, o => o.Property("ID"), i => i.Property("ID"), (o, i) => new { OName = o.Property("Name"), IName = i.Property("Name") })</code> (
        ///         <typeparamref
        ///             name="TSelector" />
        ///         is an anonymous type with DbExpression-derived properties).
        ///     </para>
        /// </remarks>
        public static DbProjectExpression Join<TSelector>(
            this DbExpression outer, DbExpression inner, Func<DbExpression, DbExpression> outerKey,
            Func<DbExpression, DbExpression> innerKey, Func<DbExpression, DbExpression, TSelector> selector)
        {
            Check.NotNull(selector, "selector");

            // Defer argument validation for all but the selector to the selector-less overload of Join
            var joinExpression = outer.Join(inner, outerKey, innerKey);

            // Bind the join expression and produce the selector based on the left and right inputs
            var joinBinding = joinExpression.Bind();
            DbExpression left = joinBinding.Variable.Property(joinExpression.Left.VariableName);
            DbExpression right = joinBinding.Variable.Property(joinExpression.Right.VariableName);
            var intermediateSelector = selector(left, right);
            var projection = ResolveToExpression(intermediateSelector);

            // Project the selector over the join expression and return the resulting DbProjectExpression
            return joinBinding.Project(projection);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that sorts the given input set by the specified sort key,
        ///     with ascending sort order and default collation.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <returns> A new DbSortExpression that represents the order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable result type.
        /// </exception>
        public static DbSortExpression OrderBy(this DbExpression source, Func<DbExpression, DbExpression> sortKey)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            DbExpression keyExpression;
            var input = ConvertToBinding(source, sortKey, out keyExpression);
            var sortClause = keyExpression.ToSortClause();
            return input.Sort(new[] { sortClause });
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that sorts the given input set by the specified sort key,
        ///     with ascending sort order and the specified collation.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <param name="collation"> The collation to sort under </param>
        /// <returns> A new DbSortExpression that represents the order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     ,
        ///     <paramref name="sortKey" />
        ///     or
        ///     <paramref name="collation" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable string result type.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="collation" />
        ///     is empty or contains only space characters
        /// </exception>
        public static DbSortExpression OrderBy(this DbExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            DbExpression keyExpression;
            var input = ConvertToBinding(source, sortKey, out keyExpression);
            var sortClause = keyExpression.ToSortClause(collation);
            return input.Sort(new[] { sortClause });
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that sorts the given input set by the specified sort key,
        ///     with descending sort order and default collation.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <returns> A new DbSortExpression that represents the order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable result type.
        /// </exception>
        public static DbSortExpression OrderByDescending(this DbExpression source, Func<DbExpression, DbExpression> sortKey)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            DbExpression keyExpression;
            var input = ConvertToBinding(source, sortKey, out keyExpression);
            var sortClause = keyExpression.ToSortClauseDescending();
            return input.Sort(new[] { sortClause });
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that sorts the given input set by the specified sort key,
        ///     with descending sort order and the specified collation.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <param name="collation"> The collation to sort under </param>
        /// <returns> A new DbSortExpression that represents the order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     ,
        ///     <paramref name="sortKey" />
        ///     or
        ///     <paramref name="collation" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable string result type.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="collation" />
        ///     is empty or contains only space characters
        /// </exception>
        public static DbSortExpression OrderByDescending(
            this DbExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            DbExpression keyExpression;
            var input = ConvertToBinding(source, sortKey, out keyExpression);
            var sortClause = keyExpression.ToSortClauseDescending(collation);
            return input.Sort(new[] { sortClause });
        }

        /// <summary>
        ///     Creates a new <see cref="DbProjectExpression" /> that selects the specified expression over the given input set.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="projection">
        ///     A method that specifies how to derive the projected expression given a member of the input set. This method must produce an instance of a type that is compatible with Select and can be resolved into a
        ///     <see
        ///         cref="DbExpression" />
        ///     . Compatibility requirements for <typeparamref name="TProjection" /> are described in remarks.
        /// </param>
        /// <typeparam name="TProjection">
        ///     The method result type of <paramref name="projection" /> .
        /// </typeparam>
        /// <returns> A new DbProjectExpression that represents the select operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="projection" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The result of
        ///     <paramref name="projection" />
        ///     is null.
        /// </exception>
        /// <remarks>
        ///     To be compatible with Select, <typeparamref name="TProjection" /> must be derived from <see cref="DbExpression" />,
        ///     or must be an anonymous type with DbExpression-derived properties.
        ///     <para>
        ///         The following are examples of supported types for <typeparamref name="TProjection" /> : <code>source.Select(x => x.Property("Name"))</code> (
        ///         <typeparamref
        ///             name="TProjection" />
        ///         is <see cref="DbPropertyExpression" /> ). <code>source.Select(x => new { Name = x.Property("Name") })</code> (
        ///         <typeparamref
        ///             name="TProjection" />
        ///         is an anonymous type with a DbExpression-derived property).
        ///     </para>
        /// </remarks>
        public static DbProjectExpression Select<TProjection>(this DbExpression source, Func<DbExpression, TProjection> projection)
        {
            Check.NotNull(source, "source");
            Check.NotNull(projection, "projection");

            TProjection intermediateProjection;
            var input = ConvertToBinding(source, projection, out intermediateProjection);
            var projectionExp = ResolveToExpression(intermediateProjection);
            return input.Project(projectionExp);
        }

        /// <summary>
        ///     Creates a new <see cref="DbApplyExpression" /> that evaluates the given <paramref name="apply" /> expression once for each element of a given input set,
        ///     producing a collection of rows with corresponding input and apply columns. Rows for which <paramref name="apply" /> evaluates to an empty set are not included.
        ///     A <see cref="DbProjectExpression" /> is then created that selects the <paramref name="apply" /> column from each row, producing the overall collection of
        ///     <paramref
        ///         name="apply" />
        ///     results.
        /// </summary>
        /// <param name="source">
        ///     A <see cref="DbExpression" /> that specifies the input set.
        /// </param>
        /// <param name="apply"> A method that represents the logic to evaluate once for each member of the input set. </param>
        /// <returns>
        ///     An new DbProjectExpression that selects the apply column from a new DbApplyExpression with the specified input and apply bindings and an
        ///     <see
        ///         cref="DbExpressionKind" />
        ///     of CrossApply.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="apply" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="apply" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="apply" />
        ///     does not have a collection type.
        /// </exception>
        public static DbProjectExpression SelectMany(this DbExpression source, Func<DbExpression, DbExpression> apply)
        {
            Check.NotNull(source, "source");
            Check.NotNull(apply, "apply");

            DbExpression functorResult;
            var inputBinding = ConvertToBinding(source, apply, out functorResult);

            var functorBinding = functorResult.Bind();
            var intermediateApply = inputBinding.CrossApply(functorBinding);

            var projectionBinding = intermediateApply.Bind();
            return projectionBinding.Project(projectionBinding.Variable.Property(functorBinding.VariableName));
        }

        /// <summary>
        ///     Creates a new <see cref="DbApplyExpression" /> that evaluates the given <paramref name="apply" /> expression once for each element of a given input set,
        ///     producing a collection of rows with corresponding input and apply columns. Rows for which <paramref name="apply" /> evaluates to an empty set are not included.
        ///     A <see cref="DbProjectExpression" /> is then created that selects the specified <paramref name="selector" /> over each row, producing the overall collection of results.
        /// </summary>
        /// <typeparam name="TSelector">
        ///     The method result type of <paramref name="selector" /> .
        /// </typeparam>
        /// <param name="source">
        ///     A <see cref="DbExpression" /> that specifies the input set.
        /// </param>
        /// <param name="apply"> A method that represents the logic to evaluate once for each member of the input set. </param>
        /// <param name="selector">
        ///     A method that specifies how an element of the result set should be derived given an element of the input and apply sets. This method must produce an instance of a type that is compatible with SelectMany and can be resolved into a
        ///     <see
        ///         cref="DbExpression" />
        ///     . Compatibility requirements for <typeparamref name="TSelector" /> are described in remarks.
        /// </param>
        /// <returns>
        ///     An new DbProjectExpression that selects the result of the given selector from a new DbApplyExpression with the specified input and apply bindings and an
        ///     <see
        ///         cref="DbExpressionKind" />
        ///     of CrossApply.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     ,
        ///     <paramref name="apply" />
        ///     or
        ///     <paramref name="selector" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="apply" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The result of
        ///     <paramref name="selector" />
        ///     is null on conversion to DbExpression
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="apply" />
        ///     does not have a collection type.
        /// </exception>
        /// <remarks>
        ///     To be compatible with SelectMany, <typeparamref name="TSelector" /> must be derived from <see cref="DbExpression" />,
        ///     or must be an anonymous type with DbExpression-derived properties.
        ///     <para>
        ///         The following are examples of supported types for <typeparamref name="TSelector" /> : <code>source.SelectMany(x => x.Property("RelatedCollection"), (source, apply) => apply.Property("Name"))</code> (
        ///         <typeparamref
        ///             name="TSelector" />
        ///         is <see cref="DbPropertyExpression" /> ). <code>source.SelectMany(x => x.Property("RelatedCollection"), (source, apply) => new { SourceName = source.Property("Name"), RelatedName = apply.Property("Name") })</code> (
        ///         <typeparamref
        ///             name="TSelector" />
        ///         is an anonymous type with DbExpression-derived properties).
        ///     </para>
        /// </remarks>
        public static DbProjectExpression SelectMany<TSelector>(
            this DbExpression source, Func<DbExpression, DbExpression> apply, Func<DbExpression, DbExpression, TSelector> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(apply, "apply");
            Check.NotNull(selector, "selector");

            DbExpression functorResult;
            var inputBinding = ConvertToBinding(source, apply, out functorResult);

            var functorBinding = functorResult.Bind();
            var intermediateApply = inputBinding.CrossApply(functorBinding);

            var projectionBinding = intermediateApply.Bind();
            DbExpression left = projectionBinding.Variable.Property(inputBinding.VariableName);
            DbExpression right = projectionBinding.Variable.Property(functorBinding.VariableName);
            var selectorResult = selector(left, right);
            var projection = ResolveToExpression(selectorResult);
            return projectionBinding.Project(projection);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSkipExpression" /> that skips the specified number of elements from the given sorted input set.
        /// </summary>
        /// <param name="argument">
        ///     A <see cref="DbSortExpression" /> that specifies the sorted input set.
        /// </param>
        /// <param name="count"> An expression the specifies how many elements of the ordered set to skip. </param>
        /// <returns> A new DbSkipExpression that represents the skip operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="count" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="count" />
        ///     is not
        ///     <see cref="DbConstantExpression" />
        ///     or
        ///     <see cref="DbParameterReferenceExpression" />
        ///     or has a
        ///     result type that is not equal or promotable to a 64-bit integer type.
        /// </exception>
        public static DbSkipExpression Skip(this DbSortExpression argument, DbExpression count)
        {
            Check.NotNull(argument, "argument");

            return argument.Input.Skip(argument.SortOrder, count);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLimitExpression" /> that restricts the number of elements in the Argument collection to the specified count Limit value.
        ///     Tied results are not included in the output.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input collection. </param>
        /// <param name="count"> An expression that specifies the limit value. </param>
        /// <returns> A new DbLimitExpression with the specified argument and count limit values that does not include tied results. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     or
        ///     <paramref name="count" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     does not have a collection result type,
        ///     or
        ///     <paramref name="count" />
        ///     does not have a result type that is equal or promotable to a 64-bit integer type.
        /// </exception>
        public static DbLimitExpression Take(this DbExpression argument, DbExpression count)
        {
            Check.NotNull(argument, "argument");
            Check.NotNull(count, "count");

            return argument.Limit(count);
        }

        private static DbSortExpression CreateThenBy(
            DbSortExpression source, Func<DbExpression, DbExpression> sortKey, bool ascending, string collation, bool useCollation)
        {
            var sortKeyResult = sortKey(source.Input.Variable);
            DbSortClause sortClause;
            if (useCollation)
            {
                sortClause = (ascending ? sortKeyResult.ToSortClause(collation) : sortKeyResult.ToSortClauseDescending(collation));
            }
            else
            {
                sortClause = (ascending ? sortKeyResult.ToSortClause() : sortKeyResult.ToSortClauseDescending());
            }

            var newSortOrder = new List<DbSortClause>(source.SortOrder.Count + 1);
            newSortOrder.AddRange(source.SortOrder);
            newSortOrder.Add(sortClause);

            return source.Input.Sort(newSortOrder);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that with a sort order that includes the sort order
        ///     of the given order input set together with the specified sort key in ascending sort order and
        ///     with default collation.
        /// </summary>
        /// <param name="source"> A DbSortExpression that specifies the ordered input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the additional sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <returns> A new DbSortExpression that represents the new overall order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable result type.
        /// </exception>
        public static DbSortExpression ThenBy(this DbSortExpression source, Func<DbExpression, DbExpression> sortKey)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            return CreateThenBy(source, sortKey, true, null, false);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that with a sort order that includes the sort order
        ///     of the given order input set together with the specified sort key in ascending sort order and
        ///     with the specified collation.
        /// </summary>
        /// <param name="source"> A DbSortExpression that specifies the ordered input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the additional sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <param name="collation"> The collation to sort under </param>
        /// <returns> A new DbSortExpression that represents the new overall order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     ,
        ///     <paramref name="sortKey" />
        ///     or
        ///     <paramref name="collation" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable string result type.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="collation" />
        ///     is empty or contains only space characters
        /// </exception>
        public static DbSortExpression ThenBy(this DbSortExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            return CreateThenBy(source, sortKey, true, collation, true);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that with a sort order that includes the sort order
        ///     of the given order input set together with the specified sort key in descending sort order and
        ///     with default collation.
        /// </summary>
        /// <param name="source"> A DbSortExpression that specifies the ordered input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the additional sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <returns> A new DbSortExpression that represents the new overall order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable result type.
        /// </exception>
        public static DbSortExpression ThenByDescending(this DbSortExpression source, Func<DbExpression, DbExpression> sortKey)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            return CreateThenBy(source, sortKey, false, null, false);
        }

        /// <summary>
        ///     Creates a new <see cref="DbSortExpression" /> that with a sort order that includes the sort order
        ///     of the given order input set together with the specified sort key in descending sort order and
        ///     with the specified collation.
        /// </summary>
        /// <param name="source"> A DbSortExpression that specifies the ordered input set. </param>
        /// <param name="sortKey"> A method that specifies how to derive the additional sort key expression given a member of the input set. This method must produce an expression with an order-comparable result type that provides the sort key definition. </param>
        /// <param name="collation"> The collation to sort under </param>
        /// <returns> A new DbSortExpression that represents the new overall order-by operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     ,
        ///     <paramref name="sortKey" />
        ///     or
        ///     <paramref name="collation" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="source" />
        ///     does not have a collection result type.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="sortKey" />
        ///     does not have an order-comparable string result type.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="collation" />
        ///     is empty or contains only space characters
        /// </exception>
        public static DbSortExpression ThenByDescending(
            this DbSortExpression source, Func<DbExpression, DbExpression> sortKey, string collation)
        {
            Check.NotNull(source, "source");
            Check.NotNull(sortKey, "sortKey");

            return CreateThenBy(source, sortKey, false, collation, true);
        }

        /// <summary>
        ///     Creates a new <see cref="DbFilterExpression" /> that filters the elements in the given input set using the specified predicate.
        /// </summary>
        /// <param name="source"> An expression that specifies the input set. </param>
        /// <param name="predicate"> A method representing the predicate to evaluate for each member of the input set. This method must produce an expression with a Boolean result type that provides the predicate logic. </param>
        /// <returns> A new DbQuantifierExpression that represents the Any operation. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="source" />
        ///     or
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     The expression produced by
        ///     <paramref name="predicate" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     The expression produced by
        ///     <paramref name="predicate" />
        ///     does not have a Boolean result type.
        /// </exception>
        public static DbFilterExpression Where(this DbExpression source, Func<DbExpression, DbExpression> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            DbExpression predicateExp;
            var input = ConvertToBinding(source, predicate, out predicateExp);
            return input.Filter(predicateExp);
        }

        /// <summary>
        ///     Creates a new <see cref="DbExpression" /> that computes the union of the left and right set arguments with duplicates removed.
        /// </summary>
        /// <param name="left"> An expression that defines the left set argument. </param>
        /// <param name="right"> An expression that defines the right set argument. </param>
        /// <returns> A new DbExpression that computes the union, without duplicates, of the the left and right arguments. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="left" />
        ///     or
        ///     <paramref name="right" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No common collection result type with an equality-comparable element type exists between
        ///     <paramref name="left" />
        ///     and
        ///     <paramref name="right" />
        ///     .
        /// </exception>
        public static DbExpression Union(this DbExpression left, DbExpression right)
        {
            return left.UnionAll(right).Distinct();
        }

        #endregion

        #region Internal Helper API - ideally these methods should be removed

        internal static AliasGenerator AliasGenerator
        {
            get { return _bindingAliases; }
        }

        internal static DbNullExpression CreatePrimitiveNullExpression(PrimitiveTypeKind primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveTypeKind.Binary:
                    return _binaryNull;
                case PrimitiveTypeKind.Boolean:
                    return _boolNull;
                case PrimitiveTypeKind.Byte:
                    return _byteNull;
                case PrimitiveTypeKind.DateTime:
                    return _dateTimeNull;
                case PrimitiveTypeKind.DateTimeOffset:
                    return _dateTimeOffsetNull;
                case PrimitiveTypeKind.Decimal:
                    return _decimalNull;
                case PrimitiveTypeKind.Double:
                    return _doubleNull;
                case PrimitiveTypeKind.Geography:
                    return _geographyNull;
                case PrimitiveTypeKind.Geometry:
                    return _geometryNull;
                case PrimitiveTypeKind.Guid:
                    return _guidNull;
                case PrimitiveTypeKind.Int16:
                    return _int16Null;
                case PrimitiveTypeKind.Int32:
                    return _int32Null;
                case PrimitiveTypeKind.Int64:
                    return _int64Null;
                case PrimitiveTypeKind.SByte:
                    return _sbyteNull;
                case PrimitiveTypeKind.Single:
                    return _singleNull;
                case PrimitiveTypeKind.String:
                    return _stringNull;
                case PrimitiveTypeKind.Time:
                    return _timeNull;

                default:
                    var paramName = typeof(PrimitiveTypeKind).Name;
                    throw new ArgumentOutOfRangeException(
                        paramName,
                        Strings.ADP_InvalidEnumerationValue(paramName, ((int)primitiveType).ToString(CultureInfo.InvariantCulture)));
            }
        }

        internal static DbApplyExpression CreateApplyExpressionByKind(
            DbExpressionKind applyKind, DbExpressionBinding input, DbExpressionBinding apply)
        {
            Debug.Assert(DbExpressionKind.CrossApply == applyKind || DbExpressionKind.OuterApply == applyKind, "Invalid ApplyType");

            switch (applyKind)
            {
                case DbExpressionKind.CrossApply:
                    return CrossApply(input, apply);

                case DbExpressionKind.OuterApply:
                    return OuterApply(input, apply);

                default:
                    var paramName = typeof(DbExpressionKind).Name;
                    throw new ArgumentOutOfRangeException(
                        paramName, Strings.ADP_InvalidEnumerationValue(paramName, ((int)applyKind).ToString(CultureInfo.InvariantCulture)));
            }
        }

        internal static DbExpression CreateJoinExpressionByKind(
            DbExpressionKind joinKind, DbExpression joinCondition, DbExpressionBinding input1, DbExpressionBinding input2)
        {
            Debug.Assert(
                DbExpressionKind.CrossJoin == joinKind ||
                DbExpressionKind.FullOuterJoin == joinKind ||
                DbExpressionKind.InnerJoin == joinKind ||
                DbExpressionKind.LeftOuterJoin == joinKind,
                "Invalid DbExpressionKind for CreateJoinExpressionByKind");

            if (DbExpressionKind.CrossJoin == joinKind)
            {
                Debug.Assert(null == joinCondition, "Condition should not be specified for CrossJoin");
                return CrossJoin(new DbExpressionBinding[2] { input1, input2 });
            }
            else
            {
                Debug.Assert(joinCondition != null, "Condition must be specified for non-CrossJoin");

                switch (joinKind)
                {
                    case DbExpressionKind.InnerJoin:
                        return InnerJoin(input1, input2, joinCondition);

                    case DbExpressionKind.LeftOuterJoin:
                        return LeftOuterJoin(input1, input2, joinCondition);

                    case DbExpressionKind.FullOuterJoin:
                        return FullOuterJoin(input1, input2, joinCondition);

                    default:
                        var paramName = typeof(DbExpressionKind).Name;
                        throw new ArgumentOutOfRangeException(
                            paramName,
                            Strings.ADP_InvalidEnumerationValue(paramName, ((int)joinKind).ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        /// <summary>
        ///     Used only by span rewriter, when a row could be specified as an argument
        /// </summary>
        internal static DbIsNullExpression CreateIsNullExpressionAllowingRowTypeArgument(DbExpression argument)
        {
            var resultType = ArgumentValidation.ValidateIsNull(argument, true);
            return new DbIsNullExpression(resultType, argument);
        }

        /// <summary>
        ///     Creates a new <see cref="DbElementExpression" /> that converts a single-member set with a single property
        ///     into a singleton.  The result type of the created <see cref="DbElementExpression" /> equals the result type
        ///     of the single property of the element of the argument.
        ///     This method should only be used when the argument is of a collection type with
        ///     element of structured type with only one property.
        /// </summary>
        /// <param name="argument"> An expression that specifies the input set. </param>
        /// <returns> A DbElementExpression that represents the conversion of the single-member set argument to a singleton. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument" />
        ///     is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="argument" />
        ///     is associated with a different command tree,
        ///     or does not have a collection result type, or its element type is not a structured type
        ///     with only one property
        /// </exception>
        internal static DbElementExpression CreateElementExpressionUnwrapSingleProperty(DbExpression argument)
        {
            var resultType = ArgumentValidation.ValidateElement(argument);

            // Change the result type of the element expression to the type of the 
            // single property of the element of its operand.
            IList<EdmProperty> properties = TypeHelpers.GetProperties(resultType);
            if (properties == null
                || properties.Count != 1)
            {
                throw new ArgumentException(Strings.Cqt_Element_InvalidArgumentForUnwrapSingleProperty, "argument");
            }
            resultType = properties[0].TypeUsage;
            return new DbElementExpression(resultType, argument, true);
        }

        /// <summary>
        ///     Creates a new <see cref="DbRelatedEntityRef" /> that describes how to satisfy the relationship
        ///     navigation operation from <paramref name="sourceEnd" /> to <paramref name="targetEnd" />, which
        ///     must be declared by the same relationship type.
        ///     DbRelatedEntityRefs are used in conjuction with <see cref="DbNewInstanceExpression" />
        ///     to construct Entity instances that are capable of resolving relationship navigation operations based on
        ///     the provided DbRelatedEntityRefs without the need for additional navigation operations.
        ///     Note also that this factory method is not intended to be part of the public Command Tree API
        ///     since its intent is to support Entity constructors in view definitions that express information about
        ///     related Entities using the 'WITH RELATIONSHIP' clause in eSQL.
        /// </summary>
        /// <param name="sourceEnd"> The relationship end from which navigation takes place </param>
        /// <param name="targetEnd"> The relationship end to which navigation may be satisifed using the target entity ref </param>
        /// <param name="targetEntity"> An expression that produces a reference to the target entity (and must therefore have a Ref result type) </param>
        internal static DbRelatedEntityRef CreateRelatedEntityRef(
            RelationshipEndMember sourceEnd, RelationshipEndMember targetEnd, DbExpression targetEntity)
        {
            return new DbRelatedEntityRef(sourceEnd, targetEnd, targetEntity);
        }

        /// <summary>
        ///     Creates a new <see cref="DbNewInstanceExpression" /> that constructs an instance of an Entity type
        ///     together with the specified information about Entities related to the newly constructed Entity by
        ///     relationship navigations where the target end has multiplicity of at most one.
        ///     Note that this factory method is not intended to be part of the public Command Tree API since its
        ///     intent is to support Entity constructors in view definitions that express information about
        ///     related Entities using the 'WITH RELATIONSHIP' clause in eSQL.
        /// </summary>
        /// <param name="entityType"> The type of the Entity instance that is being constructed </param>
        /// <param name="attributeValues"> Values for each (non-relationship) property of the Entity </param>
        /// <param name="relationships">
        ///     A (possibly empty) list of <see cref="DbRelatedEntityRef" /> s that describe Entities that are related to the constructed Entity by various relationship types.
        /// </param>
        /// <returns>
        ///     A new DbNewInstanceExpression that represents the construction of the Entity, and includes the specified related Entity information in the see
        ///     <see
        ///         cref="DbNewInstanceExpression.RelatedEntityReferences" />
        ///     collection.
        /// </returns>
        internal static DbNewInstanceExpression CreateNewEntityWithRelationshipsExpression(
            EntityType entityType, IList<DbExpression> attributeValues, IList<DbRelatedEntityRef> relationships)
        {
            DbExpressionList validAttributes;
            ReadOnlyCollection<DbRelatedEntityRef> validRelatedRefs;
            var resultType = ArgumentValidation.ValidateNewEntityWithRelationships(
                entityType, attributeValues, relationships, out validAttributes, out validRelatedRefs);
            return new DbNewInstanceExpression(resultType, validAttributes, validRelatedRefs);
        }

        /// <summary>
        ///     Same as <see cref="Navigate(DbExpression, RelationshipEndMember, RelationshipEndMember)" /> only allows the property type of
        ///     <paramref
        ///         name="fromEnd" />
        ///     to be any type in the same type hierarchy as the result type of <paramref name="navigateFrom" />.
        ///     Only used by relationship span.
        /// </summary>
        /// <param name="navigateFrom"> </param>
        /// <param name="fromEnd"> </param>
        /// <param name="toEnd"> </param>
        /// <returns> </returns>
        internal static DbRelationshipNavigationExpression NavigateAllowingAllRelationshipsInSameTypeHierarchy(
            this DbExpression navigateFrom, RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
        {
            RelationshipType relType;
            var resultType = ArgumentValidation.ValidateNavigate(
                navigateFrom, fromEnd, toEnd, out relType, allowAllRelationshipsInSameTypeHierarchy: true);
            return new DbRelationshipNavigationExpression(resultType, relType, fromEnd, toEnd, navigateFrom);
        }

        internal static DbPropertyExpression CreatePropertyExpressionFromMember(DbExpression instance, EdmMember member)
        {
            DebugCheck.NotNull(instance);
            DebugCheck.NotNull(member);

            return PropertyFromMember(instance, member, "member");
        }

        #endregion
    }
}
