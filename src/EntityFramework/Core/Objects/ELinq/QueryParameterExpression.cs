// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    // <summary>
    // A LINQ expression corresponding to a query parameter.
    // </summary>
    internal sealed class QueryParameterExpression : Expression
    {
        private readonly DbParameterReferenceExpression _parameterReference;
        private readonly Type _type;
        private readonly Expression _funcletizedExpression;
        private readonly IEnumerable<ParameterExpression> _compiledQueryParameters;
        private Delegate _cachedDelegate;

        internal QueryParameterExpression(
            DbParameterReferenceExpression parameterReference,
            Expression funcletizedExpression,
            IEnumerable<ParameterExpression> compiledQueryParameters)
        {
            DebugCheck.NotNull(parameterReference);
            DebugCheck.NotNull(funcletizedExpression);

            _compiledQueryParameters = compiledQueryParameters ?? Enumerable.Empty<ParameterExpression>();
            _parameterReference = parameterReference;
            _type = funcletizedExpression.Type;
            _funcletizedExpression = funcletizedExpression;
            _cachedDelegate = null;
        }

        // <summary>
        // Gets the current value of the parameter given (optional) compiled query arguments.
        // </summary>
        internal object EvaluateParameter(object[] arguments)
        {
            if (_cachedDelegate == null)
            {
                if (_funcletizedExpression.NodeType
                    == ExpressionType.Constant)
                {
                    return ((ConstantExpression)_funcletizedExpression).Value;
                }
                ConstantExpression ce;
                if (TryEvaluatePath(_funcletizedExpression, out ce))
                {
                    return ce.Value;
                }
            }

            try
            {
                if (_cachedDelegate == null)
                {
                    // Get the Func<> type for the property evaluator
                    var delegateType = TypeSystem.GetDelegateType(_compiledQueryParameters.Select(p => p.Type), _type);

                    // Now compile delegate for the funcletized expression
                    _cachedDelegate = Lambda(delegateType, _funcletizedExpression, _compiledQueryParameters).Compile();
                }
                return _cachedDelegate.DynamicInvoke(arguments);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        // <summary>
        // Create QueryParameterExpression based on this one, but with the funcletized expression
        // wrapped by the given method
        // </summary>
        internal QueryParameterExpression EscapeParameterForLike(Expression<Func<string, Tuple<string, bool>>> method)
        {
            Expression wrappedExpression = Expression.Property(Invoke(Constant(method), _funcletizedExpression), "Item1");
            return new QueryParameterExpression(_parameterReference, wrappedExpression, _compiledQueryParameters);
        }

        // <summary>
        // Gets the parameter reference for the parameter.
        // </summary>
        internal DbParameterReferenceExpression ParameterReference
        {
            get { return _parameterReference; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public override ExpressionType NodeType
        {
            get { return EntityExpressionVisitor.CustomExpression; }
        }

        private static bool TryEvaluatePath(Expression expression, out ConstantExpression constantExpression)
        {
            var me = expression as MemberExpression;
            constantExpression = null;
            if (me != null)
            {
                var stack = new Stack<MemberExpression>();
                stack.Push(me);
                while ((me = me.Expression as MemberExpression) != null)
                {
                    stack.Push(me);
                }
                me = stack.Pop();
                var ce = me.Expression as ConstantExpression;
                if (ce != null)
                {
                    object memberVal;
                    if (!TryGetFieldOrPropertyValue(me, ((ConstantExpression)me.Expression).Value, out memberVal))
                    {
                        return false;
                    }
                    if (stack.Count > 0)
                    {
                        foreach (var rec in stack)
                        {
                            if (!TryGetFieldOrPropertyValue(rec, memberVal, out memberVal))
                            {
                                return false;
                            }
                        }
                    }
                    constantExpression = Constant(memberVal, expression.Type);
                    return true;
                }
            }
            return false;
        }

        private static bool TryGetFieldOrPropertyValue(MemberExpression me, object instance, out object memberValue)
        {
            var result = false;
            memberValue = null;

            try
            {
                if (me.Member.MemberType
                    == MemberTypes.Field)
                {
                    memberValue = ((FieldInfo)me.Member).GetValue(instance);
                    result = true;
                }
                else if (me.Member.MemberType
                         == MemberTypes.Property)
                {
                    memberValue = ((PropertyInfo)me.Member).GetValue(instance, null);
                    result = true;
                }
                return result;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
