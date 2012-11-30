// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using ReadOnlyVariables =
    System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Entity.Core.Common.CommandTrees.DbVariableReferenceExpression>;

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    ///     Represents a Lambda function that can be invoked to produce a <see cref="DbLambdaExpression" />.
    /// </summary>
    public sealed class DbLambda
    {
        private readonly ReadOnlyVariables _variables;
        private readonly DbExpression _body;

        internal DbLambda(ReadOnlyVariables variables, DbExpression bodyExp)
        {
            DebugCheck.NotNull(variables);
            DebugCheck.NotNull(bodyExp);

            _variables = variables;
            _body = bodyExp;
        }

        /// <summary>
        ///     Gets the <see cref="DbExpression" /> that provides the definition of the Lambda function
        /// </summary>
        public DbExpression Body
        {
            get { return _body; }
        }

        /// <summary>
        ///     Gets the <see cref="DbVariableReferenceExpression" />s that represent the parameters to the Lambda function and are in scope within
        ///     <see
        ///         cref="Body" />
        ///     .
        /// </summary>
        public IList<DbVariableReferenceExpression> Variables
        {
            get { return _variables; }
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
        public static DbLambda Create(DbExpression body, IEnumerable<DbVariableReferenceExpression> variables)
        {
            return DbExpressionBuilder.Lambda(body, variables);
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
        public static DbLambda Create(DbExpression body, params DbVariableReferenceExpression[] variables)
        {
            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with a single argument of the specified type, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and single formal parameter. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(TypeUsage argument1Type, Func<DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(lambdaFunction.Method, argument1Type);
            var body = lambdaFunction(variables[0]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, Func<DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type);
            var body = lambdaFunction(variables[0], variables[1]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null
        ///     or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type,
            Func<DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type);
            var body = lambdaFunction(variables[0], variables[1], variables[2]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type,
            Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type);
            var body = lambdaFunction(variables[0], variables[1], variables[2], variables[3]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type);
            var body = lambdaFunction(variables[0], variables[1], variables[2], variables[3], variables[4]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type,
            Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type);
            var body = lambdaFunction(variables[0], variables[1], variables[2], variables[3], variables[4], variables[5]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type,
            Func<DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression>
                lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type);
            var body = lambdaFunction(variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="argument10Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the tenth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     <paramref name="argument10Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(argument10Type, "argument10Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type, argument10Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8],
                variables[9]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="argument10Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the tenth argument to the Lambda function
        /// </param>
        /// <param name="argument11Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eleventh argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     <paramref name="argument10Type" />
        ///     is null,
        ///     <paramref name="argument11Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type,
            TypeUsage argument11Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(argument10Type, "argument10Type");
            Check.NotNull(argument11Type, "argument11Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type, argument10Type, argument11Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8],
                variables[9], variables[10]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="argument10Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the tenth argument to the Lambda function
        /// </param>
        /// <param name="argument11Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eleventh argument to the Lambda function
        /// </param>
        /// <param name="argument12Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the twelfth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     <paramref name="argument10Type" />
        ///     is null,
        ///     <paramref name="argument11Type" />
        ///     is null,
        ///     <paramref name="argument12Type" />
        ///     is null,
        ///     or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type,
            TypeUsage argument11Type, TypeUsage argument12Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(argument10Type, "argument10Type");
            Check.NotNull(argument11Type, "argument11Type");
            Check.NotNull(argument12Type, "argument12Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8],
                variables[9], variables[10], variables[11]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="argument10Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the tenth argument to the Lambda function
        /// </param>
        /// <param name="argument11Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eleventh argument to the Lambda function
        /// </param>
        /// <param name="argument12Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the twelfth argument to the Lambda function
        /// </param>
        /// <param name="argument13Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the thirteenth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     <paramref name="argument10Type" />
        ///     is null,
        ///     <paramref name="argument11Type" />
        ///     is null,
        ///     <paramref name="argument12Type" />
        ///     is null,
        ///     <paramref name="argument13Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type,
            TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(argument10Type, "argument10Type");
            Check.NotNull(argument11Type, "argument11Type");
            Check.NotNull(argument12Type, "argument12Type");
            Check.NotNull(argument13Type, "argument13Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8],
                variables[9], variables[10], variables[11], variables[12]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="argument10Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the tenth argument to the Lambda function
        /// </param>
        /// <param name="argument11Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eleventh argument to the Lambda function
        /// </param>
        /// <param name="argument12Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the twelfth argument to the Lambda function
        /// </param>
        /// <param name="argument13Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the thirteenth argument to the Lambda function
        /// </param>
        /// <param name="argument14Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourteenth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     <paramref name="argument10Type" />
        ///     is null,
        ///     <paramref name="argument11Type" />
        ///     is null,
        ///     <paramref name="argument12Type" />
        ///     is null,
        ///     <paramref name="argument13Type" />
        ///     is null,
        ///     <paramref name="argument14Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type,
            TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type, TypeUsage argument14Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(argument10Type, "argument10Type");
            Check.NotNull(argument11Type, "argument11Type");
            Check.NotNull(argument12Type, "argument12Type");
            Check.NotNull(argument13Type, "argument13Type");
            Check.NotNull(argument14Type, "argument14Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type, argument14Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8],
                variables[9], variables[10], variables[11], variables[12], variables[13]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="argument10Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the tenth argument to the Lambda function
        /// </param>
        /// <param name="argument11Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eleventh argument to the Lambda function
        /// </param>
        /// <param name="argument12Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the twelfth argument to the Lambda function
        /// </param>
        /// <param name="argument13Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the thirteenth argument to the Lambda function
        /// </param>
        /// <param name="argument14Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourteenth argument to the Lambda function
        /// </param>
        /// <param name="argument15Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifteenth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     <paramref name="argument10Type" />
        ///     is null,
        ///     <paramref name="argument11Type" />
        ///     is null,
        ///     <paramref name="argument12Type" />
        ///     is null,
        ///     <paramref name="argument13Type" />
        ///     is null,
        ///     <paramref name="argument14Type" />
        ///     is null,
        ///     <paramref name="argument15Type" />
        ///     is null,
        ///     or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type,
            TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type, TypeUsage argument14Type, TypeUsage argument15Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression>
                lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(argument10Type, "argument10Type");
            Check.NotNull(argument11Type, "argument11Type");
            Check.NotNull(argument12Type, "argument12Type");
            Check.NotNull(argument13Type, "argument13Type");
            Check.NotNull(argument14Type, "argument14Type");
            Check.NotNull(argument15Type, "argument15Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type, argument14Type,
                argument15Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8],
                variables[9], variables[10], variables[11], variables[12], variables[13], variables[14]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        /// <summary>
        ///     Creates a new <see cref="DbLambda" /> with arguments of the specified types, as defined by the specified function.
        /// </summary>
        /// <param name="argument1Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the first argument to the Lambda function
        /// </param>
        /// <param name="argument2Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the second argument to the Lambda function
        /// </param>
        /// <param name="argument3Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the third argument to the Lambda function
        /// </param>
        /// <param name="argument4Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourth argument to the Lambda function
        /// </param>
        /// <param name="argument5Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifth argument to the Lambda function
        /// </param>
        /// <param name="argument6Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixth argument to the Lambda function
        /// </param>
        /// <param name="argument7Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the seventh argument to the Lambda function
        /// </param>
        /// <param name="argument8Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eighth argument to the Lambda function
        /// </param>
        /// <param name="argument9Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the ninth argument to the Lambda function
        /// </param>
        /// <param name="argument10Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the tenth argument to the Lambda function
        /// </param>
        /// <param name="argument11Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the eleventh argument to the Lambda function
        /// </param>
        /// <param name="argument12Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the twelfth argument to the Lambda function
        /// </param>
        /// <param name="argument13Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the thirteenth argument to the Lambda function
        /// </param>
        /// <param name="argument14Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fourteenth argument to the Lambda function
        /// </param>
        /// <param name="argument15Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the fifteenth argument to the Lambda function
        /// </param>
        /// <param name="argument16Type">
        ///     A <see cref="TypeUsage" /> that defines the EDM type of the sixteenth argument to the Lambda function
        /// </param>
        /// <param name="lambdaFunction">
        ///     A function that defines the logic of the Lambda function as a <see cref="DbExpression" />
        /// </param>
        /// <returns> A new DbLambda that describes an inline Lambda function with the specified body and formal parameters. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="argument1Type" />
        ///     is null,
        ///     <paramref name="argument2Type" />
        ///     is null,
        ///     <paramref name="argument3Type" />
        ///     is null,
        ///     <paramref name="argument4Type" />
        ///     is null,
        ///     <paramref name="argument5Type" />
        ///     is null,
        ///     <paramref name="argument6Type" />
        ///     is null,
        ///     <paramref name="argument7Type" />
        ///     is null,
        ///     <paramref name="argument8Type" />
        ///     is null,
        ///     <paramref name="argument9Type" />
        ///     is null,
        ///     <paramref name="argument10Type" />
        ///     is null,
        ///     <paramref name="argument11Type" />
        ///     is null,
        ///     <paramref name="argument12Type" />
        ///     is null,
        ///     <paramref name="argument13Type" />
        ///     is null,
        ///     <paramref name="argument14Type" />
        ///     is null,
        ///     <paramref name="argument15Type" />
        ///     is null,
        ///     <paramref name="argument16Type" />
        ///     is null, or
        ///     <paramref name="lambdaFunction" />
        ///     is null or produces a result of null.
        /// </exception>
        public static DbLambda Create(
            TypeUsage argument1Type, TypeUsage argument2Type, TypeUsage argument3Type, TypeUsage argument4Type, TypeUsage argument5Type,
            TypeUsage argument6Type, TypeUsage argument7Type, TypeUsage argument8Type, TypeUsage argument9Type, TypeUsage argument10Type,
            TypeUsage argument11Type, TypeUsage argument12Type, TypeUsage argument13Type, TypeUsage argument14Type, TypeUsage argument15Type,
            TypeUsage argument16Type,
            Func
                <DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression, DbExpression,
                    DbExpression> lambdaFunction)
        {
            Check.NotNull(argument1Type, "argument1Type");
            Check.NotNull(argument2Type, "argument2Type");
            Check.NotNull(argument3Type, "argument3Type");
            Check.NotNull(argument4Type, "argument4Type");
            Check.NotNull(argument5Type, "argument5Type");
            Check.NotNull(argument6Type, "argument6Type");
            Check.NotNull(argument7Type, "argument7Type");
            Check.NotNull(argument8Type, "argument8Type");
            Check.NotNull(argument9Type, "argument9Type");
            Check.NotNull(argument10Type, "argument10Type");
            Check.NotNull(argument11Type, "argument11Type");
            Check.NotNull(argument12Type, "argument12Type");
            Check.NotNull(argument13Type, "argument13Type");
            Check.NotNull(argument14Type, "argument14Type");
            Check.NotNull(argument15Type, "argument15Type");
            Check.NotNull(argument16Type, "argument16Type");
            Check.NotNull(lambdaFunction, "lambdaFunction");

            var variables = CreateVariables(
                lambdaFunction.Method, argument1Type, argument2Type, argument3Type, argument4Type, argument5Type, argument6Type,
                argument7Type, argument8Type, argument9Type, argument10Type, argument11Type, argument12Type, argument13Type, argument14Type,
                argument15Type, argument16Type);
            var body = lambdaFunction(
                variables[0], variables[1], variables[2], variables[3], variables[4], variables[5], variables[6], variables[7], variables[8],
                variables[9], variables[10], variables[11], variables[12], variables[13], variables[14], variables[15]);

            return DbExpressionBuilder.Lambda(body, variables);
        }

        private static DbVariableReferenceExpression[] CreateVariables(MethodInfo lambdaMethod, params TypeUsage[] argumentTypes)
        {
            DebugCheck.NotNull(lambdaMethod);
            var paramNames = DbExpressionBuilder.ExtractAliases(lambdaMethod);

            Debug.Assert(paramNames.Length == argumentTypes.Length, "Lambda function method parameter count does not match argument count");
            var result = new DbVariableReferenceExpression[argumentTypes.Length];
            for (var idx = 0; idx < paramNames.Length; idx++)
            {
                Debug.Assert(argumentTypes[idx] != null, "DbLambda.Create allowed null type argument");
                result[idx] = argumentTypes[idx].Variable(paramNames[idx]);
            }
            return result;
        }
    }
}
