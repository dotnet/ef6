// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    ///     Represents the application of a Lambda function.
    /// </summary>
    public sealed class DbLambdaExpression : DbExpression
    {
        private readonly DbLambda _lambda;
        private readonly DbExpressionList _arguments;

        internal DbLambdaExpression(TypeUsage resultType, DbLambda lambda, DbExpressionList args)
            : base(DbExpressionKind.Lambda, resultType)
        {
            Debug.Assert(lambda != null, "DbLambdaExpression lambda cannot be null");
            Debug.Assert(args != null, "DbLambdaExpression arguments cannot be null");
            Debug.Assert(
                ReferenceEquals(resultType, lambda.Body.ResultType), "DbLambdaExpression result type must be Lambda body result type");
            Debug.Assert(lambda.Variables.Count == args.Count, "DbLambdaExpression argument count does not match Lambda parameter count");

            _lambda = lambda;
            _arguments = args;
        }

        /// <summary>
        ///     Gets the <see cref="DbLambda" /> representing the Lambda function applied by this expression.
        /// </summary>
        public DbLambda Lambda
        {
            get { return _lambda; }
        }

        /// <summary>
        ///     Gets a <see cref="DbExpression" /> list that provides the arguments to which the Lambda function should be applied.
        /// </summary>
        public IList<DbExpression> Arguments
        {
            get { return _arguments; }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType"> The type of the result produced by <paramref name="visitor" /> </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
        /// <returns> An instance of <typeparamref name="TResultType" /> . </returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            if (visitor != null)
            {
                return visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }
    }
}
