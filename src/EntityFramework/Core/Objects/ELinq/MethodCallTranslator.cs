// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using CqtExpression = System.Data.Entity.Core.Common.CommandTrees.DbExpression;
    using LinqExpression = System.Linq.Expressions.Expression;

    internal sealed partial class ExpressionConverter
    {
        // <summary>
        // Translates System.Linq.Expression.MethodCallExpression to System.Data.Entity.Core.Common.CommandTrees.DbExpression
        // </summary>
        internal sealed partial class MethodCallTranslator : TypedTranslator<MethodCallExpression>
        {
            internal MethodCallTranslator()
                : base(ExpressionType.Call)
            {
            }

            protected override CqtExpression TypedTranslate(ExpressionConverter parent, MethodCallExpression linq)
            {
                // check if this is a known sequence method
                SequenceMethod sequenceMethod;
                SequenceMethodTranslator sequenceTranslator;
                if (ReflectionUtil.TryIdentifySequenceMethod(linq.Method, out sequenceMethod)
                    &&
                    _sequenceTranslators.TryGetValue(sequenceMethod, out sequenceTranslator))
                {
                    return sequenceTranslator.Translate(parent, linq, sequenceMethod);
                }
                // check if this is a known method
                CallTranslator callTranslator;
                if (TryGetCallTranslator(linq.Method, out callTranslator))
                {
                    return callTranslator.Translate(parent, linq);
                }

                // check if this is an ObjectQuery<> builder method
                if (ObjectQueryCallTranslator.IsCandidateMethod(linq.Method))
                {
                    ObjectQueryCallTranslator builderTranslator;
                    if (_objectQueryTranslators.TryGetValue(linq.Method.Name, out builderTranslator))
                    {
                        return builderTranslator.Translate(parent, linq);
                    }
                }

                // check if this method has the FunctionAttribute (known proxy)
                var functionAttribute = linq.Method.GetCustomAttributes<DbFunctionAttribute>(inherit: false).FirstOrDefault();
                if (null != functionAttribute)
                {
                    return _functionCallTranslator.TranslateFunctionCall(parent, linq, functionAttribute);
                }

                switch (linq.Method.Name)
                {
                    case "Contains":
                        {
                            if (linq.Method.GetParameters().Count() == 1
                                && linq.Method.ReturnType.Equals(typeof(bool)))
                            {
                                Type[] genericArguments;
                                if (linq.Method.IsImplementationOfGenericInterfaceMethod(typeof(ICollection<>), out genericArguments))
                                {
                                    return ContainsTranslator.TranslateContains(parent, linq.Object, linq.Arguments[0]);
                                }
                            }
                            break;
                        }
                }

                // fall back on the default translator
                return _defaultTranslator.Translate(parent, linq);
            }

            #region Static members and initializers

            private const string s_stringsTypeFullName = "Microsoft.VisualBasic.Strings";

            // initialize fall-back translator
            private static readonly CallTranslator _defaultTranslator = new DefaultTranslator();
            private static readonly FunctionCallTranslator _functionCallTranslator = new FunctionCallTranslator();
            private static readonly Dictionary<MethodInfo, CallTranslator> _methodTranslators = InitializeMethodTranslators();

            private static readonly Dictionary<SequenceMethod, SequenceMethodTranslator> _sequenceTranslators =
                InitializeSequenceMethodTranslators();

            private static readonly Dictionary<string, ObjectQueryCallTranslator> _objectQueryTranslators =
                InitializeObjectQueryTranslators();

            private static bool s_vbMethodsInitialized;
            private static readonly object _vbInitializerLock = new object();

            private static Dictionary<MethodInfo, CallTranslator> InitializeMethodTranslators()
            {
                // initialize translators for specific methods (e.g., Int32.op_Equality)
                var methodTranslators = new Dictionary<MethodInfo, CallTranslator>();
                foreach (var translator in GetCallTranslators())
                {
                    foreach (var method in translator.Methods)
                    {
                        methodTranslators.Add(method, translator);
                    }
                }

                return methodTranslators;
            }

            private static Dictionary<SequenceMethod, SequenceMethodTranslator> InitializeSequenceMethodTranslators()
            {
                // initialize translators for sequence methods (e.g., Sequence.Select)
                var sequenceTranslators = new Dictionary<SequenceMethod, SequenceMethodTranslator>();
                foreach (var translator in GetSequenceMethodTranslators())
                {
                    foreach (var method in translator.Methods)
                    {
                        sequenceTranslators.Add(method, translator);
                    }
                }

                return sequenceTranslators;
            }

            private static Dictionary<string, ObjectQueryCallTranslator> InitializeObjectQueryTranslators()
            {
                // initialize translators for object query methods (e.g. ObjectQuery<T>.OfType<S>(), ObjectQuery<T>.Include(string) )
                var objectQueryCallTranslators = new Dictionary<string, ObjectQueryCallTranslator>(StringComparer.Ordinal);
                foreach (var translator in GetObjectQueryCallTranslators())
                {
                    objectQueryCallTranslators[translator.MethodName] = translator;
                }

                return objectQueryCallTranslators;
            }

            // <summary>
            // Tries to get a translator for the given method info.
            // If the given method info corresponds to a Visual Basic property,
            // it also initializes the Visual Basic translators if they have not been initialized
            // </summary>
            private static bool TryGetCallTranslator(MethodInfo methodInfo, out CallTranslator callTranslator)
            {
                if (_methodTranslators.TryGetValue(methodInfo, out callTranslator))
                {
                    return true;
                }
                // check if this is the visual basic assembly
                if (s_visualBasicAssemblyFullName == methodInfo.DeclaringType.Assembly().FullName)
                {
                    lock (_vbInitializerLock)
                    {
                        if (!s_vbMethodsInitialized)
                        {
                            InitializeVBMethods(methodInfo.DeclaringType.Assembly());
                            s_vbMethodsInitialized = true;
                        }
                        // try again
                        return _methodTranslators.TryGetValue(methodInfo, out callTranslator);
                    }
                }

                callTranslator = null;
                return false;
            }

            private static void InitializeVBMethods(Assembly vbAssembly)
            {
                Debug.Assert(!s_vbMethodsInitialized);
                foreach (var translator in GetVisualBasicCallTranslators(vbAssembly))
                {
                    foreach (var method in translator.Methods)
                    {
                        _methodTranslators.Add(method, translator);
                    }
                }
            }

            private static IEnumerable<CallTranslator> GetVisualBasicCallTranslators(Assembly vbAssembly)
            {
                yield return new VBCanonicalFunctionDefaultTranslator(vbAssembly);
                yield return new VBCanonicalFunctionRenameTranslator(vbAssembly);
                yield return new VBDatePartTranslator(vbAssembly);
            }

            private static IEnumerable<CallTranslator> GetCallTranslators()
            {
                return new CallTranslator[]
                    {
                        new CanonicalFunctionDefaultTranslator(),
                        new AsUnicodeFunctionTranslator(),
                        new AsNonUnicodeFunctionTranslator(),
                        new MathTruncateTranslator(),
                        new MathPowerTranslator(),
                        new GuidNewGuidTranslator(),
                        new LikeFunctionTranslator(),
                        new StringContainsTranslator(),
                        new StartsWithTranslator(),
                        new EndsWithTranslator(),
                        new IndexOfTranslator(),
                        new SubstringTranslator(),
                        new RemoveTranslator(),
                        new InsertTranslator(),
                        new IsNullOrEmptyTranslator(),
                        new StringConcatTranslator(),
                        new TrimTranslator(),
                        new TrimStartTranslator(),
                        new TrimEndTranslator(),
                        new SpatialMethodCallTranslator(),
                        new HierarchyIdMethodCallTranslator(),
                        new HasFlagTranslator(),
                        new ToStringTranslator(),
                    };
            }

            private static IEnumerable<SequenceMethodTranslator> GetSequenceMethodTranslators()
            {
                yield return new ConcatTranslator();
                yield return new UnionTranslator();
                yield return new IntersectTranslator();
                yield return new ExceptTranslator();
                yield return new DistinctTranslator();
                yield return new WhereTranslator();
                yield return new SelectTranslator();
                yield return new OrderByTranslator();
                yield return new OrderByDescendingTranslator();
                yield return new ThenByTranslator();
                yield return new ThenByDescendingTranslator();
                yield return new SelectManyTranslator();
                yield return new AnyTranslator();
                yield return new AnyPredicateTranslator();
                yield return new AllTranslator();
                yield return new JoinTranslator();
                yield return new GroupByTranslator();
                yield return new MaxTranslator();
                yield return new MinTranslator();
                yield return new AverageTranslator();
                yield return new SumTranslator();
                yield return new CountTranslator();
                yield return new LongCountTranslator();
                yield return new CastMethodTranslator();
                yield return new GroupJoinTranslator();
                yield return new OfTypeTranslator();
                yield return new PassthroughTranslator();
                yield return new DefaultIfEmptyTranslator();
                yield return new FirstTranslator();
                yield return new FirstPredicateTranslator();
                yield return new FirstOrDefaultTranslator();
                yield return new FirstOrDefaultPredicateTranslator();
                yield return new TakeTranslator();
                yield return new SkipTranslator();
                yield return new SingleTranslator();
                yield return new SinglePredicateTranslator();
                yield return new SingleOrDefaultTranslator();
                yield return new SingleOrDefaultPredicateTranslator();
                yield return new ContainsTranslator();
            }

            private static IEnumerable<ObjectQueryCallTranslator> GetObjectQueryCallTranslators()
            {
                yield return new ObjectQueryBuilderDistinctTranslator();
                yield return new ObjectQueryBuilderExceptTranslator();
                yield return new ObjectQueryBuilderFirstTranslator();
                yield return new ObjectQueryBuilderToListTranslator();
                yield return new ObjectQueryIncludeTranslator();
                yield return new ObjectQueryBuilderIntersectTranslator();
                yield return new ObjectQueryBuilderOfTypeTranslator();
                yield return new ObjectQueryBuilderUnionTranslator();
                yield return new ObjectQueryMergeAsTranslator();
                yield return new ObjectQueryIncludeSpanTranslator();
            }

            private static bool IsTrivialRename(
                LambdaExpression selectorLambda,
                ExpressionConverter converter,
                out string leftName,
                out string rightName,
                out InitializerMetadata initializerMetadata)
            {
                leftName = null;
                rightName = null;
                initializerMetadata = null;

                if (selectorLambda.Parameters.Count != 2
                    ||
                    selectorLambda.Body.NodeType != ExpressionType.New)
                {
                    return false;
                }
                var newExpression = (NewExpression)selectorLambda.Body;

                if (newExpression.Arguments.Count != 2)
                {
                    return false;
                }

                if (newExpression.Arguments[0] != selectorLambda.Parameters[0]
                    ||
                    newExpression.Arguments[1] != selectorLambda.Parameters[1])
                {
                    return false;
                }

                leftName = newExpression.Members[0].Name;
                rightName = newExpression.Members[1].Name;

                // Construct a new initializer type in metadata for the renaming projection (provides the
                // necessary context for the object materializer)
                initializerMetadata = InitializerMetadata.CreateProjectionInitializer(converter.EdmItemCollection, newExpression);
                converter.ValidateInitializerMetadata(initializerMetadata);

                return true;
            }

            #endregion

            #region Method translators

            internal abstract class CallTranslator
            {
                private readonly IEnumerable<MethodInfo> _methods;

                protected CallTranslator(params MethodInfo[] methods)
                {
                    _methods = methods;
                }

                protected CallTranslator(IEnumerable<MethodInfo> methods)
                {
                    _methods = methods;
                }

                internal IEnumerable<MethodInfo> Methods
                {
                    get { return _methods; }
                }

                internal abstract CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call);

                public override string ToString()
                {
                    return GetType().Name;
                }
            }

            private abstract class ObjectQueryCallTranslator : CallTranslator
            {
                internal static bool IsCandidateMethod(MethodInfo method)
                {
                    var declaringType = method.DeclaringType;
                    return ((method.IsPublic || (method.IsAssembly && (method.Name == "MergeAs" || method.Name == "IncludeSpan"))) &&
                            null != declaringType &&
                            declaringType.IsGenericType() &&
                            typeof(ObjectQuery<>) == declaringType.GetGenericTypeDefinition());
                }

                internal static LinqExpression RemoveConvertToObjectQuery(LinqExpression queryExpression)
                {
                    // Remove the Convert(ObjectQuery<T>) that was placed around the LINQ expression that defines an ObjectQuery to allow it to be used as the argument in a call to MergeAs or IncludeSpan
                    if (queryExpression.NodeType
                        == ExpressionType.Convert)
                    {
                        var convertExpression = (UnaryExpression)queryExpression;
                        var argumentType = convertExpression.Operand.Type;
                        if (argumentType.IsGenericType()
                            &&
                            (typeof(IQueryable<>) == argumentType.GetGenericTypeDefinition()
                             || typeof(IOrderedQueryable<>) == argumentType.GetGenericTypeDefinition()))
                        {
                            Debug.Assert(
                                convertExpression.Type.IsGenericType()
                                && typeof(ObjectQuery<>) == convertExpression.Type.GetGenericTypeDefinition(),
                                "MethodCall with internal MergeAs/IncludeSpan method was not constructed by LINQ to Entities?");
                            queryExpression = convertExpression.Operand;
                        }
                    }

                    return queryExpression;
                }

                private readonly string _methodName;

                protected ObjectQueryCallTranslator(string methodName)
                {
                    _methodName = methodName;
                }

                internal string MethodName
                {
                    get { return _methodName; }
                }
            }

            private abstract class ObjectQueryBuilderCallTranslator : ObjectQueryCallTranslator
            {
                private readonly SequenceMethodTranslator _translator;

                protected ObjectQueryBuilderCallTranslator(string methodName, SequenceMethod sequenceEquivalent)
                    : base(methodName)
                {
                    var translatorFound = _sequenceTranslators.TryGetValue(sequenceEquivalent, out _translator);
                    Debug.Assert(translatorFound, "Translator not found for " + sequenceEquivalent.ToString());
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return _translator.Translate(parent, call);
                }
            }

            private sealed class ObjectQueryBuilderUnionTranslator : ObjectQueryBuilderCallTranslator
            {
                internal ObjectQueryBuilderUnionTranslator()
                    : base("Union", SequenceMethod.Union)
                {
                }
            }

            private sealed class ObjectQueryBuilderIntersectTranslator : ObjectQueryBuilderCallTranslator
            {
                internal ObjectQueryBuilderIntersectTranslator()
                    : base("Intersect", SequenceMethod.Intersect)
                {
                }
            }

            private sealed class ObjectQueryBuilderExceptTranslator : ObjectQueryBuilderCallTranslator
            {
                internal ObjectQueryBuilderExceptTranslator()
                    : base("Except", SequenceMethod.Except)
                {
                }
            }

            private sealed class ObjectQueryBuilderDistinctTranslator : ObjectQueryBuilderCallTranslator
            {
                internal ObjectQueryBuilderDistinctTranslator()
                    : base("Distinct", SequenceMethod.Distinct)
                {
                }
            }

            private sealed class ObjectQueryBuilderOfTypeTranslator : ObjectQueryBuilderCallTranslator
            {
                internal ObjectQueryBuilderOfTypeTranslator()
                    : base("OfType", SequenceMethod.OfType)
                {
                }
            }

            private sealed class ObjectQueryBuilderFirstTranslator : ObjectQueryBuilderCallTranslator
            {
                internal ObjectQueryBuilderFirstTranslator()
                    : base("First", SequenceMethod.First)
                {
                }
            }

            private sealed class ObjectQueryBuilderToListTranslator : ObjectQueryBuilderCallTranslator
	        {
	            internal ObjectQueryBuilderToListTranslator()
	                : base("ToList", SequenceMethod.ToList)
	            {
	            }
	        }

            private sealed class ObjectQueryIncludeTranslator : ObjectQueryCallTranslator
            {
                internal ObjectQueryIncludeTranslator()
                    : base("Include")
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    DebugCheck.NotNull(call);
                    DebugCheck.NotNull(call.Object);
                    DebugCheck.NotNull(call.Arguments);

                    Debug.Assert(
                        call.Arguments.Count == 1 && call.Arguments[0] != null
                        && call.Arguments[0].Type.Equals(typeof(string)), "Invalid Include arguments?");

                    var queryExpression = parent.TranslateExpression(call.Object);
                    Span span;
                    if (!parent.TryGetSpan(queryExpression, out span))
                    {
                        span = null;
                    }
                    var arg = parent.TranslateExpression(call.Arguments[0]);
                    string includePath = null;
                    if (arg.ExpressionKind
                        == DbExpressionKind.Constant)
                    {
                        includePath = (string)((DbConstantExpression)arg).Value;
                    }
                    else
                    {
                        // The 'Include' method implementation on ELinqQueryState creates 
                        // a method call expression with a string constant argument taking 
                        // the value of the string argument passed to ObjectQuery.Include,
                        // and so this is the only supported pattern here.
                        throw new NotSupportedException(Strings.ELinq_UnsupportedInclude);
                    }
                    if (parent.CanIncludeSpanInfo())
                    {
                        span = Span.IncludeIn(span, includePath);
                    }
                    return parent.AddSpanMapping(queryExpression, span);
                }
            }

            private sealed class ObjectQueryMergeAsTranslator : ObjectQueryCallTranslator
            {
                internal ObjectQueryMergeAsTranslator()
                    : base("MergeAs")
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    DebugCheck.NotNull(call);
                    DebugCheck.NotNull(call.Object);
                    DebugCheck.NotNull(call.Arguments);

                    Debug.Assert(
                        call.Arguments.Count == 1 && call.Arguments[0] != null
                        && call.Arguments[0].Type.Equals(typeof(MergeOption)), "Invalid MergeAs arguments?");

                    // Note that the MergeOption must be inspected and applied BEFORE visiting the argument,
                    // so that it is 'locked down' before a sub-query with a user-specified merge option is encountered.
                    if (call.Arguments[0].NodeType
                        != ExpressionType.Constant)
                    {
                        // The 'MergeAs' method implementation on ObjectQuery<T> creates 
                        // a method call expression with a MergeOption constant argument taking 
                        // the value of the merge option argument passed to ObjectQuery.MergeAs,
                        // and so this is the only supported pattern here.
                        throw new NotSupportedException(Strings.ELinq_UnsupportedMergeAs);
                    }

                    var mergeAsOption = (MergeOption)((ConstantExpression)call.Arguments[0]).Value;
                    EntityUtil.CheckArgumentMergeOption(mergeAsOption);
                    parent.NotifyMergeOption(mergeAsOption);

                    var inputQuery = RemoveConvertToObjectQuery(call.Object);
                    var queryExpression = parent.TranslateExpression(inputQuery);
                    Span span;
                    if (!parent.TryGetSpan(queryExpression, out span))
                    {
                        span = null;
                    }

                    return parent.AddSpanMapping(queryExpression, span);
                }
            }

            private sealed class ObjectQueryIncludeSpanTranslator : ObjectQueryCallTranslator
            {
                internal ObjectQueryIncludeSpanTranslator()
                    : base("IncludeSpan")
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    DebugCheck.NotNull(call);
                    DebugCheck.NotNull(call.Object);
                    DebugCheck.NotNull(call.Arguments);

                    Debug.Assert(
                        call.Arguments.Count == 1 && call.Arguments[0] != null
                        && call.Arguments[0].Type.Equals(typeof(Span)), "Invalid IncludeSpan arguments?");
                    Debug.Assert(
                        call.Arguments[0].NodeType == ExpressionType.Constant,
                        "Whenever an IncludeSpan MethodCall is inlined, the argument must be a constant");

                    var span = (Span)((ConstantExpression)call.Arguments[0]).Value;
                    var inputQuery = RemoveConvertToObjectQuery(call.Object);
                    var queryExpression = parent.TranslateExpression(inputQuery);
                    if (!(parent.CanIncludeSpanInfo()))
                    {
                        span = null;
                    }
                    return parent.AddSpanMapping(queryExpression, span);
                }
            }

            internal sealed class DefaultTranslator : CallTranslator
            {
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var unsupportedMethod = call.Method;
                    if (unsupportedMethod.DeclaringType.Assembly().FullName == s_visualBasicAssemblyFullName
                        && unsupportedMethod.Name == "Mid"
                        && new[] { typeof(string), typeof(int) }.SequenceEqual(unsupportedMethod.GetParameters().Select(p => p.ParameterType)))
                    {
                        throw new NotSupportedException(
                            Strings.ELinq_UnsupportedMethodSuggestedAlternative(unsupportedMethod, "System.String Mid(System.String, Int32, Int32)"));
                    }
                    throw new NotSupportedException(Strings.ELinq_UnsupportedMethod(unsupportedMethod));
                }
            }

            private sealed class FunctionCallTranslator
            {
                internal CqtExpression TranslateFunctionCall(
                    ExpressionConverter parent, MethodCallExpression call, DbFunctionAttribute functionAttribute)
                {
                    Debug.Assert(!string.IsNullOrWhiteSpace(functionAttribute.NamespaceName));
                    Debug.Assert(!string.IsNullOrWhiteSpace(functionAttribute.FunctionName));

                    // Translate the inputs
                    var arguments =
                        call.Arguments.Select(a => UnwrapNoOpConverts(a)).Select(
                            b => NormalizeAllSetSources(parent, parent.TranslateExpression(b))).ToList();
                    var argumentTypes = arguments.Select(a => a.ResultType).ToList();

                    //Resolve the function
                    var function = parent.FindFunction(
                        functionAttribute.NamespaceName, functionAttribute.FunctionName, argumentTypes, false, call);

                    if (!function.IsComposableAttribute)
                    {
                        throw new NotSupportedException(Strings.CannotCallNoncomposableFunction(function.FullName));
                    }

                    DbExpression result = function.Invoke(arguments);

                    return ValidateReturnType(result, result.ResultType, parent, call, call.Type, false);
                }

                // <summary>
                // Recursively rewrite the argument expression to unwrap any "structured" set sources
                // using ExpressionConverter.NormalizeSetSource(). This is currently required for IGrouping
                // and EntityCollection as argument types to functions.
                // NOTE: Changes made to this function might have to be applied to ExpressionConverter.NormalizeSetSource() too.
                // </summary>
                private CqtExpression NormalizeAllSetSources(ExpressionConverter parent, CqtExpression argumentExpr)
                {
                    DbExpression newExpr = null;
                    var type = argumentExpr.ResultType.EdmType.BuiltInTypeKind;

                    switch (type)
                    {
                        case BuiltInTypeKind.CollectionType:
                            {
                                var bindingExpr = argumentExpr.BindAs(parent.AliasGenerator.Next());
                                var normalizedExpr = NormalizeAllSetSources(parent, bindingExpr.Variable);
                                if (normalizedExpr != bindingExpr.Variable)
                                {
                                    newExpr = bindingExpr.Project(normalizedExpr);
                                }
                                break;
                            }
                        case BuiltInTypeKind.RowType:
                            {
                                var newColumns = new List<KeyValuePair<string, DbExpression>>();
                                var rowType = argumentExpr.ResultType.EdmType as RowType;
                                var isAnyPropertyChanged = false;

                                foreach (var recColumn in rowType.Properties)
                                {
                                    var propertyExpr = argumentExpr.Property(recColumn);
                                    newExpr = NormalizeAllSetSources(parent, propertyExpr);
                                    if (newExpr != propertyExpr)
                                    {
                                        isAnyPropertyChanged = true;
                                        newColumns.Add(new KeyValuePair<string, DbExpression>(propertyExpr.Property.Name, newExpr));
                                    }
                                    else
                                    {
                                        newColumns.Add(new KeyValuePair<string, DbExpression>(propertyExpr.Property.Name, propertyExpr));
                                    }
                                }

                                if (isAnyPropertyChanged)
                                {
                                    newExpr = DbExpressionBuilder.NewRow(newColumns);
                                }
                                else
                                {
                                    newExpr = argumentExpr;
                                }
                                break;
                            }
                    }

                    // If the expression has not changed, return the original expression
                    if (newExpr != null
                        && newExpr != argumentExpr)
                    {
                        return parent.NormalizeSetSource(newExpr);
                    }
                    else
                    {
                        return parent.NormalizeSetSource(argumentExpr);
                    }
                }

                // <summary>
                // Removes casts where possible, for example Cast from a Reference type to Object type
                // Handles nested converts recursively. Removing no-op casts is required to prevent the
                // expression converter from complaining.
                // </summary>
                private Expression UnwrapNoOpConverts(Expression expression)
                {
                    if (expression.NodeType
                        == ExpressionType.Convert)
                    {
                        var convertExpression = (UnaryExpression)expression;

                        // Unwrap the operand before checking assignability for a "postfix" rewrite.
                        // The modified conversion tree is constructed bottom-up.
                        var operand = UnwrapNoOpConverts(convertExpression.Operand);
                        if (expression.Type.IsAssignableFrom(operand.Type))
                        {
                            return operand;
                        }
                    }
                    return expression;
                }

                // <summary>
                // Checks if the return type specified by the call expression matches that expected by the
                // function definition. Performs a recursive check in case of Collection type.
                // </summary>
                // <param name="result"> DbFunctionExpression for the function definition </param>
                // <param name="actualReturnType"> Return type expected by the function definition </param>
                // <param name="call"> LINQ MethodCallExpression </param>
                // <param name="clrReturnType"> Return type specified by the call </param>
                // <param name="isElementOfCollection"> Indicates if current call is for an Element of a Collection type </param>
                // <returns> DbFunctionExpression with aligned return types </returns>
                private CqtExpression ValidateReturnType(
                    CqtExpression result, TypeUsage actualReturnType, ExpressionConverter parent, MethodCallExpression call,
                    Type clrReturnType, bool isElementOfCollection)
                {
                    var modelType = actualReturnType.EdmType.BuiltInTypeKind;
                    switch (modelType)
                    {
                        case BuiltInTypeKind.CollectionType:
                            {
                                //Verify if this is a collection type (if so, recursively resolve)
                                if (!clrReturnType.IsGenericType())
                                {
                                    throw new NotSupportedException(
                                        Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(
                                            call.Method, call.Method.DeclaringType));
                                }
                                var genericType = clrReturnType.GetGenericTypeDefinition();
                                if ((genericType != typeof(IEnumerable<>))
                                    && (genericType != typeof(IQueryable<>)))
                                {
                                    throw new NotSupportedException(
                                        Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(
                                            call.Method, call.Method.DeclaringType));
                                }
                                var elementType = clrReturnType.GetGenericArguments()[0];
                                result = ValidateReturnType(
                                    result, TypeHelpers.GetElementTypeUsage(actualReturnType), parent, call, elementType, true);
                                break;
                            }
                        case BuiltInTypeKind.RowType:
                            {
                                if (clrReturnType != typeof(DbDataRecord))
                                {
                                    throw new NotSupportedException(
                                        Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(
                                            call.Method, call.Method.DeclaringType));
                                }
                                break;
                            }
                        case BuiltInTypeKind.RefType:
                            {
                                if (clrReturnType != typeof(EntityKey))
                                {
                                    throw new NotSupportedException(
                                        Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(
                                            call.Method, call.Method.DeclaringType));
                                }
                                break;
                            }
                        //Handles Primitive types, Entity types and Complex types
                        default:
                            {
                                // For collection type, look for exact match of element types.
                                if (isElementOfCollection)
                                {
                                    var toType = parent.GetCastTargetType(actualReturnType, clrReturnType, null, false);
                                    if (toType != null)
                                    {
                                        throw new NotSupportedException(
                                            Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(
                                                call.Method, call.Method.DeclaringType));
                                    }
                                }

                                // Check whether the return type specified by the call can be aligned 
                                // with the actual return type of the function
                                var expectedReturnType = parent.GetValueLayerType(clrReturnType);
                                if (!TypeSemantics.IsPromotableTo(actualReturnType, expectedReturnType))
                                {
                                    throw new NotSupportedException(
                                        Strings.ELinq_DbFunctionAttributedFunctionWithWrongReturnType(
                                            call.Method, call.Method.DeclaringType));
                                }

                                // For scalar return types, align the return types if needed.
                                if (!isElementOfCollection)
                                {
                                    result = parent.AlignTypes(result, clrReturnType);
                                }
                                break;
                            }
                    }
                    return result;
                }
            }

            internal sealed class CanonicalFunctionDefaultTranslator : CallTranslator
            {
                internal CanonicalFunctionDefaultTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    var result = new List<MethodInfo>
                        {
                            //Math functions
                            typeof(Math).GetDeclaredMethod("Ceiling", typeof(decimal)),
                            typeof(Math).GetDeclaredMethod("Ceiling", typeof(double)),
                            typeof(Math).GetDeclaredMethod("Floor", typeof(decimal)),
                            typeof(Math).GetDeclaredMethod("Floor", typeof(double)),
                            typeof(Math).GetDeclaredMethod("Round", typeof(decimal)),
                            typeof(Math).GetDeclaredMethod("Round", typeof(double)),
                            typeof(Math).GetDeclaredMethod("Round", typeof(decimal), typeof(int)),
                            typeof(Math).GetDeclaredMethod("Round", typeof(double), typeof(int)),
                            //Decimal functions
                            typeof(Decimal).GetDeclaredMethod("Floor", typeof(decimal)),
                            typeof(Decimal).GetDeclaredMethod("Ceiling", typeof(decimal)),
                            typeof(Decimal).GetDeclaredMethod("Round", typeof(decimal)),
                            typeof(Decimal).GetDeclaredMethod("Round", typeof(decimal), typeof(int)),
                            //String functions
                            typeof(String).GetDeclaredMethod("Replace", typeof(String), typeof(String)),
                            typeof(String).GetDeclaredMethod("ToLower"),
                            typeof(String).GetDeclaredMethod("ToUpper"),
                            typeof(String).GetDeclaredMethod("Trim"),
                        };

                    // Math.Abs
                    result.AddRange(
                        new[] { typeof(decimal), typeof(double), typeof(float), typeof(int), typeof(long), typeof(sbyte), typeof(short) }
                            .Select(a => typeof(Math).GetDeclaredMethod("Abs", a)));

                    return result;
                }

                // Default translator for method calls into canonical functions.
                // Translation:
                //      MethodName(arg1, arg2, .., argn) -> MethodName(arg1, arg2, .., argn)
                //      this.MethodName(arg1, arg2, .., argn) -> MethodName(this, arg1, arg2, .., argn)
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    LinqExpression[] linqArguments;

                    if (!call.Method.IsStatic)
                    {
                        Debug.Assert(call.Object != null, "Instance method without this");
                        var arguments = new List<LinqExpression>(call.Arguments.Count + 1);
                        arguments.Add(call.Object);
                        arguments.AddRange(call.Arguments);
                        linqArguments = arguments.ToArray();
                    }
                    else
                    {
                        linqArguments = call.Arguments.ToArray();
                    }
                    return parent.TranslateIntoCanonicalFunction(call.Method.Name, call, linqArguments);
                }
            }

            internal sealed class LikeFunctionTranslator : CallTranslator
            {
                internal LikeFunctionTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(DbFunctions).GetDeclaredMethod(Like, typeof(string), typeof(string));
                    yield return
                        typeof(DbFunctions).GetDeclaredMethod(Like, typeof(string), typeof(string), typeof(string));
#pragma warning disable 612,618
                    yield return
                        typeof(EntityFunctions).GetDeclaredMethod(Like, typeof(string), typeof(string));
                    yield return
                        typeof(EntityFunctions).GetDeclaredMethod(Like, typeof(string), typeof(string), typeof(string));
#pragma warning restore 612,618
                }

                // Translation:
                // object.Like(likeExpression[, escapeCharacter]) ->  
                //      object like likeExpression [escape escapeCharacter]
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return parent.TranslateLike(call);
                }
            }

            internal abstract class AsUnicodeNonUnicodeBaseFunctionTranslator : CallTranslator
            {
                private readonly bool _isUnicode;

                protected AsUnicodeNonUnicodeBaseFunctionTranslator(IEnumerable<MethodInfo> methods, bool isUnicode)
                    : base(methods)
                {
                    _isUnicode = isUnicode;
                }

                // Translation:
                //   object.AsUnicode() -> object (In its TypeUsage, the unicode facet value is set to true explicitly)
                //   object.AsNonUnicode() -> object (In its TypeUsage, the unicode facet is set to false)
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var argument = parent.TranslateExpression(call.Arguments[0]);
                    DbExpression recreatedArgument;
                    var updatedType = argument.ResultType.ShallowCopy(
                        new FacetValues
                            {
                                Unicode = _isUnicode
                            });

                    switch (argument.ExpressionKind)
                    {
                        case DbExpressionKind.Constant:
                            recreatedArgument = updatedType.Constant(((DbConstantExpression)argument).Value);
                            break;
                        case DbExpressionKind.ParameterReference:
                            recreatedArgument = updatedType.Parameter(((DbParameterReferenceExpression)argument).ParameterName);
                            break;
                        case DbExpressionKind.Null:
                            recreatedArgument = updatedType.Null();
                            break;
                        default:
                            throw new NotSupportedException(Strings.ELinq_UnsupportedAsUnicodeAndAsNonUnicode(call.Method));
                    }
                    return recreatedArgument;
                }
            }

            internal sealed class AsUnicodeFunctionTranslator : AsUnicodeNonUnicodeBaseFunctionTranslator
            {
                internal AsUnicodeFunctionTranslator()
                    : base(GetMethods(), true)
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(DbFunctions).GetDeclaredMethod(AsUnicode, typeof(string));
                    yield return
#pragma warning disable 612,618
                        typeof(EntityFunctions).GetDeclaredMethod(AsUnicode, typeof(string));
#pragma warning restore 612,618
                }
            }

            internal sealed class AsNonUnicodeFunctionTranslator : AsUnicodeNonUnicodeBaseFunctionTranslator
            {
                internal AsNonUnicodeFunctionTranslator()
                    : base(GetMethods(), false)
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(DbFunctions).GetDeclaredMethod(AsNonUnicode, typeof(string));
                    yield return
#pragma warning disable 612,618
                        typeof(EntityFunctions).GetDeclaredMethod(AsNonUnicode, typeof(string));
#pragma warning restore 612,618
                }
            }
            #region System.Enum method translators
            internal sealed class HasFlagTranslator : CallTranslator
            {
                private static readonly MethodInfo _hasFlagMethod =
                    typeof(Enum).GetDeclaredMethod("HasFlag", typeof(Enum));

                internal HasFlagTranslator()
                    : base(_hasFlagMethod)
                {
                }

                [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Scope = "member", 
                    Justification = "The argument name passed to ArgumentNullException matches the name of the argument of the HasFlag method being translated.")]
                private static CqtExpression TranslateHasFlag(ExpressionConverter parent,
                     LinqExpression sourceExpression, LinqExpression valueExpression)
                {
                    if (valueExpression.NodeType == ExpressionType.Constant && 
                        ((ConstantExpression)valueExpression).Value == null)
                    {
                        throw new ArgumentNullException("flag");
                    }
                   
                    var dbValueExp = parent.TranslateExpression(valueExpression);                    
                    var dbSourceExp = parent.TranslateExpression(sourceExpression);   
                 
                    if (dbSourceExp.ResultType.EdmType != dbValueExp.ResultType.EdmType)
                    {
                        throw new NotSupportedException(
                            Strings.ELinq_HasFlagArgumentAndSourceTypeMismatch(
                                dbValueExp.ResultType.EdmType.Name, dbSourceExp.ResultType.EdmType.Name));
                    }

                    var enumUnderlyingType = TypeHelpers.CreateEnumUnderlyingTypeUsage(dbSourceExp.ResultType);
                    var valueExpresionCast = dbValueExp.CastTo(enumUnderlyingType);

                    return
                        dbSourceExp.CastTo(enumUnderlyingType)
                            .BitwiseAnd(valueExpresionCast)
                            .Equal(valueExpresionCast);
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return TranslateHasFlag(parent, call.Object, call.Arguments[0]);
                }
            }
            #endregion

            #region System.Math method translators

            internal sealed class MathTruncateTranslator : CallTranslator
            {
                internal MathTruncateTranslator()
                    : base(new[]
                        {
                            typeof(Math).GetDeclaredMethod("Truncate", typeof(decimal)),
                            typeof(Math).GetDeclaredMethod("Truncate", typeof(double))
                        })
                {
                }

                // Translation:
                //      Truncate(arg1)		     -> Truncate(arg1, 0)
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(call.Arguments.Count == 1, "Expecting 1 argument for Math.Truncate");

                    var arg1 = parent.TranslateExpression(call.Arguments[0]);
                    var zeroDigits = DbExpressionBuilder.Constant(0);
                    return arg1.Truncate(zeroDigits);
                }
            }

            internal sealed class MathPowerTranslator : CallTranslator
            {
                internal MathPowerTranslator()
                    : base(new[]
                        {
                            typeof(Math).GetDeclaredMethod("Pow", typeof(double), typeof(double))
                        })
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var arg1 = parent.TranslateExpression(call.Arguments[0]);
                    var arg2 = parent.TranslateExpression(call.Arguments[1]);
                    return arg1.Power(arg2);
                }
            }

            #endregion

            #region System.Guid method translators

            internal sealed class GuidNewGuidTranslator : CallTranslator
            {
                internal GuidNewGuidTranslator()
                    : base(new[]
                        {
                            typeof(Guid).GetDeclaredMethod("NewGuid")
                        })
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return EdmFunctions.NewGuid();
                }
            }

            #endregion

            #region System.String Method Translators

            internal sealed class StringContainsTranslator : CallTranslator
            {
                internal StringContainsTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("Contains", typeof(string));
                }

                // Translation:
                // object.EndsWith(argument) ->  
                //      1) if argument is a constant or parameter and the provider supports escaping: 
                //          object like "%" + argument1 + "%", where argument1 is argument escaped by the provider
                //      2) Otherwise:
                //           object.Contains(argument) ->  IndexOf(argument, object) > 0
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return parent.TranslateFunctionIntoLike(call, true, true, CreateDefaultTranslation);
                }

                // DefaultTranslation:
                //   object.Contains(argument) ->  IndexOf(argument, object) > 0
                private static CqtExpression CreateDefaultTranslation(
                    ExpressionConverter parent, MethodCallExpression call, CqtExpression patternExpression, CqtExpression inputExpression)
                {
                    var indexOfExpression = parent.CreateCanonicalFunction(IndexOf, call, patternExpression, inputExpression);
                    var comparisonExpression = indexOfExpression.GreaterThan(DbExpressionBuilder.Constant(0));
                    return comparisonExpression;
                }
            }

            internal sealed class IndexOfTranslator : CallTranslator
            {
                internal IndexOfTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("IndexOf", typeof(string));
                }

                // Translation:
                //      IndexOf(arg1)		     -> IndexOf(arg1, this) - 1
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(call.Arguments.Count == 1, "Expecting 1 argument for String.IndexOf");

                    var indexOfExpression = parent.TranslateIntoCanonicalFunction(IndexOf, call, call.Arguments[0], call.Object);
                    CqtExpression minusExpression = indexOfExpression.Minus(DbExpressionBuilder.Constant(1));

                    return minusExpression;
                }
            }

            internal sealed class StartsWithTranslator : CallTranslator
            {
                internal StartsWithTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("StartsWith", typeof(string));
                }

                // Translation:
                // object.StartsWith(argument) ->  
                //          1) if argument is a constant or parameter and the provider supports escaping: 
                //                  object like argument1 + "%", where argument1 is argument escaped by the provider
                //          2) otherwise: 
                //                  IndexOf(argument, object) == 1
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return parent.TranslateFunctionIntoLike(call, false, true, CreateDefaultTranslation);
                }

                // Default translation:
                //      object.StartsWith(argument) ->  IndexOf(argument, object) == 1
                private static CqtExpression CreateDefaultTranslation(
                    ExpressionConverter parent, MethodCallExpression call, CqtExpression patternExpression, CqtExpression inputExpression)
                {
                    DbExpression indexOfExpression = parent.CreateCanonicalFunction(IndexOf, call, patternExpression, inputExpression)
                                                           .Equal(DbExpressionBuilder.Constant(1));
                    return indexOfExpression;
                }
            }

            internal sealed class EndsWithTranslator : CallTranslator
            {
                internal EndsWithTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("EndsWith", typeof(string));
                }

                // Translation:
                // object.EndsWith(argument) ->  
                //      1) if argument is a constant or parameter and the provider supports escaping:
                //          object like "%" + argument1, where argument1 is argument escaped by the provider
                //      2) Otherwise:
                //          object.EndsWith(argument) ->  IndexOf(Reverse(argument), Reverse(object)) = 1
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return parent.TranslateFunctionIntoLike(call, true, false, CreateDefaultTranslation);
                }

                // Default Translation:
                //   object.EndsWith(argument) ->  IndexOf(Reverse(argument), Reverse(object)) = 1
                private static CqtExpression CreateDefaultTranslation(
                    ExpressionConverter parent, MethodCallExpression call, CqtExpression patternExpression, CqtExpression inputExpression)
                {
                    var reversePatternExpression = parent.CreateCanonicalFunction(Reverse, call, patternExpression);
                    var reverseInputExpression = parent.CreateCanonicalFunction(Reverse, call, inputExpression);

                    DbExpression indexOfExpression = parent.CreateCanonicalFunction(
                        IndexOf, call, reversePatternExpression, reverseInputExpression)
                                                           .Equal(DbExpressionBuilder.Constant(1));
                    return indexOfExpression;
                }
            }

            internal sealed class SubstringTranslator : CallTranslator
            {
                internal SubstringTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("Substring", typeof(int));
                    yield return
                        typeof(String).GetDeclaredMethod("Substring", typeof(int), typeof(int));
                }

                // Translation:
                //      Substring(arg1)        ->  Substring(this, arg1+1, Length(this) - arg1))
                //      Substring(arg1, arg2)  ->  Substring(this, arg1+1, arg2)
                //
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(call.Arguments.Count == 1 || call.Arguments.Count == 2, "Expecting 1 or 2 arguments for String.Substring");

                    var arg1 = parent.TranslateExpression(call.Arguments[0]);

                    var target = parent.TranslateExpression(call.Object);
                    DbExpression fromIndex = arg1.Plus(DbExpressionBuilder.Constant(1));

                    CqtExpression length;
                    if (call.Arguments.Count == 1)
                    {
                        length = parent.CreateCanonicalFunction(Length, call, target)
                                       .Minus(arg1);
                    }
                    else
                    {
                        length = parent.TranslateExpression(call.Arguments[1]);
                    }

                    CqtExpression substringExpression = parent.CreateCanonicalFunction(Substring, call, target, fromIndex, length);
                    return substringExpression;
                }
            }

            internal sealed class RemoveTranslator : CallTranslator
            {
                internal RemoveTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("Remove", typeof(int));
                    yield return
                        typeof(String).GetDeclaredMethod("Remove", typeof(int), typeof(int));
                }

                // Translation:
                //      Remove(arg1)        ->  Substring(this, 1, arg1)
                //      Remove(arg1, arg2)  ->  Concat(Substring(this, 1, arg1) , Substring(this, arg1 + arg2 + 1, Length(this) - (arg1 + arg2))) 
                //      Remove(arg1, arg2) is only supported if arg2 is a non-negative integer
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(call.Arguments.Count == 1 || call.Arguments.Count == 2, "Expecting 1 or 2 arguments for String.Remove");

                    var thisString = parent.TranslateExpression(call.Object);
                    var arg1 = parent.TranslateExpression(call.Arguments[0]);

                    //Substring(this, 1, arg1)
                    CqtExpression result =
                        parent.CreateCanonicalFunction(
                            Substring, call,
                            thisString,
                            DbExpressionBuilder.Constant(1),
                            arg1);

                    //Concat(result, Substring(this, (arg1 + arg2) +1, Length(this) - (arg1 + arg2))) 
                    if (call.Arguments.Count == 2)
                    {
                        //If there are two arguments, we only support cases when the second one translates to a non-negative constant
                        var arg2 = parent.TranslateExpression(call.Arguments[1]);
                        if (!IsNonNegativeIntegerConstant(arg2))
                        {
                            throw new NotSupportedException(
                                Strings.ELinq_UnsupportedStringRemoveCase(call.Method, call.Method.GetParameters()[1].Name));
                        }

                        // Build the second substring
                        // (arg1 + arg2) +1
                        CqtExpression substringStartIndex =
                            arg1.Plus(arg2).Plus(DbExpressionBuilder.Constant(1));

                        // Length(this) - (arg1 + arg2)
                        CqtExpression substringLength =
                            parent.CreateCanonicalFunction(Length, call, thisString)
                                  .Minus(arg1.Plus(arg2));

                        // Substring(this, substringStartIndex, substringLength)
                        CqtExpression secondSubstring =
                            parent.CreateCanonicalFunction(
                                Substring, call,
                                thisString,
                                substringStartIndex,
                                substringLength);

                        // result = Concat (result, secondSubstring)
                        result = parent.CreateCanonicalFunction(Concat, call, result, secondSubstring);
                    }
                    return result;
                }

                private static bool IsNonNegativeIntegerConstant(CqtExpression argument)
                {
                    // Check whether it is a constant of type Int32
                    if (argument.ExpressionKind != DbExpressionKind.Constant
                        ||
                        !TypeSemantics.IsPrimitiveType(argument.ResultType, PrimitiveTypeKind.Int32))
                    {
                        return false;
                    }

                    // Check whether its value is non-negative
                    var constantExpression = (DbConstantExpression)argument;
                    var value = (int)constantExpression.Value;
                    if (value < 0)
                    {
                        return false;
                    }

                    return true;
                }
            }

            internal sealed class InsertTranslator : CallTranslator
            {
                internal InsertTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("Insert", typeof(int), typeof(string));
                }

                // Translation:
                //      Insert(startIndex, value) ->  Concat(Concat(Substring(this, 1, startIndex), value), Substring(this, startIndex+1, Length(this) - startIndex))
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(call.Arguments.Count == 2, "Expecting 2 arguments for String.Insert");

                    //Substring(this, 1, startIndex)
                    var thisString = parent.TranslateExpression(call.Object);
                    var arg1 = parent.TranslateExpression(call.Arguments[0]);
                    CqtExpression firstSubstring =
                        parent.CreateCanonicalFunction(
                            Substring, call,
                            thisString,
                            DbExpressionBuilder.Constant(1),
                            arg1);

                    //Substring(this, startIndex+1, Length(this) - startIndex)
                    CqtExpression secondSubstring =
                        parent.CreateCanonicalFunction(
                            Substring, call,
                            thisString,
                            arg1.Plus(DbExpressionBuilder.Constant(1)),
                            parent.CreateCanonicalFunction(Length, call, thisString)
                                  .Minus(arg1));

                    // result = Concat( Concat (firstSubstring, value), secondSubstring )
                    var arg2 = parent.TranslateExpression(call.Arguments[1]);
                    CqtExpression result = parent.CreateCanonicalFunction(
                        Concat, call,
                        parent.CreateCanonicalFunction(
                            Concat, call,
                            firstSubstring,
                            arg2),
                        secondSubstring);
                    return result;
                }
            }

            internal sealed class IsNullOrEmptyTranslator : CallTranslator
            {
                internal IsNullOrEmptyTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("IsNullOrEmpty", typeof(string));
                }

                // Translation:
                //      IsNullOrEmpty(value) ->  (IsNull(value)) OR Length(value) = 0
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(call.Arguments.Count == 1, "Expecting 1 argument for String.IsNullOrEmpty");

                    //IsNull(value)
                    var value = parent.TranslateExpression(call.Arguments[0]);
                    CqtExpression isNullExpression = value.IsNull();

                    //Length(value) = 0
                    CqtExpression emptyStringExpression =
                        parent.CreateCanonicalFunction(Length, call, value)
                              .Equal(DbExpressionBuilder.Constant(0));

                    CqtExpression result = isNullExpression.Or(emptyStringExpression);
                    return result;
                }
            }

            internal sealed class StringConcatTranslator : CallTranslator
            {
                internal StringConcatTranslator()
                    : base(GetMethods())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(string), typeof(string));
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(string), typeof(string), typeof(string));
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(string), typeof(string), typeof(string), typeof(string));
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(object), typeof(object));
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(object), typeof(object), typeof(object));
#if !NETSTANDARD2_1
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(object), typeof(object), typeof(object), typeof(object));
#endif
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(object[]));
                    yield return
                        typeof(String).GetDeclaredMethod("Concat", typeof(string[]));
                }

                // Translation:
                //      Concat (arg1, arg2)                 -> Concat(arg1, arg2)
                //      Concat (arg1, arg2, arg3)           -> Concat(Concat(arg1, arg2), arg3)
                //      Concat (arg1, arg2, arg3, arg4)     -> Concat(Concat(Concat(arg1, arg2), arg3), arg4)
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Expression[] args;

                    if (call.Arguments.Count == 1 && (call.Arguments.First().Type == typeof(object[]) || call.Arguments.First().Type == typeof(string[])))
                    {
                        var newArrayExpression = call.Arguments[0] as NewArrayExpression;
                        if (newArrayExpression != null)
                        {
                            args = ((NewArrayExpression)call.Arguments[0]).Expressions.ToArray();    
                        }
                        else
                        {
                            Debug.Assert(call.Arguments[0] is ConstantExpression);

                            var valueExpression = ((ConstantExpression)call.Arguments[0]);

                            if (valueExpression.Value == null)
                            {
                                throw new ArgumentNullException(
                                    valueExpression.Type == typeof(object[]) ? "args" : "values");
                            }

                            // note: array convariance - valueExpression.Value can be string[]
                            args = ((object[])valueExpression.Value)
                                .Select(v => Expression.Constant(v)).ToArray();
                        }
                    }
                    else
                    {
                        args = call.Arguments.ToArray();
                    }

                    return StringTranslatorUtil.ConcatArgs(parent, call, args);
                }
            }

            internal sealed class ToStringTranslator : CallTranslator
            {
                private static readonly MethodInfo[] _methods = 
                {
                        typeof(string).GetDeclaredMethod("ToString"),
                        typeof(byte).GetDeclaredMethod("ToString"),
                        typeof(sbyte).GetDeclaredMethod("ToString"),
                        typeof(short).GetDeclaredMethod("ToString"),
                        typeof(int).GetDeclaredMethod("ToString"),
                        typeof(long).GetDeclaredMethod("ToString"),
                        typeof(double).GetDeclaredMethod("ToString"),
                        typeof(float).GetDeclaredMethod("ToString"),
                        typeof(Guid).GetDeclaredMethod("ToString"),
                        typeof(DateTime).GetDeclaredMethod("ToString"),
                        typeof(DateTimeOffset).GetDeclaredMethod("ToString"),
                        typeof(TimeSpan).GetDeclaredMethod("ToString"),
                        typeof(decimal).GetDeclaredMethod("ToString"),
                        typeof(bool).GetDeclaredMethod("ToString"),
                        typeof(object).GetDeclaredMethod("ToString"),
                };

                internal ToStringTranslator()
                    : base(_methods)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return StringTranslatorUtil.ConvertToString(parent, call.Object);
                }                                
            }

            internal abstract class TrimBaseTranslator : CallTranslator
            {
                private readonly string _canonicalFunctionName;

                protected TrimBaseTranslator(IEnumerable<MethodInfo> methods, string canonicalFunctionName)
                    : base(methods)
                {
                    _canonicalFunctionName = canonicalFunctionName;
                }

                // Translation:
                //      object.MethodName -> CanonicalFunctionName(object)
                // Supported only if the argument is an empty array.
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    if (call.Arguments.Count != 0 && !IsEmptyArray(call.Arguments[0]))
                    {
                        throw new NotSupportedException(Strings.ELinq_UnsupportedTrimStartTrimEndCase(call.Method));
                    }

                    return parent.TranslateIntoCanonicalFunction(_canonicalFunctionName, call, call.Object);
                }

                internal static bool IsEmptyArray(LinqExpression expression)
                {
                    var newArray = expression as NewArrayExpression;
                    if (expression.NodeType
                        == ExpressionType.NewArrayInit)
                    {
                        if (newArray.Expressions.Count == 0)
                        {
                            return true;
                        }
                    }
                    else if (expression.NodeType
                             == ExpressionType.NewArrayBounds)
                    {
                        // To be empty, the array must have rank 1 with a single bound of 0
                        if (newArray.Expressions.Count == 1
                            &&
                            newArray.Expressions[0].NodeType == ExpressionType.Constant)
                        {
                            return Equals(((ConstantExpression)newArray.Expressions[0]).Value, 0);
                        }
                    }
                    return false;
                }
            }

            internal sealed class TrimTranslator : TrimBaseTranslator
            {
                internal TrimTranslator()
                    : base(GetMethods(), Trim)
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
#if NETSTANDARD2_1
                    yield return
                        typeof(String).GetDeclaredMethod("Trim", typeof(Char));
#endif
                    yield return
                        typeof(String).GetDeclaredMethod("Trim", typeof(Char[]));
                }
            }

            internal sealed class TrimStartTranslator : TrimBaseTranslator
            {
                internal TrimStartTranslator()
                    : base(GetMethods(), LTrim)
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
#if NETSTANDARD2_1
                    yield return
                        typeof(String).GetDeclaredMethod("TrimStart");
                    yield return
                        typeof(String).GetDeclaredMethod("TrimStart", typeof(Char));
#endif
                    yield return
                        typeof(String).GetDeclaredMethod("TrimStart", typeof(Char[]));
                }
            }

            internal sealed class TrimEndTranslator : TrimBaseTranslator
            {
                internal TrimEndTranslator()
                    : base(GetMethods(), RTrim)
                {
                }

                private static IEnumerable<MethodInfo> GetMethods()
                {
#if NETSTANDARD2_1
                    yield return
                        typeof(String).GetDeclaredMethod("TrimEnd");
                    yield return
                        typeof(String).GetDeclaredMethod("TrimEnd", typeof(Char));
#endif
                    yield return
                        typeof(String).GetDeclaredMethod("TrimEnd", typeof(Char[]));
                }
            }

            #endregion

            #region Visual Basic Specific Translators

            internal sealed class VBCanonicalFunctionDefaultTranslator : CallTranslator
            {
                private const string s_stringsTypeFullName = "Microsoft.VisualBasic.Strings";
                private const string s_dateAndTimeTypeFullName = "Microsoft.VisualBasic.DateAndTime";

                internal VBCanonicalFunctionDefaultTranslator(Assembly vbAssembly)
                    : base(GetMethods(vbAssembly))
                {
                }

                private static IEnumerable<MethodInfo> GetMethods(Assembly vbAssembly)
                {
                    //Strings Types 
                    var stringsType = vbAssembly.GetType(s_stringsTypeFullName);
                    yield return
                        stringsType.GetDeclaredMethod("Trim", typeof(string));
                    yield return
                        stringsType.GetDeclaredMethod("LTrim", typeof(string));
                    yield return
                        stringsType.GetDeclaredMethod("RTrim", typeof(string));
                    yield return
                        stringsType.GetDeclaredMethod("Left", typeof(string), typeof(int));
                    yield return
                        stringsType.GetDeclaredMethod("Right", typeof(string), typeof(int));

                    //DateTimeType
                    var dateTimeType = vbAssembly.GetType(s_dateAndTimeTypeFullName);
                    yield return
                        dateTimeType.GetDeclaredMethod("Year", typeof(DateTime));
                    yield return
                        dateTimeType.GetDeclaredMethod("Month", typeof(DateTime));
                    yield return
                        dateTimeType.GetDeclaredMethod("Day", typeof(DateTime));
                    yield return
                        dateTimeType.GetDeclaredMethod("Hour", typeof(DateTime));
                    yield return
                        dateTimeType.GetDeclaredMethod("Minute", typeof(DateTime));
                    yield return
                        dateTimeType.GetDeclaredMethod("Second", typeof(DateTime));
                }

                // Default translator for vb static method calls into canonical functions.
                // Translation:
                //      MethodName(arg1, arg2, .., argn) -> MethodName(arg1, arg2, .., argn)
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return parent.TranslateIntoCanonicalFunction(call.Method.Name, call, call.Arguments.ToArray());
                }
            }

            internal sealed class VBCanonicalFunctionRenameTranslator : CallTranslator
            {
                private const string s_stringsTypeFullName = "Microsoft.VisualBasic.Strings";
                private static readonly Dictionary<MethodInfo, string> s_methodNameMap = new Dictionary<MethodInfo, string>(4);

                internal VBCanonicalFunctionRenameTranslator(Assembly vbAssembly)
                    : base(GetMethods(vbAssembly).ToArray())
                {
                }

                private static IEnumerable<MethodInfo> GetMethods(Assembly vbAssembly)
                {
                    //Strings Types 
                    var stringsType = vbAssembly.GetType(s_stringsTypeFullName);
                    yield return GetMethodInfo(stringsType, "Len", Length, new[] { typeof(string) });
                    yield return GetMethodInfo(stringsType, "Mid", Substring, new[] { typeof(string), typeof(int), typeof(int) });
                    yield return GetMethodInfo(stringsType, "UCase", ToUpper, new[] { typeof(string) });
                    yield return GetMethodInfo(stringsType, "LCase", ToLower, new[] { typeof(string) });
                }

                private static MethodInfo GetMethodInfo(
                    Type declaringType, string methodName, string canonicalFunctionName, Type[] argumentTypes)
                {
                    var methodInfo = declaringType.GetDeclaredMethod(methodName, argumentTypes);
                    s_methodNameMap.Add(methodInfo, canonicalFunctionName);
                    return methodInfo;
                }

                // Translator for static method calls into canonical functions when only the name of the canonical function
                // is different from the name of the method, but the arguments match.
                // Translation:
                //      MethodName(arg1, arg2, .., argn) -> CanonicalFunctionName(arg1, arg2, .., argn)
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return parent.TranslateIntoCanonicalFunction(s_methodNameMap[call.Method], call, call.Arguments.ToArray());
                }
            }

            internal sealed class VBDatePartTranslator : CallTranslator
            {
                private const string s_dateAndTimeTypeFullName = "Microsoft.VisualBasic.DateAndTime";
                private const string s_DateIntervalFullName = "Microsoft.VisualBasic.DateInterval";
                private const string s_FirstDayOfWeekFullName = "Microsoft.VisualBasic.FirstDayOfWeek";
                private const string s_FirstWeekOfYearFullName = "Microsoft.VisualBasic.FirstWeekOfYear";

                private static readonly HashSet<string> _supportedIntervals = new HashSet<string>
                    {
                        Year,
                        Month,
                        Day,
                        Hour,
                        Minute,
                        Second
                    };

                internal VBDatePartTranslator(Assembly vbAssembly)
                    : base(GetMethods(vbAssembly))
                {
                }

                private static IEnumerable<MethodInfo> GetMethods(Assembly vbAssembly)
                {
                    var dateAndTimeType = vbAssembly.GetType(s_dateAndTimeTypeFullName);
                    var dateIntervalEnum = vbAssembly.GetType(s_DateIntervalFullName);
                    var firstDayOfWeekEnum = vbAssembly.GetType(s_FirstDayOfWeekFullName);
                    var firstWeekOfYearEnum = vbAssembly.GetType(s_FirstWeekOfYearFullName);

                    yield return dateAndTimeType.GetDeclaredMethod(
                        "DatePart", dateIntervalEnum, typeof(DateTime), firstDayOfWeekEnum, firstWeekOfYearEnum);
                }

                // Translation:
                //      DatePart(DateInterval, date, arg3, arg4)  ->  'DateInterval'(date)
                // Note: it is only supported for the values of DateInterval listed in _supportedIntervals.
                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(call.Arguments.Count == 4, "Expecting 4 arguments for Microsoft.VisualBasic.DateAndTime.DatePart");

                    var intervalLinqExpression = call.Arguments[0] as ConstantExpression;
                    if (intervalLinqExpression == null)
                    {
                        throw new NotSupportedException(
                            Strings.ELinq_UnsupportedVBDatePartNonConstantInterval(call.Method, call.Method.GetParameters()[0].Name));
                    }

                    var intervalValue = intervalLinqExpression.Value.ToString();
                    if (!_supportedIntervals.Contains(intervalValue))
                    {
                        throw new NotSupportedException(
                            Strings.ELinq_UnsupportedVBDatePartInvalidInterval(
                                call.Method, call.Method.GetParameters()[0].Name, intervalValue));
                    }

                    CqtExpression result = parent.TranslateIntoCanonicalFunction(intervalValue, call, call.Arguments[1]);
                    return result;
                }
            }

            #endregion

            #endregion

            #region Sequence method translators

            private abstract class SequenceMethodTranslator
            {
                private readonly IEnumerable<SequenceMethod> _methods;

                protected SequenceMethodTranslator(params SequenceMethod[] methods)
                {
                    _methods = methods;
                }

                internal IEnumerable<SequenceMethod> Methods
                {
                    get { return _methods; }
                }

                internal virtual CqtExpression Translate(
                    ExpressionConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod)
                {
                    return Translate(parent, call);
                }

                internal abstract CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call);

                public override string ToString()
                {
                    return GetType().Name;
                }
            }

            private abstract class PagingTranslator : UnarySequenceMethodTranslator
            {
                protected PagingTranslator(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand, MethodCallExpression call)
                {
                    // translate count expression
                    Debug.Assert(call.Arguments.Count == 2, "Skip and Take must have 2 arguments");
                    var linqCount = call.Arguments[1];
                    var count = parent.TranslateExpression(linqCount);

                    // translate paging expression
                    var result = TranslatePagingOperator(parent, operand, count);

                    return result;
                }

                protected abstract CqtExpression TranslatePagingOperator(
                    ExpressionConverter parent, CqtExpression operand, CqtExpression count);
            }

            private sealed class TakeTranslator : PagingTranslator
            {
                internal TakeTranslator()
                    : base(SequenceMethod.Take)
                {
                }

                protected override CqtExpression TranslatePagingOperator(
                    ExpressionConverter parent, CqtExpression operand, CqtExpression count)
                {
                    var constant = count as DbConstantExpression;
                    return constant == null || !constant.Value.Equals(0)
                        ? parent.Limit(operand, count)
                        : parent.Filter(operand.BindAs(parent.AliasGenerator.Next()), DbExpressionBuilder.False);
                }
            }

            private sealed class SkipTranslator : PagingTranslator
            {
                internal SkipTranslator()
                    : base(SequenceMethod.Skip)
                {
                }

                protected override CqtExpression TranslatePagingOperator(
                    ExpressionConverter parent, CqtExpression operand, CqtExpression count)
                {
                    return parent.Skip(operand.BindAs(parent.AliasGenerator.Next()), count);
                }
            }

            private sealed class JoinTranslator : SequenceMethodTranslator
            {
                internal JoinTranslator()
                    : base(SequenceMethod.Join)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(5 == call.Arguments.Count);
                    // get expressions describing inputs to the join
                    var outer = parent.TranslateSet(call.Arguments[0]);
                    var inner = parent.TranslateSet(call.Arguments[1]);

                    // get expressions describing key selectors
                    var outerLambda = parent.GetLambdaExpression(call, 2);
                    var innerLambda = parent.GetLambdaExpression(call, 3);

                    // get outer selector expression
                    var selectorLambda = parent.GetLambdaExpression(call, 4);

                    // check if the selector is a trivial rename such as
                    //      select outer as m, inner as n from (...) as outer join (...) as inner on ...
                    // In case of the trivial rename, simply name the join inputs as m and n, 
                    // otherwise generate a projection for the selector.
                    string outerBindingName;
                    string innerBindingName;
                    InitializerMetadata initializerMetadata;
                    var selectorLambdaIsTrivialRename = IsTrivialRename(
                        selectorLambda, parent, out outerBindingName, out innerBindingName, out initializerMetadata);

                    // translator key selectors
                    DbExpressionBinding outerBinding;
                    DbExpressionBinding innerBinding;
                    var outerKeySelector = selectorLambdaIsTrivialRename
                                               ? parent.TranslateLambda(outerLambda, outer, outerBindingName, out outerBinding)
                                               : parent.TranslateLambda(outerLambda, outer, out outerBinding);
                    var innerKeySelector = selectorLambdaIsTrivialRename
                                               ? parent.TranslateLambda(innerLambda, inner, innerBindingName, out innerBinding)
                                               : parent.TranslateLambda(innerLambda, inner, out innerBinding);

                    // construct join expression
                    if (!TypeSemantics.IsEqualComparable(outerKeySelector.ResultType)
                        ||
                        !TypeSemantics.IsEqualComparable(innerKeySelector.ResultType))
                    {
                        throw new NotSupportedException(Strings.ELinq_UnsupportedKeySelector(call.Method.Name));
                    }

                    var joinCondition = parent.CreateEqualsExpression(
                        outerKeySelector, innerKeySelector, EqualsPattern.PositiveNullEqualityNonComposable, outerLambda.Body.Type,
                        innerLambda.Body.Type);

                    // In case of trivial rename create and return the join expression,
                    // otherwise continue with generation of the selector projection.
                    if (selectorLambdaIsTrivialRename)
                    {
                        var resultType = TypeUsage.Create(
                            TypeHelpers.CreateRowType(
                                new List<KeyValuePair<string, TypeUsage>>
                                    {
                                        new KeyValuePair<string, TypeUsage>(outerBinding.VariableName, outerBinding.VariableType),
                                        new KeyValuePair<string, TypeUsage>(innerBinding.VariableName, innerBinding.VariableType)
                                    },
                                initializerMetadata));

                        return new DbJoinExpression(
                            DbExpressionKind.InnerJoin, TypeUsage.Create(TypeHelpers.CreateCollectionType(resultType)), outerBinding,
                            innerBinding, joinCondition);
                    }

                    var join = outerBinding.InnerJoin(innerBinding, joinCondition);

                    // generate the projection for the non-trivial selector.
                    var joinBinding = join.BindAs(parent.AliasGenerator.Next());

                    // create property expressions for the inner and outer 
                    var joinOuter = joinBinding.Variable.Property(outerBinding.VariableName);
                    var joinInner = joinBinding.Variable.Property(innerBinding.VariableName);

                    // push outer and inner join parts into the binding scope (the order
                    // is irrelevant because the binding context matches based on parameter
                    // reference rather than ordinal)
                    parent._bindingContext.PushBindingScope(new Binding(selectorLambda.Parameters[0], joinOuter));
                    parent._bindingContext.PushBindingScope(new Binding(selectorLambda.Parameters[1], joinInner));

                    // translate join selector
                    var selector = parent.TranslateExpression(selectorLambda.Body);

                    // pop binding scope
                    parent._bindingContext.PopBindingScope();
                    parent._bindingContext.PopBindingScope();

                    return joinBinding.Project(selector);
                }
            }

            private abstract class BinarySequenceMethodTranslator : SequenceMethodTranslator
            {
                protected BinarySequenceMethodTranslator(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                // This method is not required to be virtual (but TranslateRight has to be). This helps improve
                // performance as this class is used frequently during CQT generation phase.
                private static CqtExpression TranslateLeft(ExpressionConverter parent, LinqExpression expr)
                {
                    return parent.TranslateSet(expr);
                }

                protected virtual CqtExpression TranslateRight(ExpressionConverter parent, LinqExpression expr)
                {
                    return parent.TranslateSet(expr);
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    if (null != call.Object)
                    {
                        // instance method
                        Debug.Assert(1 == call.Arguments.Count);
                        var left = TranslateLeft(parent, call.Object);
                        var right = TranslateRight(parent, call.Arguments[0]);
                        return TranslateBinary(parent, left, right);
                    }
                    else
                    {
                        // static extension method
                        Debug.Assert(2 == call.Arguments.Count);
                        var left = TranslateLeft(parent, call.Arguments[0]);
                        var right = TranslateRight(parent, call.Arguments[1]);
                        return TranslateBinary(parent, left, right);
                    }
                }

                protected abstract CqtExpression TranslateBinary(ExpressionConverter parent, CqtExpression left, CqtExpression right);
            }

            private class ConcatTranslator : BinarySequenceMethodTranslator
            {
                internal ConcatTranslator()
                    : base(SequenceMethod.Concat)
                {
                }

                protected override CqtExpression TranslateBinary(ExpressionConverter parent, CqtExpression left, CqtExpression right)
                {
                    return parent.UnionAll(left, right);
                }
            }

            private sealed class UnionTranslator : BinarySequenceMethodTranslator
            {
                internal UnionTranslator()
                    : base(SequenceMethod.Union)
                {
                }

                protected override CqtExpression TranslateBinary(ExpressionConverter parent, CqtExpression left, CqtExpression right)
                {
                    return parent.Distinct(parent.UnionAll(left, right));
                }
            }

            private sealed class IntersectTranslator : BinarySequenceMethodTranslator
            {
                internal IntersectTranslator()
                    : base(SequenceMethod.Intersect)
                {
                }

                protected override CqtExpression TranslateBinary(ExpressionConverter parent, CqtExpression left, CqtExpression right)
                {
                    return parent.Intersect(left, right);
                }
            }

            private sealed class ExceptTranslator : BinarySequenceMethodTranslator
            {
                internal ExceptTranslator()
                    : base(SequenceMethod.Except)
                {
                }

                protected override CqtExpression TranslateBinary(ExpressionConverter parent, CqtExpression left, CqtExpression right)
                {
                    return parent.Except(left, right);
                }

                protected override CqtExpression TranslateRight(ExpressionConverter parent, LinqExpression expr)
                {
#if DEBUG
                    var preValue = parent.IgnoreInclude;
#endif
                    parent.IgnoreInclude++;
                    var result = base.TranslateRight(parent, expr);
                    parent.IgnoreInclude--;
#if DEBUG
                    Debug.Assert(preValue == parent.IgnoreInclude);
#endif
                    return result;
                }
            }

            private abstract class AggregateTranslator : SequenceMethodTranslator
            {
                private readonly string _functionName;
                private readonly bool _takesPredicate;

                protected AggregateTranslator(string functionName, bool takesPredicate, params SequenceMethod[] methods)
                    : base(methods)
                {
                    _takesPredicate = takesPredicate;
                    _functionName = functionName;
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var isUnary = 1 == call.Arguments.Count;
                    Debug.Assert(isUnary || 2 == call.Arguments.Count);

                    var operand = parent.TranslateSet(call.Arguments[0]);

                    if (!isUnary)
                    {
                        var lambda = parent.GetLambdaExpression(call, 1);
                        DbExpressionBinding sourceBinding;
                        var cqtLambda = parent.TranslateLambda(lambda, operand, out sourceBinding);

                        if (_takesPredicate)
                        {
                            // treat the lambda as a filter
                            operand = parent.Filter(sourceBinding, cqtLambda);
                        }
                        else
                        {
                            // treat the lambda as a selector
                            operand = sourceBinding.Project(cqtLambda);
                        }
                    }

                    var returnType = GetReturnType(parent, call);
                    var function = FindFunction(parent, call, returnType);

                    operand = WrapCollectionOperand(parent, operand, returnType);
                    var arguments = new List<DbExpression>(1);
                    arguments.Add(operand);

                    DbExpression result = function.Invoke(arguments);
                    result = parent.AlignTypes(result, call.Type);

                    return result;
                }

                protected virtual TypeUsage GetReturnType(ExpressionConverter parent, MethodCallExpression call)
                {
                    DebugCheck.NotNull(parent);
                    DebugCheck.NotNull(call);

                    return parent.GetValueLayerType(call.Type);
                }

                // If necessary, wraps the operand to ensure the appropriate aggregate overload is called
                protected virtual CqtExpression WrapCollectionOperand(
                    ExpressionConverter parent, CqtExpression operand,
                    TypeUsage returnType)
                {
                    // check if the operand needs to be wrapped to ensure the correct function overload is called
                    if (!TypeUsageEquals(returnType, ((CollectionType)operand.ResultType.EdmType).TypeUsage))
                    {
                        var operandCastBinding = operand.BindAs(parent.AliasGenerator.Next());
                        var operandCastProjection = operandCastBinding.Project(operandCastBinding.Variable.CastTo(returnType));
                        operand = operandCastProjection;
                    }
                    return operand;
                }

                // If necessary, wraps the operand to ensure the appropriate aggregate overload is called
                protected virtual CqtExpression WrapNonCollectionOperand(
                    ExpressionConverter parent, CqtExpression operand,
                    TypeUsage returnType)
                {
                    if (!TypeUsageEquals(returnType, operand.ResultType))
                    {
                        operand = operand.CastTo(returnType);
                    }
                    return operand;
                }

                // Finds the best function overload given the expected return type
                protected virtual EdmFunction FindFunction(
                    ExpressionConverter parent, MethodCallExpression call,
                    TypeUsage argumentType)
                {
                    var argTypes = new List<TypeUsage>(1);
                    // In general, we use the return type as the parameter type to align LINQ semantics 
                    // with SQL semantics, and avoid apparent loss of precision for some LINQ aggregate operators.
                    // (e.g., AVG(1, 2) = 2.0, AVG((double)1, (double)2)) = 1.5)
                    argTypes.Add(argumentType);

                    return parent.FindCanonicalFunction(_functionName, argTypes, true /* isGroupAggregateFunction */, call);
                }
            }

            private sealed class MaxTranslator : AggregateTranslator
            {
                internal MaxTranslator()
                    : base("Max", false,
                        SequenceMethod.Max,
                        SequenceMethod.MaxSelector,
                        SequenceMethod.MaxInt,
                        SequenceMethod.MaxIntSelector,
                        SequenceMethod.MaxDecimal,
                        SequenceMethod.MaxDecimalSelector,
                        SequenceMethod.MaxDouble,
                        SequenceMethod.MaxDoubleSelector,
                        SequenceMethod.MaxLong,
                        SequenceMethod.MaxLongSelector,
                        SequenceMethod.MaxSingle,
                        SequenceMethod.MaxSingleSelector,
                        SequenceMethod.MaxNullableDecimal,
                        SequenceMethod.MaxNullableDecimalSelector,
                        SequenceMethod.MaxNullableDouble,
                        SequenceMethod.MaxNullableDoubleSelector,
                        SequenceMethod.MaxNullableInt,
                        SequenceMethod.MaxNullableIntSelector,
                        SequenceMethod.MaxNullableLong,
                        SequenceMethod.MaxNullableLongSelector,
                        SequenceMethod.MaxNullableSingle,
                        SequenceMethod.MaxNullableSingleSelector)
                {
                }

                protected override TypeUsage GetReturnType(ExpressionConverter parent, MethodCallExpression call)
                {
                    DebugCheck.NotNull(parent);
                    DebugCheck.NotNull(call);

                    var returnType = base.GetReturnType(parent, call);

                    // This allows to find and use the correct overload of Max function for enums. 
                    // Note that returnType does not have to be scalar type here (error case).
                    return TypeSemantics.IsEnumerationType(returnType)
                               ? TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(returnType.EdmType), returnType.Facets)
                               : returnType;
                }
            }

            private sealed class MinTranslator : AggregateTranslator
            {
                internal MinTranslator()
                    : base("Min", false,
                        SequenceMethod.Min,
                        SequenceMethod.MinSelector,
                        SequenceMethod.MinDecimal,
                        SequenceMethod.MinDecimalSelector,
                        SequenceMethod.MinDouble,
                        SequenceMethod.MinDoubleSelector,
                        SequenceMethod.MinInt,
                        SequenceMethod.MinIntSelector,
                        SequenceMethod.MinLong,
                        SequenceMethod.MinLongSelector,
                        SequenceMethod.MinNullableDecimal,
                        SequenceMethod.MinSingle,
                        SequenceMethod.MinSingleSelector,
                        SequenceMethod.MinNullableDecimalSelector,
                        SequenceMethod.MinNullableDouble,
                        SequenceMethod.MinNullableDoubleSelector,
                        SequenceMethod.MinNullableInt,
                        SequenceMethod.MinNullableIntSelector,
                        SequenceMethod.MinNullableLong,
                        SequenceMethod.MinNullableLongSelector,
                        SequenceMethod.MinNullableSingle,
                        SequenceMethod.MinNullableSingleSelector)
                {
                }

                protected override TypeUsage GetReturnType(ExpressionConverter parent, MethodCallExpression call)
                {
                    DebugCheck.NotNull(parent);
                    DebugCheck.NotNull(call);

                    var returnType = base.GetReturnType(parent, call);

                    // This allows to find and use the correct overload of Min function for enums.
                    // Note that returnType does not have to be scalar type here (error case).
                    return TypeSemantics.IsEnumerationType(returnType)
                               ? TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(returnType.EdmType), returnType.Facets)
                               : returnType;
                }
            }

            private sealed class AverageTranslator : AggregateTranslator
            {
                internal AverageTranslator()
                    : base("Avg", false,
                        SequenceMethod.AverageDecimal,
                        SequenceMethod.AverageDecimalSelector,
                        SequenceMethod.AverageDouble,
                        SequenceMethod.AverageDoubleSelector,
                        SequenceMethod.AverageInt,
                        SequenceMethod.AverageIntSelector,
                        SequenceMethod.AverageLong,
                        SequenceMethod.AverageLongSelector,
                        SequenceMethod.AverageSingle,
                        SequenceMethod.AverageSingleSelector,
                        SequenceMethod.AverageNullableDecimal,
                        SequenceMethod.AverageNullableDecimalSelector,
                        SequenceMethod.AverageNullableDouble,
                        SequenceMethod.AverageNullableDoubleSelector,
                        SequenceMethod.AverageNullableInt,
                        SequenceMethod.AverageNullableIntSelector,
                        SequenceMethod.AverageNullableLong,
                        SequenceMethod.AverageNullableLongSelector,
                        SequenceMethod.AverageNullableSingle,
                        SequenceMethod.AverageNullableSingleSelector)
                {
                }
            }

            private sealed class SumTranslator : AggregateTranslator
            {
                internal SumTranslator()
                    : base("Sum", false,
                        SequenceMethod.SumDecimal,
                        SequenceMethod.SumDecimalSelector,
                        SequenceMethod.SumDouble,
                        SequenceMethod.SumDoubleSelector,
                        SequenceMethod.SumInt,
                        SequenceMethod.SumIntSelector,
                        SequenceMethod.SumLong,
                        SequenceMethod.SumLongSelector,
                        SequenceMethod.SumSingle,
                        SequenceMethod.SumSingleSelector,
                        SequenceMethod.SumNullableDecimal,
                        SequenceMethod.SumNullableDecimalSelector,
                        SequenceMethod.SumNullableDouble,
                        SequenceMethod.SumNullableDoubleSelector,
                        SequenceMethod.SumNullableInt,
                        SequenceMethod.SumNullableIntSelector,
                        SequenceMethod.SumNullableLong,
                        SequenceMethod.SumNullableLongSelector,
                        SequenceMethod.SumNullableSingle,
                        SequenceMethod.SumNullableSingleSelector)
                {
                }
            }

            private abstract class CountTranslatorBase : AggregateTranslator
            {
                protected CountTranslatorBase(string functionName, params SequenceMethod[] methods)
                    : base(functionName, true, methods)
                {
                }

                protected override CqtExpression WrapCollectionOperand(
                    ExpressionConverter parent, CqtExpression operand, TypeUsage returnType)
                {
                    // always count a constant value
                    var constantProject = operand.BindAs(parent.AliasGenerator.Next()).Project(DbExpressionBuilder.Constant(1));
                    return constantProject;
                }

                protected override CqtExpression WrapNonCollectionOperand(
                    ExpressionConverter parent, CqtExpression operand, TypeUsage returnType)
                {
                    // always count a constant value
                    DbExpression constantExpression = DbExpressionBuilder.Constant(1);
                    if (!TypeUsageEquals(constantExpression.ResultType, returnType))
                    {
                        constantExpression = constantExpression.CastTo(returnType);
                    }
                    return constantExpression;
                }

                protected override EdmFunction FindFunction(
                    ExpressionConverter parent, MethodCallExpression call,
                    TypeUsage argumentType)
                {
                    // For most ELinq aggregates, the argument type is the return type. For "count", the
                    // argument type is always Int32, since we project a constant Int32 value in WrapCollectionOperand.
                    var intTypeUsage =
                        TypeUsage.CreateDefaultTypeUsage(EdmProviderManifest.Instance.GetPrimitiveType(PrimitiveTypeKind.Int32));
                    return base.FindFunction(parent, call, intTypeUsage);
                }
            }

            private sealed class CountTranslator : CountTranslatorBase
            {
                internal CountTranslator()
                    : base("Count", SequenceMethod.Count, SequenceMethod.CountPredicate)
                {
                }
            }

            private sealed class LongCountTranslator : CountTranslatorBase
            {
                internal LongCountTranslator()
                    : base("BigCount", SequenceMethod.LongCount, SequenceMethod.LongCountPredicate)
                {
                }
            }

            private abstract class UnarySequenceMethodTranslator : SequenceMethodTranslator
            {
                protected UnarySequenceMethodTranslator(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    if (null != call.Object)
                    {
                        // instance method
                        Debug.Assert(0 <= call.Arguments.Count);
                        var operand = parent.TranslateSet(call.Object);
                        return TranslateUnary(parent, operand, call);
                    }
                    else
                    {
                        // static extension method
                        Debug.Assert(1 <= call.Arguments.Count);
                        var operand = parent.TranslateSet(call.Arguments[0]);
                        return TranslateUnary(parent, operand, call);
                    }
                }

                protected abstract CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand, MethodCallExpression call);
            }

            private sealed class PassthroughTranslator : UnarySequenceMethodTranslator
            {
                internal PassthroughTranslator()
                    : base(SequenceMethod.AsQueryableGeneric, SequenceMethod.AsQueryable, SequenceMethod.AsEnumerable, SequenceMethod.ToList)
                {
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand, MethodCallExpression call)
                {
                    // make sure the operand has collection type to avoid treating (for instance) String as a
                    // sub-query
                    if (TypeSemantics.IsCollectionType(operand.ResultType))
                    {
                        return operand;
                    }
                    else
                    {
                        throw new NotSupportedException(
                            Strings.ELinq_UnsupportedPassthrough(
                                call.Method.Name, operand.ResultType.EdmType.Name));
                    }
                }
            }

            private sealed class OfTypeTranslator : UnarySequenceMethodTranslator
            {
                internal OfTypeTranslator()
                    : base(SequenceMethod.OfType)
                {
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand,
                    MethodCallExpression call)
                {
                    var clrType = call.Method.GetGenericArguments()[0];
                    TypeUsage modelType;

                    // If the model type does not exist in the perspective or is not either an EntityType
                    // or a ComplexType, fail - OfType() is not a valid operation on scalars,
                    // enumerations, collections, etc.
                    if (!parent.TryGetValueLayerType(clrType, out modelType)
                        ||
                        !(TypeSemantics.IsEntityType(modelType) || TypeSemantics.IsComplexType(modelType)))
                    {
                        throw new NotSupportedException(Strings.ELinq_InvalidOfTypeResult(DescribeClrType(clrType)));
                    }

                    // Create an of type expression to filter the original query to include
                    // only those results that are of the specified type.                    
                    var ofTypeExpression = parent.OfType(operand, modelType);
                    return ofTypeExpression;
                }
            }

            private sealed class DistinctTranslator : UnarySequenceMethodTranslator
            {
                internal DistinctTranslator()
                    : base(SequenceMethod.Distinct)
                {
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand,
                    MethodCallExpression call)
                {
                    return parent.Distinct(operand);
                }
            }

            private sealed class AnyTranslator : UnarySequenceMethodTranslator
            {
                internal AnyTranslator()
                    : base(SequenceMethod.Any)
                {
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand,
                    MethodCallExpression call)
                {
                    // "Any" is equivalent to "exists".
                    return operand.IsEmpty().Not();
                }
            }

            private abstract class OneLambdaTranslator : SequenceMethodTranslator
            {
                internal OneLambdaTranslator(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    CqtExpression source;
                    DbExpressionBinding sourceBinding;
                    CqtExpression lambda;
                    return Translate(parent, call, out source, out sourceBinding, out lambda);
                }

                // Helper method for tranlsation
                protected CqtExpression Translate(
                    ExpressionConverter parent, MethodCallExpression call, out CqtExpression source, out DbExpressionBinding sourceBinding,
                    out CqtExpression lambda)
                {
                    Debug.Assert(2 <= call.Arguments.Count);

                    // translate source
                    source = parent.TranslateExpression(call.Arguments[0]);

                    // translate lambda expression
                    var lambdaExpression = parent.GetLambdaExpression(call, 1);
                    lambda = parent.TranslateLambda(lambdaExpression, source, out sourceBinding);
                    return TranslateOneLambda(parent, sourceBinding, lambda);
                }

                protected abstract CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda);
            }

            private sealed class AnyPredicateTranslator : OneLambdaTranslator
            {
                internal AnyPredicateTranslator()
                    : base(SequenceMethod.AnyPredicate)
                {
                }

                protected override CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda)
                {
                    return sourceBinding.Any(lambda);
                }
            }

            private sealed class AllTranslator : OneLambdaTranslator
            {
                internal AllTranslator()
                    : base(SequenceMethod.All)
                {
                }

                protected override CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda)
                {
                    return sourceBinding.All(lambda);
                }
            }

            private sealed class WhereTranslator : OneLambdaTranslator
            {
                internal WhereTranslator()
                    : base(SequenceMethod.Where)
                {
                }

                protected override CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda)
                {
                    return parent.Filter(sourceBinding, lambda);
                }
            }

            private sealed class SelectTranslator : OneLambdaTranslator
            {
                internal SelectTranslator()
                    : base(SequenceMethod.Select)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    CqtExpression source;
                    DbExpressionBinding sourceBinding;
                    CqtExpression lambda;
                    var result = Translate(parent, call, out source, out sourceBinding, out lambda);
                    return result;
                }

                protected override CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda)
                {
                    return parent.Project(sourceBinding, lambda);
                }
            }

            private sealed class DefaultIfEmptyTranslator : SequenceMethodTranslator
            {
                internal DefaultIfEmptyTranslator()
                    : base(SequenceMethod.DefaultIfEmpty, SequenceMethod.DefaultIfEmptyValue)
                {
                }

                internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var operand = parent.TranslateSet(call.Arguments[0]);

                    // get default value (different translation for non-null defaults)
                    var defaultValue = call.Arguments.Count == 2
                                           ? parent.TranslateExpression(call.Arguments[1])
                                           : GetDefaultValue(parent, call.Type);

                    DbExpression left = DbExpressionBuilder.NewCollection(new DbExpression[] { 1 });
                    var leftBinding = left.BindAs(parent.AliasGenerator.Next());

                    // DefaultIfEmpty(value) syntax we may require a sentinel flag to indicate default value substitution
                    var requireSentinel = !(null == defaultValue || defaultValue.ExpressionKind == DbExpressionKind.Null);
                    if (requireSentinel)
                    {
                        var o = operand.BindAs(parent.AliasGenerator.Next());
                        operand = o.Project(new Row(((DbExpression)1).As("sentinel"), o.Variable.As("value")));
                    }

                    var rightBinding = operand.BindAs(parent.AliasGenerator.Next());
                    DbExpression join = leftBinding.LeftOuterJoin(rightBinding, true);
                    var joinBinding = join.BindAs(parent.AliasGenerator.Next());
                    DbExpression projection = joinBinding.Variable.Property(rightBinding.VariableName);

                    // Use a case statement on the sentinel flag to drop the default value in where required 
                    if (requireSentinel)
                    {
                        projection = DbExpressionBuilder.Case(
                            new[] { projection.Property("sentinel").IsNull() }, new[] { defaultValue }, projection.Property("value"));
                    }

                    DbExpression spannedProjection = joinBinding.Project(projection);
                    parent.ApplySpanMapping(operand, spannedProjection);
                    return spannedProjection;
                }

                private static DbExpression GetDefaultValue(ExpressionConverter parent, Type resultType)
                {
                    var elementType = TypeSystem.GetElementType(resultType);
                    var defaultValue = TypeSystem.GetDefaultValue(elementType);
                    var result = null == defaultValue
                                     ? null
                                     : parent.TranslateExpression(Expression.Constant(defaultValue, elementType));
                    return result;
                }
            }

            private sealed class ContainsTranslator : SequenceMethodTranslator
            {
                internal ContainsTranslator()
                    : base(SequenceMethod.Contains)
                {
                }

                internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    return TranslateContains(parent, call.Arguments[0], call.Arguments[1]);
                }

                private static DbExpression TranslateContainsHelper(
                    ExpressionConverter parent, CqtExpression left, IEnumerable<DbExpression> rightList, EqualsPattern pattern,
                    Type leftType, Type rightType)
                {
                    var predicates = rightList.
                        Select(argument => parent.CreateEqualsExpression(left, argument, pattern, leftType, rightType));
                    var expressions = new List<DbExpression>(predicates);
                    var cqt = Helpers.BuildBalancedTreeInPlace(
                        expressions,
                        (prev, next) => prev.Or(next)
                        );
                    return cqt;
                }

                internal static DbExpression TranslateContains(
                    ExpressionConverter parent, Expression sourceExpression, Expression valueExpression)
                {
                    var source = parent.NormalizeSetSource(parent.TranslateExpression(sourceExpression));
                    var value = parent.TranslateExpression(valueExpression);
                    var sourceArgumentType = TypeSystem.GetElementType(sourceExpression.Type);

                    if (source.ExpressionKind
                        == DbExpressionKind.NewInstance)
                    {
                        var arguments = ((DbNewInstanceExpression)source).Arguments;
                        if (arguments.Count > 0)
                        {
                            var useCSharpNullComparisonBehavior =
                                parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior;
                            var providerSupportsInExpression = parent.ProviderManifest.SupportsInExpression();

                            if (!useCSharpNullComparisonBehavior
                                && !providerSupportsInExpression)
                            {
                                return TranslateContainsHelper(
                                    parent, value, arguments, EqualsPattern.Store, sourceArgumentType, valueExpression.Type);
                            }

                            // Replaces this => (tbl.Col = 1 AND tbl.Col IS NOT NULL) OR (tbl.Col = 2 AND tbl.Col IS NOT NULL) OR ... 
                            // with this => (tbl.Col = 1 OR tbl.Col = 2 OR ...) AND (tbl.Col IS NOT NULL))
                            // which in turn gets simplified to this => (tbl.Col IN (1, 2, ...) AND (tbl.Col IS NOT NULL)) in SqlGenerator

                            var constantArguments = new List<DbExpression>();
                            var otherArguments = new List<DbExpression>();
                            foreach (var arg in arguments)
                            {
                                var list = (arg.ExpressionKind == DbExpressionKind.Constant) ? constantArguments : otherArguments;
                                list.Add(arg);
                            }

                            CqtExpression constantCqt = null;
                            if (constantArguments.Count > 0)
                            {
                                var equalsPattern = useCSharpNullComparisonBehavior
                                                        ? EqualsPattern.PositiveNullEqualityNonComposable
                                                        : EqualsPattern.Store;

                                constantCqt = providerSupportsInExpression
                                                  ? DbExpressionBuilder.CreateInExpression(value, constantArguments)
                                                  : TranslateContainsHelper(
                                                      parent, value, constantArguments, equalsPattern, sourceArgumentType,
                                                      valueExpression.Type);

                                if (useCSharpNullComparisonBehavior)
                                {
                                    constantCqt = constantCqt.And(value.IsNull().Not());
                                }
                            }

                            // Does not optimize conversion of variables embedded in the list.
                            CqtExpression otherCqt = null;
                            if (otherArguments.Count > 0)
                            {
                                var equalsPattern = useCSharpNullComparisonBehavior
                                                        ? EqualsPattern.PositiveNullEqualityComposable
                                                        : EqualsPattern.Store;

                                otherCqt = TranslateContainsHelper(
                                    parent, value, otherArguments, equalsPattern, sourceArgumentType, valueExpression.Type);
                            }

                            if (constantCqt == null)
                            {
                                return otherCqt;
                            }
                            if (otherCqt == null)
                            {
                                return constantCqt;
                            }
                            return constantCqt.Or(otherCqt);
                        }
                        return false;
                    }

                    var sourceBinding = source.BindAs(parent.AliasGenerator.Next());
                    var pattern = EqualsPattern.Store;
                    if (parent._funcletizer.RootContext.ContextOptions.UseCSharpNullComparisonBehavior)
                    {
                        pattern = EqualsPattern.PositiveNullEqualityComposable;
                    }
                    return
                        sourceBinding.Filter(
                            parent.CreateEqualsExpression(sourceBinding.Variable, value, pattern, sourceArgumentType, valueExpression.Type))
                                     .Exists();
                }
            }

            private abstract class FirstTranslatorBase : UnarySequenceMethodTranslator
            {
                protected FirstTranslatorBase(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                protected virtual CqtExpression LimitResult(ExpressionConverter parent, CqtExpression expression)
                {
                    // Only need the first result.
                    return parent.Limit(expression, DbExpressionBuilder.Constant(1));
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand, MethodCallExpression call)
                {
                    var result = LimitResult(parent, operand);

                    // If this FirstOrDefault/SingleOrDefault() operation is the root of the query,
                    // then the evaluation is performed in the client over the resulting set,
                    // to provide the same semantics as Linq to Objects. Otherwise, an Element
                    // expression is applied to retrieve the single element (or null, if empty)
                    // from the output set.
                    if (!parent.IsQueryRoot(call))
                    {
                        result = result.Element();
                        result = AddDefaultCase(result, call.Type);
                    }

                    // Span is preserved over First/FirstOrDefault with or without a predicate
                    Span inputSpan = null;
                    if (parent.TryGetSpan(operand, out inputSpan))
                    {
                        parent.AddSpanMapping(result, inputSpan);
                    }

                    return result;
                }

                internal static CqtExpression AddDefaultCase(CqtExpression element, Type elementType)
                {
                    // Retrieve default value.
                    var defaultValue = TypeSystem.GetDefaultValue(elementType);
                    if (null == defaultValue)
                    {
                        // Already null, which is the implicit default for DbElementExpression
                        return element;
                    }

                    Debug.Assert(TypeSemantics.IsScalarType(element.ResultType), "Primitive or enum type expected at this point.");

                    // Otherwise, use the default value for the type
                    var whenExpressions = new List<CqtExpression>(1);

                    whenExpressions.Add(CreateIsNullExpression(element, elementType));
                    var thenExpressions = new List<CqtExpression>(1);
                    thenExpressions.Add(element.ResultType.Constant(defaultValue));
                    var caseExpression = DbExpressionBuilder.Case(whenExpressions, thenExpressions, element);
                    return caseExpression;
                }
            }

            private sealed class FirstTranslator : FirstTranslatorBase
            {
                internal FirstTranslator()
                    : base(SequenceMethod.First)
                {
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand, MethodCallExpression call)
                {
                    if (!parent.IsQueryRoot(call))
                    {
                        throw new NotSupportedException(Strings.ELinq_UnsupportedNestedFirst);
                    }
                    return base.TranslateUnary(parent, operand, call);
                }
            }

            private sealed class FirstOrDefaultTranslator : FirstTranslatorBase
            {
                internal FirstOrDefaultTranslator()
                    : base(SequenceMethod.FirstOrDefault)
                {
                }
            }

            private abstract class SingleTranslatorBase : FirstTranslatorBase
            {
                protected SingleTranslatorBase(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                protected override CqtExpression TranslateUnary(
                    ExpressionConverter parent, CqtExpression operand, MethodCallExpression call)
                {
                    if (!parent.IsQueryRoot(call))
                    {
                        throw new NotSupportedException(Strings.ELinq_UnsupportedNestedSingle);
                    }
                    return base.TranslateUnary(parent, operand, call);
                }

                protected override CqtExpression LimitResult(ExpressionConverter parent, CqtExpression expression)
                {
                    // Only need two results - one to return as the actual result and another so we can throw if there is more than one
                    return parent.Limit(expression, DbExpressionBuilder.Constant(2));
                }
            }

            private sealed class SingleTranslator : SingleTranslatorBase
            {
                internal SingleTranslator()
                    : base(SequenceMethod.Single)
                {
                }
            }

            private sealed class SingleOrDefaultTranslator : SingleTranslatorBase
            {
                internal SingleOrDefaultTranslator()
                    : base(SequenceMethod.SingleOrDefault)
                {
                }
            }

            private abstract class FirstPredicateTranslatorBase : OneLambdaTranslator
            {
                protected FirstPredicateTranslatorBase(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                protected virtual CqtExpression RestrictResult(ExpressionConverter parent, CqtExpression expression)
                {
                    // Only need the first result.
                    return parent.Limit(expression, DbExpressionBuilder.Constant(1));
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    // Convert the input set and the predicate into a filter expression
                    var input = base.Translate(parent, call);

                    // If this First/FirstOrDefault/Single/SingleOrDefault is the root of the query,
                    // then the actual result will be produced by evaluated by
                    // calling First/Single() or FirstOrDefault() on the filtered input set,
                    // which is limited to at most one element by applying a limit.
                    if (parent.IsQueryRoot(call))
                    {
                        // Calling ExpressionConverter.Limit propagates the Span.
                        return RestrictResult(parent, input);
                    }
                    else
                    {
                        input = RestrictResult(parent, input);

                        CqtExpression element = input.Element();
                        element = FirstTranslatorBase.AddDefaultCase(element, call.Type);

                        // Span is preserved over First/FirstOrDefault with or without a predicate
                        Span inputSpan = null;
                        if (parent.TryGetSpan(input, out inputSpan))
                        {
                            parent.AddSpanMapping(element, inputSpan);
                        }

                        return element;
                    }
                }

                protected override CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda)
                {
                    return parent.Filter(sourceBinding, lambda);
                }
            }

            private sealed class FirstPredicateTranslator : FirstPredicateTranslatorBase
            {
                internal FirstPredicateTranslator()
                    : base(SequenceMethod.FirstPredicate)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    if (!parent.IsQueryRoot(call))
                    {
                        throw new NotSupportedException(Strings.ELinq_UnsupportedNestedFirst);
                    }
                    return base.Translate(parent, call);
                }
            }

            private sealed class FirstOrDefaultPredicateTranslator : FirstPredicateTranslatorBase
            {
                internal FirstOrDefaultPredicateTranslator()
                    : base(SequenceMethod.FirstOrDefaultPredicate)
                {
                }
            }

            private abstract class SinglePredicateTranslatorBase : FirstPredicateTranslatorBase
            {
                protected SinglePredicateTranslatorBase(params SequenceMethod[] methods)
                    : base(methods)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    if (!parent.IsQueryRoot(call))
                    {
                        throw new NotSupportedException(Strings.ELinq_UnsupportedNestedSingle);
                    }
                    return base.Translate(parent, call);
                }

                protected override CqtExpression RestrictResult(ExpressionConverter parent, CqtExpression expression)
                {
                    // Only need two results - one to return and another to see if it wasn't alone to throw.
                    return parent.Limit(expression, DbExpressionBuilder.Constant(2));
                }
            }

            private sealed class SinglePredicateTranslator : SinglePredicateTranslatorBase
            {
                internal SinglePredicateTranslator()
                    : base(SequenceMethod.SinglePredicate)
                {
                }
            }

            private sealed class SingleOrDefaultPredicateTranslator : SinglePredicateTranslatorBase
            {
                internal SingleOrDefaultPredicateTranslator()
                    : base(SequenceMethod.SingleOrDefaultPredicate)
                {
                }
            }

            private sealed class SelectManyTranslator : OneLambdaTranslator
            {
                internal SelectManyTranslator()
                    : base(SequenceMethod.SelectMany, SequenceMethod.SelectManyResultSelector)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    // perform a cross apply to implement the core logic for SelectMany (this translates the collection selector):
                    // SelectMany(i, Func<i, IEnum<o>> collectionSelector) =>
                    // i CROSS APPLY collectionSelector(i)
                    // The cross-apply yields a collection <left, right> from which we yield either the right hand side (when
                    // no explicit resultSelector is given) or over which we apply the resultSelector Lambda expression.

                    var resultSelector = (call.Arguments.Count == 3) ? parent.GetLambdaExpression(call, 2) : null;

                    var apply = base.Translate(parent, call);

                    // try detecting the linq pattern for a left outer join and produce a simpler c-tree for it.
                    DbExpressionBinding applyInput;
                    EdmProperty lojRightInput;
                    var isLeftOuterJoin = IsLeftOuterJoin(apply, out applyInput, out lojRightInput);
                    if (isLeftOuterJoin)
                    {
                        // 1)
                        // if apply looks like a cross apply with right input being a loj of {1} to a collection from the apply's left input:
                        //     (
                        //         select o, (select ...) as lojRightInput
                        //         from (...) as o
                        //     ) as x
                        //     CROSS apply
                        //     (
                        //         select loj
                        //         from {1} left outer join x.lojRightInput as loj on true
                        //     ) as y
                        // then rewrite it as outer apply
                        //     (
                        //         select o, (select ...) as lojRightInput
                        //         from (...) as o
                        //     ) as x
                        //     OUTER apply
                        //          x.lojRightInput as loj
                        //
                        // 2)
                        // if there is a trivial resultSelector that would produce something like this:
                        //     select x as m, loj as n
                        //     from (...) as x outer apply (...) as loj
                        // then rewrite it as
                        //     (...) as m outer apply (...) as n
                        string outerBindingName;
                        string innerBindingName;
                        InitializerMetadata initializerMetadata;
                        if (resultSelector != null
                            && IsTrivialRename(resultSelector, parent, out outerBindingName, out innerBindingName, out initializerMetadata))
                        {
                            // It is #1 and #2 as described above:
                            //  - produce the outer apply
                            //  - name inputs as specified in the resultSelector
                            //  - return the apply.
                            var newInput = applyInput.Expression.BindAs(outerBindingName);
                            var newApply = newInput.Variable.Property(lojRightInput.Name).BindAs(innerBindingName);

                            var resultType = TypeUsage.Create(
                                TypeHelpers.CreateRowType(
                                    new List<KeyValuePair<string, TypeUsage>>
                                        {
                                            new KeyValuePair<string, TypeUsage>(newInput.VariableName, newInput.VariableType),
                                            new KeyValuePair<string, TypeUsage>(newApply.VariableName, newApply.VariableType)
                                        },
                                    initializerMetadata));

                            return new DbApplyExpression(
                                DbExpressionKind.OuterApply, TypeUsage.Create(TypeHelpers.CreateCollectionType(resultType)), newInput,
                                newApply);
                        }
                        else
                        {
                            // It is just #1 as described above,
                            // so produce the outer apply and let the logic below generate projection using the resultSelector.
                            apply = applyInput.OuterApply(applyInput.Variable.Property(lojRightInput).BindAs(parent.AliasGenerator.Next()));
                        }
                    }

                    var applyBinding = apply.BindAs(parent.AliasGenerator.Next());
                    var applyRowType = (RowType)(applyBinding.Variable.ResultType.EdmType);
                    CqtExpression projectRight = applyBinding.Variable.Property(applyRowType.Properties[1]);

                    CqtExpression resultProjection;
                    if (resultSelector != null)
                    {
                        CqtExpression projectLeft = applyBinding.Variable.Property(applyRowType.Properties[0]);

                        // add the left and right projection terms to the binding context
                        parent._bindingContext.PushBindingScope(new Binding(resultSelector.Parameters[0], projectLeft));
                        parent._bindingContext.PushBindingScope(new Binding(resultSelector.Parameters[1], projectRight));

                        // translate the result selector
                        resultProjection = parent.TranslateSet(resultSelector.Body);

                        // pop binding context
                        parent._bindingContext.PopBindingScope();
                        parent._bindingContext.PopBindingScope();
                    }
                    else
                    {
                        // project out the right hand side of the apply
                        resultProjection = projectRight;
                    }

                    // wrap result projection in project expression
                    return applyBinding.Project(resultProjection);
                }

                private static bool IsLeftOuterJoin(
                    CqtExpression cqtExpression, out DbExpressionBinding crossApplyInput, out EdmProperty lojRightInput)
                {
                    // Check cqtExpression to see if looks like this:
                    //
                    //     (
                    //         select o, (select ...) as lojRightInput
                    //         from (...) as o
                    //     ) as x
                    //     cross apply
                    //     (
                    //         select loj
                    //         from {1} left outer join x.lojRightInput as loj on true
                    //     ) as y
                    //
                    // If yes - return true, 
                    // crossApplyInput = (
                    //                      select o, (select ...) as lojRightInput
                    //                      from (...) as o
                    //                   ) as x
                    // lojRightInput = x.lojRightInput

                    crossApplyInput = null;
                    lojRightInput = null;

                    if (cqtExpression.ExpressionKind
                        != DbExpressionKind.CrossApply)
                    {
                        return false;
                    }
                    var crossApply = (DbApplyExpression)cqtExpression;

                    if (crossApply.Input.VariableType.EdmType.BuiltInTypeKind
                        != BuiltInTypeKind.RowType)
                    {
                        return false;
                    }
                    var crossApplyInputRowType = (RowType)crossApply.Input.VariableType.EdmType;

                    // rightProject = (select loj
                    //                 from {1} left outer join x.lojRightInput as loj on true)
                    if (crossApply.Apply.Expression.ExpressionKind
                        != DbExpressionKind.Project)
                    {
                        return false;
                    }
                    var rightProject = (DbProjectExpression)crossApply.Apply.Expression;

                    // loj = {1} left outer join x.lojRightInput as loj on true
                    if (rightProject.Input.Expression.ExpressionKind
                        != DbExpressionKind.LeftOuterJoin)
                    {
                        return false;
                    }
                    var loj = (DbJoinExpression)rightProject.Input.Expression;

                    if (rightProject.Projection.ExpressionKind
                        != DbExpressionKind.Property)
                    {
                        return false;
                    }
                    var rightProjectProjection = (DbPropertyExpression)rightProject.Projection;

                    // make sure that in 
                    //    rightProject = (select loj
                    //                    from {1} left outer join x.lojRightInput as loj on true)
                    // loj comes from the right side of the left outer join.
                    if (rightProjectProjection.Instance != rightProject.Input.Variable
                        ||
                        rightProjectProjection.Property.Name != loj.Right.VariableName
                        ||
                        loj.JoinCondition.ExpressionKind != DbExpressionKind.Constant)
                    {
                        return false;
                    }
                    var lojCondition = (DbConstantExpression)loj.JoinCondition;

                    // make sure that in 
                    //    rightProject = (select loj
                    //                    from {1} left outer join x.lojRightInput as loj on true)
                    // the left outer join condition is "true".
                    if (!(lojCondition.Value is bool)
                        || (bool)lojCondition.Value != true)
                    {
                        return false;
                    }

                    // make sure that in 
                    //    rightProject = (select loj
                    //                    from {1} left outer join x.lojRightInput as loj on true)
                    // the left input into the left outer join condition is a single-element collection "{some constant}"
                    if (loj.Left.Expression.ExpressionKind
                        != DbExpressionKind.NewInstance)
                    {
                        return false;
                    }
                    var lojLeft = (DbNewInstanceExpression)loj.Left.Expression;
                    if (lojLeft.Arguments.Count != 1
                        || lojLeft.Arguments[0].ExpressionKind != DbExpressionKind.Constant)
                    {
                        return false;
                    }

                    // make sure that in 
                    //    rightProject = (select loj
                    //                    from {1} left outer join x.lojRightInput as loj on true)
                    // the x.lojRightInput comes from the left side of the cross apply
                    if (loj.Right.Expression.ExpressionKind
                        != DbExpressionKind.Property)
                    {
                        return false;
                    }
                    var lojRight = (DbPropertyExpression)loj.Right.Expression;
                    if (lojRight.Instance
                        != crossApply.Input.Variable)
                    {
                        return false;
                    }
                    var lojRightValueSource = crossApplyInputRowType.Properties.SingleOrDefault(p => p.Name == lojRight.Property.Name);
                    if (lojRightValueSource == null)
                    {
                        return false;
                    }

                    crossApplyInput = crossApply.Input;
                    lojRightInput = lojRightValueSource;

                    return true;
                }

                protected override CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda)
                {
                    // elements of the inner selector should be used
                    lambda = parent.NormalizeSetSource(lambda);
                    var applyBinding = lambda.BindAs(parent.AliasGenerator.Next());
                    var crossApply = sourceBinding.CrossApply(applyBinding);
                    return crossApply;
                }
            }

            private sealed class CastMethodTranslator : SequenceMethodTranslator
            {
                internal CastMethodTranslator()
                    : base(SequenceMethod.Cast)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    // Translate source
                    var source = parent.TranslateSet(call.Arguments[0]);

                    // Figure out the type to cast to
                    var toClrType = TypeSystem.GetElementType(call.Type);
                    var fromClrType = TypeSystem.GetElementType(call.Arguments[0].Type);

                    // Get binding to the elements of the input source
                    var binding = source.BindAs(parent.AliasGenerator.Next());

                    var cast = parent.CreateCastExpression(binding.Variable, toClrType, fromClrType);
                    return parent.Project(binding, cast);
                }
            }

            private sealed class GroupByTranslator : SequenceMethodTranslator
            {
                internal GroupByTranslator()
                    : base(
                        SequenceMethod.GroupBy, SequenceMethod.GroupByElementSelector, SequenceMethod.GroupByElementSelectorResultSelector,
                        SequenceMethod.GroupByResultSelector)
                {
                }

                // Creates a Cqt GroupByExpression with a group aggregate
                internal override CqtExpression Translate(
                    ExpressionConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod)
                {
                    // translate source
                    var source = parent.TranslateSet(call.Arguments[0]);

                    // translate key selector
                    var keySelectorLinq = parent.GetLambdaExpression(call, 1);
                    DbGroupExpressionBinding sourceGroupBinding;
                    var keySelector = parent.TranslateLambda(keySelectorLinq, source, out sourceGroupBinding);

                    // create distinct expression
                    if (!TypeSemantics.IsEqualComparable(keySelector.ResultType))
                    {
                        // to avoid confusing error message about the "distinct" type, pre-emptively raise an exception
                        // about the group by key selector
                        throw new NotSupportedException(Strings.ELinq_UnsupportedKeySelector(call.Method.Name));
                    }

                    var keys = new List<KeyValuePair<string, DbExpression>>();
                    var aggregates = new List<KeyValuePair<string, DbAggregate>>();
                    keys.Add(new KeyValuePair<string, CqtExpression>(KeyColumnName, keySelector));
                    aggregates.Add(new KeyValuePair<string, DbAggregate>(GroupColumnName, sourceGroupBinding.GroupAggregate));

                    DbExpression groupBy = sourceGroupBinding.GroupBy(keys, aggregates);
                    var groupByBinding = groupBy.BindAs(parent.AliasGenerator.Next());

                    // interpret element selector if needed
                    CqtExpression selection = groupByBinding.Variable.Property(GroupColumnName);

                    var hasElementSelector = sequenceMethod == SequenceMethod.GroupByElementSelector ||
                                             sequenceMethod == SequenceMethod.GroupByElementSelectorResultSelector;

                    //Create a project over the group by
                    if (hasElementSelector)
                    {
                        var elementSelectorLinq = parent.GetLambdaExpression(call, 2);
                        DbExpressionBinding elementSelectorSourceBinding;
                        var elementSelector = parent.TranslateLambda(elementSelectorLinq, selection, out elementSelectorSourceBinding);
                        selection = elementSelectorSourceBinding.Project(elementSelector);
                    }

                    // create top level projection <exists, key, group>
                    var projectionTerms = new CqtExpression[2];
                    projectionTerms[0] = groupByBinding.Variable.Property(KeyColumnName);
                    projectionTerms[1] = selection;

                    // build projection type with initializer information
                    var properties = new List<EdmProperty>(2);
                    properties.Add(new EdmProperty(KeyColumnName, projectionTerms[0].ResultType));
                    properties.Add(new EdmProperty(GroupColumnName, projectionTerms[1].ResultType));
                    var initializerMetadata = InitializerMetadata.CreateGroupingInitializer(
                        parent.EdmItemCollection, TypeSystem.GetElementType(call.Type));
                    var rowType = new RowType(properties, initializerMetadata);
                    var rowTypeUsage = TypeUsage.Create(rowType);

                    CqtExpression topLevelProject = groupByBinding.Project(rowTypeUsage.New(projectionTerms));

                    var result = topLevelProject;

                    // GroupBy may include a result selector; handle it
                    result = ProcessResultSelector(parent, call, sequenceMethod, topLevelProject, result);

                    return result;
                }

                private static DbExpression ProcessResultSelector(
                    ExpressionConverter parent, MethodCallExpression call, SequenceMethod sequenceMethod, CqtExpression topLevelProject,
                    DbExpression result)
                {
                    // interpret result selector if needed
                    LambdaExpression resultSelectorLinqExpression = null;
                    if (sequenceMethod == SequenceMethod.GroupByResultSelector)
                    {
                        resultSelectorLinqExpression = parent.GetLambdaExpression(call, 2);
                    }
                    else if (sequenceMethod == SequenceMethod.GroupByElementSelectorResultSelector)
                    {
                        resultSelectorLinqExpression = parent.GetLambdaExpression(call, 3);
                    }
                    if (null != resultSelectorLinqExpression)
                    {
                        // selector maps (Key, Group) -> Result
                        // push bindings for key and group
                        var topLevelProjectBinding = topLevelProject.BindAs(parent.AliasGenerator.Next());
                        var keyExpression = topLevelProjectBinding.Variable.Property(KeyColumnName);
                        var groupExpression = topLevelProjectBinding.Variable.Property(GroupColumnName);
                        parent._bindingContext.PushBindingScope(new Binding(resultSelectorLinqExpression.Parameters[0], keyExpression));
                        parent._bindingContext.PushBindingScope(new Binding(resultSelectorLinqExpression.Parameters[1], groupExpression));

                        // translate selector
                        var resultSelector = parent.TranslateExpression(
                            resultSelectorLinqExpression.Body);
                        result = topLevelProjectBinding.Project(resultSelector);

                        parent._bindingContext.PopBindingScope();
                        parent._bindingContext.PopBindingScope();
                    }
                    return result;
                }

                internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Fail("unreachable code");
                    return null;
                }
            }

            private sealed class GroupJoinTranslator : SequenceMethodTranslator
            {
                internal GroupJoinTranslator()
                    : base(SequenceMethod.GroupJoin)
                {
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    // o.GroupJoin(i, ok => outerKeySelector, ik => innerKeySelector, (o, i) => projection)
                    //      -->
                    // SELECT projection(o, i)
                    // FROM (
                    //      SELECT o, (SELECT i FROM i WHERE o.outerKeySelector = i.innerKeySelector) as i
                    //      FROM o)

                    // translate inputs
                    var outer = parent.TranslateSet(call.Arguments[0]);
                    var inner = parent.TranslateSet(call.Arguments[1]);

                    // translate key selectors
                    DbExpressionBinding outerBinding;
                    DbExpressionBinding innerBinding;
                    var outerLambda = parent.GetLambdaExpression(call, 2);
                    var innerLambda = parent.GetLambdaExpression(call, 3);
                    var outerSelector = parent.TranslateLambda(
                        outerLambda, outer, out outerBinding);
                    var innerSelector = parent.TranslateLambda(
                        innerLambda, inner, out innerBinding);

                    // create innermost SELECT i FROM i WHERE ...
                    if (!TypeSemantics.IsEqualComparable(outerSelector.ResultType)
                        ||
                        !TypeSemantics.IsEqualComparable(innerSelector.ResultType))
                    {
                        throw new NotSupportedException(Strings.ELinq_UnsupportedKeySelector(call.Method.Name));
                    }
                    var nestedCollection = parent.Filter(
                        innerBinding,
                        parent.CreateEqualsExpression(
                            outerSelector, innerSelector, EqualsPattern.PositiveNullEqualityNonComposable, outerLambda.Body.Type,
                            innerLambda.Body.Type));

                    // create "join" SELECT o, (nestedCollection)
                    const string outerColumn = "o";
                    const string innerColumn = "i";
                    var recordColumns = new List<KeyValuePair<string, CqtExpression>>(2);
                    recordColumns.Add(new KeyValuePair<string, CqtExpression>(outerColumn, outerBinding.Variable));
                    recordColumns.Add(new KeyValuePair<string, CqtExpression>(innerColumn, nestedCollection));
                    CqtExpression joinProjection = DbExpressionBuilder.NewRow(recordColumns);
                    CqtExpression joinProject = outerBinding.Project(joinProjection);
                    var joinProjectBinding = joinProject.BindAs(parent.AliasGenerator.Next());

                    // create property expressions for the outer and inner terms to bind to the parameters to the
                    // group join selector
                    CqtExpression outerProperty = joinProjectBinding.Variable.Property(outerColumn);
                    CqtExpression innerProperty = joinProjectBinding.Variable.Property(innerColumn);

                    // push the inner and the outer terms into the binding scope
                    var linqSelector = parent.GetLambdaExpression(call, 4);
                    parent._bindingContext.PushBindingScope(new Binding(linqSelector.Parameters[0], outerProperty));
                    parent._bindingContext.PushBindingScope(new Binding(linqSelector.Parameters[1], innerProperty));

                    // translate the selector
                    var selectorProject = parent.TranslateExpression(linqSelector.Body);

                    // pop the binding scope
                    parent._bindingContext.PopBindingScope();
                    parent._bindingContext.PopBindingScope();

                    // create the selector projection
                    CqtExpression selector = joinProjectBinding.Project(selectorProject);

                    selector = CollapseTrivialRenamingProjection(selector);

                    return selector;
                }

                private static CqtExpression CollapseTrivialRenamingProjection(CqtExpression cqtExpression)
                {
                    // Detect "select inner.x as m, inner.y as n
                    //         from (select ... as x, ... as y from ...) as inner"
                    // and convert to "select ... as m, ... as n from ..."

                    if (cqtExpression.ExpressionKind
                        != DbExpressionKind.Project)
                    {
                        return cqtExpression;
                    }
                    var project = (DbProjectExpression)cqtExpression;

                    if (project.Projection.ExpressionKind != DbExpressionKind.NewInstance
                        ||
                        project.Projection.ResultType.EdmType.BuiltInTypeKind != BuiltInTypeKind.RowType)
                    {
                        return cqtExpression;
                    }
                    var projection = (DbNewInstanceExpression)project.Projection;
                    var outerRowType = (RowType)projection.ResultType.EdmType;

                    var renames = new List<Tuple<EdmProperty, string>>();
                    for (var i = 0; i < projection.Arguments.Count; ++i)
                    {
                        if (projection.Arguments[i].ExpressionKind
                            != DbExpressionKind.Property)
                        {
                            return cqtExpression;
                        }
                        var rename = (DbPropertyExpression)projection.Arguments[i];

                        if (rename.Instance
                            != project.Input.Variable)
                        {
                            return cqtExpression;
                        }
                        renames.Add(Tuple.Create((EdmProperty)rename.Property, outerRowType.Properties[i].Name));
                    }

                    if (project.Input.Expression.ExpressionKind
                        != DbExpressionKind.Project)
                    {
                        return cqtExpression;
                    }
                    var innerProject = (DbProjectExpression)project.Input.Expression;

                    if (innerProject.Projection.ExpressionKind != DbExpressionKind.NewInstance
                        ||
                        innerProject.Projection.ResultType.EdmType.BuiltInTypeKind != BuiltInTypeKind.RowType)
                    {
                        return cqtExpression;
                    }
                    var innerProjection = (DbNewInstanceExpression)innerProject.Projection;
                    var innerRowType = (RowType)innerProjection.ResultType.EdmType;

                    var newProjectionArguments = new List<CqtExpression>();
                    foreach (var rename in renames)
                    {
                        var innerPropertyIndex = innerRowType.Properties.IndexOf(rename.Item1);
                        newProjectionArguments.Add(innerProjection.Arguments[innerPropertyIndex]);
                    }

                    var newProjection = projection.ResultType.New(newProjectionArguments);
                    return innerProject.Input.Project(newProjection);
                }
            }

            private abstract class OrderByTranslatorBase : OneLambdaTranslator
            {
                private readonly bool _ascending;

                protected OrderByTranslatorBase(bool ascending, params SequenceMethod[] methods)
                    : base(methods)
                {
                    _ascending = ascending;
                }

                protected override CqtExpression TranslateOneLambda(
                    ExpressionConverter parent, DbExpressionBinding sourceBinding, CqtExpression lambda)
                {
                    var keys = new List<DbSortClause>(1);
                    var sortSpec = (_ascending ? lambda.ToSortClause() : lambda.ToSortClauseDescending());
                    keys.Add(sortSpec);
                    var sort = parent.Sort(sourceBinding, keys);
                    return sort;
                }
            }

            private sealed class OrderByTranslator : OrderByTranslatorBase
            {
                internal OrderByTranslator()
                    : base(true, SequenceMethod.OrderBy)
                {
                }
            }

            private sealed class OrderByDescendingTranslator : OrderByTranslatorBase
            {
                internal OrderByDescendingTranslator()
                    : base(false, SequenceMethod.OrderByDescending)
                {
                }
            }

            // Note: because we need to "push-down" the expression binding for ThenBy, this class
            // does not inherit from OneLambdaTranslator, although it is similar.
            private abstract class ThenByTranslatorBase : SequenceMethodTranslator
            {
                private readonly bool _ascending;

                protected ThenByTranslatorBase(bool ascending, params SequenceMethod[] methods)
                    : base(methods)
                {
                    _ascending = ascending;
                }

                internal override CqtExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    Debug.Assert(2 == call.Arguments.Count);
                    var source = parent.TranslateSet(call.Arguments[0]);
                    if (DbExpressionKind.Sort
                        != source.ExpressionKind)
                    {
                        throw new InvalidOperationException(Strings.ELinq_ThenByDoesNotFollowOrderBy);
                    }
                    var sortExpression = (DbSortExpression)source;

                    // retrieve information about existing sort
                    var binding = sortExpression.Input;

                    // get information on new sort term
                    var lambdaExpression = parent.GetLambdaExpression(call, 1);
                    var parameter = lambdaExpression.Parameters[0];

                    // push-down the binding scope information and translate the new sort key
                    parent._bindingContext.PushBindingScope(new Binding(parameter, binding.Variable));
                    var lambda = parent.TranslateExpression(lambdaExpression.Body);
                    parent._bindingContext.PopBindingScope();

                    // create a new sort expression
                    var keys = new List<DbSortClause>(sortExpression.SortOrder);
                    keys.Add(new DbSortClause(lambda, _ascending, null));
                    sortExpression = parent.Sort(binding, keys);

                    return sortExpression;
                }
            }

            private sealed class ThenByTranslator : ThenByTranslatorBase
            {
                internal ThenByTranslator()
                    : base(true, SequenceMethod.ThenBy)
                {
                }
            }

            private sealed class ThenByDescendingTranslator : ThenByTranslatorBase
            {
                internal ThenByDescendingTranslator()
                    : base(false, SequenceMethod.ThenByDescending)
                {
                }
            }

            #endregion
        }
    }
}
