// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Hierarchy;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed partial class ExpressionConverter
    {
        internal sealed partial class MethodCallTranslator
            : TypedTranslator<MethodCallExpression>
        {
            internal sealed class HierarchyIdMethodCallTranslator : CallTranslator
            {
                private static readonly Dictionary<MethodInfo, string> _methodFunctionRenames = GetRenamedMethodFunctions();

                internal HierarchyIdMethodCallTranslator()
                    : base(GetSupportedMethods())
                {
                }

                private static MethodInfo GetStaticMethod<TResult>(Expression<Func<TResult>> lambda)
                {
                    var method = ((MethodCallExpression)lambda.Body).Method;
                    Debug.Assert(
                        method.IsStatic && method.IsPublic &&
                        method.DeclaringType == typeof(HierarchyId),
                        "Supported static hierarchyid methods should be public static methods declared by a hierarchyid type");
                    return method;
                }

                private static MethodInfo GetInstanceMethod<T, TResult>(Expression<Func<T, TResult>> lambda)
                {
                    var method = ((MethodCallExpression)lambda.Body).Method;
                    Debug.Assert(
                        !method.IsStatic && method.IsPublic &&
                        method.DeclaringType == typeof(HierarchyId),
                        "Supported instance hierarchyid methods should be public instance methods declared by a hierarchyid type");
                    return method;
                }

                private static IEnumerable<MethodInfo> GetSupportedMethods()
                {
                    yield return GetStaticMethod(() => HierarchyId.GetRoot());
                    yield return GetStaticMethod(() => HierarchyId.Parse(default(string)));
                    yield return GetInstanceMethod((HierarchyId h) => h.GetAncestor(default(int)));
                    yield return GetInstanceMethod((HierarchyId h) => h.GetDescendant(default(HierarchyId), default(HierarchyId)));
                    yield return GetInstanceMethod((HierarchyId h) => h.GetLevel());
                    yield return GetInstanceMethod((HierarchyId h) => h.IsDescendantOf(default(HierarchyId)));
                    yield return GetInstanceMethod((HierarchyId h) => h.GetReparentedValue(default(HierarchyId), default(HierarchyId)));
                }

                private static Dictionary<MethodInfo, string> GetRenamedMethodFunctions()
                {
                    var result = new Dictionary<MethodInfo, string>();
                    result.Add(GetStaticMethod(() => HierarchyId.GetRoot()), "HierarchyIdGetRoot");
                    result.Add(GetStaticMethod(() => HierarchyId.Parse(default(string))), "HierarchyIdParse");
                    result.Add(GetInstanceMethod((HierarchyId h) => h.GetAncestor(default(int))), "GetAncestor");
                    result.Add(
                        GetInstanceMethod((HierarchyId h) => h.GetDescendant(default(HierarchyId), default(HierarchyId))), "GetDescendant");
                    result.Add(GetInstanceMethod((HierarchyId h) => h.GetLevel()), "GetLevel");
                    result.Add(GetInstanceMethod((HierarchyId h) => h.IsDescendantOf(default(HierarchyId))), "IsDescendantOf");
                    result.Add(
                        GetInstanceMethod((HierarchyId h) => h.GetReparentedValue(default(HierarchyId), default(HierarchyId))),
                        "GetReparentedValue");
                    return result;
                }

                // Translator for hierarchyid methods into canonical functions. Both static and instance methods are handled.
                // Translation proceeds as follows:
                //      object.MethodName(args...)  -> CanonicalFunctionName(object, args...)
                //      Type.MethodName(args...)  -> CanonicalFunctionName(args...)
                internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var method = call.Method;
                    string canonicalFunctionName;
                    if (!_methodFunctionRenames.TryGetValue(method, out canonicalFunctionName))
                    {
                        canonicalFunctionName = method.Name;
                    }

                    Expression[] arguments;
                    if (method.IsStatic)
                    {
                        Debug.Assert(call.Object == null, "Static method call with instance argument?");
                        arguments = call.Arguments.ToArray();
                    }
                    else
                    {
                        Debug.Assert(call.Object != null, "Instance method call with no instance argument?");
                        arguments = new[] { call.Object }.Concat(call.Arguments).ToArray();
                    }

                    DbExpression result = parent.TranslateIntoCanonicalFunction(canonicalFunctionName, call, arguments);
                    return result;
                }
            }
        }
    }
}
