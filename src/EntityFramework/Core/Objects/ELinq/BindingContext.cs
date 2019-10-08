// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using CqtExpression = System.Data.Entity.Core.Common.CommandTrees.DbExpression;
using LinqExpression = System.Linq.Expressions.Expression;

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Linq;

    // <summary>
    // Class containing binding information for an expression converter (associating CQT bindings
    // with LINQ lambda parameter or LINQ sub-expressions)
    // </summary>
    // <remarks>
    // Usage pattern:
    // <code>BindingContext context = ...;
    // 
    //     // translate a "Where" lambda expression input.Where(i => i.X > 2);
    //     LambdaExpression whereLambda = ...;
    //     CqtExpression inputCqt = Translate(whereLambda.Arguments[1]);
    //     CqtExpression inputBinding = CreateExpressionBinding(inputCqt).Var;
    // 
    //     // push the scope defined by the parameter 
    //     context.PushBindingScope(new KeyValuePair{ParameterExpression, CqtExpression}(whereLambda.Parameters[0], inputBinding));
    // 
    //     // translate the expression in this context
    //     CqtExpression result = Translate(whereLambda.Expression);
    // 
    //     // pop the scope
    //     context.PopBindingScope();</code>
    // </remarks>
    internal sealed class BindingContext
    {
        private readonly Stack<Binding> _scopes;

        // <summary>
        // Initialize a new binding context
        // </summary>
        internal BindingContext()
        {
            _scopes = new Stack<Binding>();
        }

        // <summary>
        // Set up a new binding scope where parameter expressions map to their paired CQT expressions.
        // </summary>
        // <param name="binding"> DbExpression/LinqExpression binding </param>
        internal void PushBindingScope(Binding binding)
        {
            _scopes.Push(binding);
        }

        // <summary>
        // Removes a scope when leaving a particular sub-expression.
        // </summary>
        internal void PopBindingScope()
        {
            _scopes.Pop();
        }

        internal bool TryGetBoundExpression(LinqExpression linqExpression, out CqtExpression cqtExpression)
        {
            cqtExpression = _scopes
                .Where(binding => binding.LinqExpression == linqExpression)
                .Select(binding => binding.CqtExpression)
                .FirstOrDefault();
            return cqtExpression != null;
        }
    }
}
