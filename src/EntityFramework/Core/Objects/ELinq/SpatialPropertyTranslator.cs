// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Spatial;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed partial class ExpressionConverter
    {
        private sealed partial class MemberAccessTranslator
            : TypedTranslator<MemberExpression>
        {
            private sealed class SpatialPropertyTranslator : PropertyTranslator
            {
                private readonly Dictionary<PropertyInfo, string> propertyFunctionRenames = GetRenamedPropertyFunctions();

                internal SpatialPropertyTranslator()
                    : base(GetSupportedProperties())
                {
                }

                private static PropertyInfo GetProperty<T, TResult>(Expression<Func<T, TResult>> lambda)
                {
                    var memberEx = (MemberExpression)lambda.Body;
                    var property = (PropertyInfo)memberEx.Member;
                    Debug.Assert(
                        property.GetGetMethod().IsPublic &&
                        !property.GetGetMethod().IsStatic &&
                        (property.DeclaringType == typeof(DbGeography) || property.DeclaringType == typeof(DbGeometry)),
                        "GetProperty<T, TResult> should only be used to bind to public instance spatial properties");
                    return property;
                }

                private static IEnumerable<PropertyInfo> GetSupportedProperties()
                {
                    yield return GetProperty((DbGeography geo) => geo.CoordinateSystemId);
                    yield return GetProperty((DbGeography geo) => geo.SpatialTypeName);
                    yield return GetProperty((DbGeography geo) => geo.Dimension);
                    yield return GetProperty((DbGeography geo) => geo.IsEmpty);
                    yield return GetProperty((DbGeography geo) => geo.ElementCount);
                    yield return GetProperty((DbGeography geo) => geo.Latitude);
                    yield return GetProperty((DbGeography geo) => geo.Longitude);
                    yield return GetProperty((DbGeography geo) => geo.Elevation);
                    yield return GetProperty((DbGeography geo) => geo.Measure);
                    yield return GetProperty((DbGeography geo) => geo.Length);
                    yield return GetProperty((DbGeography geo) => geo.StartPoint);
                    yield return GetProperty((DbGeography geo) => geo.EndPoint);
                    yield return GetProperty((DbGeography geo) => geo.IsClosed);
                    yield return GetProperty((DbGeography geo) => geo.PointCount);
                    yield return GetProperty((DbGeography geo) => geo.Area);
                    yield return GetProperty((DbGeometry geo) => geo.CoordinateSystemId);
                    yield return GetProperty((DbGeometry geo) => geo.SpatialTypeName);
                    yield return GetProperty((DbGeometry geo) => geo.Dimension);
                    yield return GetProperty((DbGeometry geo) => geo.Envelope);
                    yield return GetProperty((DbGeometry geo) => geo.IsEmpty);
                    yield return GetProperty((DbGeometry geo) => geo.IsSimple);
                    yield return GetProperty((DbGeometry geo) => geo.Boundary);
                    yield return GetProperty((DbGeometry geo) => geo.IsValid);
                    yield return GetProperty((DbGeometry geo) => geo.ConvexHull);
                    yield return GetProperty((DbGeometry geo) => geo.ElementCount);
                    yield return GetProperty((DbGeometry geo) => geo.XCoordinate);
                    yield return GetProperty((DbGeometry geo) => geo.YCoordinate);
                    yield return GetProperty((DbGeometry geo) => geo.Elevation);
                    yield return GetProperty((DbGeometry geo) => geo.Measure);
                    yield return GetProperty((DbGeometry geo) => geo.Length);
                    yield return GetProperty((DbGeometry geo) => geo.StartPoint);
                    yield return GetProperty((DbGeometry geo) => geo.EndPoint);
                    yield return GetProperty((DbGeometry geo) => geo.IsClosed);
                    yield return GetProperty((DbGeometry geo) => geo.IsRing);
                    yield return GetProperty((DbGeometry geo) => geo.PointCount);
                    yield return GetProperty((DbGeometry geo) => geo.Area);
                    yield return GetProperty((DbGeometry geo) => geo.Centroid);
                    yield return GetProperty((DbGeometry geo) => geo.PointOnSurface);
                    yield return GetProperty((DbGeometry geo) => geo.ExteriorRing);
                    yield return GetProperty((DbGeometry geo) => geo.InteriorRingCount);
                }

                private static Dictionary<PropertyInfo, string> GetRenamedPropertyFunctions()
                {
                    var result = new Dictionary<PropertyInfo, string>();
                    result.Add(GetProperty((DbGeography geo) => geo.CoordinateSystemId), "CoordinateSystemId");
                    result.Add(GetProperty((DbGeography geo) => geo.SpatialTypeName), "SpatialTypeName");
                    result.Add(GetProperty((DbGeography geo) => geo.Dimension), "SpatialDimension");
                    result.Add(GetProperty((DbGeography geo) => geo.IsEmpty), "IsEmptySpatial");
                    result.Add(GetProperty((DbGeography geo) => geo.ElementCount), "SpatialElementCount");
                    result.Add(GetProperty((DbGeography geo) => geo.Latitude), "Latitude");
                    result.Add(GetProperty((DbGeography geo) => geo.Longitude), "Longitude");
                    result.Add(GetProperty((DbGeography geo) => geo.Elevation), "Elevation");
                    result.Add(GetProperty((DbGeography geo) => geo.Measure), "Measure");
                    result.Add(GetProperty((DbGeography geo) => geo.Length), "SpatialLength");
                    result.Add(GetProperty((DbGeography geo) => geo.StartPoint), "StartPoint");
                    result.Add(GetProperty((DbGeography geo) => geo.EndPoint), "EndPoint");
                    result.Add(GetProperty((DbGeography geo) => geo.IsClosed), "IsClosedSpatial");
                    result.Add(GetProperty((DbGeography geo) => geo.PointCount), "PointCount");
                    result.Add(GetProperty((DbGeography geo) => geo.Area), "Area");
                    result.Add(GetProperty((DbGeometry geo) => geo.CoordinateSystemId), "CoordinateSystemId");
                    result.Add(GetProperty((DbGeometry geo) => geo.SpatialTypeName), "SpatialTypeName");
                    result.Add(GetProperty((DbGeometry geo) => geo.Dimension), "SpatialDimension");
                    result.Add(GetProperty((DbGeometry geo) => geo.Envelope), "SpatialEnvelope");
                    result.Add(GetProperty((DbGeometry geo) => geo.IsEmpty), "IsEmptySpatial");
                    result.Add(GetProperty((DbGeometry geo) => geo.IsSimple), "IsSimpleGeometry");
                    result.Add(GetProperty((DbGeometry geo) => geo.Boundary), "SpatialBoundary");
                    result.Add(GetProperty((DbGeometry geo) => geo.IsValid), "IsValidGeometry");
                    result.Add(GetProperty((DbGeometry geo) => geo.ConvexHull), "SpatialConvexHull");
                    result.Add(GetProperty((DbGeometry geo) => geo.ElementCount), "SpatialElementCount");
                    result.Add(GetProperty((DbGeometry geo) => geo.XCoordinate), "XCoordinate");
                    result.Add(GetProperty((DbGeometry geo) => geo.YCoordinate), "YCoordinate");
                    result.Add(GetProperty((DbGeometry geo) => geo.Elevation), "Elevation");
                    result.Add(GetProperty((DbGeometry geo) => geo.Measure), "Measure");
                    result.Add(GetProperty((DbGeometry geo) => geo.Length), "SpatialLength");
                    result.Add(GetProperty((DbGeometry geo) => geo.StartPoint), "StartPoint");
                    result.Add(GetProperty((DbGeometry geo) => geo.EndPoint), "EndPoint");
                    result.Add(GetProperty((DbGeometry geo) => geo.IsClosed), "IsClosedSpatial");
                    result.Add(GetProperty((DbGeometry geo) => geo.IsRing), "IsRing");
                    result.Add(GetProperty((DbGeometry geo) => geo.PointCount), "PointCount");
                    result.Add(GetProperty((DbGeometry geo) => geo.Area), "Area");
                    result.Add(GetProperty((DbGeometry geo) => geo.Centroid), "Centroid");
                    result.Add(GetProperty((DbGeometry geo) => geo.PointOnSurface), "PointOnSurface");
                    result.Add(GetProperty((DbGeometry geo) => geo.ExteriorRing), "ExteriorRing");
                    result.Add(GetProperty((DbGeometry geo) => geo.InteriorRingCount), "InteriorRingCount");
                    return result;
                }

                // Translator for spatial properties into canonical functions. Both static and instance properties are handled.
                // Unless a canonical function name is explicitly specified for a property, the mapping from property name to
                // canonical function name consists simply of applying the 'ST' prefix. Then, translation proceeds as follows:
                //      object.PropertyName  -> CanonicalFunctionName(object)
                //      Type.PropertyName  -> CanonicalFunctionName()
                internal override DbExpression Translate(ExpressionConverter parent, MemberExpression call)
                {
                    var property = (PropertyInfo)call.Member;
                    string canonicalFunctionName;
                    if (!propertyFunctionRenames.TryGetValue(property, out canonicalFunctionName))
                    {
                        canonicalFunctionName = "ST" + property.Name;
                    }

                    Debug.Assert(call.Expression != null, "No static spatial properties currently map to canonical functions");
                    DbExpression result = parent.TranslateIntoCanonicalFunction(canonicalFunctionName, call, call.Expression);
                    return result;
                }
            }
        }
    }
}
