// Copyright © 2013, 2017, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of XG hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// XG.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of XG Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using XuguClient;
using System.Data.Entity.Spatial;


namespace Xugu.Data.EntityFramework
{
   /*nternal sealed class XGSpatialServices : DbSpatialServices
  {
    
    internal static readonly XGSpatialServices Instance = new XGSpatialServices();

    private XGSpatialServices()
      {
      }

    #region overriden methods

    public override byte[] AsBinary(DbGeography geographyValue)
    {
      throw new NotImplementedException();
    }

    public override byte[] AsBinary(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var providerValue = new XGGeometry();
      XGGeometry.TryParse(geometryValue.ProviderValue.ToString(), out providerValue);

      return providerValue.Value;
    }

    public override string AsGml(DbGeometry geometryValue)
    {
      throw new NotImplementedException();
    }

    public override string AsGml(DbGeography geographyValue)
    {
       throw new NotImplementedException();
    }

    public override string AsText(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var providerValue = new XGGeometry();
      XGGeometry.TryParse(geometryValue.ProviderValue.ToString(), out providerValue);

      return providerValue.ToString();
    }

    public override string AsText(DbGeography geographyValue)
    {
       throw new NotImplementedException();
    }

    public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      DbGeometry XGValue = DbGeometry.FromText(geometryValue.ProviderValue.ToString());
      return XGValue.Buffer(distance);     
    }

    public override DbGeography Buffer(DbGeography geographyValue, double distance)
    {
      throw new NotImplementedException();        
    }

     public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
     {
       if (geometryValue == null)
         throw new ArgumentNullException("geometryValue");

       if (otherGeometry == null)
         throw new ArgumentNullException("otherGeometry");

       DbGeometry XGValue = DbGeometry.FromText(geometryValue.ProviderValue.ToString());
       return XGValue.Contains(otherGeometry);

     }

     public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
     {
       if (wellKnownValue == null)
          throw new ArgumentNullException("wellKnownValue");

       if (wellKnownValue.WellKnownText != null)
       {
         var XGGeometry = new XGGeometry(true);
         XGGeometry.TryParse(wellKnownValue.WellKnownText.ToString(), out XGGeometry);
         return XGGeometry;          
       }
       else if (wellKnownValue.WellKnownBinary != null)
       {
         var XGGeometry = new XGGeometry(XGDbType.Geometry, wellKnownValue.WellKnownBinary);         
         return XGGeometry;                 
       }
       return null;
      }

    public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
    {
        throw new NotImplementedException();
    }

    public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");    

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.WellKnownValue;
    
    }

    public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
    {
          throw new NotImplementedException();
    }

    public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);
      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Crosses(XGOtherValue);
    }

    public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);
      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Difference(XGOtherValue);

    }

    public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
    {
        throw new System.NotImplementedException();
    }

    public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
        throw new System.NotImplementedException();
    }

    public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
    {
        throw new System.NotImplementedException();
    }

    public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");
         
      DbGeometry XGValue = DbGeometry.FromText(geometryValue.ProviderValue.ToString());

      Double? result = XGValue.Distance(otherGeometry);
      if (result != null)
        return result.Value;
      return 0;
    }

    public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
    {
       throw new NotImplementedException();
    }

    public override DbGeometry ElementAt(DbGeometry geometryValue, int index)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.ElementAt(index);
    }

    public override DbGeography ElementAt(DbGeography geographyValue, int index)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyFromBinary(byte[] wellKnownBinary, int coordinateSystemId)
    {
          throw new NotImplementedException();
    }

    public override DbGeography GeographyFromBinary(byte[] wellKnownBinary)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyFromGml(string geographyMarkup, int coordinateSystemId)
    {
          throw new NotImplementedException();
    }

    public override DbGeography GeographyFromGml(string geographyMarkup)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyFromProviderValue(object providerValue)
    {
          throw new NotImplementedException();
    }

    public override DbGeography GeographyFromText(string wellKnownText, int coordinateSystemId)
    {
          throw new NotImplementedException();
    }

    public override DbGeography GeographyFromText(string wellKnownText)
    {
          throw new NotImplementedException();
    }

    public override DbGeography GeographyLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyLineFromText(string lineWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyMultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyPointFromText(string pointWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeography GeographyPolygonFromText(string polygonWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryCollectionFromText(string geometryCollectionWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryFromBinary(byte[] wellKnownBinary, int coordinateSystemId)
    {
      if (wellKnownBinary == null)
        throw new ArgumentNullException("wellKnownBinary");
    
      DbGeometry XGValue = DbGeometry.FromBinary(wellKnownBinary, coordinateSystemId);

      return GeometryFromProviderValue(XGValue);
    }

    public override DbGeometry GeometryFromBinary(byte[] wellKnownBinary)
    {
      if (wellKnownBinary == null)
        throw new ArgumentNullException("wellKnownBinary");

      DbGeometry XGValue = DbGeometry.FromBinary(wellKnownBinary);

      return GeometryFromProviderValue(XGValue);
    }

    public override DbGeometry GeometryFromGml(string geometryMarkup, int coordinateSystemId)
    {
          throw new NotImplementedException();
    }

    public override DbGeometry GeometryFromGml(string geometryMarkup)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryFromProviderValue(object providerValue)
    {
      if (providerValue == null)
        throw new ArgumentNullException("provider value");

      var myGeom = new XGGeometry();

      if (XGGeometry.TryParse(providerValue.ToString(), out myGeom))      
       return DbGeometry.FromText(myGeom.GetWKT(), myGeom.SRID.Value);
      else
        return null;      
    }

    public override DbGeometry GeometryFromText(string wellKnownText, int coordinateSystemId)
    {
      if (String.IsNullOrEmpty(wellKnownText))
        throw new ArgumentNullException("wellKnownText");

      var geomValue = DbGeometry.FromText(wellKnownText, coordinateSystemId);

      var XGValue = GeometryFromProviderValue(geomValue);

      return XGValue;
    }

    public override DbGeometry GeometryFromText(string wellKnownText)
    {
      if (String.IsNullOrEmpty(wellKnownText))
        throw new ArgumentNullException("wellKnownText");

      var geomValue = DbGeometry.FromText(wellKnownText);
      
      var XGValue = GeometryFromProviderValue(geomValue);

      return XGValue;

    }

    public override DbGeometry GeometryLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryLineFromText(string lineWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryPointFromText(string pointWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GeometryPolygonFromText(string polygonWellKnownText, int coordinateSystemId)
    {
        throw new System.NotImplementedException();
    }

    public override double? GetArea(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");      

       var XGValue = GeometryFromProviderValue(geometryValue);
       return XGValue.Area;
    }

    public override double? GetArea(DbGeography geographyValue)
    {
          throw new NotImplementedException();
    }

    public override DbGeometry GetBoundary(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.Boundary;
    }

    public override DbGeometry GetCentroid(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.Centroid;
    }

    public override DbGeometry GetConvexHull(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.ConvexHull;
    }

    public override int GetCoordinateSystemId(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.CoordinateSystemId;
    }

    public override int GetCoordinateSystemId(DbGeography geographyValue)
    {
        throw new NotImplementedException();
    }

    public override int GetDimension(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.Dimension;
    }

    public override int GetDimension(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override int? GetElementCount(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.ElementCount;
    }

    public override int? GetElementCount(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override double? GetElevation(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.Elevation;
    }

    public override double? GetElevation(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GetEndPoint(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.EndPoint;
    }

    public override DbGeography GetEndPoint(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GetEnvelope(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.Envelope;
    }

    public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.ExteriorRing;
    }

    public override int? GetInteriorRingCount(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.InteriorRingCount;
    }

    public override bool? GetIsClosed(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.IsClosed;
    }

    public override bool? GetIsClosed(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override bool GetIsEmpty(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.IsEmpty;
    }

    public override bool GetIsEmpty(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override bool? GetIsRing(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.IsRing;
    }

    public override bool GetIsSimple(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.IsSimple;
    }

    public override bool GetIsValid(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.IsValid;
    }

    public override double? GetLatitude(DbGeography geographyValue)
    {
          throw new NotImplementedException();
    }

    public override double? GetLength(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.Length;
    }

    public override double? GetLength(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override double? GetLongitude(DbGeography geographyValue)
    {
        throw new NotImplementedException();
    }

    public override double? GetMeasure(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.Measure;
    }

    public override double? GetMeasure(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override int? GetPointCount(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.PointCount;
    }

    public override int? GetPointCount(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.PointOnSurface;
    }

    public override string GetSpatialTypeName(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.SpatialTypeName;
    }

    public override string GetSpatialTypeName(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override DbGeometry GetStartPoint(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.StartPoint;
    }

    public override DbGeography GetStartPoint(DbGeography geographyValue)
    {
        throw new System.NotImplementedException();
    }

    public override double? GetXCoordinate(DbGeometry geometryValue)
    {
      if (geometryValue == null)          
        throw new ArgumentNullException("geometryValue");

      var providerValue = new XGGeometry();
      XGGeometry.TryParse(geometryValue.ProviderValue.ToString(), out providerValue);
      return providerValue.XCoordinate;
    }

    public override double? GetYCoordinate(DbGeometry geometryValue)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      var providerValue = new XGGeometry();
      XGGeometry.TryParse(geometryValue.ProviderValue.ToString(), out providerValue);
      return providerValue.YCoordinate;
    }

    public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int index)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");
      
      var XGValue = GeometryFromProviderValue(geometryValue);

      return XGValue.InteriorRingAt(index);
    }

    public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);

      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Intersection(XGOtherValue);
    }

    public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
    {
        throw new System.NotImplementedException();
    }

    public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);

      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Intersects(XGOtherValue);
     
    }

    public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
    {
        throw new System.NotImplementedException();
    }

    public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");
      
      
      var XGValue = GeometryFromProviderValue(geometryValue);

      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Overlaps(XGOtherValue);
    }

    public override DbGeometry PointAt(DbGeometry geometryValue, int index)
    {
       throw new System.NotImplementedException();
    }

    public override DbGeography PointAt(DbGeography geographyValue, int index)
    {
        throw new System.NotImplementedException();
    }

    public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      if (String.IsNullOrEmpty(matrix))
        throw new ArgumentNullException("matrix");

      var XGValue = GeometryFromProviderValue(geometryValue);

      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Relate(XGOtherValue, matrix);
    }

    public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);

      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.SpatialEquals(XGOtherValue);
    }

    public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
    {
          throw new NotImplementedException();
    }

    public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);

      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.SymmetricDifference(XGOtherValue);
    }

    public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
    {
        throw new System.NotImplementedException();
    }

    public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);

      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Touches(XGOtherValue);
    }

    public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);
      
      var XGOtherValue = GeometryFromProviderValue(otherGeometry);

      return XGValue.Union(XGOtherValue);
    }

    public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
    {
        throw new System.NotImplementedException();
    }

    public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
    {
      if (geometryValue == null)
        throw new ArgumentNullException("geometryValue");

      if (otherGeometry == null)
        throw new ArgumentNullException("otherGeometry");

      var XGValue = GeometryFromProviderValue(geometryValue);
      var XGOtherValue = GeometryFromProviderValue(otherGeometry);
      return  XGValue.Within(XGOtherValue);      
    }   

    #endregion
  }*/
}
