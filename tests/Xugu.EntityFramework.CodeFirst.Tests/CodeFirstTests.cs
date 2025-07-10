using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Spatial;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Xugu.Data.EntityFramework;
using Xugu.EntityFramework.CodeFirst.Tests.Properties;
using Xugu.EntityFramework.Tests;
using XuguClient;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    public class CodeFirstTests : CodeFirstFixture
    {
        /// <summary>
        /// Tests for fix of http://bugs.mysql.com/bug.php?id=61230
        /// ("The provider did not return a ProviderManifestToken string.").
        /// </summary>
        [Test]
        public void SimpleCodeFirstSelect()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            MovieDBContext db = new MovieDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            MovieDBInitialize.DoDataPopulation(db);
            var l = db.Movies.ToList();
            int j = l.Count;
            foreach (var i in l)
            {
                j--;
            }
            Assert.AreEqual(0, j);
        }

        /// <summary>
        /// Tests for fix of http://bugs.mysql.com/bug.php?id=62150
        /// ("EF4.1, Code First, CreateDatabaseScript() generates an invalid MySQL script.").
        /// </summary>
        [Test]
        public void AlterTableTest()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            MovieDBContext db = new MovieDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            MovieDBInitialize.DoDataPopulation(db);
            var l = db.MovieFormats.ToList();
            int j = l.Count;
            foreach (var i in l)
            {
                j--;
            }
            Assert.AreEqual(0, j);
            MovieFormat m = new MovieFormat();
            m.Format = 8.0f;
            db.MovieFormats.Add(m);
            db.SaveChanges();
            MovieFormat m2 = db.MovieFormats.Where(p => p.Format == 8.0f).FirstOrDefault();
            Assert.NotNull(m2);
            Assert.AreEqual(8.0f, m2.Format);
        }

        /// <summary>
        /// Fix for "Connector/Net Generates Incorrect SELECT Clause after UPDATE" (MySql bug #62134, Oracle bug #13491689).
        /// </summary>
        [Test]
        public void ConcurrencyCheckWithNonDbGeneratedColumn()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Delete();
                db.Database.CreateIfNotExists();
                MovieDBInitialize.DoDataPopulation(db);
                db.Database.ExecuteSqlCommand(@"DROP TABLE IF EXISTS `MovieReleases`");

                db.Database.ExecuteSqlCommand(
        @"CREATE TABLE IF NOT EXISTS `MovieReleases` (
  `Id` int NOT NULL,
  `Name` binary(45) NOT NULL,
  `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
);");
                XGTrace.Listeners.Clear();
                XGTrace.Switch.Level = SourceLevels.All;
                GenericListener listener = new GenericListener();
                XGTrace.Listeners.Add(listener);
                try
                {
                    MovieRelease mr = db.MovieReleases.Create();
                    mr.Id = 1;
                    mr.Name = "Commercial";
                    db.MovieReleases.Add(mr);
                    db.SaveChanges();
                    mr.Name = "Director's Cut";
                    db.SaveChanges();
                }
                finally
                {
                    db.Database.ExecuteSqlCommand(@"DROP TABLE IF EXISTS `MovieReleases`");
                }
                // Check sql        
                Regex rx = new Regex(@"Query Opened: (?<item>UPDATE .*)", RegexOptions.Compiled | RegexOptions.Singleline);
                foreach (string s in listener.Strings)
                {
                    Match m = rx.Match(s);
                    if (m.Success)
                    {
                        CheckSql(m.Groups["item"].Value, SQLSyntax.UpdateWithSelectWithNonDbGeneratedLock);
                        //Assert.Pass();
                    }
                }
                //Assert.Fail();
            }
        }

        /// <summary>
        /// This tests fix for http://bugs.mysql.com/bug.php?id=64216.
        /// </summary>
        [Test]
        public void CheckByteArray()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            MovieDBContext db = new MovieDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            string dbCreationScript =
              ((IObjectContextAdapter)db).ObjectContext.CreateDatabaseScript();
            Regex rx = new Regex(@"`Data` (?<type>[^\),]*)", RegexOptions.Compiled | RegexOptions.Singleline);
            Match m = rx.Match(dbCreationScript);
            Assert.AreEqual("blob", m.Groups["type"].Value);
        }

        /// <summary>
        /// Validates a stored procedure call using Code First
        /// Bug #14008699
        [Test]
        public void CallStoredProcedure()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext context = new MovieDBContext())
            {
                context.Database.Connection.Open();
                context.Database.Initialize(true);
                context.Database.ExecuteSqlCommand(@"drop procedure if exists `GetCount`");
                context.Database.ExecuteSqlCommand(@"create procedure `GetCount`() AS begin select 5; end;");
                long count = context.Database.SqlQuery<long>("CALL \"GetCount\"").First();

                Assert.AreEqual(5, count);
            }
        }

        /// <summary>
        /// Tests for fix of http://bugs.mysql.com/bug.php?id=63920
        /// Maxlength error when it's used code-first and inheritance (discriminator generated column)
        /// </summary>
        [Test]
        public void Bug63920_Test1()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (VehicleDbContext context = new VehicleDbContext())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Initialize(true);

                context.Vehicles.Add(new Car { Id = 1, Name = "Mustang", Year = 2012, CarProperty = "Car" });
                context.Vehicles.Add(new Bike { Id = 101, Name = "Mountain", Year = 2011, BikeProperty = "Bike" });
                context.SaveChanges();

                var list = context.Vehicles.ToList();

                int records = -1;
                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    XGCommand cmd = new XGCommand("SELECT COUNT(*) FROM Vehicles", conn);
                    records = Convert.ToInt32(cmd.ExecuteScalar());
                }

                Assert.AreEqual(context.Vehicles.Count(), records);
            }
        }

        /// <summary>
        /// Tests for fix of http://bugs.mysql.com/bug.php?id=63920
        /// Key reference generation script error when it's used code-first and a single table for the inherited models
        /// </summary>
        [Test]
        public void Bug63920_Test2()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (VehicleDbContext2 context = new VehicleDbContext2())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Initialize(true);

                context.Vehicles.Add(new Car2 { Id = 1, Name = "Mustang", Year = 2012, CarProperty = "Car" });
                context.Vehicles.Add(new Bike2 { Id = 101, Name = "Mountain", Year = 2011, BikeProperty = "Bike" });
                context.SaveChanges();

                var list = context.Vehicles.ToList();

                int records = -1;
                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    XGCommand cmd = new XGCommand("SELECT COUNT(*) FROM Vehicle2", conn);
                    records = Convert.ToInt32(cmd.ExecuteScalar());
                }

                Assert.AreEqual(context.Vehicles.Count(), records);
            }
        }

        /// <summary>
        /// This test fix for precision customization for columns bug (http://bugs.mysql.com/bug.php?id=65001), 
        /// Trying to customize column precision in Code First does not work).
        /// </summary>
        [Test]
        //[Ignore("Fix this")]
        public void TestPrecisionNscale()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            MovieDBContext db = new MovieDBContext();
            db.Database.Initialize(true);
            var l = db.Movies.ToList();
            using (XGConnection connection=new XGConnection(db.Database.Connection.ConnectionString))
            {
                connection.Open();
                using (XGDataReader r = new XGCommand($@"SELECT SHR(`SCALE`,16) AS numeric_precision, MOD(`SCALE`,65536) as numeric_scale from all_columns WHERE TYPE_NAME='NUMERIC' and table_id = (SELECT TABLE_id FROM all_tables WHERE TABLE_NAME='Movies' LIMIT 1) and col_name = 'Price'", connection).ExecuteReader())
                {
                    r.Read();
                    Assert.AreEqual(16, r.GetInt32(0));
                    Assert.AreEqual(2, r.GetInt32(1));
                }
            }
            
        }

        /// <summary>
        /// Test String types to StoreType for String
        /// A string with FixedLength=true will become a char 
        /// Max Length left empty will be char(max)
        /// Max Length(100) will be char(100) 
        /// while FixedLength=false will result in nvarchar. 
        /// Max Length left empty will be nvarchar(max)
        /// Max Length(100) will be nvarchar(100)                
        /// </summary>
        [Test]
        public void TestStringTypeToStoreType()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (VehicleDbContext3 context = new VehicleDbContext3())
            {
                context.Database.Connection.Open();
                if (context.Database.Exists()) context.Database.Delete();
                context.Database.CreateIfNotExists();
                context.Accessories.Add(new Accessory { Name = "Accesory One", Description = "Accesories descriptions", LongDescription = "Some long description" });
                context.SaveChanges();

                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    XGCommand query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Accessories' LIMIT 1) AND COL_NAME='Description'", conn);
                    query.Connection = conn;
                    XGDataReader reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("Description", reader[0].ToString());
                        Assert.AreEqual("False", reader[1].ToString());
                        Assert.AreEqual("CLOB", reader[2].ToString());
                    }
                    reader.Close();

                    query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type,SCALE AS character_maximum_length FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Accessories' LIMIT 1) AND COL_NAME='Name'", conn);
                    reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("Name", reader[0].ToString());
                        Assert.AreEqual("False", reader[1].ToString());
                        Assert.AreEqual("CHAR", reader[2].ToString());
                        Assert.AreEqual("255", reader[3].ToString());
                    }
                    reader.Close();

                    query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type,SCALE AS character_maximum_length FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Accessories' LIMIT 1) AND COL_NAME='LongDescription'", conn);
                    reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("LongDescription", reader[0].ToString());
                        Assert.AreEqual("False", reader[1].ToString());
                        Assert.AreEqual("CLOB", reader[2].ToString());
                        Assert.AreEqual(/*"4294967295"*/"-1", reader[3].ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Test fix for http://bugs.mysql.com/bug.php?id=66066 / http://clustra.no.oracle.com/orabugs/bug.php?id=14479715
        /// (Using EF, crash when generating insert with no values.).
        /// </summary>
        [Test]
        public void AddingEmptyRow()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext ctx = new MovieDBContext())
            {
                ctx.Database.Connection.Open();
                ctx.Database.Initialize(true);
                ctx.EntitySingleColumns.Add(new EntitySingleColumn());
                ctx.SaveChanges();
            }

            using (MovieDBContext ctx2 = new MovieDBContext())
            {
                var q = from esc in ctx2.EntitySingleColumns where esc.Id == 1 select esc;
                Assert.AreEqual(1, q.Count());
            }
        }

        /// <summary>
        /// Test for identity columns when type is Integer or Guid (auto-generate
        /// values)
        /// </summary>
        [Test]
        public void IdentityTest()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (VehicleDbContext context = new VehicleDbContext())
            {
                context.Database.Connection.Open();
                if (context.Database.Exists()) context.Database.Delete();
                context.Database.CreateIfNotExists();

                // Identity as Guid
                Manufacturer nissan = new Manufacturer
                {
                    Name = "Nissan"
                };
                Manufacturer ford = new Manufacturer
                {
                    Name = "Ford"
                };
                context.Manufacturers.Add(nissan);
                context.Manufacturers.Add(ford);

                // Identity as Integer
                Distributor dis1 = new Distributor
                {
                    Name = "Distributor1"
                };
                Distributor dis2 = new Distributor
                {
                    Name = "Distributor2"
                };
                context.Distributors.Add(dis1);
                context.Distributors.Add(dis2);

                context.SaveChanges();

                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.Open();

                    // Validates Guid
                    XGCommand cmd = new XGCommand("SELECT * FROM Manufacturers", conn);
                    XGDataReader dr = cmd.ExecuteReader();
                    Assert.False(!dr.HasRows, "No records found");

                    while (dr.Read())
                    {
                        string name = dr.GetString(1);
                        switch (name)
                        {
                            case "Nissan":
                                Assert.AreEqual(dr.GetGuid(0), nissan.ManufacturerId);
                                Assert.AreEqual(dr.GetGuid(2), nissan.GroupIdentifier);
                                break;
                            case "Ford":
                                Assert.AreEqual(dr.GetGuid(0), ford.ManufacturerId);
                                Assert.AreEqual(dr.GetGuid(2), ford.GroupIdentifier);
                                break;
                            default:
                                //Assert.Fail();
                                break;
                        }
                    }
                    dr.Close();

                    // Validates Integer
                    cmd = new XGCommand("SELECT * FROM Distributors", conn);
                    dr = cmd.ExecuteReader();
                    if (!dr.HasRows)
                        //Assert.Fail("No records found");
                        while (dr.Read())
                        {
                            string name = dr.GetString(1);
                            switch (name)
                            {
                                case "Distributor1":
                                    Assert.AreEqual(dr.GetInt32(0), dis1.DistributorId);
                                    break;
                                case "Distributor2":
                                    Assert.AreEqual(dr.GetInt32(0), dis2.DistributorId);
                                    break;
                                default:
                                    //Assert.Fail();
                                    break;
                            }
                        }
                    dr.Close();
                }
            }
        }

        /// <summary>
        /// This test the fix for bug 67377.
        /// </summary>
        [Test]
        public void FirstOrDefaultNested()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext ctx = new MovieDBContext())
            {
                ctx.Database.Connection.Open();
                ctx.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(ctx);
                int DirectorId = 1;
                var q = ctx.Movies.Where(p => p.Director.ID == DirectorId).Select(p =>
                  new
                  {
                      Id = p.ID,
                      FirstMovieFormat = p.Formats.Count == 0 ? 0.0 : p.Formats.FirstOrDefault().Format
                  });
                string sql = q.ToString();
#if DEBUG
                Debug.WriteLine(sql);
#endif
                int j = q.Count();
                foreach (var r in q)
                {
                    j--;
                }
                Assert.AreEqual(0, j);
            }
        }

        /// <summary>
        /// This tests the fix for bug 73549, Generated Sql does not contain ORDER BY statement whose is requested by LINQ.
        /// </summary>
        [Test]
        public void FirstOrDefaultNestedWithOrderBy()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (SakilaDb db = new SakilaDb())
            {
                db.Database.Connection.Open();
                var q = from cu in db.customers
                        let curAddr = db.addresses.OrderByDescending(p => p.address_id).Where(p => p.address_id == cu.address_id).FirstOrDefault()
                        join sto in db.stores on cu.store_id equals sto.store_id
                        orderby cu.customer_id descending
                        select new
                        {
                            curAddr.city.country.country1
                        };
                string sql = q.ToString();
                CheckSql(sql, SQLSyntax.FirstOrDefaultNestedWithOrderBy);
#if DEBUG
                Debug.WriteLine(sql);
#endif
                int j = q.Count();
                foreach (var r in q)
                {
                    //Debug.WriteLine( r.country1 );
                }
                Assert.AreEqual(599, j);
            }
        }

        /// <summary>
        /// SUPPORT FOR DATE TYPES WITH PRECISION
        /// </summary>
        [Test]
        public void CanDefineDatesWithPrecisionFor56()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif

            if (Version < new Version(5, 6)) return;

            using (var db = new ProductsDbContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                using (XGConnection conn = new XGConnection(db.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    XGCommand query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type,SCALE AS DateTime_Precision FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Products' LIMIT 1) AND COL_NAME='DateTimeWithPrecision'", conn);
                    query.Connection = conn;
                    XGDataReader reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("DateTimeWithPrecision", reader[0].ToString());
                        Assert.AreEqual("False", reader[1].ToString());
                        Assert.AreEqual("DATETIME", reader[2].ToString());
                        Assert.AreEqual("3", reader[3].ToString());
                    }
                    reader.Close();

                    query = new XGCommand("SELECT COL_NAME AS Column_name,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS Is_Nullable,TYPE_NAME AS Data_Type,SCALE AS DateTime_Precision FROM ALL_Columns WHERE TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Products' LIMIT 1) AND COL_NAME='TimeStampWithPrecision'", conn);
                    query.Connection = conn;
                    reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        Assert.AreEqual("TimeStampWithPrecision", reader[0].ToString());
                        Assert.AreEqual("False", reader[1].ToString());
                        Assert.AreEqual("DATETIME", reader[2].ToString());
                        Assert.AreEqual("3", reader[3].ToString());
                    }
                    reader.Close();
                }
                db.Database.Delete();
            }
        }

        /// <summary>
        /// Orabug #15935094 SUPPORT FOR CURRENT_TIMESTAMP AS DEFAULT FOR DATETIME WITH EF
        /// </summary>
        [Test]
        //[Ignore("Fix this")]
        public void CanDefineDateTimeAndTimestampWithIdentity()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif

            using (var db = new ProductsDbContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                //XGConnection con = (XGConnection)db.Database.Connection;
                //XGCommand cmd = new XGCommand("set session sql_mode = '';", con);
                //con.Open();
                //cmd.ExecuteNonQuery();
                //con.Close();

                Product product = new Product
                {
                    //Omitting Identity Columns
                    DateTimeWithPrecision = DateTime.Now,
                    TimeStampWithPrecision = DateTime.Now
                };

                db.Products.Add(product);
                db.SaveChanges();

                var updateProduct = db.Products.First();
                updateProduct.DateTimeWithPrecision = new DateTime(2012, 3, 18, 23, 9, 7, 6);
                db.SaveChanges();

                Assert.NotNull(db.Products.First().Timestamp);
                Assert.NotNull(db.Products.First().DateCreated);
                Assert.AreEqual(new DateTime(2012, 3, 18, 23, 9, 7, 6), db.Products.First().DateTimeWithPrecision);
                Assert.AreEqual(1, db.Products.Count());
                db.Database.Delete();
            }
        }


        /// <summary>
        /// Test of fix for bug Support for EntityFramework 4.3 Code First Generated Identifiers (MySql Bug #67285, Oracle bug #16286397).
        /// FKs are renamed to met http://dev.mysql.com/doc/refman/5.0/en/identifiers.html limitations.
        /// </summary>
        [Test]
        public void LongIdentifiersInheritanceTPT()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (DinosauriaDBContext db = new DinosauriaDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                Tyrannosauridae ty = new Tyrannosauridae() { Id = 1, Name = "Genghis Rex", SpecieName = "TRex", Weight = 1000 };
                db.dinos.Add(ty);
                Oviraptorosauria ovi = new Oviraptorosauria() { Id = 2, EggsPerYear = 100, Name = "John the Velociraptor", SpecieName = "Oviraptor" };
                db.dinos.Add(ovi);
                db.SaveChanges();
            }
        }


        /// <summary>
        /// Test fix for http://bugs.mysql.com/bug.php?id=67183
        /// (Malformed Query while eager loading with EF 4 due to multiple projections).
        /// </summary>
        [Test]
        public void ShipTest()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (var context = new ShipContext())
            {
                context.Database.Connection.Open();
                context.Database.Initialize(true);

                var harbor = new Harbor
                {
                    Ships = new HashSet<Ship>
            {
                new Ship
                {
                    CrewMembers = new HashSet<CrewMember>
                    {
                        new CrewMember
                        {
                            Rank = new Rank { Description = "Rank A" },
                            Clearance = new Clearance { Description = "Clearance A" },
                            Description = "CrewMember A"
                        },
                        new CrewMember
                        {
                            Rank = new Rank { Description = "Rank B" },
                            Clearance = new Clearance { Description = "Clearance B" },
                            Description = "CrewMember B"
                        }
                    },
                    Description = "Ship AB"
                },
                new Ship
                {
                    CrewMembers = new HashSet<CrewMember>
                    {
                        new CrewMember
                        {
                            Rank = new Rank { Description = "Rank C" },
                            Clearance = new Clearance { Description = "Clearance C" },
                            Description = "CrewMember C"
                        },
                        new CrewMember
                        {
                            Rank = new Rank { Description = "Rank D" },
                            Clearance = new Clearance { Description = "Clearance D" },
                            Description = "CrewMember D"
                        }
                    },
                    Description = "Ship CD"
                }
            },
                    Description = "Harbor ABCD"
                };

                context.Harbors.Add(harbor);
                context.SaveChanges();
            }

            using (var context = new ShipContext())
            {
                DbSet<Harbor> dbSet = context.Set<Harbor>();
                IQueryable<Harbor> query = dbSet;
                query = query.Include(entity => entity.Ships);
                query = query.Include(entity => entity.Ships.Select(s => s.CrewMembers));
                query = query.Include(entity => entity.Ships.Select(s => s.CrewMembers.Select(cm => cm.Rank)));
                query = query.Include(entity => entity.Ships.Select(s => s.CrewMembers.Select(cm => cm.Clearance)));

                string[] data = new string[] {
          "1,Harbor ABCD,1,1,1,Ship AB,1,1,1,1,1,CrewMember A,1,Rank A,1,Clearance A",
          "1,Harbor ABCD,1,1,1,Ship AB,1,2,1,2,2,CrewMember B,2,Rank B,2,Clearance B",
          "1,Harbor ABCD,1,2,1,Ship CD,1,3,2,3,3,CrewMember C,3,Rank C,3,Clearance C",
          "1,Harbor ABCD,1,2,1,Ship CD,1,4,2,4,4,CrewMember D,4,Rank D,4,Clearance D"
        };
                Dictionary<string, string> outData = new Dictionary<string, string>();

                var sqlString = query.ToString();
                CheckSql(sqlString, SQLSyntax.ShipQueryMalformedDueMultipleProjecttionsCorrectedEF6);
                // see below for the generated SQL query

                var harbor = query.Single();

                foreach (var ship in harbor.Ships)
                {
                    foreach (var crewMember in ship.CrewMembers)
                    {
                        outData.Add(string.Format(
                          "{0},{1},1,{2},{3},{4},1,{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                          harbor.HarborId, harbor.Description, ship.ShipId, harbor.HarborId,
                          ship.Description, crewMember.CrewMemberId, crewMember.ShipId, crewMember.RankId,
                          crewMember.ClearanceId, crewMember.Description, crewMember.Rank.RankId,
                          crewMember.Rank.Description, crewMember.Clearance.ClearanceId,
                          crewMember.Clearance.Description), null);
                    }
                }
                // check data integrity
                Assert.AreEqual(outData.Count, data.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    Assert.True(outData.ContainsKey(data[i]));
                }
            }
        }

        /// <summary>
        /// Tests fix for bug http://bugs.mysql.com/bug.php?id=68513, Error in LINQ to Entities query when using Distinct().Count().
        /// </summary>
        [Test]
        public void DistinctCount()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (SiteDbContext ctx = new SiteDbContext())
            {
                ctx.Database.Connection.Open();
                ctx.Database.Delete();
                ctx.Database.Initialize(true);
                visitante v1 = new visitante() { nCdSite = 1, nCdVisitante = 1, sDsIp = "x1" };
                visitante v2 = new visitante() { nCdSite = 1, nCdVisitante = 2, sDsIp = "x2" };
                site s1 = new site() { nCdSite = 1, sDsTitulo = "MyNewsPage" };
                site s2 = new site() { nCdSite = 2, sDsTitulo = "MySearchPage" };
                ctx.Visitante.Add(v1);
                ctx.Visitante.Add(v2);
                ctx.Site.Add(s1);
                ctx.Site.Add(s2);
                ctx.SaveChanges();

                var q = (from vis in ctx.Visitante.Include("site")
                         group vis by vis.nCdSite into g
                         select new retorno
                         {
                             Key = g.Key,
                             Online = g.Select(e => e.sDsIp).Distinct().Count()
                         });
                string sql = q.ToString();
                CheckSql(sql, SQLSyntax.CountGroupBy);
                var q2 = q.ToList<retorno>();
                foreach (var row in q2)
                {
                }
                Assert.IsTrue(q2.Count >0);
            }
        }

        /// <summary>
        /// Tests fix for bug http://bugs.mysql.com/bug.php?id=68513, Error in LINQ to Entities query when using Distinct().Count().
        /// </summary>
        [Test]
        public void DistinctCount2()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (SiteDbContext ctx = new SiteDbContext())
            {
                ctx.Database.Connection.Open();
                ctx.Database.Initialize(true);
                visitante v1 = new visitante() { nCdSite = 1, nCdVisitante = 1, sDsIp = "x1" };
                visitante v2 = new visitante() { nCdSite = 1, nCdVisitante = 2, sDsIp = "x2" };
                site s1 = new site() { nCdSite = 1, sDsTitulo = "MyNewsPage" };
                site s2 = new site() { nCdSite = 2, sDsTitulo = "MySearchPage" };
                pagina p1 = new pagina() { nCdPagina = 1, nCdVisitante = 1, sDsTitulo = "index.html" };
                ctx.Visitante.Add(v1);
                ctx.Visitante.Add(v2);
                ctx.Site.Add(s1);
                ctx.Site.Add(s2);
                ctx.Pagina.Add(p1);
                ctx.SaveChanges();

                var q = (from pag in ctx.Pagina.Include("visitante").Include("site")
                         group pag by pag.visitante.nCdSite into g
                         select new retorno
                         {
                             Key = g.Key,
                             Online = g.Select(e => e.visitante.sDsIp).Distinct().Count()
                         });
                string sql = q.ToString();
                CheckSql(sql, SQLSyntax.CountGroupBy2);
                var q2 = q.ToList<retorno>();
                foreach (var row in q2)
                {
                }
                Assert.IsTrue(q2.Count > 0);
            }
        }

        /// <summary>
        /// Tests fix for bug http://bugs.mysql.com/bug.php?id=65723, MySql Provider for EntityFramework produces "bad" SQL for OrderBy.
        /// </summary>
        [Test]
        public void BadOrderBy()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                DateTime filterDate = new DateTime(1986, 1, 1);
                var q = db.Movies.Where(p => p.ReleaseDate >= filterDate).
                  OrderByDescending(p => p.ReleaseDate).Take(2);
                string sql = q.ToString();
                CheckSql(SQLSyntax.NestedOrderBy, sql);
                // Data integrity testing
                Movie[] data = new Movie[] {
          new Movie() { ID = 4, Title = "Star Wars, The Sith Revenge", ReleaseDate = new DateTime( 2005, 5, 19 ) },
          new Movie() { ID = 2, Title = "The Matrix", ReleaseDate = new DateTime( 1999, 3, 31 ) }
        };
                int i = 0;
                foreach (Movie m in q)
                {
                    Assert.AreEqual(data[i].ID, m.ID);
                    Assert.AreEqual(data[i].Title, m.Title);
                    Assert.AreEqual(data[i].ReleaseDate, m.ReleaseDate);
                    i++;
                }
                Assert.AreEqual(2, i);
            }
        }

        /// <summary>
        /// Tests fix for bug http://bugs.mysql.com/bug.php?id=69751, Invalid SQL query generated for query with Contains, OrderBy, and Take.
        /// </summary>
        [Test]
        public void BadContainsOrderByTake()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                //db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                string title = "t";
                var q = from m in db.Movies
                        where m.Title.Contains(title)
                        orderby m.ID descending
                        select m;
                var q1 = q.Take(10);
                string sql = q1.ToString();

                CheckSql(SQLSyntax.QueryWithOrderByTakeContains, sql);

                int i = 0;
                foreach (var row in q1)
                {
                    Assert.AreEqual(MovieDBInitialize.data[i].ID, row.ID);
                    Assert.AreEqual(MovieDBInitialize.data[i].Title, row.Title);
                    Assert.AreEqual(MovieDBInitialize.data[i].ReleaseDate, row.ReleaseDate);
                    i++;
                }
            }
        }

        /// <summary>
        /// Tests fix for bug http://bugs.mysql.com/bug.php?id=69922, Unknown column Extent1...
        /// </summary>
        [Test]
        public void BadAliasTable()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (PromotionsDB db = new PromotionsDB())
            {
                db.Database.Initialize(true);
                DateTime now = DateTime.Now;
                var q = db
                  .HomePromoes
                  .Where(x =>
                     x.Active
                       &&
                     (x.ActiveFrom == null || x.ActiveFrom <= now)
                       &&
                     (x.ActiveTo == null || x.ActiveTo >= now)
                  )
                  .OrderBy(x => x.DisplayOrder).Select(d => d);
                string sql = q.ToString();
                foreach (var row in q)
                {
                }
            }
        }

        /// <summary>
        /// Tests other variants of bug http://bugs.mysql.com/bug.php?id=69751, Invalid SQL query generated for query with Contains, OrderBy, and Take.
        /// </summary>
        [Test]
        public void BadContainsOrderByTake2()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                //db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                var q = db.Movies.
                        Where(m => !string.IsNullOrEmpty(m.Title) && m.Title.Contains("x")).
                        OrderByDescending(m => m.ID).
                        Skip(1).
                        Take(1);
                string sql = q.ToString();
#if DEBUG
                Debug.WriteLine(sql);
#endif
                List<Movie> l = q.ToList();
                int j = l.Count;
                foreach (Movie m in l)
                {
                    j--;
                }
                Assert.AreEqual(0, j);
            }
        }

        /// <summary>
        /// Tests other variants of bug http://bugs.mysql.com/bug.php?id=69751, Invalid SQL query generated for query with Contains, OrderBy, and Take.
        /// </summary>
        [Test]
        public void BadContainsOrderByTake3()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                var q = db.Movies.
                        Where(m => !string.IsNullOrEmpty(m.Title) && m.Title.Contains("x")).
                        OrderByDescending(m => m.ID).
                        Skip(1).
                        Take(1).Select(m => new
                        {
                            Id = m.ID,
                            CriticsScore = (
                            m.Title == "Terminator 1" ? "Good" :
                            m.Title == "Predator" ? "Sunday best, cheese" :
                            m.Title == "The Matrix" ? "Really Good" :
                            m.Title == "Star Wars, The Sith Revenge" ? "Really Good" : "Unknown")
                        });
                string sql = q.ToString();
#if DEBUG
                Debug.WriteLine(sql);
#endif
                int j = q.Count();
                foreach (var row in q)
                {
                    j--;
                }
                Assert.AreEqual(0, j);
            }
        }

        /// <summary>
        /// Tests other variants of bug http://bugs.mysql.com/bug.php?id=69751, Invalid SQL query generated for query with Contains, OrderBy, and Take.
        /// </summary>
        [Test]
        public void BadContainsOrderByTake4()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                bool q = db.Movies.Any(m => m.ReleaseDate.Year > 1985);
                //        string sql = q.ToString();
                //#if DEBUG
                //        Debug.WriteLine(sql);
                //#endif
                //foreach (var row in q)
                //{
                //}
            }
        }

        /// <summary>
        /// Tests other variants of bug http://bugs.mysql.com/bug.php?id=69751, Invalid SQL query generated for query with Contains, OrderBy, and Take.
        /// </summary>
        [Test]
        public void BadContainsOrderByTake5()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                // TODO: add subquery like
                // var shifts = Shifts.Where(s => !EmployeeShifts.Where(es => es.ShiftID == s.ShiftID).Any());
                bool q = db.Movies.Where(m => m.ReleaseDate.Month != 10).Any(m => m.ReleaseDate.Year > 1985);
                //        string sql = q.ToString();
                //#if DEBUG
                //        Debug.WriteLine(sql);
                //#endif
                //        foreach (var row in q)
                //        {
                //        }
            }
        }

        /// <summary>
        /// Tests other variants of bug http://bugs.mysql.com/bug.php?id=69751, Invalid SQL query generated for query with Contains, OrderBy, and Take.
        /// </summary>
        [Test]
        public void BadContainsOrderByTake6()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                var q = from m in db.Movies
                        where m.Title.Contains("x") && db.Medias.Where(mm => mm.Format == "Digital").Any()
                        select m;
                string sql = q.ToString();
#if DEBUG
                Debug.WriteLine(sql);
#endif
                int j = q.Count();
                foreach (var row in q)
                {
                    j--;
                }
                Assert.AreEqual(0, j);
            }
        }

        /// <summary>
        /// Test for Mysql Bug 70602: http://bugs.mysql.com/bug.php?id=70602
        /// </summary>
        [Test]
        public void AutoIncrementBug()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            AutoIncrementBugContext dbContext = new AutoIncrementBugContext();
            dbContext.Database.Connection.Open();
            dbContext.Database.Initialize(true);
            dbContext.AutoIncrementBug.Add(new AutoIncrementBug() { Description = "Test" });
            dbContext.SaveChanges();
            using (var reader = XGHelper.ExecuteReader(dbContext.Database.Connection.ConnectionString,
              @"SELECT COL_NAME AS `FIELD`,TYPE_NAME AS `TYPE`,CASE WHEN NOT_NULL THEN FALSE ELSE TRUE END AS `NULL`,CONS_TYPE AS `Key`,DEF_VAL AS `Defualt`,SERIAL_ID AS Extra FROM ALL_COLUMNS c LEFT JOIN ALL_CONSTRAINTS s ON(c.TABLE_ID=s.TABLE_ID AND s.DEFINE LIKE '%""' || c.COL_NAME || '""%') LEFT JOIN ALL_TABLES t ON(c.TABLE_ID=t.TABLE_ID) WHERE c.IS_SERIAL AND t.TABLE_NAME='" + nameof(dbContext.AutoIncrementBug) + "s';"))
            {
                Assert.True(reader.HasRows);
            }
            dbContext.Database.Delete();
        }

        [Test]
        public void SimpleCodeFirstSelectCbc()
        {
            MovieCodedBasedConfigDBContext db = new MovieCodedBasedConfigDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            var l = db.Movies.ToList();
            foreach (var i in l)
            {
                Console.WriteLine(i);
            }
        }

        [Test]
        //[Ignore("Need to check. Bad statemen: CALL `MovieCodedBasedConfigDBContext`.``insert_movie``(@movie_name, @ReleaseDate, @Genre, @Price)")]
        public void TestStoredProcedureMapping()
        {
            using (var db = new MovieCodedBasedConfigDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                var movie = new MovieCBC()
                {
                    Title = "Sharknado",
                    Genre = "Documental",
                    Price = 1.50M,
                    ReleaseDate = DateTime.Parse("01/07/2013")
                };

                db.Movies.Add(movie);
                db.SaveChanges();
                movie.Genre = "Fiction";
                db.SaveChanges();
                db.Movies.Remove(movie);
                db.SaveChanges();
            }
        }

        [Test]
        public void MigrationHistoryConfigurationTest()
        {
            MovieCodedBasedConfigDBContext db = new MovieCodedBasedConfigDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            var l = db.Movies.ToList();
            foreach (var i in l)
            {
            }
            var result = XGHelper.ExecuteScalar($"IP=127.0.0.1;DB={db.Database.Connection.Database};User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK", "SELECT COUNT(_MigrationId) FROM __XGMigrations;");
            Assert.AreEqual(1, int.Parse(result.ToString()));
        }

        [Test]
        public void DbSetRangeTest()
        {
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                Movie m1 = new Movie() { Title = "Terminator 1", ReleaseDate = new DateTime(1984, 10, 26) };
                Movie m2 = new Movie() { Title = "The Matrix", ReleaseDate = new DateTime(1999, 3, 31) };
                Movie m3 = new Movie() { Title = "Predator", ReleaseDate = new DateTime(1987, 6, 12) };
                Movie m4 = new Movie() { Title = "Star Wars, The Sith Revenge", ReleaseDate = new DateTime(2005, 5, 19) };
                db.Movies.AddRange(new Movie[] { m1, m2, m3, m4 });
                db.SaveChanges();
                var q = from m in db.Movies select m;
                Assert.AreEqual(4, q.Count());
                foreach (var row in q)
                {
                }
                db.Movies.RemoveRange(q.ToList());
                db.SaveChanges();
                var q2 = from m in db.Movies select m;
                Assert.AreEqual(0, q2.Count());
            }
        }

        [Test]
        public void EnumSupportTest()
        {
            using (var dbCtx = new EnumTestSupportContext())
            {
                dbCtx.Database.Connection.Open();
                //dbCtx.Database.Initialize(true);
                dbCtx.SchoolSchedules.Add(new SchoolSchedule() { TeacherName = "Pako", Subject = SchoolSubject.History });
                dbCtx.SaveChanges();

                var schedule = (from s in dbCtx.SchoolSchedules
                                where s.Subject == SchoolSubject.History
                                select s).FirstOrDefault();

                Assert.AreNotEqual(null, schedule);
                Assert.AreEqual(SchoolSubject.History, schedule.Subject);
            }
        }


        [Test]
        [Ignore("This test needs MicrosoftSqlServer.Types which is not available for all the target frameworks.")]
        public void SpatialSupportTest()
        {
            using (var dbCtx = new JourneyContext())
            {
                dbCtx.Database.Connection.Open();
                dbCtx.Database.Initialize(true);
                dbCtx.MyPlaces.Add(new MyPlace()
                {
                    name = "JFK INTERNATIONAL AIRPORT OF NEW YORK",
                    location = DbGeometry.FromText("POINT(40.644047 -73.782291)"),
                });
                dbCtx.MyPlaces.Add(new MyPlace
                {
                    name = "ALLEY POND PARK",
                    location = DbGeometry.FromText("POINT(40.745696 -73.742638)")
                });
                dbCtx.MyPlaces.Add(new MyPlace
                {
                    name = "CUNNINGHAM PARK",
                    location = DbGeometry.FromText("POINT(40.735031 -73.768387)")
                });
                dbCtx.MyPlaces.Add(new MyPlace
                {
                    name = "QUEENS VILLAGE STATION",
                    location = DbGeometry.FromText("POINT(40.717957 -73.736501)")
                });
                dbCtx.SaveChanges();

                var place = (from p in dbCtx.MyPlaces
                             where p.name == "JFK INTERNATIONAL AIRPORT OF NEW YORK"
                             select p).FirstOrDefault();

                var point = DbGeometry.FromText("POINT(40.717957 -73.736501)");

                var distance = (point.Distance(place.location) * 100);

                Assert.AreNotEqual(null, place);
                Assert.AreEqual(8.6944880240295852D, distance.Value);

                var points = from p in dbCtx.MyPlaces
                             select new { name = p.name, location = p.location };
                foreach (var item in points)
                {
                    var distanceX = DbGeometry.FromText("POINT(40.717957 -73.736501)").Distance(item.location) * 100;
                    Assert.IsNotNull(distanceX);
                }

                foreach (MyPlace p in dbCtx.MyPlaces)
                    dbCtx.MyPlaces.Remove(p);
                dbCtx.SaveChanges();

                dbCtx.MyPlaces.Add(new MyPlace
                {
                    name = "AGraphic Design Institute",
                    location = DbGeometry.FromText(string.Format("POINT({0} {1})", -122.336106, 47.605049), 101)
                });

                dbCtx.MyPlaces.Add(new MyPlace
                {
                    name = "AGraphic Design Institute",
                    location = DbGeometry.FromText("POINT(-123.336106 47.605049)", 102)
                });

                dbCtx.MyPlaces.Add(new MyPlace
                {
                    name = "BGraphic Design Institute",
                    location = DbGeometry.FromText("POINT(-113.336106 47.605049)", 103)
                });

                dbCtx.MyPlaces.Add(new MyPlace
                {
                    name = "Graphic Design Institute",
                    location = DbGeometry.FromText(string.Format("POINT({0} {1})", 51.5, -1.28), 4326)
                });
                dbCtx.SaveChanges();

                var result = (from u in dbCtx.MyPlaces select u.location.CoordinateSystemId).ToList();
                foreach (var item in result)
                    Assert.IsNotNull(item);
                var res = dbCtx.MyPlaces.OrderBy(q => q.name.Take(1).Skip(1).ToList());
                Assert.IsNotNull(res);

                var pointA1 = DbGeometry.FromText(string.Format("POINT(40.644047 -73.782291)"));
                var pointB1 = DbGeometry.FromText("POINT(40.717957 -73.736501)");
                var distance1 = pointA1.Distance(pointB1);

                var pointA2 = DbGeometry.FromText("POINT(2.5 2.5)");
                var pointB2 = DbGeometry.FromText("POINT(4 0.8)");
                var distance2 = pointA2.Distance(pointB2);

                var pointA3 = DbGeometry.FromText("POINT(3 -4)");
                var pointB3 = DbGeometry.FromText("POINT(-1 3)");
                var distance3 = pointA3.Distance(pointB3);

                Assert.True(distance1.Value == 0.086944880240295855 && distance2.Value == 2.2671568097509267 &&
                     distance3.Value == 8.06225774829855);

            }
        }

        [Test]
        public void BeginTransactionSupportTest()
        {
            using (var dbcontext = new MovieCodedBasedConfigDBContext())
            {
                dbcontext.Database.Connection.Open();
                dbcontext.Database.Initialize(true);
                using (var transaction = dbcontext.Database.BeginTransaction())
                {
                    try
                    {
                        dbcontext.Movies.Add(new MovieCBC()
                        {
                            Title = "Sharknado",
                            Genre = "Documental",
                            Price = 1.50M,
                            ReleaseDate = DateTime.Parse("01/07/2013")
                        });

                        dbcontext.SaveChanges();
                        var result = XGHelper.ExecuteScalar("IP=127.0.0.1;DB=test;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK", "select COUNT(*) from moviecbcs;");
                        Assert.AreEqual(0, int.Parse(result.ToString()));

                        transaction.Commit();

                        result = XGHelper.ExecuteScalar("IP=127.0.0.1;DB=test;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK", "select COUNT(*) from moviecbcs;");
                        Assert.AreEqual(1, int.Parse(result.ToString()));
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        /// <summary>
        /// This test covers two new features on EF6: 
        /// 1- "DbContext.Database.UseTransaction, that use a transaction created from an open connection"
        /// 2- "DbContext can now be created with a DbConnection that is already opened"
        /// </summary>
        [Test]
        public void UseTransactionSupportTest()
        {
            using (var context = new MovieCodedBasedConfigDBContext())
            {
                context.Database.CreateIfNotExists();
            }
            using (var connection = new XGConnection($"IP=127.0.0.1;DB={nameof(MovieCodedBasedConfigDBContext)};User=SYSDBA;PWD=SYSDBA;Port={Port};AUTO_COMMIT=off;CHAR_SET=GBK"))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var dbcontext = new MovieCodedBasedConfigDBContext(connection, contextOwnsConnection: false))
                        {
                            dbcontext.Database.Initialize(true);
                            dbcontext.Database.UseTransaction(transaction);
                            dbcontext.Movies.Add(new MovieCBC()
                            {
                                Title = "Sharknado",
                                Genre = "Documental",
                                Price = 1.50M,
                                ReleaseDate = DateTime.Parse("01/07/2013")
                            });

                            dbcontext.SaveChanges();
                        }
                        var result = XGHelper.ExecuteScalar($"IP=127.0.0.1;DB={nameof(MovieCodedBasedConfigDBContext)};User=SYSDBA;PWD=SYSDBA;Port={Port};AUTO_COMMIT=off;CHAR_SET=GBK", "select COUNT(*) from moviecbc;");
                        Assert.AreEqual(0, int.Parse(result.ToString()));

                        transaction.Commit();

                        result = XGHelper.ExecuteScalar("server=localhost;User Id=root;database=test;logging=true; port=" + Port + ";", "select COUNT(*) from moviecbcs;");
                        Assert.AreEqual(1, int.Parse(result.ToString()));
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        [Test]
        [Ignore("Need to check. Bad statemen: CALL `MovieCodedBasedConfigDBContext`.``insert_movie``(@movie_name, @ReleaseDate, @Genre, @Price)")]
        public void HasChangesSupportTest()
        {
            using (var dbcontext = new MovieCodedBasedConfigDBContext())
            {
                dbcontext.Database.Connection.Open();
                dbcontext.Database.Initialize(true);

                dbcontext.Movies.Add(new MovieCBC()
                {
                    Title = "Sharknado",
                    Genre = "Documental",
                    Price = 1.50M,
                    ReleaseDate = DateTime.Parse("01/07/2013")
                });

                Assert.True(dbcontext.ChangeTracker.HasChanges());
                dbcontext.SaveChanges();
                Assert.False(dbcontext.ChangeTracker.HasChanges());
            }
        }

        [Test]
        [Ignore("Need to check. Bad statemen: CALL `MovieCodedBasedConfigDBContext`.``insert_movie``(@movie_name, @ReleaseDate, @Genre, @Price)")]
        public void XGLoggingToFileSupportTest()
        {
            string logName = "xugu.log";
            //if (System.IO.File.Exists(logName))
            //  System.IO.File.Delete(logName);

            using (var dbcontext = new MovieCodedBasedConfigDBContext())
            {
                dbcontext.Database.Log = XGLogger.Logger(logName, true).Write;

                dbcontext.Database.Initialize(true);
                dbcontext.Movies.Add(new MovieCBC()
                {
                    Title = "Sharknado",
                    Genre = "Documental",
                    Price = 1.50M,
                    ReleaseDate = DateTime.Parse("01/07/2013")
                });
                dbcontext.SaveChanges();
            }

            Assert.AreEqual(true, System.IO.File.Exists(logName));
        }

        [Test]
        [Ignore("Need to check. Bad statemen: CALL `MovieCodedBasedConfigDBContext`.``insert_movie``(@movie_name, @ReleaseDate, @Genre, @Price)")]
        public void XGLoggingToConsoleSupportTest()
        {
            string logName = "xugu_2.log";
            if (System.IO.File.Exists(logName))
                System.IO.File.Delete(logName);

            System.IO.FileStream file;
            System.IO.StreamWriter writer;
            System.IO.TextWriter txtOut = Console.Out;
            try
            {
                file = new System.IO.FileStream(logName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                writer = new System.IO.StreamWriter(file);
            }
            catch (Exception e)
            {
                throw e;
            }
            Console.SetOut(writer);

            using (var dbcontext = new MovieCodedBasedConfigDBContext())
            {
                dbcontext.Database.Log = new XGLogger(s => Console.Write(s)).Write;

                dbcontext.Database.Initialize(true);
                dbcontext.Movies.Add(new MovieCBC()
                {
                    Title = "Sharknado",
                    Genre = "Documental",
                    Price = 1.50M,
                    ReleaseDate = DateTime.Parse("01/07/2013")
                });
                dbcontext.SaveChanges();
            }
            Console.SetOut(txtOut);
            writer.Close();
            file.Close();

            Assert.AreEqual(true, System.IO.File.Exists(logName));
        }

        [Test]
        public void EntityAndComplexTypeSupportTest()
        {
            using (var dbContext = new EntityAndComplexTypeContext())
            {
                dbContext.Database.Connection.Open();
                dbContext.Database.Delete();
                dbContext.Database.CreateIfNotExists();
                dbContext.Database.Initialize(true);
                dbContext.Students.Add(
                      new Student()
                      {
                          Name = "Pakorasu Pakolas",
                          Address = new Address() { City = "Mazatlan", Street = "Tierra de Venados 440" },
                          Schedule = new List<SchoolSchedule>() { new SchoolSchedule() { TeacherName = "Pako", Subject = SchoolSubject.History } }
                      });
                dbContext.SaveChanges();

                var student = (from s in dbContext.Students.Include(i=>i.Schedule)
                               select s).FirstOrDefault();

                Assert.AreNotEqual(null, student);
                Assert.AreNotEqual(null, student.Schedule);
                Assert.AreNotEqual(true, string.IsNullOrEmpty(student.Address.Street));
                Assert.AreNotEqual(0, student.Schedule.Count());
            }
        }

        /// <summary>
        /// TO RUN THIS TEST ITS NECESSARY TO ENABLE THE EXECUTION STRATEGY IN THE CLASS MySqlEFConfiguration (Source\MySql.Data.Entity\MySqlConfiguration.cs) AS WELL AS START A MYSQL SERVER INSTACE WITH THE OPTION "--max_connections=3"
        /// WHY 3?: 1)For main process (User: root, DB: mysql). 2)For Setup Class. 3)For the connections in this test.
        /// The expected result is that opening a third connection and trying to open a fourth(with an asynchronous task) the execution strategy implementation handle the reconnection process until the third one is closed.
        /// </summary>
        //[Test] //<---DON'T FORGET ME TO RUN! =D
        public void ExecutionStrategyTest()
        {
            var connection = new XGConnection("server=localhost;User Id=root;logging=true; port=" + Port + ";");
            using (var dbcontext = new MovieCodedBasedConfigDBContext())
            {
                dbcontext.Database.Initialize(true);
                dbcontext.Movies.Add(new MovieCBC()
                {
                    Title = "Sharknado",
                    Genre = "Documental",
                    Price = 1.50M,
                    ReleaseDate = DateTime.Parse("01/07/2013")
                });
                connection.Open();
                System.Threading.Tasks.Task.Factory.StartNew(() => { dbcontext.SaveChanges(); });
                Thread.Sleep(1000);
                connection.Close();
                connection.Dispose();
            }
            var result = XGHelper.ExecuteScalar("server=localhost;User Id=root;database=test;logging=true; port=" + Port + ";", "select COUNT(*) from moviecbcs;");
            Assert.AreEqual(1, int.Parse(result.ToString()));
        }

        [Test]
        public void UnknownProjectC1()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);
                MovieDBInitialize.DoDataPopulation(db);
                long myKey = 20;
                var q = (from r in db.Movies where (r.ID == myKey) select (long)r.ID).OrderBy(p => p);
                string sql = q.ToString();
                CheckSql(sql, SQLSyntax.UnknownProjectC1EF6);

#if DEBUG
                Debug.WriteLine(sql);
#endif
                long[] array = (from r in db.Movies where (r.ID == myKey) select (long)r.ID).OrderBy(p => p).ToArray();
            }
        }

        [Test]
        public void StartsWithTest()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            MovieDBContext db = new MovieDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            MovieDBInitialize.DoDataPopulation(db);
            string term = "The";
            var l = db.Movies.Where(p => p.Title.StartsWith(term));

            string sql = l.ToString();

            CheckSql(sql, SQLSyntax.QueryWithStartsWith);

#if DEBUG
            Debug.WriteLine(sql);
#endif
            int j = l.Count();
            foreach (var i in l)
            {
                j--;
            }
            Assert.AreEqual(0, j);
        }

        [Test]
        public void EndsWithTest()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            MovieDBContext db = new MovieDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            MovieDBInitialize.DoDataPopulation(db);
            string term = "The";
            var l = db.Movies.Where(p => p.Title.EndsWith(term));

            string sql = l.ToString();

            CheckSql(sql, SQLSyntax.QueryWithEndsWith);

#if DEBUG
            Debug.WriteLine(sql);
#endif
            int j = l.Count();
            foreach (var i in l)
            {
                j--;
            }
            Assert.AreEqual(0, j);
        }

        [Test]
        public void ContainsTest()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            MovieDBContext db = new MovieDBContext();
            db.Database.Connection.Open();
            db.Database.Initialize(true);
            MovieDBInitialize.DoDataPopulation(db);
            string term = "The";
            var l = db.Movies.Where(p => p.Title.Contains(term));

            string sql = l.ToString();
            CheckSql(sql, SQLSyntax.QueryWithContains);

#if DEBUG
            Debug.WriteLine(sql);
#endif
            int j = l.Count();
            foreach (var i in l)
            {
                j--;
            }
            Assert.AreEqual(0, j);
        }


        /// <summary>
        /// Test to reproduce bug http://bugs.mysql.com/bug.php?id=73643, Exception when using IEnumera.Contains(model.property) in Where predicate
        /// </summary>
        [Test]
        public void TestContainsListWithCast()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);

                long[] longs = new long[] { 1, 2, 3 };
                var q = db.Movies.Where(p => longs.Contains((long)p.ID));
                string sql = q.ToString();
                CheckSql(sql, SQLSyntax.TestContainsListWithCast);
#if DEBUG
                Debug.WriteLine(sql);
#endif
                //var l = q.ToList();
            }
        }

        /// <summary>
        /// Test to reproduce bug http://bugs.mysql.com/bug.php?id=73643, Exception when using IEnumera.Contains(model.property) in Where predicate
        /// </summary>
        [Test]
        public void TestContainsListWitConstant()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);

                List<string> strIds = new List<string>(new string[] { "two" });
                var q = db.Movies.Where(p => strIds.Contains("two"));
                string sql = q.ToString();
                CheckSql(sql, SQLSyntax.TestContainsListWitConstant);
#if DEBUG
                Debug.WriteLine(sql);
#endif
                //var l = q.ToList();
            }
        }

        /// <summary>
        /// Test to reproduce bug http://bugs.mysql.com/bug.php?id=73643, Exception when using IEnumera.Contains(model.property) in Where predicate
        /// </summary>
        [Test]
        public void TestContainsListWithParameterReference()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Initialize(true);

                long[] longs = new long[] { 1, 2, 3 };
                int myNum = 1;
                var q = db.Movies.Where(p => longs.Contains(myNum));
                string sql = q.ToString();
                CheckSql(sql, SQLSyntax.TestContainsListWithParameterReference);
#if DEBUG
                Debug.WriteLine(sql);
#endif
                //var l = q.ToList();
            }
        }

        [Test]
        public void ReplaceTableNameVisitor()
        {
            using (SakilaDb context = new SakilaDb())
            {
                context.Database.Connection.Open();
                context.Database.Initialize(true);
                var date = new DateTime(2005, 6, 1);
                var rentals = context.customers.Where(t => t.rentals.Any(r => r.rental_date < date)).OrderBy(o => o.customer_id);
                string sql = rentals.ToString();
                CheckSql(sql, SQLSyntax.ReplaceNameVisitorQuery);
#if DEBUG
                Debug.WriteLine(sql);
#endif
                var result = rentals.ToList();
                Assert.AreEqual(520, rentals.Count());
            }
        }


        /// <summary>
        /// Bug #70941 - Invalid SQL query when eager loading two nested collections
        /// </summary>
        [Test]
        public void InvalidQuery()
        {
            using (UsingUnionContext context = new UsingUnionContext())
            {
                //context.Database.Connection.Open();
                if (context.Database.Exists())
                    context.Database.Delete();

                context.Database.Create();

                for (int i = 1; i <= 3; i++)
                {
                    var order = new Order();
                    var items = new List<Item>();

                    items.Add(new Item { Id = 1 });
                    items.Add(new Item { Id = 2 });
                    items.Add(new Item { Id = 3 });

                    order.Items = items;
                    var client = new Client { Id = i };
                    client.Orders = new List<Order>();
                    client.Orders.Add(order);

                    context.Clients.Add(client);
                }
                context.SaveChanges();

                var clients = context.Clients
                            .Include(c => c.Orders.Select(o => o.Items))
                            .Include(c => c.Orders.Select(o => o.Discounts)).ToList();

                Assert.AreEqual(clients.Count(), 3);
                Assert.AreEqual(clients.Where(t => t.Id == 1).Single().Orders.Count(), 1);
                Assert.AreEqual(clients.Where(t => t.Id == 1).Single().Orders.Where(j => j.Id == 1).Single().Items.Count(), 3);
            }
        }

        /// <summary>
        /// Bug #28095165 - CONTRIBUTION: FIX CONCURRENCYCHECK + DATABASEGENERATEDOPTION.COMPUTED
        /// </summary>
        [Test]
        public void ConcurrencyCheckWithDbGeneratedColumn()
        {
#if DEBUG
            Debug.WriteLine(new StackTrace().GetFrame(0).GetMethod().Name);
#endif
            using (MovieDBContext db = new MovieDBContext())
            {
                db.Database.Connection.Open();
                db.Database.Delete();
                db.Database.CreateIfNotExists();
                db.Database.Log = (e) => Debug.WriteLine(e);
                db.Database.ExecuteSqlCommand(@"DROP TABLE IF EXISTS `MovieReleases2`");

                db.Database.ExecuteSqlCommand(
                  @"CREATE TABLE IF NOT EXISTS `MovieRelease2` (
          `Id` int NOT NULL,
          `Name` varchar(45) NOT NULL,
          `Timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
          `RowVersion` bigint NOT NULL DEFAULT 0,
          PRIMARY KEY (`Id`)
          );");

                db.Database.ExecuteSqlCommand(
                  @"CREATE TRIGGER `trg_MovieRelease2_before_update` 
          BEFORE UPDATE ON `MovieRelease2`
          FOR EACH ROW
          BEGIN
          SET NEW.RowVersion = OLD.RowVersion + 1;
          END;");

                XGTrace.Listeners.Clear();
                XGTrace.Switch.Level = SourceLevels.All;
                GenericListener listener = new GenericListener();
                XGTrace.Listeners.Add(listener);

                try
                {
                    MovieRelease2 mr = db.MovieReleases2.Create();
                    mr.Id = 1;
                    mr.Name = "Commercial";
                    db.MovieReleases2.Add(mr);
                    Assert.AreEqual(mr.RowVersion, 0);
                    db.SaveChanges(); // ADD
                    Assert.AreEqual(mr.RowVersion, 0);

                    mr.Name = "Director's Cut";
                    db.SaveChanges(); // UPDATE #1
                    Assert.AreEqual(mr.RowVersion, 0);

                    mr.Name = "Avengers";
                    db.SaveChanges(); // UPDATE #2
                    Assert.AreEqual(mr.RowVersion, 0);
                }
                finally
                {
                    db.Database.ExecuteSqlCommand(@"DROP TABLE IF EXISTS `MovieReleases2`");
                }
                // Check sql        
                Regex rx = new Regex(@"Query Opened: (?<item>UPDATE .*)", RegexOptions.Compiled | RegexOptions.Singleline);
                int n = 0;
                foreach (string s in listener.Strings)
                {
                    Match m = rx.Match(s);
                    if (m.Success)
                    {
                        if (n++ == 0)
                        {
                            CheckSql(m.Groups["item"].Value, SQLSyntax.UpdateWithSelectWithDbGeneratedLock1);
                        }
                        else
                        {
                            CheckSql(m.Groups["item"].Value, SQLSyntax.UpdateWithSelectWithDbGeneratedLock2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Bug #31323788 - EF6 CODE FIRST - TABLE SCHEMAS ARE LOST, BUT AUTOMATIC MIGRATIONS USES THEM
        /// </summary>
        [Test]
        public void TablesWithSchema()
        {
            using (BlogContext context = new BlogContext())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.CreateIfNotExists();
                //context.Database.Initialize(true);
                var blog = new Blog { Title = "Blog_1" };
                context.Blog.Add(blog);

                blog = new Blog { Title = "Blog_2" };
                context.Blog.Add(blog);

                context.SaveChanges();
                Assert.AreEqual(2, context.Blog.Count());
                Assert.AreEqual(2, context.Blog.First(b => b.Title == "Blog_2").BlogId);

                context.Blog.Remove(blog);
                context.SaveChanges();
                Assert.AreEqual(1, context.Blog.Count());
                Assert.AreEqual("Blog_1", context.Blog.First().Title);
            }
        }

        [Test, Description("UNION SYNTAX MISSING REQUIRED PARENTHESIS")]
        public void UnionSyntax()
        {
            using (var context = new ContextForString())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Create();
                context.StringUsers.Add(new StringUser
                {
                    StringUserId = 1,
                    Name50 = "Juan",
                    Name100 = "100",
                    Name200 = "200",
                    Name300 = "300"
                });
                context.StringUsers.Add(new StringUser
                {
                    StringUserId = 2,
                    Name50 = "Pedro",
                    Name100 = "cien",
                    Name200 = "doscientos",
                    Name300 = "trescientos"
                });
                context.StringUsers.Add(new StringUser
                {
                    StringUserId = 3,
                    Name50 = "Lupe",
                    Name100 = "101",
                    Name200 = "cxvbx",
                    Name300 = "301"
                });
                context.StringUsers.Add(new StringUser
                {
                    StringUserId = 4,
                    Name50 = "Luis",
                    Name100 = "asdf",
                    Name200 = "wrwe",
                    Name300 = "xcvb"
                });
                context.StringUsers.Add(new StringUser
                {
                    StringUserId = 5,
                    Name50 = "Pepe",
                    Name100 = "asdf",
                    Name200 = "zxvz",
                    Name300 = "fgsd"
                });
                context.SaveChanges();

                var query1 = context.StringUsers;
                var query2 = query1.Take(0).Concat(query1);
                var query3 = query1.Concat(query1.Take(0));
                Assert.True((query1.Count() == 5) & (query2.Count() == 5) & (query3.Count() == 5));
            }
        }

        [Test, Description("FK name ,longer than 64 chars are named to FK_<guid>")]
        public void NormalForeignKey()
        {
            using (var context = new ContextForNormalFk())
            {
                context.Database.Connection.Open();
                context.Database.Initialize(true);
                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    var cmd = new XGCommand();
                    var entityName = (context.Permisos.GetType().FullName.Split(',')[0]).Substring(66).ToLowerInvariant();
                    var contextName = context.GetType().Name.ToLowerInvariant();
                    cmd.Connection = conn;
                    cmd.CommandText =
                        $"SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE TABLE_ID = (SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='{entityName}' LIMIT 1);";
                    cmd.ExecuteNonQuery();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var val = reader.GetValue(0);
                            Assert.True(val.ToString().Contains("FK_"));
                        }
                    }
                }
            }
        }

        [Test, Description("FK name ,longer than 64 chars are named to FK_<guid>")]
        public void LongForeignKey()
        {
            using (var context = new ContextForLongFk())
            {
                context.Database.Connection.Open();
                context.Database.Initialize(true);
                var entityName = (context.Permisos.GetType().FullName.Split(',')[0]).Substring(66).ToLowerInvariant();
                var contextName = context.GetType().Name.ToLowerInvariant();
                using (XGConnection conn = new XGConnection(context.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    var cmd = new XGCommand();
                    cmd.Connection = conn;
                    cmd.CommandText =
                        $"SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE TABLE_ID = (SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='{entityName}' LIMIT 1);";
                    cmd.ExecuteNonQuery();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var val = reader.GetValue(0);
                            Assert.True(val.ToString().Contains("FK_"));
                        }
                    }
                }
            }
        }

        [Test, Description("Verify that Null Reference Exception is not thrown when try to save entity with TINYINT AS PK ")]
        public void SaveTinyIntAsPK()
        {
            using (var context = new ContextForTinyPk())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Create();
                context.TinyPkUseRs.Add(new TinyPkUser
                {
                    StringUserId = 1,
                    Name50 = "Juan",
                    Name100 = "100",
                    Name200 = "200",
                    Name300 = "300"
                });

                context.TinyPkUseRs.Add(new TinyPkUser
                {
                    StringUserId = 2,
                    Name50 = "Pedro",
                    Name100 = "cien",
                    Name200 = "doscientos",
                    Name300 = "trescientos"
                });

                context.TinyPkUseRs.Add(new TinyPkUser
                {
                    StringUserId = 3,
                    Name50 = "Lupe",
                    Name100 = "101",
                    Name200 = "cxvbx",
                    Name300 = "301"
                });

                context.TinyPkUseRs.Add(new TinyPkUser
                {
                    StringUserId = 4,
                    Name50 = "Luis",
                    Name100 = "asdf",
                    Name200 = "wrwe",
                    Name300 = "xcvb"
                });

                context.TinyPkUseRs.Add(new TinyPkUser
                {
                    StringUserId = 5,
                    Name50 = "Pepe",
                    Name100 = "asdf",
                    Name200 = "zxvz",
                    Name300 = "fgsd"
                });
                context.SaveChanges();
                var query1 = context.TinyPkUseRs;
                var query2 = query1.Take(0).Concat(query1);
                var query3 = query1.Concat(query1.Take(0));
                Assert.True((query1.Count() == 5) & (query2.Count() == 5) & (query3.Count() == 5));
            }
        }

        [Test, Description("Verify that Null Reference Exception is not thrown when try to save entity with BIGINT AS PK ")]
        public void SaveBigIntAsPK()
        {
            using (var context = new ContextForBigIntPk())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Create();
                context.BigIntPkUseRs.Add(new BigIntPkUser
                {
                    StringUserId = 934157136952,
                    Name50 = "Juan",
                    Name100 = "100",
                    Name200 = "200",
                    Name300 = "300"
                });

                context.BigIntPkUseRs.Add(new BigIntPkUser
                {
                    StringUserId = 934157136953,
                    Name50 = "Pedro",
                    Name100 = "cien",
                    Name200 = "doscientos",
                    Name300 = "trescientos"
                });

                context.BigIntPkUseRs.Add(new BigIntPkUser
                {
                    StringUserId = 9223372036854775807,
                    Name50 = "Lupe",
                    Name100 = "101",
                    Name200 = "cxvbx",
                    Name300 = "301"
                });

                context.BigIntPkUseRs.Add(new BigIntPkUser
                {
                    StringUserId = 0,
                    Name50 = "Luis",
                    Name100 = "asdf",
                    Name200 = "wrwe",
                    Name300 = "xcvb"
                });

                context.BigIntPkUseRs.Add(new BigIntPkUser
                {
                    StringUserId = -9223372036854775808,
                    Name50 = "Pepe",
                    Name100 = "asdf",
                    Name200 = "zxvz",
                    Name300 = "fgsd"
                });
                context.SaveChanges();
                var query1 = context.BigIntPkUseRs;
                var query2 = query1.Take(0).Concat(query1);
                var query3 = query1.Concat(query1.Take(0));
                Assert.True((query1.Count() == 5) & (query2.Count() == 5) & (query3.Count() == 5));
            }
        }

        [Test, Description("TRANSACTION AFTER A FAILED TRANSACTION((USING BeginTransaction)) Commit")]
        public void BeginTransNested()
        {
            using (var context = new EnumTestSupportContext())
            {
                context.Database.Connection.Open();
                //context.Database.Delete();
                context.Database.Initialize(true);
                //context.Database.Delete();
                //context.Database.CreateIfNotExists();
                using (var trans = context.Database.BeginTransaction())
                {
                    Thread.Sleep(5000);
                    Assert.Catch(() => context.Database.ExecuteSqlCommand("update table schoolschedule"));
                    trans.Commit();
                }
                using (var trans = context.Database.BeginTransaction())
                {
                    context.SchoolSchedules.Add(new SchoolSchedule
                    {
                        TeacherName = "Ruben",
                        Subject = SchoolSubject.History
                    });
                    ;

                    context.SchoolSchedules.Add(new SchoolSchedule
                    {
                        TeacherName = "Peter",
                        Subject = SchoolSubject.Chemistry
                    });
                    ;

                    context.SchoolSchedules.Add(new SchoolSchedule
                    {
                        TeacherName = "Juan",
                        Subject = SchoolSubject.Math
                    });
                    ;
                    context.SaveChanges();
                    trans.Commit();
                }
                var count = context.SchoolSchedules.Count();
                Assert.AreEqual(3, count);
                //Rollback
                using (var trans = context.Database.BeginTransaction())
                {
                    Assert.Catch(() => context.Database.ExecuteSqlCommand("update table schoolschedule"));
                    trans.Rollback();
                }
                using (var trans = context.Database.BeginTransaction())
                {
                    context.SchoolSchedules.Add(new SchoolSchedule
                    {
                        TeacherName = "Andrew",
                        Subject = SchoolSubject.History
                    }); ;
                    ;
                    context.SaveChanges();
                    trans.Commit();
                }
                count = context.SchoolSchedules.Count();
                Assert.AreEqual(4, count);
            }

        }

        [Test, Description("TRANSACTION AFTER A FAILED TRANSACTION((USING BeginTransaction)) Stress Test")]
        public void TransactionAfterFailStressTest()
        {
            for (var i = 0; i < 100; i++)
            {
                using (var context = new EnumTestSupportContext())
                {
                    context.Database.Connection.Open();
                    using (var trans = context.Database.BeginTransaction())
                    {
                        Assert.Catch(() => context.Database.ExecuteSqlCommand("update table schoolschedule"));
                        trans.Commit();
                    }
                    using (var trans = context.Database.BeginTransaction())
                    {
                        context.SchoolSchedules.Add(new SchoolSchedule
                        {
                            TeacherName = "Ruben",
                            Subject = SchoolSubject.History
                        });
                        ;

                        context.SchoolSchedules.Add(new SchoolSchedule
                        {
                            TeacherName = "Peter",
                            Subject = SchoolSubject.Chemistry
                        });
                        ;

                        context.SchoolSchedules.Add(new SchoolSchedule
                        {
                            TeacherName = "Juan",
                            Subject = SchoolSubject.Math
                        });
                        context.SaveChanges();
                        trans.Commit();
                        var count = context.SchoolSchedules.Count();
                        Assert.True(count > 0);
                    }
                }
            }
        }


        [Test, Description("Wrong SQL Statement to set primary key ")]
        public void WrongSQLStatementPK()
        {
            //using (var context = new EducationContext())
            //{
            //    context.Database.Connection.Open();
            //    using (var trans = context.Database.BeginTransaction())
            //    {

            //        context.Database.Delete();
            //        context.Database.Create();
            //        context.Passports.Add(new Passport { Key = 1 });
            //        context.SaveChanges();
            //        trans.Commit();
            //    }
            //    using (var trans = context.Database.BeginTransaction())
            //    {
            //        context.Database.ExecuteSqlCommand("ALTER TABLE `passports` modify `Key` int IDENTITY UNIQUE NOT NULL;ALTER TABLE `passports` RENAME `Key` TO `Key1`;");
            //        trans.Commit();
            //    }

            //    //context.Database.ExecuteSqlCommand("ALTER TABLE `passports` DROP PRIMARY KEY");
            //}

            ////using (var context = new EducationContext())
            ////{
            ////    context.Database.Connection.Open();
            ////    context.Passports.Add(new Passport { Key = 1 });
            ////    context.SaveChanges();
            ////}

            ////using (var context = new EducationContext())
            ////{
            ////    context.Database.Connection.Open();
            ////    context.Database.ExecuteSqlCommand("ALTER TABLE `passports` modify `Key` int IDENTITY UNIQUE NOT NULL;ALTER TABLE `passports` RENAME `Key` TO `Key1`;");
            ////    //context.Database.ExecuteSqlCommand("ALTER TABLE `passports` DROP PRIMARY KEY");
            ////}

            //using (var context = new EducationContext())
            //{
            //    context.Passports.Add(new Passport { Key = 1 });
            //    Exception ex = Assert.Catch(() => context.SaveChanges());
            //    context.Database.Delete();
            //}
            using (var context = new EducationContext())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Create();
                context.Passports.Add(new Passport { Key = 1 });
                context.SaveChanges();
                context.Database.ExecuteSqlCommand("ALTER TABLE `passports` modify `Key` int IDENTITY UNIQUE NOT NULL;");
            }
            using (var context = new EducationContext())
            {
                context.Passports.Add(new Passport { Key = 1 });
                Exception ex = Assert.Catch(() => context.SaveChanges());
                context.Database.Delete();
            }
        }

        /// <summary>
        /// Bug #34498485 [MySQL.Data.EntityFramework does not handle LIKE (Edm.IndexOf) cases]
        /// </summary>
        [Test]
        public void TestListMatchingLike()
        {
            using (VehicleDbContext2 context = new VehicleDbContext2())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Initialize(true);

                context.Vehicles.Add(new Car2 { Id = 1, Name = "Mustang", Year = 2012, CarProperty = "Car" });
                context.Vehicles.Add(new Bike2 { Id = 101, Name = "Mountain", Year = 2011, BikeProperty = "Bike" });
                context.SaveChanges();

                string[] matchText = new string[] { "must", "tan" };
                var list = context.Vehicles.Where(v => matchText.Any(t => v.Name.Contains(t)));
                Assert.AreEqual(1, list.Count());

                matchText = new string[] { "mus't", "tan" };
                list = context.Vehicles.Where(v => matchText.Any(t => v.Name.Contains(t)));
                Assert.AreEqual(1, list.Count());

                matchText = new string[] { "%" };
                list = context.Vehicles.Where(v => matchText.Any(t => v.Name.Contains(t)));
                Assert.AreEqual(0, list.Count());

                matchText = new string[] { "tan" };
                list = context.Vehicles.Where(v => matchText.Any(t => v.Name.Contains(t)));
                Assert.AreEqual(1, list.Count());

                matchText = new string[] { "_" };
                list = context.Vehicles.Where(v => matchText.Any(t => v.Name.Contains(t)));
                Assert.AreEqual(0, list.Count());
            }
        }
        [Test]
        public void TestForeign()
        {
            using (TestForeignContext context = new TestForeignContext())
            {
                context.Database.Connection.Open();
                //context.Database.Delete();
                context.Database.Initialize(true);
            }
        }

        [Test]
        public void TestForeign2()
        {
            using (TestForeignContext2 context = new TestForeignContext2())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.CreateIfNotExists();
                context.Database.Initialize(true);
                context.SaveChanges();
            }
        }

        [Test]
        public void TestForeign3()
        {
            using (TestForeignContext3 context = new TestForeignContext3())
            {
                context.Database.Connection.Open();
                //context.Database.Delete();
                context.Database.Initialize(true);
            }
        }

        [Test]
        public void TestManyToMany()
        {
            using (TestManyContext context = new TestManyContext())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Initialize(true);
            }
        }

        [Test]
        public void TestBeginTransNested()
        {
            using (var context = new TestEnumTestSupportContext())
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.Initialize(true);
                //context.Database.Delete();
                //context.Database.CreateIfNotExists();
                using (var trans = context.Database.BeginTransaction())
                {
                    Thread.Sleep(5000);
                    Assert.Catch(() => context.Database.ExecuteSqlCommand("update table testschoolschedule"));
                    trans.Commit();
                }
                using (var trans = context.Database.BeginTransaction())
                {
                    context.SchoolSchedules.Add(new TestSchoolSchedule
                    {
                        TeacherName = "Ruben",
                        Subject = TestSchoolSubject.History
                    });

                    context.SchoolSchedules.Add(new TestSchoolSchedule
                    {
                        TeacherName = "Peter",
                        Subject = TestSchoolSubject.Chemistry
                    });

                    context.SchoolSchedules.Add(new TestSchoolSchedule
                    {
                        TeacherName = "Juan",
                        Subject = TestSchoolSubject.Math
                    });
                    context.SaveChanges();
                    trans.Commit();
                }
                var count = context.SchoolSchedules.Count();
                Assert.AreEqual(3, count);
                //Rollback
                using (var trans = context.Database.BeginTransaction())
                {
                    Assert.Catch(() => context.Database.ExecuteSqlCommand("update table testschoolschedule"));
                    trans.Rollback();
                }
                using (var trans = context.Database.BeginTransaction())
                {
                    context.SchoolSchedules.Add(new TestSchoolSchedule
                    {
                        TeacherName = "Andrew",
                        Subject = TestSchoolSubject.History
                    }); ;
                    ;
                    context.SaveChanges();
                    trans.Commit();
                }
                count = context.SchoolSchedules.Count();
                Assert.AreEqual(4, count);
            }

        }
    }
}