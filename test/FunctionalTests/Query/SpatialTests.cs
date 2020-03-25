// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;

    public class SpatialTests : FunctionalTestBase
    {
        [Fact]
        public void GeometryDistanceFrom()
        {
            var query = @"
select Edm.Distance(g.c32_geometry,CAST(
        Edm.GeometryFromText(""MULTIPOINT ((10 20), EMPTY)"", 32768) 
        AS Edm.Geometry))
from ArubaContext.AllTypes as g 
where (Edm.Distance(g.c32_geometry,CAST(
    Edm.GeometryFromText(""MULTIPOINT ((10 20), EMPTY)"", 32768) 
    AS Edm.Geometry)) <= 5)";

            // verifying that all of the results returned are <= 5
            using (var db = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, "STDistance"))
            {
                VerifyValueCondition(reader, a => (double) a <= 5);
            }
        }
        
        [Fact]
        public void GeographyDistanceFromMultiPolygon()
        {
            var query = @"
select Edm.Distance(g.c31_geography,
    CAST(Edm.GeographyFromText(""MULTIPOLYGON (((-136.34518219919187 -45.444057174306, 100.08107983924452 0.029396673640468753, -12.771444237628261 0.029396673640468753, -136.34518219919187 -45.444057174306), (-136.34518219919187 -59.574853846584816, -6.6232329320655019 -12.718185214660565, 93.93286853368177 -12.718185214660565, -136.34518219919187 -59.574853846584816)))"", 4326) AS Edm.Geography))
from ArubaContext.AllTypes as g 
where (Edm.Distance(g.c31_geography,
    CAST(Edm.GeographyFromText(""MULTIPOLYGON (((-136.34518219919187 -45.444057174306, 100.08107983924452 0.029396673640468753, -12.771444237628261 0.029396673640468753, -136.34518219919187 -45.444057174306), (-136.34518219919187 -59.574853846584816, -6.6232329320655019 -12.718185214660565, 93.93286853368177 -12.718185214660565, -136.34518219919187 -59.574853846584816)))"", 4326) AS Edm.Geography)) <= 600000)";

            // verifying that all of the results are less than or equal to 600000 as specified in the query
            using (var db = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, "STDistance"))
            {
                VerifyValueCondition(reader, a => (double)a <= 600000);
            }
        }
        
        [Fact]
        public void GeometryPointsWithinPolygon()
        {
            var query = @"
select value 
    g.c32_geometry
from 
    ArubaContext.AllTypes as g
where 
    Edm.SpatialWithin(
        g.c32_geometry,
        GeometryFromText(""MULTILINESTRING ((10 20, 15 20, 15 25, 10 25, 10 20), (12 22, 13 22, 13 23, 12 23, 12 22))"", 32768))";

            // verifying that the points returned are within the specified polygon
            using (var db = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, "STWithin"))
            {
                var shape = DbGeometry.MultiLineFromText(
                        "MULTILINESTRING ((10 20, 15 20, 15 25, 10 25, 10 20), (12 22, 13 22, 13 23, 12 23, 12 22))", 32768);
                VerifyValueCondition(reader, o =>
                    {
                        var g = (DbGeometry)o;
                        return DbSpatialServices.Default.Within(g, shape);
                    });
            }
        }

        [Fact]
        public void GeographyPointsIntersectsPolygon()
        {
            var query = @"  
select value 
    g.c31_geography
from 
    ArubaContext.AllTypes as g
where 
    Edm.SpatialIntersects(
        g.c31_geography,
        GeographyFromText(""MULTIPOLYGON (((-136.34518219919187 -45.444057174306, 100.08107983924452 0.029396673640468753, -12.771444237628261 0.029396673640468753, -136.34518219919187 -45.444057174306), (-136.34518219919187 -59.574853846584816, -6.6232329320655019 -12.718185214660565, 93.93286853368177 -12.718185214660565, -136.34518219919187 -59.574853846584816)))"", 4326))";

            // verifying that the results that are returned intersect the specified polygons
            using (var db = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, "STIntersect"))
            {
                var polygon = DbGeography.MultiPolygonFromText(
                            "MULTIPOLYGON (((-136.34518219919187 -45.444057174306, 100.08107983924452 0.029396673640468753, -12.771444237628261 0.029396673640468753, -136.34518219919187 -45.444057174306), (-136.34518219919187 -59.574853846584816, -6.6232329320655019 -12.718185214660565, 93.93286853368177 -12.718185214660565, -136.34518219919187 -59.574853846584816)))",
                            4326);

                VerifyValueCondition(reader, o =>
                {
                    var g = (DbGeography)o;
                    return DbSpatialServices.Default.Intersects(polygon, g);

                });
            }
        }

        [Fact]
        public void GeometryPolygonsIntersectionAndDisjointAnother()
        {
            var query = @"
select value 
	g.c32_geometry
from 
	ArubaContext.AllTypes as g 
where 
	Edm.SpatialDisjoint(
		GeometryFromText(""POLYGON ((13 22, 12 22, 12 23, 13 22))"", 32768),
		Edm.SpatialIntersection(
			g.c32_geometry,
			GeometryFromText(""MULTILINESTRING ((12 22, 15 22, 15 25, 12 25, 12 22), (13 23, 14 23, 14 24, 13 24, 13 23))"", 32768)
		)
	)";
            var polygon = DbGeometry.PolygonFromText("POLYGON ((13 22, 12 22, 12 23, 13 22))", 32768);
            var multiString =
                DbGeometry.MultiLineFromText(
                    "MULTILINESTRING ((12 22, 15 22, 15 25, 12 25, 12 22), (13 23, 14 23, 14 24, 13 24, 13 23))", 32768);


            // verifying that the results that are returned, when intersected with the given lineStrings are disjoint from the 
            // specified polygon as stated in the query
            using (var db = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, "STDisjoint"))
            {
                VerifyValueCondition(reader, o =>
                    {
                        var g = (DbGeometry) o;
                        return DbSpatialServices.Default.Disjoint(polygon, DbSpatialServices.Default.Intersection(g, multiString));
                    });
            }
        }

        [Fact]
        public void GeometryPointsIntersectWithBufferOfPolygon()
        {
            var query = @"
select value 
	g.c32_geometry
from 
	ArubaContext.AllTypes as g 
where 
	Edm.SpatialIntersects(
		GeometryFromText(""POLYGON ((11 20, 10 20, 10 21, 11 20))"", 32768),
		Edm.SpatialBuffer(g.c32_geometry, 5)
	)";
            var polygon = DbGeometry.PolygonFromText("POLYGON ((11 20, 10 20, 10 21, 11 20))", 32768);

            // verifying that the results returned intersect the buffer (5)
            using (var db = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, "STIntersects"))
            {
                VerifyValueCondition(reader, o =>
                {
                    var g = (DbGeometry)o;
                    return DbSpatialServices.Default.Intersects(polygon, DbSpatialServices.Default.Buffer(g, 5));
                });
            }
        }

        [Fact]
        public void GeometryPolygonsHavingLineCross()
        {
            var query = @"
select 
	o.c32_geometry, 
	i.Geometry
from 
	ArubaContext.AllTypes as o 
left outer join 
	ArubaContext.Runs as i
on 
	Edm.SpatialCrosses(
		o.c32_geometry,
		i.Geometry
	)";
            
            // verifying that the correct sql is generated
            using (var db = new ArubaContext())
            {
                QueryTestHelpers.EntityCommandSetup(db, query, "STCrosses");
            }
        }

        [Fact]
        public void GeometryClosestDistance()
        {
            var query = @"
select 
	Edm.Distance(
		i.[Geometry],
		o.[c32_geometry]
		) as [Distance]
from 
	ArubaContext.[Runs] as [i],
    ArubaContext.[AllTypes] as [o]
where 
	i.[Geometry] is not null 
order by 
	Edm.Distance(
		i.[Geometry],
		o.[c32_geometry]
		) asc";
            
            // verifying that the distances returned match the distances returned by a matching linq query
            using (var db = new ArubaContext())
            using (var db2 = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db2, query, "STDistance"))
            {
                var allTypes = db.AllTypes.Select(s => s.c32_geometry).ToList();
                var runs = db.Runs.Select(s => s.Geometry);
                var expectedResults = from atGeometry in allTypes
                           from rGeometry in runs
                           orderby DbSpatialServices.Default.Distance(atGeometry, rGeometry) ascending 
                           select DbSpatialServices.Default.Distance(atGeometry, rGeometry);

                VerifyAgainstBaselineResults(reader, expectedResults);
            }
        }

        [Fact]
        public void GeometryJoinWhereContains()
        {
            var query = @"
select 
    i.c36_geometry_linestring,
	o.Geometry	
from 
	ArubaContext.Runs as o 
left outer join 
		ArubaContext.AllTypes as i
	on 
		Edm.SpatialContains(
            i.c36_geometry_linestring,
			o.Geometry) 
	where 
		Edm.IsClosedSpatial(i.c36_geometry_linestring)";

            // verifying that the correct sql is generated and the linestrings returned are closed per the query
            using (var db = new ArubaContext())
            using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, "STContains"))
            {

                VerifyValueCondition(reader, o =>
                {
                    var g = (DbGeometry)o;
                    var isClosed = DbSpatialServices.Default.GetIsClosed(g);
                    return isClosed != null && (bool) isClosed;
                });
            }
        }

        #region helpers

        // Verifies that all values contained in the reader satisfy the given condition
        private static void VerifyValueCondition(EntityDataReader reader, Func<object, bool> condition)
        {
            while (reader.Read())
            {
                var value = reader.GetValue(0);
                Assert.True(condition(value));
            }
        }

        private static void VerifySortAscDouble(EntityDataReader reader)
        {
            double value = double.MinValue;
            while (reader.Read())
            {
                var newValue = reader.GetDouble(0);
                Assert.True(value <= newValue);
                value = newValue;
            }
        }

        private static void VerifySortDescDouble(EntityDataReader reader)
        {
            double value = double.MaxValue;
            while (reader.Read())
            {
                var newValue = reader.GetDouble(0);
                Assert.True(value >= newValue);
                value = newValue;
            }
        }

        private static void VerifyAgainstBaselineResults(EntityDataReader reader, IEnumerable<double> expectedResults)
        {
            VerifyAgainstBaselineResults(reader, expectedResults.Cast<object>());
        }

        private static void VerifyAgainstBaselineResults(EntityDataReader reader, IEnumerable<object> expectedResults)
        {
            var actualResults = new List<object>();
            while (reader.Read())
            {
                actualResults.Add(reader.GetValue(0));
            }

            Assert.True(expectedResults.SequenceEqual(actualResults));
        }
        #endregion
    }
}

#endif
