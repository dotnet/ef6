// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Spatial;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed partial class ExpressionConverter
    {
        internal sealed partial class MethodCallTranslator
            : TypedTranslator<MethodCallExpression>
        {
            private sealed class SpatialMethodCallTranslator : CallTranslator
            {
                private static readonly Dictionary<MethodInfo, string> _methodFunctionRenames = GetRenamedMethodFunctions();

                internal SpatialMethodCallTranslator()
                    : base(GetSupportedMethods())
                {
                }

                private static MethodInfo GetStaticMethod<TResult>(Expression<Func<TResult>> lambda)
                {
                    var method = ((MethodCallExpression)lambda.Body).Method;
                    Debug.Assert(
                        method.IsStatic && method.IsPublic &&
                        (method.DeclaringType == typeof(DbGeography) || method.DeclaringType == typeof(DbGeometry)),
                        "Supported static spatial methods should be public static methods declared by a spatial type");
                    return method;
                }

                private static MethodInfo GetInstanceMethod<T, TResult>(Expression<Func<T, TResult>> lambda)
                {
                    var method = ((MethodCallExpression)lambda.Body).Method;
                    Debug.Assert(
                        !method.IsStatic && method.IsPublic &&
                        (method.DeclaringType == typeof(DbGeography) || method.DeclaringType == typeof(DbGeometry)),
                        "Supported instance spatial methods should be public instance methods declared by a spatial type");
                    return method;
                }

                private static IEnumerable<MethodInfo> GetSupportedMethods()
                {
                    yield return GetStaticMethod(() => DbGeography.FromText(default(string)));
                    yield return GetStaticMethod(() => DbGeography.FromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.PointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.LineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.PolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiLineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.GeographyCollectionFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeography.FromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.FromBinary(default(byte[])));
                    yield return GetStaticMethod(() => DbGeography.PointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.LineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.PolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiLineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.MultiPolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.GeographyCollectionFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeography.FromGml(default(string)));
                    yield return GetStaticMethod(() => DbGeography.FromGml(default(string), default(int)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.AsBinary());
                    yield return GetInstanceMethod((DbGeography geo) => geo.AsGml());
                    yield return GetInstanceMethod((DbGeography geo) => geo.AsText());
                    yield return GetInstanceMethod((DbGeography geo) => geo.SpatialEquals(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Disjoint(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Intersects(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Buffer(default(double)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Distance(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Intersection(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Union(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.Difference(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.SymmetricDifference(default(DbGeography)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.ElementAt(default(int)));
                    yield return GetInstanceMethod((DbGeography geo) => geo.PointAt(default(int)));
                    yield return GetStaticMethod(() => DbGeometry.FromText(default(string)));
                    yield return GetStaticMethod(() => DbGeometry.FromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.LineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPointFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiLineFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPolygonFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.GeometryCollectionFromText(default(string), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.FromBinary(default(byte[])));
                    yield return GetStaticMethod(() => DbGeometry.FromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.LineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.PolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPointFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiLineFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.MultiPolygonFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.GeometryCollectionFromBinary(default(byte[]), default(int)));
                    yield return GetStaticMethod(() => DbGeometry.FromGml(default(string)));
                    yield return GetStaticMethod(() => DbGeometry.FromGml(default(string), default(int)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.AsBinary());
                    yield return GetInstanceMethod((DbGeometry geo) => geo.AsGml());
                    yield return GetInstanceMethod((DbGeometry geo) => geo.AsText());
                    yield return GetInstanceMethod((DbGeometry geo) => geo.SpatialEquals(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Disjoint(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Intersects(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Touches(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Crosses(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Within(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Contains(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Overlaps(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Relate(default(DbGeometry), default(string)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Buffer(default(double)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Distance(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Intersection(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Union(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.Difference(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.SymmetricDifference(default(DbGeometry)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.ElementAt(default(int)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.PointAt(default(int)));
                    yield return GetInstanceMethod((DbGeometry geo) => geo.InteriorRingAt(default(int)));
                }

                private static Dictionary<MethodInfo, string> GetRenamedMethodFunctions()
                {
                    var result = new Dictionary<MethodInfo, string>();
                    result.Add(GetStaticMethod(() => DbGeography.FromText(default(string))), "GeographyFromText");
                    result.Add(GetStaticMethod(() => DbGeography.FromText(default(string), default(int))), "GeographyFromText");
                    result.Add(GetStaticMethod(() => DbGeography.PointFromText(default(string), default(int))), "GeographyPointFromText");
                    result.Add(GetStaticMethod(() => DbGeography.LineFromText(default(string), default(int))), "GeographyLineFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeography.PolygonFromText(default(string), default(int))), "GeographyPolygonFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeography.MultiPointFromText(default(string), default(int))), "GeographyMultiPointFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeography.MultiLineFromText(default(string), default(int))), "GeographyMultiLineFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeography.MultiPolygonFromText(default(string), default(int))),
                        "GeographyMultiPolygonFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeography.GeographyCollectionFromText(default(string), default(int))),
                        "GeographyCollectionFromText");
                    result.Add(GetStaticMethod(() => DbGeography.FromBinary(default(byte[]), default(int))), "GeographyFromBinary");
                    result.Add(GetStaticMethod(() => DbGeography.FromBinary(default(byte[]))), "GeographyFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeography.PointFromBinary(default(byte[]), default(int))), "GeographyPointFromBinary");
                    result.Add(GetStaticMethod(() => DbGeography.LineFromBinary(default(byte[]), default(int))), "GeographyLineFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeography.PolygonFromBinary(default(byte[]), default(int))), "GeographyPolygonFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeography.MultiPointFromBinary(default(byte[]), default(int))),
                        "GeographyMultiPointFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeography.MultiLineFromBinary(default(byte[]), default(int))),
                        "GeographyMultiLineFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeography.MultiPolygonFromBinary(default(byte[]), default(int))),
                        "GeographyMultiPolygonFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeography.GeographyCollectionFromBinary(default(byte[]), default(int))),
                        "GeographyCollectionFromBinary");
                    result.Add(GetStaticMethod(() => DbGeography.FromGml(default(string))), "GeographyFromGml");
                    result.Add(GetStaticMethod(() => DbGeography.FromGml(default(string), default(int))), "GeographyFromGml");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.AsBinary()), "AsBinary");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.AsGml()), "AsGml");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.AsText()), "AsText");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.SpatialEquals(default(DbGeography))), "SpatialEquals");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.Disjoint(default(DbGeography))), "SpatialDisjoint");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.Intersects(default(DbGeography))), "SpatialIntersects");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.Buffer(default(double))), "SpatialBuffer");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.Distance(default(DbGeography))), "Distance");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.Intersection(default(DbGeography))), "SpatialIntersection");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.Union(default(DbGeography))), "SpatialUnion");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.Difference(default(DbGeography))), "SpatialDifference");
                    result.Add(
                        GetInstanceMethod((DbGeography geo) => geo.SymmetricDifference(default(DbGeography))), "SpatialSymmetricDifference");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.ElementAt(default(int))), "SpatialElementAt");
                    result.Add(GetInstanceMethod((DbGeography geo) => geo.PointAt(default(int))), "PointAt");
                    result.Add(GetStaticMethod(() => DbGeometry.FromText(default(string))), "GeometryFromText");
                    result.Add(GetStaticMethod(() => DbGeometry.FromText(default(string), default(int))), "GeometryFromText");
                    result.Add(GetStaticMethod(() => DbGeometry.PointFromText(default(string), default(int))), "GeometryPointFromText");
                    result.Add(GetStaticMethod(() => DbGeometry.LineFromText(default(string), default(int))), "GeometryLineFromText");
                    result.Add(GetStaticMethod(() => DbGeometry.PolygonFromText(default(string), default(int))), "GeometryPolygonFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.MultiPointFromText(default(string), default(int))), "GeometryMultiPointFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.MultiLineFromText(default(string), default(int))), "GeometryMultiLineFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.MultiPolygonFromText(default(string), default(int))),
                        "GeometryMultiPolygonFromText");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.GeometryCollectionFromText(default(string), default(int))),
                        "GeometryCollectionFromText");
                    result.Add(GetStaticMethod(() => DbGeometry.FromBinary(default(byte[]))), "GeometryFromBinary");
                    result.Add(GetStaticMethod(() => DbGeometry.FromBinary(default(byte[]), default(int))), "GeometryFromBinary");
                    result.Add(GetStaticMethod(() => DbGeometry.PointFromBinary(default(byte[]), default(int))), "GeometryPointFromBinary");
                    result.Add(GetStaticMethod(() => DbGeometry.LineFromBinary(default(byte[]), default(int))), "GeometryLineFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.PolygonFromBinary(default(byte[]), default(int))), "GeometryPolygonFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.MultiPointFromBinary(default(byte[]), default(int))),
                        "GeometryMultiPointFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.MultiLineFromBinary(default(byte[]), default(int))), "GeometryMultiLineFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.MultiPolygonFromBinary(default(byte[]), default(int))),
                        "GeometryMultiPolygonFromBinary");
                    result.Add(
                        GetStaticMethod(() => DbGeometry.GeometryCollectionFromBinary(default(byte[]), default(int))),
                        "GeometryCollectionFromBinary");
                    result.Add(GetStaticMethod(() => DbGeometry.FromGml(default(string))), "GeometryFromGml");
                    result.Add(GetStaticMethod(() => DbGeometry.FromGml(default(string), default(int))), "GeometryFromGml");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.AsBinary()), "AsBinary");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.AsGml()), "AsGml");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.AsText()), "AsText");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.SpatialEquals(default(DbGeometry))), "SpatialEquals");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Disjoint(default(DbGeometry))), "SpatialDisjoint");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Intersects(default(DbGeometry))), "SpatialIntersects");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Touches(default(DbGeometry))), "SpatialTouches");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Crosses(default(DbGeometry))), "SpatialCrosses");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Within(default(DbGeometry))), "SpatialWithin");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Contains(default(DbGeometry))), "SpatialContains");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Overlaps(default(DbGeometry))), "SpatialOverlaps");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Relate(default(DbGeometry), default(string))), "SpatialRelate");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Buffer(default(double))), "SpatialBuffer");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Distance(default(DbGeometry))), "Distance");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Intersection(default(DbGeometry))), "SpatialIntersection");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Union(default(DbGeometry))), "SpatialUnion");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.Difference(default(DbGeometry))), "SpatialDifference");
                    result.Add(
                        GetInstanceMethod((DbGeometry geo) => geo.SymmetricDifference(default(DbGeometry))), "SpatialSymmetricDifference");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.ElementAt(default(int))), "SpatialElementAt");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.PointAt(default(int))), "PointAt");
                    result.Add(GetInstanceMethod((DbGeometry geo) => geo.InteriorRingAt(default(int))), "InteriorRingAt");
                    return result;
                }

                // Translator for spatial methods into canonical functions. Both static and instance methods are handled.
                // Unless a canonical function name is explicitly specified for a method, the mapping from method name to
                // canonical function name consists simply of applying the 'ST' prefix. Then, translation proceeds as follows:
                //      object.MethodName(args...)  -> CanonicalFunctionName(object, args...)
                //      Type.MethodName(args...)  -> CanonicalFunctionName(args...)
                internal override DbExpression Translate(ExpressionConverter parent, MethodCallExpression call)
                {
                    var method = call.Method;
                    string canonicalFunctionName;
                    if (!_methodFunctionRenames.TryGetValue(method, out canonicalFunctionName))
                    {
                        canonicalFunctionName = "ST" + method.Name;
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
