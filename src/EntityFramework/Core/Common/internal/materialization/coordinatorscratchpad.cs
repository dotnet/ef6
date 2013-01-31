// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     Used in the Translator to aggregate information about a (nested) reader
    ///     coordinator. After the translator visits the columnMaps, it will compile
    ///     the coordinator(s) which produces an immutable CoordinatorFactory that
    ///     can be shared amongst many query instances.
    /// </summary>
    internal class CoordinatorScratchpad
    {
        #region private state

        private readonly Type _elementType;
        private CoordinatorScratchpad _parent;
        private readonly List<CoordinatorScratchpad> _nestedCoordinatorScratchpads;

        /// <summary>
        ///     Map from original expressions to expressions with detailed error handling.
        /// </summary>
        private readonly Dictionary<Expression, Expression> _expressionWithErrorHandlingMap;

        /// <summary>
        ///     Expressions that should be precompiled (i.e. reduced to constants in
        ///     compiled delegates.
        /// </summary>
        private readonly HashSet<LambdaExpression> _inlineDelegates;

        #endregion

        #region constructor

        internal CoordinatorScratchpad(Type elementType)
        {
            _elementType = elementType;
            _nestedCoordinatorScratchpads = new List<CoordinatorScratchpad>();
            _expressionWithErrorHandlingMap = new Dictionary<Expression, Expression>();
            _inlineDelegates = new HashSet<LambdaExpression>();
        }

        #endregion

        #region "public" surface area

        /// <summary>
        ///     For nested collections, returns the parent coordinator.
        /// </summary>
        internal CoordinatorScratchpad Parent
        {
            get { return _parent; }
        }

        /// <summary>
        ///     Gets or sets an Expression setting key values (these keys are used
        ///     to determine when a collection has entered a new chapter) from the
        ///     underlying store data reader.
        /// </summary>
        internal Expression SetKeys { get; set; }

        /// <summary>
        ///     Gets or sets an Expression returning 'true' when the key values for
        ///     the current nested result (see SetKeys) are equal to the current key
        ///     values on the underlying data reader.
        /// </summary>
        internal Expression CheckKeys { get; set; }

        /// <summary>
        ///     Gets or sets an expression returning 'true' if the current row in
        ///     the underlying data reader contains an element of the collection.
        /// </summary>
        internal Expression HasData { get; set; }

        /// <summary>
        ///     Gets or sets an Expression yielding an element of the current collection
        ///     given values in the underlying data reader.
        /// </summary>
        internal Expression Element { get; set; }

        /// <summary>
        ///     Gets or sets an Expression initializing the collection storing results from this coordinator.
        /// </summary>
        internal Expression InitializeCollection { get; set; }

        /// <summary>
        ///     Indicates which Shaper.State slot is home for this collection's coordinator.
        ///     Used by Parent to pull out nested collection aggregators/streamers.
        /// </summary>
        internal int StateSlotNumber { get; set; }

        /// <summary>
        ///     Gets or sets the depth of the current coordinator. A root collection has depth 0.
        /// </summary>
        internal int Depth { get; set; }

        /// <summary>
        ///     List of all record types that we can return at this level in the query.
        /// </summary>
        private List<RecordStateScratchpad> _recordStateScratchpads;

        /// <summary>
        ///     Allows sub-expressions to register an 'interest' in exceptions thrown when reading elements
        ///     for this coordinator. When an exception is thrown, we rerun the delegate using the slower
        ///     but more error-friendly versions of expressions (e.g. reader.GetValue + type check instead
        ///     of reader.GetInt32())
        /// </summary>
        /// <param name="expression"> The lean and mean raw version of the expression </param>
        /// <param name="expressionWithErrorHandling"> The slower version of the same expression with better error handling </param>
        internal void AddExpressionWithErrorHandling(Expression expression, Expression expressionWithErrorHandling)
        {
            _expressionWithErrorHandlingMap[expression] = expressionWithErrorHandling;
        }

        /// <summary>
        ///     Registers a lambda expression for pre-compilation (i.e. reduction to a constant expression)
        ///     within materialization expression. Otherwise, the expression will be compiled every time
        ///     the enclosing delegate is invoked.
        /// </summary>
        /// <param name="expression"> Lambda expression to register. </param>
        internal void AddInlineDelegate(LambdaExpression expression)
        {
            _inlineDelegates.Add(expression);
        }

        /// <summary>
        ///     Registers a coordinator for a nested collection contained in elements of this collection.
        /// </summary>
        internal void AddNestedCoordinator(CoordinatorScratchpad nested)
        {
            Debug.Assert(nested.Depth == Depth + 1, "can only nest depth + 1");
            nested._parent = this;
            _nestedCoordinatorScratchpads.Add(nested);
        }

        /// <summary>
        ///     Use the information stored on the scratchpad to compile an immutable factory used
        ///     to construct the coordinators used at runtime when materializing results.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal CoordinatorFactory Compile()
        {
            RecordStateFactory[] recordStateFactories;
            if (null != _recordStateScratchpads)
            {
                recordStateFactories = new RecordStateFactory[_recordStateScratchpads.Count];
                for (var i = 0; i < recordStateFactories.Length; i++)
                {
                    recordStateFactories[i] = _recordStateScratchpads[i].Compile();
                }
            }
            else
            {
                recordStateFactories = new RecordStateFactory[0];
            }

            var nestedCoordinators = new CoordinatorFactory[_nestedCoordinatorScratchpads.Count];
            for (var i = 0; i < nestedCoordinators.Length; i++)
            {
                nestedCoordinators[i] = _nestedCoordinatorScratchpads[i].Compile();
            }

            // compile inline delegates
            var replacementVisitor = new ReplacementExpressionVisitor(null, _inlineDelegates);
            var element = replacementVisitor.Visit(Element);

            // substitute expressions that have error handlers into a new expression (used
            // when a more detailed exception message is needed)
            replacementVisitor = new ReplacementExpressionVisitor(_expressionWithErrorHandlingMap, _inlineDelegates);
            var elementWithErrorHandling = replacementVisitor.Visit(Element);

            var result =
                (CoordinatorFactory)Activator.CreateInstance(
                    typeof(CoordinatorFactory<>).MakeGenericType(_elementType), new object[]
                                                                                    {
                                                                                        Depth,
                                                                                        StateSlotNumber,
                                                                                        HasData,
                                                                                        SetKeys,
                                                                                        CheckKeys,
                                                                                        nestedCoordinators,
                                                                                        element,
                                                                                        elementWithErrorHandling,
                                                                                        InitializeCollection,
                                                                                        recordStateFactories
                                                                                    });
            return result;
        }

        /// <summary>
        ///     Allocates a new RecordStateScratchpad and adds it to the list of the ones we're
        ///     responsible for; will create the list if it hasn't alread been created.
        /// </summary>
        internal RecordStateScratchpad CreateRecordStateScratchpad()
        {
            var recordStateScratchpad = new RecordStateScratchpad();

            if (null == _recordStateScratchpads)
            {
                _recordStateScratchpads = new List<RecordStateScratchpad>();
            }
            _recordStateScratchpads.Add(recordStateScratchpad);
            return recordStateScratchpad;
        }

        #endregion

        #region Nested types

        /// <summary>
        ///     Visitor supporting (non-recursive) replacement of LINQ sub-expressions and
        ///     compilation of inline delegates.
        /// </summary>
        private class ReplacementExpressionVisitor : EntityExpressionVisitor
        {
            // Map from original expressions to replacement expressions.
            private readonly Dictionary<Expression, Expression> _replacementDictionary;
            private readonly HashSet<LambdaExpression> _inlineDelegates;

            internal ReplacementExpressionVisitor(
                Dictionary<Expression, Expression> replacementDictionary,
                HashSet<LambdaExpression> inlineDelegates)
            {
                _replacementDictionary = replacementDictionary;
                _inlineDelegates = inlineDelegates;
            }

            internal override Expression Visit(Expression expression)
            {
                if (null == expression)
                {
                    return expression;
                }

                Expression result;

                // check to see if a substitution has been provided for this expression
                Expression replacement;
                if (null != _replacementDictionary
                    && _replacementDictionary.TryGetValue(expression, out replacement))
                {
                    // once a substitution is found, we stop walking the sub-expression and
                    // return immediately (since recursive replacement is not needed or wanted)
                    result = replacement;
                }
                else
                {
                    // check if we need to precompile an inline delegate
                    var preCompile = false;
                    LambdaExpression lambda = null;

                    if (expression.NodeType == ExpressionType.Lambda
                        &&
                        null != _inlineDelegates)
                    {
                        lambda = (LambdaExpression)expression;
                        preCompile = _inlineDelegates.Contains(lambda);
                    }

                    if (preCompile)
                    {
                        // do replacement in the body of the lambda expression
                        var body = Visit(lambda.Body);

                        // compile to a delegate
                        result = Expression.Constant(CodeGenEmitter.Compile(body.Type, body));
                    }
                    else
                    {
                        result = base.Visit(expression);
                    }
                }

                return result;
            }
        }

        #endregion
    }
}
